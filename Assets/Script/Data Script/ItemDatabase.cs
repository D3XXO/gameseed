using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "Item Database", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public static ItemDatabase Instance;

    public List<ItemData> allItems;

    private Dictionary<string, ItemData> itemDictionary;

    public void Init()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        if (itemDictionary == null || itemDictionary.Count == 0 || itemDictionary.Count != allItems.Count)
        {
            itemDictionary = allItems.ToDictionary(item => item.id, item => item);
        }
    }

    public ItemData GetItemData(string id)
    {
        if (itemDictionary == null || itemDictionary.Count == 0)
        {
            Init();
        }

        if (itemDictionary.TryGetValue(id, out ItemData item))
        {
            return item;
        }
        return null;
    }
}