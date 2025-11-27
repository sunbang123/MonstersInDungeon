using UnityEngine;

public class PlayerInputLogger : MonoBehaviour
{
    // 플레이어의 현재 위치(월드 좌표)를 저장할 변수
    private Vector3 currentWorldPosition;

    void Update()
    {
        // 1. this.transform.position은 이미 Vector3입니다.
        currentWorldPosition = this.transform.position;

        // 2. 이 위치 정보를 활용합니다.
        // 예를 들어, 5초마다 현재 위치를 Debug.Log로 출력합니다.
        if (Time.frameCount % 300 == 0) // 약 5초마다 (60FPS 기준)
        {
            Debug.Log($"현재 플레이어 월드 좌표 벡터: {currentWorldPosition}");
            Debug.Log($"X 좌표: {currentWorldPosition.x}, Y 좌표: {currentWorldPosition.y}");
        }
    }
}