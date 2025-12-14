using UnityEngine;

/// <summary>
/// 배틀 시스템을 관리하는 메인 매니저
/// </summary>
public class BattleManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BattleUIController uiController;

    // 플레이어 참조 (인스펙터에서 설정하거나 자동 찾기)
    [SerializeField] private Player playerReference;

    // 컴포넌트들
    private BattleStateMachine stateMachine;
    private TurnExecutor turnExecutor;
    private BattleFlowController flowController;

    // 플레이어 관련 컴포넌트들
    private PlayerController playerController;
    private PlayerInteraction playerInteraction;
    private Player player;

    // 싱글톤 인스턴스
    protected static BattleManager m_Instance;

    public static BattleManager Instance
    {
        get { return m_Instance; }
    }

    protected void Awake()
    {
        Init();
    }
    protected void Init()
    {
        if (m_Instance == null)
        {
            m_Instance = (BattleManager)this;
        }

        // 플레이어 참조 설정
        SetupPlayerReferences();

        // 게임 오브젝트의 컴포넌트들 가져오기
        stateMachine = GetComponent<BattleStateMachine>();
        turnExecutor = GetComponent<TurnExecutor>();
        flowController = GetComponent<BattleFlowController>();

        // 컴포넌트 초기화
        turnExecutor.Initialize(stateMachine, uiController);
        flowController.Initialize(stateMachine, uiController, turnExecutor);
        flowController.SetPlayerReferences(player, playerController);

        // UI 이벤트 구독
        SubscribeUIEvents();
    }

    /// <summary>
    /// 플레이어 참조 설정 (인스펙터 우선, 없으면 자동 찾기)
    /// </summary>
    private void SetupPlayerReferences()
    {
        // 1. 인스펙터에서 설정된 참조 사용
        if (playerReference != null)
        {
            player = playerReference;
        }
        else
        {
            // 2. 태그로 플레이어 찾기 (더 효율적)
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.GetComponent<Player>();
            }
            else
            {
                // 3. 최후의 수단으로 FindObjectOfType 사용 (비효율적)
                Logger.LogWarning("Player reference not set in inspector, using FindObjectOfType");
                player = FindObjectOfType<Player>();
            }
        }

        // 플레이어 컴포넌트들 가져오기
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
            playerInteraction = player.GetComponent<PlayerInteraction>();

            if (playerController == null)
                Logger.LogError("PlayerController component not found on Player");
            if (playerInteraction == null)
                Logger.LogError("PlayerInteraction component not found on Player");
        }
        else
        {
            Logger.LogError("Player not found in scene!");
        }
    }

    /// <summary>
    /// UI ��ư �̺�Ʈ ����
    /// </summary>
    private void SubscribeUIEvents()
    {
        uiController.OnAttackClicked += OnAttackClicked;
        uiController.OnItemUseClicked += OnItemUseClicked;
        uiController.OnDefenseClicked += OnDefenseClicked;
        uiController.OnSpecialAttackClicked += OnSpecialAttackClicked;
    }

    /// <summary>
    /// ���� ���� (�ܺο��� ȣ��)
    /// </summary>
    public void StartBattle(Enemy enemy)
    {
        StartCoroutine(flowController.StartBattle(enemy));
    }

    // ========== UI �̺�Ʈ �ڵ鷯 ==========

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

    // ========== ���� ==========

    protected void OnDestroy()
    {
        Dispose();
    }

    private void Dispose()
    {
        m_Instance = null;
        // UI �̺�Ʈ ���� ����
        if (uiController != null)
        {
            uiController.OnAttackClicked -= OnAttackClicked;
            uiController.OnItemUseClicked -= OnItemUseClicked;
            uiController.OnDefenseClicked -= OnDefenseClicked;
            uiController.OnSpecialAttackClicked -= OnSpecialAttackClicked;
        }
    }
}