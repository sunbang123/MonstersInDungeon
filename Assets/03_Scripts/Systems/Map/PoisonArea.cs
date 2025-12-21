using System.Collections;
using UnityEngine;

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

    private Coroutine damageCoroutine = null;
    private Coroutine recoveryCoroutine = null;
    private GameObject currentPlayer = null;
    private Renderer playerRenderer;
    private Color originalPlayerColor;
    private bool isRecovering = false;
    private bool hasOriginalColor = false; // 플레이어 원본 색상 저장 여부

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
        }

        // 이미 데미지 중이 아닌 경우에만 설정
        if (damageCoroutine == null)
        {
            SetupPlayer(other.gameObject);
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
}
