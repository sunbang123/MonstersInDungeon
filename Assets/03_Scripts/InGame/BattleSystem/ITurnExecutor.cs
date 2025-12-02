using System.Collections;

/// <summary>
/// 턴 실행을 담당하는 인터페이스
/// </summary>
public interface ITurnExecutor
{
    void Initialize(BattleStateMachine stateMachine, BattleUIController uiController);
    void SetCombatants(Player p, Enemy e);
    IEnumerator ExecutePlayerTurn();
    IEnumerator ExecuteEnemyTurn();
    IEnumerator ExecuteAttack();
    IEnumerator ExecuteItemUse(int itemIndex);
    IEnumerator ExecuteDefense();
    IEnumerator ExecuteSpecialAttack();
}