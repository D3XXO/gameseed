using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    public float playerPositionX;
    public float playerPositionY;
    public float playerPositionZ;

    public List<SerializableInventorySlot> inventorySlotsData;

    public string lastGameplaySceneName;

    public int playerCurrentHP;
    public int playerMaxHP;

    public GameData()
    {
        playerPositionX = 0;
        playerPositionY = 0;
        playerPositionZ = 0;
        inventorySlotsData = new List<SerializableInventorySlot>();
        lastGameplaySceneName = "Gameplay";
        playerCurrentHP = 3;
        playerMaxHP = 3;
    }
}

[System.Serializable]
public class SerializableInventorySlot
{
    public string itemID;
    public int amount;

    public SerializableInventorySlot(string id, int amt)
    {
        itemID = id;
        amount = amt;
    }
}