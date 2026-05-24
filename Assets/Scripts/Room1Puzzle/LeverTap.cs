using UnityEngine;

namespace Room1Puzzle
{
    /// <summary>
    /// Tap to toggle. Matches course-library lever motion (tilt down/up).
    /// </summary>
    public class LeverTap : MonoBehaviour
    {
        [Tooltip("Auto-found: Lever_Switch or Lever_Switch2")]
        public Transform handle;

        [Header("Motion")]
        [Tooltip("ON = stick tilted up/out. OFF = stick tilted down/in.")]
        public float onAngle = 35f;
        public float offAngle = -35f;

        public bool startOn;

        public bool IsOn { get; private set; }

        LeverPuzzlePanel panel;
        Quaternion restLocalRotation;

        void Awake()
        {
            panel = GetComponentInParent<LeverPuzzlePanel>();

            if (handle == null)
                handle = FindHandle(transform);

            if (handle != null)
                restLocalRotation = handle.localRotation;

            SetOn(startOn, notify: false);
        }

        public void Toggle()
        {
            SetOn(!IsOn);
        }

        public void SetOn(bool on, bool notify = true)
        {
            IsOn = on;
            ApplyPose();

            if (notify)
                panel?.OnLeverChanged(this);
        }

        void ApplyPose()
        {
            if (handle == null) return;

            // Parent local X = hinge for course levers (stick mesh goes up in Y).
            float angle = IsOn ? onAngle : offAngle;
            handle.localRotation = Quaternion.AngleAxis(angle, Vector3.right) * restLocalRotation;
        }

        static Transform FindHandle(Transform root)
        {
            Transform best = null;
            foreach (Transform t in root.GetComponentsInChildren<Transform>())
            {
                if (!t.name.Contains("Lever_Switch"))
                    continue;

                if (best == null || t.name == "Lever_Switch")
                    best = t;
            }

            return best;
        }
    }
}
