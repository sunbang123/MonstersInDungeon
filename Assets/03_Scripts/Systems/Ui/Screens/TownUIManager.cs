using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TownUIManager : MonoBehaviour
{
    // UI 데이터 모델
    private TownUIData uiData;
    private TownStateData stateData = new TownStateData();

    // 캐시된 UI 참조들
    private TMP_Text mapText;
    private TMP_Text townText;
    
    private float fadeDuration = 1f; //페이드 인/아웃 시간
    private float showDuration = 1.5f; //표시 시간

    private Coroutine fadeRoutine;

    // 마을 이름 변경 이벤트 (외부에서 호출 가능하도록 Action으로 변경)
    public static Action<string> OnTownNameChanged;

    // 프로퍼티: UI 요소들을 자동으로 찾아서 반환
    private TMP_Text MapText
    {
        get
        {
            if (mapText == null)
                mapText = UIHelper.FindComponentInChildren<TMP_Text>(transform, "MapText");
            return mapText;
        }
    }

    private TMP_Text TownText
    {
        get
        {
            if (townText == null)
                townText = UIHelper.FindComponentInChildren<TMP_Text>(transform, "TownText");
            return townText;
        }
    }

    void Start()
    {
        TMP_Text text = TownText;
        if (text != null)
        {
            Color c = text.color;
            c.a = 0;
            text.color = c;
        }

        // 마을 이름 변경 이벤트 구독
        OnTownNameChanged += SetTownName;
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        OnTownNameChanged -= SetTownName;
    }

    public void SetTownName(string townName)
    {
        TMP_Text mapTxt = MapText;
        TMP_Text townTxt = TownText;
        
        if (mapTxt != null)
            mapTxt.text = townName;
        if (townTxt != null)
            townTxt.text = townName;

        stateData.townName = townName;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeText());
    }

    IEnumerator FadeText()
    {
        TMP_Text text = TownText;
        if (text == null)
            yield break;

        stateData.isFading = true;
        Color c = text.color;

        // Fade In
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, t / fadeDuration);
            text.color = c;
            yield return null;
        }
        c.a = 1f;
        text.color = c;

        // 표시 시간
        yield return new WaitForSeconds(showDuration);

        // Fade Out
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / fadeDuration);
            text.color = c;
            yield return null;
        }
        c.a = 0f;
        text.color = c;
        
        stateData.isFading = false;
    }
}
