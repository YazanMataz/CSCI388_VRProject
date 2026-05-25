#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Room1Puzzle
{
    [CustomEditor(typeof(Room1NoteSetup))]
    public class Room1NoteSetupEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(12);

            var setup = (Room1NoteSetup)target;

            EditorGUILayout.HelpBox(
                "Drag PNG or Sprite into the fields above, then click Apply.\n" +
                "PNG Texture = drag file as Default Texture.\n" +
                "Sprite = set PNG Texture Type to Sprite (2D and UI) first.",
                MessageType.Info);

            GUI.backgroundColor = new Color(0.4f, 0.85f, 0.5f);
            if (GUILayout.Button("Apply Images To All Notes", GUILayout.Height(36)))
            {
                setup.ApplyToAllNotes();
                EditorUtility.SetDirty(setup);
                MarkNotesDirty(setup);
                EditorSceneManager.MarkSceneDirty(setup.gameObject.scene);
            }
            GUI.backgroundColor = Color.white;
        }

        static void MarkNotesDirty(Room1NoteSetup setup)
        {
            IntroNote intro = setup.GetComponentInChildren<IntroNote>();
            if (intro == null)
            {
                GameObject go = GameObject.Find("IntroNote");
                if (go != null) intro = go.GetComponent<IntroNote>();
            }
            if (intro != null) EditorUtility.SetDirty(intro);

            foreach (string name in new[] { "Note1", "Note2", "Note3" })
            {
                GameObject go = GameObject.Find(name);
                if (go != null) EditorUtility.SetDirty(go);
            }
        }
    }

    public static class Room1NoteSetupMenu
    {
        [MenuItem("Room1/Create Note Image Setup (drag all PNGs here)")]
        public static void CreateSetupObject()
        {
            Room1NoteSetup existing = Object.FindFirstObjectByType<Room1NoteSetup>();
            if (existing != null)
            {
                Selection.activeGameObject = existing.gameObject;
                EditorGUIUtility.PingObject(existing.gameObject);
                return;
            }

            GameObject go = new GameObject("Room1NoteSetup");
            go.AddComponent<Room1NoteSetup>();
            Undo.RegisterCreatedObjectUndo(go, "Create Room1NoteSetup");
            Selection.activeGameObject = go;
            EditorSceneManager.MarkSceneDirty(go.scene);
        }
    }
}
#endif
