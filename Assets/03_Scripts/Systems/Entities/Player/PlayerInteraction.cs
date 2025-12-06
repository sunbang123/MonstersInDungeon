using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    bool isFunctionExecuted = false;

    [Header("Button")]
    public Button interButton;
    public Button hideButton;
    public Button pickUpButton;
    public Button runButton;
    public Button inventoryButton;

    private PlayerController _controll;

    [Header("Text")]
    [SerializeField] private TMP_Text hideText;
    [SerializeField] private string walkButtonText = "걷기";
    [SerializeField] private string runButtonText = "달리기";

    [Header("Image")]
    [SerializeField] public Image newItemText;
    [SerializeField] public Canvas inventoryCanvas;

    private void Awake()
    {
        _controll = GetComponent<PlayerController>();

        if (hideButton && pickUpButton != null)
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

        if (newItemText != null)
        {
            newItemText.gameObject.SetActive(false);
        }
        
        if (inventoryCanvas != null)
        {
            inventoryCanvas.gameObject.SetActive(false);
        }

        // player 움직임 이벤트 구독
        if (_controll != null)
        {
            _controll.OnMovementModeChanged += UpdateRunButtonText;
        }
    }
    private void OnDestroy()
    {
        if (_controll != null)
        {
            _controll.OnMovementModeChanged -= UpdateRunButtonText;
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
        InventoryManager.Instance.PickUpItem();

        isFunctionExecuted = true;
        if (isFunctionExecuted == true)
        {
            newItemText.gameObject.SetActive(true);
        }
    }

    // 이동 모드 토글 (걷기 ↔ 뛰기)
    public void ToggleMovementMode()
    {
        MovementMode currentMode = _controll.GetCurrentMode();
        MovementMode newMode = currentMode == MovementMode.Walk ? MovementMode.Run : MovementMode.Walk;

        _controll.SetMovementMode(newMode);
        // UpdateRunButtonText(newMode);
    }

    public void SetMovementMode(MovementMode mode)
    {
        UpdateRunButtonText(mode);
    }

    // 버튼 텍스트 업데이트
    private void UpdateRunButtonText(MovementMode mode)
    {
        if (runButton == null) return;

        TMP_Text buttonText = runButton.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.text = mode == MovementMode.Walk ? runButtonText : walkButtonText;
        }
    }

    public void Inventory()
    {
        isFunctionExecuted = false;
        
        if (inventoryCanvas.gameObject.activeSelf == false)
        {
            inventoryCanvas.gameObject.SetActive(true);
            newItemText.gameObject.SetActive(false);
            _controll.enabled = false;

        }
        else
        {
            inventoryCanvas.gameObject.SetActive(false);
            _controll.enabled = true;
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
