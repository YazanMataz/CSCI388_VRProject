using UnityEngine;

namespace Room1Puzzle
{
    public static class Room1SwitchRuntimeSetup
    {
        const float TopY = 0.056f;
        const float BottomY = -0.056f;

        public static void Prepare(SwitchPanel panel)
        {
            if (panel == null)
                return;

            LeverPuzzlePanel old = panel.GetComponent<LeverPuzzlePanel>();
            if (old != null)
                old.enabled = false;

            Collider panelCol = panel.GetComponent<Collider>();
            if (panelCol != null)
                panelCol.enabled = false;

            foreach (Collider col in panel.GetComponentsInChildren<Collider>(true))
            {
                Switch sw = col.GetComponentInParent<Switch>();
                if (sw != null)
                    continue;

                col.enabled = false;
            }

            Room1NoteDisplay.EnsureExists();
            EnsureIntroNote();
            EnsureClueNotes();
            EnsureAllSwitches(panel.transform);
            EnsureInteractor();
            EnsureInventoryManager();

            panel.switches = panel.GetComponentsInChildren<Switch>();
            System.Array.Sort(panel.switches, (a, b) =>
                a.transform.localPosition.x.CompareTo(b.transform.localPosition.x));

            for (int i = 0; i < panel.switches.Length; i++)
            {
                Switch sw = panel.switches[i];
                if (sw == null)
                    continue;

                int number = Switch.ParseSwitchNumber(sw.gameObject.name);
                if (number <= 0)
                    number = i + 1;

                sw.ConfigureFromPanel(panel, number);
                sw.FixInteractionColliders();
            }

            Debug.Log($"Room1 setup: {panel.switches.Length} switches ready.");
            for (int i = 0; i < panel.switches.Length; i++)
            {
                Switch sw = panel.switches[i];
                if (sw == null)
                    continue;

                Debug.Log($"  {sw.gameObject.name} -> Switch {sw.switchNumber} at X={sw.transform.localPosition.x:F3}");
            }
        }

        static void EnsureIntroNote()
        {
            IntroNote intro = Object.FindFirstObjectByType<IntroNote>();
            if (intro != null)
                return;

            foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                if (go.name != "IntroNote")
                    continue;

                if (go.GetComponent<IntroNote>() == null)
                    go.AddComponent<IntroNote>();
                return;
            }

            GameObject created = new GameObject("IntroNote");
            created.AddComponent<IntroNote>();
        }

        static void EnsureClueNotes()
        {
            WireClueNote("NOTE1", 1);
            WireClueNote("NOTE2", 2);
            WireClueNote("NOTE3", 3);
        }

        static void WireClueNote(string objectName, int switchNumber)
        {
            foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                if (!NamesMatch(go.name, objectName))
                    continue;

                ClueNote note = go.GetComponent<ClueNote>();
                if (note == null)
                    note = go.AddComponent<ClueNote>();

                int previousNumber = note.switchNumber;
                note.switchNumber = switchNumber;

                if (previousNumber != switchNumber)
                    note.switchMustBeUp = DefaultSwitchMustBeUp(switchNumber);
            }
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

        static bool NamesMatch(string a, string b)
        {
            return string.Equals(a, b, System.StringComparison.OrdinalIgnoreCase);
        }

        static void EnsureAllSwitches(Transform panel)
        {
            foreach (Transform child in panel)
            {
                if (IsBackboard(child.name))
                    continue;

                SetupSwitchRoot(child);
            }

            foreach (Transform t in panel.GetComponentsInChildren<Transform>(true))
            {
                if (!t.name.ToLowerInvariant().Contains("dimmer_handle"))
                    continue;

                if (t.parent == null || t.parent == panel)
                    continue;

                SetupSwitchRoot(t.parent);
            }
        }

        static bool SetupSwitchRoot(Transform switchRoot)
        {
            Switch sw = switchRoot.GetComponent<Switch>();
            if (sw == null)
                sw = switchRoot.gameObject.AddComponent<Switch>();

            DisableBoardColliders(switchRoot);

            Transform handle = Switch.FindHandle(switchRoot);
            if (handle == null)
                return false;

            sw.handle = handle;
            sw.ConfigureDimmerSlide(TopY, BottomY, beginOn: false);
            sw.FixInteractionColliders();
            return true;
        }

        static void DisableBoardColliders(Transform switchRoot)
        {
            Collider rootCol = switchRoot.GetComponent<Collider>();
            if (rootCol != null)
                rootCol.enabled = false;

            foreach (Transform child in switchRoot)
            {
                if (!child.name.ToLowerInvariant().Contains("board"))
                    continue;

                foreach (Collider col in child.GetComponents<Collider>())
                    col.enabled = false;
            }
        }

        static bool IsBackboard(string name)
        {
            string n = name.ToLowerInvariant();
            return n == "cube" || n.Contains("backboard");
        }

        static void EnsureInteractor()
        {
            if (SwitchPointer.Instance != null)
                return;

            GameObject host = null;
            foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                if (go.name.Contains("XR Origin"))
                {
                    host = go;
                    break;
                }
            }

            if (host == null && Camera.main != null)
                host = Camera.main.transform.root.gameObject;

            if (host != null && host.GetComponent<SwitchPointer>() == null)
                host.AddComponent<SwitchPointer>();
        }

        static void EnsureInventoryManager()
        {
            if (Object.FindFirstObjectByType<InventoryManager>() != null)
                return;

            new GameObject("InventoryManager").AddComponent<InventoryManager>();
        }
    }
}
