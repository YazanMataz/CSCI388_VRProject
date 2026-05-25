#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Room1Puzzle
{
    public static class Room1SwitchSetupEditor
    {
        [MenuItem("Room1/Auto-Setup Switch Puzzle")]
        public static void AutoSetup()
        {
            int changes = 0;

            GameObject panel = FindSwitchPanel();
            if (panel == null)
            {
                EditorUtility.DisplayDialog(
                    "Room 1 Setup",
                    "Create a GameObject named \"switch panel\" with Switch1, Switch2, Switch3 as children.",
                    "OK");
                return;
            }

            changes += SetupSwitchPanel(panel);
            changes += SetupIntroNote();
            changes += SetupClueNotes();
            changes += SetupSwitchesOnPanel(panel);
            changes += SetupSwitchPointer();
            changes += SetupNoteDisplay();
            changes += SetupSwitchPanelLabel(panel);
            changes += SetupInventoryManager();
            changes += TryWireDoor(panel.GetComponent<SwitchPanel>());

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            EditorUtility.DisplayDialog(
                "Room 1 Setup",
                $"Done ({changes} steps).\n\n" +
                "1. Drag PNG/Sprite into IntroNote (introSprite OR introTexture)\n" +
                "2. Drag PNG/Sprite into Note1/2/3 (noteSprite OR noteTexture)\n" +
                "3. Set each ClueNote switchMustBeUp to match your clues\n" +
                "4. Drag victory sound onto Switch Panel → victoryClip\n" +
                "5. Save scene and Play",
                "OK");

            Selection.activeGameObject = panel;
        }

        static GameObject FindSwitchPanel()
        {
            foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                string n = go.name.Trim().ToLowerInvariant();
                if (n.Contains("switch panel"))
                    return go;
            }

            return null;
        }

        static int SetupSwitchPanel(GameObject panel)
        {
            int changes = 0;

            LeverPuzzlePanel old = panel.GetComponent<LeverPuzzlePanel>();
            if (old != null)
            {
                Object.DestroyImmediate(old);
                changes++;
            }

            SwitchPanel switchPanel = panel.GetComponent<SwitchPanel>();
            if (switchPanel == null)
            {
                switchPanel = panel.AddComponent<SwitchPanel>();
                changes++;
            }

            switchPanel.useClueNotesForPattern = true;
            switchPanel.autoSetupOnAwake = true;
            EditorUtility.SetDirty(switchPanel);
            return changes;
        }

        static int SetupIntroNote()
        {
            GameObject intro = GameObject.Find("IntroNote");
            if (intro == null)
            {
                intro = new GameObject("IntroNote");
                Undo.RegisterCreatedObjectUndo(intro, "Create IntroNote");
            }

            if (intro.GetComponent<IntroNote>() == null)
            {
                intro.AddComponent<IntroNote>();
                EditorUtility.SetDirty(intro);
                return 1;
            }

            return 0;
        }

        static int SetupClueNotes()
        {
            int changes = 0;
            changes += EnsureClueNoteObject("NOTE1", 1);
            changes += EnsureClueNoteObject("NOTE2", 2);
            changes += EnsureClueNoteObject("NOTE3", 3);
            return changes;
        }

        static GameObject FindNoteObject(string noteName)
        {
            foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                if (string.Equals(go.name, noteName, System.StringComparison.OrdinalIgnoreCase))
                    return go;
            }

            return null;
        }

        static int EnsureClueNoteObject(string noteName, int switchNumber)
        {
            GameObject note = FindNoteObject(noteName);
            if (note == null)
                return 0;

            int changes = 0;

            ClueNote clue = note.GetComponent<ClueNote>();
            if (clue == null)
            {
                clue = note.AddComponent<ClueNote>();
                changes++;
            }

            int previousNumber = clue.switchNumber;
            clue.switchNumber = switchNumber;
            if (previousNumber != switchNumber)
                clue.switchMustBeUp = DefaultSwitchMustBeUp(switchNumber);

            if (note.GetComponent<Collider>() == null)
            {
                BoxCollider box = note.AddComponent<BoxCollider>();
                box.size = new Vector3(0.3f, 0.4f, 0.02f);
                changes++;
            }

            EditorUtility.SetDirty(note);
            return changes;
        }

        static bool DefaultSwitchMustBeUp(int switchNumber)
        {
            switch (switchNumber)
            {
                case 1: return true;
                case 2: return false;
                case 3: return true;
                default: return true;
            }
        }

        static int SetupSwitchesOnPanel(GameObject panel)
        {
            int changes = 0;

            foreach (Transform child in panel.transform)
            {
                string name = child.name.ToLowerInvariant();
                if (!name.Contains("switch"))
                    continue;

                Switch sw = child.GetComponent<Switch>();
                if (sw == null)
                {
                    sw = child.gameObject.AddComponent<Switch>();
                    changes++;
                }

                Transform handle = Switch.FindHandle(child);
                if (handle != null)
                    sw.handle = handle;

                sw.ConfigureDimmerSlide(0.056f, -0.056f, beginOn: false);
                sw.FixInteractionColliders();
                EditorUtility.SetDirty(sw);
            }

            SwitchPanel switchPanel = panel.GetComponent<SwitchPanel>();
            if (switchPanel != null)
            {
                var list = new System.Collections.Generic.List<Switch>(panel.GetComponentsInChildren<Switch>());
                list.Sort((a, b) => a.transform.localPosition.x.CompareTo(b.transform.localPosition.x));
                switchPanel.switches = list.ToArray();

                for (int i = 0; i < switchPanel.switches.Length; i++)
                {
                    Switch sw = switchPanel.switches[i];
                    if (sw == null)
                        continue;

                    int number = Switch.ParseSwitchNumber(sw.gameObject.name);
                    if (number <= 0)
                        number = i + 1;

                    sw.ConfigureFromPanel(switchPanel, number);
                }

                EditorUtility.SetDirty(switchPanel);
            }

            return changes;
        }

        static int SetupSwitchPointer()
        {
            foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                if (!go.name.Contains("XR Origin"))
                    continue;

                if (go.GetComponent<SwitchPointer>() == null)
                {
                    go.AddComponent<SwitchPointer>();
                    EditorUtility.SetDirty(go);
                    return 1;
                }

                return 0;
            }

            return 0;
        }

        static int SetupSwitchPanelLabel(GameObject panel)
        {
            if (panel.transform.Find("SwitchPanelLabel") != null)
                return 0;

            Room1PanelLabelMenu.AddSwitchPanelLabel();
            return 1;
        }

        static int SetupNoteDisplay()
        {
            if (Object.FindFirstObjectByType<Room1NoteDisplay>() != null)
                return 0;

            GameObject go = new GameObject("Room1NoteDisplay");
            go.AddComponent<Room1NoteDisplay>();
            Undo.RegisterCreatedObjectUndo(go, "Create Room1NoteDisplay");
            return 1;
        }

        static int SetupInventoryManager()
        {
            if (Object.FindFirstObjectByType<InventoryManager>() != null)
                return 0;

            GameObject go = new GameObject("InventoryManager");
            go.AddComponent<InventoryManager>();
            Undo.RegisterCreatedObjectUndo(go, "Create InventoryManager");
            return 1;
        }

        static int TryWireDoor(SwitchPanel switchPanel)
        {
            if (switchPanel == null || switchPanel.exitDoor != null || switchPanel.doorObject != null)
                return 0;

            ExitDoorController door = Object.FindFirstObjectByType<ExitDoorController>();
            if (door != null)
            {
                switchPanel.exitDoor = door;
                EditorUtility.SetDirty(switchPanel);
                return 1;
            }

            foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                if (go.name.ToLowerInvariant().Contains("door"))
                {
                    switchPanel.doorObject = go;
                    EditorUtility.SetDirty(switchPanel);
                    return 1;
                }
            }

            return 0;
        }
    }
}
#endif
