using System.Collections;
using UnityEngine;

namespace Room1Puzzle
{
    /// <summary>
    /// Shows intro/rules image on a camera billboard at Play start.
    /// Assign Sprite OR Texture2D (PNG) in the Inspector.
    /// </summary>
    public class IntroNote : MonoBehaviour
    {
        [Header("Intro image — drag Sprite OR PNG Texture")]
        public Sprite introSprite;

        [Tooltip("Optional. Drag PNG here if it is not imported as Sprite.")]
        public Texture2D introTexture;

        [Header("Timing")]
        public float visibleSeconds = 6f;
        public float fadeOutSeconds = 1f;

        [Header("Billboard layout")]
        public float distanceFromCamera = 0.65f;
        public float heightMeters = 0.42f;

        bool introQueued;

        void Start()
        {
            PlayIntro();
        }

        public void PlayIntro()
        {
            if (introQueued)
                return;

            introQueued = true;
            StartCoroutine(ShowIntroBillboard());
        }

        IEnumerator ShowIntroBillboard()
        {
            Camera cam = null;
            for (int i = 0; i < 180; i++)
            {
                cam = Room1NoteDisplay.ResolveCamera();
                if (cam != null)
                    break;

                yield return null;
            }

            if (cam == null)
            {
                Debug.LogWarning("IntroNote: no camera found for intro billboard.");
                yield break;
            }

            Texture tex = Room1NoteImage.GetWorldTexture(introSprite, introTexture);
            if (tex == null)
            {
                Debug.LogWarning("IntroNote: assign introSprite or introTexture in the Inspector.");
                yield break;
            }

            GameObject root = new GameObject("IntroBillboard");
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "IntroImage";
            quad.transform.SetParent(root.transform, false);

            Collider quadCol = quad.GetComponent<Collider>();
            if (quadCol != null)
                Destroy(quadCol);

            Renderer rend = quad.GetComponent<Renderer>();
            rend.material = CreateImageMaterial(tex);
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rend.receiveShadows = false;

            float aspect = tex.height > 0 ? (float)tex.width / tex.height : 1f;
            quad.transform.localScale = new Vector3(heightMeters * aspect, heightMeters, 1f);

            root.transform.SetParent(cam.transform, false);
            root.transform.localPosition = new Vector3(0f, 0.04f, distanceFromCamera);
            root.transform.localRotation = Quaternion.identity;

            Debug.Log("[Room1 intro] showing rules image.");

            yield return new WaitForSeconds(visibleSeconds);

            float fade = Mathf.Max(0.05f, fadeOutSeconds);
            float t = 0f;
            Color startColor = Color.white;

            while (t < fade)
            {
                t += Time.deltaTime;
                float a = Mathf.Lerp(1f, 0f, t / fade);
                SetMaterialAlpha(rend.material, a);
                yield return null;
            }

            Destroy(root);
        }

        static Material CreateImageMaterial(Texture tex)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
                shader = Shader.Find("Unlit/Texture");

            Material mat = new Material(shader);
            mat.mainTexture = tex;

            if (mat.HasProperty("_BaseMap"))
                mat.SetTexture("_BaseMap", tex);

            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", Color.white);

            return mat;
        }

        static void SetMaterialAlpha(Material mat, float alpha)
        {
            if (mat == null)
                return;

            if (mat.HasProperty("_BaseColor"))
            {
                Color c = mat.GetColor("_BaseColor");
                c.a = alpha;
                mat.SetColor("_BaseColor", c);
            }

            Color col = mat.color;
            col.a = alpha;
            mat.color = col;
        }
    }
}
