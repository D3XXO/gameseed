using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class GameData
{
    public string lastGameplaySceneName;
    public float playerPositionX;
    public float playerPositionY;
    public float playerPositionZ;
    public int playerCurrentHP;
    public int playerMaxHP;
    public double currentWorldTimeTotalMinutes;
    public int currentWorldDay;
    public List<SerializableInventorySlot> inventorySlotsData;

    public bool useOverrideSpawnPosition;
    public float overrideSpawnX;
    public float overrideSpawnY;
    public float overrideSpawnZ;

    public GameData()
    {
        playerPositionX = 0;
        playerPositionY = 0;
        playerPositionZ = 0;
        inventorySlotsData = new List<SerializableInventorySlot>();
        lastGameplaySceneName = "Gameplay";
        playerCurrentHP = 3;
        playerMaxHP = 3;

        currentWorldTimeTotalMinutes = new TimeSpan(21, 0, 0).TotalMinutes;
        currentWorldDay = 1;
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