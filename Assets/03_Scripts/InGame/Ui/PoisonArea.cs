using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonArea : MonoBehaviour
{
    [Header("Poison Settings")]
    public float poisonDmg = 10.0f;

    public float damageInterval = 1.0f;

    [Header("Visual Settings")]
    public Color poisonColor = new Color(0.5f, 0.0f, 0.5f, 1.0f); // 보라색

    // 현재 중독 상태인 플레이어들
    private Dictionary<GameObject, Coroutine> poisonedPlayers = new Dictionary<GameObject, Coroutine>();

    // 플레이어의 원래 색상 저장
    private Dictionary<GameObject, Color> originalColors = new Dictionary<GameObject, Color>();

    public Player player;

    void Start()
    {
        // Collider2D가 Trigger로 설정되어 있는지 확인
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning($"PoisonArea: {gameObject.name}의 Collider2D가 Trigger로 설정되지 않았습니다.");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        GameObject player = other.gameObject;

        if (!poisonedPlayers.ContainsKey(player))
        {
            SaveOriginalColor(player);
            ChangePlayerColor(player, poisonColor);

            Coroutine damageCoroutine = StartCoroutine(ApplyPoisonDamage(player));
            poisonedPlayers[player] = damageCoroutine;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        GameObject player = other.gameObject;

        // 중독 상태 해제
        if (poisonedPlayers.ContainsKey(player))
        {
            // 데미지 코루틴 중지
            if (poisonedPlayers[player] != null)
            {
                StopCoroutine(poisonedPlayers[player]);
            }
            poisonedPlayers.Remove(player);

            // 플레이어 색상을 원래대로 복원
            RestoreOriginalColor(player);
        }
    }

    IEnumerator ApplyPoisonDamage(GameObject player)
    {
        while (player != null)
        {
            // 플레이어에게 데미지 적용
            ApplyDamageToPlayer(player, poisonDmg);            

            // 대기 시간
            yield return new WaitForSeconds(damageInterval);
        }
    }

    void ApplyDamageToPlayer(GameObject p_go, float damage)
    {
        Player player = p_go.GetComponent<Player>();
        if (player != null)
        {
            player.TakeDamage(damage);
            return;
        }
    }

    void SaveOriginalColor(GameObject player)
    {
        // SpriteRenderer에서 원래 색상 저장
        SpriteRenderer spriteRenderer = player.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColors[player] = spriteRenderer.color;
            return;
        }

        // 기본 색상으로 설정 (흰색)
        originalColors[player] = Color.white;
    }

    void ChangePlayerColor(GameObject player, Color color)
    {
        // SpriteRenderer 색상 변경
        SpriteRenderer spriteRenderer = player.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
            return;
        }
    }

    void RestoreOriginalColor(GameObject player)
    {
        if (!originalColors.ContainsKey(player))
            return;

        Color originalColor = originalColors[player];

        // SpriteRenderer 색상 복원
        SpriteRenderer spriteRenderer = player.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
            originalColors.Remove(player);
            return;
        }
    }

    void OnDestroy()
    {
        // 오브젝트가 파괴될 때 모든 플레이어의 색상 복원
        foreach (var player in new List<GameObject>(poisonedPlayers.Keys))
        {
            if (player != null)
            {
                RestoreOriginalColor(player);
            }
        }
        poisonedPlayers.Clear();
        originalColors.Clear();
    }
}