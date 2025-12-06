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
        // UserDataManager가 초기화되었는지 확인
        if (UserDataManager.Instance == null)
        {
            Debug.LogError("UserDataManager가 초기화되지 않았습니다!");
            return;
        }
        // 저장된 데이터 존재 여부로 Continue 버튼 활성화
        bool hasPlayerData = UserDataManager.Instance.ExistsSavedData;
        ContinueButton.interactable = hasPlayerData;
        NewGameButton.interactable = true;
        OptionButton.interactable = true;

        Debug.Log($"로비 초기화: 저장된 데이터 존재 = {hasPlayerData}");
    }

    public void OnClickContinue()
    {
        if (UserDataManager.Instance == null)
        {
            Debug.LogError("UserDataManager를 찾을 수 없습니다!");
            return;
        }

        Debug.Log("Continue Game");
        LoadInGameScene();
    }

    public void OnClickNewGame()
    {
        if (UserDataManager.Instance == null)
        {
            Debug.LogError("UserDataManager를 찾을 수 없습니다!");
            return;
        }

        Debug.Log("NewGame 클릭됨");

        // StartNewGame 메서드로 데이터 초기화
        UserDataManager.Instance.StartNewGame();

        Debug.Log("NewGame 이후 TutorialEnd: " +
            UserDataManager.Instance.Get<UserPlayerStatusData>()?.TutorialEnd);

        LoadInGameScene();
    }

    public void OnClickOption()
    {
        Debug.Log("Open Options");
    }
    private void LoadInGameScene()
    {
        // Addressables 방식으로 씬 로드
        InGameSceneReference.LoadSceneAsync(UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
