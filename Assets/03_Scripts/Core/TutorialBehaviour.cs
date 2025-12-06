using UnityEngine;

public class TutorialBehaviour : MonoBehaviour
{
    protected virtual void Start()
    {
        var status = UserDataManager.Instance.Get<UserPlayerStatusData>();

        if (status == null)
        {
            Debug.LogError("TutorialBehaviour: UserPlayerStatusData is NULL");
            return;
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
