using UnityEngine;
using UnityEngine.AddressableAssets;

public class BridgeTrigger : TutorialBehaviour
{
    public AssetReference InGameStorySceneReference;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            LoadStoryScene();
        }
    }
    private void LoadStoryScene()
    {
        InGameStorySceneReference.LoadSceneAsync(UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
