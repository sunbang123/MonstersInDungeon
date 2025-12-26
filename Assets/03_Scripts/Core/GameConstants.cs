using UnityEngine;

/// <summary>
/// 게임 전반에서 사용되는 상수 값들을 정의하는 클래스
/// </summary>
public static class GameConstants
{
    // ========== 태그 상수 ==========
    public const string TAG_PLAYER = "Player";
    public const string TAG_HIDE = "Hide";
    public const string TAG_ITEM = "Item";

    // ========== 경로 상수 ==========
    public const string PATH_AUDIO = "Audio";
    public const string PATH_POISON_EFFECT_PREFAB = "poisonEffect.prefab";
    public const string PATH_POISON_EFFECT_FULL = "Assets/05_Prefabs/Effect/poisonEffect.prefab";
    
    // ========== 맵 관련 상수 ==========
    public const string MAP_SCENE_PREFIX = "Map";
    public const string MAP_SCENE_FORMAT = "Map{0:D2}";
    public const int DEFAULT_MAP_INDEX = 0;
    public const int MAX_FALLBACK_MAP_INDICES = 5; // 맵 로드 실패 시 시도할 최대 맵 인덱스 개수

    // ========== 전투 관련 상수 ==========
    [System.Serializable]
    public class Battle
    {
        // 대기 시간 (초)
        public const float TURN_DELAY = 1.0f;
        public const float BATTLE_START_DELAY = 1.0f;
        public const float BATTLE_END_DELAY = 1.0f;
        public const float ENEMY_DEFEAT_MESSAGE_DELAY = 1.0f;

        // 기본 데미지 (임시 값 - 나중에 데이터 기반으로 변경 가능)
        public const float DEFAULT_ENEMY_ATTACK_DAMAGE = 20f;
        public const float DEFAULT_PLAYER_ATTACK_DAMAGE = 30f;
        public const float DEFAULT_SPECIAL_ATTACK_DAMAGE = 50f;
    }

    // ========== 플레이어 관련 상수 ==========
    [System.Serializable]
    public class Player
    {
        // 경험치 계산 공식 상수
        public const float BASE_EXP_REQUIREMENT = 100f;
        public const float EXP_MULTIPLIER = 1.2f;
        
        // 기본 스탯
        public const float DEFAULT_MAX_HP = 500f;
        public const float DEFAULT_MAX_MP = 100f;
        public const int DEFAULT_LEVEL = 1;
        public const float DEFAULT_EXP = 0f;
    }

    // ========== 적 관련 상수 ==========
    [System.Serializable]
    public class Enemy
    {
        // 기본 스탯
        public const float DEFAULT_MAX_HP = 50f;
        public const float DEFAULT_MAX_PP = 50f;
        public const float DEFAULT_EXP_REWARD = 50f;
        public const int DEFAULT_LEVEL = 1;
        
        // 아이템 드롭
        public const float DEFAULT_DROP_CHANCE = 1f;
    }

    // ========== 독 영역 관련 상수 ==========
    [System.Serializable]
    public class PoisonArea
    {
        public const float DEFAULT_POISON_DAMAGE = 10.0f;
        public const float DEFAULT_DAMAGE_INTERVAL = 1.0f;
        public const float DEFAULT_RECOVERY_DURATION = 2.0f;
        public const float DEFAULT_EFFECT_FADE_DURATION = 0.3f;
        public const float DEFAULT_FLASH_DURATION = 0.2f;
        public const float DEFAULT_ANIMATION_DURATION_FALLBACK = 0.5f;
        
        // 색상 상수
        public static readonly Color DEFAULT_FLASH_COLOR = new Color(0.5f, 0f, 1f, 0.3f);
        public static readonly Color DEFAULT_POISON_COLOR = new Color(0.7f, 0.2f, 1f, 1f);
        
        // 색상 거리 임계값
        public const float COLOR_DISTANCE_THRESHOLD = 0.1f;
    }

    // ========== 인벤토리 관련 상수 ==========
    [System.Serializable]
    public class Inventory
    {
        public const int DEFAULT_MAX_SLOTS = 20;
        public const int DEFAULT_BATTLE_MAX_SLOTS = 20;
    }

    // ========== 오디오 관련 상수 ==========
    [System.Serializable]
    public class Audio
    {
        public const float DEFAULT_VOLUME = 1f;
        public const float MUTED_VOLUME = 0f;
    }

    // ========== UI 관련 상수 ==========
    [System.Serializable]
    public class UI
    {
        public const string BUTTON_TEXT_WALK = "달리기";
        public const string BUTTON_TEXT_RUN = "걷기";
        public const string BUTTON_TEXT_HIDE = "숨기기";
        public const string BUTTON_TEXT_SHOW = "나타나기";
    }

    // ========== 맵 로드 관련 상수 ==========
    [System.Serializable]
    public class MapLoading
    {
        // 맵 로드 실패 시 시도할 맵 인덱스 목록 (기본값)
        public static readonly int[] FALLBACK_MAP_INDICES = { 0, 1, 2, 3, 4 };
    }
}

