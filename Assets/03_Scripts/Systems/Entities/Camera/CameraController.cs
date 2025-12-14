using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Vector3 base_pos;
    public static CameraController instance;
    
    [Header("Map Data (Legacy - MapManager 사용 권장)")]
    [Tooltip("레거시 지원용. MapManager를 통해 자동으로 가져옵니다.")]
    public List<MapData> mapData;
    
    private MapManager mapManager;
    private int currentMapIndex = 0;

    private void Start()
    {
        instance = this.GetComponent<CameraController>();
        base_pos = Camera.main.gameObject.transform.position;
        mapManager = FindObjectOfType<MapManager>();
    }

    private void LateUpdate()
    {
        // MapManager에서 현재 맵의 MapData 가져오기
        MapData currentMapData = null;
        if (mapManager != null)
        {
            currentMapData = mapManager.GetCurrentMapData();
        }

        // MapManager에서 가져온 MapData가 없으면 레거시 mapData 리스트 사용
        if (currentMapData == null && mapData != null && mapData.Count > 0)
        {
            currentMapIndex = mapManager != null ? mapManager.GetCurrentMapIndex() : 0;
            if (currentMapIndex >= 0 && currentMapIndex < mapData.Count)
            {
                currentMapData = mapData[currentMapIndex];
            }
        }

        // xMaxPos와 yMaxPos 이상으로 이동하지 못하게 제한
        if (instance != null && currentMapData != null)
        {
            float clampedX = Mathf.Clamp(this.transform.position.x, -currentMapData.xMaxPos, currentMapData.xMaxPos);
            float clampedY = Mathf.Clamp(this.transform.position.y, -currentMapData.yMaxPos, currentMapData.yMaxPos);
            Vector3 pos = this.transform.position;
            Camera.main.gameObject.transform.position = new Vector3(clampedX, clampedY, base_pos.z);
        }
    }

    public void UpdateBasePos(Vector3 newPos)
    {
        base_pos = newPos;
    }
}
