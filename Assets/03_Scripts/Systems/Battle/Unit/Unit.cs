using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

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

        // 공격받는 이펙트 생성
        StartCoroutine(LoadAndSpawnBattleEffect());

        // 쓰러짐 확인
        if (currentHp <= 0f && !isDead)
        {
            Debug.Log($"[쓰러짐] {gameObject.name}이(가) 쓰러졌습니다.");
            Die();
        }
    }

    /// <summary>
    /// Addressable에서 battleEffect 프리팹을 로드하고 공격받은 대상의 자식으로 생성
    /// </summary>
    private IEnumerator LoadAndSpawnBattleEffect()
    {
        GameObject effectPrefab = null;

        // 방법 1: GameManager에서 이미 로드된 프리팹 확인 (preload로 이미 로드되어 있을 수 있음, 경고 없이)
        if (GameManager.Instance != null)
        {
            effectPrefab = GameManager.Instance.TryGetPrefabByName("battleEffect");
        }

        // 방법 2: GameManager에 없으면 Addressable에서 직접 로드
        if (effectPrefab == null)
        {
            // Addressable Address는 "battleEffect.prefab"으로 설정되어 있음
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>("battleEffect.prefab");

            // 로드 완료 대기
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
            {
                effectPrefab = handle.Result;
                Debug.Log($"[이펙트 로드 성공] battleEffect.prefab을 Addressable에서 로드했습니다.");
            }
            else
            {
                Debug.LogWarning($"[이펙트 로드 실패] battleEffect.prefab을 찾을 수 없습니다. Addressable에 등록되어 있고 빌드되었는지 확인하세요.");
                yield break;
            }
        }
        else
        {
            Debug.Log($"[이펙트 로드 성공] battleEffect를 GameManager에서 찾았습니다.");
        }

        // 공격받은 대상의 자식으로 이펙트 생성
        if (effectPrefab != null)
        {
            GameObject effectInstance = Instantiate(effectPrefab, transform);
            effectInstance.transform.localPosition = Vector3.zero;
            effectInstance.transform.localRotation = Quaternion.identity;

            Debug.Log($"[이펙트 생성] {gameObject.name}에 battleEffect 이펙트가 생성되었습니다.");
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

    // 추상 메서드 - 자식 클래스에서 반드시 구현 필요
    protected abstract float GetCurrentHp();
    protected abstract float GetMaxHp();
    protected abstract void SetCurrentHp(float value);
    protected abstract void InvokeHealthChanged(float current, float max);
    protected abstract void InvokeDeath();
}
