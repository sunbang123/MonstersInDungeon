using System;
using UnityEngine;

public class Enemy : Unit
{
    [Header("Health")]
    public float enemyHp = 300f;
    public float maxHp = 300f;

    [Header("Mana")]
    [SerializeField] private float _enemyPp = 50f;
    public float maxPp = 50f;

    [Header("Level & Experience")]
    public int level = 1;
    [Tooltip("이 적을 물리치면 얻는 경험치")]
    public float expReward = 50f;

    [Header("Portrait")]
    public Sprite portrait;

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
    public event Action OnEnemyDeath;

    [Header("Item Drop")]
    [Tooltip("이 적이 죽었을 때 드롭될 아이템의 프리팹")]
    public GameObject dropItemPrefab;
    [Tooltip("아이템 드롭 확률 (0 ~ 1)")]
    [Range(0f, 1f)]
    public float dropChance = 1f;

    private Player targetPlayer;
    private bool battleStarted = false;

    public void SetTarget(Player player)
    {
        targetPlayer = player;
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
    protected override void SetCurrentHp(float value) => enemyHp = value;
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
        if (other.CompareTag("Player") && !battleStarted)
        {
            battleStarted = true;
            BattleManager.Instance.StartBattle(this);
        }
    }
}