using UnityEngine;

namespace Room1Puzzle
{
    /// <summary>
    /// World clue note quad. Assign Sprite OR Texture2D (PNG) for world + popup.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ClueNote : MonoBehaviour
    {
        [Header("World note — drag Sprite OR PNG Texture")]
        public Sprite noteSprite;

        [Tooltip("Optional. Drag PNG here if it is not imported as Sprite.")]
        public Texture2D noteTexture;

        [Header("Popup image (optional — uses world image if empty)")]
        public Sprite popupSprite;

        public Texture2D popupTexture;

        [Header("Switch clue")]
        [Tooltip("1 = Switch 1, 2 = Switch 2, 3 = Switch 3")]
        public int switchNumber = 1;

        [Tooltip("True = switch must be UP (ON). False = DOWN (OFF).")]
        public bool switchMustBeUp = true;

        [Header("Popup timing")]
        public float popupSeconds = 4f;

        [Header("Paper size (world units)")]
        public float paperWidth = 0.25f;
        public float paperHeight = 0.35f;

        static Mesh quadMesh;

        void Awake()
        {
            CleanupWrongComponents();
            EnsurePaperMesh();
            ApplyWorldTexture();
            EnsureCollider();
        }

        void OnValidate()
        {
            if (!Application.isPlaying)
                EnsurePaperMesh();

            ApplyWorldTexture();
        }

        void CleanupWrongComponents()
        {
            Room1NoteDisplay strayDisplay = GetComponent<Room1NoteDisplay>();
            if (strayDisplay != null)
                Destroy(strayDisplay);
        }

        void EnsurePaperMesh()
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null)
                meshFilter = gameObject.AddComponent<MeshFilter>();

            MeshRenderer renderer = GetComponent<MeshRenderer>();
            if (renderer == null)
                renderer = gameObject.AddComponent<MeshRenderer>();

            if (quadMesh == null)
            {
                GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quadMesh = temp.GetComponent<MeshFilter>().sharedMesh;
                if (Application.isPlaying)
                    Destroy(temp);
                else
                    DestroyImmediate(temp);
            }

            meshFilter.sharedMesh = quadMesh;

            Vector3 scale = transform.localScale;
            bool thinCube = scale.y > 0f && scale.y < 0.01f;
            if (thinCube || scale.y > scale.x * 2f)
            {
                float width = scale.x > 0.01f ? scale.x : paperWidth;
                float height = scale.z > 0.01f ? scale.z : paperHeight;
                transform.localScale = new Vector3(width, height, 1f);
            }
        }

        public void Read()
        {
            Sprite popup = Room1NoteImage.GetDisplaySprite(popupSprite, popupTexture);
            Texture2D popupTex = popup == null ? (popupTexture != null ? popupTexture : noteTexture) : null;

            if (popup == null)
                popup = Room1NoteImage.GetDisplaySprite(noteSprite, noteTexture);

            if (popup == null && popupTex == null)
            {
                Debug.LogWarning($"ClueNote {name}: assign noteSprite/noteTexture or popupSprite/popupTexture.");
                return;
            }

            Room1NoteDisplay.EnsureExists().ShowPopup(popup, popupTex, popupSeconds);
        }

        public void ApplyWorldTexture()
        {
            Texture tex = Room1NoteImage.GetWorldTexture(noteSprite, noteTexture);
            if (tex == null)
                return;

            Renderer rend = GetComponent<Renderer>();
            if (rend == null)
                return;

            Room1NoteImage.ApplyToRenderer(rend, tex);
        }

#if UNITY_EDITOR
        public void EditorApplyWorldTexture()
        {
            EnsurePaperMesh();
            ApplyWorldTexture();
        }
#endif

        void EnsureCollider()
        {
            BoxCollider box = GetComponent<BoxCollider>();
            if (box == null)
                box = gameObject.AddComponent<BoxCollider>();

            box.size = new Vector3(1f, 1f, 0.05f);
            box.center = Vector3.zero;
        }
    }
}
