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
        // 1) 로고 애니메이션
        LogoAnimator.Play(LogoAnimationStateName);
        yield return new WaitForSeconds(LogoAnimationDuration);

        LogoAnimator.gameObject.SetActive(false);
        Title.SetActive(true);

        // 2) 프리로드
        LoadingItemsTxt.text = "Loading Assets...";
        yield return StartCoroutine(PreloadAssetGroups());

        // 3) 로비 씬 로딩
        LoadingItemsTxt.text = "Loading Scene...";
        var handle = LobbySceneReference.LoadSceneAsync(LoadSceneMode.Single, false);

        while (!handle.IsDone)
        {
            LoadingSlider.value = handle.PercentComplete;
            LoadingProgressTxt.text = $"{(int)(LoadingSlider.value * 100)}%";
            yield return null;
        }

        // 4) 씬 활성화
        yield return handle.Result.ActivateAsync();

        // 5) 유저 데이터 적용
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

            LoadingItemsTxt.text = $"Loading {group.GroupName}...";

            // BuiltInData + Prefab 모두 로드
            var handle = Addressables.LoadAssetsAsync<object>(
                group.LabelReference.labelString,
                loadedAsset =>
                {
                    // Prefab만 풀에 등록
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