using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SocketFeedback : MonoBehaviour
{
    public TextMeshProUGUI messageText;

    private XRSocketInteractor socket;

    void Awake()
    {
        socket = GetComponent<XRSocketInteractor>();
    }

    void OnEnable()
    {
        socket.selectEntered.AddListener(OnPlaced);
        socket.selectExited.AddListener(OnRemoved);
    }

    void OnDisable()
    {
        socket.selectEntered.RemoveListener(OnPlaced);
        socket.selectExited.RemoveListener(OnRemoved);
    }

    void OnPlaced(SelectEnterEventArgs args)
    {
        messageText.text = "Item placed in socket.";
    }

    void OnRemoved(SelectExitEventArgs args)
    {
        messageText.text = "Item removed from socket.";
    }
}