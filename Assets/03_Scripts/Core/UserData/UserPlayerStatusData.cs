using UnityEngine;

public class UserPlayerStatusData : IUserData
{
    public float HP { get; set; }
    public Vector3 Position { get; set; }
    public bool TutorialEnd { get; set; }
    public void SetDefaultData()
    {
        HP = 100f;
        Position = Vector3.zero;
        TutorialEnd = false;
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

            PlayerPrefs.Save();
            return true;
        }
        catch { return false; }
    }
}
