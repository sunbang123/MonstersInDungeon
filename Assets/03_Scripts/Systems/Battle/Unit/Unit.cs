using System;
using UnityEngine;

/// <summary>
/// Player와 Enemy의 공통 기능을 담당하는 기본 유닛 클래스
/// </summary>
public abstract class Unit : MonoBehaviour
{
    protected bool isDead = false;

    /// <summary>
    /// 데미지를 받는 메서드
    /// </summary>
    public virtual void TakeDamage(float damage)
    {
        if (isDead) return;

        float finalDamage = Mathf.Max(1f, damage);
        float currentHp = GetCurrentHp();
        float maxHp = GetMaxHp();

        currentHp -= finalDamage;
        currentHp = Mathf.Max(0f, currentHp);
        SetCurrentHp(currentHp);

        Debug.Log($"[데미지 적용 후] {gameObject.name} - HP: {currentHp}/{maxHp} (데미지: {finalDamage})");

        InvokeHealthChanged(currentHp, maxHp);

        // 사망 체크
        if (currentHp <= 0f && !isDead)
        {
            Debug.Log($"[사망] {gameObject.name}이(가) 사망했습니다.");
            Die();
        }
    }

    /// <summary>
    /// 사망 처리
    /// </summary>
    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;
        InvokeDeath();
    }

    /// <summary>
    /// 사망 상태를 반환하는 메서드
    /// </summary>
    public bool IsDead()
    {
        return isDead;
    }

    // 추상 메서드 - 자식 클래스에서 구현 필요
    protected abstract float GetCurrentHp();
    protected abstract float GetMaxHp();
    protected abstract void SetCurrentHp(float value);
    protected abstract void InvokeHealthChanged(float current, float max);
    protected abstract void InvokeDeath();
}