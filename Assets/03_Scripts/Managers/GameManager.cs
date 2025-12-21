using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonBehaviour<GameManager>
{
    // 카테고리별로 프리팹을 저장하는 딕셔너리
    private Dictionary<string, List<GameObject>> prefabPool = new Dictionary<string, List<GameObject>>();

    // 프리팹 이름으로 프리팹을 찾기 위한 딕셔너리
    private Dictionary<string, GameObject> prefabByName = new Dictionary<string, GameObject>();

    protected override void Init()
    {
        base.Init();

        // GameManager는 씬 전환 시에도 유지
        DontDestroyOnLoad(gameObject);

        Logger.Log("GameManager initialized");
    }

    /// <summary>
    /// Addressable에서 로드한 프리팹을 등록하는 함수
    /// </summary>
    public void RegisterPrefab(string category, GameObject prefab)
    {
        if (prefab == null)
        {
            Logger.LogWarning($"Trying to register null prefab in category: {category}");
            return;
        }

        // 카테고리별 리스트 초기화
        if (!prefabPool.ContainsKey(category))
        {
            prefabPool[category] = new List<GameObject>();
        }

        // 중복 확인
        if (prefabPool[category].Contains(prefab))
        {
            Logger.LogWarning($"Prefab {prefab.name} already registered in {category}");
            return;
        }

        // 프리팹 추가
        prefabPool[category].Add(prefab);
        prefabByName[prefab.name] = prefab;

        Logger.Log($"Registered prefab: {prefab.name} in category: {category}");
    }

    /// <summary>
    /// 카테고리별로 프리팹 리스트를 반환
    /// </summary>
    public List<GameObject> GetPrefabsByCategory(string category)
    {
        if (prefabPool.ContainsKey(category))
        {
            return prefabPool[category];
        }

        Logger.LogWarning($"Category not found: {category}");
        return new List<GameObject>();
    }

    /// <summary>
    /// 프리팹 이름으로 프리팹을 반환
    /// </summary>
    public GameObject GetPrefabByName(string prefabName)
    {
        if (prefabByName.ContainsKey(prefabName))
        {
            return prefabByName[prefabName];
        }

        Logger.LogWarning($"Prefab not found: {prefabName}");
        return null;
    }

    /// <summary>
    /// 프리팹 인스턴스 생성
    /// </summary>
    public GameObject InstantiatePrefab(string prefabName, Vector3 position, Quaternion rotation)
    {
        GameObject prefab = GetPrefabByName(prefabName);

        if (prefab == null)
        {
            Logger.LogError($"Cannot instantiate. Prefab not found: {prefabName}");
            return null;
        }

        return Instantiate(prefab, position, rotation);
    }

    /// <summary>
    /// 프리팹 인스턴스 생성 (위치만)
    /// </summary>
    public GameObject InstantiatePrefab(string prefabName, Vector3 position)
    {
        return InstantiatePrefab(prefabName, position, Quaternion.identity);
    }

    /// <summary>
    /// 프리팹 인스턴스 생성 (기본 위치)
    /// </summary>
    public GameObject InstantiatePrefab(string prefabName)
    {
        return InstantiatePrefab(prefabName, Vector3.zero, Quaternion.identity);
    }

    /// <summary>
    /// 카테고리에서 랜덤 프리팹을 반환
    /// </summary>
    public GameObject GetRandomPrefabFromCategory(string category)
    {
        var list = GetPrefabsByCategory(category);

        if (list.Count == 0)
        {
            Logger.LogWarning($"No prefabs in category: {category}");
            return null;
        }

        return list[Random.Range(0, list.Count)];
    }

    /// <summary>
    /// 카테고리에서 랜덤 프리팹 인스턴스 생성
    /// </summary>
    public GameObject InstantiateRandomFromCategory(string category, Vector3 position, Quaternion rotation)
    {
        GameObject prefab = GetRandomPrefabFromCategory(category);

        if (prefab == null)
        {
            return null;
        }

        return Instantiate(prefab, position, rotation);
    }

    /// <summary>
    /// 로드된 모든 프리팹 목록 출력 (디버깅용)
    /// </summary>
    public void PrintLoadedPrefabs()
    {
        Logger.Log("=== Loaded Prefabs ===");

        foreach (var category in prefabPool)
        {
            Logger.Log($"Category: {category.Key} ({category.Value.Count} prefabs)");

            foreach (var prefab in category.Value)
            {
                Logger.Log($"  - {prefab.name}");
            }
        }
    }

    /// <summary>
    /// 특정 카테고리가 로드되었는지 확인
    /// </summary>
    public bool IsCategoryLoaded(string category)
    {
        return prefabPool.ContainsKey(category) && prefabPool[category].Count > 0;
    }

    /// <summary>
    /// 특정 프리팹이 로드되었는지 확인
    /// </summary>
    public bool IsPrefabLoaded(string prefabName)
    {
        return prefabByName.ContainsKey(prefabName);
    }

    /// <summary>
    /// 풀 초기화
    /// </summary>
    public void ClearPool()
    {
        prefabPool.Clear();
        prefabByName.Clear();
        Logger.Log("Prefab pool cleared");
    }
}
