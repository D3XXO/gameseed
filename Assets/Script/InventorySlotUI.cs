using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    public Image itemIcon;
    public Text itemAmountText;

    private InventorySlot assignedSlot;

    public void Init(InventorySlot slot)
    {
        assignedSlot = slot;
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
}