using UnityEngine;

public class DrawerKeyPickup : MonoBehaviour
{
    public void PickUpKey()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.GetDrawerKey();
        }

        gameObject.SetActive(false);
    }
}