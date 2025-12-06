using System;
using System.Collections;
using UnityEngine;

public class Player : Unit
{
    [Header("Health & Mana")]
    public float playerHp = 500f;
    public float maxHp = 500f;
    public float playerPp = 100f;
    public float maxMp = 100f;

    public event Action<float, float> OnHealthChanged;

    [Tooltip("다음 레벨까지 필요한 경험치")]
    public float expToNextLevel = 100f;

    [Header("Skills")]
    public GameObject[] playerSkills;

    public event Action OnPlayerDeath;

    private Coroutine regenCoroutine;

    void Start()
    {
        var data = UserDataManager.Instance.Get<UserPlayerStatusData>();
        transform.position = data.Position;

        // 초기값 설정
        playerHp = Mathf.Clamp(playerHp, 0f, maxHp);
        playerPp = Mathf.Clamp(playerPp, 0f, maxMp);
    }

    void Update()
    {
        // HP/MP가 최대값을 초과하지 않도록 제한
        playerHp = Mathf.Clamp(playerHp, 0f, maxHp);
        playerPp = Mathf.Clamp(playerPp, 0f, maxMp);
    }

    // Unit 클래스의 추상 메서드 구현
    protected override float GetCurrentHp() => playerHp;
    protected override float GetMaxHp() => maxHp;
    protected override void SetCurrentHp(float value)
    {
        playerHp = value;
        var data = UserDataManager.Instance.Get<UserPlayerStatusData>();
        data.HP = playerHp;
    }

    protected override void InvokeHealthChanged(float current, float max)
        => OnHealthChanged?.Invoke(current, max);
    protected override void InvokeDeath() => OnPlayerDeath?.Invoke();

    void OnDestroy()
    {
        // 코루틴 정리
        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
        }
    }
}