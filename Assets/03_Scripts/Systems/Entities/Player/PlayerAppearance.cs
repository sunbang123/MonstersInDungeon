using System;
using System.Collections;
using UnityEngine;

public class PlayerAppearance : MonoBehaviour
{
    /// <summary>
    /// 스프라이트가 변경될 때 발생하는 이벤트
    /// </summary>
    public event Action<Sprite> OnSpriteChanged;
    [Header("Sprites")]
    public Sprite fireSprite;
    public Sprite waterSprite;
    public Sprite plantSprite;

    [Header("Animator Controllers")]
    public RuntimeAnimatorController fireAnimator;
    public RuntimeAnimatorController waterAnimator;
    public RuntimeAnimatorController plantAnimator;

    [Header("Default (요소 미선택시)")]
    public Sprite defaultSprite; // 기본 스프라이트
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
        Sprite selectedSprite = null;

        switch (element)
        {
            case "FIRE":
                selectedSprite = fireSprite;
                sr.sprite = fireSprite;
                if (anim != null && fireAnimator != null)
                    anim.runtimeAnimatorController = fireAnimator;
                break;

            case "WATER":
                selectedSprite = waterSprite;
                sr.sprite = waterSprite;
                if (anim != null && waterAnimator != null)
                    anim.runtimeAnimatorController = waterAnimator;
                break;

            case "PLANT":
                selectedSprite = plantSprite;
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
                // 기본 스프라이트 사용 (defaultSprite가 없으면 fireSprite를 기본으로 사용)
                if (defaultSprite != null)
                {
                    selectedSprite = defaultSprite;
                    sr.sprite = defaultSprite;
                }
                else if (fireSprite != null)
                {
                    selectedSprite = fireSprite;
                    sr.sprite = fireSprite;
                }
                break;
        }

        // Player의 portrait 업데이트
        UpdatePlayerPortrait(selectedSprite);

        // 이벤트 발생
        OnSpriteChanged?.Invoke(selectedSprite);

        Debug.Log($"PlayerAppearance 적용: {element}");
    }

    /// <summary>
    /// Player의 portrait를 업데이트
    /// portrait 속성을 통해 설정하면 OnPortraitChanged 이벤트가 자동으로 발생합니다.
    /// </summary>
    private void UpdatePlayerPortrait(Sprite sprite)
    {
        if (sprite == null) return;

        // 같은 GameObject에서 Player 컴포넌트 찾기
        Player player = GetComponent<Player>();
        if (player != null)
        {
            player.portrait = sprite; // 속성을 통해 설정하면 이벤트가 자동 발생
            Debug.Log($"Player portrait 업데이트: {sprite.name}");
        }
        else
        {
            // 부모나 자식에서 찾기
            player = GetComponentInParent<Player>();
            if (player == null)
                player = GetComponentInChildren<Player>();
            
            if (player != null)
            {
                player.portrait = sprite; // 속성을 통해 설정하면 이벤트가 자동 발생
                Debug.Log($"Player portrait 업데이트 (부모/자식에서 찾음): {sprite.name}");
            }
            else
            {
                Debug.LogWarning("PlayerAppearance: Player 컴포넌트를 찾을 수 없습니다.");
            }
        }
    }
}