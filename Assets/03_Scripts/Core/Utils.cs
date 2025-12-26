using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 게임 전반에서 사용되는 유틸리티 메소드 모음
/// </summary>
public static class Utils
{
    /// <summary>
    /// Vector3의 Y값을 0으로 설정 (2D 평면 좌표로 변환)
    /// </summary>
    public static Vector3 FlattenVector3(Vector3 vector)
    {
        return new Vector3(vector.x, 0f, vector.z);
    }

    /// <summary>
    /// 두 벡터 사이의 방향을 반환
    /// </summary>
    public static Vector3 Direction(Vector3 from, Vector3 to)
    {
        return (to - from).normalized;
    }

    /// <summary>
    /// 두 점 사이의 거리 계산 (Y축 무시)
    /// </summary>
    public static float Distance2D(Vector3 a, Vector3 b)
    {
        return Vector3.Distance(FlattenVector3(a), FlattenVector3(b));
    }

    /// <summary>
    /// 각도를 라디안으로 변환
    /// </summary>
    public static float DegreesToRadians(float degrees)
    {
        return degrees * Mathf.Deg2Rad;
    }

    /// <summary>
    /// 라디안을 각도로 변환
    /// </summary>
    public static float RadiansToDegrees(float radians)
    {
        return radians * Mathf.Rad2Deg;
    }

    /// <summary>
    /// 범위 내 랜덤 값 반환 (포함)
    /// </summary>
    public static int RandomRangeInclusive(int min, int max)
    {
        return Random.Range(min, max + 1);
    }

    /// <summary>
    /// 확률에 따른 성공 여부 반환
    /// </summary>
    /// <param name="probability">0.0 ~ 1.0 사이의 확률</param>
    public static bool RandomChance(float probability)
    {
        return Random.value <= probability;
    }

    /// <summary>
    /// 배열에서 랜덤 요소 반환
    /// </summary>
    public static T GetRandomElement<T>(T[] array)
    {
        if (array == null || array.Length == 0)
            return default(T);

        return array[Random.Range(0, array.Length)];
    }

    /// <summary>
    /// 리스트에서 랜덤 요소 반환
    /// </summary>
    public static T GetRandomElement<T>(List<T> list)
    {
        if (list == null || list.Count == 0)
            return default(T);

        return list[Random.Range(0, list.Count)];
    }

    /// <summary>
    /// 리스트 섞기 (Fisher-Yates 알고리즘)
    /// </summary>
    public static void ShuffleList<T>(List<T> list)
    {
        if (list == null || list.Count <= 1)
            return;

        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    /// <summary>
    /// UI 텍스트의 알파 값 설정
    /// </summary>
    public static void SetTextAlpha(Text text, float alpha)
    {
        if (text != null)
        {
            Color color = text.color;
            color.a = Mathf.Clamp01(alpha);
            text.color = color;
        }
    }

    /// <summary>
    /// UI 이미지의 알파 값 설정
    /// </summary>
    public static void SetImageAlpha(Image image, float alpha)
    {
        if (image != null)
        {
            Color color = image.color;
            color.a = Mathf.Clamp01(alpha);
            image.color = color;
        }
    }

    /// <summary>
    /// 캔버스 그룹 알파 값 설정
    /// </summary>
    public static void SetCanvasGroupAlpha(CanvasGroup canvasGroup, float alpha)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = Mathf.Clamp01(alpha);
        }
    }

    /// <summary>
    /// UI 요소 활성화/비활성화 (페이드 효과)
    /// </summary>
    public static IEnumerator FadeUIElement(MonoBehaviour monoBehaviour, CanvasGroup canvasGroup, float targetAlpha, float duration)
    {
        if (canvasGroup == null)
            yield break;

        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float normalizedTime = time / duration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, normalizedTime);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }

    /// <summary>
    /// TextMeshProUGUI의 알파 값 페이드
    /// </summary>
    public static IEnumerator FadeTextMeshProUGUI(TextMeshProUGUI text, float targetAlpha, float duration)
    {
        if (text == null)
            yield break;

        Color startColor = text.color;
        float startAlpha = startColor.a;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float normalizedTime = time / duration;
            Color color = startColor;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, normalizedTime);
            text.color = color;
            yield return null;
        }

        Color finalColor = startColor;
        finalColor.a = targetAlpha;
        text.color = finalColor;
    }

    /// <summary>
    /// 카메라 배경색 페이드
    /// </summary>
    public static IEnumerator FadeCameraBackground(Camera camera, Color targetColor, float duration)
    {
        if (camera == null)
            yield break;

        Color startColor = camera.backgroundColor;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float normalizedTime = time / duration;
            camera.backgroundColor = Color.Lerp(startColor, targetColor, normalizedTime);
            yield return null;
        }

        camera.backgroundColor = targetColor;
    }

    /// <summary>
    /// Image의 알파 값 페이드
    /// </summary>
    public static IEnumerator FadeImage(Image image, float targetAlpha, float duration)
    {
        if (image == null)
            yield break;

        Color startColor = image.color;
        float startAlpha = startColor.a;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float normalizedTime = time / duration;
            Color color = startColor;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, normalizedTime);
            image.color = color;
            yield return null;
        }

        Color finalColor = startColor;
        finalColor.a = targetAlpha;
        image.color = finalColor;
    }

    /// <summary>
    /// 게임오브젝트의 모든 자식 UI 요소들을 페이드 인
    /// </summary>
    public static IEnumerator FadeInChildrenUI(MonoBehaviour monoBehaviour, GameObject parent, float duration)
    {
        if (parent == null)
            yield break;

        List<Coroutine> fadeCoroutines = new List<Coroutine>();

        // 모든 자식 요소들을 재귀적으로 찾아서 페이드 인
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.gameObject == parent)
                continue;

            // CanvasGroup이 있으면 CanvasGroup으로 페이드
            CanvasGroup canvasGroup = child.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                fadeCoroutines.Add(monoBehaviour.StartCoroutine(FadeUIElement(monoBehaviour, canvasGroup, 1f, duration)));
                continue;
            }

            // TextMeshProUGUI가 있으면 텍스트로 페이드
            TextMeshProUGUI textMesh = child.GetComponent<TextMeshProUGUI>();
            if (textMesh != null)
            {
                Color textColor = textMesh.color;
                textColor.a = 0f;
                textMesh.color = textColor;
                fadeCoroutines.Add(monoBehaviour.StartCoroutine(FadeTextMeshProUGUI(textMesh, 1f, duration)));
                continue;
            }

            // Image가 있으면 이미지로 페이드
            Image image = child.GetComponent<Image>();
            if (image != null)
            {
                Color imageColor = image.color;
                imageColor.a = 0f;
                image.color = imageColor;
                fadeCoroutines.Add(monoBehaviour.StartCoroutine(FadeImage(image, 1f, duration)));
                continue;
            }
        }

        // 모든 페이드 효과가 완료될 때까지 대기
        yield return new WaitForSeconds(duration);
    }

    /// <summary>
    /// 게임 오브젝트의 모든 자식 비활성화
    /// </summary>
    public static void DeactivateAllChildren(GameObject parent)
    {
        if (parent == null)
            return;

        foreach (Transform child in parent.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 게임 오브젝트의 모든 자식 활성화
    /// </summary>
    public static void ActivateAllChildren(GameObject parent)
    {
        if (parent == null)
            return;

        foreach (Transform child in parent.transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 컴포넌트가 존재하지 않으면 추가
    /// </summary>
    public static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
    {
        if (gameObject == null)
            return null;

        T component = gameObject.GetComponent<T>();
        if (component == null)
        {
            component = gameObject.AddComponent<T>();
        }
        return component;
    }

    /// <summary>
    /// RectTransform의 앵커 포지션 설정
    /// </summary>
    public static void SetAnchoredPosition(RectTransform rectTransform, Vector2 position)
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = position;
        }
    }

    /// <summary>
    /// 월드 좌표를 화면 좌표로 변환
    /// </summary>
    public static Vector2 WorldToScreenPoint(Camera camera, Vector3 worldPoint)
    {
        if (camera == null)
            return Vector2.zero;

        return camera.WorldToScreenPoint(worldPoint);
    }

    /// <summary>
    /// 화면 좌표를 월드 좌표로 변환
    /// </summary>
    public static Vector3 ScreenToWorldPoint(Camera camera, Vector2 screenPoint, float distance = 10f)
    {
        if (camera == null)
            return Vector3.zero;

        return camera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, distance));
    }
}
