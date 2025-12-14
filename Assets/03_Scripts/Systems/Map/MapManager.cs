using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("Map References")]
    [Tooltip("맵 GameObject 리스트 (Inspector에서 설정)")]
    public List<GameObject> maps = new List<GameObject>();
    
    [Tooltip("맵 데이터 ScriptableObject 리스트 (Inspector에서 설정)")]
    public List<MapData> mapDatas = new List<MapData>();

    // ScriptableObject 기반 맵 인덱스 딕셔너리 (mapIndex -> MapData 리스트)
    private Dictionary<int, List<MapData>> mapIndexToDataDict = new Dictionary<int, List<MapData>>();
    
    // ScriptableObject 기반 맵 인덱스 딕셔너리 (mapIndex -> GameObject 리스트)
    private Dictionary<int, List<GameObject>> mapIndexToGameObjectDict = new Dictionary<int, List<GameObject>>();
    
    // 방향별 맵 딕셔너리 (mapIndex -> direction -> GameObject)
    private Dictionary<int, Dictionary<MapData.MapDirection, GameObject>> mapIndexDirectionToGameObjectDict = new Dictionary<int, Dictionary<MapData.MapDirection, GameObject>>();
    
    // 고유한 맵 인덱스 리스트 (정렬된)
    private List<int> uniqueMapIndices = new List<int>();

    private static int currentMapIndex = 0;

    void Awake()
    {
        // 맵 초기화
        InitializeMaps();
    }

    void InitializeMaps()
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

        // 저장된 맵 활성화, 나머지 비활성화 (방향 없이 활성화)
        ActivateMapByIndex(currentMapIndex, MapData.MapDirection.None);

        Logger.Log($"Initialized {uniqueMapIndices.Count} unique map indices, starting with index {currentMapIndex}");
    }

    /// <summary>
    /// ScriptableObject 기반으로 맵 인덱스 딕셔너리 구축
    /// </summary>
    private void BuildMapIndexDictionary()
    {
        mapIndexToDataDict.Clear();
        mapIndexToGameObjectDict.Clear();
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

        // MapData를 이름으로 빠르게 조회하기 위한 딕셔너리 구축 (O(1) 조회)
        Dictionary<string, MapData> mapDataByNameDict = new Dictionary<string, MapData>();
        Dictionary<string, MapData> mapDataByNormalizedNameDict = new Dictionary<string, MapData>();
        
        foreach (var mapData in mapDatas)
        {
            if (mapData == null) continue;
            
            // gameObjectName이 있으면 우선 사용
            if (!string.IsNullOrEmpty(mapData.gameObjectName))
            {
                string key = mapData.gameObjectName.ToLower();
                if (!mapDataByNameDict.ContainsKey(key))
                {
                    mapDataByNameDict[key] = mapData;
                }
            }
            
            // mapName도 인덱스로 추가
            string normalizedName = NormalizeName(mapData.mapName);
            if (!mapDataByNormalizedNameDict.ContainsKey(normalizedName))
            {
                mapDataByNormalizedNameDict[normalizedName] = mapData;
            }
        }

        // MapData를 기준으로 GameObject 찾아서 매칭 (같은 GameObject가 여러 방향에 사용될 수 있음)
        foreach (var mapData in mapDatas)
        {
            if (mapData == null) continue;
            
            int mapIdx = mapData.mapIndex;
            GameObject matchedGameObject = null;
            
            // gameObjectName으로 GameObject 찾기
            if (!string.IsNullOrEmpty(mapData.gameObjectName))
            {
                foreach (var map in maps)
                {
                    if (map != null && map.name.Equals(mapData.gameObjectName, System.StringComparison.OrdinalIgnoreCase))
                    {
                        matchedGameObject = map;
                        break;
                    }
                }
            }
            
            // GameObject를 찾지 못했으면 이름으로 찾기
            if (matchedGameObject == null)
            {
                string normalizedDataName = NormalizeName(mapData.mapName);
                foreach (var map in maps)
                {
                    if (map == null) continue;
                    string normalizedMapName = NormalizeName(map.name);
                    if (normalizedMapName.Contains(normalizedDataName) || normalizedDataName.Contains(normalizedMapName))
                    {
                        matchedGameObject = map;
                        break;
                    }
                }
            }
            
            // GameObject 딕셔너리 초기화
            if (!mapIndexToGameObjectDict.ContainsKey(mapIdx))
            {
                mapIndexToGameObjectDict[mapIdx] = new List<GameObject>();
                mapIndexDirectionToGameObjectDict[mapIdx] = new Dictionary<MapData.MapDirection, GameObject>();
                if (!uniqueMapIndices.Contains(mapIdx))
                {
                    uniqueMapIndices.Add(mapIdx);
                }
            }
            
            // GameObject 추가 (중복 방지)
            if (matchedGameObject != null && !mapIndexToGameObjectDict[mapIdx].Contains(matchedGameObject))
            {
                mapIndexToGameObjectDict[mapIdx].Add(matchedGameObject);
                Logger.Log($"Matched MapData '{mapData.mapName}' with GameObject '{matchedGameObject.name}' (Index: {mapIdx}, Direction: {mapData.direction})");
            }
            
            // 방향별 딕셔너리에 추가 (같은 GameObject를 여러 방향에 매핑)
            if (matchedGameObject != null && mapData.direction != MapData.MapDirection.None)
            {
                mapIndexDirectionToGameObjectDict[mapIdx][mapData.direction] = matchedGameObject;
            }
        }

        // 인덱스 정렬
        uniqueMapIndices.Sort();

        Logger.Log($"Built map index dictionary: {uniqueMapIndices.Count} unique indices");
        foreach (var idx in uniqueMapIndices)
        {
            int dataCount = mapIndexToDataDict.ContainsKey(idx) ? mapIndexToDataDict[idx].Count : 0;
            int goCount = mapIndexToGameObjectDict.ContainsKey(idx) ? mapIndexToGameObjectDict[idx].Count : 0;
            Logger.Log($"  Map Index {idx}: {goCount} GameObjects, {dataCount} MapData");
        }
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
    /// 지정된 맵 인덱스와 방향의 맵 활성화
    /// </summary>
    private void ActivateMapByIndex(int mapIndex, MapData.MapDirection direction = MapData.MapDirection.None)
    {
        // 모든 맵 비활성화
        foreach (var mapList in mapIndexToGameObjectDict.Values)
        {
            foreach (var map in mapList)
            {
                if (map != null)
                {
                    map.SetActive(false);
                }
            }
        }

        // 지정된 인덱스의 맵 활성화 (같은 GameObject이므로 방향과 무관하게 활성화)
        if (mapIndexToGameObjectDict.ContainsKey(mapIndex))
        {
            // 같은 인덱스의 모든 GameObject 활성화 (보통 하나)
            foreach (var map in mapIndexToGameObjectDict[mapIndex])
            {
                if (map != null)
                {
                    map.SetActive(true);
                    Logger.Log($"Activated map: {map.name} (Index: {mapIndex}, Direction: {direction})");
                }
            }
        }
        else
        {
            Logger.LogWarning($"Map index {mapIndex} not found in dictionary");
        }
    }

    /// <summary>
    /// 지정된 맵으로 전환하고 플레이어를 해당 위치로 이동
    /// </summary>
    public void TransitionToMap(int mapIndex, Transform playerTransform, MapData.MapDirection direction = MapData.MapDirection.None)
    {
        // 범위 검사
        if (!uniqueMapIndices.Contains(mapIndex))
        {
            Logger.LogWarning($"Invalid map index: {mapIndex}. Available map indices: {string.Join(", ", uniqueMapIndices)}");
            return;
        }

        // 맵 전환 (방향 고려)
        ActivateMapByIndex(mapIndex, direction);

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
            // MapData가 없으면 첫 번째 GameObject의 위치 사용 (fallback)
            if (mapIndexToGameObjectDict.ContainsKey(mapIndex) && mapIndexToGameObjectDict[mapIndex].Count > 0)
            {
                GameObject targetMap = mapIndexToGameObjectDict[mapIndex][0];
                if (targetMap != null)
                {
                    Vector3 mapCenter = targetMap.transform.position;
                    playerTransform.position = new Vector3(mapCenter.x, playerTransform.position.y, mapCenter.z);
                    Logger.Log($"Player spawned at map center (fallback): {playerTransform.position}");
                }
            }
            else
            {
                Logger.LogWarning($"Cannot set spawn position: invalid map index {mapIndex}");
            }
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
}
