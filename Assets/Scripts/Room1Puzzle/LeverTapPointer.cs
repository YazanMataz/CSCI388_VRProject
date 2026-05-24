using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace Room1Puzzle
{
    /// <summary>
    /// Main Camera or XR Origin. G / click = head ray. VR trigger = controller ray only.
    /// </summary>
    public class LeverTapPointer : MonoBehaviour
    {
        public float range = 12f;
        public float rayRadius = 0.08f;
        public LayerMask hitMask = ~0;

        public Transform headRay;
        public Transform rightRay;
        public Transform leftRay;

        public bool debugLog;

        void Start()
        {
            if (headRay == null && Camera.main != null)
                headRay = Camera.main.transform;

            if (rightRay == null)
                rightRay = FindTransform("Right Controller");

            if (leftRay == null)
                leftRay = FindTransform("Left Controller");
        }

        void Update()
        {
            if (WasKeyboardOrMouseTap())
            {
                TryTapFromTransform(headRay);
                return;
            }

            if (!WasVrTriggerTap())
                return;

            if (TryTapFromTransform(rightRay))
                return;

            TryTapFromTransform(leftRay);
        }

        bool WasKeyboardOrMouseTap()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame)
                return true;

            return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
            return Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.G);
#endif
        }

        bool WasVrTriggerTap()
        {
#if ENABLE_INPUT_SYSTEM
            return AnyXrTriggerPressedThisFrame();
#else
            var r = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.RightHand);
            var l = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.LeftHand);
            return (r.isValid && r.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out bool rp) && rp)
                || (l.isValid && l.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out bool lp) && lp);
#endif
        }

#if ENABLE_INPUT_SYSTEM
        static bool AnyXrTriggerPressedThisFrame()
        {
            foreach (InputDevice device in InputSystem.devices)
            {
                if (!device.enabled)
                    continue;

                if (WasPressedThisFrame(device, "triggerPressed"))
                    return true;

                if (WasPressedThisFrame(device, "triggerButton"))
                    return true;
            }

            return false;
        }

        static bool WasPressedThisFrame(InputDevice device, string controlName)
        {
            ButtonControl button = device.TryGetChildControl<ButtonControl>(controlName);
            return button != null && button.wasPressedThisFrame;
        }
#endif

        bool TryTapFromTransform(Transform origin)
        {
            if (origin == null)
                return false;

            LeverTap lever = RaycastForLever(origin.position, origin.forward, origin.name);
            if (lever == null)
                return false;

            lever.Toggle();

            if (debugLog)
                Debug.Log($"LeverTap: toggled {lever.name} ({(lever.IsOn ? "ON" : "OFF")}) via {origin.name}");

            return true;
        }

        LeverTap RaycastForLever(Vector3 origin, Vector3 direction, string sourceName)
        {
            Ray ray = new Ray(origin, direction.normalized);
            RaycastHit[] hits = Physics.SphereCastAll(
                ray, rayRadius, range, hitMask, QueryTriggerInteraction.Ignore);

            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (RaycastHit hit in hits)
            {
                LeverTap lever = hit.collider.GetComponentInParent<LeverTap>();
                if (lever != null)
                    return lever;
            }

            if (debugLog)
                Debug.Log($"LeverTap: no lever hit from {sourceName}");

            return null;
        }

        static Transform FindTransform(string objectName)
        {
            foreach (GameObject go in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                if (go.name == objectName)
                    return go.transform;
            }

            return null;
        }
    }
}
