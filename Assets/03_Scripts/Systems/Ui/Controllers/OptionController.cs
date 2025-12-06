using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

enum OptionButton
{
    Save,
    ToLobby,
    Quit
}

public class OptionController : MonoBehaviour
{
    private List<Button> buttons = new List<Button>();
    [Header("Addressable Settings")]
    public AssetReference LobbySceneReference; // Addressable 씬을 직접 드래그

    private void Awake()
    {
        // 자식 오브젝트에서 Button 자동 수집
        buttons.AddRange(GetComponentsInChildren<Button>(true));

        // enum 순서대로 기능 연결
        for (int i = 0; i < buttons.Count; i++)
        {
            int index = i; // 클로저 문제 방지
            buttons[i].onClick.AddListener(() => HandleButton((OptionButton)index));
        }
    }

    private void HandleButton(OptionButton type)
    {
        switch (type)
        {
            case OptionButton.Save:
                Debug.Log("저장 기능 실행");

                var player = FindObjectOfType<Player>();
                if (player != null)
                {
                    var data = UserDataManager.Instance.Get<UserPlayerStatusData>();
                    data.Position = player.transform.position;
                    data.HP = player.playerHp; // HP도 같이 저장하고 싶으면
                }

                UserDataManager.Instance.SaveUserData();
                Debug.Log("Saved HP: " + PlayerPrefs.GetFloat("PlayerHP"));
                break;

            case OptionButton.ToLobby:
                Debug.Log("로비로 이동");
                Addressables.LoadSceneAsync(LobbySceneReference);
                break;

            case OptionButton.Quit:
                Debug.Log("게임 종료");
                Application.Quit();
                break;
        }
    }
}