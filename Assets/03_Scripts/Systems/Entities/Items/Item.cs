using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    public ItemData IData;
    private SpriteRenderer spriteRenderer;

    private bool isPlayerNear = false;

    public ItemData GetItemData()
    {
        return IData;
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        spriteRenderer.sprite = IData.itemImage; //아이템의 이미지를 스프라이트에
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNear = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNear = false;
        }
    }
    public bool IsPlayerNear()
    {
        return isPlayerNear;
    }

    public void DestroyItem()
    {
        Destroy(this.gameObject);
    }
}
