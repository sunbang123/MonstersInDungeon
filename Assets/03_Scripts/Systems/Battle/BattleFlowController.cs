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
        enemy = e;
        e.SetTarget(player);

        turnExecutor.SetCombatants(player, enemy);

        uiController.ShowBattleUI();
        playerController.SetMovementMode(MovementMode.Stop);

        // Player 이벤트 구독 (전투 종료 시에만 사용하는 것만)
        // Player UI 업데이트는 BattleUIController의 Start에서 이미 구독됨
        player.OnPlayerDeath += () => stateMachine.ChangeState(BattleState.Lose);

        // Enemy 이벤트 구독
        enemy.OnHealthChanged += uiController.UpdateEnemyHealthSlider;
        enemy.OnPPChanged += uiController.UpdateEnemyPPSlider;
        enemy.OnLevelChanged += uiController.UpdateEnemyLevel;
        enemy.OnPortraitChanged += uiController.UpdateEnemyPortrait;
        enemy.OnEnemyDeath += OnEnemyDefeated;
        
        // Enemy 초기값 설정 (속성을 통해 이벤트 발생)
        enemy.enemyHp = enemy.enemyHp; // 이미 설정되어 있지만 이벤트 발생을 위해
        enemy.enemyPp = enemy.enemyPp;
        enemy.level = enemy.level;
        enemy.portrait = enemy.portrait;

        stateMachine.ChangeState(BattleState.Start);

        // 전투 시작 메시지
        BattleUIController.OnBattleLogChanged?.Invoke("Battle Start!");
        yield return new WaitForSeconds(1f);

        stateMachine.ChangeState(BattleState.PlayerTurn);
        while (stateMachine.BattleState != BattleState.Win &&
               stateMachine.BattleState != BattleState.Lose)
        {
            switch (stateMachine.BattleState)
            {
                case BattleState.PlayerTurn:
                    BattleUIController.OnBattleLogChanged?.Invoke($"전투 상태: {stateMachine.BattleState}\n");
                    yield return StartCoroutine(turnExecutor.ExecutePlayerTurn());

                    // 승부 확인
                    if (CheckBattleEnd())
                        break;

                    // PlayerTurn 완료 후 EnemyTurn으로 전환
                    stateMachine.ChangeState(BattleState.EnemyTurn);
                    break;

                case BattleState.EnemyTurn:
                    BattleUIController.OnBattleLogChanged?.Invoke($"전투 상태: {stateMachine.BattleState}\n");
                    yield return StartCoroutine(turnExecutor.ExecuteEnemyTurn());

                    // 승부 확인
                    if (CheckBattleEnd())
                        break;

                    // EnemyTurn 완료 후 PlayerTurn으로 전환
                    stateMachine.ChangeState(BattleState.PlayerTurn);
                    break;
            }
        }

        // 전투 종료
        yield return new WaitForSeconds(1f);
        BattleUIController.OnBattleLogChanged?.Invoke($"Battle End: {stateMachine.BattleState}");
        yield return new WaitForSeconds(1f);

        EndBattle();
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
            // 적을 물리쳤을 때 경험치 획득
            if (player != null && enemy != null)
            {
                player.GainExperience(enemy.expReward);
                BattleUIController.OnBattleLogChanged?.Invoke($"경험치 {enemy.expReward} 획득!");
            }
            stateMachine.ChangeState(BattleState.Win);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 적이 패배했을 때 호출
    /// </summary>
    private void OnEnemyDefeated()
    {
        stateMachine.ChangeState(BattleState.Win);
    }

    /// <summary>
    /// 전투 종료 처리
    /// </summary>
    private void EndBattle()
    {
        // Player 이벤트 구독 해제
        player.OnPlayerDeath -= () => stateMachine.ChangeState(BattleState.Lose);

        // Enemy 이벤트 구독 해제
        enemy.OnHealthChanged -= uiController.UpdateEnemyHealthSlider;
        enemy.OnPPChanged -= uiController.UpdateEnemyPPSlider;
        enemy.OnLevelChanged -= uiController.UpdateEnemyLevel;
        enemy.OnPortraitChanged -= uiController.UpdateEnemyPortrait;
        enemy.OnEnemyDeath -= OnEnemyDefeated;

        uiController.HideBattleUI();
        playerController.SetMovementMode(MovementMode.Walk);
        enemy.gameObject.SetActive(false);
    }
}
