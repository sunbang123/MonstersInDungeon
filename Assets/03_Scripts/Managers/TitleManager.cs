using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

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

    // 타이틀
    public GameObject Title;
    public Slider LoadingSlider;
    public TextMeshProUGUI LoadingProgressTxt;
    public TextMeshProUGUI LoadingItemsTxt;

    private AsyncOperationHandle<SceneInstance> m_SceneLoadHandle;

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

        // 로고 애니메이션 재생
        LogoAnimator.Play(LogoAnimationStateName);

        // 로고 애니메이션 동안 대기
        yield return new WaitForSeconds(LogoAnimationDuration);

        // 로고 숨기고 타이틀 화면 표시
        LogoAnimator.gameObject.SetActive(false);
        Title.SetActive(true);

        Logger.Log("Starting Addressable scene load");

        // 로고가 끝난 후 Addressable 로딩 시작
        m_SceneLoadHandle = LobbySceneReference.LoadSceneAsync(
            LoadSceneMode.Single,
            false // activateOnLoad = false (씬 자동 활성화 비활성화)
        );

        if (!m_SceneLoadHandle.IsValid())
        {
            Logger.Log("Lobby addressable loading error.");
            yield break;
        }

        /*
         * Addressable 로딩 진행률을 로딩바로 표시
         * 로딩이 이미 완료되었을 수도 있고, 아직 진행 중일 수도 있음
         */
        while (!m_SceneLoadHandle.IsDone)
        {
            // Addressable 다운로드/로드 진행률 (0.0 ~ 1.0)
            float progress = m_SceneLoadHandle.PercentComplete;

            // 로딩 슬라이더 업데이트
            LoadingSlider.value = progress;
            LoadingProgressTxt.text = $"{(int)(progress * 100)}%";

            yield return null;
        }

        // 로딩 완료 - 100% 표시
        LoadingSlider.value = 1f;
        LoadingProgressTxt.text = "100%";

        Logger.Log("Addressable scene loaded. Activating scene...");

        // 약간의 딜레이 후 씬 활성화 (100% 표시를 보여주기 위함)
        yield return new WaitForSeconds(0.3f);

        // 씬 활성화 (로비 씬으로 전환)
        yield return m_SceneLoadHandle.Result.ActivateAsync();
    }

    private void OnDestroy()
    {
        // 씬 언로드
        if (m_SceneLoadHandle.IsValid())
        {
            Addressables.UnloadSceneAsync(m_SceneLoadHandle);
        }
    }
}