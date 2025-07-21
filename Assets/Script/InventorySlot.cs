using UnityEngine;
using System;

[Serializable]
public class InventorySlot
{
    public ItemData itemData;
    public int amount;

    public InventorySlot()
    {
        itemData = null;
        amount = 0;
    }

    public InventorySlot(ItemData data, int count)
    {
        itemData = data;
        amount = count;
    }

    public void AddAmount(int value)
    {
        amount += value;
    }

    public void RemoveAmount(int value)
    {
        amount -= value;
    }

    public void Clear()
    {
        itemData = null;
        amount = 0;
    }

    public bool IsEmpty()
    {
        return itemData == null || amount <= 0;
    }

    public bool CanAddAmount(int value)
    {
        if (itemData == null || !itemData.isStackable) return false;
        return amount + value <= itemData.maxStackSize;
    }
}