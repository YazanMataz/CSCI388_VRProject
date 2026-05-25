using UnityEngine;

namespace Room1Puzzle
{
    /// <summary>
    /// Drag all note PNGs/Sprites here, then click Apply in the Inspector (or context menu).
    /// </summary>
    public class Room1NoteSetup : MonoBehaviour
    {
        [Header("Intro rules paper")]
        public Sprite introSprite;
        public Texture2D introTexture;

        [Header("Note 1 — Switch 1 clue")]
        public Sprite note1Sprite;
        public Texture2D note1Texture;
        public Sprite note1PopupSprite;
        public Texture2D note1PopupTexture;

        [Header("Note 2 — Switch 2 clue")]
        public Sprite note2Sprite;
        public Texture2D note2Texture;
        public Sprite note2PopupSprite;
        public Texture2D note2PopupTexture;

        [Header("Note 3 — Switch 3 clue")]
        public Sprite note3Sprite;
        public Texture2D note3Texture;
        public Sprite note3PopupSprite;
        public Texture2D note3PopupTexture;

        [ContextMenu("Apply Images To All Notes")]
        public void ApplyToAllNotes()
        {
            ApplyIntro();
            ApplyClue("Note1", note1Sprite, note1Texture, note1PopupSprite, note1PopupTexture);
            ApplyClue("Note2", note2Sprite, note2Texture, note2PopupSprite, note2PopupTexture);
            ApplyClue("Note3", note3Sprite, note3Texture, note3PopupSprite, note3PopupTexture);
            Debug.Log("Room1NoteSetup: applied images to IntroNote + Note1/2/3.");
        }

        void ApplyIntro()
        {
            IntroNote intro = FindIntro();
            if (intro == null)
            {
                Debug.LogWarning("Room1NoteSetup: IntroNote not found. Create GameObject named IntroNote with IntroNote script.");
                return;
            }

            intro.introSprite = introSprite;
            intro.introTexture = introTexture;
        }

        void ApplyClue(string noteName, Sprite sprite, Texture2D texture, Sprite popupSprite, Texture2D popupTexture)
        {
            ClueNote note = FindClue(noteName);
            if (note == null)
            {
                Debug.LogWarning($"Room1NoteSetup: {noteName} not found in scene.");
                return;
            }

            note.noteSprite = sprite;
            note.noteTexture = texture;
            note.popupSprite = popupSprite;
            note.popupTexture = popupTexture;

#if UNITY_EDITOR
            note.EditorApplyWorldTexture();
#else
            note.ApplyWorldTexture();
#endif
        }

        static IntroNote FindIntro()
        {
            IntroNote intro = FindFirstObjectByType<IntroNote>();
            if (intro != null)
                return intro;

            GameObject go = GameObject.Find("IntroNote");
            return go != null ? go.GetComponent<IntroNote>() : null;
        }

        static ClueNote FindClue(string noteName)
        {
            foreach (ClueNote note in FindObjectsByType<ClueNote>(FindObjectsSortMode.None))
            {
                if (string.Equals(note.name, noteName, System.StringComparison.OrdinalIgnoreCase))
                    return note;
            }

            foreach (GameObject go in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            {
                if (string.Equals(go.name, noteName, System.StringComparison.OrdinalIgnoreCase))
                {
                    ClueNote note = go.GetComponent<ClueNote>();
                    if (note != null)
                        return note;
                }
            }

            return null;
        }
    }
}
