using UnityEngine;
using TMPro;
using System.Collections;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Inventory")]
    public bool hasDrawerKey = false;
    public bool hasRoomKey = false;

    [Header("UI")]
    public TextMeshProUGUI messageText;
    public float messageDuration = 2f;

    private Coroutine messageCoroutine;

    void Awake()
    {
        Instance = this;

        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
        }
    }

    public void GetDrawerKey()
    {
        hasDrawerKey = true;
        ShowMessage("You obtained Drawer Key!");
    }

    public void GetRoomKey()
    {
        hasRoomKey = true;
        ShowMessage("You have obtained the Room Key!");
    }

    public void ShowMessage(string message, float xOffset = 0f)
    {
        if (messageText == null) return;

        RectTransform rect = messageText.GetComponent<RectTransform>();

        if (rect != null)
        {
            rect.anchoredPosition = new Vector2(xOffset, rect.anchoredPosition.y);
        }

        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }

        messageCoroutine = StartCoroutine(ShowMessageRoutine(message));
    }

    IEnumerator ShowMessageRoutine(string message)
    {
        messageText.text = message;
        messageText.gameObject.SetActive(true);

        yield return new WaitForSeconds(messageDuration);

        messageText.gameObject.SetActive(false);
    }
}