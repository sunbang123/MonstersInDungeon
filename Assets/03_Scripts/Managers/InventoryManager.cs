using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : SingletonBehaviour<InventoryManager>
{
    [Header("Inventory Settings")]
    [SerializeField] private Transform itemSlotParent; // ItemSlot들이 배치될 부모 오브젝트
    [SerializeField] private GameObject itemSlotPrefab; // ItemSlot 프리팹
    [SerializeField] private int maxSlots = 20; // 최대 슬롯 개수

    private List<ItemData> items = new List<ItemData>(); // 인벤토리에 담긴 아이템들

    // 
    public void PickUpItem()
    {
        Item[] items = FindObjectsOfType<Item>();

        foreach (Item item in items)
        {
            if (item.IsPlayerNear())
            { 
                item.DestroyItem();
            }
        }

        // 아이템을 인벤토리에 추가하는 로직 구현 필요
        // itemSlotParent 아래에 itemSlotPrefab을 인스턴스화하고,
        // 해당 슬롯에 아이템 정보를 설정하는 코드를 작성해야 합니다.
    }
}