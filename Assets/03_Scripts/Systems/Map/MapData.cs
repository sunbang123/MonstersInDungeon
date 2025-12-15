using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Map", menuName = "Game Data/Map Data")]
public class MapData : ScriptableObject
{
    // 맵 정보
    [Header("Map Information")]
    [Tooltip("맵의 고유 인덱스입니다. 같은 번호의 북쪽/남쪽 맵은 같은 인덱스를 가집니다.")]
    public int mapIndex = 0; // 맵 인덱스 (Map00=0, Map01_N/S=1, Map02_N/S=2 등)
    
    [Tooltip("맵의 표시 이름입니다.")]
    public string mapName = "Default Map"; // 기본값 맵
    
    [Tooltip("매칭할 GameObject 이름 (예: Map01_N, Map01_S). 비어있으면 mapName으로 매칭합니다.")]
    public string gameObjectName = ""; // GameObject 이름
    
    public enum MapDirection { None, North, South }
    [Tooltip("맵의 방향 (None: 방향 없음, North: 북쪽, South: 남쪽)")]
    public MapDirection direction = MapDirection.None;

    // 맵 크기를 나타내는 최대 X, Y 좌표 (float로 저장됩니다)
    [Tooltip("맵의 최대 X 축 좌표입니다.")]
    public float xMaxPos; // 기본값 없음

    [Tooltip("맵의 최대 Y 축 좌표입니다.")]
    public float yMaxPos; // 기본값 없음

    [Header("Spawn Points")]
    [Tooltip("플레이어 스폰 위치입니다.")]
    public Vector3 playerSpawnPosition; // 스폰 위치를 저장 (playerSpawnPositions -> playerSpawnPosition)
}
