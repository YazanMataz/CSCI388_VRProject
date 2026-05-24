using UnityEngine;

namespace Room1Puzzle
{
    /// <summary>
    /// Parent of your switch panel. All switches must be ON to unlock the door.
    /// </summary>
    public class SwitchPanel : MonoBehaviour
    {
        [Tooltip("Leave empty to auto-find Switch components in children.")]
        public Switch[] switches;

        public ExitDoorController exitDoor;
        public AudioClip successClip;

        bool solved;
        AudioSource audioSource;

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;

            if (switches == null || switches.Length == 0)
                switches = GetComponentsInChildren<Switch>();

            if (exitDoor == null)
                exitDoor = FindFirstObjectByType<ExitDoorController>();
        }

        public void OnSwitchChanged()
        {
            if (solved || switches == null) return;

            foreach (Switch s in switches)
            {
                if (s == null || !s.IsOn) return;
            }

            solved = true;
            if (successClip != null)
                audioSource.PlayOneShot(successClip);

            exitDoor?.UnlockAndOpen();
        }
    }
}
