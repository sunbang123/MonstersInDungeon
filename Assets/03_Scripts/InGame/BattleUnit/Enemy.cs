using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Health")]
    public float enemyHp = 300f;
    public float maxHp = 300f;
    public event Action<float, float> OnHealthChanged;
    public event Action OnEnemyDeath;

    private Player targetPlayer;
    private bool battleStarted = false;
    private bool isDead = false;

    public void SetTarget(Player player)
    {
        targetPlayer = player;
    }
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        float finalDamage = Mathf.Max(1f, damage);

        enemyHp -= finalDamage;
        enemyHp = Mathf.Max(0f, enemyHp);

        Debug.Log($"[데미지 적용 후] {gameObject.name} - HP: {enemyHp}/{maxHp} (데미지: {finalDamage})");

        OnHealthChanged?.Invoke(enemyHp, maxHp);

        // 사망 체크
        if (enemyHp <= 0f && !isDead)
        {
            Debug.Log($"[사망] {gameObject.name}이(가) 사망했습니다.");
            Die();
        }
    }

    /// <summary>
    /// 적의 사망 상태를 반환하는 메서드
    /// </summary>
    public bool IsDead()
    {
        return isDead;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        OnEnemyDeath?.Invoke();
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