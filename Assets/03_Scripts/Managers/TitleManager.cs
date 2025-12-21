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

    // 타이틀
    public GameObject Title;
    public Slider LoadingSlider;
    public TextMeshProUGUI LoadingProgressTxt;
    public TextMeshProUGUI LoadingItemsTxt;

    private AsyncOperationHandle<SceneInstance> m_SceneLoadHandle;
    private List<AsyncOperationHandle> m_PreloadHandles = new List<AsyncOperationHandle>();

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

        // 3) 로비 씬 로드
        OnLoadingTextChanged?.Invoke("Loading Scene...");
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

        var invData = UserDataManager.Instance.Get<UserInventoryData>();
        InventoryManager.Instance.LoadInventory(invData.ItemNames);

        InventoryManager.Instance.LoadInventory(invData.ItemNames);
    }

    private IEnumerator PreloadAssetGroups()
    {
        foreach (var group in GroupsToLoad)
        {
            if (group?.LabelReference == null)
                continue;

            OnLoadingTextChanged?.Invoke($"Loading {group.GroupName}...");

            // BuiltInData + Prefab 모두 로드
            var handle = Addressables.LoadAssetsAsync<object>(
                group.LabelReference.labelString,
                loadedAsset =>
                {
                    // Prefab을 등록하는 로직
                    if (loadedAsset is GameObject go)
                    {
                        GameManager.Instance.RegisterPrefab(group.GroupName, go);

                        if (go.TryGetComponent<Item>(out var item))
                            ItemDatabase.Register(item.IData);
                    }
                }
            );

            m_PreloadHandles.Add(handle);
            yield return handle;
        }

        OnLoadingTextChanged?.Invoke("All Assets Loaded!");
    }
}
