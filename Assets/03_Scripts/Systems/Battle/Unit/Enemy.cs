using System;
using UnityEngine;

public class Enemy : Unit
{
    [Header("Health")]
    [SerializeField] private float _enemyHp = GameConstants.Enemy.DEFAULT_MAX_HP;
    public float maxHp = GameConstants.Enemy.DEFAULT_MAX_HP;

    public float enemyHp
    {
        get => _enemyHp;
        set
        {
            _enemyHp = Mathf.Clamp(value, 0f, maxHp);
            OnHealthChanged?.Invoke(_enemyHp, maxHp);
        }
    }

    [Header("Mana")]
    [SerializeField] private float _enemyPp = GameConstants.Enemy.DEFAULT_MAX_PP;
    public float maxPp = GameConstants.Enemy.DEFAULT_MAX_PP;

    [Header("Level & Experience")]
    [SerializeField] private int _level = GameConstants.Enemy.DEFAULT_LEVEL;
    [Tooltip("이 적을 물리치면 얻는 경험치")]
    public float expReward = GameConstants.Enemy.DEFAULT_EXP_REWARD;

    public int level
    {
        get => _level;
        set
        {
            _level = Mathf.Max(1, value);
            OnLevelChanged?.Invoke(_level);
        }
    }

    [Header("Portrait")]
    [SerializeField] private Sprite _portrait;
    public Sprite portrait
    {
        get => _portrait;
        set
        {
            _portrait = value;
            OnPortraitChanged?.Invoke(_portrait);
        }
    }

    public float enemyPp
    {
        get => _enemyPp;
        set
        {
            _enemyPp = Mathf.Clamp(value, 0f, maxPp);
            OnPPChanged?.Invoke(_enemyPp, maxPp);
        }
    }

    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnPPChanged;
    public event Action<int> OnLevelChanged;
    public event Action<Sprite> OnPortraitChanged;
    public event Action OnEnemyDeath;

    [Header("Item Drop")]
    [Tooltip("이 적이 죽었을 때 드롭될 아이템의 프리팹")]
    public GameObject dropItemPrefab;
    [Tooltip("아이템 드롭 확률 (0 ~ 1)")]
    [Range(0f, 1f)]
    public float dropChance = GameConstants.Enemy.DEFAULT_DROP_CHANCE;

    private bool battleStarted = false;

    public void SetTarget(Player player)
    {
        // 타겟 설정 (현재는 사용되지 않지만 향후 확장을 위해 유지)
    }

    /// <summary>
    /// 적의 죽음 상태를 반환하는 메서드
    /// </summary>
    public new bool IsDead()
    {
        return isDead;
    }

    // Unit 클래스의 추상 메서드 구현
    protected override float GetCurrentHp() => enemyHp;
    protected override float GetMaxHp() => maxHp;
    protected override void SetCurrentHp(float value)
    {
        enemyHp = value; // 속성을 통해 이벤트 자동 발생
    }
    protected override void InvokeHealthChanged(float current, float max)
        => OnHealthChanged?.Invoke(current, max);
    protected override void InvokeDeath() => OnEnemyDeath?.Invoke();

    protected override void Die()
    {
        base.Die();

        // 아이템 드롭
        DropItem();
    }

    /// <summary>
    /// 아이템 드롭 처리
    /// </summary>
    private void DropItem()
    {
        if (dropItemPrefab == null) return;

        // 드롭 확률 확인
        float randomValue = UnityEngine.Random.Range(0f, 1f);
        if (randomValue <= dropChance)
        {
            // 현재 위치에 아이템 생성
            GameObject droppedItem = Instantiate(dropItemPrefab, transform.position, Quaternion.identity);
            Debug.Log($"[아이템 드롭] {gameObject.name}이(가) {dropItemPrefab.name}을(를) 드롭했습니다.");
        }
        else
        {
            Debug.Log($"[아이템 드롭 실패] {gameObject.name} - 확률: {dropChance * 100}%");
        }
    }

    // 플레이어와 충돌 시 전투 시작
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(GameConstants.TAG_PLAYER) && !battleStarted)
        {
            battleStarted = true;
            
            // BattleStarter 또는 BattleManager 찾기
            BattleStarter battleStarter = FindObjectOfType<BattleStarter>();
            if (battleStarter != null)
            {
                // InGame 씬: BattleStarter 사용 (씬 전환만)
                battleStarter.StartBattle(this);
            }
            else if (BattleManager.Instance != null)
            {
                // InBattle 씬: BattleManager 사용 (실제 전투)
                BattleManager.Instance.StartBattle(this);
            }
            else
            {
                Logger.LogWarning("BattleStarter 또는 BattleManager를 찾을 수 없습니다.");
            }
        }
    }
}