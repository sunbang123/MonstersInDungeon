using UnityEngine;
using UnityEngine.SceneManagement;

// 만들 씬 3개 선언
public enum SceneType
{
    Title,
    Lobby,
    InGame,
    StoryMode,
}

// SingletonBehaviour 상속
public class SceneLoader : SingletonBehaviour<SceneLoader>
{
    public void LoadScene(SceneType sceneType)
    {
        Logger.Log($"{sceneType} scene loading...");

        Time.timeScale = 1f; // 일시정지 했을때 타임스케일이 0이 될 수도 있고
        // 게임 기획상 타임스케일이 1이 아닌 경우도 있을 수 있기 때문에
        // 씬을 로딩했을 때 타임스케일을 초기화 해줌.
        SceneManager.LoadScene(sceneType.ToString());
    }

    public void ReloadScene()
    {
        Logger.Log($"{SceneManager.GetActiveScene().name} scene loading...");

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 비동기로 로딩하는 함수
    public AsyncOperation LoadSceneAsync(SceneType sceneType)
    {
        Logger.Log($"{sceneType} Scene async loading...");
        Time.timeScale = 1f;

        return SceneManager.LoadSceneAsync(sceneType.ToString());
    }
}