using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraController : MonoBehaviour
{
    // === 기존 변수 ===
    Vector3 base_pos;
    public static CameraController instance;

    // === 경계 제한 추가 변수 ===
    [Header("타일맵 경계 설정")]
    public Tilemap targetTilemap;

    // 타일맵 경계 좌표 (Start에서 계산됨)
    private float minX, maxX, minY, maxY;

    private void Start()
    {
        // === 기존 Start 로직 ===
        instance = this;

        base_pos = Camera.main.gameObject.transform.position;

        // === 경계 계산 로직 추가 ===
        CalculateCameraBounds();
    }

    /// <summary>
    /// 타일맵의 크기를 기반으로 카메라가 이동할 수 있는 경계를 계산합니다.
    /// </summary>
    private void CalculateCameraBounds()
    {
        if (targetTilemap == null)
        {
            Debug.LogError("Target Tilemap이 설정되지 않았습니다! 카메라 경계 제한이 작동하지 않습니다.");
            return;
        }

        // 타일맵의 월드 좌표 경계를 계산합니다.
        targetTilemap.CompressBounds();
        BoundsInt bounds = targetTilemap.cellBounds;

        Vector3 minWorld = targetTilemap.CellToWorld(new Vector3Int(bounds.xMin, bounds.yMin, 0));
        Vector3 maxWorld = targetTilemap.CellToWorld(new Vector3Int(bounds.xMax, bounds.yMax, 0));

        // 카메라의 절반 크기 계산
        Camera cam = Camera.main;
        float camHalfHeight = cam.orthographicSize;
        float camHalfWidth = cam.orthographicSize * cam.aspect;

        // 최종 카메라 경계 좌표 설정
        minX = minWorld.x + camHalfWidth;
        maxX = maxWorld.x - camHalfWidth;
        minY = minWorld.y + camHalfHeight;
        maxY = maxWorld.y - camHalfHeight;
    }

    private void LateUpdate()
    {
        // 1. 목표 위치 계산 (기존 로직)
        Vector3 pos = this.transform.position;
        // 기존 카메라 위치 계산 로직: X는 대상 위치, Y는 현재 Y와 대상 Y의 중간값, Z는 고정값
        Vector3 desiredPosition = new Vector3(
            pos.x,
            (this.transform.position.y + pos.y) / 2,
            base_pos.z
        );

        // 2. 경계 제한 적용 (Clamping)
        // 경계가 유효한지 확인
        if (maxX > minX && maxY > minY)
        {
            // Mathf.Clamp를 사용하여 계산된 경계 내에서만 움직이도록 강제합니다.
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }

        // 3. 최종 위치 적용
        Camera.main.gameObject.transform.position = desiredPosition;
    }

    // === 기존 함수 ===
    public void UpdateBasePos(Vector3 newPos)
    {
        base_pos = newPos;
    }

    public void StopCameraUpdate()
    {
        this.enabled = false;
    }

    public void ResumeCameraUpdate()
    {
        this.enabled = true;
    }
}