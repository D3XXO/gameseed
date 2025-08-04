using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
    private bool isSellMode;
    private int sellAmount;

    void Awake()
    {
        if (selectionBorder != null) selectionBorder.enabled = false;
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
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            InventoryUI.Instance.OnInventorySlotClicked(this, assignedSlot);
        }

        if (isSellMode && eventData.button == PointerEventData.InputButton.Left)
        {
            ToggleItemForSale();
        }
    }

    public void SetAsSellSlot()
    {
        isSellMode = true;
    }

    private void ToggleItemForSale()
    {
        if (assignedSlot.IsEmpty()) return;

        if (sellAmount < assignedSlot.amount)
        {
            sellAmount++;
            sellOverlay.gameObject.SetActive(true);
            sellOverlay.GetComponentInChildren<Text>().text = sellAmount.ToString();
            MarketUI.Instance.AddToSellTotal(assignedSlot.itemData.sellPrice);
        }
        else
        {
            sellAmount = 0;
            sellOverlay.gameObject.SetActive(false);
            MarketUI.Instance.AddToSellTotal(-assignedSlot.itemData.sellPrice * assignedSlot.amount);
        }
    }

    public void ResetSelection()
    {
        if (selectionBorder != null) selectionBorder.enabled = false;
    }
}