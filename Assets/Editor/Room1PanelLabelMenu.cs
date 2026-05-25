#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Room1Puzzle
{
    public static class Room1PanelLabelMenu
    {
        [MenuItem("Room1/Add Switch Panel Label (SWITCH PANEL)")]
        public static void AddSwitchPanelLabel()
        {
            GameObject panel = FindSwitchPanel();
            if (panel == null)
            {
                EditorUtility.DisplayDialog("Room 1", "Could not find GameObject named \"switch panel\".", "OK");
                return;
            }

            Transform existing = panel.transform.Find("SwitchPanelLabel");
            if (existing != null)
            {
                Selection.activeGameObject = existing.gameObject;
                EditorGUIUtility.PingObject(existing.gameObject);
                return;
            }

            GameObject labelGo = new GameObject("SwitchPanelLabel");
            Undo.RegisterCreatedObjectUndo(labelGo, "Add Switch Panel Label");
            labelGo.transform.SetParent(panel.transform, false);
            labelGo.transform.localPosition = new Vector3(0f, 0.28f, 0f);
            labelGo.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            TextMeshPro tmp = labelGo.AddComponent<TextMeshPro>();
            tmp.text = "SWITCH PANEL";
            tmp.fontSize = 0.14f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            if (TMP_Settings.defaultFontAsset != null)
            {
                tmp.font = TMP_Settings.defaultFontAsset;
                if (TMP_Settings.defaultFontAsset.material != null)
                    tmp.fontSharedMaterial = TMP_Settings.defaultFontAsset.material;
            }

            tmp.outlineWidth = 0.08f;
            tmp.outlineColor = Color.black;

            Selection.activeGameObject = labelGo;
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
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
    }
}
#endif
