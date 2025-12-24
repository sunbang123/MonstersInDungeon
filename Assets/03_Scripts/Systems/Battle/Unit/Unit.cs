using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Player와 Enemy가 공통으로 사용하는 기본 추상 클래스
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

        Debug.Log($"[데미지 받음] {gameObject.name} - HP: {currentHp}/{maxHp} (데미지: {finalDamage})");

        InvokeHealthChanged(currentHp, maxHp);

        // 쓰러짐 확인
        if (currentHp <= 0f && !isDead)
        {
            Debug.Log($"[쓰러짐] {gameObject.name}이(가) 쓰러졌습니다.");
            // 적인 경우 코루틴으로 지연 처리
            if (this is Enemy)
            {
                StartCoroutine(DelayedEnemyDeath());
            }
            else
            {
                Die();
            }
        }
    }

    /// <summary>
    /// 적의 죽음을 지연시키는 코루틴 (메시지 표시 후 죽음 처리)
    /// </summary>
    private IEnumerator DelayedEnemyDeath()
    {
        // "적에게 큰 데미지!" 같은 데미지 메시지가 먼저 표시되도록 약간의 지연
        yield return new WaitForSeconds(1f);
        
        // "적이 쓰러졌습니다" 메시지 표시 (줄바꿈 후 추가)
        BattleUIController.OnBattleLogAppended?.Invoke($"적이 쓰러졌습니다.\n");
        yield return new WaitForSeconds(1f);
        
        // 그 다음 죽음 처리
        Die();
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

    // 추상 메서드 - 자식 클래스에서 반드시 구현 필요
    protected abstract float GetCurrentHp();
    protected abstract float GetMaxHp();
    protected abstract void SetCurrentHp(float value);
    protected abstract void InvokeHealthChanged(float current, float max);
    protected abstract void InvokeDeath();
}
