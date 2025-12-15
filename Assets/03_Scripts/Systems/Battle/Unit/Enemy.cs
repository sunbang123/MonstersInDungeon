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
    [Tooltip("���� �׾��� �� ����� ������ ������")]
    public GameObject dropItemPrefab;
    [Tooltip("������ ��� Ȯ�� (0 ~ 1)")]
    [Range(0f, 1f)]
    public float dropChance = 1f;

    private Player targetPlayer;
    private bool battleStarted = false;

    public void SetTarget(Player player)
    {
        targetPlayer = player;
    }

    /// <summary>
    /// ���� ��� ���¸� ��ȯ�ϴ� �޼���
    /// </summary>
    public new bool IsDead()
    {
        return isDead;
    }

    // Unit Ŭ������ �߻� �޼��� ����
    protected override float GetCurrentHp() => enemyHp;
    protected override float GetMaxHp() => maxHp;
    protected override void SetCurrentHp(float value) => enemyHp = value;
    protected override void InvokeHealthChanged(float current, float max)
        => OnHealthChanged?.Invoke(current, max);
    protected override void InvokeDeath() => OnEnemyDeath?.Invoke();

    protected override void Die()
    {
        base.Die();

        // ������ ���
        DropItem();
    }

    /// <summary>
    /// ������ ��� ó��
    /// </summary>
    private void DropItem()
    {
        if (dropItemPrefab == null) return;

        // ��� Ȯ�� üũ
        float randomValue = UnityEngine.Random.Range(0f, 1f);
        if (randomValue <= dropChance)
        {
            // ���� �ִ� ��ġ�� ������ ����
            GameObject droppedItem = Instantiate(dropItemPrefab, transform.position, Quaternion.identity);
            Debug.Log($"[������ ���] {gameObject.name}��(��) {dropItemPrefab.name}��(��) ����߽��ϴ�.");
        }
        else
        {
            Debug.Log($"[������ ��� ����] {gameObject.name} - Ȯ��: {dropChance * 100}%");
        }
    }

    // �÷��̾�� �浹 �� ���� ����
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !battleStarted)
        {
            battleStarted = true;
            BattleManager.Instance.StartBattle(this);
        }
    }
}