using System;
using UnityEngine;

public class Enemy : Unit
{
    [Header("Health")]
    public float enemyHp = 300f;
    public float maxHp = 300f;

    public event Action<float, float> OnHealthChanged;
    public event Action OnEnemyDeath;

    [Header("Item Drop")]
    [Tooltip("적이 죽었을 때 드랍할 아이템 프리팹")]
    public GameObject dropItemPrefab;
    [Tooltip("아이템 드랍 확률 (0 ~ 1)")]
    [Range(0f, 1f)]
    public float dropChance = 1f;

    private Player targetPlayer;
    private bool battleStarted = false;

    public void SetTarget(Player player)
    {
        targetPlayer = player;
    }

    /// <summary>
    /// 적의 사망 상태를 반환하는 메서드
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

        // 아이템 드랍
        DropItem();
    }

    /// <summary>
    /// 아이템 드랍 처리
    /// </summary>
    private void DropItem()
    {
        if (dropItemPrefab == null) return;

        // 드랍 확률 체크
        float randomValue = UnityEngine.Random.Range(0f, 1f);
        if (randomValue <= dropChance)
        {
            // 적이 있던 위치에 아이템 생성
            GameObject droppedItem = Instantiate(dropItemPrefab, transform.position, Quaternion.identity);
            Debug.Log($"[아이템 드랍] {gameObject.name}이(가) {dropItemPrefab.name}을(를) 드랍했습니다.");
        }
        else
        {
            Debug.Log($"[아이템 드랍 실패] {gameObject.name} - 확률: {dropChance * 100}%");
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