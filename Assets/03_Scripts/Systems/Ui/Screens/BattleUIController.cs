using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ���� UI�� �����ϴ� Ŭ����
/// </summary>
public class BattleUIController : MonoBehaviour
{
    [Header("Canvas References")]
    public GameObject battle_canvas;
    public GameObject nonbattle_canvas;
    public TextMeshProUGUI battle_log;

    [Header("UI Buttons")]
    public Button Atk_btn;
    public GameObject Inventory;
    public List<Button> item_btn;
    public Button specialAtk_btn;
    public Button defense_btn;

    [Header("Player Status")]
    public GameObject playerStatus;
    public Transform batTransform;
    public Transform nonBatTransform;

    [Header("Health Sliders")]
    public Slider playerHealthSlider;
    public Slider enemyHealthSlider;

    [Header("PP Sliders")]
    public Slider playerPPSlider;
    public Slider enemyPPSlider;

    [Header("Portraits")]
    public Image playerPortrait;
    public Image enemyPortrait;

    [Header("Level Text")]
    public TextMeshProUGUI playerLevelText;
    public TextMeshProUGUI enemyLevelText;

    // ��ư Ŭ�� �̺�Ʈ
    public event Action OnAttackClicked;
    public event Action<int> OnItemUseClicked;
    public event Action OnSpecialAttackClicked;
    public event Action OnDefenseClicked;

    private void Start()
    {
        GetInventoryButtons();
        BindButtons();

        // Player ã��
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.OnHealthChanged += UpdatePlayerHealthSlider;
            player.OnPortraitChanged += UpdatePlayerPortrait;
        }

        SetButtonsInteractable(false);
    }

    /// <summary>
    /// Inventory�� �ڽ� ��ҿ��� Button ������Ʈ�� ���� �͵��� ã�� ����Ʈ�� �߰�
    /// </summary>
    private void GetInventoryButtons()
    {
        item_btn.Clear();

        if (Inventory != null)
        {
            Button[] buttons = Inventory.GetComponentsInChildren<Button>();
            item_btn.AddRange(buttons);
        }
    }

    /// <summary>
    /// ��ư�� �̺�Ʈ ������ ���
    /// </summary>
    private void BindButtons()
    {
        if (Atk_btn != null)
            Atk_btn.onClick.AddListener(() => OnAttackClicked?.Invoke());

        if (item_btn != null && item_btn.Count > 0)
        {
            for (int i = 0; i < item_btn.Count; i++)
            {
                int index = i; // Ŭ���� ���� ����
                item_btn[i].onClick.AddListener(() => OnItemUseClicked?.Invoke(index));
            }
        }

        if (specialAtk_btn != null)
            specialAtk_btn.onClick.AddListener(() => OnSpecialAttackClicked?.Invoke());

        if (defense_btn != null)
            defense_btn.onClick.AddListener(() => OnDefenseClicked?.Invoke());
    }

    /// <summary>
    /// ��� ��ư�� ��ȣ�ۿ� ���� ���� ����
    /// </summary>
    public void SetButtonsInteractable(bool interactable)
    {
        if (Atk_btn != null)
            Atk_btn.interactable = interactable;

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


    /// <summary>
    /// ���� UI ���� ���
    /// </summary>
    private bool isBattleMode = false;

    public void ToggleBattleUI(bool enableBattle)
    {
        isBattleMode = enableBattle;

        battle_canvas.SetActive(enableBattle);
        nonbattle_canvas.SetActive(!enableBattle);

        Transform targetParent = enableBattle ? batTransform : nonBatTransform;
        bool useWorldSpace = !enableBattle;

        playerStatus.transform.SetParent(null, false);
        playerStatus.transform.SetParent(targetParent, useWorldSpace);

        // playerStatus�� rectTransform �ʱ�ȭ (�θ� ���� ����)
        RectTransform rectTransform = playerStatus.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    public void ShowBattleUI()
    {
        ToggleBattleUI(true);
    }

    public void HideBattleUI()
    {
        ToggleBattleUI(false);
    }

    /// <summary>
    /// ��Ʋ �α� �ؽ�Ʈ ����
    /// </summary>
    public void SetBattleLog(string text)
    {
        if (battle_log != null)
            battle_log.text = text;
    }
    public void UpdatePlayerHealthSlider(float currentHp, float maxHp)
    {
        if (playerHealthSlider != null)
        {
            playerHealthSlider.maxValue = maxHp;
            playerHealthSlider.value = currentHp;
        }
        var data = UserDataManager.Instance.Get<UserPlayerStatusData>();
        data.HP = currentHp;
    }
    public void UpdateEnemyHealthSlider(float currentHp, float maxHp)
    {
        if (enemyHealthSlider != null)
        {
            enemyHealthSlider.maxValue = maxHp;
            enemyHealthSlider.value = currentHp;
        }
    }

    public void UpdatePlayerPPSlider(float currentPp, float maxPp)
    {
        if (playerPPSlider != null)
        {
            playerPPSlider.maxValue = maxPp;
            playerPPSlider.value = currentPp;
        }
    }

    public void UpdateEnemyPPSlider(float currentPp, float maxPp)
    {
        if (enemyPPSlider != null)
        {
            enemyPPSlider.maxValue = maxPp;
            enemyPPSlider.value = currentPp;
        }
    }

    public void UpdatePlayerPortrait(Sprite portrait)
    {
        if (playerPortrait != null && portrait != null)
        {
            playerPortrait.sprite = portrait;
        }
    }

    public void UpdateEnemyPortrait(Sprite portrait)
    {
        if (enemyPortrait != null && portrait != null)
        {
            enemyPortrait.sprite = portrait;
        }
    }

    public void UpdatePlayerLevel(int level)
    {
        if (playerLevelText != null)
        {
            playerLevelText.text = $"Lv.{level}";
        }
    }

    public void UpdateEnemyLevel(int level)
    {
        if (enemyLevelText != null)
        {
            enemyLevelText.text = $"Lv.{level}";
        }
    }

    /// <summary>
    /// ��Ʋ �α׿� �ؽ�Ʈ �߰�
    /// </summary>
    public void AppendBattleLog(string text)
    {
        if (battle_log != null)
            battle_log.text += text;
    }

    private void OnDestroy()
    {
        // �̺�Ʈ ������ ����
        if (Atk_btn != null)
            Atk_btn.onClick.RemoveAllListeners();

        if (item_btn != null)
        {
            foreach (var btn in item_btn)
            {
                if (btn != null)
                    btn.onClick.RemoveAllListeners();
            }
        }

        if (specialAtk_btn != null)
            specialAtk_btn.onClick.RemoveAllListeners();

        if (defense_btn != null)
            defense_btn.onClick.RemoveAllListeners();
    }
}