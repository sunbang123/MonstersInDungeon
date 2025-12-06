using System.Diagnostics;

// 1. 추가적인 정보 표현(ex.타임스탬프)
// 2. 출시용 빌드를 위한 로그 제거
public class Logger
{
    [Conditional("DEV_VER")] // 콘디셔널 속성의 기능 : 조건부 컴파일 심볼
    public static void Log(string msg)
    {
        // 현재 시간을 날짜와 시간의 형식으로 표현해 {0} 넣고, 로깅하려는 메세지를 {1}에 넣는다.
        UnityEngine.Debug.LogFormat("[{0}] {1}", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), msg);
    }

    [Conditional("DEV_VER")]
    public static void LogWarning(string msg) // 워밍 로그 함수
    {
        UnityEngine.Debug.LogWarningFormat("[{0}] {1}", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), msg);
    }

    public static void LogError(string msg) // 워밍 로그 함수
    {
        UnityEngine.Debug.LogErrorFormat("[{0}] {1}", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), msg);
    }
}