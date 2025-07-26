using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Inventory Settings")]
    public int inventorySize;

    public List<InventorySlot> inventorySlots = new List<InventorySlot>();

    public InventorySlot activeHandSlot = new InventorySlot();

    public delegate void OnActiveHandChanged(ItemData newItemData);
    public event OnActiveHandChanged onActiveHandChangedCallback;

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

        if (inventorySlots.Count == 0)
        {
            for (int i = 0; i < inventorySize; i++)
            {
                inventorySlots.Add(new InventorySlot());
            }
        }
    }

    public void ResetInventoryState()
    {
        Debug.Log("Resetting InventoryManager state.");
        foreach (InventorySlot slot in inventorySlots)
        {
            slot.Clear();
        }
        ClearActiveHandItem();
        TriggerInventoryChanged();
    }

    public void TriggerInventoryChanged()
    {
        onInventoryChangedCallback?.Invoke();
    }

    public void TriggerActiveHandChanged(ItemData newItemData)
    {
        onActiveHandChangedCallback?.Invoke(newItemData);
    }

    public bool SetActiveHandItem(int inventorySlotIndex)
    {
        if (inventorySlotIndex < 0 || inventorySlotIndex >= inventorySlots.Count)
        {
            Debug.LogWarning("Invalid inventory slot index.");
            return false;
        }

        InventorySlot selectedSlot = inventorySlots[inventorySlotIndex];

        if (selectedSlot.IsEmpty())
        {
            ClearActiveHandItem();
            return true;
        }

        if (activeHandSlot.itemData == selectedSlot.itemData)
        {
            ClearActiveHandItem();
            return true;
        }

        activeHandSlot.itemData = selectedSlot.itemData;
        activeHandSlot.amount = 1;

        Debug.Log($"Active hand item set to: {activeHandSlot.itemData.itemName}");
        TriggerActiveHandChanged(activeHandSlot.itemData);
        return true;
    }

    public void ClearActiveHandItem()
    {
        if (!activeHandSlot.IsEmpty())
        {
            Debug.Log("Active hand item cleared.");
            activeHandSlot.Clear();
            TriggerActiveHandChanged(null);
        }
    }

    public bool AddItem(ItemData itemToAdd, int amount = 1)
    {
        if (itemToAdd == null)
        {
            Debug.LogWarning("Attempted to add null item.");
            return false;
        }

        if (itemToAdd.isStackable)
        {
            foreach (InventorySlot slot in inventorySlots)
            {
                if (slot.itemData == itemToAdd && slot.CanAddAmount(amount))
                {
                    slot.AddAmount(amount);
                    TriggerInventoryChanged();
                    Debug.Log($"Added {amount} {itemToAdd.itemName} to existing stack.");
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
                TriggerInventoryChanged();
                Debug.Log($"Added {amount} {itemToAdd.itemName} to a new slot.");
                return true;
            }
        }

        Debug.LogWarning($"Inventory is full! Could not add {itemToAdd.itemName}.");
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
                if (activeHandSlot.itemData == itemToRemove)
                {
                    ClearActiveHandItem();
                }
            }
            TriggerInventoryChanged();
            Debug.Log($"Removed {amount} {itemToRemove.itemName}.");
            return true;
        }

        Debug.LogWarning($"Could not remove {amount} {itemToRemove.itemName}. Not enough items or item not found.");
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

    public bool UseItem(ItemData itemToUse)
    {
        if (itemToUse == null)
        {
            Debug.LogWarning("Attempted to use null item.");
            return false;
        }

        InventorySlot slotWithItem = inventorySlots.FirstOrDefault(slot => slot.itemData == itemToUse && slot.amount > 0);

        if (slotWithItem != null)
        {
            switch (itemToUse.itemType)
            {
                case ItemType.Consumable:

                    BoatController boat = FindObjectOfType<BoatController>();
                    if (boat != null)
                    {
                        if (boat.currentHP < boat.maxHP)
                        {
                            boat.Heal(itemToUse.healAmount);
                            RemoveItem(itemToUse, 1);
                            Debug.Log($"Used {itemToUse.itemName} to heal.");
                            return true;
                        }
                        else
                        {
                            Debug.Log("Boat is already at full HP. Cannot use " + itemToUse.itemName);
                            return false;
                        }
                    }
                    else
                    {
                        Debug.LogWarning("BoatController not found. Cannot use consumable.");
                        return false;
                    }
            }
        }

        Debug.LogWarning($"Cannot use {itemToUse.itemName}. Not found in inventory.");
        return false;
    }
}