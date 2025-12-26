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

        // 레벨과 경험치 로드
        _level = data.Level;
        _currentExp = data.CurrentExp;
        
        // HP 변경 이벤트 발생 (UI 업데이트를 위해)
        OnHealthChanged?.Invoke(_playerHp, maxHp);
        expToNextLevel = CalculateExpForNextLevel(_level);
        
        // 레벨과 경험치 이벤트 발생
        OnLevelChanged?.Invoke(_level);
        OnExpChanged?.Invoke(_currentExp, expToNextLevel);

        // PlayerAppearance 이벤트 구독
        SubscribeToPlayerAppearance();
    }

    /// <summary>
    /// PlayerAppearance의 스프라이트 변경을 구독
    /// </summary>
    private void SubscribeToPlayerAppearance()
    {
        PlayerAppearance appearance = GetComponent<PlayerAppearance>();
        if (appearance == null)
            appearance = GetComponentInParent<PlayerAppearance>();
        if (appearance == null)
            appearance = GetComponentInChildren<PlayerAppearance>();

        if (appearance != null)
        {
            // PlayerAppearance의 스프라이트 변경 이벤트 구독
            appearance.OnSpriteChanged += OnAppearanceSpriteChanged;
        }
    }

    /// <summary>
    /// PlayerAppearance의 스프라이트가 변경되었을 때 호출
    /// </summary>
    private void OnAppearanceSpriteChanged(Sprite newSprite)
    {
        if (newSprite != null)
        {
            portrait = newSprite; // 속성을 통해 설정하면 OnPortraitChanged 이벤트가 자동 발생
        }
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

    /// <summary>
    /// 경험치 획득
    /// </summary>
    public void GainExperience(float exp)
    {
        currentExp += exp;
    }

#if UNITY_EDITOR
    [ContextMenu("경험치 50 추가")]
    private void EditorAddExp50()
    {
        GainExperience(50f);
    }

    [ContextMenu("경험치 100 추가")]
    private void EditorAddExp100()
    {
        GainExperience(100f);
    }

    [ContextMenu("경험치 200 추가")]
    private void EditorAddExp200()
    {
        GainExperience(200f);
    }
#endif

    /// <summary>
    /// 다음 레벨까지 필요한 경험치 계산
    /// </summary>
    private float CalculateExpForNextLevel(int currentLevel)
    {
        // 기본 공식: 레벨이 올라갈수록 더 많은 경험치 필요
        return GameConstants.Player.BASE_EXP_REQUIREMENT * Mathf.Pow(GameConstants.Player.EXP_MULTIPLIER, currentLevel - 1);
    }

    void OnDestroy()
    {
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
