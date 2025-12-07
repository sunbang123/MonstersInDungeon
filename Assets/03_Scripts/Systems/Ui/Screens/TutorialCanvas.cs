using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialCanvas : TutorialBehaviour
{
    public float fadeDuration = 1f;

    private CanvasGroup cg;

    protected override IEnumerator Start()
    {
        // 부모 Start() 먼저 실행 (TutorialEnd 체크)
        yield return StartCoroutine(base.Start());

        // 부모가 Destroy(gameObject)를 했으면 여기서 종료
        if (this == null)
            yield break;

        // 1) 맨 위로 올리기
        transform.SetAsLastSibling();

        // 2) 페이드 인 준비
        cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        // 3) 페이드 인 시작
        yield return StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = t / fadeDuration;
            yield return null;
        }

        cg.alpha = 1f;
    }
}
