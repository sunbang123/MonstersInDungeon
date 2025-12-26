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
        enemy.OnPPChanged += uiController.UpdateEnemyPPSlider;
        enemy.OnLevelChanged += uiController.UpdateEnemyLevel;
        enemy.OnPortraitChanged += uiController.UpdateEnemyPortrait;
        enemy.OnEnemyDeath += OnEnemyDefeated;
    }

    /// <summary>
    /// Enemy UI 초기화
    /// </summary>
    private void InitializeEnemyUI()
    {
        enemy.enemyHp = enemy.enemyHp;
        enemy.enemyPp = enemy.enemyPp;
        enemy.level = enemy.level;
        enemy.portrait = enemy.portrait;
    }

    /// <summary>
    /// 플레이어 사망 처리
    /// </summary>
    private void OnPlayerDeath()
    {
        stateMachine.ChangeState(BattleState.Lose);
    }

    /// <summary>
    /// 전투 시작 시퀀스
    /// </summary>
    private IEnumerator BattleStartSequence()
    {
        BattleUIController.OnBattleLogChanged?.Invoke("Battle Start!");
        yield return new WaitForSeconds(GameConstants.Battle.BATTLE_START_DELAY);
        stateMachine.ChangeState(BattleState.PlayerTurn);
    }

    /// <summary>
    /// 전투 루프
    /// </summary>
    private IEnumerator BattleLoop()
    {
        while (!IsBattleEnd())
        {
            yield return StartCoroutine(ExecuteCurrentTurn());
        }
    }

    /// <summary>
    /// 현재 턴 실행
    /// </summary>
    private IEnumerator ExecuteCurrentTurn()
    {
        switch (stateMachine.BattleState)
        {
            case BattleState.PlayerTurn:
                yield return StartCoroutine(ExecutePlayerTurn());
                break;
            case BattleState.EnemyTurn:
                yield return StartCoroutine(ExecuteEnemyTurn());
                break;
        }
    }

    /// <summary>
    /// 플레이어 턴 실행
    /// </summary>
    private IEnumerator ExecutePlayerTurn()
    {
        BattleUIController.OnBattleLogChanged?.Invoke($"전투 상태: {stateMachine.BattleState}\n");
        yield return StartCoroutine(turnExecutor.ExecutePlayerTurn());

        if (CheckBattleEnd()) yield break;

        if (stateMachine.BattleState == BattleState.PlayerTurn)
        {
            stateMachine.ChangeState(BattleState.EnemyTurn);
        }
    }

    /// <summary>
    /// 적 턴 실행
    /// </summary>
    private IEnumerator ExecuteEnemyTurn()
    {
        BattleUIController.OnBattleLogChanged?.Invoke($"전투 상태: {stateMachine.BattleState}\n");
        yield return StartCoroutine(turnExecutor.ExecuteEnemyTurn());

        if (CheckBattleEnd()) yield break;

        stateMachine.ChangeState(BattleState.PlayerTurn);
    }

    /// <summary>
    /// 전투 종료 여부 확인
    /// </summary>
    private bool IsBattleEnd()
    {
        return stateMachine.BattleState == BattleState.Win || 
               stateMachine.BattleState == BattleState.Lose;
    }

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

        // 적의 HP가 0 이하이거나 이미 죽은 경우
        if (enemy != null && (enemy.enemyHp <= 0f || enemy.IsDead()))
        {
            // 아직 Win 상태가 아니면 Win 상태로 변경 (중복 방지)
            if (stateMachine.BattleState != BattleState.Win)
            {
                stateMachine.ChangeState(BattleState.Win);
            }
            return true;
        }

        return false;
    }

    /// <summary>
    /// 적이 패배했을 때 호출 (코루틴으로 메시지 표시 후 전투 종료)
    /// </summary>
    private void OnEnemyDefeated()
    {
        StartCoroutine(HandleEnemyDefeat());
    }

    /// <summary>
    /// 적 패배 처리 코루틴 (경험치 처리만 담당)
    /// </summary>
    private IEnumerator HandleEnemyDefeat()
    {
        // 이미 "적이 쓰러졌습니다" 메시지가 표시되었으므로
        // 잠시 대기 후 경험치 표시
        yield return new WaitForSeconds(GameConstants.Battle.ENEMY_DEFEAT_MESSAGE_DELAY);
        
        // 경험치 획득
        if (player != null && enemy != null)
        {
            player.GainExperience(enemy.expReward);
            BattleUIController.OnBattleLogChanged?.Invoke($"경험치 {enemy.expReward} 획득!");
        }
        
        // Win 상태는 CheckBattleEnd()에서 이미 변경됨
    }

    /// <summary>
    /// 전투 종료 처리
    /// </summary>
    private void EndBattle()
    {
        UnsubscribeBattleEvents();
        RestorePlayerState();
        HideEnemy();
    }

    /// <summary>
    /// 전투 이벤트 구독 해제
    /// </summary>
    private void UnsubscribeBattleEvents()
    {
        player.OnPlayerDeath -= OnPlayerDeath;
        enemy.OnHealthChanged -= uiController.UpdateEnemyHealthSlider;
        enemy.OnPPChanged -= uiController.UpdateEnemyPPSlider;
        enemy.OnLevelChanged -= uiController.UpdateEnemyLevel;
        enemy.OnPortraitChanged -= uiController.UpdateEnemyPortrait;
        enemy.OnEnemyDeath -= OnEnemyDefeated;
    }

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
