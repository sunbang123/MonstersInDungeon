using System;
using System.Collections;
using TMPro;
using UnityEditor.U2D.Path.GUIFramework;
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
    Deffense,
    Damaged,
}
public enum EnemyState
{
    None,
    ItemUse,
    Attack,
    Deffense,
    Damaged,
}

public class BattleManager : SingletonBehaviour<BattleManager>
{
    public BattleState BattleState { get; private set; } = BattleState.None;
    public PlayerState PlayerState { get; private set; } = PlayerState.None;
    public EnemyState EnemyState { get; private set; } = EnemyState.None;

    [Header("UI References")]
    public GameObject battle_canvas;
    public GameObject nonbattle_canvas;
    public TextMeshProUGUI battle_log;

    [HideInInspector]
    public PlayerController _controll;

    private Player player;
    private Enemy enemy;

    protected override void Init()
    {
        base.Init();
        player = FindObjectOfType<Player>();
        enemy = FindObjectOfType<Enemy>();
        if (enemy != null && player != null)
        {
            enemy.SetTarget(player);
        }

        _controll = player.GetComponent<PlayerController>();
    }

    public void Update()
    {
        battle_log.text = $"전투 상황{BattleState}\n당신은 {PlayerState}을 했다.\n적은 {EnemyState}";
    }
    public void StartBattle()
    {
        battle_canvas.SetActive(true);
        nonbattle_canvas.SetActive(false);
        _controll.SetMovementMode(MovementMode.Stop);
        ChangeState(BattleState.Start);
        StartCoroutine(BattleFlow());
    }

    private IEnumerator BattleFlow()
    {
        Logger.Log("=== Battle Start! ===");
        battle_log.text = "Battle Start!";
        yield return new WaitForSeconds(1f);

        ChangeState(BattleState.PlayerTurn);

        // 전투 루프 필요
        while (BattleState != BattleState.Win && BattleState != BattleState.Lose)
        {
            battle_log.text = $"Current State: {BattleState}";

            switch (BattleState)
            {
                case BattleState.PlayerTurn:
                    yield return StartCoroutine(PlayerTurnRoutine());
                    break;
                case BattleState.EnemyTurn:
                    yield return StartCoroutine(EnemyTurnRoutine());
                    break;
            }

            CheckBattleEnd();
        }

        yield return new WaitForSeconds(1f);
        Logger.Log($"=== Battle End: {BattleState} ===");
        battle_log.text = $"Battle End: {BattleState}";

        // 전투 종료 처리
        battle_canvas.SetActive(false);
        nonbattle_canvas.SetActive(true);
        _controll.SetMovementMode(MovementMode.Walk);
    }

    private void CheckBattleEnd()
    {
        throw new NotImplementedException();
    }

    private string EnemyTurnRoutine()
    {
        throw new NotImplementedException();
    }

    private string PlayerTurnRoutine()
    {
        throw new NotImplementedException();
    }

    public void OnItemUse()
    {
        ChangeState(PlayerState.ItemUse);
    }

    public void OnAttack()
    {
        Logger.Log("Player attacks!");
        ChangeState(BattleState.EnemyTurn);
    }

    public void ChangeState(BattleState newState)
    {
        Logger.Log($"State: {BattleState} → {newState}");
        BattleState = newState;
    }
    public void ChangeState(PlayerState newState)
    {
        Logger.Log($"State: {PlayerState} → {newState}");
        PlayerState = newState;
    }
    public void ChangeState(EnemyState newState)
    {
        Logger.Log($"State: {EnemyState} → {newState}");
        EnemyState = newState;
    }

    protected override void Dispose()
    {
        base.Dispose();
    }
}