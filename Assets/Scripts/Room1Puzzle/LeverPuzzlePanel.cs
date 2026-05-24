using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Room1Puzzle
{
    public class LeverPuzzlePanel : MonoBehaviour
    {
        [Header("Solution (left → right on panel)")]
        [Tooltip("true = ON, false = OFF")]
        public bool[] correctPattern = { true, false, true, false, true };

        [Header("Exit")]
        public ExitDoorController exitDoor;
        public GameObject doorObject;

        [Header("Audio")]
        public AudioClip successClip;

        [Header("UI Feedback")]
        [Tooltip("Drag a TextMeshProUGUI here (on a Screen Space Canvas).")]
        public TextMeshProUGUI feedbackText;

        [Tooltip("Shown when a lever is turned ON. {0} = lever number (1–5).")]
        public string switchOnMessage = "Switch {0} is ON";

        public string successMessage = "You got the correct combination!";

        [Tooltip("How long ON messages stay visible. Success message stays longer.")]
        public float switchOnMessageDuration = 2f;
        public float successMessageDuration = 5f;

        LeverTap[] levers;
        bool solved;
        AudioSource audio;
        Coroutine hideRoutine;

        void Awake()
        {
            DisablePanelBlocker();

            audio = GetComponent<AudioSource>();
            if (audio == null)
                audio = gameObject.AddComponent<AudioSource>();
            audio.spatialBlend = 1f;

            if (exitDoor == null)
                exitDoor = FindFirstObjectByType<ExitDoorController>();

            levers = ConfigureTapLevers();

            if (feedbackText != null)
                feedbackText.text = string.Empty;
        }

        void DisablePanelBlocker()
        {
            Collider panelCollider = GetComponent<Collider>();
            if (panelCollider != null)
                panelCollider.enabled = false;
        }

        public void OnLeverChanged(LeverTap lever)
        {
            if (solved || lever == null) return;

            if (lever.IsOn)
            {
                int index = GetLeverIndex(lever);
                ShowMessage(string.Format(switchOnMessage, index), switchOnMessageDuration);
            }

            CheckSolution();
        }

        int GetLeverIndex(LeverTap lever)
        {
            if (levers == null) return 0;

            for (int i = 0; i < levers.Length; i++)
            {
                if (levers[i] == lever)
                    return i + 1;
            }

            return 0;
        }

        public LeverTap[] ConfigureTapLevers()
        {
            DisablePanelBlocker();

            List<Transform> roots = CollectLeverRoots(transform);
            var result = new LeverTap[roots.Count];

            for (int i = 0; i < roots.Count; i++)
                result[i] = EnsureTapLever(roots[i]);

            levers = result;
            return result;
        }

        static List<Transform> CollectLeverRoots(Transform panel)
        {
            var roots = new List<Transform>();
            foreach (Transform child in panel)
            {
                if (child.name.Contains("Lever"))
                    roots.Add(child);
            }

            roots.Sort((a, b) => a.localPosition.x.CompareTo(b.localPosition.x));
            return roots;
        }

        static LeverTap EnsureTapLever(Transform root)
        {
            RemoveInteractableConflicts(root);

            LeverTap tap = root.GetComponent<LeverTap>();
            if (tap == null)
                tap = root.gameObject.AddComponent<LeverTap>();

            if (tap.handle == null)
                tap.handle = FindHandle(root);

            EnsureHitCollider(tap.handle != null ? tap.handle : root);
            return tap;
        }

        static void EnsureHitCollider(Transform target)
        {
            BoxCollider box = target.GetComponent<BoxCollider>();
            if (box == null)
            {
                box = target.gameObject.AddComponent<BoxCollider>();
                box.center = new Vector3(0f, 0.075f, 0f);
                box.size = new Vector3(0.2f, 0.35f, 0.15f);
            }
            else
            {
                box.size *= 1.6f;
            }
        }

        static void RemoveInteractableConflicts(Transform root)
        {
            XRLever grab = root.GetComponent<XRLever>();
            if (grab != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(grab);
                else
#endif
                    Destroy(grab);
            }

            XRSimpleInteractable simple = root.GetComponent<XRSimpleInteractable>();
            if (simple != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(simple);
                else
#endif
                    Destroy(simple);
            }
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

        public void CheckSolution()
        {
            if (solved || levers == null || levers.Length == 0) return;

            if (correctPattern == null || levers.Length != correctPattern.Length)
            {
                Debug.LogWarning(
                    $"LeverPuzzlePanel: {levers.Length} levers but pattern has {correctPattern?.Length ?? 0} entries.");
                return;
            }

            for (int i = 0; i < levers.Length; i++)
            {
                if (levers[i] == null || levers[i].IsOn != correctPattern[i])
                    return;
            }

            solved = true;

            if (successClip != null)
                audio.PlayOneShot(successClip);

            ShowMessage(successMessage, successMessageDuration);

            if (exitDoor != null)
                exitDoor.UnlockAndOpen();
            else if (doorObject != null)
                doorObject.SetActive(false);
        }

        void ShowMessage(string message, float duration)
        {
            if (feedbackText != null)
                feedbackText.text = message;

            Debug.Log(message);

            if (hideRoutine != null)
                StopCoroutine(hideRoutine);

            if (duration > 0f)
                hideRoutine = StartCoroutine(HideMessageAfter(duration));
        }

        IEnumerator HideMessageAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);

            if (!solved && feedbackText != null)
                feedbackText.text = string.Empty;
        }

#if UNITY_EDITOR
        [ContextMenu("Setup Tap Levers")]
        public void EditorSetupTapLevers()
        {
            ConfigureTapLevers();
            UnityEditor.EditorUtility.SetDirty(gameObject);
        }
#endif
    }
}
