using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    public ItemData IData;
    private SpriteRenderer spriteRenderer;

    private bool isPlayerNear = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        spriteRenderer.sprite = IData.itemImage; //아이템 이미지 가져오기
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
