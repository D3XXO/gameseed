using UnityEngine;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryPanel;
    public Transform slotsParent;
    public GameObject inventorySlotUIPrefab;

    private List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();

    void Awake()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
        }
    }

    void Start()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onInventoryChangedCallback += UpdateInventoryUI;
            GenerateInventorySlots();
            UpdateInventoryUI();
        }

        Time.timeScale = 1f;
    }

    void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onInventoryChangedCallback -= UpdateInventoryUI;
        }
    }

    void GenerateInventorySlots()
    {
        foreach (Transform child in slotsParent)
        {
            Destroy(child.gameObject);
        }
        uiSlots.Clear();

        for (int i = 0; i < InventoryManager.Instance.inventorySize; i++)
        {
            GameObject slotGO = Instantiate(inventorySlotUIPrefab, slotsParent);
            InventorySlotUI slotUI = slotGO.GetComponent<InventorySlotUI>();
            if (slotUI != null)
            {
                slotUI.Init(InventoryManager.Instance.inventorySlots[i]);
                uiSlots.Add(slotUI);
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
}