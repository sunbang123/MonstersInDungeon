using System.Collections.Generic;
using UnityEngine;

public class UserDataManager : SingletonBehaviour<UserDataManager>
{
    public bool ExistsSavedData { get; private set; }
    public List<IUserData> UserDataList { get; private set; } = new List<IUserData>();

    protected override void Init()
    {
        base.Init();

        // 기존 데이터
        UserDataList.Add(new UserSettingsData());
        UserDataList.Add(new UserGoodsData());
        UserDataList.Add(new UserPlayerStatusData());
        UserDataList.Add(new UserInventoryData());

        LoadUserData();
    }
    public T Get<T>() where T : class, IUserData
    {
        foreach (var data in UserDataList)
        {
            if (data is T typed)
                return typed;
        }
        return null;
    }

    public void SetDefaultUserData()
    {
        foreach (var data in UserDataList)
            data.SetDefaultData();
    }

    public void LoadUserData()
    {
        ExistsSavedData = PlayerPrefs.GetInt("ExistsSavedData") == 1;

        if (ExistsSavedData)
        {
            foreach (var data in UserDataList)
                data.LoadData();
        }
    }

    public void SaveUserData()
    {
        bool hasError = false;

        foreach (var data in UserDataList)
        {
            if (!data.SaveData())
                hasError = true;
        }

        if (!hasError)
        {
            ExistsSavedData = true;
            PlayerPrefs.SetInt("ExistsSavedData", 1);
        }
    }
}
