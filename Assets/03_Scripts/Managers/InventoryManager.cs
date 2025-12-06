using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : SingletonBehaviour<InventoryManager>
{
    [Header("Inventory Settings")]
    [SerializeField] private Transform itemSlotParent;
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private int maxSlots = 20;

    [Header("Inventory Data Settings")]
    [SerializeField] private TextMeshProUGUI ItemName;
    [SerializeField] private TextMeshProUGUI ItemDescription;
    [SerializeField] private TextMeshProUGUI StatName;
    [SerializeField] private TextMeshProUGUI StatValue;

    private List<ItemData> items = new List<ItemData>();
    private List<ItemSlot> slots = new List<ItemSlot>();

    private void Start()
    {
        InitializeSlots();
    }

    private void InitializeSlots()
    {
        for (int i = 0; i < maxSlots; i++)
        {
            GameObject slotObj = Instantiate(itemSlotPrefab, itemSlotParent);
            ItemSlot slot = slotObj.AddComponent<ItemSlot>();
            slot.Initialize();
            slots.Add(slot);
        }
    }

    public void PickUpItem()
    {
        Item[] foundItems = FindObjectsOfType<Item>();
        for (int i = 0; i < foundItems.Length; i++)
        {
            TryPickUpItem(foundItems[i]);
        }
    }

    private void TryPickUpItem(Item item)
    {
        if (item == null || !item.IsPlayerNear() || item.IData == null) return;

        ItemSlot emptySlot = FindEmptySlot();
        if (emptySlot == null)
        {
            Debug.Log("인벤토리가 가득 찼습니다!");
            return;
        }

        items.Add(item.IData);
        emptySlot.SetItem(item.IData);
        item.DestroyItem();
    }

    private ItemSlot FindEmptySlot()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].IsEmpty()) return slots[i];
        }
        return null;
    }
}