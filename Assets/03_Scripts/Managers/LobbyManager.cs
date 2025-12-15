using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public Button ContinueButton;
    public Button NewGameButton;
    public Button OptionButton;

    public AssetReference InGameSceneReference;

    private void Start()
    {
        // UserDataManager�� �ʱ�ȭ�Ǿ����� Ȯ��
        if (UserDataManager.Instance == null)
        {
            Debug.LogError("UserDataManager�� �ʱ�ȭ���� �ʾҽ��ϴ�!");
            return;
        }
        // ����� ������ ���� ���η� Continue ��ư Ȱ��ȭ
        bool hasPlayerData = UserDataManager.Instance.ExistsSavedData;
        ContinueButton.interactable = hasPlayerData;
        NewGameButton.interactable = true;
        OptionButton.interactable = true;

        Debug.Log($"�κ� �ʱ�ȭ: ����� ������ ���� = {hasPlayerData}");
    }

    public void OnClickContinue()
    {
        if (UserDataManager.Instance == null)
        {
            Debug.LogError("UserDataManager�� ã�� �� �����ϴ�!");
            return;
        }

        Debug.Log("Continue Game");
        LoadInGameScene();
    }

    public void OnClickNewGame()
    {
        if (UserDataManager.Instance == null)
        {
            Debug.LogError("UserDataManager�� ã�� �� �����ϴ�!");
            return;
        }

        Debug.Log("NewGame Ŭ����");

        // StartNewGame �޼���� ������ �ʱ�ȭ
        UserDataManager.Instance.StartNewGame();

        // ���õ� ������(SelectedElement)�� �ʱ�ȭ �Ͽ� �޸��ε� �ڵ��� ���� �ʱ�ȭ�� �� �ֵ���
        var status = UserDataManager.Instance.Get<UserPlayerStatusData>();
        if (status != null)
        {
            status.SelectedElement = "";
            PlayerPrefs.SetString("SelectedElement", "");
            PlayerPrefs.Save();
        }

        Debug.Log("NewGame ���� TutorialEnd: " +
            UserDataManager.Instance.Get<UserPlayerStatusData>()?.TutorialEnd);

        LoadInGameScene();
    }

    public void OnClickOption()
    {
        Debug.Log("Open Options");
    }
    private void LoadInGameScene()
    {
        // Addressables ������� �� �ε�
        InGameSceneReference.LoadSceneAsync(UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
