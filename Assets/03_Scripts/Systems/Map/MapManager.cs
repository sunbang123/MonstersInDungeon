using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
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

    void Awake()
    {
        // 맵 초기화
        StartCoroutine(InitializeMaps());
    }

    IEnumerator InitializeMaps()
    {
        // ScriptableObject 기반으로 맵 인덱스화
        BuildMapIndexDictionary();

        // 저장된 맵 인덱스 로드
        if (UserDataManager.Instance != null)
        {
            var statusData = UserDataManager.Instance.Get<UserPlayerStatusData>();
            if (statusData != null && UserDataManager.Instance.ExistsSavedData)
            {
                int savedMapIndex = statusData.CurrentMapIndex;
                // 유효한 맵 인덱스인지 확인
                if (uniqueMapIndices.Contains(savedMapIndex))
                {
                    currentMapIndex = savedMapIndex;
                    Logger.Log($"Loaded saved map index: {currentMapIndex}");
                }
                else
                {
                    Logger.LogWarning($"Invalid saved map index: {savedMapIndex}, using default (0)");
                    currentMapIndex = uniqueMapIndices.Count > 0 ? uniqueMapIndices[0] : 0;
                }
            }
            else
            {
                // 저장된 데이터가 없으면 기본값(첫 번째 맵 인덱스) 사용
                currentMapIndex = uniqueMapIndices.Count > 0 ? uniqueMapIndices[0] : 0;
                Logger.Log($"No saved data found, using default map index: {currentMapIndex}");
            }
        }
        else
        {
            currentMapIndex = uniqueMapIndices.Count > 0 ? uniqueMapIndices[0] : 0;
        }

        // 저장된 맵 씬 로드
        yield return StartCoroutine(LoadMapScene(currentMapIndex, MapData.MapDirection.None));

        Logger.Log($"Initialized {uniqueMapIndices.Count} unique map indices, starting with index {currentMapIndex}");
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

        Logger.Log($"Built map index dictionary: {uniqueMapIndices.Count} unique indices");
        foreach (var idx in uniqueMapIndices)
        {
            int dataCount = mapIndexToDataDict.ContainsKey(idx) ? mapIndexToDataDict[idx].Count : 0;
            Logger.Log($"  Map Index {idx}: {dataCount} MapData");
        }
    }

    /// <summary>
    /// 맵 인덱스에서 씬 주소 생성 (예: 0 -> "Map00", 1 -> "Map01")
    /// </summary>
    private string GetMapSceneAddress(int mapIndex)
    {
        return $"Map{mapIndex:D2}";
    }

    /// <summary>
    /// 이름 정규화 (공백, 특수문자 제거)
    /// </summary>
    private string NormalizeName(string name)
    {
        return name.Replace(" ", "").Replace("_", "").Replace("-", "").Replace("(", "").Replace(")", "").ToLower();
    }

    /// <summary>
    /// 맵 이름에서 인덱스 추출 (예: "Map01_N" -> 1, "Map02_S" -> 2, "dangerousarea01" -> 1)
    /// </summary>
    private int ExtractMapIndexFromName(string mapName)
    {
        // 먼저 "Map" 다음의 숫자 추출 시도
        int mapPos = mapName.IndexOf("Map", System.StringComparison.OrdinalIgnoreCase);
        if (mapPos >= 0)
        {
            string remaining = mapName.Substring(mapPos + 3);
            string numberStr = "";
            foreach (char c in remaining)
            {
                if (char.IsDigit(c))
                {
                    numberStr += c;
                }
                else
                {
                    break;
                }
            }
            if (int.TryParse(numberStr, out int parsedIndex))
            {
                return parsedIndex;
            }
        }
        
        // "Map"이 없으면 이름 끝에 있는 숫자 추출 시도 (예: "dangerousarea01" -> 1)
        string reversedName = "";
        for (int i = mapName.Length - 1; i >= 0; i--)
        {
            if (char.IsDigit(mapName[i]))
            {
                reversedName = mapName[i] + reversedName;
            }
            else if (reversedName.Length > 0)
            {
                break;
            }
        }
        
        if (int.TryParse(reversedName, out int extractedIndex))
        {
            return extractedIndex;
        }
        
        // 숫자를 찾을 수 없으면 0 반환
        Logger.LogWarning($"Could not extract map index from name: {mapName}, using 0");
        return 0;
    }

    /// <summary>
    /// 맵 씬을 비동기로 로드
    /// </summary>
    private IEnumerator LoadMapScene(int mapIndex, MapData.MapDirection direction = MapData.MapDirection.None)
    {
        // 현재 맵 씬이 로드되어 있으면 언로드 전에 독 상태 정리
        if (currentMapSceneHandle.IsValid())
        {
            // 씬 언로드 전에 PoisonArea의 독 상태 정리
            CleanupPoisonAreas();
            
            Logger.Log($"Unloading current map scene: Map{currentMapIndex:D2}");
            yield return Addressables.UnloadSceneAsync(currentMapSceneHandle);
        }

        // 새 맵 씬 주소 생성
        string sceneAddress = GetMapSceneAddress(mapIndex);
        Logger.Log($"Loading map scene: {sceneAddress}");

        // Addressable에서 씬 로드 (Additive 모드)
        currentMapSceneHandle = Addressables.LoadSceneAsync(sceneAddress, LoadSceneMode.Additive, false);

        // 로드 완료 대기
        yield return currentMapSceneHandle;

        if (currentMapSceneHandle.Status == AsyncOperationStatus.Succeeded)
        {
            // 씬 활성화
            yield return currentMapSceneHandle.Result.ActivateAsync();
            Logger.Log($"Map scene loaded and activated: {sceneAddress}");
        }
        else
        {
            Logger.LogError($"Failed to load map scene: {sceneAddress}");
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
        // 범위 검사
        if (!uniqueMapIndices.Contains(mapIndex))
        {
            Logger.LogWarning($"Invalid map index: {mapIndex}. Available map indices: {string.Join(", ", uniqueMapIndices)}");
            yield break;
        }

        // 맵 씬 로드
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

        Logger.Log($"Transitioned to map index {mapIndex}, direction: {direction}");
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
                        Logger.Log($"Found MapData for direction {direction}: {mapData.mapName}");
                        break;
                    }
                }
                
                if (mapData == null)
                {
                    Logger.LogWarning($"No MapData found for mapIndex {mapIndex} with direction {direction}, available directions:");
                    foreach (var data in mapIndexToDataDict[mapIndex])
                    {
                        Logger.LogWarning($"  - {data.mapName}: {data.direction}");
                    }
                }
            }
            
            // 방향으로 찾지 못했으면 첫 번째 MapData 사용
            if (mapData == null)
            {
                mapData = mapIndexToDataDict[mapIndex][0];
                Logger.LogWarning($"Using first MapData (no direction specified): {mapData.mapName}, direction: {mapData.direction}");
            }
            
            playerTransform.position = mapData.playerSpawnPosition;
            Logger.Log($"Player spawned at MapData position: {mapData.playerSpawnPosition} (Map: {mapData.mapName}, Index: {mapIndex}, Direction: {mapData.direction})");
        }
        else
        {
            Logger.LogWarning($"Cannot set spawn position: MapData not found for map index {mapIndex}");
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
    /// 현재 로드된 맵 씬의 모든 PoisonArea 정리 (씬 전환 시 독 상태 초기화 방지)
    /// </summary>
    private void CleanupPoisonAreas()
    {
        // 현재 활성화된 모든 씬에서 PoisonArea 찾기
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded)
            {
                var rootObjects = scene.GetRootGameObjects();
                foreach (var rootObj in rootObjects)
                {
                    // 모든 자식에서 PoisonArea 찾기
                    var poisonAreas = rootObj.GetComponentsInChildren<PoisonArea>(true);
                    foreach (var poisonArea in poisonAreas)
                    {
                        if (poisonArea != null)
                        {
                            // 독 상태 강제 정리
                            poisonArea.ForceCleanup();
                        }
                    }
                }
            }
        }
    }
}
