using System.Diagnostics;
using UnityEngine;

/// <summary>
/// 게임 로그 관리 클래스
/// </summary>
public static class Logger
{
    /// <summary>
    /// 로그 레벨 enum
    /// </summary>
    public enum LogLevel
    {
        None = 0,
        Error = 1,
        Warning = 2,
        Info = 3,
        Debug = 4
    }

    /// <summary>
    /// 현재 로그 레벨 (DEV_VER가 정의되면 Debug, 아니면 Warning부터 출력)
    /// </summary>
    private static LogLevel CurrentLogLevel
    {
        get
        {
#if DEV_VER
            return LogLevel.Debug;
#else
            return LogLevel.Warning;
#endif
        }
    }

    /// <summary>
    /// 정보 로그 출력 (개발 버전에서만)
    /// </summary>
    public static void Log(string msg)
    {
        if (CurrentLogLevel >= LogLevel.Info)
        {
            UnityEngine.Debug.LogFormat("[{0}] {1}", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), msg);
        }
    }

    /// <summary>
    /// 경고 로그 출력
    /// </summary>
    public static void LogWarning(string msg)
    {
        if (CurrentLogLevel >= LogLevel.Warning)
        {
            UnityEngine.Debug.LogWarningFormat("[{0}] {1}", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), msg);
        }
    }

    /// <summary>
    /// 에러 로그 출력 (항상 출력)
    /// </summary>
    public static void LogError(string msg)
    {
        UnityEngine.Debug.LogErrorFormat("[{0}] {1}", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), msg);
    }
}