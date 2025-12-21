using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TownUIManager : MonoBehaviour
{
    public TMP_Text mapText;
    public TMP_Text townText;
    private float fadeDuration = 1f; //페이드 인/아웃 시간
    private float showDuration = 1.5f; //표시 시간


    private Coroutine fadeRoutine;


    void Start()
    {
        Color c = townText.color;
        c.a = 0;
        townText.color = c;
    }

    public void SetTownName(string townName)
    {
        mapText.text = townName;
        townText.text = townName;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeText());
    }

    IEnumerator FadeText()
    {
        Color c = townText.color;

        // Fade In
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, t / fadeDuration);
            townText.color = c;
            yield return null;
        }
        c.a = 1f;
        townText.color = c;

        // 표시 시간
        yield return new WaitForSeconds(showDuration);

        // Fade Out
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / fadeDuration);
            townText.color = c;
            yield return null;
        }
        c.a = 0f;
        townText.color = c;
    }
}
