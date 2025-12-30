using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// 전투 흐름을 관리하는 클래스
/// </summary>
public class BattleFlowController : MonoBehaviour
{
    private BattleStateMachine stateMachine;
    private BattleUIController uiController;
    private ITurnExecutor turnExecutor;
    private Enemy enemy;

    // 씬 전환 관련
    private AssetReference inBattleSceneReference;
    private AssetReference inGameSceneReference;
    private AsyncOperationHandle<SceneInstance> battleSceneHandle;

    public void Initialize(BattleStateMachine sm, BattleUIController ui, TurnExecutor te)
    {
        stateMachine = sm;
        uiController = ui;
        turnExecutor = te;
    }

    /// <summary>
    /// 씬 참조 설정
    /// </summary>
    public void SetSceneReferences(AssetReference battleScene, AssetReference gameScene)
    {
        inBattleSceneReference = battleScene;
        inGameSceneReference = gameScene;
    }

    /// <summary>
    /// 전투 시작
    /// Note: InBattle 씬에서 호출되므로 씬 전환 없이 바로 전투 시작
    /// </summary>
    public IEnumerator StartBattle(Enemy e)
    {
        // InBattle 씬에서 호출되므로 씬 전환 없이 바로 전투 시작
        if (e == null)
        {
            Logger.LogError("Enemy가 null입니다.");
            yield break;
        }

        // 전투 시작
        InitializeBattle(e);
        yield return StartCoroutine(BattleStartSequence());
        yield return StartCoroutine(BattleLoop());
        yield return StartCoroutine(BattleEndSequence());
        
        // 전투 종료 및 InGame 씬으로 복귀
        EndBattle();
        
        if (inGameSceneReference != null)
        {
            yield return StartCoroutine(ReturnToGameScene());
        }
    }

    // Note: LoadBattleScene 메서드는 BattleStarter에서 처리하므로 제거됨

    /// <summary>
    /// 전투 초기화
    /// </summary>
    private void InitializeBattle(Enemy e)
    {
        if (enemy == null) enemy = e;
        if (enemy != null)
        {
            if (turnExecutor != null)
            {
                // TurnExecutor는 Player 데이터를 BattleDataTransfer에서 가져옴
                turnExecutor.SetCombatants(null, enemy);
            }
        }
        
        if (uiController != null)
        {
            uiController.ShowBattleUI();
        }
        
        SubscribeBattleEvents();
        InitializeEnemyUI();
        InitializePlayerUI();
        
        if (stateMachine != null)
        {
            stateMachine.ChangeState(BattleState.Start);
        }
    }

    /// <summary>
    /// 전투 이벤트 구독
    /// </summary>
    private void SubscribeBattleEvents()
    {
        // Player 사망은 BattleDataTransfer의 Player 데이터를 통해 확인

        if (enemy != null && uiController != null)
        {
            enemy.OnHealthChanged += uiController.UpdateEnemyHealthSlider;
            enemy.OnPPChanged += uiController.UpdateEnemyPPSlider;
            enemy.OnLevelChanged += uiController.UpdateEnemyLevel;
            enemy.OnPortraitChanged += uiController.UpdateEnemyPortrait;
            enemy.OnEnemyDeath += OnEnemyDefeated;
        }
    }

    /// <summary>
    /// Enemy UI 초기화 (이벤트 구독 후 호출되므로 프로퍼티 재설정으로 이벤트 발생)
    /// </summary>
    private void InitializeEnemyUI()
    {
        if (enemy != null && uiController != null)
        {
            // 프로퍼티를 다시 설정하여 이벤트 발생 (UI 업데이트)
            float currentHp = enemy.enemyHp;
            float currentPp = enemy.enemyPp;
            int currentLevel = enemy.level;
            Sprite currentPortrait = enemy.portrait;
            
            // 프로퍼티 setter를 통해 이벤트 발생 및 UI 업데이트
            enemy.enemyHp = currentHp;
            enemy.enemyPp = currentPp;
            enemy.level = currentLevel;
            enemy.portrait = currentPortrait;
        }
    }

    /// <summary>
    /// Player UI 초기화 (전투씬에는 Player 오브젝트가 없으므로 BattleDataTransfer에서 데이터 가져옴)
    /// </summary>
    private void InitializePlayerUI()
    {
        if (uiController == null) return;

        var playerData = BattleDataTransfer.GetPlayerData();
        if (playerData.HasValue)
        {
            // 초기 UI 업데이트
            uiController.UpdatePlayerHealthSlider(playerData.Value.playerHp, playerData.Value.maxHp);
            uiController.UpdatePlayerPPSlider(playerData.Value.playerPp, playerData.Value.maxPp);
            uiController.UpdatePlayerLevel(playerData.Value.level);
            
            // BattleDataTransfer 이벤트 구독 (전투 중 HP/PP 변경 시 UI 업데이트)
            BattleDataTransfer.OnPlayerHealthChanged += uiController.UpdatePlayerHealthSlider;
            BattleDataTransfer.OnPlayerPPChanged += uiController.UpdatePlayerPPSlider;
            BattleDataTransfer.OnPlayerLevelChanged += uiController.UpdatePlayerLevel;
        }
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
        // Player 오브젝트가 없으므로 BattleDataTransfer에서 Player 데이터 확인
        var playerData = BattleDataTransfer.GetPlayerData();
        if (playerData.HasValue && playerData.Value.playerHp <= 0f)
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
        
        // 경험치 획득 (Player 오브젝트가 없으므로 BattleDataTransfer에 저장)
        if (enemy != null)
        {
            var playerData = BattleDataTransfer.GetPlayerData();
            if (playerData.HasValue)
            {
                var updatedData = playerData.Value;
                updatedData.currentExp += enemy.expReward;
                BattleDataTransfer.StorePlayerData(null); // 임시로 데이터 업데이트
                BattleUIController.OnBattleLogChanged?.Invoke($"경험치 {enemy.expReward} 획득!");
            }
        }
        
        // Win 상태는 CheckBattleEnd()에서 이미 변경됨
    }

    /// <summary>
    /// InGame 씬으로 복귀
    /// </summary>
    private IEnumerator ReturnToGameScene()
    {
        if (inGameSceneReference == null)
        {
            Logger.LogWarning("InGame 씬 참조가 설정되지 않았습니다. 씬 전환 없이 종료합니다.");
            yield break;
        }

        // InGame 씬 로드
        Logger.Log("InGame 씬으로 복귀 중...");
        var gameSceneHandle = Addressables.LoadSceneAsync(
            inGameSceneReference,
            LoadSceneMode.Single,
            activateOnLoad: true
        );

        yield return gameSceneHandle;

        if (gameSceneHandle.Status == AsyncOperationStatus.Succeeded)
        {
            var gameScene = gameSceneHandle.Result.Scene;
            SceneManager.SetActiveScene(gameScene);
            Logger.Log("InGame 씬 복귀 완료");

            // InGame 씬의 Player에 업데이트된 데이터 적용
            Player gamePlayer = FindObjectOfType<Player>();
            if (gamePlayer != null)
            {
                var playerData = BattleDataTransfer.GetPlayerData();
                if (playerData.HasValue)
                {
                    BattleDataTransfer.ApplyPlayerData(gamePlayer, playerData.Value);
                }
            }

            // 전투에서 승리했다면 Enemy 처리
            if (stateMachine != null && stateMachine.BattleState == BattleState.Win)
            {
                // InGame 씬에서 Enemy 찾아서 비활성화
                Enemy gameEnemy = FindObjectOfType<Enemy>();
                if (gameEnemy != null)
                {
                    gameEnemy.gameObject.SetActive(false);
                }
            }

            // 전투 데이터 초기화
            BattleDataTransfer.ClearData();
        }
        else
        {
            Logger.LogError($"InGame 씬 로드 실패: {gameSceneHandle.OperationException}");
        }
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
        if (enemy != null && uiController != null)
        {
            enemy.OnHealthChanged -= uiController.UpdateEnemyHealthSlider;
            enemy.OnPPChanged -= uiController.UpdateEnemyPPSlider;
            enemy.OnLevelChanged -= uiController.UpdateEnemyLevel;
            enemy.OnPortraitChanged -= uiController.UpdateEnemyPortrait;
            enemy.OnEnemyDeath -= OnEnemyDefeated;
        }

        // BattleDataTransfer 이벤트 구독 해제
        if (uiController != null)
        {
            BattleDataTransfer.OnPlayerHealthChanged -= uiController.UpdatePlayerHealthSlider;
            BattleDataTransfer.OnPlayerPPChanged -= uiController.UpdatePlayerPPSlider;
            BattleDataTransfer.OnPlayerLevelChanged -= uiController.UpdatePlayerLevel;
        }
    }

    /// <summary>
    /// 플레이어 상태 복원
    /// </summary>
    private void RestorePlayerState()
    {
        if (uiController != null)
        {
            uiController.HideBattleUI();
        }
    }

    /// <summary>
    /// 적 숨기기
    /// </summary>
    private void HideEnemy()
    {
        if (enemy != null && enemy.gameObject != null)
        {
            enemy.gameObject.SetActive(false);
        }
    }
}
