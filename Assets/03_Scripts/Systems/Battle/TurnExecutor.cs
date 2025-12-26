using System.Collections;
using System;
using UnityEngine;

/// <summary>
/// 각 턴의 실행을 담당하는 클래스
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
    /// 전투 참가자 설정 (ITurnExecutor 구현)
    /// </summary>
    public void SetCombatants(Player p, Enemy e)
    {
        player = p;
        enemy = e;
    }

    /// <summary>
    /// 공통 액션 실행 패턴 (상태 변경, 로그, 대기, 액션 실행)
    /// </summary>
    private IEnumerator ExecuteActionSequence(
        Action onStart,
        Action onAction,
        Action onComplete,
        float delay = -1f)
    {
        if (delay < 0f)
            delay = GameConstants.Battle.TURN_DELAY;

        onStart?.Invoke();
        yield return new WaitForSeconds(delay);
        
        onAction?.Invoke();
        yield return new WaitForSeconds(delay);
        
        onComplete?.Invoke();
    }

    /// <summary>
    /// 데미지를 주는 공통 메서드
    /// </summary>
    private void ApplyDamage(Unit target, float damage, PlayerState playerState, EnemyState enemyState, string logMessage)
    {
        if (target == null) return;

        if (target is Player)
        {
            stateMachine.ChangeState(playerState);
        }
        else if (target is Enemy)
        {
            stateMachine.ChangeState(enemyState);
        }

        BattleUIController.OnBattleLogAppended?.Invoke(logMessage);
        target.TakeDamage(damage);
    }

    /// <summary>
    /// 플레이어 턴 실행
    /// </summary>
    public IEnumerator ExecutePlayerTurn()
    {
        // 아이템 사용 중이 아니고, BattleState가 PlayerTurn일 때만 버튼 활성화
        if (stateMachine.BattleState == BattleState.PlayerTurn && 
            stateMachine.PlayerState != PlayerState.ItemUse &&
            stateMachine.PlayerState != PlayerState.Attack &&
            stateMachine.PlayerState != PlayerState.Defense)
        {
            uiController.SetButtonsInteractable(true);
        }

        // 플레이어의 입력을 기다림 (상태가 변경될 때까지)
        yield return new WaitUntil(() => stateMachine.BattleState != BattleState.PlayerTurn);
    }

    /// <summary>
    /// 적 턴 실행
    /// </summary>
    public IEnumerator ExecuteEnemyTurn()
    {
        uiController.SetButtonsInteractable(false);

        yield return ExecuteActionSequence(
            onStart: () =>
            {
                stateMachine.ChangeState(EnemyState.Attack);
                BattleUIController.OnBattleLogAppended?.Invoke($"적이 {EnemyState.Attack}했다!\n");
            },
            onAction: () =>
            {
                if (player != null)
                {
                    ApplyDamage(player, GameConstants.Battle.DEFAULT_ENEMY_ATTACK_DAMAGE, 
                        PlayerState.Damaged, EnemyState.None, 
                        $"플레이어가 {PlayerState.Damaged} 되었다.\n");
                }
            },
            onComplete: () =>
            {
                stateMachine.ChangeState(PlayerState.None);
                stateMachine.ChangeState(BattleState.PlayerTurn);
            }
        );
    }

    /// <summary>
    /// 공격 실행
    /// </summary>
    public IEnumerator ExecuteAttack()
    {
        uiController.SetButtonsInteractable(false);

        stateMachine.ChangeState(PlayerState.Attack);
        uiController.AppendBattleLog($"당신은 {stateMachine.PlayerState}을 했다.\n");

    /// <summary>
    /// 아이템 사용 실행
    /// </summary>
    public IEnumerator ExecuteItemUse(int itemIndex)
    {
        // 이미 아이템 사용 중이면 무시
        if (stateMachine.PlayerState == PlayerState.ItemUse)
        {
            yield break;
        }

        // 인벤토리에서 아이템 데이터 가져오기
        ItemData itemData = InventoryManager.Instance?.GetBattleSlotItem(itemIndex);
        
        if (itemData == null)
        {
            HandleItemUseFailure();
            yield break;
        }

        stateMachine.ChangeState(EnemyState.Damaged);
        uiController.AppendBattleLog($"적 {stateMachine.EnemyState}!\n");

        yield return new WaitForSeconds(1f);

        stateMachine.ChangeState(BattleState.EnemyTurn);
    }

    /// <summary>
    /// 아이템 효과를 적용합니다.
    /// </summary>
    private void ApplyItemEffect(ItemData itemData)
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
    /// 아이템 효과를 적용합니다.
    /// </summary>
    private void ApplyItemEffect(ItemData itemData)
    {
        string statName = itemData.StatName;
        int statValue = itemData.StatValue;

        // 대소문자 구분 없이 비교
        string statNameLower = statName.ToLower();

        if (statNameLower.Contains("heal") || statNameLower.Contains("회복"))
        {
            // 플레이어 회복
            if (player != null)
            {
                float healAmount = statValue;
                float newHp = Mathf.Min(player.playerHp + healAmount, player.maxHp);
                player.playerHp = newHp;
                BattleUIController.OnBattleLogAppended?.Invoke($"플레이어 체력이 {healAmount} 회복되었습니다.\n");
            }
        }
        else if (statNameLower.Contains("damage") || statNameLower.Contains("attack") || statNameLower.Contains("데미지") || statNameLower.Contains("공격"))
        {
            // 적에게 데미지
            if (enemy != null)
            {
                enemy.TakeDamage(statValue);
                stateMachine.ChangeState(EnemyState.Damaged);
                BattleUIController.OnBattleLogAppended?.Invoke($"아이템으로 적에게 {statValue}의 데미지를 주었습니다.\n");
            }
        }
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
            stateMachine.ChangeState(EnemyState.Damaged);
            BattleUIController.OnBattleLogAppended?.Invoke($"적에게 큰 데미지!\n");
            yield return null; // 데미지 메시지가 먼저 표시된 후 데미지 적용
            enemy.TakeDamage(GameConstants.Battle.DEFAULT_SPECIAL_ATTACK_DAMAGE);
        }

        stateMachine.ChangeState(EnemyState.Damaged);
        uiController.AppendBattleLog($"적에게 큰 피해!\n");

        yield return new WaitForSeconds(1f);

        stateMachine.ChangeState(BattleState.EnemyTurn);
    }
}

