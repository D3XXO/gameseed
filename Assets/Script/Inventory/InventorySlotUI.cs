using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    public Image itemIcon;
    public Text itemAmountText;
    public Image selectionBorder;

    [HideInInspector]
    public InventorySlot assignedSlot;
    private int slotIndex;

    [Header("Selling")]
    public Image sellOverlay;
    public Text sellAmountText;
    private bool isSellMode;
    private int sellAmount;

    void Awake()
    {
        if (selectionBorder != null) selectionBorder.enabled = false;
        if (sellOverlay != null) sellOverlay.gameObject.SetActive(false);
    }

    public void Init(InventorySlot slot, int index)
    {
        assignedSlot = slot;
        slotIndex = index;

        if (selectionBorder != null) selectionBorder.enabled = false;
        UpdateSlotUI();
    }

    public void UpdateSlotUI()
    {
        if (assignedSlot == null || assignedSlot.IsEmpty())
        {
            itemIcon.gameObject.SetActive(false);
            itemAmountText.text = "";
            ResetSelection();
            ResetSellSelection();
        }
        else
        {
            itemIcon.gameObject.SetActive(true);
            itemIcon.sprite = assignedSlot.itemData.icon;
            itemAmountText.text = assignedSlot.amount > 1 ? assignedSlot.amount.ToString() : "";
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isSellMode)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                InventoryUI.Instance.OnInventorySlotClicked(this, assignedSlot);
            }
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            ToggleItemForSale();

            // Update market UI
            if (sellAmount > 0)
            {
                MarketUI.Instance.AddToSellTotal(assignedSlot.itemData.sellPrice);
                MarketUI.Instance.RegisterItemForSale(this, sellAmount);
            }
            else
            {
                MarketUI.Instance.AddToSellTotal(-assignedSlot.itemData.sellPrice * assignedSlot.amount);
                MarketUI.Instance.RegisterItemForSale(this, 0);
            }
        }
    }

    public void SetSellMode(bool active)
    {
        isSellMode = active;
        if (!active)
        {
            ResetSellSelection();
        }
    }

    private void ToggleItemForSale()
    {
        if (assignedSlot.IsEmpty()) return;

        if (sellAmount < assignedSlot.amount)
        {
            sellAmount++;
        }
        else
        {
            sellAmount = 0;
        }

        UpdateSellUI();
    }

    private void UpdateSellUI()
    {
        if (sellAmount > 0)
        {
            sellOverlay.gameObject.SetActive(true);
            if (sellAmountText != null)
            {
                sellAmountText.text = sellAmount.ToString();
            }
        }
        else
        {
            sellOverlay.gameObject.SetActive(false);
        }
    }

    public void ResetSellSelection()
    {
        sellAmount = 0;
        if (sellOverlay != null)
        {
            sellOverlay.gameObject.SetActive(false);
        }
    }

    public int GetSellAmount()
    {
        return sellAmount;
    }

    public void ResetSelection()
    {
        if (selectionBorder != null) selectionBorder.enabled = false;
    }
}