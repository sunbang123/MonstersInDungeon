using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using DG.Tweening;

public class LobbyManager : MonoBehaviour
{
    public Image Logo;
    public GameObject LobbyPanel;
    public Button ContinueButton;
    public Button NewGameButton;
    public Button FeedbackButton;

    [Header("Animation Settings")]
    [Tooltip("페이드 인 시간 (초)")]
    public float FadeInDuration = 0.5f;
    [Tooltip("확대축소 애니메이션 시간 (초)")]
    public float ScaleDuration = 0.5f;
    [Tooltip("확대축소 시작 스케일")]
    public float StartScale = 0.8f;
    [Tooltip("버튼들 사이의 딜레이 시간 (초)")]
    public float ButtonDelay = 0.1f;

    public AssetReference InGameSceneReference;

    private void Start()
    {
        // UserDataManager가 초기화되었는지 확인
        if (UserDataManager.Instance == null)
        {
            Debug.LogError("UserDataManager가 초기화되지 않았습니다!");
            return;
        }
        // 저장된 데이터가 있으면 Continue 버튼 활성화
        bool hasPlayerData = UserDataManager.Instance.ExistsSavedData;
        ContinueButton.interactable = hasPlayerData;
        NewGameButton.interactable = true;

        Debug.Log($"로비 초기화: 저장된 데이터 존재 = {hasPlayerData}");

        // 애니메이션 시작
        PlayFadeInAnimations();
    }

    private void PlayFadeInAnimations()
    {
        // Logo 페이드 인 및 확대
        if (Logo != null)
        {
            Logo.color = new Color(Logo.color.r, Logo.color.g, Logo.color.b, 0f);
            Logo.transform.localScale = Vector3.one * StartScale;
            
            Sequence logoSequence = DOTween.Sequence();
            logoSequence.Append(Logo.DOFade(1f, FadeInDuration));
            logoSequence.Join(Logo.transform.DOScale(Vector3.one, ScaleDuration).SetEase(Ease.OutBack));
        }

        // LobbyPanel 페이드 인 및 확대
        if (LobbyPanel != null)
        {
            CanvasGroup panelCanvasGroup = LobbyPanel.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = LobbyPanel.AddComponent<CanvasGroup>();
            }
            
            panelCanvasGroup.alpha = 0f;
            LobbyPanel.transform.localScale = Vector3.one * StartScale;
            
            Sequence panelSequence = DOTween.Sequence();
            panelSequence.Append(panelCanvasGroup.DOFade(1f, FadeInDuration));
            panelSequence.Join(LobbyPanel.transform.DOScale(Vector3.one, ScaleDuration).SetEase(Ease.OutBack));
        }

        // 버튼들 순차적으로 페이드 인 및 확대
        float delay = 0f;
        
        if (ContinueButton != null)
        {
            AnimateButton(ContinueButton.gameObject, delay);
            delay += ButtonDelay;
        }
        
        if (NewGameButton != null)
        {
            AnimateButton(NewGameButton.gameObject, delay);
            delay += ButtonDelay;
        }
        
        if (FeedbackButton != null)
        {
            AnimateButton(FeedbackButton.gameObject, delay);
        }
    }

    private void AnimateButton(GameObject button, float delay)
    {
        if (button == null) return;

        CanvasGroup buttonCanvasGroup = button.GetComponent<CanvasGroup>();
        if (buttonCanvasGroup == null)
        {
            buttonCanvasGroup = button.AddComponent<CanvasGroup>();
        }

        buttonCanvasGroup.alpha = 0f;
        button.transform.localScale = Vector3.one * StartScale;

        Sequence buttonSequence = DOTween.Sequence();
        buttonSequence.SetDelay(delay);
        buttonSequence.Append(buttonCanvasGroup.DOFade(1f, FadeInDuration));
        buttonSequence.Join(button.transform.DOScale(Vector3.one, ScaleDuration).SetEase(Ease.OutBack));
    }

    public void OnClickContinue()
    {
        if (UserDataManager.Instance == null)
        {
            Debug.LogError("UserDataManager를 찾을 수 없습니다!");
            return;
        }

        Debug.Log("Continue Game");
        // 계속하기 전에 저장된 데이터를 다시 로드
        UserDataManager.Instance.LoadUserData();
        LoadInGameScene();
    }

    public void OnClickNewGame()
    {
        if (UserDataManager.Instance == null)
        {
            Debug.LogError("UserDataManager를 찾을 수 없습니다!");
            return;
        }

        Debug.Log("NewGame 클릭됨");

        // StartNewGame 메서드로 데이터 초기화
        UserDataManager.Instance.StartNewGame();

        // 선택된 요소(SelectedElement)를 초기화 해서 세이브 데이터 코드에서도 새로 초기화되도록 함
        var status = UserDataManager.Instance.Get<UserPlayerStatusData>();
        if (status != null)
        {
            status.SelectedElement = "";
            PlayerPrefs.SetString("SelectedElement", "");
            PlayerPrefs.Save();
        }

        Debug.Log("NewGame 후 TutorialEnd: " +
            UserDataManager.Instance.Get<UserPlayerStatusData>()?.TutorialEnd);

        LoadInGameScene();
    }

    public void OnClickOption()
    {
        Debug.Log("Open Options");
    }
    private void LoadInGameScene()
    {
        // Addressables 시스템으로 씬 로드
        InGameSceneReference.LoadSceneAsync(UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
