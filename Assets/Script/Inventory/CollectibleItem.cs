using UnityEngine;
using System;

public class CollectibleItem : MonoBehaviour
{
    public ItemData itemData;
    public int itemAmount;

    public event Action OnItemDestroyed;

    public void Collect()
    {
        bool addedToInventory = InventoryManager.Instance.AddItem(itemData, itemAmount);

        if (addedToInventory)
        {
            OnItemDestroyed?.Invoke();
            Destroy(gameObject);
        }
    }
}