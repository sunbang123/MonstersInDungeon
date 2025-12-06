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
using static UnityEditor.Progress;

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
    public AssetReference LobbySceneReference; // Addressable 씬을 직접 드래그

    [Header("Addressable Groups to Load")]
    public List<AddressableGroup> GroupsToLoad = new List<AddressableGroup>();

    // 타이틀
    public GameObject Title;
    public Slider LoadingSlider;
    public TextMeshProUGUI LoadingProgressTxt;
    public TextMeshProUGUI LoadingItemsTxt;

    private AsyncOperationHandle<SceneInstance> m_SceneLoadHandle;
    private List<AsyncOperationHandle> m_PreloadHandles = new List<AsyncOperationHandle>();

    private void Awake()
    {
        LogoAnimator.gameObject.SetActive(true);
        Title.SetActive(false);
    }

    private void Start()
    {
        StartCoroutine(LoadGameCo());
    }

    private IEnumerator LoadGameCo()
    {
        Logger.Log($"{GetType()}::LoadGameCo - Starting logo animation");

        // 로고 애니메이션 재생 (Animator State 확인!)
        if (!string.IsNullOrEmpty(LogoAnimationStateName))
        {
            LogoAnimator.Play(LogoAnimationStateName);
        }
        else
        {
            Logger.LogWarning("LogoAnimationStateName is empty!");
        }

        // 로고 애니메이션 동안 대기
        yield return new WaitForSeconds(LogoAnimationDuration);

        // 로고 숨기고 타이틀 화면 표시
        LogoAnimator.gameObject.SetActive(false);
        Title.SetActive(true);

        Logger.Log("Starting asset preload");

        // 1단계: 그룹별로 에셋 프리로드
        if (GroupsToLoad.Count > 0)
        {
            yield return StartCoroutine(PreloadAssetGroups());
        }
        else
        {
            Logger.LogWarning("No addressable groups to load!");
        }

        Logger.Log("Starting scene load");

        // 2단계: 로비 씬 로딩
        LoadingItemsTxt.text = "Loading Scene...";

        m_SceneLoadHandle = LobbySceneReference.LoadSceneAsync(
            LoadSceneMode.Single,
            false
        );

        if (!m_SceneLoadHandle.IsValid())
        {
            Logger.Log("Lobby addressable loading error.");
            yield break;
        }

        // 씬 로딩 진행률 표시
        while (!m_SceneLoadHandle.IsDone)
        {
            float progress = m_SceneLoadHandle.PercentComplete;

            // 전체 로딩에서 씬 로딩은 마지막 10%로 계산
            float totalProgress = 0.9f + (progress * 0.1f);
            LoadingSlider.value = totalProgress;
            LoadingProgressTxt.text = $"{(int)(totalProgress * 100)}%";

            yield return null;
        }

        // 로딩 완료
        LoadingSlider.value = 1f;
        LoadingProgressTxt.text = "100%";
        LoadingItemsTxt.text = "Complete!";

        Logger.Log("All assets loaded. Activating scene...");

        yield return new WaitForSeconds(0.3f);

        // 씬 활성화
        yield return m_SceneLoadHandle.Result.ActivateAsync();
        // 저장된 데이터 로드
        UserDataManager.Instance.LoadUserData();

        // 게임 오브젝트에 적용
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
        // GameManager 존재 확인
        if (GameManager.Instance == null)
        {
            Logger.LogError("GameManager.Instance is null! Make sure GameManager exists in the scene.");
            yield break;
        }

        int totalGroups = GroupsToLoad.Count;
        int currentGroup = 0;

        foreach (var group in GroupsToLoad)
        {
            // null 체크
            if (group == null)
            {
                Logger.LogWarning("Null group found in GroupsToLoad!");
                continue;
            }

            if (group.LabelReference == null)
            {
                Logger.LogWarning($"LabelReference is null for group: {group.GroupName}");
                continue;
            }

            // Scenes 그룹은 GameObject가 아니므로 스킵
            if (group.GroupName.ToLower().Contains("scene"))
            {
                Logger.Log($"Skipping scene group: {group.GroupName} (Scenes are loaded separately)");
                continue;
            }

            currentGroup++;
            LoadingItemsTxt.text = $"Loading {group.GroupName}...";
            Logger.Log($"Preloading group: {group.GroupName}");

            // 그룹의 레이블로 에셋 로딩 (labelString 사용!)
            var handle = Addressables.LoadAssetsAsync<GameObject>(
                group.LabelReference.labelString,  // ← 이게 핵심!
                (loadedAsset) =>
                {
                    // 로드된 에셋을 GameManager의 풀에 저장
                    if (loadedAsset != null)
                    {
                        GameManager.Instance.RegisterPrefab(group.GroupName, loadedAsset);
                        // ItemDatabase 등록 (여기가 정확한 위치)
                        Item item = loadedAsset.GetComponent<Item>();
                        if (item != null)
                            ItemDatabase.Register(item.IData);
                    Logger.Log($"Registered to pool [{group.GroupName}]: {loadedAsset.name}");
                    }
                }
            );

            m_PreloadHandles.Add(handle);

            // 현재 그룹 로딩 완료 대기
            yield return handle;

            // 진행률 업데이트 (에셋 로딩은 전체의 90%)
            float progress = ((float)currentGroup / totalGroups) * 0.9f;
            LoadingSlider.value = progress;
            LoadingProgressTxt.text = $"{(int)(progress * 100)}%";

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Logger.Log($"Successfully loaded {handle.Result.Count} assets for group: {group.GroupName}");
                LoadingItemsTxt.text = $"{group.GroupName} Loaded! ({handle.Result.Count} items)";
                yield return new WaitForSeconds(0.2f); // 사용자가 메시지를 볼 수 있도록
            }
            else
            {
                Logger.LogError($"Failed to load assets for group: {group.GroupName}");
                LoadingItemsTxt.text = $"Failed to load {group.GroupName}";
            }
        }

        LoadingItemsTxt.text = "All Assets Loaded!";
        yield return new WaitForSeconds(0.3f);
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