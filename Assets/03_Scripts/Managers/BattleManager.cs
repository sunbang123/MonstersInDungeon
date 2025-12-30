using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

/// <summary>
/// 배틀 시스템을 관리하는 메인 매니저
/// </summary>
public class BattleManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BattleUIController uiController;

    [Header("Scene References")]
    [SerializeField] private AssetReference inBattleSceneReference;
    [SerializeField] private AssetReference inGameSceneReference;

    // 컴포넌트 프로퍼티 (런타임에 자동으로 찾아서 설정)
    private BattleStateMachine _stateMachine;
    private TurnExecutor _turnExecutor;
    private BattleFlowController _flowController;

    /// <summary>
    /// BattleUIController 참조 (런타임에 설정 가능)
    /// </summary>
    public BattleUIController UIController
    {
        get => uiController;
        set
        {
            uiController = value;
            // 참조가 변경되면 관련 컴포넌트 재초기화
            if (uiController != null && StateMachine != null && TurnExecutor != null && FlowController != null)
            {
                TurnExecutor.Initialize(StateMachine, uiController);
                FlowController.Initialize(StateMachine, uiController, TurnExecutor);
                SubscribeUIEvents();
            }
        }
    }

    /// <summary>
    /// BattleStateMachine 프로퍼티 (자동으로 찾아서 반환)
    /// </summary>
    public BattleStateMachine StateMachine
    {
        get
        {
            if (_stateMachine == null)
            {
                _stateMachine = GetComponent<BattleStateMachine>();
            }
            return _stateMachine;
        }
    }

    /// <summary>
    /// TurnExecutor 프로퍼티 (자동으로 찾아서 반환)
    /// </summary>
    public TurnExecutor TurnExecutor
    {
        get
        {
            if (_turnExecutor == null)
            {
                _turnExecutor = GetComponent<TurnExecutor>();
            }
            return _turnExecutor;
        }
    }

    /// <summary>
    /// BattleFlowController 프로퍼티 (자동으로 찾아서 반환)
    /// </summary>
    public BattleFlowController FlowController
    {
        get
        {
            if (_flowController == null)
            {
                _flowController = GetComponent<BattleFlowController>();
            }
            return _flowController;
        }
    }

    /// <summary>
    /// InBattle 씬 참조 (런타임에 설정 가능)
    /// </summary>
    public AssetReference InBattleSceneReference
    {
        get => inBattleSceneReference;
        set => inBattleSceneReference = value;
    }

    /// <summary>
    /// InGame 씬 참조 (런타임에 설정 가능)
    /// </summary>
    public AssetReference InGameSceneReference
    {
        get => inGameSceneReference;
        set => inGameSceneReference = value;
    }


    // 싱글톤 인스턴스
    private static BattleManager m_Instance;

    public static BattleManager Instance
    {
        get 
        { 
            if (m_Instance == null)
            {
                m_Instance = FindObjectOfType<BattleManager>();
            }
            return m_Instance; 
        }
    }

    protected void Awake()
    {
        Init();
    }

    private void Start()
    {
        // InBattle 씬이 로드되면 자동으로 전투 시작
        if (BattleDataTransfer.IsBattleActive())
        {
            StartBattleFromData();
        }
    }

    protected void Init()
    {
        // 싱글톤 중복 방지
        if (m_Instance != null && m_Instance != this)
        {
            Logger.LogWarning("BattleManager 인스턴스가 이미 존재합니다. 중복 인스턴스를 제거합니다.");
            Destroy(gameObject);
            return;
        }
        
        m_Instance = this;

        // UI Controller가 인스펙터에서 설정되지 않았으면 자동으로 찾기
        if (uiController == null)
        {
            uiController = FindObjectOfType<BattleUIController>();
            if (uiController == null)
            {
                Logger.LogWarning("BattleUIController를 찾을 수 없습니다. 런타임에 UIController property로 설정해주세요.");
            }
        }

        // 컴포넌트 초기화 (프로퍼티를 통해 자동으로 가져옴)
        if (uiController != null && StateMachine != null && TurnExecutor != null && FlowController != null)
        {
            TurnExecutor.Initialize(StateMachine, uiController);
            FlowController.Initialize(StateMachine, uiController, TurnExecutor);
            FlowController.SetSceneReferences(inBattleSceneReference, inGameSceneReference);

            // UI 이벤트 구독
            SubscribeUIEvents();
        }
    }

    /// <summary>
    /// BattleDataTransfer에서 데이터를 읽어 전투 시작
    /// </summary>
    private void StartBattleFromData()
    {
        if (FlowController == null)
        {
            Logger.LogError("BattleFlowController가 초기화되지 않았습니다.");
            return;
        }
        if (uiController == null)
        {
            Logger.LogError("BattleUIController가 설정되지 않았습니다.");
            return;
        }

        // Enemy 데이터 가져오기
        var enemyData = BattleDataTransfer.GetEnemyData();
        if (!enemyData.HasValue)
        {
            Logger.LogError("Enemy 데이터를 찾을 수 없습니다.");
            return;
        }

        Logger.Log($"비전투씬에서 전달받은 적 정보 - HP: {enemyData.Value.enemyHp}/{enemyData.Value.maxHp}, Level: {enemyData.Value.level}");

        // InBattle 씬에서 Enemy 찾기 또는 생성
        Enemy enemy = FindObjectOfType<Enemy>();
        
        if (enemy == null)
        {
            // Enemy가 없으면 생성
            enemy = CreateEnemyFromData(enemyData.Value);
            if (enemy == null)
            {
                Logger.LogError("Enemy를 생성할 수 없습니다.");
                return;
            }
            Logger.Log("Enemy를 새로 생성했습니다.");
        }
        else
        {
            // Enemy가 있으면 데이터만 적용 (비전투씬에서 충돌한 적의 정보로 업데이트)
            BattleDataTransfer.ApplyEnemyData(enemy, enemyData.Value);
            Logger.Log($"기존 Enemy에 비전투씬 데이터 적용 완료 - HP: {enemy.enemyHp}/{enemy.maxHp}, Level: {enemy.level}");
        }

        // 전투 시작
        Logger.Log("InBattle 씬에서 전투 자동 시작");
        StartCoroutine(FlowController.StartBattle(enemy));
    }

    /// <summary>
    /// Enemy 데이터로부터 Enemy 오브젝트 생성
    /// </summary>
    private Enemy CreateEnemyFromData(BattleDataTransfer.EnemyData data)
    {
        GameObject enemyObj = null;

        // 1. GameManager에서 Enemy 프리팹 가져오기
        if (GameManager.Instance != null)
        {
            GameObject prefab = GameManager.Instance.TryGetPrefabByName("Enemy");
            if (prefab != null)
            {
                enemyObj = Instantiate(prefab);
            }
        }

        // 2. 프리팹이 없으면 빈 게임오브젝트에 Enemy 컴포넌트 추가
        if (enemyObj == null)
        {
            // InBattle 씬에서 "Enemy" 이름을 가진 빈 게임오브젝트 찾기
            GameObject existingObj = GameObject.Find("Enemy");
            if (existingObj != null)
            {
                enemyObj = existingObj;
            }
            else
            {
                // 없으면 새로 생성
                enemyObj = new GameObject("Enemy");
            }
        }

        // Enemy 컴포넌트 추가 또는 가져오기
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        if (enemy == null)
        {
            enemy = enemyObj.AddComponent<Enemy>();
        }

        // 데이터 적용
        if (enemy != null)
        {
            BattleDataTransfer.ApplyEnemyData(enemy, data);
        }

        return enemy;
    }

    /// <summary>
    /// UI 버튼 이벤트 구독
    /// </summary>
    private void SubscribeUIEvents()
    {
        if (uiController == null)
        {
            Logger.LogWarning("BattleUIController가 null이어서 UI 이벤트를 구독할 수 없습니다.");
            return;
        }

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
        if (FlowController == null)
        {
            Logger.LogError("BattleFlowController가 초기화되지 않았습니다.");
            return;
        }
        if (uiController == null)
        {
            Logger.LogError("BattleUIController가 설정되지 않았습니다. UIController property를 설정해주세요.");
            return;
        }
        StartCoroutine(FlowController.StartBattle(enemy));
    }

    // ========== UI 이벤트 핸들러 ==========

    private void OnAttackClicked()
    {
        if (TurnExecutor != null)
        {
            StartCoroutine(TurnExecutor.ExecuteAttack());
        }
    }

    private void OnItemUseClicked(int itemIndex)
    {
        // 아이템 사용 시작 시 즉시 모든 버튼 비활성화 (중복 클릭 방지)
        if (uiController != null)
        {
            uiController.SetButtonsInteractable(false);
        }
        
        if (TurnExecutor != null)
        {
            StartCoroutine(TurnExecutor.ExecuteItemUse(itemIndex));
        }
    }

    private void OnDefenseClicked()
    {
        if (TurnExecutor != null)
        {
            StartCoroutine(TurnExecutor.ExecuteDefense());
        }
    }

    private void OnSpecialAttackClicked()
    {
        if (TurnExecutor != null)
        {
            StartCoroutine(TurnExecutor.ExecuteSpecialAttack());
        }
    }

    // ========== 정리 ==========

    protected void OnDestroy()
    {
        Dispose();
    }

    private void Dispose()
    {
        // UI 이벤트 구독 해제
        if (uiController != null)
        {
            uiController.OnAttackClicked -= OnAttackClicked;
            uiController.OnItemUseClicked -= OnItemUseClicked;
            uiController.OnDefenseClicked -= OnDefenseClicked;
            uiController.OnSpecialAttackClicked -= OnSpecialAttackClicked;
        }

        // 싱글톤 인스턴스 정리 (현재 인스턴스인 경우에만)
        if (m_Instance == this)
        {
            m_Instance = null;
        }
    }
}
