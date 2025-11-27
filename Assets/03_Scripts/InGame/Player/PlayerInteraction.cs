using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.U2D.Path.GUIFramework;

public class PlayerInteraction : MonoBehaviour
{
    public Button interButton;
    public Button hideButton;
    public Button pickUpButton;
    public Button runButton;

    private PlayerController _controll;
    [SerializeField] private TMP_Text hideText;
    [SerializeField] private string walkButtonText = "걷기";
    [SerializeField] private string runButtonText = "달리기";

    private void Awake()
    {
        _controll = GetComponent<PlayerController>();

        if (hideButton && pickUpButton!= null)
        {
            hideButton.gameObject.SetActive(false);
            pickUpButton.gameObject.SetActive(false); 
        }
        if (interButton != null)
        {
            interButton.interactable = false;
        }
        if (runButton != null)
        {
            runButton.onClick.AddListener(ToggleMovementMode);
            UpdateRunButtonText(MovementMode.Walk); // 초기 텍스트 설정
        }
    }

    public void Hide()
    {
        SpriteRenderer _sprite = GetComponent<SpriteRenderer>();
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

    // 이동 모드 토글 (걷기 ↔ 뛰기)
    public void ToggleMovementMode()
    {
        MovementMode currentMode = _controll.GetCurrentMode();
        MovementMode newMode = currentMode == MovementMode.Walk ? MovementMode.Run : MovementMode.Walk;

        _controll.SetMovementMode(newMode);
        UpdateRunButtonText(newMode);
    }

    // 버튼 텍스트 업데이트
    private void UpdateRunButtonText(MovementMode mode)
    {
        TMP_Text buttonText = runButton.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.text = mode == MovementMode.Walk ? runButtonText : walkButtonText;
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
