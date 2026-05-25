#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Room1Puzzle
{
    [CustomEditor(typeof(ClueNote))]
    public class ClueNoteEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var note = (ClueNote)target;

            EditorGUILayout.Space(8);
            EditorGUILayout.HelpBox(
                "Drag PNG into Note Texture for the paper in the world.\n" +
                "Use a Quad (not a thin Cube) so the image faces the player.",
                MessageType.Info);

            if (GUILayout.Button("Refresh World Image Preview"))
            {
                note.EditorApplyWorldTexture();
                EditorUtility.SetDirty(note);
            }
        }
    }
}
#endif
