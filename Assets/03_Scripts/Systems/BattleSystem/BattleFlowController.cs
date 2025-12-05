using System.Collections;
using UnityEngine;
using System;

/// <summary>
/// 전투 흐름을 제어하는 클래스
/// </summary>
public class BattleFlowController : MonoBehaviour
{
    private BattleStateMachine stateMachine;
    private BattleUIController uiController;
    private ITurnExecutor turnExecutor;

    private PlayerController playerController;
    private Player player;
    private Enemy enemy;

    public void Initialize(BattleStateMachine sm, BattleUIController ui, TurnExecutor te)
    {
        stateMachine = sm;
        uiController = ui;
        turnExecutor = te;
    }

    public void SetPlayerReferences(Player p, PlayerController pc)
    {
        player = p;
        playerController = pc;
    }

    /// <summary>
    /// 전투 시작
    /// </summary>
    public IEnumerator StartBattle(Enemy e)
    {
        enemy = e;
        e.SetTarget(player);

        turnExecutor.SetCombatants(player, enemy);

        uiController.ShowBattleUI();
        playerController.SetMovementMode(MovementMode.Stop);

        player.OnHealthChanged += uiController.UpdatePlayerHealthSlider;
        uiController.UpdatePlayerHealthSlider(player.playerHp, player.maxHp); // 초기 HP 설정

        player.OnPlayerDeath += () => stateMachine.ChangeState(BattleState.Lose);

        enemy.OnHealthChanged += uiController.UpdateEnemyHealthSlider;
        uiController.UpdateEnemyHealthSlider(enemy.enemyHp, enemy.maxHp);

        enemy.OnEnemyDeath += () => stateMachine.ChangeState(BattleState.Win);

        stateMachine.ChangeState(BattleState.Start);

        // 전투 시작 메시지
        uiController.SetBattleLog("Battle Start!");
        yield return new WaitForSeconds(1f);

        stateMachine.ChangeState(BattleState.PlayerTurn);
        while (stateMachine.BattleState != BattleState.Win &&
               stateMachine.BattleState != BattleState.Lose)
        {
            switch (stateMachine.BattleState)
            {
                case BattleState.PlayerTurn:
                    uiController.SetBattleLog($"전투 상황: {stateMachine.BattleState}\n");
                    yield return StartCoroutine(turnExecutor.ExecutePlayerTurn());

                    // 사망 체크
                    if (CheckBattleEnd())
                        break;

                    // PlayerTurn 완료 후 EnemyTurn으로 전환
                    stateMachine.ChangeState(BattleState.EnemyTurn);
                    break;

                case BattleState.EnemyTurn:
                    uiController.SetBattleLog($"전투 상황: {stateMachine.BattleState}\n");
                    yield return StartCoroutine(turnExecutor.ExecuteEnemyTurn());

                    // 사망 체크
                    if (CheckBattleEnd())
                        break;

                    // EnemyTurn 완료 후 PlayerTurn으로 전환
                    stateMachine.ChangeState(BattleState.PlayerTurn);
                    break;
            }
        }

        // 전투 종료
        yield return new WaitForSeconds(1f);
        uiController.SetBattleLog($"Battle End: {stateMachine.BattleState}");
        yield return new WaitForSeconds(1f);

        EndBattle();
    }

    /// <summary>
    /// 전투 종료 조건 체크
    /// </summary>
    public bool CheckBattleEnd()
    {
        if (player.IsDead())
        {
            stateMachine.ChangeState(BattleState.Lose);
            return true;
        }

        if (enemy.IsDead())
        {
            stateMachine.ChangeState(BattleState.Win);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 전투 종료 처리
    /// </summary>
    private void EndBattle()
    {
        player.OnHealthChanged -= uiController.UpdatePlayerHealthSlider;
        enemy.OnHealthChanged -= uiController.UpdateEnemyHealthSlider;

        player.OnPlayerDeath -= () => stateMachine.ChangeState(BattleState.Lose);
        enemy.OnEnemyDeath -= () => stateMachine.ChangeState(BattleState.Win);

        uiController.HideBattleUI();
        playerController.SetMovementMode(MovementMode.Walk);
        enemy.gameObject.SetActive(false);
    }
}