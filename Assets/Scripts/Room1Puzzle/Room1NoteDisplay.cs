using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Room1Puzzle
{
    /// <summary>
    /// Screen-space HUD in front of the VR/main camera for intro images + switch feedback.
    /// </summary>
    public class Room1NoteDisplay : MonoBehaviour
    {
        public static Room1NoteDisplay Instance { get; private set; }

        [Header("Camera HUD")]
        public float planeDistance = 0.55f;

        [Header("Layout")]
        [Range(0.2f, 0.95f)] public float screenFill = 0.72f;

        [Header("Feedback text")]
        public float feedbackFontSize = 54f;

        Canvas canvas;
        CanvasGroup popupGroup;
        Transform popupRoot;
        Image popupImage;
        RawImage popupRawImage;
        TextMeshProUGUI feedbackText;
        Coroutine imageRoutine;
        Coroutine feedbackRoutine;
        bool feedbackPersists;

        void Awake()
        {
            if (HasWorldContentChildren())
            {
                Debug.LogWarning("Room1NoteDisplay: move NOTE1/2/3 off this object. Creating Room1Hud instead.");
                Destroy(this);
                return;
            }

            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            BuildUi();
            BindCamera();
            HideImageInstant();
            ClearFeedbackInstant();
        }

        void LateUpdate()
        {
            BindCamera();
        }

        public static Room1NoteDisplay EnsureExists()
        {
            if (Instance != null)
            {
                Instance.EnsureUiBuilt();
                Instance.BindCamera();
                return Instance;
            }

            foreach (Room1NoteDisplay existing in FindObjectsByType<Room1NoteDisplay>(FindObjectsSortMode.None))
            {
                if (existing != null && !existing.HasWorldContentChildren())
                {
                    Instance = existing;
                    Instance.EnsureUiBuilt();
                    Instance.BindCamera();
                    return Instance;
                }
            }

            GameObject go = new GameObject("Room1Hud");
            return go.AddComponent<Room1NoteDisplay>();
        }

        void EnsureUiBuilt()
        {
            if (canvas == null)
                BuildUi();
        }

        public static Camera ResolveCamera()
        {
            Camera cam = Camera.main;
            if (cam != null && cam.enabled && cam.gameObject.activeInHierarchy)
                return cam;

            Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
            foreach (Camera candidate in cameras)
            {
                if (candidate == null || !candidate.enabled || !candidate.gameObject.activeInHierarchy)
                    continue;

                if (candidate.CompareTag("MainCamera"))
                    return candidate;
            }

            foreach (Camera candidate in cameras)
            {
                if (candidate != null && candidate.enabled && candidate.gameObject.activeInHierarchy)
                    return candidate;
            }

            return null;
        }

        void BindCamera()
        {
            if (canvas == null)
                return;

            Camera cam = ResolveCamera();
            if (cam == null)
                return;

            if (canvas.worldCamera != cam)
            {
                canvas.worldCamera = cam;
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.planeDistance = planeDistance;
            }
        }

        bool HasWorldContentChildren()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (IsUiChildName(child.name))
                    continue;

                if (child.GetComponent<Graphic>() != null)
                    continue;

                return true;
            }

            return false;
        }

        static bool IsUiChildName(string name)
        {
            return name == "PopupRoot"
                || name == "PopupImage"
                || name == "PopupRawImage"
                || name == "FeedbackPanel"
                || name == "FeedbackText";
        }

        void BuildUi()
        {
            if (canvas != null)
                return;

            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.planeDistance = planeDistance;
            canvas.sortingOrder = 500;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            gameObject.AddComponent<GraphicRaycaster>();

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            canvasRect.sizeDelta = Vector2.zero;
            canvasRect.localScale = Vector3.one;

            GameObject popupGo = new GameObject("PopupRoot");
            popupGo.transform.SetParent(transform, false);
            popupRoot = popupGo.transform;

            RectTransform popupRect = popupGo.AddComponent<RectTransform>();
            popupRect.anchorMin = Vector2.zero;
            popupRect.anchorMax = Vector2.one;
            popupRect.offsetMin = Vector2.zero;
            popupRect.offsetMax = Vector2.zero;

            popupGroup = popupGo.AddComponent<CanvasGroup>();
            popupGroup.alpha = 0f;
            popupGroup.blocksRaycasts = false;

            popupImage = CreatePopupImage<Image>(popupRoot, "PopupImage");
            popupRawImage = CreatePopupImage<RawImage>(popupRoot, "PopupRawImage");
            popupRawImage.gameObject.SetActive(false);

            feedbackText = CreateFeedbackText();
            BindCamera();
        }

        T CreatePopupImage<T>(Transform parent, string objectName) where T : Graphic
        {
            GameObject imageGo = new GameObject(objectName);
            imageGo.transform.SetParent(parent, false);

            T graphic = imageGo.AddComponent<T>();
            graphic.raycastTarget = false;
            graphic.color = Color.white;

            if (graphic is Image image)
                image.preserveAspect = true;

            if (graphic is RawImage rawImage)
                rawImage.uvRect = new Rect(0f, 0f, 1f, 1f);

            RectTransform rt = graphic.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(1920f * screenFill, 1080f * screenFill);

            return graphic;
        }

        TextMeshProUGUI CreateFeedbackText()
        {
            GameObject panelGo = new GameObject("FeedbackPanel");
            panelGo.transform.SetParent(transform, false);

            RectTransform panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0f);
            panelRect.anchorMax = new Vector2(0.5f, 0f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.anchoredPosition = new Vector2(0f, 40f);
            panelRect.sizeDelta = new Vector2(1500f, 120f);

            Image panelBg = panelGo.AddComponent<Image>();
            panelBg.color = new Color(0f, 0f, 0f, 0.72f);
            panelBg.raycastTarget = false;

            GameObject textGo = new GameObject("FeedbackText");
            textGo.transform.SetParent(panelGo.transform, false);

            RectTransform textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(16f, 8f);
            textRect.offsetMax = new Vector2(-16f, -8f);

            TextMeshProUGUI text = textGo.AddComponent<TextMeshProUGUI>();
            ApplyDefaultFont(text);
            text.raycastTarget = false;
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = feedbackFontSize;
            text.color = new Color(0.2f, 1f, 0.35f, 1f);
            text.fontStyle = FontStyles.Bold;
            text.enableWordWrapping = true;
            text.text = string.Empty;

            panelGo.SetActive(false);
            return text;
        }

        static void ApplyDefaultFont(TextMeshProUGUI text)
        {
            if (TMP_Settings.defaultFontAsset == null)
                return;

            text.font = TMP_Settings.defaultFontAsset;
            if (TMP_Settings.defaultFontAsset.material != null)
                text.fontSharedMaterial = TMP_Settings.defaultFontAsset.material;
        }

        public void ShowPopup(Sprite sprite, Texture2D texture, float duration)
        {
            if (sprite == null && texture == null)
                return;

            EnsureExists();
            BindCamera();
            if (imageRoutine != null)
                StopCoroutine(imageRoutine);

            imageRoutine = StartCoroutine(PopupRoutine(sprite, texture, duration, 0.15f, 0.25f, duration));
        }

        public void ShowIntro(Sprite sprite, Texture2D texture, float visibleSeconds, float fadeOutSeconds)
        {
            if (sprite == null && texture == null)
                return;

            EnsureExists();
            BindCamera();
            if (imageRoutine != null)
                StopCoroutine(imageRoutine);

            imageRoutine = StartCoroutine(PopupRoutine(sprite, texture, visibleSeconds, 0.35f, fadeOutSeconds, visibleSeconds));
        }

        public void ShowSwitchFeedback(int switchNumber, bool isOn, float duration = 2f)
        {
            string state = isOn ? "ON" : "OFF";
            ShowFeedback($"Switch {switchNumber} is {state}", duration);
        }

        public void ShowFeedback(string message, float duration, bool keepAfterDuration = false)
        {
            if (string.IsNullOrEmpty(message))
                return;

            EnsureExists();
            BindCamera();

            if (feedbackText == null)
                feedbackText = CreateFeedbackText();

            feedbackPersists = keepAfterDuration;
            feedbackText.text = message;
            feedbackText.ForceMeshUpdate();

            Transform panel = feedbackText.transform.parent;
            if (panel != null)
                panel.gameObject.SetActive(true);

            Debug.Log("[Room1 feedback] " + message);

            if (feedbackRoutine != null)
                StopCoroutine(feedbackRoutine);

            if (duration > 0f)
                feedbackRoutine = StartCoroutine(HideFeedbackAfter(duration));
        }

        IEnumerator HideFeedbackAfter(float seconds)
        {
            yield return new WaitForSeconds(seconds);

            if (!feedbackPersists)
                ClearFeedbackInstant();

            feedbackRoutine = null;
        }

        IEnumerator PopupRoutine(Sprite sprite, Texture2D texture, float holdSeconds, float fadeIn, float fadeOut, float holdOverride)
        {
            for (int i = 0; i < 90 && ResolveCamera() == null; i++)
                yield return null;

            BindCamera();
            yield return null;

            if (popupGroup == null || popupImage == null || popupRawImage == null)
                yield break;

            if (sprite != null)
            {
                popupImage.sprite = sprite;
                popupImage.gameObject.SetActive(true);
                popupRawImage.gameObject.SetActive(false);
            }
            else
            {
                popupRawImage.texture = texture;
                popupRawImage.gameObject.SetActive(true);
                popupImage.gameObject.SetActive(false);
            }

            popupGroup.blocksRaycasts = false;
            yield return FadePopupTo(1f, fadeIn);
            yield return new WaitForSeconds(holdOverride);
            yield return FadePopupTo(0f, fadeOut);
            HideImageInstant();
            imageRoutine = null;
        }

        IEnumerator FadePopupTo(float targetAlpha, float duration)
        {
            float start = popupGroup.alpha;
            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                popupGroup.alpha = Mathf.Lerp(start, targetAlpha, t / duration);
                yield return null;
            }

            popupGroup.alpha = targetAlpha;
        }

        void HideImageInstant()
        {
            if (popupGroup != null)
                popupGroup.alpha = 0f;

            if (popupImage != null)
                popupImage.gameObject.SetActive(false);

            if (popupRawImage != null)
                popupRawImage.gameObject.SetActive(false);
        }

        void ClearFeedbackInstant()
        {
            if (feedbackText == null)
                return;

            feedbackText.text = string.Empty;

            Transform panel = feedbackText.transform.parent;
            if (panel != null)
                panel.gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
