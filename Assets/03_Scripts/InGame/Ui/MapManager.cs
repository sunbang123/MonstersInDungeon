using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public List<GameObject> maps = new List<GameObject>();   

    private static int currentMapIndex = 0;

    void Awake()
    {

        // 초기 맵 설정
        InitializeMaps();
    }

    void InitializeMaps()
    {
        // 첫 번째 맵만 활성화, 나머지는 비활성화
        for (int i = 0; i < maps.Count; i++)
        {
            if (maps[i] != null)
            {
                maps[i].SetActive(i == currentMapIndex);
            }
        }
    }

    void TransitionToMap(int mapIndex, Transform playerTransform)
    {
        // 현재 맵 비활성화
        if (currentMapIndex < maps.Count && maps[currentMapIndex] != null)
        {
            maps[currentMapIndex].SetActive(false);
        }

        // 타겟 맵 활성화
        if (maps[mapIndex] != null)
        {
            maps[mapIndex].SetActive(true);
        }

        // 현재 맵 인덱스 업데이트
        currentMapIndex = mapIndex;
    }
}