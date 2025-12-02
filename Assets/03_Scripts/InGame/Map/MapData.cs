using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Map", menuName = "Game Data/Map Data")]
public class MapData : ScriptableObject
{
    // 맵의 이름
    [Header("Map Information")]
    [Tooltip("맵의 식별 가능한 이름입니다.")]
    public string mapName = "Default Map"; // 기본값 설정

    // 맵의 경계를 나타내는 최대 X, Y 좌표 (float을 사용했습니다)
    [Tooltip("맵의 최대 X 경계 좌표입니다.")]
    public float xMaxPos; // 기본값 설정

    [Tooltip("맵의 최대 Y 경계 좌표입니다.")]
    public float yMaxPos; // 기본값 설정

    [Header("Spawn Points")]
    [Tooltip("플레이어 스폰 위치입니다.")]
    public Vector3 playerSpawnPosition; // 단일 위치로 변경 (playerSpawnPositions -> playerSpawnPosition)
}