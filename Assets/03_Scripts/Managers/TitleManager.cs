using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using DG.Tweening;

[System.Serializable]
public class AddressableGroup
{
    public string GroupName;
    public AssetLabelReference LabelReference;
}

public class TitleManager : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("UI References")]
    public Image LogoImg;
    public Animator LogoAnimator;
    public TextMeshProUGUI LogoTxt;
    public GameObject Title;
    public Slider LoadingSlider;
    public TextMeshProUGUI LoadingProgressTxt;
    public TextMeshProUGUI LoadingItemsTxt;
    
    [Header("Camera Settings")]
    [Tooltip("배경 카메라 (없으면 Main Camera 사용)")]
    public Camera BackgroundCamera;

    [Header("Animation Settings")]
    public string LogoAnimationStateName = "LogoAnimation";
    public float LogoAnimationDuration = 0.2f;
    [Tooltip("로고 페이드 인 시간 (초)")]
    public float LogoFadeInDuration = 0.5f;
    [Tooltip("로고 확대축소 애니메이션 시간 (초)")]
    public float LogoScaleDuration = 0.5f;
    [Tooltip("로고 시작 스케일")]
    public float LogoStartScale = 0.5f;
    [Tooltip("로고 페이드 아웃 시간 (초)")]
    public float LogoFadeOutDuration = 0.5f;
    [Tooltip("배경 카메라 페이드 시간 (초)")]
    public float BackgroundFadeDuration = 0.5f;
    [Tooltip("Title 자식 요소 페이드 인 시간 (초)")]
    public float TitleFadeInDuration = 0.5f;

    [Header("Addressable Settings")]
    public AssetReference LobbySceneReference;

    [Header("Addressable Groups to Load")]
    public List<AddressableGroup> GroupsToLoad = new List<AddressableGroup>();

    [Header("Loading Settings")]
    [Tooltip("각 에셋 그룹 로딩 사이의 딜레이 시간 (초)")]
    public float DelayBetweenGroups = 0.8f;
    [Tooltip("각 에셋 로딩 후 딜레이 시간 (초)")]
    public float DelayPerAsset = 0.1f;
    [Tooltip("최소 에셋 로딩 시간 (초) - 실제 로딩이 빨라도 이 시간만큼은 보장")]
    public float MinAssetLoadingDuration = 3.0f;

    #endregion

    #region Private Fields

    private AsyncOperationHandle<SceneInstance> m_SceneLoadHandle;
    private List<AsyncOperationHandle> m_PreloadHandles = new List<AsyncOperationHandle>();
    private List<MapData> m_LoadedMapData = new List<MapData>();

    private Camera m_BackgroundCamera;
    private Color m_OriginalBackgroundColor;
    private CanvasGroup m_LogoCanvasGroup;
    private Color m_OriginalLogoTextColor;

    #endregion

    #region Events

    public event Action<string> OnLoadingTextChanged;
    public event Action<float> OnLoadingProgressChanged;
    public event Action<bool> OnLogoVisibilityChanged;
    public event Action<bool> OnTitleVisibilityChanged;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeEvents();
        InitializeCamera();
        InitializeLogo();
    }

    private void Start()
    {
        StartCoroutine(LoadGameCo());
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
        ReleaseAddressables();
    }

    #endregion

    #region Initialization

    private void InitializeEvents()
    {
        OnLogoVisibilityChanged += UpdateLogoVisibility;
        OnTitleVisibilityChanged += UpdateTitleVisibility;
        OnLoadingTextChanged += UpdateLoadingText;
        OnLoadingProgressChanged += UpdateLoadingProgress;
    }

    private void UnsubscribeEvents()
    {
        OnLogoVisibilityChanged -= UpdateLogoVisibility;
        OnTitleVisibilityChanged -= UpdateTitleVisibility;
        OnLoadingTextChanged -= UpdateLoadingText;
        OnLoadingProgressChanged -= UpdateLoadingProgress;
    }

    private void InitializeCamera()
    {
        m_BackgroundCamera = BackgroundCamera != null ? BackgroundCamera : Camera.main;
        
        if (m_BackgroundCamera != null)
        {
            m_OriginalBackgroundColor = m_BackgroundCamera.backgroundColor;
        }
    }

    private void InitializeLogo()
    {
        // 로고 이미지 초기 상태 설정
        if (LogoImg != null)
        {
            LogoImg.color = new Color(LogoImg.color.r, LogoImg.color.g, LogoImg.color.b, 0f);
            LogoImg.transform.localScale = Vector3.one * LogoStartScale;
        }

        // 로고 CanvasGroup 설정
        if (LogoAnimator != null)
        {
            m_LogoCanvasGroup = LogoAnimator.GetComponent<CanvasGroup>();
            if (m_LogoCanvasGroup == null)
            {
                m_LogoCanvasGroup = LogoAnimator.gameObject.AddComponent<CanvasGroup>();
            }
            m_LogoCanvasGroup.alpha = 1f;
        }

        // 로고 텍스트 초기 상태 설정
        if (LogoTxt != null)
        {
            m_OriginalLogoTextColor = LogoTxt.color;
            Color textColor = m_OriginalLogoTextColor;
            textColor.a = 0f;
            LogoTxt.color = textColor;
            LogoTxt.transform.localScale = Vector3.one * LogoStartScale;
        }

        OnLogoVisibilityChanged?.Invoke(true);
        OnTitleVisibilityChanged?.Invoke(false);
    }

    private void ReleaseAddressables()
    {
        foreach (var handle in m_PreloadHandles)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }
        m_PreloadHandles.Clear();

        if (m_SceneLoadHandle.IsValid())
        {
            Addressables.UnloadSceneAsync(m_SceneLoadHandle);
        }
    }

    #endregion

    #region UI Update Handlers

    private void UpdateLogoVisibility(bool isVisible)
    {
        if (LogoAnimator != null)
            LogoAnimator.gameObject.SetActive(isVisible);
    }

    private void UpdateTitleVisibility(bool isVisible)
    {
        if (Title != null)
        {
            Title.SetActive(isVisible);
            
            if (isVisible)
            {
                SetChildrenAlphaToZero(Title);
            }
        }
    }

    private void SetChildrenAlphaToZero(GameObject parent)
    {
        if (parent == null)
            return;

        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.gameObject == parent)
                continue;

            CanvasGroup canvasGroup = child.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                continue;
            }

            TextMeshProUGUI textMesh = child.GetComponent<TextMeshProUGUI>();
            if (textMesh != null)
            {
                Color textColor = textMesh.color;
                textColor.a = 0f;
                textMesh.color = textColor;
                continue;
            }

            Image image = child.GetComponent<Image>();
            if (image != null)
            {
                Color imageColor = image.color;
                imageColor.a = 0f;
                image.color = imageColor;
                continue;
            }
        }
    }

    private void UpdateLoadingText(string text)
    {
        if (LoadingItemsTxt != null)
            LoadingItemsTxt.text = text;
    }

    private void UpdateLoadingProgress(float progress)
    {
        if (LoadingSlider != null)
        {
            LoadingSlider.value = progress;
            if (LoadingProgressTxt != null)
                LoadingProgressTxt.text = $"{(int)(progress * 100)}%";
        }
    }

    #endregion

    #region Main Game Load Flow

    private IEnumerator LoadGameCo()
    {
        // 1) 로고 애니메이션
        yield return StartCoroutine(PlayLogoSequence());

        // 2) Title 표시
        OnLogoVisibilityChanged?.Invoke(false);
        OnTitleVisibilityChanged?.Invoke(true);
        
        if (Title != null)
        {
            yield return StartCoroutine(Utils.FadeInChildrenUI(this, Title, TitleFadeInDuration));
        }

        // 3) 에셋 프리로드
        OnLoadingTextChanged?.Invoke("Loading Assets...");
        yield return StartCoroutine(PreloadAssetGroups());
        
        // 4) MapData 전달
        TransferMapDataToMapManager();

        // 5) 로비 씬 로드
        OnLoadingTextChanged?.Invoke("Loading Scene...");
        yield return StartCoroutine(LoadLobbyScene());

        // 6) 저장 데이터 로드
        UserDataManager.Instance.LoadUserData();
        ApplyLoadedUserData();
        
        // 7) MapData 다시 전달 시도 (씬 로드 후 MapManager가 생성되었을 수 있음)
        TransferMapDataToMapManager();
    }

    private IEnumerator PlayLogoSequence()
    {
        // 로고 페이드 인 및 확대
        PlayLogoFadeInAnimation();
        yield return new WaitForSeconds(Mathf.Max(LogoFadeInDuration, LogoScaleDuration));
        
        // 로고 애니메이션
        if (LogoAnimator != null)
        {
            LogoAnimator.Play(LogoAnimationStateName);
            yield return new WaitForSeconds(LogoAnimationDuration);
        }

        // 로고 페이드 아웃 및 배경 어둡게
        yield return StartCoroutine(FadeOutLogoAndBackground());
    }

    private void TransferMapDataToMapManager()
    {
        if (MapManager.Instance != null && m_LoadedMapData.Count > 0)
        {
            foreach (var mapData in m_LoadedMapData)
            {
                MapManager.Instance.AddMapData(mapData);
            }
        }
    }

    private IEnumerator LoadLobbyScene()
    {
        var handle = LobbySceneReference.LoadSceneAsync(LoadSceneMode.Single, false);

        while (!handle.IsDone)
        {
            OnLoadingProgressChanged?.Invoke(handle.PercentComplete);
            yield return null;
        }

        yield return handle.Result.ActivateAsync();
    }

    private void ApplyLoadedUserData()
    {
        var status = UserDataManager.Instance.Get<UserPlayerStatusData>();
        var player = FindObjectOfType<Player>();

        if (player != null)
        {
            player.playerHp = status.HP;
            player.transform.position = status.Position;
        }
    }

    #endregion

    #region Asset Loading

    private IEnumerator PreloadAssetGroups()
    {
        if (GroupsToLoad.Count == 0)
        {
            OnLoadingProgressChanged?.Invoke(0.7f);
            yield break;
        }

        float startTime = Time.time;
        int totalGroups = GroupsToLoad.Count;
        int currentGroupIndex = 0;
        List<(string groupName, string assetName)> allLoadedAssets = new List<(string, string)>();

        foreach (var group in GroupsToLoad)
        {
            if (group?.LabelReference == null)
                continue;

            OnLoadingTextChanged?.Invoke($"Loading {group.GroupName}...");
            string labelString = group.LabelReference.labelString;
            Debug.Log($"[TitleManager] Loading assets with label: {labelString}");
            
            // GameObject 로드
            yield return StartCoroutine(LoadGameObjectsAsync(
                labelString, 
                group.GroupName, 
                allLoadedAssets, 
                startTime, 
                currentGroupIndex, 
                totalGroups
            ));

            // ScriptableObject 로드
            yield return StartCoroutine(LoadScriptableObjectsAsync(
                labelString, 
                group.GroupName, 
                allLoadedAssets, 
                startTime, 
                currentGroupIndex, 
                totalGroups
            ));
            
            Debug.Log($"[TitleManager] Finished loading {labelString}, total assets in list: {allLoadedAssets.Count}");
            
            currentGroupIndex++;
            
            if (currentGroupIndex < totalGroups)
            {
                yield return new WaitForSeconds(DelayBetweenGroups);
            }
        }
        
        // 에셋 표시 애니메이션
        yield return StartCoroutine(DisplayLoadedAssets(allLoadedAssets, startTime));

        OnLoadingTextChanged?.Invoke("All Assets Loaded!");
        OnLoadingProgressChanged?.Invoke(0.7f);
    }

    private IEnumerator LoadGameObjectsAsync(
        string labelString, 
        string groupName, 
        List<(string, string)> allLoadedAssets, 
        float startTime, 
        int currentGroupIndex, 
        int totalGroups)
    {
        var handle = Addressables.LoadAssetsAsync<GameObject>(
            labelString,
            loadedAsset =>
            {
                if (loadedAsset == null) return;

                string assetName = loadedAsset.name;
                Debug.Log($"[TitleManager] Loaded GameObject: {assetName}");
                GameManager.Instance.RegisterPrefab(groupName, loadedAsset);

                if (loadedAsset.TryGetComponent<Item>(out var item))
                {
                    ItemDatabase.Register(item.IData);
                }
                
                allLoadedAssets.Add((groupName, assetName));
            }
        );

        m_PreloadHandles.Add(handle);

        while (!handle.IsDone)
        {
            float actualProgress = handle.PercentComplete * 0.5f; // GameObject는 50% 가중치
            UpdateLoadingProgressForGroup(startTime, currentGroupIndex, totalGroups, actualProgress);
            yield return null;
        }
        
        if (handle.Status == AsyncOperationStatus.Failed)
        {
            string errorMsg = handle.OperationException?.Message ?? "Unknown error";
            Debug.LogWarning($"[TitleManager] Failed to load GameObjects with label '{labelString}': {errorMsg}");
        }
    }

    private IEnumerator LoadScriptableObjectsAsync(
        string labelString, 
        string groupName, 
        List<(string, string)> allLoadedAssets, 
        float startTime, 
        int currentGroupIndex, 
        int totalGroups)
    {
        // ✅ 1단계: 해당 타입의 리소스가 있는지 먼저 확인 (메타데이터만 확인, 실제 로드 X)
        var checkHandle = Addressables.LoadResourceLocationsAsync(labelString, typeof(ScriptableObject));
        
        yield return checkHandle;
        
        // ✅ 2단계: ScriptableObject가 없으면 스킵
        if (checkHandle.Status != AsyncOperationStatus.Succeeded || checkHandle.Result == null || checkHandle.Result.Count == 0)
        {
            Debug.Log($"[TitleManager] No ScriptableObjects in label '{labelString}' (skipping)");
            Addressables.Release(checkHandle);
            
            // 진행률 업데이트
            float elapsedTime = Time.time - startTime;
            float timeBasedProgress = Mathf.Clamp01(elapsedTime / MinAssetLoadingDuration);
            float groupProgress = (currentGroupIndex + 1.0f) / totalGroups;
            float targetProgress = groupProgress * 0.7f;
            float displayProgress = Mathf.Min(targetProgress, timeBasedProgress * 0.7f);
            OnLoadingProgressChanged?.Invoke(displayProgress);
            
            yield break;
        }
        
        Addressables.Release(checkHandle);
        
        // ✅ 3단계: ScriptableObject가 있을 때만 실제 로드
        var handle = Addressables.LoadAssetsAsync<ScriptableObject>(
            labelString,
            loadedAsset =>
            {
                if (loadedAsset == null) return;

                if (loadedAsset is MapData mapData)
                {
                    string assetName = mapData.name;
                    Debug.Log($"[TitleManager] Loaded MapData: {assetName}");
                    
                    if (!m_LoadedMapData.Contains(mapData))
                    {
                        m_LoadedMapData.Add(mapData);
                    }
                    
                    allLoadedAssets.Add((groupName, assetName));
                }
                else
                {
                    string assetName = loadedAsset.name;
                    Debug.Log($"[TitleManager] Loaded ScriptableObject: {assetName} (Type: {loadedAsset.GetType().Name})");
                    allLoadedAssets.Add((groupName, assetName));
                }
            }
        );
        
        m_PreloadHandles.Add(handle);
        
        while (!handle.IsDone)
        {
            float actualProgress = 0.5f + (handle.PercentComplete * 0.5f); // 50% + ScriptableObject 진행률
            UpdateLoadingProgressForGroup(startTime, currentGroupIndex, totalGroups, actualProgress);
            yield return null;
        }
        
        if (handle.Status == AsyncOperationStatus.Failed)
        {
            Debug.Log($"[TitleManager] Failed to load ScriptableObjects for label: {labelString}");
        }
        else if (handle.Result != null && handle.Result.Count == 0)
        {
            Debug.Log($"[TitleManager] No ScriptableObjects in label: {labelString}");
        }
    }

    private IEnumerator DisplayLoadedAssets(List<(string groupName, string assetName)> allLoadedAssets, float startTime)
    {
        int displayedAssetIndex = 0;
        
        while (displayedAssetIndex < allLoadedAssets.Count || (Time.time - startTime) < MinAssetLoadingDuration)
        {
            float elapsedTime = Time.time - startTime;
            
            if (displayedAssetIndex < allLoadedAssets.Count)
            {
                var (groupName, assetName) = allLoadedAssets[displayedAssetIndex];
                OnLoadingTextChanged?.Invoke($"Loading {groupName}...\n{assetName}");
                displayedAssetIndex++;
                
                if (DelayPerAsset > 0)
                {
                    yield return new WaitForSeconds(DelayPerAsset);
                }
            }
            
            float timeBasedProgress = Mathf.Clamp01(elapsedTime / MinAssetLoadingDuration);
            float assetDisplayProgress = allLoadedAssets.Count > 0 
                ? (float)displayedAssetIndex / allLoadedAssets.Count 
                : 1f;
            float displayProgress = Mathf.Min(assetDisplayProgress, timeBasedProgress) * 0.7f;
            OnLoadingProgressChanged?.Invoke(displayProgress);
            
            yield return null;
        }
    }

    private void UpdateLoadingProgressForGroup(float startTime, int currentGroupIndex, int totalGroups, float actualProgress)
    {
        float elapsedTime = Time.time - startTime;
        float timeBasedProgress = Mathf.Clamp01(elapsedTime / MinAssetLoadingDuration);
        float groupProgress = (currentGroupIndex + actualProgress) / totalGroups;
        float targetProgress = groupProgress * 0.7f;
        float displayProgress = Mathf.Min(targetProgress, timeBasedProgress * 0.7f);
        OnLoadingProgressChanged?.Invoke(displayProgress);
    }

    #endregion

    #region Logo Animations

    private void PlayLogoFadeInAnimation()
    {
        if (LogoImg != null)
        {
            Sequence logoImageSequence = DOTween.Sequence();
            logoImageSequence.Append(LogoImg.DOFade(1f, LogoFadeInDuration));
            logoImageSequence.Join(LogoImg.transform.DOScale(Vector3.one, LogoScaleDuration).SetEase(Ease.OutBack));
        }

        if (LogoTxt != null)
        {
            Sequence textSequence = DOTween.Sequence();
            textSequence.Append(LogoTxt.DOFade(1f, LogoFadeInDuration));
            textSequence.Join(LogoTxt.transform.DOScale(Vector3.one, LogoScaleDuration).SetEase(Ease.OutBack));
        }
    }

    private IEnumerator FadeOutLogoAndBackground()
    {
        Color targetBackgroundColor = CalculateTargetBackgroundColor();

        var fadeCoroutines = new List<Coroutine>();

        if (LogoImg != null)
        {
            fadeCoroutines.Add(StartCoroutine(Utils.FadeImage(LogoImg, 0f, LogoFadeOutDuration)));
        }

        if (LogoTxt != null)
        {
            fadeCoroutines.Add(StartCoroutine(Utils.FadeTextMeshProUGUI(LogoTxt, 0f, LogoFadeOutDuration)));
        }

        if (m_BackgroundCamera != null)
        {
            fadeCoroutines.Add(StartCoroutine(Utils.FadeCameraBackground(m_BackgroundCamera, targetBackgroundColor, BackgroundFadeDuration)));
        }

        float maxDuration = Mathf.Max(LogoFadeOutDuration, BackgroundFadeDuration);
        yield return new WaitForSeconds(maxDuration);
    }

    private Color CalculateTargetBackgroundColor()
    {
        if (m_BackgroundCamera != null)
        {
            Color startBackgroundColor = m_BackgroundCamera.backgroundColor;
            return new Color(
                startBackgroundColor.r * 0.2f,
                startBackgroundColor.g * 0.2f,
                startBackgroundColor.b * 0.2f,
                startBackgroundColor.a
            );
        }
        return Color.white;
    }

    #endregion
}
