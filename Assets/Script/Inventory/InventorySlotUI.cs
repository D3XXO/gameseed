using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    public Image itemIcon;
    public Text itemAmountText;

    private InventorySlot assignedSlot;
    private int slotIndex;

    public void Init(InventorySlot slot, int index)
    {
        assignedSlot = slot;
        slotIndex = index;
        UpdateSlotUI();
    }

    public void UpdateSlotUI()
    {
        if (assignedSlot == null || assignedSlot.IsEmpty())
        {
            itemIcon.gameObject.SetActive(false);
            itemAmountText.text = "";
        }
        else
        {
            itemIcon.gameObject.SetActive(true);
            itemIcon.sprite = assignedSlot.itemData.icon;
            itemAmountText.text = assignedSlot.amount.ToString();

            if (!assignedSlot.itemData.isStackable || assignedSlot.amount <= 1)
            {
                itemAmountText.text = "";
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (assignedSlot == null || assignedSlot.IsEmpty())
        {
            InventoryManager.Instance.ClearActiveHandItem();
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            InventoryManager.Instance.SetActiveHandItem(slotIndex);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            InventoryManager.Instance.UseItem(assignedSlot.itemData);
        }
    }
}