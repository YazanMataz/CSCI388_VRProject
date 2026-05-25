using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Room1Puzzle
{
    public class SwitchPanel : MonoBehaviour
    {
        [Header("Switches (left → right)")]
        public Switch[] switches;

        [Header("Solution")]
        [Tooltip("Built from Note1, Note2, Note3 unless disabled.")]
        public bool useClueNotesForPattern = true;

        [Tooltip("Fallback if no clue notes found. true = UP/ON")]
        public bool[] correctPattern = { true, false, true };

        [Header("Exit")]
        public ExitDoorController exitDoor;
        public GameObject doorObject;

        [Header("Victory")]
        public AudioClip victoryClip;

        [Header("UI Feedback")]
        public string switchOnMessage = "Switch {0} is ON";
        public string switchOffMessage = "Switch {0} is OFF";
        public string successMessage = "You got the correct combination!";
        public float switchMessageDuration = 2f;
        public float successMessageDuration = 5f;

        [Header("Auto Setup")]
        public bool autoSetupOnAwake = true;

        bool solved;
        AudioSource audioSource;

        void Awake()
        {
            if (autoSetupOnAwake)
                Room1SwitchRuntimeSetup.Prepare(this);

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;

            DisablePanelBlocker();

            if (switches == null || switches.Length == 0)
                switches = CollectSwitchesSorted(transform);

            AssignSwitchNumbers();

            if (useClueNotesForPattern)
                BuildPatternFromClueNotes();

            EnsureDoorController();
        }

        void DisablePanelBlocker()
        {
            Collider panelCollider = GetComponent<Collider>();
            if (panelCollider != null)
                panelCollider.enabled = false;
        }

        void AssignSwitchNumbers()
        {
            if (switches == null)
                return;

            for (int i = 0; i < switches.Length; i++)
            {
                if (switches[i] == null)
                    continue;

                int number = Switch.ParseSwitchNumber(switches[i].gameObject.name);
                if (number <= 0)
                    number = i + 1;

                switches[i].ConfigureFromPanel(this, number);
                switches[i].FixInteractionColliders();
            }
        }

        void BuildPatternFromClueNotes()
        {
            int count = switches != null && switches.Length > 0 ? switches.Length : 3;
            var pattern = NormalizePattern(correctPattern, count);

            ClueNote[] notes = FindObjectsByType<ClueNote>(FindObjectsSortMode.None);
            bool found = false;

            foreach (ClueNote note in notes)
            {
                if (note == null || note.switchNumber < 1 || note.switchNumber > count)
                    continue;

                pattern[note.switchNumber - 1] = note.switchMustBeUp;
                found = true;
            }

            correctPattern = pattern;
            Debug.Log($"SwitchPanel solution: {FormatPattern(correctPattern)}{(found ? " (from clue notes)" : " (fallback)")}");
        }

        static bool[] NormalizePattern(bool[] source, int count)
        {
            bool[] fallback = { true, false, true };
            var pattern = new bool[count];

            for (int i = 0; i < count; i++)
            {
                if (source != null && i < source.Length)
                    pattern[i] = source[i];
                else if (i < fallback.Length)
                    pattern[i] = fallback[i];
                else
                    pattern[i] = false;
            }

            return pattern;
        }

        static string FormatPattern(bool[] pattern)
        {
            if (pattern == null || pattern.Length == 0)
                return "(empty)";

            var parts = new string[pattern.Length];
            for (int i = 0; i < pattern.Length; i++)
                parts[i] = $"Switch {i + 1}={(pattern[i] ? "UP" : "DOWN")}";

            return string.Join(", ", parts);
        }

        void EnsureDoorController()
        {
            if (exitDoor != null)
                return;

            exitDoor = FindFirstObjectByType<ExitDoorController>();
            if (exitDoor != null)
                return;

            if (doorObject == null)
            {
                foreach (GameObject go in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
                {
                    if (go.name.ToLowerInvariant().Contains("door"))
                    {
                        doorObject = go;
                        break;
                    }
                }
            }

            if (doorObject == null)
                return;

            exitDoor = doorObject.GetComponent<ExitDoorController>();
            if (exitDoor == null)
                exitDoor = doorObject.AddComponent<ExitDoorController>();

            if (exitDoor.doorLeaf == null)
                exitDoor.doorLeaf = doorObject.transform;
        }

        public void OnSwitchChanged(Switch changedSwitch)
        {
            CheckSolution();
        }

        void CheckSolution()
        {
            if (solved || switches == null || switches.Length == 0)
                return;

            if (correctPattern == null || switches.Length != correctPattern.Length)
            {
                Debug.LogWarning(
                    $"SwitchPanel: {switches.Length} switches but pattern has {correctPattern?.Length ?? 0} entries.");
                return;
            }

            for (int i = 0; i < switches.Length; i++)
            {
                if (switches[i] == null || switches[i].IsOn != correctPattern[i])
                    return;
            }

            solved = true;

            if (victoryClip != null)
                audioSource.PlayOneShot(victoryClip);

            Room1NoteDisplay.EnsureExists().ShowFeedback(successMessage, successMessageDuration, keepAfterDuration: true);

            if (InventoryManager.Instance != null)
                InventoryManager.Instance.hasRoom1Completed = true;

            UnlockDoor();
        }

        void UnlockDoor()
        {
            EnsureDoorController();

            if (exitDoor != null)
            {
                exitDoor.UnlockAndOpen();
                return;
            }

            if (doorObject != null)
                doorObject.SetActive(false);
        }

        static Switch[] CollectSwitchesSorted(Transform panel)
        {
            var list = new List<Switch>(panel.GetComponentsInChildren<Switch>());
            list.Sort((a, b) => a.transform.localPosition.x.CompareTo(b.transform.localPosition.x));
            return list.ToArray();
        }

#if UNITY_EDITOR
        [ContextMenu("Sort Switches Left To Right")]
        void EditorSortSwitches()
        {
            switches = CollectSwitchesSorted(transform);
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
