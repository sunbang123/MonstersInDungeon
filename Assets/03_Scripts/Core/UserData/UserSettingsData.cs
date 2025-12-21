using UnityEngine;

// IUserData 인터페이스를 구현하여 데이터 저장/로드 기능 제공
// 코드중복을 함수로 추상화
public class UserSettingsData : IUserData
{
    // 사운드 on / off 설정
    public bool Sound { get; set; }

    public void SetDefaultData()
    {
        // GetType()을 호출해 클래스 이름을 출력하고 함수명도 같이 출력
        Logger.Log($"{GetType()}::SetDefaultData");

        Sound = true;
    }

    public bool LoadData()
    {
        Logger.Log($"{GetType()}::LoadData");

        bool result = false;
        // (트랜잭션처럼 처리할 작성할 때 try블록으로 감싸면 코드중복을 줄일 수 있다.)
        try
        {
            // 플레이어 설정데이터를 불러와서 기본 데이터로 초기화할 필요가 있을 때 bool로 반환
            // 플레이어 설정데이터를 불러와, 사운드, 볼륨, 포인트 등 설정 불러오기.
            Sound = PlayerPrefs.GetInt("Sound") == 1 ? true : false;
            result = true;

            Logger.Log($"Sound:{Sound}");
        }
        // 데이터를 로드하거나 저장할 때 오류가 발생하면 에러 메시지 출력
        catch (System.Exception e)
        {
            Logger.Log("Load failed (" + e.Message + ")");
        }

        return result;
    }

    public bool SaveData()
    {
        Logger.Log($"{GetType()}::SaveData");

        bool result = false;

        try
        {
            PlayerPrefs.SetInt("Sound", Sound ? 1 : 0); // true가 true면 1, false면 0
            PlayerPrefs.Save();

            result = true;

            Logger.Log($"Sound:{Sound}");
        }
        catch(System.Exception e)
        {
            Logger.Log("Save failed (" + e.Message + ")");
        }

        return result;
    }
}
