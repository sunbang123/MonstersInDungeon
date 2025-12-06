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
        // PlayerData 존재 여부 확인
        bool hasPlayerData = PlayerPrefs.HasKey("PlayerStatus");

        ContinueButton.interactable = hasPlayerData;
        NewGameButton.interactable = true;
        OptionButton.interactable = true;
    }

    public void OnClickContinue()
    {
        // 이미 TitleManager가 데이터 로드 후 씬을 활성화했으므로
        // Continue는 그냥 게임 시작
        Debug.Log("Continue Game");
        LoadInGameScene();
    }

    public void OnClickNewGame()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("New Game Start");
        // 새 게임 로직 실행
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
