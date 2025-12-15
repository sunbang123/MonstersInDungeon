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
        // �ڽ� ������Ʈ���� Button �ڵ� ����
        buttons.AddRange(GetComponentsInChildren<Button>(true));

        // enum ������� ��� ����
        for (int i = 0; i < buttons.Count; i++)
        {
            int index = i; // Ŭ���� ���� ����
            buttons[i].onClick.AddListener(() => HandleButton((OptionButton)index));
        }
    }

    private void HandleButton(OptionButton type)
    {
        switch (type)
        {
            case OptionButton.Save:
                Debug.Log("���� ��� ����");
                SaveGame();
                break;

            case OptionButton.ToLobby:
                Debug.Log("�κ�� �̵�");
                LoadLobbyScene();
                break;

            case OptionButton.Quit:
                Debug.Log("���� ����");
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
            Debug.LogError("UserDataManager.Instance�� null�Դϴ�!");
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

        UserDataManager.Instance.SaveUserData();
        Debug.Log("Saved HP: " + PlayerPrefs.GetFloat("PlayerHP"));
    }

    private void LoadLobbyScene()
    {
        // ���� InGame ���� ��ε��ϰ�, �κ� ���� �ε�
        // �� ����� DontDestroyOnLoad ������Ʈ�� �����մϴ�
        StartCoroutine(LoadLobbySceneCoroutine());
    }

    private System.Collections.IEnumerator LoadLobbySceneCoroutine()
    {
        // 1. ���� �� �̸� ����
        var currentScene = SceneManager.GetActiveScene();
        Debug.Log($"���� �� ��ε� ����: {currentScene.name}");

        // 2. �κ� �� �ε�
        var loadHandle = Addressables.LoadSceneAsync(
            LobbySceneReference,
            LoadSceneMode.Single,
            activateOnLoad: true
        );

        yield return loadHandle;

        if (loadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log("�κ� �� �ε� �Ϸ� (Additive)");

            // 3. �� ���� Ȱ�� ������ ����
            var lobbyScene = loadHandle.Result.Scene;
            SceneManager.SetActiveScene(lobbyScene);

            // 4. ���� InGame �� ��ε�
            var unloadOp = SceneManager.UnloadSceneAsync(currentScene);
            yield return unloadOp;

            Debug.Log("���� �� ��ε� �Ϸ�");

            // 5. UserDataManager Ȯ��
            if (UserDataManager.Instance != null)
            {
                Debug.Log("UserDataManager ���� ������!");
            }
            else
            {
                Debug.LogError("�� ��ȯ �Ŀ��� UserDataManager�� ��������ϴ�!");
            }
        }
        else
        {
            Debug.LogError($"�κ� �� �ε� ����: {loadHandle.OperationException}");
        }
    }
}