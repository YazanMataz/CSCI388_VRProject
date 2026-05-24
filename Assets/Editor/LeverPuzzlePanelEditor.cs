#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Room1Puzzle
{
    [CustomEditor(typeof(LeverPuzzlePanel))]
    public class LeverPuzzlePanelEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "UI setup:\n" +
                "1. GameObject > UI > Canvas (Screen Space - Overlay)\n" +
                "2. Right-click Canvas > UI > Text - TextMeshPro\n" +
                "3. Drag that text into Feedback Text on this panel\n" +
                "4. Setup Tap Levers + Play",
                MessageType.Info);

            if (GUILayout.Button("Setup Tap Levers"))
            {
                var panel = (LeverPuzzlePanel)target;
                panel.ConfigureTapLevers();
                EditorUtility.SetDirty(panel);
            }
        }
    }
}
#endif
