using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Player targetPlayer;
    private bool battleStarted = false;
    public void SetTarget(Player player)
    {
        targetPlayer = player;
    }

    // 플레이어와 충돌 시 전투 시작
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !battleStarted)
        {
            battleStarted = true;
            BattleManager.Instance.StartBattle();
        }
    }
}