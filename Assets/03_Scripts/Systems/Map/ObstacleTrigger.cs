using System.Collections;
using UnityEngine;

public class ObstacleTrigger : TutorialBehaviour
{
    public GameObject m_AlertPannel;

    protected override IEnumerator Start()
    {
        // 부모 Start() 먼저 실행 (TutorialEnd 체크)
        yield return StartCoroutine(base.Start());

        // 부모 Destroy(gameObject)를 했으면 여기서 종료
        if (this == null)
            yield break;

        // 디버그: AlertPanel 참조 확인
        if (m_AlertPannel == null)
        {
            Logger.LogWarning("ObstacleTrigger: AlertPanel 참조가 null입니다. Inspector에서 설정해주세요.");
        }
        else
        {
            Logger.Log($"ObstacleTrigger: AlertPanel 참조 확인됨 - {m_AlertPannel.name}");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (m_AlertPannel != null)
            {
                if (!m_AlertPannel.activeSelf)
                {
                    m_AlertPannel.SetActive(true);
                    Logger.Log($"ObstacleTrigger: AlertPanel 활성화 - {m_AlertPannel.name}");
                }
            }
            else
            {
                Logger.LogWarning("ObstacleTrigger: AlertPanel이 null입니다. OnTriggerEnter2D 호출됨");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (m_AlertPannel != null && m_AlertPannel.activeSelf)
            {
                m_AlertPannel.SetActive(false);
                Logger.Log($"ObstacleTrigger: AlertPanel 비활성화 - {m_AlertPannel.name}");
            }
        }
    }
}
