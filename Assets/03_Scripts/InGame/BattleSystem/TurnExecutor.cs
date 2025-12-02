using System.Collections;
using UnityEngine;

/// <summary>
/// 턴 실행 로직을 담당하는 클래스
/// </summary>
public class TurnExecutor : MonoBehaviour, ITurnExecutor
{
    private BattleStateMachine stateMachine;
    private BattleUIController uiController;
    private Player player;
    private Enemy enemy;
    public void Initialize(BattleStateMachine sm, BattleUIController ui)
    {
        stateMachine = sm;
        uiController = ui;
    }

    /// <summary>
    /// 전투 참여자 설정 (ITurnExecutor 구현)
    /// </summary>
    public void SetCombatants(Player p, Enemy e)
    {
        player = p;
        enemy = e;
    }

    /// <summary>
    /// 플레이어 턴 실행
    /// </summary>
    public IEnumerator ExecutePlayerTurn()
    {
        // 버튼 활성화
        uiController.SetButtonsInteractable(true);

        // 플레이어의 입력을 기다림 (상태가 변경될 때까지)
        yield return new WaitUntil(() => stateMachine.BattleState != BattleState.PlayerTurn);
    }

    /// <summary>
    /// 적 턴 실행
    /// </summary>
    public IEnumerator ExecuteEnemyTurn()
    {
        // 버튼 비활성화
        uiController.SetButtonsInteractable(false);

        yield return new WaitForSeconds(1f);

        stateMachine.ChangeState(EnemyState.Attack);
        uiController.AppendBattleLog($"적은 {stateMachine.EnemyState}했다!\n");

        yield return new WaitForSeconds(1f);

        if (player != null)
        {
            float enemyAttackDamage = 20f; // 임시 데미지
            player.TakeDamage(enemyAttackDamage);
        }

        stateMachine.ChangeState(PlayerState.Damaged);
        uiController.AppendBattleLog($"당신은 {stateMachine.PlayerState} 되었다.\n");

        yield return new WaitForSeconds(1f);

        stateMachine.ChangeState(BattleState.PlayerTurn);
    }

    /// <summary>
    /// 공격 실행
    /// </summary>
    public IEnumerator ExecuteAttack()
    {
        uiController.SetButtonsInteractable(false);

        stateMachine.ChangeState(PlayerState.Attack);
        uiController.AppendBattleLog($"당신은 {stateMachine.PlayerState}을 했다.\n");

        yield return new WaitForSeconds(1f);

        if (enemy != null)
        {
            float playerAttackDamage = 30f;
            enemy.TakeDamage(playerAttackDamage);
        }

        stateMachine.ChangeState(EnemyState.Damaged);
        uiController.AppendBattleLog($"적 {stateMachine.EnemyState}!\n");

        yield return new WaitForSeconds(1f);

        stateMachine.ChangeState(BattleState.EnemyTurn);
    }

    /// <summary>
    /// 아이템 사용 실행
    /// </summary>
    public IEnumerator ExecuteItemUse(int itemIndex)
    {
        uiController.SetButtonsInteractable(false);

        stateMachine.ChangeState(PlayerState.ItemUse);
        uiController.AppendBattleLog($"당신은 아이템 {itemIndex + 1}을 사용했다.\n");

        yield return new WaitForSeconds(1f);

        if (itemIndex == 0 && player != null) // 예: 첫 번째 아이템이 회복 아이템일 경우
        {
            // player.Heal(50f); // 실제 힐 로직
            uiController.AppendBattleLog($"플레이어 체력을 50 회복했습니다.\n");
        }
        else if (itemIndex == 1 && enemy != null) // 예: 두 번째 아이템이 공격 아이템일 경우
        {
            float itemDamage = 25f;
            enemy.TakeDamage(itemDamage); // 적에게 데미지
            uiController.AppendBattleLog($"아이템으로 적에게 {itemDamage}의 피해를 입혔습니다.\n");
        }
        yield return new WaitForSeconds(1f);

        stateMachine.ChangeState(BattleState.EnemyTurn);
    }

    /// <summary>
    /// 방어 실행
    /// </summary>
    public IEnumerator ExecuteDefense()
    {
        uiController.SetButtonsInteractable(false);

        stateMachine.ChangeState(PlayerState.Defense);
        uiController.AppendBattleLog($"당신은 방어 자세를 취했다.\n");

        yield return new WaitForSeconds(1f);

        stateMachine.ChangeState(BattleState.EnemyTurn);
    }

    /// <summary>
    /// 특수 공격 실행
    /// </summary>
    public IEnumerator ExecuteSpecialAttack()
    {
        uiController.SetButtonsInteractable(false);

        stateMachine.ChangeState(PlayerState.Attack);
        uiController.AppendBattleLog($"당신은 특수 공격을 했다!\n");

        yield return new WaitForSeconds(1f);

        if (enemy != null)
        {
            float specialAttackDamage = 50f; // 임시 데미지
            enemy.TakeDamage(specialAttackDamage);
        }

        stateMachine.ChangeState(EnemyState.Damaged);
        uiController.AppendBattleLog($"적에게 큰 피해!\n");

        yield return new WaitForSeconds(1f);

        stateMachine.ChangeState(BattleState.EnemyTurn);
    }
}