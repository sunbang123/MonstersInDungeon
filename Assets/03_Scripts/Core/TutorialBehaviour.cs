using UnityEngine;

public class TutorialBehaviour : MonoBehaviour
{
    protected virtual void Start()
    {
        var status = UserDataManager.Instance.Get<UserPlayerStatusData>();

        if (status != null && status.TutorialEnd)
        {
            Destroy(gameObject);
        }
    }
}
