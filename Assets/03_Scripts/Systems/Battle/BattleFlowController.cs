using System.Collections;
using UnityEngine;
using System;

/// <summary>
/// 전투 흐름을 관리하는 클래스
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
        InitializeBattle(e);
        yield return StartCoroutine(BattleStartSequence());
        yield return StartCoroutine(BattleLoop());
        yield return StartCoroutine(BattleEndSequence());
        EndBattle();
    }

    /// <summary>
    /// 전투 초기화
    /// </summary>
    private void InitializeBattle(Enemy e)
    {
        enemy = e;
        e.SetTarget(player);
        turnExecutor.SetCombatants(player, enemy);
        uiController.ShowBattleUI();
        playerController.SetMovementMode(MovementMode.Stop);
        SubscribeBattleEvents();
        InitializeEnemyUI();
        stateMachine.ChangeState(BattleState.Start);
    }

    /// <summary>
    /// 전투 이벤트 구독
    /// </summary>
    private void SubscribeBattleEvents()
    {
        player.OnPlayerDeath += OnPlayerDeath;
        enemy.OnHealthChanged += uiController.UpdateEnemyHealthSlider;
        uiController.UpdateEnemyHealthSlider(enemy.enemyHp, enemy.maxHp);

        enemy.OnEnemyDeath += () => stateMachine.ChangeState(BattleState.Win);

    /// <summary>
    /// 플레이어 사망 처리
    /// </summary>
    private void OnPlayerDeath()
    {
        stateMachine.ChangeState(BattleState.Lose);
    }

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

        stateMachine.ChangeState(BattleState.PlayerTurn);
    }

        // 전투 종료
        yield return new WaitForSeconds(1f);
        uiController.SetBattleLog($"Battle End: {stateMachine.BattleState}");
        yield return new WaitForSeconds(1f);

    /// <summary>
    /// 전투 종료 시퀀스
    /// </summary>
    private IEnumerator BattleEndSequence()
    {
        yield return new WaitForSeconds(GameConstants.Battle.BATTLE_END_DELAY);
        BattleUIController.OnBattleLogChanged?.Invoke($"Battle End: {stateMachine.BattleState}");
        yield return new WaitForSeconds(GameConstants.Battle.BATTLE_END_DELAY);
    }

    /// <summary>
    /// 전투 종료 조건 확인
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

    /// <summary>
    /// 플레이어 상태 복원
    /// </summary>
    private void RestorePlayerState()
    {
        uiController.HideBattleUI();
        playerController.SetMovementMode(MovementMode.Walk);
    }

    /// <summary>
    /// 적 숨기기
    /// </summary>
    private void HideEnemy()
    {
        enemy.gameObject.SetActive(false);
    }
}
