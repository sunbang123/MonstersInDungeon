using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PoisonArea : MonoBehaviour
{
    [Header("Poison Settings")]
    public float poisonDmg = 10.0f;
    public float damageInterval = 1.0f;

    [Header("ScreenFlash Settings")]
    public ScreenFlash screenFlash;
    public Color flashColor = new Color(0.5f, 0f, 1f, 0.3f);
    public float flashDuration = 0.2f;

    [Header("Player Color Settings")]
    public Color poisonColor = new Color(0.7f, 0.2f, 1f, 1f);

    [Header("Recovery Settings")]
    public float recoveryDuration = 2.0f;

    [Header("Effect Fade Settings")]
    [Tooltip("이펙트가 파괴되기 전 페이드 아웃 시간 (초)")]
    public float effectFadeDuration = 0.3f;

    private Coroutine damageCoroutine = null;
    private Coroutine recoveryCoroutine = null;
    private GameObject currentPlayer = null;
    private Renderer playerRenderer;
    private Color originalPlayerColor;
    private bool isRecovering = false;
    private bool hasOriginalColor = false; // 플레이어 원본 색상 저장 여부
    private bool hasPoisonEffectSpawned = false; // poisonEffect가 이미 생성되었는지 확인
    private GameObject currentPoisonEffect = null; // 현재 생성된 poisonEffect 인스턴스
    private Coroutine destroyPoisonEffectCoroutine = null; // poisonEffect 파괴 코루틴

    void Start()
    {
        ValidateTriggerCollider();
        EnsureScreenFlash();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsValidPlayer(other))
            return;

        // 회복 중이었다면 회복 중지
        if (isRecovering)
        {
            StopRecovery();
            // 회복이 중단되었지만, 색상이 이미 복원되었다면 플래그 초기화
            if (playerRenderer != null && hasOriginalColor)
            {
                Color currentColor = playerRenderer.material.color;
                // 색상이 원본 색상과 비슷하면 복원된 것으로 간주
                if (ColorDistance(currentColor, originalPlayerColor) < 0.1f)
                {
                    hasPoisonEffectSpawned = false;
                }
            }
        }

        // 이미 데미지 중이 아닌 경우에만 설정
        if (damageCoroutine == null)
        {
            SetupPlayer(other.gameObject);
            // 독 영역에 처음 진입했을 때 poisonEffect 생성
            if (!hasPoisonEffectSpawned)
            {
                StartCoroutine(LoadAndSpawnPoisonEffect(other.transform));
                hasPoisonEffectSpawned = true;
            }
            else if (currentPoisonEffect != null)
            {
                // 독 상태에서 또 밟았을 때, 기존 poisonEffect의 파괴를 늦춤
                ExtendPoisonEffectLifetime();
            }
        }

        damageCoroutine = StartCoroutine(ApplyPoisonDamage());
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!IsValidPlayer(other) || other.gameObject != currentPlayer)
            return;

        // 데미지 코루틴 중지
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }

        // 회복 시작
        recoveryCoroutine = StartCoroutine(RecoveryProcess());
    }

    IEnumerator ApplyPoisonDamage()
    {
        while (currentPlayer != null && !isRecovering)
        {
            ApplyDamageToPlayer(currentPlayer, poisonDmg, 1.0f);
            yield return new WaitForSeconds(damageInterval);
        }
    }

    IEnumerator RecoveryProcess()
    {
        isRecovering = true;
        float elapsed = 0f;

        while (elapsed < recoveryDuration)
        {
            elapsed += damageInterval;
            float recoveryProgress = elapsed / recoveryDuration;
            float damageMultiplier = 1f - recoveryProgress;

            // 회복 진행도에 따라 데미지 비율을 줄여나가는 데미지 적용
            if (damageMultiplier > 0)
            {
                ApplyDamageToPlayer(currentPlayer, poisonDmg, damageMultiplier);
            }

            // 플레이어 색상을 점진적으로 복원
            if (playerRenderer != null)
            {
                playerRenderer.material.color = Color.Lerp(poisonColor, originalPlayerColor, recoveryProgress);
            }

            yield return new WaitForSeconds(damageInterval);
        }

        // 상태 복원
        RestorePlayerState();
        
        // 플레이어 색상이 복원된 후에만 poisonEffect 재생성 가능하도록 초기화
        hasPoisonEffectSpawned = false;
        
        CleanupPoisonState();
    }

    void ApplyDamageToPlayer(GameObject playerObj, float damage, float multiplier)
    {
        Player player = playerObj.GetComponent<Player>();
        if (player != null)
        {
            float actualDamage = damage * multiplier;
            player.TakeDamage(actualDamage);
            TriggerScreenFlash(multiplier);
            
            // 독 상태에서 계속 밟고 있을 때도 poisonEffect의 파괴를 연장
            if (currentPoisonEffect != null)
            {
                ExtendPoisonEffectLifetime();
            }
        }
    }

    // === 유틸리티 함수들 ===

    bool IsValidPlayer(Collider2D collider)
    {
        return collider.CompareTag("Player");
    }

    void SetupPlayer(GameObject player)
    {
        currentPlayer = player;
        playerRenderer = currentPlayer.GetComponent<Renderer>();

        if (playerRenderer != null)
        {
            // 처음 진입했거나 색상이 저장되지 않은 경우에만 저장
            if (!hasOriginalColor)
            {
                originalPlayerColor = playerRenderer.material.color;
                hasOriginalColor = true;
            }
            playerRenderer.material.color = poisonColor;
        }
    }

    void RestorePlayerState()
    {
        if (playerRenderer != null)
        {
            playerRenderer.material.color = originalPlayerColor;
        }
    }

    void CleanupPoisonState()
    {
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
        }
        if (recoveryCoroutine != null)
        {
            StopCoroutine(recoveryCoroutine);
        }

        damageCoroutine = null;
        recoveryCoroutine = null;
        currentPlayer = null;
        playerRenderer = null;
        isRecovering = false;
        hasOriginalColor = false; // 플레이어가 나가면 다시 저장하도록 초기화
        
        // poisonEffect 관련 정리
        if (destroyPoisonEffectCoroutine != null)
        {
            StopCoroutine(destroyPoisonEffectCoroutine);
            destroyPoisonEffectCoroutine = null;
        }
        currentPoisonEffect = null;
        // hasPoisonEffectSpawned는 RestorePlayerState() 직후에만 초기화됨
    }

    /// <summary>
    /// 씬 전환 시 독 상태를 정리하는 public 메서드
    /// </summary>
    public void ForceCleanup()
    {
        // 플레이어 상태 복원
        if (playerRenderer != null && hasOriginalColor)
        {
            RestorePlayerState();
        }
        
        // 모든 코루틴 중지 및 상태 초기화
        CleanupPoisonState();
        
        Logger.Log($"PoisonArea 강제 정리 완료: {gameObject.name}");
    }

    void StopRecovery()
    {
        if (recoveryCoroutine != null)
        {
            StopCoroutine(recoveryCoroutine);
            recoveryCoroutine = null;
        }
        isRecovering = false;
    }

    void ValidateTriggerCollider()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning($"PoisonArea: {gameObject.name}의 Collider2D가 Trigger로 설정되어 있지 않습니다.");
        }
    }

    void EnsureScreenFlash()
    {
        if (screenFlash == null)
        {
            screenFlash = FindObjectOfType<ScreenFlash>();
        }
    }

    void TriggerScreenFlash(float intensity)
    {
        if (screenFlash != null)
        {
            Color adjustedColor = new Color(
                flashColor.r,
                flashColor.g,
                flashColor.b,
                flashColor.a * intensity
            );
            screenFlash.FlashWithColor(adjustedColor, flashDuration);
        }
    }

    /// <summary>
    /// Addressable에서 poisonEffect 프리팹을 로드하고 플레이어 위치에 생성
    /// </summary>
    private IEnumerator LoadAndSpawnPoisonEffect(Transform playerTransform)
    {
        GameObject effectPrefab = null;

        // 방법 1: GameManager에서 이미 로드된 프리팹 확인 (preload로 이미 로드되어 있을 수 있음, 경고 없이)
        if (GameManager.Instance != null)
        {
            effectPrefab = GameManager.Instance.TryGetPrefabByName("poisonEffect");
        }

        // 방법 2: GameManager에 없으면 Addressable에서 직접 로드
        if (effectPrefab == null)
        {
            // Addressable Address 시도: 먼저 짧은 주소로 시도
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>("poisonEffect.prefab");

            // 로드 완료 대기
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
            {
                effectPrefab = handle.Result;
                Debug.Log($"[이펙트 로드 성공] poisonEffect.prefab을 Addressable에서 로드했습니다.");
            }
            else
            {
                // 짧은 주소로 실패하면 전체 경로로 재시도
                handle = Addressables.LoadAssetAsync<GameObject>("Assets/05_Prefabs/Effect/poisonEffect.prefab");
                yield return handle;

                if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
                {
                    effectPrefab = handle.Result;
                    Debug.Log($"[이펙트 로드 성공] poisonEffect.prefab을 전체 경로로 Addressable에서 로드했습니다.");
                }
                else
                {
                    Debug.LogWarning($"[이펙트 로드 실패] poisonEffect.prefab을 찾을 수 없습니다. Addressable에 등록되어 있고 빌드되었는지 확인하세요.");
                    yield break;
                }
            }
        }
        else
        {
            Debug.Log($"[이펙트 로드 성공] poisonEffect를 GameManager에서 찾았습니다.");
        }

        // 플레이어의 자식으로 이펙트 생성 (battleEffect와 동일한 방식)
        if (effectPrefab != null && playerTransform != null)
        {
            GameObject effectInstance = Instantiate(effectPrefab, playerTransform);
            effectInstance.transform.localPosition = Vector3.zero;
            effectInstance.transform.localRotation = Quaternion.identity;
            
            // 현재 poisonEffect 인스턴스 저장
            currentPoisonEffect = effectInstance;
            
            Debug.Log($"[이펙트 생성] {playerTransform.name}에 poisonEffect 이펙트가 생성되었습니다.");
            
            // 애니메이션 길이 확인하여 자동 제거
            float animationDuration = GetAnimationDuration(effectInstance);
            StartDestroyPoisonEffectCoroutine(effectInstance, animationDuration > 0f ? animationDuration : 0.5f);
        }
    }

    /// <summary>
    /// 두 색상 간의 거리를 계산 (0~1 범위)
    /// </summary>
    private float ColorDistance(Color a, Color b)
    {
        float rDiff = a.r - b.r;
        float gDiff = a.g - b.g;
        float bDiff = a.b - b.b;
        float aDiff = a.a - b.a;
        return Mathf.Sqrt(rDiff * rDiff + gDiff * gDiff + bDiff * bDiff + aDiff * aDiff);
    }

    /// <summary>
    /// 이펙트의 애니메이션 길이를 확인 (Animator 또는 Animation 컴포넌트에서)
    /// </summary>
    private float GetAnimationDuration(GameObject effectInstance)
    {
        // Animator 컴포넌트 확인
        Animator animator = effectInstance.GetComponent<Animator>();
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            if (clips != null && clips.Length > 0)
            {
                // 첫 번째 클립의 길이 반환
                return clips[0].length;
            }
        }

        // Animation 컴포넌트 확인 (Legacy Animation)
        Animation animation = effectInstance.GetComponent<Animation>();
        if (animation != null && animation.clip != null)
        {
            return animation.clip.length;
        }

        return 0f; // 애니메이션을 찾을 수 없음
    }

    /// <summary>
    /// poisonEffect의 파괴 코루틴 시작
    /// </summary>
    private void StartDestroyPoisonEffectCoroutine(GameObject effect, float delay)
    {
        // 기존 파괴 코루틴이 있다면 중지
        if (destroyPoisonEffectCoroutine != null)
        {
            StopCoroutine(destroyPoisonEffectCoroutine);
        }
        
        // 새로운 파괴 코루틴 시작
        destroyPoisonEffectCoroutine = StartCoroutine(DestroyEffectAfterDelay(effect, delay));
    }

    /// <summary>
    /// 독 상태에서 또 밟았을 때, 기존 poisonEffect의 파괴를 늦춤
    /// </summary>
    private void ExtendPoisonEffectLifetime()
    {
        if (currentPoisonEffect != null)
        {
            // 애니메이션 길이 확인
            float animationDuration = GetAnimationDuration(currentPoisonEffect);
            float delay = animationDuration > 0f ? animationDuration : 0.5f;
            
            // 파괴 코루틴 재시작 (기존 것을 중지하고 새로 시작)
            StartDestroyPoisonEffectCoroutine(currentPoisonEffect, delay);
            
            Debug.Log($"[이펙트 파괴 연장] poisonEffect의 파괴가 {delay}초로 연장되었습니다.");
        }
    }

    /// <summary>
    /// 일정 시간 후 이펙트를 제거하는 코루틴 (페이드 아웃 효과 포함)
    /// </summary>
    private IEnumerator DestroyEffectAfterDelay(GameObject effect, float delay)
    {
        // 페이드 아웃 시작 전까지 대기
        float fadeStartTime = delay - effectFadeDuration;
        if (fadeStartTime > 0f)
        {
            yield return new WaitForSeconds(fadeStartTime);
        }

        // 페이드 아웃 효과
        if (effect != null)
        {
            SpriteRenderer spriteRenderer = effect.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Color originalColor = spriteRenderer.color;
                float elapsed = 0f;
                
                while (elapsed < effectFadeDuration && effect != null)
                {
                    elapsed += Time.deltaTime;
                    float alpha = Mathf.Lerp(1f, 0f, elapsed / effectFadeDuration);
                    spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                    yield return null;
                }
            }
            else
            {
                // SpriteRenderer가 없으면 바로 파괴하지 않고 페이드 시간만큼 대기
                yield return new WaitForSeconds(effectFadeDuration);
            }
        }

        // 이펙트 파괴
        if (effect != null)
        {
            Destroy(effect);
            // 현재 poisonEffect 인스턴스가 파괴되면 null로 설정
            if (effect == currentPoisonEffect)
            {
                currentPoisonEffect = null;
                destroyPoisonEffectCoroutine = null;
            }
        }
    }
}
