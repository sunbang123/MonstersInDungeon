using UnityEngine;

public class ObstacleTrigger : TutorialBehaviour
{
    public GameObject m_AlertPannel;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (m_AlertPannel != null && !m_AlertPannel.activeSelf)
            {
                m_AlertPannel.SetActive(true);
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
            }
        }
    }
}
