using UnityEngine;

public enum BattleState
{
    None,
    Start,
    PlayerTurn,
    EnemyTurn,
    Win,
    Lose
}

public enum PlayerState
{
    None,
    ItemUse,
    Attack,
    Defense,
    Damaged,
}

public enum EnemyState
{
    None,
    ItemUse,
    Attack,
    Defense,
    Damaged,
}

/// <summary>
/// 전투 상태를 관리하는 클래스
/// </summary>
public class BattleStateMachine : MonoBehaviour
{
    public BattleState BattleState { get; private set; } = BattleState.None;
    public PlayerState PlayerState { get; private set; } = PlayerState.None;
    public EnemyState EnemyState { get; private set; } = EnemyState.None;

    public void ChangeState(BattleState newState)
    {
        BattleState = newState;
    }

    public void ChangeState(PlayerState newState)
    {
        PlayerState = newState;
    }

    public void ChangeState(EnemyState newState)
    {
        EnemyState = newState;
    }
}
