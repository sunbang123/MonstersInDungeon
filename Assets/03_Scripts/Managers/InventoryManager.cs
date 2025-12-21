using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

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

    // 싱글톤 패턴 인스턴스 변수
    protected static InventoryManager m_Instance;

    public static InventoryManager Instance
    {
        get { return m_Instance; }
    }

    // 인벤토리 UI 업데이트 이벤트
    public event Action<int, bool> OnSlotVisibilityChanged; // slotIndex, isVisible (일반 인벤토리)
    public event Action<int, bool> OnBattleSlotVisibilityChanged; // slotIndex, isVisible (전투 인벤토리)
    public event Action OnInventoryChanged; // 인벤토리가 변경되었을 때

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
        
        // 이벤트 구독
        OnSlotVisibilityChanged += UpdateSlotVisibility;
        OnBattleSlotVisibilityChanged += UpdateBattleSlotVisibility;
        
        // 저장된 인벤토리 데이터 로드
        LoadSavedInventory();
        
        UpdateSlotVisibility();
    }

    /// <summary>
    /// 저장된 인벤토리 데이터를 자동으로 로드합니다.
    /// </summary>
    private void LoadSavedInventory()
    {
        if (UserDataManager.Instance == null)
            return;

        var invData = UserDataManager.Instance.Get<UserInventoryData>();
        if (invData != null && invData.ItemNames != null && invData.ItemNames.Count > 0)
        {
            LoadInventory(invData.ItemNames);
            Debug.Log($"인벤토리 로드 완료: {invData.ItemNames.Count}개 아이템");
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        OnSlotVisibilityChanged -= UpdateSlotVisibility;
        OnBattleSlotVisibilityChanged -= UpdateBattleSlotVisibility;
    }

    // 슬롯 가시성 업데이트 핸들러
    private void UpdateSlotVisibility(int slotIndex, bool isVisible)
    {
        if (slotIndex >= 0 && slotIndex < slots.Count && slots[slotIndex] != null)
        {
            slots[slotIndex].gameObject.SetActive(isVisible);
        }
    }

    private void UpdateBattleSlotVisibility(int slotIndex, bool isVisible)
    {
        if (slotIndex >= 0 && slotIndex < b_Slots.Count && b_Slots[slotIndex] != null)
        {
            b_Slots[slotIndex].gameObject.SetActive(isVisible);
        }
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

        // 전투 인벤토리에도 동기화
        ItemSlot b_EmptySlot = FindEmptyBattleSlot();
        if (b_EmptySlot != null)
        {
            b_EmptySlot.SetItem(item.IData);
        }

        item.DestroyItem();

        // 슬롯 가시성 업데이트 (이벤트 기반)
        UpdateSlotVisibility();
        
        // 인벤토리 변경 이벤트 발행
        OnInventoryChanged?.Invoke();
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
        // 일반 인벤토리 슬롯 가시성 업데이트 (이벤트 기반)
        for (int i = 0; i < slots.Count; i++)
        {
            OnSlotVisibilityChanged?.Invoke(i, !slots[i].IsEmpty());
        }

        // 전투 인벤토리 슬롯 가시성 업데이트 (이벤트 기반)
        for (int i = 0; i < b_Slots.Count; i++)
        {
            OnBattleSlotVisibilityChanged?.Invoke(i, !b_Slots[i].IsEmpty());
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

    /// <summary>
    /// 현재 인벤토리에 있는 아이템 이름 목록을 반환합니다.
    /// </summary>
    public List<string> GetInventoryItemNames()
    {
        List<string> itemNames = new List<string>();
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty())
            {
                ItemData itemData = slot.GetItemData();
                if (itemData != null && !string.IsNullOrEmpty(itemData.itemName))
                {
                    itemNames.Add(itemData.itemName);
                }
            }
        }
        return itemNames;
    }
    public void AddItemToSlot(ItemData data)
    {
        ItemSlot empty = FindEmptySlot();
        if (empty != null)
            empty.SetItem(data);

        ItemSlot bEmpty = FindEmptyBattleSlot();
        if (bEmpty != null)
            bEmpty.SetItem(data);

        // 슬롯 가시성 업데이트 (이벤트 기반)
        UpdateSlotVisibility();
        
        // 인벤토리 변경 이벤트 발행
        OnInventoryChanged?.Invoke();
    }
}
