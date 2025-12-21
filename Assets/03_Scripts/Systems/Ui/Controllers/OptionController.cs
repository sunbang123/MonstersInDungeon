using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

enum OptionButton
{
    Save,
    ToLobby,
    Quit
}

public class OptionController : MonoBehaviour
{
    private List<Button> buttons = new List<Button>();

    [Header("Addressable Settings")]
    public AssetReference LobbySceneReference;

    private void Awake()
    {
        // 자식 GameObject들에서 Button 컴포넌트 찾기
        buttons.AddRange(GetComponentsInChildren<Button>(true));

        // enum 순서대로 버튼 할당
        for (int i = 0; i < buttons.Count; i++)
        {
            int index = i; // 클로저 문제 해결
            buttons[i].onClick.AddListener(() => HandleButton((OptionButton)index));
        }
    }

    private void HandleButton(OptionButton type)
    {
        switch (type)
        {
            case OptionButton.Save:
                Debug.Log("게임 저장 처리");
                SaveGame();
                break;

            case OptionButton.ToLobby:
                Debug.Log("로비로 이동");
                LoadLobbyScene();
                break;

            case OptionButton.Quit:
                Debug.Log("게임 종료");
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                break;
        }
    }

    private void SaveGame()
    {
        if (UserDataManager.Instance == null)
        {
            Debug.LogError("UserDataManager.Instance가 null입니다!");
            return;
        }

        var player = FindObjectOfType<Player>();
        if (player != null)
        {
            var data = UserDataManager.Instance.Get<UserPlayerStatusData>();
            if (data != null)
            {
                data.Position = player.transform.position;
                data.HP = player.playerHp;
                data.Level = player.level;
                data.CurrentExp = player.currentExp;
            }
        }

        // 현재 맵 인덱스 저장
        var mapManager = FindObjectOfType<MapManager>();
        if (mapManager != null)
        {
            var data = UserDataManager.Instance.Get<UserPlayerStatusData>();
            if (data != null)
            {
                data.CurrentMapIndex = mapManager.GetCurrentMapIndex();
                Debug.Log($"Saved current map index: {data.CurrentMapIndex}");
            }
        }

        // 인벤토리 데이터 저장
        if (InventoryManager.Instance != null)
        {
            var invData = UserDataManager.Instance.Get<UserInventoryData>();
            if (invData != null)
            {
                invData.ItemNames = InventoryManager.Instance.GetInventoryItemNames();
                Debug.Log($"Saved inventory: {invData.ItemNames.Count} items");
            }
        }

        UserDataManager.Instance.SaveUserData();
        Debug.Log("Saved HP: " + PlayerPrefs.GetFloat("PlayerHP"));
    }

    private void LoadLobbyScene()
    {
        // 현재 InGame 씬을 언로드하고, 로비 씬 로드
        // 이 방법으로 DontDestroyOnLoad 오브젝트를 보존합니다
        StartCoroutine(LoadLobbySceneCoroutine());
    }

    private System.Collections.IEnumerator LoadLobbySceneCoroutine()
    {
        // 1. 현재 씬 이름 저장
        var currentScene = SceneManager.GetActiveScene();
        Debug.Log($"현재 씬 언로드 시작: {currentScene.name}");

        // 2. 로비 씬 로드
        var loadHandle = Addressables.LoadSceneAsync(
            LobbySceneReference,
            LoadSceneMode.Single,
            activateOnLoad: true
        );

        yield return loadHandle;

        if (loadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log("로비 씬 로드 완료 (Additive)");

            // 3. 로비 씬을 활성화 상태로 설정
            var lobbyScene = loadHandle.Result.Scene;
            SceneManager.SetActiveScene(lobbyScene);

            // 4. 이전 InGame 씬 언로드
            var unloadOp = SceneManager.UnloadSceneAsync(currentScene);
            yield return unloadOp;

            Debug.Log("이전 씬 언로드 완료");

            // 5. UserDataManager 확인
            if (UserDataManager.Instance != null)
            {
                Debug.Log("UserDataManager 정상 작동 중!");
            }
            else
            {
                Debug.LogError("씬 전환 후에 UserDataManager가 사라졌습니다!");
            }
        }
        else
        {
            Debug.LogError($"로비 씬 로드 실패: {loadHandle.OperationException}");
        }
    }
}
