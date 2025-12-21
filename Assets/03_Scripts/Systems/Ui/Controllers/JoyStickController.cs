using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JoyStickController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private Image background;
    [SerializeField] private Image controller;

    private Vector2 input;

    public float Horizontal => input.x;
    public float Vertical => input.y;

    private void Awake()
    {
        // Inspector에서 설정되지 않았다면 자동으로 컴포넌트 찾기
        if (background == null) background = GetComponent<Image>();
        if (controller == null) controller = transform.GetChild(0).GetComponent<Image>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 필요한 위치 계산 후 추가
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background.rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            // 로컬 좌표를 -1 ~ 1 범위로 정규화
            Vector2 sizeDelta = background.rectTransform.sizeDelta;
            input = new Vector2(
                (localPoint.x / sizeDelta.x) * 2,
                (localPoint.y / sizeDelta.y) * 2
            );

            // 거리 제한
            input = input.magnitude > 1 ? input.normalized : input;

            // 컨트롤러 위치 업데이트
            controller.rectTransform.anchoredPosition = input * (sizeDelta / 2);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        input = Vector2.zero;
        controller.rectTransform.anchoredPosition = Vector2.zero;
    }
}
