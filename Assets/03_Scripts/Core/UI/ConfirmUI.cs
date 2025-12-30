using System;
using TMPro;
using UnityEngine.UI;

public enum ConfirmType
{
    OK,
    OK_CANCEL,
}

public class ConfirmUIData : BaseUIData
{
    public ConfirmType ConfirmType; // 확인타입
    public string TitleText; // 화면제목
    public string DescTxt; // 화면내용
    public string OKBtnTxt; // 확인 버튼
    public Action OnClickOKBtn; // 확인버튼을 눌렀을 때 처리
    public string CancelBtnTxt; // 취소 버튼에 들어갈 텍스트
    public Action OnClickCancelBtn;// 취소버튼을 눌렀을때
}

public class ConfirmUI : BaseUI
{
    // 캐시된 UI 참조들
    private TextMeshProUGUI titleTxt;
    private TextMeshProUGUI descTxt;
    private Button okBtn;
    private Button cancelBtn;
    private TextMeshProUGUI okBtnTxt;
    private TextMeshProUGUI cancelBtnTxt;

    // 화면내에서 매개변수로 받은 UIData를 저장하고 있는 변수
    private ConfirmUIData m_ConfirmUIData = null;
    
    // 확인 버튼을 눌렀을 때 처리할 액션
    private Action m_OnClickOKBtn = null;
    // 취소버튼을 눌렀을 때 처리할 액션
    private Action m_OnClickCancelBtn = null;

    // 프로퍼티: UI 요소들을 자동으로 찾아서 반환
    private TextMeshProUGUI TitleTxt
    {
        get
        {
            if (titleTxt == null)
                titleTxt = UIHelper.FindComponentInChildren<TextMeshProUGUI>(transform, "TitleTxt");
            return titleTxt;
        }
    }

    private TextMeshProUGUI DescTxt
    {
        get
        {
            if (descTxt == null)
                descTxt = UIHelper.FindComponentInChildren<TextMeshProUGUI>(transform, "DescTxt");
            return descTxt;
        }
    }

    private Button OKBtn
    {
        get
        {
            if (okBtn == null)
                okBtn = UIHelper.FindComponentInChildren<Button>(transform, "OKBtn");
            return okBtn;
        }
    }

    private Button CancelBtn
    {
        get
        {
            if (cancelBtn == null)
                cancelBtn = UIHelper.FindComponentInChildren<Button>(transform, "CancelBtn");
            return cancelBtn;
        }
    }

    private TextMeshProUGUI OKBtnTxt
    {
        get
        {
            if (okBtnTxt == null)
                okBtnTxt = UIHelper.FindComponentInChildren<TextMeshProUGUI>(transform, "OKBtnTxt");
            return okBtnTxt;
        }
    }

    private TextMeshProUGUI CancelBtnTxt
    {
        get
        {
            if (cancelBtnTxt == null)
                cancelBtnTxt = UIHelper.FindComponentInChildren<TextMeshProUGUI>(transform, "CancelBtnTxt");
            return cancelBtnTxt;
        }
    }

    public override void SetInfo(BaseUIData uiData)
    {
        base.SetInfo(uiData);

        // BaseUIData를 ConfirmUIData로 캐스팅
        m_ConfirmUIData = uiData as ConfirmUIData;

        if (m_ConfirmUIData == null)
        {
            Logger.LogError($"{GetType()}::SetInfo - ConfirmUIData로 캐스팅 실패");
            return;
        }

        // UI 요소 설정
        TextMeshProUGUI title = TitleTxt;
        if (title != null)
            title.text = m_ConfirmUIData.TitleText;

        TextMeshProUGUI desc = DescTxt;
        if (desc != null)
            desc.text = m_ConfirmUIData.DescTxt;

        // 확인 버튼 설정
        Button ok = OKBtn;
        if (ok != null)
        {
            TextMeshProUGUI okTxt = OKBtnTxt;
            if (okTxt != null)
                okTxt.text = m_ConfirmUIData.OKBtnTxt;

            m_OnClickOKBtn = m_ConfirmUIData.OnClickOKBtn;
            ok.onClick.RemoveAllListeners();
            ok.onClick.AddListener(OnClickOK);
            ok.gameObject.SetActive(true);
        }

        // 취소 버튼 설정 (OK_CANCEL 타입일 때만)
        Button cancel = CancelBtn;
        if (cancel != null)
        {
            bool showCancel = m_ConfirmUIData.ConfirmType == ConfirmType.OK_CANCEL;
            cancel.gameObject.SetActive(showCancel);

            if (showCancel)
            {
                TextMeshProUGUI cancelTxt = CancelBtnTxt;
                if (cancelTxt != null)
                    cancelTxt.text = m_ConfirmUIData.CancelBtnTxt;

                m_OnClickCancelBtn = m_ConfirmUIData.OnClickCancelBtn;
                cancel.onClick.RemoveAllListeners();
                cancel.onClick.AddListener(OnClickCancel);
            }
        }
    }

    private void OnClickOK()
    {
        m_OnClickOKBtn?.Invoke();
        CloseUI();
    }

    private void OnClickCancel()
    {
        m_OnClickCancelBtn?.Invoke();
        CloseUI();
    }

    private void OnDestroy()
    {
        // 이벤트 리스너 정리
        Button ok = OKBtn;
        if (ok != null)
            ok.onClick.RemoveAllListeners();

        Button cancel = CancelBtn;
        if (cancel != null)
            cancel.onClick.RemoveAllListeners();
    }
}
