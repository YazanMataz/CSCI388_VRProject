using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class GarageSequenceManager : MonoBehaviour
{
    public XRSocketInteractor[] sockets;
    public string[] correctSequence;
    public GameObject successUI;
    public TextMeshProUGUI messageText;

    private bool completed = false;

    public void CheckSequence()
    {
        if (completed) return;

        for (int i = 0; i < sockets.Length; i++)
        {
            if (sockets[i] == null)
            {
                Debug.LogError("Socket " + i + " is not assigned.");
                return;
            }

            if (!sockets[i].hasSelection)
            {
                ShowMessage("Place all 4 items into the sockets.");
                return;
            }

            var selected = sockets[i].GetOldestInteractableSelected();

            if (selected == null)
            {
                ShowMessage("Socket has no selected item.");
                return;
            }

            SequenceObject obj = selected.transform.GetComponent<SequenceObject>();

            if (obj == null)
            {
                ShowMessage("Selected item is missing SequenceObject script.");
                return;
            }

            if (obj.objectID != correctSequence[i])
            {
                ShowMessage("Wrong order. Check the hint again.");
                return;
            }
        }

        completed = true;
        ShowMessage("Sequence Complete!");

        if (successUI != null)
            successUI.SetActive(true);
        else
            Debug.LogError("Success UI is not assigned.");

        GameManager.Instance.LoadNextRoom();
    }

    private void ShowMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;
        else
            Debug.LogWarning("Message Text is not assigned.");
    }
}