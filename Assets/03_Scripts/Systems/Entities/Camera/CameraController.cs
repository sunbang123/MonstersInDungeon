using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CameraController : MonoBehaviour
{
    Vector3 base_pos;
    public static CameraController instance;
    
    
    private bool isInitialized = false;

    private void Awake()
    {
        instance = this.GetComponent<CameraController>();
    }

    private IEnumerator Start()
    {
        // 씬이 완전히 로드될 때까지 대기 (안드로이드 씬 로딩 타이밍 대응)
        yield return null;
        yield return null;

        // 카메라 찾기
        while (Camera.main == null)
        {
            yield return null;
        }
        
        base_pos = Camera.main.transform.position;
        
        // MapManager가 이미 존재하는지 확인 (비활성화된 것도 포함)
        MapManager existingManager = FindObjectOfType<MapManager>(true);
        
        if (existingManager != null)
        {
            // GameObject는 존재하지만 Instance가 null인 경우
            if (MapManager.Instance == null)
            {
                // GameObject가 비활성화되어 있으면 활성화
                if (!existingManager.gameObject.activeInHierarchy)
                {
                    existingManager.gameObject.SetActive(true);
                    yield return null; // Awake 대기
                }
                
                // 여전히 null이면 Awake가 호출되지 않았을 수 있음
                // Init()을 직접 호출하여 초기화
                if (MapManager.Instance == null)
                {
                    existingManager.Init();
                    yield return null;
                    
                    // 여전히 null이면 문제가 있음
                    if (MapManager.Instance == null)
                    {
                        Logger.LogWarning($"CameraController: MapManager Init() called but Instance is still null. This should not happen.");
                    }
                }
            }
        }
        else if (MapManager.Instance == null)
        {
            // Addressable에서 MapManager 로드
            AsyncOperationHandle<GameObject> mapManagerHandle = Addressables.LoadAssetAsync<GameObject>("MapManager");
            yield return mapManagerHandle;

            if (mapManagerHandle.Status == AsyncOperationStatus.Succeeded)
            {
                // 다시 한번 확인 (로딩 중에 다른 곳에서 생성되었을 수 있음)
                existingManager = FindObjectOfType<MapManager>(true);
                if (existingManager == null && MapManager.Instance == null)
                {
                    GameObject mapManagerObj = Instantiate(mapManagerHandle.Result);
                    
                    // GameObject가 활성화되어 있는지 확인
                    if (!mapManagerObj.activeInHierarchy)
                    {
                        mapManagerObj.SetActive(true);
                    }
                    
                    // Awake와 Init이 완료될 때까지 대기 (최대 20프레임)
                    for (int i = 0; i < 20 && MapManager.Instance == null; i++)
                    {
                        yield return null;
                    }
                    
                    // 여전히 null이면 문제가 있음
                    if (MapManager.Instance == null)
                    {
                        existingManager = mapManagerObj.GetComponent<MapManager>();
                        if (existingManager != null)
                        {
                            Logger.LogWarning($"CameraController: MapManager created but Instance is null. GameObject: {mapManagerObj.name}, Active: {mapManagerObj.activeInHierarchy}, Enabled: {existingManager.enabled}");
                        }
                    }
                }
            }
            else
            {
                Logger.LogError("CameraController: Failed to load MapManager from Addressables");
            }
        }

        // MapManager 인스턴스가 설정될 때까지 대기
        float waitTime = 0f;
        while (MapManager.Instance == null && waitTime < 5f)
        {
            existingManager = FindObjectOfType<MapManager>(true);
            if (existingManager != null && MapManager.Instance == null)
            {
                // GameObject는 있지만 Instance가 null - Awake 트리거 시도
                if (!existingManager.gameObject.activeInHierarchy)
                {
                    existingManager.gameObject.SetActive(true);
                }
                else
                {
                    // 이미 활성화되어 있으면 비활성화 후 재활성화하여 Awake 트리거
                    existingManager.gameObject.SetActive(false);
                    yield return null;
                    existingManager.gameObject.SetActive(true);
                }
                yield return null;
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
            }
            waitTime += 0.1f;
        }

        // MapManager의 초기화 완료 대기
        if (MapManager.Instance != null)
        {
            while (!MapManager.Instance.IsInitialized)
            {
                yield return new WaitForSeconds(0.1f);
            }
            
            Logger.Log("CameraController: MapManager initialized successfully");
        }
        else
        {
            existingManager = FindObjectOfType<MapManager>(true);
            if (existingManager != null)
            {
                Logger.LogWarning($"CameraController: MapManager GameObject exists but Instance is null! GameObject: {existingManager.gameObject.name}");
            }
            else
            {
                Logger.LogWarning("CameraController: MapManager.Instance is null after waiting and no GameObject found!");
            }
        }

        isInitialized = true;
    }


    private void LateUpdate()
    {
        // 초기화가 완료되지 않았으면 업데이트 중단
        if (!isInitialized || Camera.main == null)
        {
            return;
        }

        // MapManager에서 현재 맵의 MapData 가져오기
        MapData currentMapData = null;
        if (MapManager.Instance != null && MapManager.Instance.IsInitialized)
        {
            currentMapData = MapManager.Instance.GetCurrentMapData();
            
            // 디버그: MapData가 null인 경우 로그 출력
            if (currentMapData == null)
            {
                int currentMapIdx = MapManager.Instance.GetCurrentMapIndex();
                Logger.LogWarning($"CameraController: MapData is null for map index {currentMapIdx}");
            }
        }

        // 원래 로직: this.transform.position을 사용하여 맵 경계 제한
        if (instance != null && Camera.main != null)
        {
            Vector3 targetPosition = this.transform.position;
            
            if (currentMapData != null)
            {
                float clampedX = Mathf.Clamp(targetPosition.x, -currentMapData.xMaxPos, currentMapData.xMaxPos);
                float clampedY = Mathf.Clamp(targetPosition.y, -currentMapData.yMaxPos, currentMapData.yMaxPos);
                Camera.main.transform.position = new Vector3(clampedX, clampedY, base_pos.z);
            }
            else
            {
                // MapData가 없으면 제한 없이 이동
                Camera.main.transform.position = new Vector3(targetPosition.x, targetPosition.y, base_pos.z);
            }
        }
    }

    public void UpdateBasePos(Vector3 newPos)
    {
        base_pos = newPos;
    }
}
