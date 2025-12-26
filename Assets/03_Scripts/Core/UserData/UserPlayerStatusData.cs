using UnityEngine;

public class UserPlayerStatusData : IUserData
{
    public float HP { get; set; }
    public Vector3 Position { get; set; }
    public bool TutorialEnd { get; set; }
    public int CurrentMapIndex { get; set; }
    public int Level { get; set; }
    public float CurrentExp { get; set; }

    public string SelectedElement; // "FIRE", "WATER", "PLANT"
    public void SetDefaultData()
    {
        HP = GameConstants.Player.DEFAULT_MAX_HP;
        Position = Vector3.zero;
        TutorialEnd = false;
        CurrentMapIndex = GameConstants.DEFAULT_MAP_INDEX; // 기본값은 첫 번째 맵
        SelectedElement = ""; // 기본값
        Level = GameConstants.Player.DEFAULT_LEVEL;
        CurrentExp = GameConstants.Player.DEFAULT_EXP;
    }

    public bool LoadData()
    {
        try
        {
            HP = PlayerPrefs.GetFloat("PlayerHP", GameConstants.Player.DEFAULT_MAX_HP);

            float x = PlayerPrefs.GetFloat("PlayerPosX", 0f);
            float y = PlayerPrefs.GetFloat("PlayerPosY", 0f);
            float z = PlayerPrefs.GetFloat("PlayerPosZ", 0f);
            Position = new Vector3(x, y, z);

            TutorialEnd = PlayerPrefs.GetInt("TutorialEnd", 0) == 1;
            SelectedElement = PlayerPrefs.GetString("SelectedElement", "");
            CurrentMapIndex = PlayerPrefs.GetInt("CurrentMapIndex", 0);

            return true;
        }
        catch { return false; }
    }

    public bool SaveData()
    {
        try
        {
            Debug.Log($"[UserPlayerStatusData.SaveData] 저장 시작 - HP: {HP}");
            PlayerPrefs.SetFloat("PlayerHP", HP);
            PlayerPrefs.SetFloat("PlayerPosX", Position.x);
            PlayerPrefs.SetFloat("PlayerPosY", Position.y);
            PlayerPrefs.SetFloat("PlayerPosZ", Position.z);

            PlayerPrefs.SetInt("TutorialEnd", TutorialEnd ? 1 : 0);
            PlayerPrefs.SetString("SelectedElement", SelectedElement);
            PlayerPrefs.SetInt("CurrentMapIndex", CurrentMapIndex);
            PlayerPrefs.SetInt("PlayerLevel", Level);
            PlayerPrefs.SetFloat("PlayerCurrentExp", CurrentExp);

            PlayerPrefs.Save();
            Debug.Log($"[UserPlayerStatusData.SaveData] 저장 완료 - PlayerPrefs에 저장된 HP: {PlayerPrefs.GetFloat("PlayerHP")}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[UserPlayerStatusData.SaveData] 저장 실패: {e.Message}");
            return false;
        }
    }
}
