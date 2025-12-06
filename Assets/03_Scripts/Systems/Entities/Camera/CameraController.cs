using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Vector3 base_pos;
    public static CameraController instance;
    public List<MapData> mapData;
    public int currentMapIndex = 0;
    private void Start()
    {
        instance = this.GetComponent<CameraController>();
        base_pos = Camera.main.gameObject.transform.position;
    }

    private void LateUpdate()
    {
        // xMaxPos와 yMaxPos 이상으로 이동하지 않음
        if (instance != null)
        {
            float clampedX = Mathf.Clamp(this.transform.position.x, -mapData[currentMapIndex].xMaxPos, mapData[currentMapIndex].xMaxPos);
            float clampedY = Mathf.Clamp(this.transform.position.y, -mapData[currentMapIndex].yMaxPos, mapData[currentMapIndex].yMaxPos);
            Vector3 pos = this.transform.position;
            Camera.main.gameObject.transform.position = new Vector3(clampedX, clampedY, base_pos.z);
        }
    }

    public void UpdateBasePos(Vector3 newPos)
    {
        base_pos = newPos;
    }
}
