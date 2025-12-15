using System;
using System.Collections;
using UnityEngine;

public class Player : Unit
{
    [Header("Health & Mana")]
    [SerializeField] private float _playerHp = 500f;
    public float maxHp = 500f;
    [SerializeField] private float _playerPp = 100f;
    public float maxMp = 100f;

    // 속성을 사용해서 값 검증
    public float playerHp
    {
        get => _playerHp;
        set
        {
            _playerHp = Mathf.Clamp(value, 0f, maxHp);
            var data = UserDataManager.Instance.Get<UserPlayerStatusData>();
            data.HP = _playerHp;
        }
    }

    public float playerPp
    {
        get => _playerPp;
        set => _playerPp = Mathf.Clamp(value, 0f, maxMp);
    }

    public event Action<float, float> OnHealthChanged;

    [Tooltip("���� �������� �ʿ��� ����ġ")]
    public float expToNextLevel = 100f;

    [Header("Skills")]
    public GameObject[] playerSkills;

    public event Action OnPlayerDeath;

    private Coroutine regenCoroutine;

    void Start()
    {
        var data = UserDataManager.Instance.Get<UserPlayerStatusData>();
        transform.position = data.Position;

        // 초기화 시 데이터에서 값 불러오기
        _playerHp = Mathf.Clamp(data.HP, 0f, maxHp);
        _playerPp = Mathf.Clamp(_playerPp, 0f, maxMp);

        // 초기 HP 설정 (속성 사용으로 데이터 저장 자동 처리)
        playerHp = _playerHp;
    }

    // Update 메소드 제거 - 더 이상 필요 없음
    // 값 검증은 속성에서 자동으로 처리됨

    // Unit Ŭ������ �߻� �޼��� ����
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
        // �ڷ�ƾ ����
        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
        }
    }
}