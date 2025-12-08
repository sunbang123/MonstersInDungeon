using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class BootLoader : MonoBehaviour
{
    [SerializeField] private AssetReference titleScene;

    private IEnumerator Start()
    {
        // 매니저 Awake() → Init() 실행될 시간 확보
        yield return null;

        // TitleScene 로드
        var handle = titleScene.LoadSceneAsync(UnityEngine.SceneManagement.LoadSceneMode.Single);
        yield return handle;
    }
}