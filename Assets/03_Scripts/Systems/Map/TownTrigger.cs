using System.Collections.Generic;
using UnityEngine;

public class TownTrigger : MonoBehaviour
{
    [Header("MapSet")]
    public List<GameObject> map;
    public MapData mapData;
    public TownUIManager townText;
    public int mapIndex;

    void Awake()
    {
        townText = FindObjectOfType<TownUIManager>();
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        townText.SetTownName(mapData.mapName);

        if (map.Count >= 2)
        {
            map[0].SetActive(!map[0].activeSelf);
            map[1].SetActive(!map[1].activeSelf);
        }

        other.transform.position = mapData.playerSpawnPosition;
        CameraController.instance.currentMapIndex = mapIndex;
    }
}
