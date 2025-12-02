using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 전투 UI를 관리하는 클래스
/// </summary>
public class BattleUIController : MonoBehaviour
{
    [Header("Canvas References")]
    public GameObject battle_canvas;
    public GameObject nonbattle_canvas;
    public TextMeshProUGUI battle_log;

    [Header("UI Buttons")]
    public Button Atk_btn;
    public GameObject Inventory;
    public List<Button> item_btn;
    public Button specialAtk_btn;
    public Button defense_btn;

    [Header("Health Sliders")]
    public Slider playerHealthSlider;
    public Slider enemyHealthSlider;

    // 버튼 클릭 이벤트
    public event Action OnAttackClicked;
    public event Action<int> OnItemUseClicked;
    public event Action OnSpecialAttackClicked;
    public event Action OnDefenseClicked;

    private void Start()
    {
        // Inventory의 자식 버튼들 가져오기
        GetInventoryButtons();

        // 버튼 바인드
        BindButtons();

        // 초기에는 버튼 비활성화
        SetButtonsInteractable(false);
    }

    /// <summary>
    /// Inventory의 자식 요소에서 Button 컴포넌트를 가진 것들을 찾아 리스트에 추가
    /// </summary>
    private void GetInventoryButtons()
    {
        item_btn.Clear();

        if (Inventory != null)
        {
            Button[] buttons = Inventory.GetComponentsInChildren<Button>();
            item_btn.AddRange(buttons);
        }
    }

    /// <summary>
    /// 버튼에 이벤트 리스너 등록
    /// </summary>
    private void BindButtons()
    {
        if (Atk_btn != null)
            Atk_btn.onClick.AddListener(() => OnAttackClicked?.Invoke());

        if (item_btn != null && item_btn.Count > 0)
        {
            for (int i = 0; i < item_btn.Count; i++)
            {
                int index = i; // 클로저 문제 방지
                item_btn[i].onClick.AddListener(() => OnItemUseClicked?.Invoke(index));
            }
        }

        if (specialAtk_btn != null)
            specialAtk_btn.onClick.AddListener(() => OnSpecialAttackClicked?.Invoke());

        if (defense_btn != null)
            defense_btn.onClick.AddListener(() => OnDefenseClicked?.Invoke());
    }

    /// <summary>
    /// 모든 버튼의 상호작용 가능 여부 설정
    /// </summary>
    public void SetButtonsInteractable(bool interactable)
    {
        if (Atk_btn != null)
            Atk_btn.interactable = interactable;

        if (item_btn != null && item_btn.Count > 0)
        {
            foreach (var btn in item_btn)
            {
                if (btn != null)
                    btn.interactable = interactable;
            }
        }

        if (specialAtk_btn != null)
            specialAtk_btn.interactable = interactable;

        if (defense_btn != null)
            defense_btn.interactable = interactable;
    }

    /// <summary>
    /// 전투 UI 활성화
    /// </summary>
    public void ShowBattleUI()
    {
        battle_canvas.SetActive(true);
        nonbattle_canvas.SetActive(false);
    }

    /// <summary>
    /// 일반 UI 활성화
    /// </summary>
    public void HideBattleUI()
    {
        battle_canvas.SetActive(false);
        nonbattle_canvas.SetActive(true);
    }

    /// <summary>
    /// 배틀 로그 텍스트 설정
    /// </summary>
    public void SetBattleLog(string text)
    {
        if (battle_log != null)
            battle_log.text = text;
    }
    public void UpdatePlayerHealthSlider(float currentHp, float maxHp)
    {
        if (playerHealthSlider != null)
        {
            playerHealthSlider.maxValue = maxHp;
            playerHealthSlider.value = currentHp;
        }
    }
    public void UpdateEnemyHealthSlider(float currentHp, float maxHp)
    {
        if (enemyHealthSlider != null)
        {
            enemyHealthSlider.maxValue = maxHp;
            enemyHealthSlider.value = currentHp;
        }
    }

    /// <summary>
    /// 배틀 로그에 텍스트 추가
    /// </summary>
    public void AppendBattleLog(string text)
    {
        if (battle_log != null)
            battle_log.text += text;
    }

    private void OnDestroy()
    {
        // 이벤트 리스너 해제
        if (Atk_btn != null)
            Atk_btn.onClick.RemoveAllListeners();

        if (item_btn != null)
        {
            foreach (var btn in item_btn)
            {
                if (btn != null)
                    btn.onClick.RemoveAllListeners();
            }
        }

        if (specialAtk_btn != null)
            specialAtk_btn.onClick.RemoveAllListeners();

        if (defense_btn != null)
            defense_btn.onClick.RemoveAllListeners();
    }
}