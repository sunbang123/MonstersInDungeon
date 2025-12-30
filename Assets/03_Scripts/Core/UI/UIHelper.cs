using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI 요소를 자동으로 찾는 헬퍼 클래스
/// </summary>
public static class UIHelper
{
    /// <summary>
    /// Transform의 자식에서 이름으로 GameObject 찾기
    /// </summary>
    public static GameObject FindChild(Transform parent, string name)
    {
        if (parent == null) return null;

        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child.gameObject;

            GameObject found = FindChild(child, name);
            if (found != null)
                return found;
        }

        return null;
    }

    /// <summary>
    /// Transform의 자식에서 이름으로 컴포넌트 찾기
    /// </summary>
    public static T FindChildComponent<T>(Transform parent, string name) where T : Component
    {
        GameObject child = FindChild(parent, name);
        return child != null ? child.GetComponent<T>() : null;
    }

    /// <summary>
    /// Transform 또는 그 자식들에서 컴포넌트 찾기 (GetComponentInChildren과 유사하지만 이름으로 찾기)
    /// </summary>
    public static T FindComponentInChildren<T>(Transform parent, string name) where T : Component
    {
        if (parent == null) return null;

        if (parent.name == name)
        {
            T component = parent.GetComponent<T>();
            if (component != null)
                return component;
        }

        foreach (Transform child in parent)
        {
            T component = FindComponentInChildren<T>(child, name);
            if (component != null)
                return component;
        }

        return null;
    }
}

