using System.Collections;
using UnityEngine;

public class TutorialBehaviour : MonoBehaviour
{
    protected virtual IEnumerator Start()
    {
        // UserDataManager.Instance가 준비될 때까지 대기
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
            Debug.Log("TutorialBehaviour: 튜토리얼이 끝났다면 오브젝트 삭제");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("TutorialBehaviour: 튜토리얼이 진행 중이라면 오브젝트 유지");
        }
    }
}
