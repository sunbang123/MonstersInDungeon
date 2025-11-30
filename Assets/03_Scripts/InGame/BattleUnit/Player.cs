using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : MonoBehaviour
{
    [Header("Health & Mana")]
    public float playerHp = 500f;

    public float maxHp = 500f;

    public float playerMp = 100f;

    public float maxMp = 100f;

    [Tooltip("다음 레벨까지 필요한 경험치")]
    public float expToNextLevel = 100f;

    [Header("Skills")]
    public GameObject[] playerSkills;

    public event Action OnPlayerDeath;

    private bool isDead = false;
    private Coroutine regenCoroutine;

    void Start()
    {
        // 초기값 설정
        playerHp = Mathf.Clamp(playerHp, 0f, maxHp);
        playerMp = Mathf.Clamp(playerMp, 0f, maxMp);
    }

    void Update()
    {
        // HP/MP가 최대값을 초과하지 않도록 제한
        playerHp = Mathf.Clamp(playerHp, 0f, maxHp);
        playerMp = Mathf.Clamp(playerMp, 0f, maxMp);
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        float finalDamage = Mathf.Max(1f, damage);

        playerHp -= finalDamage;
        playerHp = Mathf.Max(0f, playerHp);

        Debug.Log($"[데미지 적용 후] {gameObject.name} - HP: {playerHp}/{maxHp} (데미지: {finalDamage})");

        // 사망 체크
        if (playerHp <= 0f && !isDead)
        {
            Debug.Log($"[사망] {gameObject.name}이(가) 사망했습니다.");
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        OnPlayerDeath?.Invoke();
    }

    public bool IsDead()
    {
        return isDead;
    }

    void OnDestroy()
    {
        // 코루틴 정리
        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
        }
    }
}