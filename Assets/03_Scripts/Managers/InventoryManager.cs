using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class InventoryManager : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] private int maxSlots = GameConstants.Inventory.DEFAULT_MAX_SLOTS;

    [Header("Battle Inventory Settings")]
    [SerializeField] private int b_MaxSlots = GameConstants.Inventory.DEFAULT_BATTLE_MAX_SLOTS;

    // 캐시된 UI 참조들
    private Transform itemSlotParent;
    private GameObject itemSlotPrefab;
    private Transform b_ItemSlotParent;
    private GameObject b_ItemSlotPrefab;
    private TextMeshProUGUI itemName;
    private TextMeshProUGUI itemDescription;
    private TextMeshProUGUI statName;
    private TextMeshProUGUI statValue;

    private List<ItemData> items = new List<ItemData>();
    private List<ItemSlot> slots = new List<ItemSlot>();
    private List<ItemSlot> b_Slots = new List<ItemSlot>();

    // 프로퍼티: UI 요소들을 자동으로 찾아서 반환
    private Transform ItemSlotParent
    {
        get
        {
            if (itemSlotParent == null)
            {
                GameObject foundObj = UIHelper.FindChild(transform, "ItemSlotParent");
                Transform foundTrs = foundObj != null ? foundObj.transform as Transform : null;
                itemSlotParent = foundTrs != null ? foundTrs : transform;
            }
            return itemSlotParent;
        }
    }

    private GameObject ItemSlotPrefab
    {
        get
        {
            if (itemSlotPrefab == null)
            {
                // GameManager를 통해 Addressable에서 로드된 프리팹 가져오기
                if (GameManager.Instance != null)
                {
                    itemSlotPrefab = GameManager.Instance.TryGetPrefabByName("ItemSlot");
                }
                
                if (itemSlotPrefab == null)
                    Logger.LogWarning("ItemSlot 프리팹을 찾을 수 없습니다. Addressable에서 로드되었는지 확인하세요.");
            }
            return itemSlotPrefab;
        }
    }

    private Transform BattleItemSlotParent
    {
        get
        {
            if (b_ItemSlotParent == null)
            {
                GameObject foundObj = UIHelper.FindChild(transform, "BattleItemSlotParent");
                Transform foundTrs = foundObj != null ? foundObj.transform as Transform : null;
                b_ItemSlotParent = foundTrs != null ? foundTrs : transform;
            }
            return b_ItemSlotParent;
        }
    }

    private GameObject BattleItemSlotPrefab
    {
        get
        {
            if (b_ItemSlotPrefab == null)
            {
                // GameManager를 통해 Addressable에서 로드된 프리팹 가져오기
                if (GameManager.Instance != null)
                {
                    b_ItemSlotPrefab = GameManager.Instance.TryGetPrefabByName("BattleItemSlot");
                }
                
                if (b_ItemSlotPrefab == null)
                    Logger.LogWarning("BattleItemSlot 프리팹을 찾을 수 없습니다. Addressable에서 로드되었는지 확인하세요.");
            }
            return b_ItemSlotPrefab;
        }
    }

    private TextMeshProUGUI ItemName
    {
        get
        {
            if (itemName == null)
                itemName = UIHelper.FindComponentInChildren<TextMeshProUGUI>(transform, "ItemName");
            return itemName;
        }
    }

    private TextMeshProUGUI ItemDescription
    {
        get
        {
            if (itemDescription == null)
                itemDescription = UIHelper.FindComponentInChildren<TextMeshProUGUI>(transform, "ItemDescription");
            return itemDescription;
        }
    }

    private TextMeshProUGUI StatName
    {
        get
        {
            if (statName == null)
                statName = UIHelper.FindComponentInChildren<TextMeshProUGUI>(transform, "StatName");
            return statName;
        }
    }

    private TextMeshProUGUI StatValue
    {
        get
        {
            if (statValue == null)
                statValue = UIHelper.FindComponentInChildren<TextMeshProUGUI>(transform, "StatValue");
            return statValue;
        }
    }

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
        GameObject prefab = ItemSlotPrefab;
        Transform parent = ItemSlotParent;
        
        if (prefab == null || parent == null)
        {
            Logger.LogError("ItemSlot 프리팹 또는 부모 Transform을 찾을 수 없습니다.");
            return;
        }

        for (int i = 0; i < maxSlots; i++)
        {
            GameObject slotObj = Instantiate(prefab, parent);
            ItemSlot slot = slotObj.GetComponent<ItemSlot>();
            if (slot == null)
                slot = slotObj.AddComponent<ItemSlot>();
            slot.Initialize();
            slots.Add(slot);
        }
    }

    private void InitializeBattleSlots()
    {
        GameObject prefab = BattleItemSlotPrefab;
        Transform parent = BattleItemSlotParent;
        
        if (prefab == null || parent == null)
        {
            Logger.LogError("BattleItemSlot 프리팹 또는 부모 Transform을 찾을 수 없습니다.");
            return;
        }

        for (int i = 0; i < b_MaxSlots; i++)
        {
            GameObject slotObj = Instantiate(prefab, parent);
            ItemSlot slot = slotObj.GetComponent<ItemSlot>();
            if (slot == null)
                slot = slotObj.AddComponent<ItemSlot>();
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

    /// <summary>
    /// 전투 인벤토리의 특정 슬롯에서 아이템 데이터를 가져옵니다.
    /// </summary>
    /// <param name="slotIndex">슬롯 인덱스 (버튼 인덱스)</param>
    /// <returns>아이템 데이터, 없으면 null</returns>
    public ItemData GetBattleSlotItem(int slotIndex)
    {
        // 버튼 인덱스를 기반으로 실제 아이템이 있는 슬롯을 찾습니다.
        // 재정렬 후에는 버튼 인덱스와 슬롯 인덱스가 일치해야 하지만,
        // 안전을 위해 실제 아이템이 있는 슬롯을 찾습니다.
        if (slotIndex >= 0 && slotIndex < b_Slots.Count)
        {
            ItemSlot slot = b_Slots[slotIndex];
            if (slot != null && !slot.IsEmpty())
            {
                return slot.GetItemData();
            }
        }
        return null;
    }

    /// <summary>
    /// 전투 인벤토리의 특정 슬롯에서 아이템을 제거합니다.
    /// </summary>
    /// <param name="slotIndex">슬롯 인덱스</param>
    /// <returns>제거 성공 여부</returns>
    public bool RemoveBattleSlotItem(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < b_Slots.Count)
        {
            ItemSlot slot = b_Slots[slotIndex];
            if (slot != null && !slot.IsEmpty())
            {
                ItemData itemData = slot.GetItemData();
                
                // 전투 슬롯에서 제거
                slot.Clear();
                
                // 일반 인벤토리에서도 같은 아이템 제거 (동기화)
                RemoveItemFromInventory(itemData);
                
                // 슬롯 재정렬 (뒤의 아이템들을 앞으로 당김)
                ReorganizeBattleSlots();
                
                // 슬롯 가시성 업데이트
                UpdateSlotVisibility();
                
                // 인벤토리 변경 이벤트 발행
                OnInventoryChanged?.Invoke();
                
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 전투 슬롯을 재정렬합니다. 빈 슬롯을 뒤로 밀고 아이템이 있는 슬롯을 앞으로 당깁니다.
    /// </summary>
    private void ReorganizeBattleSlots()
    {
        // 모든 아이템을 임시 리스트에 저장
        List<ItemData> itemList = new List<ItemData>();
        for (int i = 0; i < b_Slots.Count; i++)
        {
            if (b_Slots[i] != null && !b_Slots[i].IsEmpty())
            {
                itemList.Add(b_Slots[i].GetItemData());
                b_Slots[i].Clear();
            }
        }

        // 앞에서부터 아이템을 다시 배치
        for (int i = 0; i < itemList.Count && i < b_Slots.Count; i++)
        {
            if (b_Slots[i] != null)
            {
                b_Slots[i].SetItem(itemList[i]);
            }
        }
    }

    /// <summary>
    /// 일반 인벤토리에서 특정 아이템을 제거합니다.
    /// </summary>
    /// <param name="itemData">제거할 아이템 데이터</param>
    private void RemoveItemFromInventory(ItemData itemData)
    {
        if (itemData == null) return;

        // 일반 인벤토리에서 같은 아이템 찾아서 제거
        for (int i = 0; i < slots.Count; i++)
        {
            ItemSlot slot = slots[i];
            if (slot != null && !slot.IsEmpty())
            {
                ItemData slotItem = slot.GetItemData();
                if (slotItem != null && slotItem.itemName == itemData.itemName)
                {
                    slot.Clear();
                    // 첫 번째 일치하는 아이템만 제거하고 종료
                    break;
                }
            }
        }

        // items 리스트에서도 제거
        items.RemoveAll(item => item != null && item.itemName == itemData.itemName);
    }

}
