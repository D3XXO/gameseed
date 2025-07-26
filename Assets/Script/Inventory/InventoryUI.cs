using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [Header("UI References")]
    public GameObject inventoryPanel;
    public Transform slotsParent;
    public GameObject inventorySlotUIPrefab;

    [Header("Held Item UI")]
    public Image heldItemIconUI;
    public Text heldItemNameUI;
    public Transform playerPivotTransform;

    private GameObject currentHeldItemVisual;

    private List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();

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

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onInventoryChangedCallback -= UpdateInventoryUI;
            InventoryManager.Instance.onActiveHandChangedCallback -= UpdateHeldItemVisual;
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string[] gameplayScenes = {"Gameplay"};

        bool isGameplayScene = false;
        foreach (string sceneName in gameplayScenes)
        {
            if (scene.name == sceneName)
            {
                isGameplayScene = true;
                break;
            }
        }

        if (isGameplayScene)
        {
            if (inventoryPanel == null)
            {
                Canvas mainCanvas = FindObjectOfType<Canvas>();
                if (mainCanvas != null)
                {
                    Transform foundInventoryPanelTransform = mainCanvas.transform.Find("Inventory Panel");
                    if (foundInventoryPanelTransform != null)
                    {
                        inventoryPanel = foundInventoryPanelTransform.gameObject;
                        slotsParent = foundInventoryPanelTransform.Find("Inventory Grid");
                        Debug.Log("Inventory Panel and Slots Parent re-found in " + scene.name);
                    }
                    else
                    {
                        Debug.LogWarning("Inventory Panel not found in " + scene.name + ". Make sure its named 'Inventory Panel' and is active.");
                    }
                }
                else
                {
                    Debug.LogWarning("Main Canvas not found in " + scene.name + ".");
                }
            }

            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(true);
            }

            Canvas currentCanvas = FindObjectOfType<Canvas>();
            if (currentCanvas != null)
            {
                Transform foundIconTransform = currentCanvas.transform.Find("Held Item Icon");
                if (foundIconTransform != null) heldItemIconUI = foundIconTransform.GetComponent<Image>();
                if (heldItemIconUI == null) Debug.LogWarning("Held Item Icon UI not found or has no Image component in " + scene.name);

                Transform foundNameTransform = currentCanvas.transform.Find("Held Item Name");
                if (foundNameTransform != null) heldItemNameUI = foundNameTransform.GetComponent<Text>();
                if (heldItemNameUI == null) Debug.LogWarning("Held Item Name UI not found or has no Text component in " + scene.name);

            }
            else
            {
                Debug.LogWarning("Cannot find Main Canvas to re-establish Held Item UI references.");
            }

            BoatController playerMovement = FindObjectOfType<BoatController>();
            if (playerMovement != null)
            {
                playerPivotTransform = playerMovement.GetCollectPivot();
                if (playerPivotTransform == null)
                {
                    Debug.LogError("playerHandTransform is null even after getting from PlayerTapMovement.GetCollectPivot(). Check PlayerTapMovement script.");
                }
                else
                {
                    Debug.Log("playerHandTransform re-found in " + scene.name + " at: " + playerPivotTransform.position);
                }
            }
            else
            {
                Debug.LogWarning("PlayerTapMovement not found in " + scene.name + ". Cannot attach held item visual.");
                playerPivotTransform = null;
            }

            if (inventoryPanel != null && slotsParent != null && heldItemIconUI != null && heldItemNameUI != null && playerPivotTransform != null)
            {
                if (InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.onInventoryChangedCallback -= UpdateInventoryUI;
                    InventoryManager.Instance.onInventoryChangedCallback += UpdateInventoryUI;

                    InventoryManager.Instance.onActiveHandChangedCallback -= UpdateHeldItemVisual;
                    InventoryManager.Instance.onActiveHandChangedCallback += UpdateHeldItemVisual;

                    GenerateInventorySlots();
                    UpdateInventoryUI();
                    UpdateHeldItemVisual(InventoryManager.Instance.activeHandSlot.itemData);
                }
            }
            else
            {
                Debug.LogError("Failed to establish ALL necessary UI references in " + scene.name + ". Inventory UI might not function correctly. (Panel: " + (inventoryPanel != null) + ", Slots: " + (slotsParent != null) + ", IconUI: " + (heldItemIconUI != null) + ", NameUI: " + (heldItemNameUI != null) + ", Pivot: " + (playerPivotTransform != null) + ")");
            }
        }
        else if (scene.name == "Main Menu")
        {
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(false);
            }
            DestroyHeldItemVisual();
            playerPivotTransform = null;
            heldItemIconUI = null;
            heldItemNameUI = null;
        }
        else
        {
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(false);
            }
            DestroyHeldItemVisual();
            playerPivotTransform = null;
            heldItemIconUI = null;
            heldItemNameUI = null;
        }
    }

    void Start()
    {
        Time.timeScale = 1f;
    }

    void GenerateInventorySlots()
    {
        if (slotsParent == null)
        {
            Debug.LogError("Slots Parent is null! Cannot generate inventory slots.");
            return;
        }

        foreach (Transform child in slotsParent)
        {
            Destroy(child.gameObject);
        }
        uiSlots.Clear();

        if (InventoryManager.Instance != null)
        {
            for (int i = 0; i < InventoryManager.Instance.inventorySize; i++)
            {
                GameObject slotGO = Instantiate(inventorySlotUIPrefab, slotsParent);
                InventorySlotUI slotUI = slotGO.GetComponent<InventorySlotUI>();
                if (slotUI != null)
                {
                    slotUI.Init(InventoryManager.Instance.inventorySlots[i], i);
                    uiSlots.Add(slotUI);
                }
            }
        }
    }

    void UpdateInventoryUI()
    {
        for (int i = 0; i < uiSlots.Count; i++)
        {
            if (uiSlots[i] != null)
            {
                uiSlots[i].UpdateSlotUI();
            }
        }
    }

    void UpdateHeldItemVisual(ItemData newItemData)
    {
        DestroyHeldItemVisual();

        if (heldItemIconUI != null)
        {
            if (newItemData != null && newItemData.icon != null)
            {
                heldItemIconUI.gameObject.SetActive(true);
                heldItemIconUI.sprite = newItemData.icon;
            }
            else
            {
                heldItemIconUI.gameObject.SetActive(false);
            }
        }

        if (heldItemNameUI != null)
        {
            if (newItemData != null)
            {
                heldItemNameUI.text = newItemData.itemName;
            }
            else
            {
                heldItemNameUI.text = "";
            }
        }
    }

    private void DestroyHeldItemVisual()
    {
        currentHeldItemVisual = null;

        if (heldItemIconUI != null) heldItemIconUI.gameObject.SetActive(false);
        if (heldItemNameUI != null) heldItemNameUI.text = "";
    }
}