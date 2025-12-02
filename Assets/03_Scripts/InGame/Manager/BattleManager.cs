using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("UI Buttons")]
    public Button Atk_btn;
    public GameObject Inventory;
    public List<Button> item_btn;
    public Button specialAtk_btn;
    public Button defense_btn;

    [HideInInspector]
    public PlayerController _controll;
    private PlayerInteraction _playerInteraction;

    private Player player;
    private Enemy enemy;

    // player슬라이더
    // enemy슬라이더
    protected override void Init()
    {
        base.Init();
        player = FindObjectOfType<Player>();

        _controll = player.GetComponent<PlayerController>();
        _playerInteraction = player.GetComponent<PlayerInteraction>();

        // Inventory의 자식 버튼들 가져오기
        GetInventoryButtons();

        // 버튼 바인드
        BindButtons();
        // 초기에는 버튼 비활성화
        SetButtonsInteractable(false);
    }

    // Inventory의 자식 요소에서 Button 컴포넌트를 가진 것들을 찾아 리스트에 추가
    private void GetInventoryButtons()
    {
        item_btn.Clear();

        if (Inventory != null)
        {
            // Inventory의 모든 자식에서 Button 컴포넌트 찾기
            Button[] buttons = Inventory.GetComponentsInChildren<Button>();
            item_btn.AddRange(buttons);
        }
    }

    private void BindButtons()
    {
        if (Atk_btn != null)
            Atk_btn.onClick.AddListener(OnAttack);

        // 리스트의 각 버튼에 리스너 추가
        if (item_btn != null && item_btn.Count > 0)
        {
            for (int i = 0; i < item_btn.Count; i++)
            {
                int index = i; // 클로저 문제 방지
                item_btn[i].onClick.AddListener(() => OnItemUse(index));
            }
        }
    }
    private void SetButtonsInteractable(bool interactable)
    {
        if (Atk_btn != null)
            Atk_btn.interactable = interactable;

        // 리스트의 모든 버튼 활성화/비활성화
        if (item_btn != null && item_btn.Count > 0)
        {
            foreach (var btn in item_btn)
            {
                if (btn != null)
                    btn.interactable = interactable;
            }
        }

        if (specialAtk_btn != null)
            specialAtk_btn.interactable = interactable;
        if (defense_btn != null)
            defense_btn.interactable = interactable;
    }
    public void StartBattle(Enemy e)
    {
        enemy = e;
        e.SetTarget(player);
        battle_canvas.SetActive(true);
        nonbattle_canvas.SetActive(false);
        _controll.SetMovementMode(MovementMode.Stop);
        ChangeState(BattleState.Start);
        StartCoroutine(BattleFlow());
    }

    private IEnumerator BattleFlow()
    {
        battle_log.text = "Battle Start!";
        yield return new WaitForSeconds(1f);

        ChangeState(BattleState.PlayerTurn);

        int turnCount = 0;
        // 전투 루프 필요
        while (BattleState != BattleState.Win && BattleState != BattleState.Lose)
        {
            battle_log.text = $"전투 상황: {BattleState}\n";

            switch (BattleState)
            {
                case BattleState.PlayerTurn:
                    yield return StartCoroutine(PlayerTurnRoutine());
                    break;
                case BattleState.EnemyTurn:
                    yield return StartCoroutine(EnemyTurnRoutine());
                    break;
            }

            turnCount++;

            if (CheckBattleEnd(turnCount)) break;
        }

        yield return new WaitForSeconds(1f);
        ChangeState(BattleState.Win);
        battle_log.text = $"Battle End: {BattleState}";
        yield return new WaitForSeconds(1f);

        // 전투 종료 처리
        battle_canvas.SetActive(false);
        nonbattle_canvas.SetActive(true);
        _controll.SetMovementMode(MovementMode.Walk);
        enemy.gameObject.SetActive(false);
    }

    private bool CheckBattleEnd(int turnCount)
    {
        return turnCount == 4;
    }

    private IEnumerator EnemyTurnRoutine()
    {
        // 적 턴 시작 - 버튼 비활성화
        SetButtonsInteractable(false);

        yield return new WaitForSeconds(1f);
        ChangeState(EnemyState.Attack);
        battle_log.text += $"적은 {EnemyState}했다!\n";
        yield return new WaitForSeconds(1f);
        ChangeState(PlayerState.Damaged);
        battle_log.text += $"당신은 {PlayerState} 되었다.\n";
        yield return new WaitForSeconds(1f);
        ChangeState(BattleState.PlayerTurn);
    }


    private IEnumerator PlayerTurnRoutine()
    {
        // 적 턴 시작 - 버튼 활성화
        SetButtonsInteractable(true);

        // 여기서 플레이어의 입력을 기다림
        yield return new WaitUntil(() => BattleState != BattleState.PlayerTurn);
    }

    public void OnItemUse(int itemIndex)
    {
        StartCoroutine(ItemUseRoutine(itemIndex));
    }
    private IEnumerator ItemUseRoutine(int itemIndex)
    {
        SetButtonsInteractable(false);

        ChangeState(PlayerState.ItemUse);
        battle_log.text += $"당신은 아이템 {itemIndex + 1}을 사용했다.\n";
        yield return new WaitForSeconds(1f);

        // TODO: 실제 아이템 사용 로직

        ChangeState(BattleState.EnemyTurn);
    }

    public void OnAttack()
    {
        StartCoroutine(AttackRoutine());
    }
    IEnumerator AttackRoutine()
    {
        SetButtonsInteractable(false);

        ChangeState(PlayerState.Attack);
        battle_log.text += $"당신은 {PlayerState}을 했다.\n";
        yield return new WaitForSeconds(1f);
        ChangeState(EnemyState.Damaged);
        battle_log.text += $"적 {EnemyState}!";
        yield return new WaitForSeconds(1f);
        ChangeState(BattleState.EnemyTurn);
    }

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

    protected override void Dispose()
    {
        base.Dispose();
    }
}