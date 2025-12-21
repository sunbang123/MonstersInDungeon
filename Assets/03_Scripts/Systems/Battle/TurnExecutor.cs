using System.Collections;
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
        // 버튼 비활성화
        uiController.SetButtonsInteractable(false);

        yield return new WaitForSeconds(1f);

        stateMachine.ChangeState(EnemyState.Attack);
        BattleUIController.OnBattleLogAppended?.Invoke($"적이 {stateMachine.EnemyState}했다!\n");

        yield return new WaitForSeconds(1f);

        if (player != null)
        {
            float enemyAttackDamage = 20f; // 임시 데미지
            player.TakeDamage(enemyAttackDamage);
        }

        stateMachine.ChangeState(PlayerState.Damaged);
        BattleUIController.OnBattleLogAppended?.Invoke($"플레이어가 {stateMachine.PlayerState} 되었다.\n");

        yield return new WaitForSeconds(1f);

        // PlayerState 리셋 후 BattleState 변경
        stateMachine.ChangeState(PlayerState.None);
        stateMachine.ChangeState(BattleState.PlayerTurn);
    }

    /// <summary>
    /// 공격 실행
    /// </summary>
    public IEnumerator ExecuteAttack()
    {
        uiController.SetButtonsInteractable(false);

        stateMachine.ChangeState(PlayerState.Attack);
        BattleUIController.OnBattleLogAppended?.Invoke($"플레이어가 {stateMachine.PlayerState}를 했다.\n");

        yield return new WaitForSeconds(1f);

        if (enemy != null)
        {
            float playerAttackDamage = 30f;
            enemy.TakeDamage(playerAttackDamage);
        }

        stateMachine.ChangeState(EnemyState.Damaged);
        BattleUIController.OnBattleLogAppended?.Invoke($"적 {stateMachine.EnemyState}!\n");

        yield return new WaitForSeconds(1f);

        // PlayerState 리셋 후 BattleState 변경
        stateMachine.ChangeState(PlayerState.None);
        stateMachine.ChangeState(BattleState.EnemyTurn);
    }

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
            BattleUIController.OnBattleLogAppended?.Invoke($"아이템을 찾을 수 없습니다.\n");
            stateMachine.ChangeState(PlayerState.None);
            stateMachine.ChangeState(BattleState.PlayerTurn);
            uiController.SetButtonsInteractable(true);
            yield break;
        }

        // 버튼은 이미 BattleManager에서 비활성화됨 (중복 클릭 방지)
        // PlayerState.ItemUse를 설정하여 ExecutePlayerTurn에서 버튼이 다시 활성화되지 않도록 함
        stateMachine.ChangeState(PlayerState.ItemUse);
        
        BattleUIController.OnBattleLogAppended?.Invoke($"플레이어가 {itemData.itemName}을(를) 사용했다.\n");

        yield return new WaitForSeconds(1f);

        // 아이템 효과 적용
        ApplyItemEffect(itemData);

        // 아이템 사용 후 인벤토리에서 제거
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RemoveBattleSlotItem(itemIndex);
        }

        yield return new WaitForSeconds(1f);

        // 아이템 사용 완료 - PlayerState 리셋 후 BattleState 변경
        stateMachine.ChangeState(PlayerState.None);
        // BattleState를 EnemyTurn으로 변경하여 ExecutePlayerTurn의 WaitUntil이 깨지도록 함
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
        BattleUIController.OnBattleLogAppended?.Invoke($"플레이어가 방어 자세를 취했다.\n");

        yield return new WaitForSeconds(1f);

        // PlayerState 리셋 후 BattleState 변경
        stateMachine.ChangeState(PlayerState.None);
        stateMachine.ChangeState(BattleState.EnemyTurn);
    }

    /// <summary>
    /// 특수 공격 실행
    /// </summary>
    public IEnumerator ExecuteSpecialAttack()
    {
        uiController.SetButtonsInteractable(false);

        stateMachine.ChangeState(PlayerState.Attack);
        BattleUIController.OnBattleLogAppended?.Invoke($"플레이어가 특수 공격을 했다!\n");

        yield return new WaitForSeconds(1f);

        if (enemy != null)
        {
            float specialAttackDamage = 50f; // 임시 데미지
            enemy.TakeDamage(specialAttackDamage);
        }

        stateMachine.ChangeState(EnemyState.Damaged);
        BattleUIController.OnBattleLogAppended?.Invoke($"적에게 큰 데미지!\n");

        yield return new WaitForSeconds(1f);

        // PlayerState 리셋 후 BattleState 변경
        stateMachine.ChangeState(PlayerState.None);
        stateMachine.ChangeState(BattleState.EnemyTurn);
    }
}

