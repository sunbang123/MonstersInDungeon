using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 전투 UI를 관리하는 클래스 - Data-Driven UI 구조
/// </summary>
public class BattleUIController : MonoBehaviour
{
    // UI 데이터 모델
    private BattleUIData uiData;
    private BattleStateData stateData = new BattleStateData();

    // 캐시된 UI 참조들
    private GameObject battleCanvas;
    private TextMeshProUGUI battleLog;
    private Button attackButton;
    private Button specialAttackButton;
    private Button defenseButton;
    private GameObject inventoryContainer;
    private GameObject playerStatus;
    private Slider playerHealthSlider;
    private Slider enemyHealthSlider;
    private Slider playerPPSlider;
    private Slider enemyPPSlider;
    private Slider playerExpSlider;
    private Image playerPortrait;
    private Image enemyPortrait;
    private TextMeshProUGUI playerLevelText;
    private TextMeshProUGUI enemyLevelText;

    private List<Button> itemButtons = new List<Button>();

    // 버튼 클릭 이벤트
    public event Action OnAttackClicked;
    public event Action<int> OnItemUseClicked;
    public event Action OnSpecialAttackClicked;
    public event Action OnDefenseClicked;

    // 전투 로그 이벤트 (외부에서 호출 가능하도록 Action으로 변경)
    public static Action<string> OnBattleLogChanged;
    public static Action<string> OnBattleLogAppended;

    // 프로퍼티: UI 요소들을 자동으로 찾아서 반환
    private GameObject BattleCanvas
    {
        get
        {
            if (battleCanvas == null)
            {
                // Canvas 찾기
                Canvas canvas = GetComponentInParent<Canvas>();
                if (canvas == null)
                    canvas = FindObjectOfType<Canvas>();
                battleCanvas = canvas != null ? canvas.gameObject : gameObject;
            }
            return battleCanvas;
        }
    }

    private TextMeshProUGUI BattleLog
    {
        get
        {
            if (battleLog == null)
                battleLog = UIHelper.FindComponentInChildren<TextMeshProUGUI>(transform, "BattleLog");
            return battleLog;
        }
    }

    private Button AttackButton
    {
        get
        {
            if (attackButton == null)
                attackButton = UIHelper.FindComponentInChildren<Button>(transform, "AttackButton");
            return attackButton;
        }
    }

    private Button SpecialAttackButton
    {
        get
        {
            if (specialAttackButton == null)
                specialAttackButton = UIHelper.FindComponentInChildren<Button>(transform, "SpecialAttackButton");
            return specialAttackButton;
        }
    }

    private Button DefenseButton
    {
        get
        {
            if (defenseButton == null)
                defenseButton = UIHelper.FindComponentInChildren<Button>(transform, "DefenseButton");
            return defenseButton;
        }
    }

    private GameObject InventoryContainer
    {
        get
        {
            if (inventoryContainer == null)
                inventoryContainer = UIHelper.FindChild(transform, "Inventory");
            return inventoryContainer;
        }
    }

    private GameObject PlayerStatus
    {
        get
        {
            if (playerStatus == null)
                playerStatus = UIHelper.FindChild(transform, "PlayerStatus");
            return playerStatus;
        }
    }

    private Slider PlayerHealthSlider
    {
        get
        {
            if (playerHealthSlider == null)
                playerHealthSlider = UIHelper.FindComponentInChildren<Slider>(transform, "PlayerHealthSlider");
            return playerHealthSlider;
        }
    }

    private Slider EnemyHealthSlider
    {
        get
        {
            if (enemyHealthSlider == null)
                enemyHealthSlider = UIHelper.FindComponentInChildren<Slider>(transform, "EnemyHealthSlider");
            return enemyHealthSlider;
        }
    }

    private Slider PlayerPPSlider
    {
        get
        {
            if (playerPPSlider == null)
                playerPPSlider = UIHelper.FindComponentInChildren<Slider>(transform, "PlayerPPSlider");
            return playerPPSlider;
        }
    }

    private Slider EnemyPPSlider
    {
        get
        {
            if (enemyPPSlider == null)
                enemyPPSlider = UIHelper.FindComponentInChildren<Slider>(transform, "EnemyPPSlider");
            return enemyPPSlider;
        }
    }

    private Slider PlayerExpSlider
    {
        get
        {
            if (playerExpSlider == null)
                playerExpSlider = UIHelper.FindComponentInChildren<Slider>(transform, "PlayerExpSlider");
            return playerExpSlider;
        }
    }

    private Image PlayerPortrait
    {
        get
        {
            if (playerPortrait == null)
                playerPortrait = UIHelper.FindComponentInChildren<Image>(transform, "PlayerPortrait");
            return playerPortrait;
        }
    }

    private Image EnemyPortrait
    {
        get
        {
            if (enemyPortrait == null)
                enemyPortrait = UIHelper.FindComponentInChildren<Image>(transform, "EnemyPortrait");
            return enemyPortrait;
        }
    }

    private TextMeshProUGUI PlayerLevelText
    {
        get
        {
            if (playerLevelText == null)
                playerLevelText = UIHelper.FindComponentInChildren<TextMeshProUGUI>(transform, "PlayerLevelText");
            return playerLevelText;
        }
    }

    private TextMeshProUGUI EnemyLevelText
    {
        get
        {
            if (enemyLevelText == null)
                enemyLevelText = UIHelper.FindComponentInChildren<TextMeshProUGUI>(transform, "EnemyLevelText");
            return enemyLevelText;
        }
    }

    private List<Button> ItemButtons
    {
        get
        {
            if (itemButtons == null || itemButtons.Count == 0)
            {
                itemButtons.Clear();
                GameObject container = InventoryContainer;
                if (container != null)
                {
                    Button[] buttons = container.GetComponentsInChildren<Button>();
                    itemButtons.AddRange(buttons);
                }
            }
            return itemButtons;
        }
    }

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
            player.OnHealthChanged += UpdatePlayerHealth;
            player.OnPPChanged += UpdatePlayerPP;
            player.OnLevelChanged += UpdatePlayerLevel;
            player.OnExpChanged += UpdatePlayerExp;
            player.OnPortraitChanged += UpdatePlayerPortrait;
            
            // 초기값 설정 (Player.Start()가 실행된 후)
            UpdatePlayerHealth(player.playerHp, player.maxHp);
            UpdatePlayerPP(player.playerPp, player.maxMp);
            UpdatePlayerLevel(player.level);
            UpdatePlayerExp(player.currentExp, player.expToNextLevel);
            UpdatePlayerPortrait(player.portrait);
        }

        // InventoryManager 이벤트 구독
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnBattleSlotVisibilityChanged += OnBattleSlotVisibilityChanged;
            InventoryManager.Instance.OnInventoryChanged += OnInventoryChanged;
        }

        // 전투 로그 이벤트 구독
        OnBattleLogChanged += SetBattleLog;
        OnBattleLogAppended += AppendBattleLog;

        SetButtonsInteractable(false);
    }

    /// <summary>
    /// Inventory의 자식 오브젝트에서 Button 컴포넌트를 가져와서 리스트에 추가
    /// </summary>
    private void GetInventoryButtons()
    {
        itemButtons.Clear();
        GameObject container = InventoryContainer;
        if (container != null)
        {
            Button[] buttons = container.GetComponentsInChildren<Button>();
            itemButtons.AddRange(buttons);
        }
    }

    /// <summary>
    /// 버튼의 이벤트 바인딩 처리
    /// </summary>
    private void BindButtons()
    {
        Button atkBtn = AttackButton;
        if (atkBtn != null)
            atkBtn.onClick.AddListener(() => OnAttackClicked?.Invoke());

        BindItemButtons();

        Button specialBtn = SpecialAttackButton;
        if (specialBtn != null)
            specialBtn.onClick.AddListener(() => OnSpecialAttackClicked?.Invoke());

        Button defBtn = DefenseButton;
        if (defBtn != null)
            defBtn.onClick.AddListener(() => OnDefenseClicked?.Invoke());
    }

    /// <summary>
    /// 아이템 버튼만 바인딩하는 메서드
    /// </summary>
    private void BindItemButtons()
    {
        List<Button> buttons = ItemButtons;
        if (buttons != null && buttons.Count > 0)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                int index = i; // 클로저 문제 해결
                if (buttons[i] != null)
                    buttons[i].onClick.AddListener(() => OnItemUseClicked?.Invoke(index));
            }
        }
    }

    /// <summary>
    /// 모든 버튼의 활성화 상태를 설정하는 메서드
    /// </summary>
    public void SetButtonsInteractable(bool interactable)
    {
        Button atkBtn = AttackButton;
        if (atkBtn != null)
            atkBtn.interactable = interactable;

        List<Button> buttons = ItemButtons;
        if (buttons != null && buttons.Count > 0)
        {
            foreach (var btn in buttons)
            {
                if (btn != null)
                    btn.interactable = interactable;
            }
        }

        Button specialBtn = SpecialAttackButton;
        if (specialBtn != null)
            specialBtn.interactable = interactable;

        Button defBtn = DefenseButton;
        if (defBtn != null)
            defBtn.interactable = interactable;

        stateData.buttonsInteractable = interactable;
    }

    /// <summary>
    /// 특정 아이템 버튼의 활성화 상태를 설정하는 메서드
    /// </summary>
    /// <param name="itemIndex">아이템 버튼 인덱스</param>
    /// <param name="interactable">활성화 여부</param>
    public void SetItemButtonInteractable(int itemIndex, bool interactable)
    {
        List<Button> buttons = ItemButtons;
        if (buttons != null && itemIndex >= 0 && itemIndex < buttons.Count)
        {
            if (buttons[itemIndex] != null)
            {
                buttons[itemIndex].interactable = interactable;
            }
        }
    }

    /// <summary>
    /// 배틀 인벤토리 슬롯 가시성 변경 이벤트 핸들러
    /// </summary>
    private void OnBattleSlotVisibilityChanged(int slotIndex, bool isVisible)
    {
        List<Button> buttons = ItemButtons;
        if (buttons != null && slotIndex >= 0 && slotIndex < buttons.Count)
        {
            if (buttons[slotIndex] != null)
            {
                // 아이템 사용 중이거나 다른 액션 중이면 버튼을 활성화하지 않음
                bool shouldBeInteractable = isVisible;
                
                // BattleStateMachine을 찾아서 현재 전투 상태 확인
                BattleStateMachine stateMachine = FindObjectOfType<BattleStateMachine>();
                if (stateMachine != null)
                {
                    // 아이템 사용 중이거나 다른 액션 중이면 비활성화 유지
                    if (stateMachine.PlayerState == PlayerState.ItemUse ||
                        stateMachine.PlayerState == PlayerState.Attack ||
                        stateMachine.PlayerState == PlayerState.Defense ||
                        stateMachine.BattleState != BattleState.PlayerTurn)
                    {
                        shouldBeInteractable = false;
                    }
                }
                
                buttons[slotIndex].interactable = shouldBeInteractable;
            }
        }
    }

    /// <summary>
    /// 인벤토리 변경 이벤트 핸들러 - 버튼을 다시 가져와서 바인딩
    /// </summary>
    private void OnInventoryChanged()
    {
        // 기존 아이템 버튼 리스너 제거
        List<Button> buttons = ItemButtons;
        if (buttons != null)
        {
            foreach (var btn in buttons)
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
    public void ToggleBattleUI(bool enableBattle)
    {
        GameObject canvas = BattleCanvas;
        if (canvas != null)
        {
            canvas.SetActive(enableBattle);
        }
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
        TextMeshProUGUI log = BattleLog;
        if (log != null)
            log.text = text;
        
        stateData.battleLogText = text;
    }

    public void UpdatePlayerHealth(float currentHp, float maxHp)
    {
        Slider slider = PlayerHealthSlider;
        if (slider != null)
        {
            slider.maxValue = maxHp;
            slider.value = currentHp;
        }
        
        stateData.playerHealth = currentHp;
        stateData.playerMaxHealth = maxHp;
    }

    public void UpdateEnemyHealth(float currentHp, float maxHp)
    {
        Slider slider = EnemyHealthSlider;
        if (slider != null)
        {
            slider.maxValue = maxHp;
            slider.value = currentHp;
        }
        
        stateData.enemyHealth = currentHp;
        stateData.enemyMaxHealth = maxHp;
    }

    public void UpdatePlayerPP(float currentPp, float maxPp)
    {
        Slider slider = PlayerPPSlider;
        if (slider != null)
        {
            slider.maxValue = maxPp;
            slider.value = currentPp;
        }
        
        stateData.playerPP = currentPp;
        stateData.playerMaxPP = maxPp;
    }

    public void UpdateEnemyPP(float currentPp, float maxPp)
    {
        Slider slider = EnemyPPSlider;
        if (slider != null)
        {
            slider.maxValue = maxPp;
            slider.value = currentPp;
        }
        
        stateData.enemyPP = currentPp;
        stateData.enemyMaxPP = maxPp;
    }

    public void UpdatePlayerPortrait(Sprite portrait)
    {
        Image img = PlayerPortrait;
        if (img != null && portrait != null)
        {
            img.sprite = portrait;
        }
        
        stateData.playerPortrait = portrait;
    }

    public void UpdateEnemyPortrait(Sprite portrait)
    {
        Image img = EnemyPortrait;
        if (img != null && portrait != null)
        {
            img.sprite = portrait;
        }
        
        stateData.enemyPortrait = portrait;
    }

    public void UpdatePlayerLevel(int level)
    {
        TextMeshProUGUI text = PlayerLevelText;
        if (text != null)
        {
            text.text = $"Lv.{level}";
        }
        
        stateData.playerLevel = level;
    }

    public void UpdatePlayerExp(float currentExp, float maxExp)
    {
        Slider slider = PlayerExpSlider;
        if (slider != null)
        {
            slider.maxValue = maxExp;
            slider.value = currentExp;
        }
        
        stateData.playerExp = currentExp;
        stateData.playerExpToNextLevel = maxExp;
    }

    public void UpdateEnemyLevel(int level)
    {
        TextMeshProUGUI text = EnemyLevelText;
        if (text != null)
        {
            text.text = $"Lv.{level}";
        }
        
        stateData.enemyLevel = level;
    }

    // 이전 메서드명 호환성을 위한 래퍼
    public void UpdatePlayerHealthSlider(float currentHp, float maxHp) => UpdatePlayerHealth(currentHp, maxHp);
    public void UpdatePlayerPPSlider(float currentPp, float maxPp) => UpdatePlayerPP(currentPp, maxPp);
    public void UpdatePlayerExpSlider(float currentExp, float maxExp) => UpdatePlayerExp(currentExp, maxExp);
    public void UpdateEnemyHealthSlider(float currentHp, float maxHp) => UpdateEnemyHealth(currentHp, maxHp);
    public void UpdateEnemyPPSlider(float currentPp, float maxPp) => UpdateEnemyPP(currentPp, maxPp);

    /// <summary>
    /// 전투 로그에 텍스트 추가
    /// </summary>
    public void AppendBattleLog(string text)
    {
        TextMeshProUGUI log = BattleLog;
        if (log != null)
            log.text += text;
        
        stateData.battleLogText += text;
    }

    private void OnDestroy()
    {
        // Player 이벤트 구독 해제
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.OnHealthChanged -= UpdatePlayerHealth;
            player.OnPPChanged -= UpdatePlayerPP;
            player.OnLevelChanged -= UpdatePlayerLevel;
            player.OnExpChanged -= UpdatePlayerExp;
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
        Button atkBtn = AttackButton;
        if (atkBtn != null)
            atkBtn.onClick.RemoveAllListeners();

        List<Button> buttons = ItemButtons;
        if (buttons != null)
        {
            foreach (var btn in buttons)
            {
                if (btn != null)
                    btn.onClick.RemoveAllListeners();
            }
        }

        Button specialBtn = SpecialAttackButton;
        if (specialBtn != null)
            specialBtn.onClick.RemoveAllListeners();

        Button defBtn = DefenseButton;
        if (defBtn != null)
            defBtn.onClick.RemoveAllListeners();
    }
}
