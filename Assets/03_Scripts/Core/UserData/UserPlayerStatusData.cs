using UnityEngine;

public class UserPlayerStatusData : IUserData
{
    public float HP { get; set; }
    public Vector3 Position { get; set; }
    public bool TutorialEnd { get; set; }
    public int CurrentMapIndex { get; set; }

    public string SelectedElement; // "FIRE", "WATER", "PLANT"
    public void SetDefaultData()
    {
        HP = 100f;
        Position = Vector3.zero;
        TutorialEnd = false;
        CurrentMapIndex = 0; // 기본값은 첫 번째 맵
        SelectedElement = ""; // 기본값
    }

    public bool LoadData()
    {
        try
        {
            HP = PlayerPrefs.GetFloat("PlayerHP", 100f);

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
            PlayerPrefs.SetFloat("PlayerHP", HP);
            PlayerPrefs.SetFloat("PlayerPosX", Position.x);
            PlayerPrefs.SetFloat("PlayerPosY", Position.y);
            PlayerPrefs.SetFloat("PlayerPosZ", Position.z);

            PlayerPrefs.SetInt("TutorialEnd", TutorialEnd ? 1 : 0);
            PlayerPrefs.SetString("SelectedElement", SelectedElement);
            PlayerPrefs.SetInt("CurrentMapIndex", CurrentMapIndex);

            PlayerPrefs.Save();
            return true;
        }
        catch { return false; }
    }
}
