using System.Collections.Generic;
using UnityEngine;

public class UserDataManager : SingletonBehaviour<UserDataManager>
{
    // 저장된 유저 데이터 존재 여부
    public bool ExistsSavedData { get; private set; }
    // 모든 유저 데이터 인스턴스를 저장하는 컨테이너
    // 모든 유저 UserDate 클래스는 IUserData 인터페이스를 구현하기 때문에
    // IUserData 타입으로 컨테이너를 선언하면 모든 유저 데이터 클래스를 이 컨테이너에 저장할 수 있음.
    public List<IUserData> UserDataList { get; private set; } = new List<IUserData>();

    protected override void Init()
    {
        base.Init();// Singleton Instance 처리가 init 함수에서 실행되기 때문에 해줘야함.

        // 모든 유저 데이터를 UserDataList에 추가
        UserDataList.Add(new UserSettingsData());
        UserDataList.Add(new UserGoodsData());
    }

    //모든 유저 데이터를 기본값으로 초기화하는 함수
    public void SetDefaultUserData()
    {
        for (int i = 0; i < UserDataList.Count; i++)
        {
            UserDataList[i].SetDefaultData();
        }
    }

    // 모든 유저데이터 클래스에 LoadData 함수를 호출해주는 함수
    public void LoadUserData()
    {
        ExistsSavedData = PlayerPrefs.GetInt("ExistsSavedData") == 1 ? true : false;
        // 만약에 저장된 데이터가 존재한다면
        if(ExistsSavedData)
        {
            // 모든 유저 데이터 클래스에 LoadData를 호출
            for(int i = 0; i<UserDataList.Count; i++)
            {
                UserDataList[i].LoadData();
            }
        }
    }
    // 모든 유저데이터 클래스의 SaveData 함수를 호출해서 모든 유저데이터를 저장하는 함수
    public void SaveUserData()
    {
        bool hasSaveError = false;

        for(int i = 0; i < UserDataList.Count; i++)
        {
            bool isSaveSuccess = UserDataList[i].SaveData(); // save가 성공적으로 이루어졌는지 확인
            if(!isSaveSuccess) // 에러가 났다면
            {
                hasSaveError = true;
            }
        }

        // 이렇게 되면 for문을 빠져나왔을때 즉, 모든 세이브 과정이 끝났을 때
        // 하나라도 에러가 발생한 유저 데이터 클래스가 있다면 hasSaveError = true 될것임.
        
        // 세이브 에러가 하나라도 발생하지 않았다면(세이브가 정상적으로 이뤄졌을 때만)
        if(!hasSaveError)
        {
            ExistsSavedData = true;
            PlayerPrefs.SetInt("ExistsSavedData", 1);
        }
    }
}