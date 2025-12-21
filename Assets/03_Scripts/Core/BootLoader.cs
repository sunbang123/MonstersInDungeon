using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class BootLoader : MonoBehaviour
{
    public AssetReference TitleSceneReference;

    private IEnumerator Start()
    {
        // Addressables 초기화
        yield return Addressables.InitializeAsync();

        // 다음 TitleScene 로드
        var handle = TitleSceneReference.LoadSceneAsync(LoadSceneMode.Single);
        yield return handle;
    }
}
