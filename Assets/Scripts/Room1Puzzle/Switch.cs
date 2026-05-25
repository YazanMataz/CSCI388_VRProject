using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Room1Puzzle
{
    /// <summary>
    /// Vertical dimmer switch. TOP = ON (green), BOTTOM = OFF (red).
    /// </summary>
    public class Switch : MonoBehaviour
    {
        [Header("Handle")]
        public Transform handle;

        [Header("Slide positions (local Y)")]
        public float topLocalY = 0.056f;
        public float bottomLocalY = -0.056f;
        public float moveSpeed = 8f;

        [Header("Start")]
        public bool startOn;

        [Header("Glow colors")]
        public Color onColor = new Color(0.15f, 0.95f, 0.2f);
        public Color offColor = new Color(0.95f, 0.15f, 0.15f);
        public float emissionStrength = 0.6f;

        public UnityEvent<bool> onStateChanged;

        public bool IsOn { get; private set; }
        public int switchNumber;

        SwitchPanel panel;
        Renderer colorTarget;
        MaterialPropertyBlock colorBlock;
        Coroutine moveRoutine;

        void Awake()
        {
            ResolveHandle();
            FixInteractionColliders();
            SetOn(startOn, notify: false, instant: true);
        }

        void Start()
        {
            if (panel == null)
                panel = GetComponentInParent<SwitchPanel>();
            if (panel == null)
                panel = FindFirstObjectByType<SwitchPanel>();
        }

        public void ConfigureFromPanel(SwitchPanel owner, int number)
        {
            panel = owner;
            switchNumber = number;
        }

        public void ConfigureDimmerSlide(float topY, float bottomY, bool beginOn = false)
        {
            topLocalY = topY;
            bottomLocalY = bottomY;
            startOn = beginOn;
            ResolveHandle();
            SetOn(startOn, notify: false, instant: true);
        }

        void ResolveHandle()
        {
            if (handle == null)
                handle = FindHandle(transform) ?? transform;

            colorTarget = handle.GetComponent<Renderer>();
            if (colorTarget == null)
                colorTarget = handle.GetComponentInChildren<Renderer>();
        }

        void EnsureInteractionCollider()
        {
            FixInteractionColliders();
        }

        public void FixInteractionColliders()
        {
            ResolveHandle();

            foreach (Collider col in GetComponentsInChildren<Collider>(true))
            {
                bool isHandle = IsHandleTransform(col.transform);
                col.enabled = isHandle;
            }

            if (handle == null)
                return;

            BoxCollider handleCol = handle.GetComponent<BoxCollider>();
            if (handleCol == null)
                handleCol = handle.gameObject.AddComponent<BoxCollider>();

            handleCol.enabled = true;
            handleCol.isTrigger = false;
            handleCol.center = Vector3.zero;

            Vector3 worldSize = new Vector3(0.05f, 0.1f, 0.04f);
            Vector3 localSize = handle.InverseTransformVector(worldSize);
            handleCol.size = new Vector3(
                Mathf.Max(Mathf.Abs(localSize.x), 0.01f),
                Mathf.Max(Mathf.Abs(localSize.y), 0.01f),
                Mathf.Max(Mathf.Abs(localSize.z), 0.01f));
        }

        bool IsHandleTransform(Transform t)
        {
            if (handle == null)
                handle = FindHandle(transform) ?? transform;

            return t == handle || t.IsChildOf(handle);
        }

        public static bool IsHandleHit(Collider col, Switch sw)
        {
            if (col == null || sw == null)
                return false;

            Transform handle = sw.handle != null ? sw.handle : FindHandle(sw.transform);
            if (handle == null)
                return false;

            return col.transform == handle || col.transform.IsChildOf(handle);
        }

        public static int ParseSwitchNumber(string objectName)
        {
            if (string.IsNullOrEmpty(objectName))
                return 0;

            string n = objectName.ToLowerInvariant();
            if (n.Contains("switch1") || n.Contains("switch 1"))
                return 1;
            if (n.Contains("switch2") || n.Contains("switch 2"))
                return 2;
            if (n.Contains("switch3") || n.Contains("switch 3"))
                return 3;

            return 0;
        }

        public void Toggle()
        {
            SetOn(!IsOn);
        }

        public void SetOn(bool on, bool notify = true, bool instant = false)
        {
            if (IsOn == on && !instant)
                return;

            IsOn = on;

            if (instant)
                ApplyY(on ? topLocalY : bottomLocalY);
            else
                AnimateTo(on ? topLocalY : bottomLocalY);

            ApplyGlow(on);

            if (notify)
            {
                onStateChanged?.Invoke(IsOn);

                if (panel == null)
                    panel = GetComponentInParent<SwitchPanel>();
                if (panel == null)
                    panel = FindFirstObjectByType<SwitchPanel>();

                if (switchNumber > 0)
                {
                    float duration = panel != null ? panel.switchMessageDuration : 2f;
                    Room1NoteDisplay.EnsureExists().ShowSwitchFeedback(switchNumber, IsOn, duration);
                }

                panel?.OnSwitchChanged(this);
            }
        }

        void ApplyGlow(bool on)
        {
            if (colorTarget == null || !Application.isPlaying)
                return;

            if (colorBlock == null)
                colorBlock = new MaterialPropertyBlock();

            Color c = on ? onColor : offColor;
            colorTarget.GetPropertyBlock(colorBlock);
            colorBlock.SetColor("_Color", c);
            colorBlock.SetColor("_BaseColor", c);
            colorBlock.SetColor("_EmissionColor", c * emissionStrength);
            colorTarget.SetPropertyBlock(colorBlock);
        }

        void AnimateTo(float targetY)
        {
            if (moveRoutine != null)
                StopCoroutine(moveRoutine);
            moveRoutine = StartCoroutine(SlideRoutine(targetY));
        }

        IEnumerator SlideRoutine(float targetY)
        {
            float startY = handle.localPosition.y;
            float elapsed = 0f;
            float duration = Mathf.Max(0.08f, Mathf.Abs(targetY - startY) / moveSpeed);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float y = Mathf.Lerp(startY, targetY, Mathf.SmoothStep(0f, 1f, elapsed / duration));
                ApplyY(y);
                yield return null;
            }

            ApplyY(targetY);
            moveRoutine = null;
        }

        void ApplyY(float y)
        {
            Vector3 p = handle.localPosition;
            p.y = y;
            handle.localPosition = p;
        }

        public static Transform FindHandle(Transform root)
        {
            Transform best = null;
            foreach (Transform t in root.GetComponentsInChildren<Transform>())
            {
                string n = t.name.ToLowerInvariant();
                if (!n.Contains("dimmer_handle"))
                    continue;

                if (best == null)
                    best = t;
            }

            if (best != null)
                return best;

            foreach (Transform t in root.GetComponentsInChildren<Transform>())
            {
                string n = t.name.ToLowerInvariant();
                if (n.Contains("handle") && !n.Contains("board"))
                    return t;
            }

            return null;
        }
    }
}
