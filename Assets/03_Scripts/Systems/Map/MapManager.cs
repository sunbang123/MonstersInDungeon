using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class MapManager : SingletonBehaviour<MapManager>
{
    [Header("Map References")]
    [Tooltip("맵 데이터 ScriptableObject 리스트 (Inspector에서 설정)")]
    public List<MapData> mapDatas = new List<MapData>();

    // ScriptableObject 기반 맵 인덱스 딕셔너리 (mapIndex -> MapData 리스트)
    private Dictionary<int, List<MapData>> mapIndexToDataDict = new Dictionary<int, List<MapData>>();
    
    // 고유한 맵 인덱스 리스트 (정렬된)
    private List<int> uniqueMapIndices = new List<int>();

    // 현재 로드된 맵 씬 Handle
    private AsyncOperationHandle<SceneInstance> currentMapSceneHandle;

    private static int currentMapIndex = 0;
    
    // 초기화 완료 플래그
    public bool IsInitialized { get; private set; } = false;
    
    // 초기화 진행 중 플래그 (정적 변수로 전역 중복 방지)
    private static bool isInitializing = false;
    
    // 맵 로딩 중 플래그 (정적 변수로 전역 중복 방지)
    private static bool isLoadingMap = false;

    public override void Init()
    {
        base.Init();
        
        // 중복 인스턴스는 base.Init()에서 Destroy되므로 즉시 종료
        if (m_Instance != this)
        {
            enabled = false;
            StopAllCoroutines();
            return;
        }
        
        // 이미 초기화 중이면 스킵
        if (isInitializing)
        {
            return;
        }
        
        // 이미 초기화되었지만 맵이 로드되지 않았으면 다시 로드
        if (IsInitialized)
        {
            // 맵 씬이 로드되어 있는지 확인
            bool hasMapScene = false;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.name.StartsWith("Map"))
                {
                    hasMapScene = true;
                    break;
                }
            }
            
            // 맵 씬이 없으면 다시 로드
            if (!hasMapScene)
            {
                StartCoroutine(InitializeMaps());
            }
            return;
        }
        
        // 맵 초기화 시작
        StartCoroutine(InitializeMaps());
    }

    IEnumerator InitializeMaps()
    {
        isInitializing = true;
        
        // 모든 기존 맵 씬 완전히 정리
        yield return StartCoroutine(CleanupAllMapScenes());
        
        // ScriptableObject 기반으로 맵 인덱스화
        BuildMapIndexDictionary();

        // 저장된 맵 인덱스 결정
        int targetMapIndex = 0;
        if (uniqueMapIndices.Count > 0)
        {
            var statusData = UserDataManager.Instance?.Get<UserPlayerStatusData>();
            if (statusData != null && UserDataManager.Instance.ExistsSavedData && uniqueMapIndices.Contains(statusData.CurrentMapIndex))
            {
                targetMapIndex = statusData.CurrentMapIndex;
            }
            else
            {
                targetMapIndex = uniqueMapIndices[0];
            }
        }
        
        // 맵 로드 (성공할 때까지 시도)
        int[] mapIndicesToTry = { targetMapIndex, 0, 1, 2, 3, 4 };
        
        for (int i = 0; i < mapIndicesToTry.Length; i++)
        {
            currentMapIndex = mapIndicesToTry[i];
            yield return StartCoroutine(LoadMapScene(currentMapIndex));
            
            if (currentMapSceneHandle.Status == AsyncOperationStatus.Succeeded)
            {
                break;
            }
        }
        
        IsInitialized = true;
        isInitializing = false;
    }
    
    protected override void OnDestroy()
    {
        // 초기화 플래그 리셋
        if (m_Instance == this)
        {
            isInitializing = false;
        }
        base.OnDestroy();
    }

    /// <summary>
    /// ScriptableObject 기반으로 맵 인덱스 딕셔너리 구축
    /// </summary>
    private void BuildMapIndexDictionary()
    {
        mapIndexToDataDict.Clear();
        uniqueMapIndices.Clear();

        // MapData를 기반으로 인덱스 딕셔너리 구축
        for (int i = 0; i < mapDatas.Count; i++)
        {
            if (mapDatas[i] == null) continue;

            int mapIdx = mapDatas[i].mapIndex;
            
            // MapData 딕셔너리에 추가
            if (!mapIndexToDataDict.ContainsKey(mapIdx))
            {
                mapIndexToDataDict[mapIdx] = new List<MapData>();
                uniqueMapIndices.Add(mapIdx);
            }
            mapIndexToDataDict[mapIdx].Add(mapDatas[i]);
        }

        // 인덱스 정렬
        uniqueMapIndices.Sort();
    }

    /// <summary>
    /// 맵 인덱스에서 씬 주소 생성 (예: 0 -> "Map00", 1 -> "Map01")
    /// </summary>
    private string GetMapSceneAddress(int mapIndex)
    {
        return $"Map{mapIndex:D2}";
    }

    /// <summary>
    /// 맵 씬을 비동기로 로드
    /// </summary>
    private IEnumerator LoadMapScene(int mapIndex, MapData.MapDirection direction = MapData.MapDirection.None)
    {
        // 다른 곳에서 이미 로딩 중이면 대기
        while (isLoadingMap)
        {
            yield return null;
        }
        
        string sceneAddress = GetMapSceneAddress(mapIndex);

        // 이미 로드된 씬이면 스킵
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name == sceneAddress)
            {
                yield break;
            }
        }

        // 이전 맵 씬 언로드
        if (currentMapSceneHandle.IsValid())
        {
            CleanupPoisonAreas();
            yield return Addressables.UnloadSceneAsync(currentMapSceneHandle);
        }
        
        // 다른 맵 씬들도 언로드
        for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.name.StartsWith("Map") && scene.name != sceneAddress && scene.IsValid())
            {
                yield return SceneManager.UnloadSceneAsync(scene);
            }
        }

        isLoadingMap = true;
        
        // Addressable에서 씬 로드
        currentMapSceneHandle = Addressables.LoadSceneAsync(sceneAddress, LoadSceneMode.Additive, false);
        yield return currentMapSceneHandle;

        // 씬 활성화
        if (currentMapSceneHandle.Status == AsyncOperationStatus.Succeeded)
        {
            yield return currentMapSceneHandle.Result.ActivateAsync();
        }
        
        isLoadingMap = false;
    }
    
    /// <summary>
    /// TitleManager에서 로드한 MapData를 추가 (게임 로딩 시 호출)
    /// </summary>
    public void AddMapData(MapData mapData)
    {
        if (mapData != null && !mapDatas.Contains(mapData))
        {
            mapDatas.Add(mapData);
        }
    }

    /// <summary>
    /// 지정된 맵으로 전환하고 플레이어를 해당 위치로 이동 (비동기)
    /// </summary>
    public void TransitionToMap(int mapIndex, Transform playerTransform, MapData.MapDirection direction = MapData.MapDirection.None)
    {
        StartCoroutine(TransitionToMapCoroutine(mapIndex, playerTransform, direction));
    }

    /// <summary>
    /// 맵 전환 코루틴
    /// </summary>
    private IEnumerator TransitionToMapCoroutine(int mapIndex, Transform playerTransform, MapData.MapDirection direction = MapData.MapDirection.None)
    {
        // 범위 검사 (mapDatas가 없어도 맵 전환 허용)
        if (uniqueMapIndices.Count > 0 && !uniqueMapIndices.Contains(mapIndex))
        {
            Logger.LogWarning($"Invalid map index: {mapIndex}");
            yield break;
        }

        // 맵 씬 로드 (LoadMapScene에서 이전 맵 언로드 처리)
        yield return StartCoroutine(LoadMapScene(mapIndex, direction));

        // 플레이어 위치 설정 (방향 고려)
        if (playerTransform != null)
        {
            SetPlayerSpawnPosition(mapIndex, playerTransform, direction);
        }

        // 현재 맵 인덱스 업데이트
        currentMapIndex = mapIndex;

        // 맵 변경 시 자동 저장
        if (UserDataManager.Instance != null)
        {
            var statusData = UserDataManager.Instance.Get<UserPlayerStatusData>();
            if (statusData != null)
            {
                statusData.CurrentMapIndex = currentMapIndex;
            }
        }
    }

    /// <summary>
    /// 플레이어의 스폰 위치 설정
    /// </summary>
    private void SetPlayerSpawnPosition(int mapIndex, Transform playerTransform, MapData.MapDirection direction = MapData.MapDirection.None)
    {
        // MapData에서 스폰 위치 가져오기 (방향 고려)
        if (mapIndexToDataDict.ContainsKey(mapIndex) && mapIndexToDataDict[mapIndex].Count > 0)
        {
            MapData mapData = null;
            
            // 방향이 지정되어 있으면 해당 방향의 MapData 찾기
            if (direction != MapData.MapDirection.None)
            {
                foreach (var data in mapIndexToDataDict[mapIndex])
                {
                    if (data.direction == direction)
                    {
                        mapData = data;
                        break;
                    }
                }
            }
            
            // 방향으로 찾지 못했으면 첫 번째 MapData 사용
            if (mapData == null)
            {
                mapData = mapIndexToDataDict[mapIndex][0];
            }
            
            playerTransform.position = mapData.playerSpawnPosition;
        }
        else
        {
            Logger.LogWarning($"MapData not found for map index {mapIndex}");
        }
    }

    /// <summary>
    /// 현재 맵 인덱스 반환
    /// </summary>
    public int GetCurrentMapIndex()
    {
        return currentMapIndex;
    }

    /// <summary>
    /// 총 고유 맵 개수 반환
    /// </summary>
    public int GetMapCount()
    {
        return uniqueMapIndices.Count;
    }

    /// <summary>
    /// 특정 인덱스의 맵이 존재하는지 확인
    /// </summary>
    public bool HasMapIndex(int mapIndex)
    {
        return uniqueMapIndices.Contains(mapIndex);
    }

    /// <summary>
    /// 현재 맵의 MapData 가져오기 (카메라 제한용)
    /// </summary>
    public MapData GetCurrentMapData()
    {
        if (mapIndexToDataDict.ContainsKey(currentMapIndex) && mapIndexToDataDict[currentMapIndex].Count > 0)
        {
            return mapIndexToDataDict[currentMapIndex][0];
        }
        return null;
    }

    /// <summary>
    /// 모든 맵 씬 정리 (초기화 시 사용)
    /// </summary>
    private IEnumerator CleanupAllMapScenes()
    {
        // 현재 핸들 언로드
        if (currentMapSceneHandle.IsValid())
        {
            CleanupPoisonAreas();
            yield return Addressables.UnloadSceneAsync(currentMapSceneHandle);
        }
        
        // SceneManager로 모든 맵 씬 찾아서 언로드
        for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.name.StartsWith("Map") && scene.IsValid())
            {
                yield return SceneManager.UnloadSceneAsync(scene);
            }
        }
    }

    /// <summary>
    /// 현재 로드된 맵 씬의 모든 PoisonArea 정리 (씬 전환 시 독 상태 초기화 방지)
    /// </summary>
    private void CleanupPoisonAreas()
    {
        // 현재 씬에서만 PoisonArea 찾기 (성능 최적화)
        if (currentMapSceneHandle.IsValid() && currentMapSceneHandle.Status == AsyncOperationStatus.Succeeded)
        {
            var scene = currentMapSceneHandle.Result.Scene;
            if (scene.IsValid() && scene.isLoaded)
            {
                var rootObjects = scene.GetRootGameObjects();
                for (int i = 0; i < rootObjects.Length; i++)
                {
                    var poisonAreas = rootObjects[i].GetComponentsInChildren<PoisonArea>(true);
                    for (int j = 0; j < poisonAreas.Length; j++)
                    {
                        if (poisonAreas[j] != null)
                        {
                            poisonAreas[j].ForceCleanup();
                        }
                    }
                }
            }
        }
    }
}

