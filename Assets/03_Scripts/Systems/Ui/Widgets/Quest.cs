using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Quest : TutorialBehaviour
{
    public GameObject questPanel;

    protected override IEnumerator Start()
    {
        // 부모 Start() 먼저 실행 (튜토리얼 끝났으면 여기서 Destroy됨)
        yield return StartCoroutine(base.Start());

        // 부모가 Destroy(gameObject) 했다면 여기서 종료
        if (this == null)
            yield break;

        // 버튼 이벤트 등록
        var btn = GetComponent<Button>();
        if (btn != null)
            btn.onClick.AddListener(Toggle);
    }

    public void Toggle()
    {
        if (questPanel == null) return;

        questPanel.SetActive(!questPanel.activeSelf);
    }
}
