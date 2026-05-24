using UnityEngine;
using UnityEngine.Events;

namespace Room1Puzzle
{
    /// <summary>
    /// Put on each switch collider in your panel. Toggle via SwitchPointer raycast.
    /// Wire visuals yourself — optional off/on objects or material color.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Switch : MonoBehaviour
    {
        [Header("Visuals (optional)")]
        public GameObject offVisual;
        public GameObject onVisual;
        public Renderer colorTarget;
        public Color offColor = new Color(0.85f, 0.15f, 0.15f);
        public Color onColor = new Color(0.15f, 0.85f, 0.25f);

        public UnityEvent<bool> onStateChanged;

        public bool IsOn { get; private set; }

        SwitchPanel panel;

        void Awake()
        {
            panel = GetComponentInParent<SwitchPanel>();
            ApplyVisuals(false);
        }

        public void Toggle()
        {
            SetOn(!IsOn);
        }

        public void SetOn(bool on)
        {
            if (IsOn == on) return;
            IsOn = on;
            ApplyVisuals(IsOn);
            onStateChanged?.Invoke(IsOn);
            panel?.OnSwitchChanged();
        }

        void ApplyVisuals(bool on)
        {
            if (offVisual != null) offVisual.SetActive(!on);
            if (onVisual != null) onVisual.SetActive(on);

            if (colorTarget != null)
                colorTarget.material.color = on ? onColor : offColor;
        }
    }
}
