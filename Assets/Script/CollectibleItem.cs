using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    public ItemData itemData;
    public int itemAmount;

    public void Collect()
    {
        bool addedToInventory = InventoryManager.Instance.AddItem(itemData, itemAmount);

        if (addedToInventory)
        {
            Destroy(gameObject);
        }
    }
}