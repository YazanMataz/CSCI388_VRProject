using UnityEngine;

namespace Room1Puzzle
{
    /// <summary>
    /// Accept either a Sprite or a PNG Texture2D in Inspector fields.
    /// </summary>
    public static class Room1NoteImage
    {
        static Material s_noteMaterialTemplate;

        public static Sprite GetDisplaySprite(Sprite sprite, Texture2D texture)
        {
            if (sprite != null)
                return sprite;

            if (texture == null || !texture.isReadable)
                return null;

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f);
        }

        public static Texture GetWorldTexture(Sprite sprite, Texture2D texture)
        {
            if (sprite != null)
                return sprite.texture;

            return texture;
        }

        public static void ApplyToRenderer(Renderer rend, Texture tex)
        {
            if (rend == null || tex == null)
                return;

            if (s_noteMaterialTemplate == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (shader == null)
                    shader = Shader.Find("Unlit/Texture");

                s_noteMaterialTemplate = new Material(shader);
                s_noteMaterialTemplate.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            }

            Material mat = new Material(s_noteMaterialTemplate);
            mat.mainTexture = tex;

            if (mat.HasProperty("_BaseMap"))
                mat.SetTexture("_BaseMap", tex);

            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", Color.white);

            rend.material = mat;
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rend.receiveShadows = false;
        }
    }
}
