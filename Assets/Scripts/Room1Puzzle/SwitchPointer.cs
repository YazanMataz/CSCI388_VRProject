using UnityEngine;
using UnityEngine.XR;

namespace Room1Puzzle
{
    /// <summary>
    /// Add to Main Camera (XR Origin). Look at switch + click / trigger to toggle.
    /// </summary>
    public class SwitchPointer : MonoBehaviour
    {
        public float range = 6f;
        public LayerMask hitMask = ~0;

        bool triggerWasDown;

        void Update()
        {
            bool triggerDown = Input.GetMouseButton(0)
                || TriggerPressed(XRNode.RightHand)
                || TriggerPressed(XRNode.LeftHand);

            if (triggerDown && !triggerWasDown)
                TryHitSwitch();

            triggerWasDown = triggerDown;
        }

        static bool TriggerPressed(XRNode node)
        {
            var device = InputDevices.GetDeviceAtXRNode(node);
            return device.isValid
                && device.TryGetFeatureValue(CommonUsages.triggerButton, out bool pressed)
                && pressed;
        }

        void TryHitSwitch()
        {
            Ray ray = new Ray(transform.position, transform.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
                return;

            Switch sw = hit.collider.GetComponent<Switch>();
            sw?.Toggle();
        }
    }
}
