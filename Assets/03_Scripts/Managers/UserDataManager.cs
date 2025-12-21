using System.Collections.Generic;
using UnityEngine;

public class UserDataManager : SingletonBehaviour<UserDataManager>
{
    public bool ExistsSavedData { get; private set; }
    public List<IUserData> UserDataList { get; private set; } = new List<IUserData>();

    protected override void Init()
    {
        base.Init();
        Logger.Log("UserDataManager Init 시작");

        // 데이터 등록
        UserDataList.Add(new UserSettingsData());
        UserDataList.Add(new UserGoodsData());
        UserDataList.Add(new UserPlayerStatusData());
        UserDataList.Add(new UserInventoryData());
        LoadUserData();

        Logger.Log("UserDataManager Init 완료");
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
        Logger.Log($"LoadUserData - ExistsSavedData: {ExistsSavedData}");

        if (ExistsSavedData)
        {
            foreach (var data in UserDataList)
                data.LoadData();
        }
        else
        {
            // 저장된 데이터가 없으면 기본값으로 초기화
            SetDefaultUserData();
        }
    }

    /// <summary>
    /// 새 게임 시작: 모든 데이터를 초기화 하고 기본값으로 설정
    /// </summary>
    public void StartNewGame()
    {
        Logger.Log("StartNewGame 호출");

        // 1. PlayerPrefs 초기화
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // 2. 메모리상 모든 데이터를 기본값으로 초기화
        SetDefaultUserData();

        // 3. 저장 플래그 초기화
        ExistsSavedData = false;

        Logger.Log("새 게임 데이터를 초기화 완료. TutorialEnd: " +
            Get<UserPlayerStatusData>()?.TutorialEnd);
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
            PlayerPrefs.Save();
            Logger.Log("SaveUserData 완료");
        }
    }
}
