using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Room1Puzzle
{
    /// <summary>
    /// Raycast interact: G / click / VR trigger on switches and clue notes.
    /// </summary>
    [DefaultExecutionOrder(50)]
    public class SwitchPointer : MonoBehaviour
    {
        public static SwitchPointer Instance { get; private set; }

        public float range = 20f;
        [Tooltip("How close the ray must pass to a switch handle (meters).")]
        public float switchAimRadius = 0.1f;
        public LayerMask hitMask = ~0;

        Transform headRay;
        Transform rightRay;
        Transform leftRay;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;

            foreach (LeverTapPointer old in FindObjectsByType<LeverTapPointer>(FindObjectsSortMode.None))
                old.enabled = false;
        }

        void Start()
        {
            ResolveRays();
        }

        void LateUpdate()
        {
            ResolveRays();

            if (!WasInteractPressedThisFrame())
                return;

            if (TryInteractFromTransform(headRay))
                return;

            if (TryInteractFromTransform(rightRay))
                return;

            TryInteractFromTransform(leftRay);
        }

        void ResolveRays()
        {
            if (headRay == null && Camera.main != null)
                headRay = Camera.main.transform;

            if (rightRay == null)
                rightRay = FindExactTransform("Right Controller");

            if (leftRay == null)
                leftRay = FindExactTransform("Left Controller");
        }

        bool WasInteractPressedThisFrame()
        {
            if (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame)
                return true;

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                return true;

            return AnyXrTriggerPressedThisFrame();
        }

        static bool AnyXrTriggerPressedThisFrame()
        {
            foreach (InputDevice device in InputSystem.devices)
            {
                if (!device.enabled)
                    continue;

                ButtonControl trigger = device.TryGetChildControl<ButtonControl>("triggerPressed");
                if (trigger != null && trigger.wasPressedThisFrame)
                    return true;

                ButtonControl triggerButton = device.TryGetChildControl<ButtonControl>("triggerButton");
                if (triggerButton != null && triggerButton.wasPressedThisFrame)
                    return true;
            }

            return false;
        }

        bool TryInteractFromTransform(Transform origin)
        {
            if (origin == null)
                return false;

            Ray ray = new Ray(origin.position, origin.forward);

            Switch aimed = PickAimedSwitch(ray, range, switchAimRadius);
            if (aimed != null)
            {
                aimed.Toggle();
                return true;
            }

            RaycastHit[] hits = Physics.RaycastAll(ray, range, hitMask, QueryTriggerInteraction.Collide);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (RaycastHit hit in hits)
            {
                if (IsSwitchPanelBlocker(hit.collider))
                    continue;

                ClueNote note = hit.collider.GetComponent<ClueNote>();
                if (note == null)
                    note = hit.collider.GetComponentInParent<ClueNote>();

                if (note != null)
                {
                    note.Read();
                    return true;
                }
            }

            return false;
        }

        public static Switch PickAimedSwitch(Ray ray, float maxRange, float aimRadius)
        {
            Switch[] switches = FindObjectsByType<Switch>(FindObjectsSortMode.None);
            Switch best = null;
            float bestLateral = aimRadius;

            foreach (Switch sw in switches)
            {
                if (sw == null || sw.handle == null)
                    continue;

                Vector3 handlePos = sw.handle.position;
                Vector3 toHandle = handlePos - ray.origin;
                float forward = Vector3.Dot(toHandle, ray.direction);
                if (forward < 0.02f || forward > maxRange)
                    continue;

                Vector3 closestOnRay = ray.origin + ray.direction * forward;
                float lateral = Vector3.Distance(closestOnRay, handlePos);
                if (lateral >= bestLateral)
                    continue;

                bestLateral = lateral;
                best = sw;
            }

            return best;
        }

        static bool IsSwitchPanelBlocker(Collider col)
        {
            if (col == null)
                return false;

            Transform t = col.transform;
            while (t != null)
            {
                string n = t.name.ToLowerInvariant();
                if (n.Contains("switch panel"))
                    return true;

                t = t.parent;
            }

            return false;
        }

        static Transform FindExactTransform(string objectName)
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
