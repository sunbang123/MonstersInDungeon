using System.Collections.Generic;
using UnityEngine;

public class UIManager : SingletonBehaviour<UIManager>
{
    public Transform UICanvasTrs;
    public Transform ClosedUITrs;

    private BaseUI m_FrontUI; // UI화면에 가장 상단 UI인데 그걸 멤버변수로 쓰고있다.

    private Dictionary<System.Type, GameObject> m_OpenUIPool = new Dictionary<System.Type, GameObject>();
    private Dictionary<System.Type, GameObject> m_ClosedUIPool = new Dictionary<System.Type, GameObject>();

    // 여러가지 값이나 참조를 반환하고 싶을때 이렇게 out 매개변수 사용함.
    private BaseUI GetUI<T>(out bool isAlreadyOpen)
    {
        System.Type uiType = typeof(T); // T는 열고자 하는 화면 UI 클래스 타입. 이것을 uiType으로 받아온다.

        BaseUI ui = null;
        isAlreadyOpen = false;

        if(m_OpenUIPool.ContainsKey(uiType))
        {
            ui = m_OpenUIPool[uiType].GetComponent<BaseUI>();
            isAlreadyOpen = true;
        }
        else if(m_ClosedUIPool.ContainsKey(uiType))
        {
            ui = m_ClosedUIPool[uiType].GetComponent<BaseUI>();
            m_ClosedUIPool.Remove(uiType);
        }
        else
        {
            var uiObj = Instantiate(Resources.Load($"UI/{uiType}", typeof(GameObject))) as GameObject;

            ui = uiObj.GetComponent<BaseUI>();
        }

        return ui;
    }

    // UI 화면을 여는 기능 하는 함수
    public void OpenUI<T>(BaseUIData uiData)
    {
        System.Type uiType = typeof(T);

        Logger.Log($"{GetType()}::OpenUI({uiType})"); // 어떤 UI화면을 열고자하는지 로그를 찍는다.

        bool isAlreadyOpen = false; // 이미 열려있는지 알 수 있는 변수 선언

        var ui = GetUI<T>(out isAlreadyOpen);

        if(!ui) // 없으면 에러 로그
        {
            Logger.LogError($"{uiType} does not exist.");
            return;
        }

        if(isAlreadyOpen)
        {
            Logger.LogError($"{uiType} is already oepn.");
            return;
        }

        var siblingIdx = UICanvasTrs.childCount;
        ui.Init(UICanvasTrs);
        ui.transform.SetSiblingIndex(siblingIdx);

        ui.gameObject.SetActive(true);
        ui.SetInfo(uiData);
        ui.ShowUI();

        m_FrontUI = ui;
        m_OpenUIPool[uiType] = ui.gameObject;
    }

    // 화면닫는함수
    public void CloseUI(BaseUI ui)
    {
        System.Type uiType = ui.GetType();

        Logger.Log($"CloseUI UI:{uiType}"); // 어떤 UI인지 닫아주는로그

        ui.gameObject.SetActive(false);

        m_OpenUIPool.Remove(uiType); //오픈풀에서 제거
        m_ClosedUIPool[uiType] = ui.gameObject;
        ui.transform.SetParent(ClosedUITrs);

        m_FrontUI = null;

        var lastChild = UICanvasTrs.GetChild(UICanvasTrs.childCount - 1);

        if(lastChild)
        {
            m_FrontUI = lastChild.gameObject.GetComponent<BaseUI>();
        }
    }

    // 특정 UI 화면이 열려있는지 확인 그 열려있는 UI화면을 가져옴
    public BaseUI GetActiveUI<T>()
    {
        var uiType = typeof(T);

        return m_OpenUIPool.ContainsKey(uiType) ? m_OpenUIPool[uiType].GetComponent<BaseUI>() : null;
    }

    // UI화면이 열린것이 하나라도 있는지 확인하는 함수
    public bool ExistsOpenUI()
    {
        return m_FrontUI != null; // m_FrontUI가 null인지 아닌지 확인해서 그 불값을 반환
    }

    // 현재 가장 최상단에 있는 인스턴스를 리턴하는 함수
    public BaseUI GetCurrentFrontUI()
    {
        return m_FrontUI;
    }

    // 가장 최상단에 있는 UI화면 인스턴스를 닫는 함수
    public void CloseCurrFrontUI()
    {
        m_FrontUI.CloseUI();
    }

    public void CloseAllOpenUI()
    {
        while(m_FrontUI)
        {
            m_FrontUI.CloseUI(true);
        }
    }
}