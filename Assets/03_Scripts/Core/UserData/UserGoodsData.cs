using UnityEngine;

public class UserGoodsData : IUserData
{
    // 보석, int 범위를 벗어날 수 있다.
    public long Gem { get; set; }
    // 골드
    public long Gold { get; set; }
    public void SetDefaultData()
    {
        Logger.Log($"{GetType()}::SetDefaultData");

        Gem = 0;
        Gold = 0;
    }

    public bool LoadData()
    {
        Logger.Log($"{GetType()}::LoadData");

        bool result = false;

        try
        {
            // Gem, Gold는 long이기 때문에 문자열을 이용해서 데이터를 저장하고 불러와야 함.
            Gem = long.Parse(PlayerPrefs.GetString("Gem"));
            Gold = long.Parse(PlayerPrefs.GetString("Gold"));
            result = true;

            Logger.Log($"Gem:{Gem} Gold:{Gold}");
        }
        catch(System.Exception e)
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
            PlayerPrefs.SetString("Gem", Gem.ToString());
            PlayerPrefs.SetString("Gold", Gold.ToString());
            PlayerPrefs.Save();

            result = true;

            Logger.Log($"Gem:{Gem} Gold:{Gold}");
        }
        catch (System.Exception e)
        {
            Logger.Log("Save failed (" + e.Message + ")");
        }

        return result;
    }
}