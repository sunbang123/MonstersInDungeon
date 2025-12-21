using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    private Image itemImage;
    private ItemData currentItem;

    public void Initialize()
    {
        Image[] images = GetComponentsInChildren<Image>();
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i].gameObject != gameObject)
            {
                itemImage = images[i];
                break;
            }
        }
        Clear();
    }

    public bool IsEmpty() => currentItem == null;

    public ItemData GetItemData() => currentItem;

    public void SetItem(ItemData itemData)
    {
        currentItem = itemData;
        if (itemImage != null)
        {
            itemImage.sprite = itemData.itemImage;
            itemImage.enabled = true;
        }
    }

    public void Clear()
    {
        currentItem = null;
        if (itemImage != null)
        {
            itemImage.sprite = null;
            itemImage.enabled = false;
        }
    }
}