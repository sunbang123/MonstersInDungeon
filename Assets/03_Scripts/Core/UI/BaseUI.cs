using System;
using UnityEngine;

public class BaseUIData
{
    // 액션으로 받아서 전달할 수 있는 데이터를 저장
    // 예를 들어 UI 화면에 따라서 어떤 상황에서 A라는 처리를 해야하고
    // 어떤 상황에서 B 라는 처리를 해야하는데 그것을 Action으로
    // 어떤 상황에서 B 라는 처리를 해야하는데 그것을 Action으로
    public Action OnShow; // UI 화면이 열릴 때 보여지고 전달할 데이터를 저장
    public Action OnClose; // UI 화면이 닫히면서 처리해야 할 데이터 저장
}

public class BaseUI: MonoBehaviour
{
    // 캐시된 Animation 참조
    private Animation uiOpenAnim;

    // 화면이 열 때 처리해야할 데이터
    // 화면이 닫을 때 처리해야할 액션 데이터 저장
    private Action m_OnShow;
    private Action m_OnClose;

    /// <summary>
    /// UI가 열릴 때 재생할 애니메이션 프로퍼티 - 자동으로 찾아서 반환
    /// </summary>
    protected Animation UIOpenAnim
    {
        get
        {
            if (uiOpenAnim == null)
                uiOpenAnim = GetComponent<Animation>();
            return uiOpenAnim;
        }
    }
    // 각 화면에서 화면이 열 때 매개변수로 전달된 UIData 클래스를
    // 저장하고 있는 OnShow와 OnClose 바로 BaseUI 클래스에 있는 m_OnShow와...
    // m_OnShow = uiData.OnShow; 이렇게

    public virtual void Init(Transform anchor)
    {
        Logger.Log($"{GetType()} init.");

        m_OnShow = null;
        m_OnClose = null;

        transform.SetParent(anchor);

        var rectTransform = GetComponent<RectTransform>();
        if(!rectTransform)
        {
            Logger.LogError("UI does not have rectransform.");
            return;
        }

        // 기본 위치를 설정 초기화
        rectTransform.localPosition = new Vector3(0f, 0f, 0f);
        rectTransform.localScale = new Vector3(1f, 1f, 1f);
        rectTransform.offsetMin = new Vector2(0, 0);
        rectTransform.offsetMax = new Vector2(0, 0);
    }

    // UI화면에 UI요소를 설정하는 함수
    public virtual void SetInfo(BaseUIData uiData)
    {
        Logger.Log($"{GetType()} set info");

        m_OnShow = uiData.OnShow;
        m_OnClose = uiData.OnClose;
    }

    // UI 화면이 열릴 때 처리해서 화면에 표시하는 함수
    public virtual void ShowUI()
    {
        Animation anim = UIOpenAnim;
        if(anim != null)
        {
            anim.Play();
        }

        m_OnShow?.Invoke(); // m_OnShow가 null이 아니면 m_OnShow 호출
        m_OnShow = null; // 호출 후 변수를 초기화
    }

    // 화면을 닫는 함수
    public virtual void CloseUI(bool isCloseAll = false)
    {
        // isCloseAll: 모든 화면을 닫거나 한 개만 닫는 화면을
        if(!isCloseAll)
        {
            m_OnClose?.Invoke();
        }
        m_OnClose = null;
    }

    public virtual void OnClickCloseButton()
    {
        AudioManager.Instance.PlaySFX(SFX.ui_button_click);
        CloseUI();
    }
}
