using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Quest : TutorialBehaviour
{
    public GameObject questPanel;

    protected override IEnumerator Start()
    {
        // 부모 Start() 먼저 실행 (튜토리얼 시스템에서 조건 체크 후 Destroy를)
        yield return StartCoroutine(base.Start());

        // 부모 Destroy(gameObject) 했다면 여기서 종료
        if (this == null)
            yield break;

        // 버튼 이벤트 연결
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
