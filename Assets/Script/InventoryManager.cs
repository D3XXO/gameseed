using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Inventory Settings")]
    public int inventorySize = 12;

    public List<InventorySlot> inventorySlots = new List<InventorySlot>();

    public delegate void OnInventoryChanged();
    public event OnInventoryChanged onInventoryChangedCallback;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        for (int i = 0; i < inventorySize; i++)
        {
            inventorySlots.Add(new InventorySlot());
        }
    }

    public bool AddItem(ItemData itemToAdd, int amount = 1)
    {
        if (itemToAdd == null)
        {
            return false;
        }

        if (itemToAdd.isStackable)
        {
            foreach (InventorySlot slot in inventorySlots)
            {
                if (slot.itemData == itemToAdd && slot.CanAddAmount(amount))
                {
                    slot.AddAmount(amount);
                    onInventoryChangedCallback?.Invoke();
                    return true;
                }
            }
        }

        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.IsEmpty())
            {
                slot.itemData = itemToAdd;
                slot.amount = amount;
                onInventoryChangedCallback?.Invoke();
                return true;
            }
        }

        return false;
    }

    public bool RemoveItem(ItemData itemToRemove, int amount = 1)
    {
        InventorySlot targetSlot = inventorySlots.FirstOrDefault(slot => slot.itemData == itemToRemove && slot.amount >= amount);

        if (targetSlot != null)
        {
            targetSlot.RemoveAmount(amount);
            if (targetSlot.IsEmpty())
            {
                targetSlot.Clear();
            }
            onInventoryChangedCallback?.Invoke();
            return true;
        }

        return false;
    }

    public InventorySlot GetSlot(int index)
    {
        if (index >= 0 && index < inventorySlots.Count)
        {
            return inventorySlots[index];
        }
        return null;
    }

    public bool HasItem(ItemData item, int amount = 1)
    {
        int count = 0;
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.itemData == item)
            {
                count += slot.amount;
            }
        }
        return count >= amount;
    }
}