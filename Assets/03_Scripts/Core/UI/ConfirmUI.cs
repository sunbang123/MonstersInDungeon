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
    public ConfirmType ConfirmType; // 팝업유형
    public string TitleText; // 화면제목
    public string DescTxt; // 화면중앙
    public string OKBtnTxt; // 확인 버튼
    public Action OnClickOKBtn; // 확인버튼에 누를시 처리
    public string CancelBtnTxt; // 취소 버튼에 보여질 텍스트
    public Action OnClickCancelBtn;// 취소버튼 누럴ㅆ을시
}
public class ConfirmUI : BaseUI
{
    public TextMeshProUGUI TitleTxt = null; // 화면 제목 텍스트
    public TextMeshProUGUI DescTxt = null; // 화면 중앙에 설치
    public Button OKBtn = null; // 확인버튼선언
    public Button CancelBtn = null; // 취소 버튼 선언
    public TextMeshProUGUI OKBtnTxt = null; // 확인버튼텍스트
    public TextMeshProUGUI CancelBtnTxt = null; // 취소버튼

    // 화면열때 매개변수로 받은 UIDaa를 저장할 변수선언
    private ConfirmUIData m_ConfirmUIData = null;
    // 확인 버튼을 눌렀을시 액션선언
    private Action m_OnClickOKBtn = null;
    // 취소버튼을 눌렀을시 액션선언
    private Action m_OnClickCancelBtn = null;

    public override void SetInfo(BaseUIData uiData)
    {
        base.SetInfo(uiData);
    }
}
