using UnityEngine;

public class RoomKeyPickup : MonoBehaviour
{
    public void PickUpRoomKey()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.GetRoomKey();
        }

        gameObject.SetActive(false);
    }
}