using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MarketUI : MonoBehaviour
{
    public static MarketUI Instance;

    [Header("UI References")]
    public GameObject marketPanel;
    public Transform sellSlotsParent;
    public Button confirmSellButton;
    public Text totalValueText;

    private int totalValue;
    private Dictionary<InventorySlotUI, int> itemsToSell = new Dictionary<InventorySlotUI, int>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        marketPanel.SetActive(false);
    }

    public void ToggleMarket()
    {
        bool isActive = !marketPanel.activeSelf;
        marketPanel.SetActive(isActive);
        Time.timeScale = isActive ? 0f : 1f;

        if (isActive)
        {
            SetupSellSlots();
        }
        else
        {
            ClearSellSelection();
        }
    }

    private void SetupSellSlots()
    {
        totalValue = 0;
        UpdateTotalUI();

        foreach (Transform child in sellSlotsParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var slot in InventoryManager.Instance.inventorySlots)
        {
            if (!slot.IsEmpty() && slot.itemData.canBeSold)
            {
                var sellSlot = Instantiate(InventoryUI.Instance.inventorySlotUIPrefab, sellSlotsParent);
                var slotUI = sellSlot.GetComponent<InventorySlotUI>();
                slotUI.Init(slot, -1);
                slotUI.SetAsSellSlot();
            }
        }
    }

    // This is the method that was missing
    public void AddToSellTotal(int value)
    {
        totalValue += value;
        UpdateTotalUI();
    }

    public void RegisterItemForSale(InventorySlotUI slotUI, int amount)
    {
        if (itemsToSell.ContainsKey(slotUI))
        {
            // Remove previous value from total
            totalValue -= itemsToSell[slotUI] * slotUI.assignedSlot.itemData.sellPrice;

            if (amount > 0)
            {
                itemsToSell[slotUI] = amount;
                totalValue += amount * slotUI.assignedSlot.itemData.sellPrice;
            }
            else
            {
                itemsToSell.Remove(slotUI);
            }
        }
        else if (amount > 0)
        {
            itemsToSell.Add(slotUI, amount);
            totalValue += amount * slotUI.assignedSlot.itemData.sellPrice;
        }

        UpdateTotalUI();
    }

    private void UpdateTotalUI()
    {
        totalValueText.text = $"Total: {totalValue}G";
        confirmSellButton.interactable = totalValue > 0;
    }

    public void ConfirmSell()
    {
        foreach (var kvp in itemsToSell)
        {
            InventorySlotUI slotUI = kvp.Key;
            int amount = kvp.Value;

            if (slotUI.assignedSlot.itemData != null && amount > 0)
            {
                InventoryManager.Instance.RemoveItem(slotUI.assignedSlot.itemData, amount);
            }
        }

        PlayerEconomy.Instance.AddGold(totalValue);
        ClearSellSelection();
        InventoryUI.Instance.UpdateInventoryUI();
        ToggleMarket();
    }

    private void ClearSellSelection()
    {
        itemsToSell.Clear();
        totalValue = 0;
        UpdateTotalUI();
    }

    public bool IsMarketActive()
    {
        return marketPanel.activeSelf;
    }
}