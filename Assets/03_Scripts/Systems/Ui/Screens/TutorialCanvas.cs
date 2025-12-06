using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialCanvas : TutorialBehaviour
{
    public float fadeDuration = 1f;

    private CanvasGroup cg;

    private void Start()
    {
        base.Start();

        if(this == null) return;

        // 1)맨 위로 올리기
        transform.SetAsLastSibling();

        // 2) 페이드 인 준비
        cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        // 3) 페이드 인 시작
        StartCoroutine(FadeIn());
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
