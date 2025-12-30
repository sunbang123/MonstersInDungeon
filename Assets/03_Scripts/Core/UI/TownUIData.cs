using System;
using UnityEngine;
using TMPro;

/// <summary>
/// 마을 UI 데이터 모델 - Data-Driven UI 구조
/// </summary>
[Serializable]
public class TownUIData
{
    public TMP_Text mapText;
    public TMP_Text townText;
}

/// <summary>
/// 마을 상태 데이터 모델
/// </summary>
[Serializable]
public class TownStateData
{
    public string townName;
    public bool isFading;
}

