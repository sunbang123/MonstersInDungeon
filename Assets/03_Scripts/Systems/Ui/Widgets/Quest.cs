using UnityEngine;
using UnityEngine.UI;

public class Quest : TutorialBehaviour
{
    public GameObject questPanel;

    private void Start()
    {
        base.Start();
        this.GetComponent<Button>().onClick.AddListener(Toggle);
    }

    public void Toggle()
    {
        if (questPanel == null) return;

        questPanel.SetActive(!questPanel.activeSelf);
    }
}
