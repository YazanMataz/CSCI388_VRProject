using UnityEngine;
using UnityEngine.SceneManagement;

namespace Room1Puzzle
{
    public static class Room1SceneBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void BootstrapRoom1()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.name.Contains("Room1"))
                return;

            foreach (LeverTapPointer old in Object.FindObjectsByType<LeverTapPointer>(FindObjectsSortMode.None))
                old.enabled = false;

            Room1NoteDisplay.EnsureExists();
            EnsureRealtimeLighting();

            GameObject panelObject = FindSwitchPanelObject();
            if (panelObject == null)
                return;

            SwitchPanel panel = panelObject.GetComponent<SwitchPanel>();
            if (panel == null)
                panel = panelObject.AddComponent<SwitchPanel>();

            Room1SwitchRuntimeSetup.Prepare(panel);

            IntroNote intro = Object.FindFirstObjectByType<IntroNote>();
            if (intro != null)
                intro.PlayIntro();
        }

        static void EnsureRealtimeLighting()
        {
            if (Object.FindFirstObjectByType<Room1RealtimeLighting>() != null)
                return;

            GameObject host = new GameObject("Room1RealtimeLighting");
            host.AddComponent<Room1RealtimeLighting>();
        }

        static GameObject FindSwitchPanelObject()
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
