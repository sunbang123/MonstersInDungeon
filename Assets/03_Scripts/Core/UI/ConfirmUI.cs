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
    public TextMeshProUGUI TitleTxt = null; // 화면 제목 텍스트
    public TextMeshProUGUI DescTxt = null; // 화면 중앙에 위치
    public Button OKBtn = null; // 확인버튼
    public Button CancelBtn = null; // 취소 버튼
    public TextMeshProUGUI OKBtnTxt = null; // 확인버튼텍스트
    public TextMeshProUGUI CancelBtnTxt = null; // 취소버튼

    // 화면내에서 매개변수로 받은 UIDaa를 저장하고 있는 변수
    private ConfirmUIData m_ConfirmUIData = null;
    // 확인 버튼을 눌렀을 때 처리할 액션
    private Action m_OnClickOKBtn = null;
    // 취소버튼을 눌렀을 때 처리할 액션
    private Action m_OnClickCancelBtn = null;

    public override void SetInfo(BaseUIData uiData)
    {
        base.SetInfo(uiData);
    }
}
