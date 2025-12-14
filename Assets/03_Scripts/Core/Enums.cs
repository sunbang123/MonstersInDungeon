/// <summary>
/// 게임에서 사용되는 열거형(Enum) 정의
/// </summary>
public static class Enums
{
    /// <summary>
    /// 게임 요소 타입 (불, 물, 식물)
    /// </summary>
    public enum ElementType
    {
        None,
        Fire,
        Water,
        Plant
    }

    /// <summary>
    /// 아이템 타입
    /// </summary>
    public enum ItemType
    {
        None,
        Consumable,
        Equipment,
        Key
    }

    /// <summary>
    /// 아이템 등급
    /// </summary>
    public enum ItemGrade
    {
        Common,
        Rare,
        Epic,
        Legendary
    }

    /// <summary>
    /// 맵 타입
    /// </summary>
    public enum MapType
    {
        Village,
        Forest,
        Cave,
        Castle
    }

    /// <summary>
    /// UI 패널 타입
    /// </summary>
    public enum UIPanelType
    {
        None,
        MainMenu,
        Battle,
        Inventory,
        Settings,
        Tutorial
    }

    /// <summary>
    /// 사운드 타입
    /// </summary>
    public enum SoundType
    {
        BGM,
        SFX,
        Voice
    }
}
