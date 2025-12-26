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

    [Header("Player Status")]
    public GameObject playerStatus;
    public Transform batTransform;
    public Transform nonBatTransform;

    [Header("Health Sliders")]
    public Slider playerHealthSlider;
    public Slider enemyHealthSlider;

    [Header("PP Sliders")]
    public Slider playerPPSlider;
    public Slider enemyPPSlider;

    [Header("Experience Slider")]
    public Slider playerExpSlider;

    [Header("Portraits")]
    public Image playerPortrait;
    public Image enemyPortrait;

    [Header("Level Text")]
    public TextMeshProUGUI playerLevelText;
    public TextMeshProUGUI enemyLevelText;

    // 버튼 클릭 이벤트
    public event Action OnAttackClicked;
    public event Action<int> OnItemUseClicked;
    public event Action OnSpecialAttackClicked;
    public event Action OnDefenseClicked;

    // 전투 로그 이벤트 (외부에서 호출 가능하도록 Action으로 변경)
    public static Action<string> OnBattleLogChanged;
    public static Action<string> OnBattleLogAppended;

    private void Start()
    {
        GetInventoryButtons();
        BindButtons();

        // Player 찾기 및 이벤트 구독 (전투/비전투 모두)
        // Start()보다 늦게 실행되도록 코루틴 사용
        StartCoroutine(InitializePlayerUI());
    }
    
    private System.Collections.IEnumerator InitializePlayerUI()
    {
        // Player가 초기화될 때까지 대기
        yield return null;
        
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.OnHealthChanged += UpdatePlayerHealthSlider;
        }

        SetButtonsInteractable(false);
    }

    /// <summary>
    /// Inventory의 자식 오브젝트에서 Button 컴포넌트를 가져와서 리스트에 추가
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
    /// 버튼의 이벤트 바인딩 처리
    /// </summary>
    private void BindButtons()
    {
        if (Atk_btn != null)
            Atk_btn.onClick.AddListener(() => OnAttackClicked?.Invoke());

        BindItemButtons();

        if (specialAtk_btn != null)
            specialAtk_btn.onClick.AddListener(() => OnSpecialAttackClicked?.Invoke());

        if (defense_btn != null)
            defense_btn.onClick.AddListener(() => OnDefenseClicked?.Invoke());
    }

    /// <summary>
    /// 아이템 버튼만 바인딩하는 메서드
    /// </summary>
    private void BindItemButtons()
    {
        if (item_btn != null && item_btn.Count > 0)
        {
            for (int i = 0; i < item_btn.Count; i++)
            {
                int index = i; // 클로저 문제 해결
                item_btn[i].onClick.AddListener(() => OnItemUseClicked?.Invoke(index));
            }
        }
    }

    /// <summary>
    /// 모든 버튼의 활성화 상태를 설정하는 메서드
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
    /// 전투 UI 상태 토글
    /// </summary>
    private void OnInventoryChanged()
    {
        // 기존 아이템 버튼 리스너 제거
        if (item_btn != null)
        {
            foreach (var btn in item_btn)
            {
                if (btn != null)
                    btn.onClick.RemoveAllListeners();
            }
        }

        // 버튼 다시 가져오기 및 아이템 버튼만 바인딩
        GetInventoryButtons();
        BindItemButtons();
    }


    /// <summary>
    /// 전투 UI 전환 처리
    /// </summary>
    private bool isBattleMode = false;

    public void ToggleBattleUI(bool enableBattle)
    {
        isBattleMode = enableBattle;

        battle_canvas.SetActive(enableBattle);
        nonbattle_canvas.SetActive(!enableBattle);

        Transform targetParent = enableBattle ? batTransform : nonBatTransform;
        bool useWorldSpace = !enableBattle;

        // 스케일 보존을 위해 원래 스케일 저장
        Vector3 originalScale = playerStatus.transform.localScale;

        playerStatus.transform.SetParent(null, false);
        playerStatus.transform.SetParent(targetParent, useWorldSpace);

        // 부모 변경 후 스케일 복원 및 위치 오프셋 초기화 (anchor는 유지)
        playerStatus.transform.localScale = originalScale;
        RectTransform rectTransform = playerStatus.GetComponent<RectTransform>();
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    public void ShowBattleUI()
    {
        ToggleBattleUI(true);
    }

    public void HideBattleUI()
    {
        ToggleBattleUI(false);
    }

    /// <summary>
    /// 전투 로그 텍스트 설정
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
        // 데이터는 playerHp setter에서 자동으로 업데이트되므로 여기서는 업데이트하지 않음
    }
    public void UpdateEnemyHealthSlider(float currentHp, float maxHp)
    {
        if (enemyHealthSlider != null)
        {
            enemyHealthSlider.maxValue = maxHp;
            enemyHealthSlider.value = currentHp;
        }
    }

    public void UpdatePlayerPPSlider(float currentPp, float maxPp)
    {
        if (playerPPSlider != null)
        {
            playerPPSlider.maxValue = maxPp;
            playerPPSlider.value = currentPp;
        }
    }

    public void UpdateEnemyPPSlider(float currentPp, float maxPp)
    {
        if (enemyPPSlider != null)
        {
            enemyPPSlider.maxValue = maxPp;
            enemyPPSlider.value = currentPp;
        }
    }

    public void UpdatePlayerPortrait(Sprite portrait)
    {
        if (playerPortrait != null && portrait != null)
        {
            playerPortrait.sprite = portrait;
        }
    }

    public void UpdateEnemyPortrait(Sprite portrait)
    {
        if (enemyPortrait != null && portrait != null)
        {
            enemyPortrait.sprite = portrait;
        }
    }

    public void UpdatePlayerLevel(int level)
    {
        if (playerLevelText != null)
        {
            playerLevelText.text = $"Lv.{level}";
        }
    }

    public void UpdatePlayerExpSlider(float currentExp, float maxExp)
    {
        if (playerExpSlider != null)
        {
            playerExpSlider.maxValue = maxExp;
            playerExpSlider.value = currentExp;
        }
    }

    public void UpdateEnemyLevel(int level)
    {
        if (enemyLevelText != null)
        {
            enemyLevelText.text = $"Lv.{level}";
        }
    }

    /// <summary>
    /// 전투 로그에 텍스트 추가
    /// </summary>
    public void AppendBattleLog(string text)
    {
        if (battle_log != null)
            battle_log.text += text;
    }

    private void OnDestroy()
    {
        // Player 이벤트 구독 해제
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.OnHealthChanged -= UpdatePlayerHealthSlider;
            player.OnPPChanged -= UpdatePlayerPPSlider;
            player.OnLevelChanged -= UpdatePlayerLevel;
            player.OnExpChanged -= UpdatePlayerExpSlider;
            player.OnPortraitChanged -= UpdatePlayerPortrait;
        }

        // InventoryManager 이벤트 구독 해제
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnBattleSlotVisibilityChanged -= OnBattleSlotVisibilityChanged;
            InventoryManager.Instance.OnInventoryChanged -= OnInventoryChanged;
        }

        // 전투 로그 이벤트 구독 해제
        OnBattleLogChanged -= SetBattleLog;
        OnBattleLogAppended -= AppendBattleLog;

        // 이벤트 리스너 정리
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
