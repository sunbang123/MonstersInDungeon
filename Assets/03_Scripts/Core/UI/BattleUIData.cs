using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 전투 UI 데이터 모델 - Data-Driven UI 구조
/// </summary>
[Serializable]
public class BattleUIData
{
    [Header("Canvas")]
    public GameObject battleCanvas;

    [Header("Battle Log")]
    public TMPro.TextMeshProUGUI battleLog;

    [Header("Buttons")]
    public Button attackButton;
    public Button specialAttackButton;
    public Button defenseButton;
    public GameObject inventoryContainer;

    [Header("Player Status")]
    public GameObject playerStatus;

    [Header("Health Sliders")]
    public Slider playerHealthSlider;
    public Slider enemyHealthSlider;

    [Header("PP Sliders")]
    public Slider playerPPSlider;
    public Slider enemyPPSlider;

    [Header("Experience Slider")]
    public Slider playerExpSlider;

    [Header("Portraits")]
    public UnityEngine.UI.Image playerPortrait;
    public UnityEngine.UI.Image enemyPortrait;

    [Header("Level Text")]
    public TMPro.TextMeshProUGUI playerLevelText;
    public TMPro.TextMeshProUGUI enemyLevelText;
}

/// <summary>
/// 전투 상태 데이터 모델
/// </summary>
[Serializable]
public class BattleStateData
{
    public float playerHealth;
    public float playerMaxHealth;
    public float playerPP;
    public float playerMaxPP;
    public float playerExp;
    public float playerExpToNextLevel;
    public int playerLevel;
    public Sprite playerPortrait;

    public float enemyHealth;
    public float enemyMaxHealth;
    public float enemyPP;
    public float enemyMaxPP;
    public int enemyLevel;
    public Sprite enemyPortrait;

    public bool buttonsInteractable;
    public string battleLogText;
}

