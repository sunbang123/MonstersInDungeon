using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
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

    [Header("Battle Inventory Settings")]
    [SerializeField] private Transform b_ItemSlotParent;
    [SerializeField] private GameObject b_ItemSlotPrefab;
    [SerializeField] private int b_MaxSlots = 20;

    private List<ItemData> items = new List<ItemData>();
    private List<ItemSlot> slots = new List<ItemSlot>();
    private List<ItemSlot> b_Slots = new List<ItemSlot>();

    // 이 클래스와 스태틱 인스턴스 변수
    protected static InventoryManager m_Instance;

    public static InventoryManager Instance
    {
        get { return m_Instance; }
    }

    protected void Awake()
    {
        Init();
    }
    protected void Init()
    {
        if (m_Instance == null)
        {
            m_Instance = (InventoryManager)this;
        }
    }

    private void Start()
    {
        InitializeSlots();
        InitializeBattleSlots();
        UpdateSlotVisibility();
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

    private void InitializeBattleSlots()
    {
        for (int i = 0; i < b_MaxSlots; i++)
        {
            GameObject slotObj = Instantiate(b_ItemSlotPrefab, b_ItemSlotParent);
            ItemSlot slot = slotObj.AddComponent<ItemSlot>();
            slot.Initialize();
            b_Slots.Add(slot);
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

        // 배틀 인벤토리에도 동기화
        ItemSlot b_EmptySlot = FindEmptyBattleSlot();
        if (b_EmptySlot != null)
        {
            b_EmptySlot.SetItem(item.IData);
        }

        item.DestroyItem();

        // 슬롯 가시성 업데이트
        UpdateSlotVisibility();
    }

    private ItemSlot FindEmptySlot()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].IsEmpty()) return slots[i];
        }
        return null;
    }

    private ItemSlot FindEmptyBattleSlot()
    {
        for (int i = 0; i < b_Slots.Count; i++)
        {
            if (b_Slots[i].IsEmpty()) return b_Slots[i];
        }
        return null;
    }

    private void UpdateSlotVisibility()
    {
        // 일반 인벤토리 슬롯 가시성 업데이트
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].gameObject.SetActive(!slots[i].IsEmpty());
        }

        // 배틀 인벤토리 슬롯 가시성 업데이트
        for (int i = 0; i < b_Slots.Count; i++)
        {
            b_Slots[i].gameObject.SetActive(!b_Slots[i].IsEmpty());
        }
    }

    public void LoadInventory(List<string> itemNames)
    {
        foreach (var name in itemNames)
        {
            ItemData data = ItemDatabase.GetItem(name);
            AddItemToSlot(data);
        }
    }
    public void AddItemToSlot(ItemData data)
    {
        ItemSlot empty = FindEmptySlot();
        if (empty != null)
            empty.SetItem(data);

        ItemSlot bEmpty = FindEmptyBattleSlot();
        if (bEmpty != null)
            bEmpty.SetItem(data);
    }
}