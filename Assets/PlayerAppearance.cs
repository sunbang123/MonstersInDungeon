using System.Collections;
using UnityEngine;

public class PlayerAppearance : MonoBehaviour
{
    public Sprite fireSprite;
    public Sprite waterSprite;
    public Sprite plantSprite;

    private SpriteRenderer sr;

    private IEnumerator Start()
    {
        // UserDataManager 준비될 때까지 대기
        while (UserDataManager.Instance == null)
            yield return null;

        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError("PlayerAppearance: SpriteRenderer 없음!");
            yield break;
        }

        var status = UserDataManager.Instance.Get<UserPlayerStatusData>();
        if (status == null)
        {
            Debug.LogError("PlayerAppearance: UserPlayerStatusData 없음!");
            yield break;
        }

        ApplySprite(status.SelectedElement);
    }

    private void ApplySprite(string element)
    {
        switch (element)
        {
            case "FIRE":
                sr.sprite = fireSprite;
                break;

            case "WATER":
                sr.sprite = waterSprite;
                break;

            case "PLANT":
                sr.sprite = plantSprite;
                break;

            default:
                Debug.LogWarning("PlayerAppearance: 선택된 원소가 없음. 기본 스프라이트 유지");
                break;
        }

        Debug.Log($"PlayerAppearance 적용됨: {element}");
    }
}
