using System;
using UnityEngine;

public class BaseUIData
{
    // 함수를 담을 수 있는 변수라고 생각
    // 동일한 UI 화면에 대해서도 어떤 상황에서는 A라는 기능을 실행해줘야 하고
    // 어떤 상황에서는 B 라는 기능을 실행해줘야 할때가 있는데 그래서 Action으로
    public Action OnShow; // UI 화면을 열었을 때 해주고 싶은 행위를 정의
    public Action OnClose; // UI 화면을 닫으면서 실행해야 되는 기능 정의
}

public class BaseUI: MonoBehaviour
{
    // UI를 열어줄 때 재생할 애니메이션 변수
    public Animation m_UIOpenAnim;

    // 화면을 열 때 실행해야할 기능
    // 화면을 닫을 때 실행해야할 액션 변수 선언
    private Action m_OnShow;
    private Action m_OnClose;
    // 이 변수들은 화면을 열 때 매개변수로 넘어온 UIData 클래스에
    // 정의된 OnShow와 OnClose 그대로 BaseUI 클래스에 있는 m_OnShow와...
    // m_OnShow = uiData.OnShow; 이런식으로

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

        // 기본 값으로 전부 초기화
        rectTransform.localPosition = new Vector3(0f, 0f, 0f);
        rectTransform.localScale = new Vector3(1f, 1f, 1f);
        rectTransform.offsetMin = new Vector2(0, 0);
        rectTransform.offsetMax = new Vector2(0, 0);
    }

    // UI화면에 UI요소를 세팅해주는 함수
    public virtual void SetInfo(BaseUIData uiData)
    {
        Logger.Log($"{GetType()} set info");

        m_OnShow = uiData.OnShow;
        m_OnClose = uiData.OnClose;
    }

    // UI 화면을 실제로 열어서 화면에 표시해 주는 함수
    public virtual void ShowUI()
    {
        if(m_UIOpenAnim)
        {
            m_UIOpenAnim.Play();
        }

        m_OnShow?.Invoke(); // m_OnShow가 null이 아니라면 m_OnShow 실행
        m_OnShow = null; // 실행 후 널값으로 초기화
    }

    // 화면을 닫는 함수
    public virtual void CloseUI(bool isCloseAll = false)
    {
        // isCloseAll: 씬을 전환하거나 할 때 열려있는 화면을
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