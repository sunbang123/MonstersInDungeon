using System.Collections;
using UnityEngine;

public class TutorialBehaviour : MonoBehaviour
{
    protected virtual IEnumerator Start()
    {
        // UserDataManager.Instance가 준비될 때까지 기다림
        while (UserDataManager.Instance == null)
            yield return null;

        var status = UserDataManager.Instance.Get<UserPlayerStatusData>();

        if (status == null)
        {
            Debug.LogError("TutorialBehaviour: UserPlayerStatusData is NULL");
            yield break;
        }

        Debug.Log($"TutorialBehaviour Start - TutorialEnd: {status.TutorialEnd}");

        if (status.TutorialEnd)
        {
            Debug.Log("TutorialBehaviour: 튜토리얼 끝 상태라 오브젝트 파괴");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("TutorialBehaviour: 튜토리얼 진행 중이라 오브젝트 유지");
        }
    }
}
