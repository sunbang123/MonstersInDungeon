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

[System.Serializable]
public class AddressableGroup
{
    public string GroupName;
    public AssetLabelReference LabelReference;
}

public class TitleManager : MonoBehaviour
{
    // 로고
    public Animator LogoAnimator;
    public TextMeshProUGUI LogoTxt;

    [Header("Animation Settings")]
    public string LogoAnimationStateName = "LogoAnimation";
    public float LogoAnimationDuration = 0.7f;

    [Header("Addressable Settings")]
    public AssetReference LobbySceneReference; // Addressable 시스템 사용 권장

    [Header("Addressable Groups to Load")]
    public List<AddressableGroup> GroupsToLoad = new List<AddressableGroup>();

    [Header("Loading Settings")]
    [Tooltip("각 에셋 그룹 로딩 사이의 딜레이 시간 (초)")]
    public float DelayBetweenGroups = 0.8f;
    [Tooltip("각 에셋 로딩 후 딜레이 시간 (초)")]
    public float DelayPerAsset = 0.1f;
    [Tooltip("최소 에셋 로딩 시간 (초) - 실제 로딩이 빨라도 이 시간만큼은 보장")]
    public float MinAssetLoadingDuration = 3.0f;

    // 타이틀
    public GameObject Title;
    public Slider LoadingSlider;
    public TextMeshProUGUI LoadingProgressTxt;
    public TextMeshProUGUI LoadingItemsTxt;

    private AsyncOperationHandle<SceneInstance> m_SceneLoadHandle;
    private List<AsyncOperationHandle> m_PreloadHandles = new List<AsyncOperationHandle>();
    private List<MapData> m_LoadedMapData = new List<MapData>(); // 로드된 MapData 임시 저장

    // 로딩 UI 업데이트 이벤트
    public event Action<string> OnLoadingTextChanged;
    public event Action<float> OnLoadingProgressChanged;
    public event Action<bool> OnLogoVisibilityChanged;
    public event Action<bool> OnTitleVisibilityChanged;

    private void Awake()
    {
        // 이벤트 구독
        OnLogoVisibilityChanged += UpdateLogoVisibility;
        OnTitleVisibilityChanged += UpdateTitleVisibility;
        OnLoadingTextChanged += UpdateLoadingText;
        OnLoadingProgressChanged += UpdateLoadingProgress;

        OnLogoVisibilityChanged?.Invoke(true);
        OnTitleVisibilityChanged?.Invoke(false);
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        OnLogoVisibilityChanged -= UpdateLogoVisibility;
        OnTitleVisibilityChanged -= UpdateTitleVisibility;
        OnLoadingTextChanged -= UpdateLoadingText;
        OnLoadingProgressChanged -= UpdateLoadingProgress;
        // 에셋프리로드 해제 처리
        foreach (var handle in m_PreloadHandles)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }
        m_PreloadHandles.Clear();

        // 씬 언로드
        if (m_SceneLoadHandle.IsValid())
        {
            Addressables.UnloadSceneAsync(m_SceneLoadHandle);
        }
    }

    // UI 업데이트 핸들러
    private void UpdateLogoVisibility(bool isVisible)
    {
        if (LogoAnimator != null)
            LogoAnimator.gameObject.SetActive(isVisible);
    }

    private void UpdateTitleVisibility(bool isVisible)
    {
        if (Title != null)
            Title.SetActive(isVisible);
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

    private void Start()
    {
        StartCoroutine(LoadGameCo());
    }

    private IEnumerator LoadGameCo()
    {
        // 1) 로고 애니메이션
        LogoAnimator.Play(LogoAnimationStateName);
        yield return new WaitForSeconds(LogoAnimationDuration);

        OnLogoVisibilityChanged?.Invoke(false);
        OnTitleVisibilityChanged?.Invoke(true);

        // 2) 에셋프리로드
        OnLoadingTextChanged?.Invoke("Loading Assets...");
        yield return StartCoroutine(PreloadAssetGroups());

        // 3) 로비 씬 로딩
        LoadingItemsTxt.text = "Loading Scene...";
        var handle = LobbySceneReference.LoadSceneAsync(LoadSceneMode.Single, false);

        while (!handle.IsDone)
        {
            OnLoadingProgressChanged?.Invoke(handle.PercentComplete);
            yield return null;
        }

        // 4) 씬 활성화
        yield return handle.Result.ActivateAsync();

        // 5) 저장 데이터 로드
        UserDataManager.Instance.LoadUserData();
        ApplyLoadedUserData();
        
        // 6) MapManager가 생성되었을 수 있으므로 MapData 다시 전달 시도
        if (MapManager.Instance != null && m_LoadedMapData.Count > 0)
        {
            foreach (var mapData in m_LoadedMapData)
            {
                MapManager.Instance.AddMapData(mapData);
            }
        }
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

        // 인벤토리는 InventoryManager의 Start()에서 자동으로 로드됩니다.
        // (InventoryManager는 InGame 씬에 있으므로 여기서는 로드하지 않습니다)
    }

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
        
        // 모든 그룹의 로드된 에셋 정보를 저장할 리스트
        List<(string groupName, string assetName)> allLoadedAssets = new List<(string, string)>();

        foreach (var group in GroupsToLoad)
        {
            if (group?.LabelReference == null)
                continue;

            OnLoadingTextChanged?.Invoke($"Loading {group.GroupName}...");

            // BuiltInData + Prefab 모두 로드
            string labelString = group.LabelReference.labelString;
            Debug.Log($"[TitleManager] Loading assets with label: {labelString}");
            
            var handle = Addressables.LoadAssetsAsync<object>(
                labelString,
                loadedAsset =>
                {
                    // Prefab만 풀에 등록
                    if (loadedAsset is GameObject go)
                    {
                        assetName = go.name;
                        Debug.Log($"[TitleManager] Loaded GameObject: {assetName}");
                        GameManager.Instance.RegisterPrefab(group.GroupName, go);

                        if (go.TryGetComponent<Item>(out var item))
                            ItemDatabase.Register(item.IData);
                    }
                    // MapData를 임시 저장 (나중에 MapManager에 전달)
                    else if (loadedAsset is MapData mapData)
                    {
                        assetName = mapData != null ? mapData.name : "MapData";
                        Debug.Log($"[TitleManager] Loaded MapData: {assetName}");
                        if (mapData != null && !m_LoadedMapData.Contains(mapData))
                        {
                            m_LoadedMapData.Add(mapData);
                        }
                    }
                    else if (loadedAsset != null)
                    {
                        assetName = loadedAsset.GetType().Name;
                        Debug.Log($"[TitleManager] Loaded Asset: {assetName} (Type: {loadedAsset.GetType().Name})");
                    }
                    
                    // 로드된 에셋 정보를 리스트에 추가
                    allLoadedAssets.Add((group.GroupName, assetName));
                }
            );
            
            Debug.Log($"[TitleManager] Started loading {labelString}, total assets in list: {allLoadedAssets.Count}");

            m_PreloadHandles.Add(handle);
            yield return handle;
        }

        LoadingItemsTxt.text = "All Assets Loaded!";
    }


    private void OnDestroy()
    {
        // 프리로드된 에셋 해제
        foreach (var handle in m_PreloadHandles)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }
        m_PreloadHandles.Clear();

        // 씬 언로드
        if (m_SceneLoadHandle.IsValid())
        {
            Addressables.UnloadSceneAsync(m_SceneLoadHandle);
        }
    }
}
