using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }

    private string savePath;
    private string[] gameplayScenes = {"Gameplay"};

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
        Debug.Log("Save path: " + savePath);
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
            Debug.Log("Save file deleted: " + savePath);
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
            Debug.LogError("No GameData to save.");
            return;
        }

        try
        {
            string jsonData = JsonUtility.ToJson(dataToSave, true);
            File.WriteAllText(savePath, jsonData);
            Debug.Log("Game saved successfully to: " + savePath);
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
            Debug.Log("Game loaded successfully from: " + savePath);
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
        bool isCurrentSceneGameplay = false;
        foreach (string sceneName in gameplayScenes)
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
            BoatController player = FindObjectOfType<BoatController>();
            if (player != null)
            {
                dataToSave.playerPositionX = player.transform.position.x;
                dataToSave.playerPositionY = player.transform.position.y;
                dataToSave.playerPositionZ = player.transform.position.z;
                dataToSave.lastGameplaySceneName = currentScene;

                dataToSave.playerCurrentHP = player.currentHP;
                dataToSave.playerMaxHP = player.maxHP;
            }
            else
            {
                Debug.LogWarning("PlayerTapMovement not found in current gameplay scene when gathering save data. Player position will be default or from last save.");

                GameData existingData = LoadGame();
                dataToSave.playerPositionX = existingData.playerPositionX;
                dataToSave.playerPositionY = existingData.playerPositionY;
                dataToSave.playerPositionZ = existingData.playerPositionZ;
                dataToSave.lastGameplaySceneName = existingData.lastGameplaySceneName;
            }
        }
        else
        {
            dataToSave = LoadGame();
            Debug.Log("Gathering data from non-gameplay scene, preserving last saved state.");
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
        else
        {
            Debug.LogWarning("InventoryManager not found when gathering save data. Inventory will be empty in save.");
        }

        return dataToSave;
    }

    public void ApplyGameData(GameData loadedData)
    {
        if (loadedData == null)
        {
            Debug.LogError("No loaded GameData to apply.");
            return;
        }

        BoatController player = FindObjectOfType<BoatController>();
        if (player != null)
        {
            player.transform.position = player.SnapToGrid(new Vector3(loadedData.playerPositionX, loadedData.playerPositionY, loadedData.playerPositionZ));
            Debug.Log("Player position loaded: " + player.transform.position);

            player.maxHP = loadedData.playerMaxHP;
            player.Heal(loadedData.playerCurrentHP - player.currentHP);
        }
        else
        {
            Debug.LogWarning("PlayerTapMovement not found when applying loaded data. Player position not applied.");
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
                else
                {
                    Debug.LogWarning($"ItemData with ID '{sSlot.itemID}' not found. Cannot load this item.");
                }
            }
            InventoryManager.Instance.TriggerInventoryChanged();
        }
        else
        {
            Debug.LogWarning("InventoryManager not found when applying loaded data. Inventory not applied.");
        }
    }
}