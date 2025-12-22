using System.Collections.Generic;
using UnityEngine;

public class TownTrigger : MonoBehaviour
{
    [Header("Map Settings")]
    [Tooltip("맵 데이터 ScriptableObject (맵 이름 표시용)")]
    public MapData mapData;
    
    [Tooltip("전환할 맵 인덱스 (MapManager의 mapIndex와 동일)")]
    public int mapIndex;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        // 마을 이름 표시 (이벤트 기반)
        if (mapData != null)
        {
            TownUIManager.OnTownNameChanged?.Invoke(mapData.mapName);
        }

        // MapManager를 통해 맵 전환
        if (MapManager.Instance != null)
        {
            // 현재 맵과 다른 맵으로 전환하는 경우만
            if (MapManager.Instance.GetCurrentMapIndex() != mapIndex || mapData == null)
            {
                // MapData의 방향 정보 사용
                MapData.MapDirection direction = mapData != null ? mapData.direction : MapData.MapDirection.None;
                MapManager.Instance.TransitionToMap(mapIndex, other.transform, direction);
            }
        }
        else
        {
            Logger.LogWarning("TownTrigger: MapManager.Instance is null!");
        }
    }
}
