using UnityEngine;

/// <summary>
/// 전투 시스템을 조율하는 메인 매니저
/// </summary>
public class BattleManager : SingletonBehaviour<BattleManager>
{
    [Header("Dependencies")]
    [SerializeField] private BattleUIController uiController;

    // 컴포넌트들
    private BattleStateMachine stateMachine;
    private TurnExecutor turnExecutor;
    private BattleFlowController flowController;

    // 플레이어 참조
    private PlayerController playerController;
    private PlayerInteraction playerInteraction;
    private Player player;

    protected override void Init()
    {
        base.Init();

        // 플레이어 찾기
        player = FindObjectOfType<Player>();
        playerController = player.GetComponent<PlayerController>();
        playerInteraction = player.GetComponent<PlayerInteraction>();

        // 같은 GameObject의 컴포넌트들 가져오기
        stateMachine = GetComponent<BattleStateMachine>();
        turnExecutor = GetComponent<TurnExecutor>();
        flowController = GetComponent<BattleFlowController>();

        // 의존성 주입
        turnExecutor.Initialize(stateMachine, uiController);
        flowController.Initialize(stateMachine, uiController, turnExecutor);
        flowController.SetPlayerReferences(player, playerController);

        // UI 이벤트 구독
        SubscribeUIEvents();
    }

    /// <summary>
    /// UI 버튼 이벤트 구독
    /// </summary>
    private void SubscribeUIEvents()
    {
        uiController.OnAttackClicked += OnAttackClicked;
        uiController.OnItemUseClicked += OnItemUseClicked;
        uiController.OnDefenseClicked += OnDefenseClicked;
        uiController.OnSpecialAttackClicked += OnSpecialAttackClicked;
    }

    /// <summary>
    /// 전투 시작 (외부에서 호출)
    /// </summary>
    public void StartBattle(Enemy enemy)
    {
        StartCoroutine(flowController.StartBattle(enemy));
    }

    // ========== UI 이벤트 핸들러 ==========

    private void OnAttackClicked()
    {
        StartCoroutine(turnExecutor.ExecuteAttack());
    }

    private void OnItemUseClicked(int itemIndex)
    {
        StartCoroutine(turnExecutor.ExecuteItemUse(itemIndex));
    }

    private void OnDefenseClicked()
    {
        StartCoroutine(turnExecutor.ExecuteDefense());
    }

    private void OnSpecialAttackClicked()
    {
        StartCoroutine(turnExecutor.ExecuteSpecialAttack());
    }

    // ========== 정리 ==========

    protected override void Dispose()
    {
        base.Dispose();

        // UI 이벤트 구독 해제
        if (uiController != null)
        {
            uiController.OnAttackClicked -= OnAttackClicked;
            uiController.OnItemUseClicked -= OnItemUseClicked;
            uiController.OnDefenseClicked -= OnDefenseClicked;
            uiController.OnSpecialAttackClicked -= OnSpecialAttackClicked;
        }
    }
}