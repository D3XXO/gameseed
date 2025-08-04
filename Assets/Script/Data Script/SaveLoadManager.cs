using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using WorldTime;
using System;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }

    private string savePath;

    [SerializeField] private string[] _gameplayScenes = { "Gameplay", "Harbour" };
    public string[] GameplayScenes => _gameplayScenes;

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

        savePath = Path.Combine(Application.persistentDataPath, "gamedata.json");
    }

    public bool DoesSaveFileExist()
    {
        return File.Exists(savePath);
    }

    public void DeleteSaveFile()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
        }
        else
        {
            Debug.LogWarning("No save file to delete at: " + savePath);
        }
    }

    public void SaveGame(GameData dataToSave)
    {
        if (dataToSave == null)
        {
            return;
        }

        try
        {
            string jsonData = JsonUtility.ToJson(dataToSave, true);
            File.WriteAllText(savePath, jsonData);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
        }
    }

    public GameData LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("No save file found at: " + savePath);
            return new GameData();
        }

        try
        {
            string jsonData = File.ReadAllText(savePath);
            GameData loadedData = JsonUtility.FromJson<GameData>(jsonData);
            return loadedData;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load game: {e.Message}. Returning new game data.");
            return new GameData();
        }
    }

    public GameData GatherGameData()
    {
        GameData dataToSave;



        string currentScene = SceneManager.GetActiveScene().name;
        bool isCurrentSceneGameplay = Array.Exists(GameplayScenes, scene => scene == currentScene);
        foreach (string sceneName in GameplayScenes)
        {
            if (currentScene == sceneName)
            {
                isCurrentSceneGameplay = true;
                break;
            }

        }

        if (isCurrentSceneGameplay)
        {
            dataToSave = new GameData();
            BoatController boat = FindObjectOfType<BoatController>();
            if (boat != null)
            {
                dataToSave.playerPositionX = boat.transform.position.x;
                dataToSave.playerPositionY = boat.transform.position.y;
                dataToSave.playerPositionZ = boat.transform.position.z;
                dataToSave.lastGameplaySceneName = currentScene;

                dataToSave.playerCurrentHP = boat.currentHP;
                dataToSave.playerMaxHP = boat.maxHP;
                dataToSave.currentMoveSpeed = boat.moveSpeed;
                if (UpgradeManager.Instance != null)
                {
                    dataToSave.speedUpgradeLevel = UpgradeManager.Instance.currentSpeedUpgradeLevel;
                    dataToSave.healthUpgradeLevel = UpgradeManager.Instance.currentHealthUpgradeLevel;
                }

                if (currentScene != "Harbour")
                {
                    dataToSave.lastGameplaySceneName = currentScene;
                }
            }
            else
            {
                GameData existingData = LoadGame();
                dataToSave.playerPositionX = existingData.playerPositionX;
                dataToSave.playerPositionY = existingData.playerPositionY;
                dataToSave.playerPositionZ = existingData.playerPositionZ;
                dataToSave.lastGameplaySceneName = existingData.lastGameplaySceneName;
                dataToSave.playerCurrentHP = existingData.playerCurrentHP;
                dataToSave.playerMaxHP = existingData.playerMaxHP;
            }

            if (WorldTime.WorldTime.Instance != null)
            {
                dataToSave.currentWorldTimeTotalMinutes = WorldTime.WorldTime.Instance.CurrentTime.TotalMinutes;
                dataToSave.currentWorldDay = WorldTime.WorldTime.Instance.CurrentDay;
            }
        }
        else
        {
            dataToSave = LoadGame();
        }

        if (InventoryManager.Instance != null)
        {
            dataToSave.inventorySlotsData = new List<SerializableInventorySlot>();
            foreach (InventorySlot slot in InventoryManager.Instance.inventorySlots)
            {
                if (!slot.IsEmpty())
                {
                    dataToSave.inventorySlotsData.Add(new SerializableInventorySlot(slot.itemData.id, slot.amount));
                }
            }
        }
        if (PlayerEconomy.Instance != null)
        {
            dataToSave.playerGold = PlayerEconomy.Instance.currentGold;
        }

        return dataToSave;
    }


    public void ApplyGameData(GameData loadedData)
    {
        if (loadedData == null) return;

        BoatController boat = FindObjectOfType<BoatController>();
        if (boat != null)
        {

            if (loadedData.useOverrideSpawnPosition)
            {
                Vector3 spawnPos = new Vector3(loadedData.overrideSpawnX, loadedData.overrideSpawnY, loadedData.overrideSpawnZ);
                boat.transform.position = boat.SnapToGrid(spawnPos);

                loadedData.useOverrideSpawnPosition = false;
                SaveGame(loadedData);
            }
            else
            {
                boat.transform.position = boat.SnapToGrid(new Vector3(loadedData.playerPositionX, loadedData.playerPositionY, loadedData.playerPositionZ));
            }

            boat.SetHealth(loadedData.playerCurrentHP, loadedData.playerMaxHP);
            boat.moveSpeed = loadedData.currentMoveSpeed;
            boat.originalMoveSpeed = loadedData.currentMoveSpeed;
        }

        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.LoadFromGameData(loadedData);
        }

        if (WorldTime.WorldTime.Instance != null)
        {
            TimeSpan loadedTime = TimeSpan.FromMinutes(loadedData.currentWorldTimeTotalMinutes);
            int loadedDay = loadedData.currentWorldDay;
            WorldTime.WorldTime.Instance.SetTimeAndDay(loadedTime, loadedDay);
        }

        if (InventoryManager.Instance != null)
        {
            foreach (InventorySlot slot in InventoryManager.Instance.inventorySlots)
            {
                slot.Clear();
            }

            foreach (SerializableInventorySlot sSlot in loadedData.inventorySlotsData)
            {
                ItemData itemData = ItemDatabase.Instance.GetItemData(sSlot.itemID);
                if (itemData != null)
                {
                    InventoryManager.Instance.AddItem(itemData, sSlot.amount);
                }
            }
            InventoryManager.Instance.TriggerInventoryChanged();
        }

        if (PlayerEconomy.Instance != null)
        {
            PlayerEconomy.Instance.SetGold(loadedData.playerGold);
        }

    }
}