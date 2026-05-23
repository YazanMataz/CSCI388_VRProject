using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class DrawerOpenClose : MonoBehaviour
{
    [Header("Drawer Movement")]
    public Transform drawerSection;

    public Vector3 closedLocalPosition;
    public Vector3 openedLocalPosition;

    [Header("Open Amount")]
    public float openZOffset = -0.7f;

    [Header("Speed")]
    public float moveSpeed = 2f;

    [Header("Lock")]
    public bool requireDrawerKey = true;

    private bool isOpen = false;
    private bool permanentlyOpened = false;
    private Vector3 targetPosition;

    private XRBaseInteractable interactable;

    void Start()
    {
        if (drawerSection == null)
        {
            drawerSection = transform;
        }

        interactable = GetComponent<XRBaseInteractable>();

        closedLocalPosition = drawerSection.localPosition;
        openedLocalPosition = closedLocalPosition + new Vector3(0f, 0f, openZOffset);

        targetPosition = closedLocalPosition;
    }

    void Update()
    {
        drawerSection.localPosition = Vector3.Lerp(
            drawerSection.localPosition,
            targetPosition,
            Time.deltaTime * moveSpeed
        );
    }

    public void TryToggleDrawer()
    {
        if (permanentlyOpened) return;

        if (requireDrawerKey)
        {
            if (InventoryManager.Instance == null || !InventoryManager.Instance.hasDrawerKey)
            {
                if (InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.ShowMessage("The drawer is locked. Find the Drawer Key!", -5f);
                }

                return;
            }
        }

        OpenDrawerPermanently();
    }

    private void OpenDrawerPermanently()
    {
        isOpen = true;
        permanentlyOpened = true;
        targetPosition = openedLocalPosition;

        if (interactable != null)
        {
            interactable.enabled = false;
        }
    }
}