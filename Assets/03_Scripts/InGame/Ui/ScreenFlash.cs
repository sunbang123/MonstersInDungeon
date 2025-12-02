using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFlash : MonoBehaviour
{
    [Header("Flash Settings")]
    [SerializeField] private Image flashImage; // 전체 화면을 덮는 UI Image
    [SerializeField] private Color flashColor = new Color(0.5f, 0f, 1f, 0.3f); // 보라색, 알파 0.3
    private float flashDuration = 0.2f; // 깜빡임 지속 시간

    private Coroutine flashCoroutine;

    void Start()
    {
        // 초기 상태: 투명하게 설정
        if (flashImage != null)
        {
            flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
        }
    }

    private void CreateFlashImage()
    {
        RectTransform rectTransform = flashImage.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;

        // 다른 UI 요소 위에 표시되도록 설정
        flashImage.raycastTarget = false;
    }
    
    public void Flash()
    {
        if (flashImage == null) return;

        // 이미 실행 중인 깜빡임이 있으면 중지
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        // 즉시 색상 적용
        flashImage.color = flashColor;

        // 점진적으로 페이드 아웃
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(flashColor.a, 0f, elapsed / flashDuration);
            flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            yield return null;
        }

        // 완전히 투명하게 설정
        flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
        flashCoroutine = null;
    }
    
    public void FlashWithColor(Color color, float? duration = null) // 투명도
    {
        if (flashImage == null) return;

        float actualduration = duration ?? flashDuration;

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(FlashCoroutineWithColor(color, actualduration));
    }

    private IEnumerator FlashCoroutineWithColor(Color color, float duration)
    {
        flashImage.color = color;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(color.a, 0f, elapsed / duration);
            flashImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        flashImage.color = new Color(color.r, color.g, color.b, 0f);
        flashCoroutine = null;
    }
}

