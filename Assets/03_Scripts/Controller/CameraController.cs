using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Vector3 base_pos;
    public static CameraController instance;
    private void Start()
    {
        instance = this.GetComponent<CameraController>();
        base_pos = Camera.main.gameObject.transform.position;
    }

    private void LateUpdate()
    {
        Vector3 pos = this.transform.position;
        Camera.main.gameObject.transform.position = new Vector3(pos.x, (this.transform.position.y + pos.y) / 2, base_pos.z);
    }
    public void UpdateBasePos(Vector3 newPos)
    {
        base_pos = newPos;
    }
}

