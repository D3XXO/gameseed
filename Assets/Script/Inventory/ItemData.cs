using UnityEngine;

[CreateAssetMenu(fileName = "New Item Data", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    public string id;
    public string itemName;
    public Sprite icon;
    [TextArea(3, 5)]
    public string description;
    public bool isStackable = true;
    public int maxStackSize;

    public ItemType itemType;

    public int healAmount;

    [Header("Selling")]
    public int sellPrice;
    public bool canBeSold = true;
}

public enum ItemType
{
    Consumable,
    Fish
}