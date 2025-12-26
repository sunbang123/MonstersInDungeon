using System;
using System.Collections;
using UnityEngine;

public class Player : Unit
{
    [Header("Health & Mana")]
    [SerializeField] private float _playerHp = GameConstants.Player.DEFAULT_MAX_HP;
    public float maxHp = GameConstants.Player.DEFAULT_MAX_HP;
    [SerializeField] private float _playerPp = GameConstants.Player.DEFAULT_MAX_MP;
    public float maxMp = GameConstants.Player.DEFAULT_MAX_MP;

    [Header("Level & Experience")]
    [SerializeField] private int _level = GameConstants.Player.DEFAULT_LEVEL;
    [SerializeField] private float _currentExp = GameConstants.Player.DEFAULT_EXP;
    [Tooltip("다음 레벨까지 필요한 경험치")]
    public float expToNextLevel = GameConstants.Player.BASE_EXP_REQUIREMENT;

    [Header("Portrait")]
    [SerializeField] private Sprite _portrait;
    public Sprite portrait
    {
        get => _portrait;
        set
        {
            _portrait = value;
            OnPortraitChanged?.Invoke(_portrait);
        }
    }

    // 속성을 사용해서 값 검증
    public float playerHp
    {
        get => _playerHp;
        set
        {
            _playerHp = Mathf.Clamp(value, 0f, maxHp);
            // UserDataManager가 초기화되어 있을 때만 데이터 업데이트
            if (UserDataManager.Instance != null)
            {
                var data = UserDataManager.Instance.Get<UserPlayerStatusData>();
                if (data != null)
                {
                    data.HP = _playerHp;
                }
            }
            // HP 변경 이벤트 발생 (UI 업데이트를 위해)
            OnHealthChanged?.Invoke(_playerHp, maxHp);
        }
    }

    public float playerPp
    {
        get => _playerPp;
        set
        {
            _playerPp = Mathf.Clamp(value, 0f, maxMp);
            OnPPChanged?.Invoke(_playerPp, maxMp);
        }
    }

    public int level
    {
        get => _level;
        set
        {
            _level = Mathf.Max(1, value);
            OnLevelChanged?.Invoke(_level);
            // 레벨 변경 시 데이터 저장
            var data = UserDataManager.Instance?.Get<UserPlayerStatusData>();
            if (data != null)
            {
                data.Level = _level;
            }
        }
    }

    public float currentExp
    {
        get => _currentExp;
        set
        {
            _currentExp = Mathf.Max(0f, value);
            // 경험치가 다음 레벨 요구량을 넘으면 레벨업
            while (_currentExp >= expToNextLevel)
            {
                _currentExp -= expToNextLevel;
                level++;
                expToNextLevel = CalculateExpForNextLevel(_level);
            }
            // 경험치 변경 이벤트 발생
            OnExpChanged?.Invoke(_currentExp, expToNextLevel);
            // 경험치 변경 시 데이터 저장
            var data = UserDataManager.Instance?.Get<UserPlayerStatusData>();
            if (data != null)
            {
                data.CurrentExp = _currentExp;
            }
        }
    }

    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnPPChanged;
    public event Action<int> OnLevelChanged;
    public event Action<float, float> OnExpChanged;
    public event Action<Sprite> OnPortraitChanged;

    [Header("Skills")]
    public GameObject[] playerSkills;

    public event Action OnPlayerDeath;

    void Start()
    {
        var data = UserDataManager.Instance.Get<UserPlayerStatusData>();
        if (data == null)
        {
            Debug.LogError("[Player] UserPlayerStatusData를 찾을 수 없습니다!");
            return;
        }
        
        transform.position = data.Position;

        // 초기화 시 데이터에서 값 불러오기
        // 레벨과 위치처럼 직접 필드에 할당 (setter 거치지 않음)
        _playerHp = Mathf.Clamp(data.HP, 0f, maxHp);
        _playerPp = Mathf.Clamp(_playerPp, 0f, maxMp);

        // 초기 HP 설정 (속성 사용으로 데이터 저장 자동 처리)
        playerHp = _playerHp;
    }

    // Update 메소드 제거 - 더 이상 필요 없음
    // 값 검증은 속성에서 자동으로 처리됨

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
        // �ڷ�ƾ ����
        if (regenCoroutine != null)
        {
            appearance.OnSpriteChanged -= OnAppearanceSpriteChanged;
        }

        // PlayerAppearance 이벤트 구독 해제
        PlayerAppearance appearance = GetComponent<PlayerAppearance>();
        if (appearance == null)
            appearance = GetComponentInParent<PlayerAppearance>();
        if (appearance == null)
            appearance = GetComponentInChildren<PlayerAppearance>();

        if (appearance != null)
        {
            appearance.OnSpriteChanged -= OnAppearanceSpriteChanged;
        }
    }
}
