using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    public Button interButton;
    public Button hideButton;
    public Button pickUpButton;



    [SerializeField] private TMP_Text hideText;

    private void Awake()
    {
        if (hideButton && pickUpButton!= null)
        {
            hideButton.gameObject.SetActive(false);
            pickUpButton.gameObject.SetActive(false); 
        }
        if (interButton != null)
        {
            interButton.interactable = false;
        }
    }

    public void Hide()
    {
        SpriteRenderer _sprite = GetComponent<SpriteRenderer>();
        PlayerController _controll = GetComponent<PlayerController>();
        if (_sprite.enabled == true)
        {
            _sprite.enabled = false;
            _controll.enabled = false;
            hideText.text = "나가기";
        }
        else if (_sprite.enabled == false)
        {
            _sprite.enabled = true;
            _controll.enabled = true;
            hideText.text = "숨기";
        }
    }


    public void PickUp()
    {
        Item[] items = FindObjectsOfType<Item>();

        foreach (Item item in items)
        {
            if (item.IsPlayerNear())
            {
                item.DestroyItem();
                Debug.Log("Item picked up!");
                return;
            }
        }
    }


    //버튼 활성화 비활성화
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Hide"))
        {
            hideButton.gameObject.SetActive(true);
            interButton.gameObject.SetActive(false);
        }
        else if (collision.gameObject.CompareTag("Item"))
        {
            pickUpButton.gameObject.SetActive(true);
            interButton.gameObject.SetActive(false);
            Debug.Log("줍기");
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Hide"))
        {
            hideButton.gameObject.SetActive(false);
            interButton.gameObject.SetActive(true);
        }
        else if (collision.gameObject.CompareTag("Item"))
        {
            pickUpButton.gameObject.SetActive(false);
            interButton.gameObject.SetActive(true);
        }
    }
}
