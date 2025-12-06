using System.Collections.Generic;
using UnityEngine;

public class UserInventoryData : IUserData
{
    public List<string> ItemNames = new List<string>();

    public void SetDefaultData()
    {
        ItemNames.Clear();
    }

    public bool LoadData()
    {
        try
        {
            string json = PlayerPrefs.GetString("Inventory", "");
            if (!string.IsNullOrEmpty(json))
            {
                ItemNames = JsonUtility.FromJson<Wrapper>(json).Items;
            }
            return true;
        }
        catch { return false; }
    }

    public bool SaveData()
    {
        try
        {
            Wrapper w = new Wrapper();
            w.Items = ItemNames;

            string json = JsonUtility.ToJson(w);
            PlayerPrefs.SetString("Inventory", json);
            PlayerPrefs.Save();
            return true;
        }
        catch { return false; }
    }

    [System.Serializable]
    private class Wrapper
    {
        public List<string> Items;
    }
}
