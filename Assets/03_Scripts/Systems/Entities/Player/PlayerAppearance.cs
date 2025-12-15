using System.Collections;
using UnityEngine;

public class PlayerAppearance : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite fireSprite;
    public Sprite waterSprite;
    public Sprite plantSprite;

    [Header("Animator Controllers")]
    public RuntimeAnimatorController fireAnimator;
    public RuntimeAnimatorController waterAnimator;
    public RuntimeAnimatorController plantAnimator;

    [Header("Default (요소 미선택시)")]
    public RuntimeAnimatorController defaultAnimator; // 기본 애니메이터 추가

    private SpriteRenderer sr;
    private Animator anim;

    private IEnumerator Start()
    {
        // UserDataManager 로딩 대기
        while (UserDataManager.Instance == null)
            yield return null;

        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

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

        ApplyAppearance(status.SelectedElement);
    }

    private void ApplyAppearance(string element)
    {
        switch (element)
        {
            case "FIRE":
                sr.sprite = fireSprite;
                if (anim != null && fireAnimator != null)
                    anim.runtimeAnimatorController = fireAnimator;
                break;

            case "WATER":
                sr.sprite = waterSprite;
                if (anim != null && waterAnimator != null)
                    anim.runtimeAnimatorController = waterAnimator;
                break;

            case "PLANT":
                sr.sprite = plantSprite;
                if (anim != null && plantAnimator != null)
                    anim.runtimeAnimatorController = plantAnimator;
                break;

            default:
                Debug.LogWarning("PlayerAppearance: 선택된 요소가 없음. 기본 외형 사용");
                // 기본 애니메이터 적용
                if (anim != null && defaultAnimator != null)
                {
                    anim.runtimeAnimatorController = defaultAnimator;
                    Debug.Log("기본 애니메이터 적용됨");
                }
                break;
        }

        Debug.Log($"PlayerAppearance 적용: {element}");
    }
}