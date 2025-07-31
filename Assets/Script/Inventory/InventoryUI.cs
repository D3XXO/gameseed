using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [Header("UI References")]
    public GameObject inventoryPanel;
    public Transform slotsParent;
    public GameObject inventorySlotUIPrefab;

    public GameObject _itemInfoPanel;
    public Text _descriptionText;
    public Button _healButton;
    public Button _deleteButton;

    public GameObject _deleteConfirmationPanel;
    public Slider _deleteAmountSlider;
    public Text _amountText;
    public Button _confirmDeleteButton;
    public Button _cancelDeleteButton;

    private List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();
    private InventorySlotUI currentlySelectedSlotUI;

    private bool isHarbourScene = false;
    private bool referencesInitialized = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        Time.timeScale = 1f;
        InitializeReferences();
    }

    void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onInventoryChangedCallback -= UpdateInventoryUI;
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (_healButton != null) _healButton.onClick.RemoveAllListeners();
        if (_deleteButton != null) _deleteButton.onClick.RemoveAllListeners();
        if (_confirmDeleteButton != null) _confirmDeleteButton.onClick.RemoveAllListeners();
        if (_cancelDeleteButton != null) _cancelDeleteButton.onClick.RemoveAllListeners();
        if (_deleteAmountSlider != null) _deleteAmountSlider.onValueChanged.RemoveAllListeners();
    }

    private void InitializeReferences()
    {
        if (referencesInitialized) return;

        // Find main canvas
        Canvas mainCanvas = FindObjectOfType<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogWarning("Main Canvas not found in the scene.");
            return;
        }

        // Find inventory panel
        if (inventoryPanel == null)
        {
            inventoryPanel = mainCanvas.transform.Find("Inventory Panel")?.gameObject;
            if (inventoryPanel == null)
            {
                Debug.LogWarning("Inventory Panel GameObject not found in the scene.");
                return;
            }
        }

        // Find slots parent if not set
        if (slotsParent == null)
        {
            slotsParent = inventoryPanel.transform.Find("Inventory Grid");
            if (slotsParent == null)
            {
                Debug.LogWarning("Inventory Grid not found in Inventory Panel.");
                return;
            }
        }

        // Find item info panel
        if (_itemInfoPanel == null)
        {
            _itemInfoPanel = inventoryPanel.transform.Find("ItemInfoPanel")?.gameObject;
            if (_itemInfoPanel == null)
            {
                Debug.LogWarning("ItemInfoPanel not found in Inventory Panel.");
                return;
            }

            _descriptionText = _itemInfoPanel.GetComponentInChildren<Text>(true);
            _healButton = _itemInfoPanel.transform.Find("HealButtonUI")?.GetComponent<Button>();
            _deleteButton = _itemInfoPanel.transform.Find("DeleteButtonUI")?.GetComponent<Button>();

            if (_descriptionText == null) Debug.LogWarning("DescriptionText not found in ItemInfoPanel.");
            if (_healButton == null) Debug.LogWarning("HealButtonUI not found in ItemInfoPanel.");
            if (_deleteButton == null) Debug.LogWarning("DeleteButtonUI not found in ItemInfoPanel.");
        }

        // Find delete confirmation panel
        if (_deleteConfirmationPanel == null)
        {
            _deleteConfirmationPanel = mainCanvas.transform.Find("DeleteConfirmationPanel")?.gameObject;
            if (_deleteConfirmationPanel == null)
            {
                Debug.LogWarning("DeleteConfirmationPanel not found in Canvas.");
                return;
            }

            _deleteAmountSlider = _deleteConfirmationPanel.GetComponentInChildren<Slider>(true);
            _amountText = _deleteConfirmationPanel.transform.Find("AmountText")?.GetComponent<Text>();
            _confirmDeleteButton = _deleteConfirmationPanel.transform.Find("ConfirmDeleteButton")?.GetComponent<Button>();
            _cancelDeleteButton = _deleteConfirmationPanel.transform.Find("CancelDeleteButton")?.GetComponent<Button>();

            if (_deleteAmountSlider == null) Debug.LogWarning("DeleteAmountSlider not found in DeleteConfirmationPanel.");
            if (_amountText == null) Debug.LogWarning("AmountText not found in DeleteConfirmationPanel.");
            if (_confirmDeleteButton == null) Debug.LogWarning("ConfirmDeleteButton not found in DeleteConfirmationPanel.");
            if (_cancelDeleteButton == null) Debug.LogWarning("CancelDeleteButton not found in DeleteConfirmationPanel.");
        }

        // Setup button listeners
        if (_healButton != null)
        {
            _healButton.onClick.RemoveAllListeners();
            _healButton.onClick.AddListener(OnHealButtonClicked);
        }

        if (_deleteButton != null)
        {
            _deleteButton.onClick.RemoveAllListeners();
            _deleteButton.onClick.AddListener(OnDeleteButtonClicked);
        }

        if (_confirmDeleteButton != null)
        {
            _confirmDeleteButton.onClick.RemoveAllListeners();
            _confirmDeleteButton.onClick.AddListener(OnConfirmDeleteButtonClicked);
        }

        if (_cancelDeleteButton != null)
        {
            _cancelDeleteButton.onClick.RemoveAllListeners();
            _cancelDeleteButton.onClick.AddListener(OnCancelDeleteButtonClicked);
        }

        if (_deleteAmountSlider != null)
        {
            _deleteAmountSlider.onValueChanged.RemoveAllListeners();
            _deleteAmountSlider.onValueChanged.AddListener(OnDeleteSliderChanged);
        }

        // Initialize UI state
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (_itemInfoPanel != null) _itemInfoPanel.SetActive(false);
        if (_deleteConfirmationPanel != null) _deleteConfirmationPanel.SetActive(false);

        referencesInitialized = true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isHarbourScene = scene.name == "Harbour";
        
        // Re-initialize references when scene changes
        referencesInitialized = false;
        InitializeReferences();

        if (isHarbourScene)
        {
            if (_deleteButton != null)
            {
                _deleteButton.GetComponentInChildren<Text>().text = "SELL";
                _deleteButton.image.color = Color.yellow;
            }

            if (_confirmDeleteButton != null)
            {
                _confirmDeleteButton.GetComponentInChildren<Text>().text = "CONFIRM SELL";
            }
        }
        else
        {
            if (_deleteButton != null)
            {
                _deleteButton.GetComponentInChildren<Text>().text = "DELETE";
                _deleteButton.image.color = Color.white;
            }

            if (_confirmDeleteButton != null)
            {
                _confirmDeleteButton.GetComponentInChildren<Text>().text = "CONFIRM DELETE";
            }
        }

        // Connect to inventory manager if in gameplay scene
        if (scene.name == "Gameplay" || scene.name == "Harbour")
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.onInventoryChangedCallback -= UpdateInventoryUI;
                InventoryManager.Instance.onInventoryChangedCallback += UpdateInventoryUI;

                GenerateInventorySlots();
                UpdateInventoryUI();
            }
        }
    }

    void GenerateInventorySlots()
    {
        if (slotsParent == null) return;

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

    public void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            bool isActive = !inventoryPanel.activeSelf;
            inventoryPanel.SetActive(isActive);

            Time.timeScale = isActive ? 0f : 1f;

            if (!isActive)
            {
                if (_itemInfoPanel != null) _itemInfoPanel.SetActive(false);
                if (_deleteConfirmationPanel != null) _deleteConfirmationPanel.SetActive(false);
                DeselectAllSlotsExcept(null);
                currentlySelectedSlotUI = null;
            }
        }
    }

    public void UpdateInventoryUI()
    {
        if (!referencesInitialized) return;

        for (int i = 0; i < uiSlots.Count; i++)
        {
            if (uiSlots[i] != null)
            {
                uiSlots[i].UpdateSlotUI();
            }
        }

        if (currentlySelectedSlotUI == null || currentlySelectedSlotUI.assignedSlot == null || currentlySelectedSlotUI.assignedSlot.IsEmpty() || currentlySelectedSlotUI.assignedSlot.itemData == null || !currentlySelectedSlotUI.selectionBorder.enabled)
        {
            if (_itemInfoPanel != null) _itemInfoPanel.SetActive(false);
            if (_deleteConfirmationPanel != null) _deleteConfirmationPanel.SetActive(false);
        }
        else
        {
            if (_descriptionText != null && currentlySelectedSlotUI.assignedSlot.itemData != null)
            {
                _descriptionText.text = currentlySelectedSlotUI.assignedSlot.itemData.description;
            }

            bool isConsumable = currentlySelectedSlotUI.assignedSlot.itemData.itemType == ItemType.Consumable;
            if (_healButton != null) _healButton.gameObject.SetActive(isConsumable);

            if (_deleteButton != null) _deleteButton.gameObject.SetActive(true);

            if (_itemInfoPanel != null) _itemInfoPanel.SetActive(true);
        }
    }

    public void DeselectAllSlotsExcept(InventorySlotUI exceptSlot = null)
    {
        foreach (InventorySlotUI slot in uiSlots)
        {
            if (slot != exceptSlot)
            {
                slot.ResetSelection();
            }
        }
        if (exceptSlot == null || (exceptSlot != null && !exceptSlot.selectionBorder.enabled))
        {
            if (_itemInfoPanel != null) _itemInfoPanel.SetActive(false);
            if (_deleteConfirmationPanel != null) _deleteConfirmationPanel.SetActive(false);
            currentlySelectedSlotUI = null;
        }
    }

    public void OnInventorySlotClicked(InventorySlotUI clickedSlotUI, InventorySlot assignedSlot)
    {
        bool isAlreadySelected = currentlySelectedSlotUI == clickedSlotUI && clickedSlotUI.selectionBorder.enabled;
        bool isClickedSlotEmpty = (assignedSlot == null || assignedSlot.IsEmpty());

        if (isAlreadySelected || isClickedSlotEmpty)
        {
            DeselectAllSlotsExcept(null);
            currentlySelectedSlotUI = null;
        }
        else
        {
            DeselectAllSlotsExcept(clickedSlotUI);
            clickedSlotUI.selectionBorder.enabled = true;
            currentlySelectedSlotUI = clickedSlotUI;

            if (_itemInfoPanel != null)
            {
                if (_descriptionText != null && assignedSlot.itemData != null)
                {
                    _descriptionText.text = assignedSlot.itemData.description;
                }

                bool isConsumable = assignedSlot.itemData != null && assignedSlot.itemData.itemType == ItemType.Consumable;
                if (_healButton != null) _healButton.gameObject.SetActive(isConsumable);
                if (_deleteButton != null) _deleteButton.gameObject.SetActive(true);

                _itemInfoPanel.SetActive(true);
            }
            if (_deleteConfirmationPanel != null) _deleteConfirmationPanel.SetActive(false);
        }
    }

    private void OnDeleteButtonClicked()
    {
        if (currentlySelectedSlotUI == null || currentlySelectedSlotUI.assignedSlot == null ||
            currentlySelectedSlotUI.assignedSlot.IsEmpty()) return;

        if (_deleteConfirmationPanel != null)
        {
            _deleteConfirmationPanel.SetActive(true);
            _itemInfoPanel?.SetActive(false);

            int maxAmount = currentlySelectedSlotUI.assignedSlot.amount;
            if (_deleteAmountSlider != null)
            {
                _deleteAmountSlider.minValue = 1;
                _deleteAmountSlider.maxValue = maxAmount;
                _deleteAmountSlider.value = 1;
            }

            if (isHarbourScene && currentlySelectedSlotUI.assignedSlot.itemData.canBeSold)
            {
                UpdateSellValueDisplay((int)_deleteAmountSlider.value);
            }
            else
            {
                if (_amountText != null)
                {
                    _amountText.text = $"{(int)_deleteAmountSlider.value} / {maxAmount}";
                }
            }
        }
    }

    private void UpdateSellValueDisplay(int amount)
    {
        if (!isHarbourScene || currentlySelectedSlotUI == null ||
            currentlySelectedSlotUI.assignedSlot == null) return;

        ItemData item = currentlySelectedSlotUI.assignedSlot.itemData;
        int totalValue = item.sellPrice * amount;

        if (_amountText != null)
        {
            _amountText.text = $"Sell: {amount} x {item.sellPrice}G = {totalValue}G";
        }
    }

    private void OnDeleteSliderChanged(float value)
    {
        if (isHarbourScene && currentlySelectedSlotUI != null &&
            currentlySelectedSlotUI.assignedSlot != null &&
            currentlySelectedSlotUI.assignedSlot.itemData.canBeSold)
        {
            UpdateSellValueDisplay((int)value);
        }
        else
        {
            if (_amountText != null && currentlySelectedSlotUI != null && currentlySelectedSlotUI.assignedSlot != null)
            {
                _amountText.text = $"{(int)value} / {currentlySelectedSlotUI.assignedSlot.amount}";
            }
        }
    }

    private void OnConfirmDeleteButtonClicked()
    {
        if (currentlySelectedSlotUI == null || currentlySelectedSlotUI.assignedSlot == null ||
            currentlySelectedSlotUI.assignedSlot.IsEmpty() || _deleteAmountSlider == null) return;

        int amount = (int)_deleteAmountSlider.value;
        ItemData item = currentlySelectedSlotUI.assignedSlot.itemData;

        if (isHarbourScene && item.canBeSold)
        {
            int totalValue = item.sellPrice * amount;
            if (InventoryManager.Instance.RemoveItem(item, amount))
            {
                PlayerEconomy.Instance.AddGold(totalValue);
                Debug.Log($"Sold {amount}x {item.itemName} for {totalValue}G");
            }
        }
        else
        {
            InventoryManager.Instance.RemoveItem(item, amount);
        }

        _deleteConfirmationPanel?.SetActive(false);
        currentlySelectedSlotUI?.ResetSelection();
        currentlySelectedSlotUI = null;
    }

    private void OnCancelDeleteButtonClicked()
    {
        if (_deleteConfirmationPanel != null)
        {
            _deleteConfirmationPanel.SetActive(false);
        }
    }

    private void OnHealButtonClicked()
    {
        if (currentlySelectedSlotUI != null && currentlySelectedSlotUI.assignedSlot != null && !currentlySelectedSlotUI.assignedSlot.IsEmpty() && currentlySelectedSlotUI.assignedSlot.itemData.itemType == ItemType.Consumable)
        {
            bool used = InventoryManager.Instance.UseItem(currentlySelectedSlotUI.assignedSlot.itemData);
            if (used)
            {
                currentlySelectedSlotUI.ResetSelection();
                if (_itemInfoPanel != null) _itemInfoPanel.SetActive(false);
                if (_deleteConfirmationPanel != null) _deleteConfirmationPanel.SetActive(false);
                currentlySelectedSlotUI = null;
            }
        }
    }
}