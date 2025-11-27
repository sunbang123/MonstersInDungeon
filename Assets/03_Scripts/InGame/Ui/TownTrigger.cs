using System.Collections.Generic;
using UnityEngine;

public class TownTrigger : MonoBehaviour
{
    [Header("MapSet")]
    public List<GameObject> map;
    public List<Transform> spawnPoint;

    public TownUIManager townText;
    public string townA;
    public string townB;

    private string lastTown = "";

    void Awake()
    {
        townText = FindObjectOfType<TownUIManager>();
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        string result = (lastTown == townA) ? townB : townA;
        lastTown = result;

        townText.SetTownName(result);

        if (map.Count >= 2)
        {
            map[0].SetActive(!map[0].activeSelf);
            map[1].SetActive(!map[1].activeSelf);
        }

        if(spawnPoint.Count >= 2)
        {
            other.transform.position = spawnPoint[(lastTown == townA) ? 0 : 1].position;
            this.transform.position = spawnPoint[(lastTown == townA) ? 0 : 1].position;
        }
    }
}
