using System;
using UnityEngine;

/// <summary>
/// 씬 전환 시 전투 데이터를 전달하는 클래스 (DontDestroyOnLoad 없이 사용)
/// </summary>
public static class BattleDataTransfer
{
    // Player 데이터 변경 이벤트 (전투씬에서 UI 업데이트용)
    public static event Action<float, float> OnPlayerHealthChanged;
    public static event Action<float, float> OnPlayerPPChanged;
    public static event Action<int> OnPlayerLevelChanged;
    public static event Action<float, float> OnPlayerExpChanged;
    // Enemy 데이터
    public struct EnemyData
    {
        public float enemyHp;
        public float maxHp;
        public float enemyPp;
        public float maxPp;
        public int level;
        public float expReward;
        public Sprite portrait;
        public GameObject dropItemPrefab;
        public float dropChance;
    }

    // Player 데이터
    public struct PlayerData
    {
        public float playerHp;
        public float maxHp;
        public float playerPp;
        public float maxPp;
        public int level;
        public float currentExp;
        public Vector3 position;
    }

    private static EnemyData? storedEnemyData;
    private static PlayerData? storedPlayerData;
    private static bool isBattleActive = false;

    /// <summary>
    /// Enemy 데이터 저장
    /// </summary>
    public static void StoreEnemyData(Enemy enemy)
    {
        if (enemy == null) return;

        storedEnemyData = new EnemyData
        {
            enemyHp = enemy.enemyHp,
            maxHp = enemy.maxHp,
            enemyPp = enemy.enemyPp,
            maxPp = enemy.maxPp,
            level = enemy.level,
            expReward = enemy.expReward,
            portrait = enemy.portrait,
            dropItemPrefab = enemy.dropItemPrefab,
            dropChance = enemy.dropChance
        };
    }

    /// <summary>
    /// Player 데이터 저장
    /// </summary>
    public static void StorePlayerData(Player player)
    {
        if (player == null) return;

        storedPlayerData = new PlayerData
        {
            playerHp = player.playerHp,
            maxHp = player.maxHp,
            playerPp = player.playerPp,
            maxPp = player.maxMp,
            level = player.level,
            currentExp = player.currentExp,
            position = player.transform.position
        };
    }

    /// <summary>
    /// 저장된 Enemy 데이터 가져오기
    /// </summary>
    public static EnemyData? GetEnemyData()
    {
        return storedEnemyData;
    }

    /// <summary>
    /// 저장된 Player 데이터 가져오기
    /// </summary>
    public static PlayerData? GetPlayerData()
    {
        return storedPlayerData;
    }

    /// <summary>
    /// Enemy 데이터를 Enemy 컴포넌트에 적용
    /// </summary>
    public static void ApplyEnemyData(Enemy enemy, EnemyData data)
    {
        if (enemy == null) return;

        enemy.maxHp = data.maxHp;
        enemy.enemyHp = data.enemyHp;
        enemy.maxPp = data.maxPp;
        enemy.enemyPp = data.enemyPp;
        enemy.level = data.level;
        enemy.expReward = data.expReward;
        enemy.portrait = data.portrait;
        enemy.dropItemPrefab = data.dropItemPrefab;
        enemy.dropChance = data.dropChance;
    }

    /// <summary>
    /// Player 데이터를 Player 컴포넌트에 적용
    /// </summary>
    public static void ApplyPlayerData(Player player, PlayerData data)
    {
        if (player == null) return;

        player.maxHp = data.maxHp;
        player.playerHp = data.playerHp;
        player.maxMp = data.maxPp;
        player.playerPp = data.playerPp;
        player.level = data.level;
        player.currentExp = data.currentExp;
        player.transform.position = data.position;
    }

    /// <summary>
    /// Player 데이터 업데이트 (HP, PP 등)
    /// </summary>
    public static void UpdatePlayerData(float? hp = null, float? pp = null, float? exp = null)
    {
        if (!storedPlayerData.HasValue) return;

        var data = storedPlayerData.Value;
        bool hpChanged = hp.HasValue && data.playerHp != hp.Value;
        bool ppChanged = pp.HasValue && data.playerPp != pp.Value;
        bool expChanged = exp.HasValue && data.currentExp != exp.Value;
        
        if (hp.HasValue) data.playerHp = hp.Value;
        if (pp.HasValue) data.playerPp = pp.Value;
        if (exp.HasValue) data.currentExp = exp.Value;
        storedPlayerData = data;

        // 이벤트 발생 (전투씬에서 UI 업데이트용)
        if (hpChanged)
        {
            OnPlayerHealthChanged?.Invoke(data.playerHp, data.maxHp);
        }
        if (ppChanged)
        {
            OnPlayerPPChanged?.Invoke(data.playerPp, data.maxPp);
        }
        if (expChanged)
        {
            OnPlayerExpChanged?.Invoke(data.currentExp, data.maxHp); // maxExp는 별도로 관리 필요할 수 있음
        }
    }

    /// <summary>
    /// Player HP 가져오기
    /// </summary>
    public static float GetPlayerHp()
    {
        return storedPlayerData?.playerHp ?? 0f;
    }

    /// <summary>
    /// Player HP 설정
    /// </summary>
    public static void SetPlayerHp(float hp)
    {
        UpdatePlayerData(hp: hp);
    }

    /// <summary>
    /// 전투 활성화 상태 설정
    /// </summary>
    public static void SetBattleActive(bool active)
    {
        isBattleActive = active;
    }

    /// <summary>
    /// 전투 활성화 상태 확인
    /// </summary>
    public static bool IsBattleActive()
    {
        return isBattleActive;
    }

    /// <summary>
    /// 저장된 데이터 초기화
    /// </summary>
    public static void ClearData()
    {
        storedEnemyData = null;
        storedPlayerData = null;
        isBattleActive = false;
    }
}

