using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Game Data/Item")]
public class ItemData : ScriptableObject
{
    [Header("Item Information")]
    public string itemName;

    public Sprite itemImage;

    public string StatName;

    public int StatValue;
}
