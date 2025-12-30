using System.Collections.Generic;
using UnityEngine;

public class UIManager : SingletonBehaviour<UIManager>
{
    // 캐시된 Transform 참조
    private Transform uiCanvasTrs;
    private Transform closedUITrs;

    private BaseUI m_FrontUI; // UI화면에 보이는 최상위 UI인데 그것이 무엇인지 저장하고 있는 변수.

    private Dictionary<System.Type, GameObject> m_OpenUIPool = new Dictionary<System.Type, GameObject>();
    private Dictionary<System.Type, GameObject> m_ClosedUIPool = new Dictionary<System.Type, GameObject>();

    /// <summary>
    /// UI Canvas Transform 프로퍼티 - 자동으로 찾아서 반환
    /// </summary>
    private Transform UICanvasTrs
    {
        get
        {
            if (uiCanvasTrs == null)
            {
                // Canvas 찾기
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                    uiCanvasTrs = canvas.transform;
                else
                {
                    // Canvas가 없으면 새로 생성
                    GameObject canvasObj = new GameObject("UICanvas");
                    Canvas newCanvas = canvasObj.AddComponent<Canvas>();
                    newCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                    canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                    uiCanvasTrs = canvasObj.transform;
                    Logger.LogWarning("UICanvas를 찾을 수 없어 새로 생성했습니다.");
                }
            }
            return uiCanvasTrs;
        }
    }

    /// <summary>
    /// Closed UI Transform 프로퍼티 - 자동으로 찾아서 반환
    /// </summary>
    private Transform ClosedUITrs
    {
        get
        {
            if (closedUITrs == null)
            {
                // ClosedUI라는 이름의 자식 오브젝트 찾기
                if (UICanvasTrs != null)
                {
                    Transform found = UICanvasTrs.Find("ClosedUI");
                    if (found == null)
                    {
                        // 없으면 새로 생성
                        GameObject closedUIObj = new GameObject("ClosedUI");
                        closedUIObj.transform.SetParent(UICanvasTrs);
                        closedUITrs = closedUIObj.transform;
                        closedUIObj.SetActive(false);
                    }
                    else
                    {
                        closedUITrs = found;
                    }
                }
            }
            return closedUITrs;
        }
    }

    // 제네릭으로 타입을 받아서 반환하고 있는데 이렇게 out 매개변수를 사용.
    private BaseUI GetUI<T>(out bool isAlreadyOpen)
    {
        System.Type uiType = typeof(T); // T로 전달된 화면 UI 클래스 타입. 이것을 uiType으로 받아온다.

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

    // UI 화면에 보이는 최상위로 올리는 함수
    public void OpenUI<T>(BaseUIData uiData)
    {
        System.Type uiType = typeof(T);

        Logger.Log($"{GetType()}::OpenUI({uiType})"); // 어떤 UI화면을 열었는지 로그를 남긴다.

        bool isAlreadyOpen = false; // 이미 열려있는지 여부를 확인하는 변수

        var ui = GetUI<T>(out isAlreadyOpen);

        if(!ui) // 생성에 실패 로그
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

    // 화면닫기함수
    public void CloseUI(BaseUI ui)
    {
        System.Type uiType = ui.GetType();

        Logger.Log($"CloseUI UI:{uiType}"); // 어떤 UI를 닫는지 로그

        ui.gameObject.SetActive(false);

        m_OpenUIPool.Remove(uiType); //열린풀에서 제거
        m_ClosedUIPool[uiType] = ui.gameObject;
        ui.transform.SetParent(ClosedUITrs);

        m_FrontUI = null;

        var lastChild = UICanvasTrs.GetChild(UICanvasTrs.childCount - 1);

        if(lastChild)
        {
            m_FrontUI = lastChild.gameObject.GetComponent<BaseUI>();
        }
    }

    // 특정 UI 화면이 열려있는지 확인 하고 열려있는 UI화면을 반환
    public BaseUI GetActiveUI<T>()
    {
        var uiType = typeof(T);

        return m_OpenUIPool.ContainsKey(uiType) ? m_OpenUIPool[uiType].GetComponent<BaseUI>() : null;
    }

    // UI화면이 열려있는지 하나라도 있는지 확인하는 함수
    public bool ExistsOpenUI()
    {
        return m_FrontUI != null; // m_FrontUI가 null이 아닌지 확인해서 열려있는지 반환
    }

    // 현재 최상위에 있는 오브젝트를 반환하는 함수
    public BaseUI GetCurrentFrontUI()
    {
        return m_FrontUI;
    }

    // 현재 최상위에 있는 UI화면 오브젝트를 닫는 함수
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
