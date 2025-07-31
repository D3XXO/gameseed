using UnityEngine;
using UnityEngine.UI;

public class MarketUI : MonoBehaviour
{
    public static MarketUI Instance;

    [Header("UI References")]
    public GameObject marketPanel;
    public Transform sellSlotsParent;
    public Button confirmSellButton;
    public Text totalValueText;

    private int totalValue;
    private InventorySlot[] sellSlots;

    void Awake()
    {
        Instance = this;
        marketPanel.SetActive(false);
    }

    public void ToggleMarket()
    {
        bool isActive = !marketPanel.activeSelf;
        marketPanel.SetActive(isActive);
        Time.timeScale = isActive ? 0f : 1f;

        if (isActive) SetupSellSlots();
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

    public void AddToSellTotal(int value)
    {
        totalValue += value;
        UpdateTotalUI();
    }

    private void UpdateTotalUI()
    {
        totalValueText.text = $"Total: {totalValue}G";
        confirmSellButton.interactable = totalValue > 0;
    }

    public void ConfirmSell()
    {
        PlayerEconomy.Instance.AddGold(totalValue);

        InventoryUI.Instance.UpdateInventoryUI();
        ToggleMarket();
    }

    public bool IsMarketActive()
    {
        return marketPanel.activeSelf;
    }
}