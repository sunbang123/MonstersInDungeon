public interface IUserData
{
    // 기본데이터로 데이터를 초기화
    void SetDefaultData();
    // 데이터를 로드
    bool LoadData();
    // 데이터를 저장
    bool SaveData();
}
