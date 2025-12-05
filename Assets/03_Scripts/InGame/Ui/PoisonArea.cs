using System.Collections;
using UnityEngine;

public class PoisonArea : MonoBehaviour
{
    [Header("Poison Settings")]
    // 초당 독 데미지
    public float poisonDmg = 10.0f;
    // 데미지 적용 간격
    public float damageInterval = 1.0f;

    [Header("ScreenFlash Settings")]
    public ScreenFlash screenFlash;
    // 독 데미지용 깜빡임 색상 (보라색)
    public Color flashColor = new Color(0.5f, 0f, 1f, 0.3f);
    // 깜빡임 지속 시간
    public float flashDuration = 0.2f;

    // 현재 독 데미지를 받고 있는 플레이어의 Coroutine
    private Coroutine damageCoroutine = null;
    // 현재 독 구역에 들어와 있는 GameObject
    private GameObject currentPlayer = null;

    void Start()
    {
        // Collider2D가 Trigger로 설정되어 있는지 확인
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning($"PoisonArea: {gameObject.name}의 Collider2D가 Trigger로 설정되지 않았습니다.");
        }

        // ScreenFlash가 없으면 씬에서 찾기
        if (screenFlash == null)
        {
            screenFlash = FindObjectOfType<ScreenFlash>();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // "Player" 태그가 아니면 무시
        if (!other.CompareTag("Player"))
            return;

        // 이미 플레이어가 독 상태이면 무시 (단일 플레이어 가정)
        if (damageCoroutine != null)
            return;

        currentPlayer = other.gameObject;

        // 독 데미지 코루틴 시작
        damageCoroutine = StartCoroutine(ApplyPoisonDamage(currentPlayer));
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // "Player" 태그가 아니면 무시
        if (!other.CompareTag("Player"))
            return;

        // 현재 독 상태인 플레이어가 나가면
        if (other.gameObject == currentPlayer)
        {
            // 데미지 코루틴 중지 및 초기화
            if (damageCoroutine != null)
            {
                StopCoroutine(damageCoroutine);
            }
            damageCoroutine = null;
            currentPlayer = null;
        }
    }

    IEnumerator ApplyPoisonDamage(GameObject player)
    {
        // 플레이어가 존재하는 동안
        while (player != null)
        {
            // 플레이어에게 데미지 적용
            ApplyDamageToPlayer(player, poisonDmg);

            // 데미지 간격만큼 대기
            yield return new WaitForSeconds(damageInterval);
        }

        // 플레이어가 파괴되면 Coroutine을 중지하고 정리
        damageCoroutine = null;
        currentPlayer = null;
    }

    void ApplyDamageToPlayer(GameObject p_go, float damage)
    {
        // Player 컴포넌트 찾기
        Player player = p_go.GetComponent<Player>();
        if (player != null)
        {
            // 플레이어에게 데미지 적용
            player.TakeDamage(damage);

            // 스크린 플래쉬 적용
            if (screenFlash != null)
            {
                screenFlash.FlashWithColor(flashColor, flashDuration);
            }
        }
    }
}