using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TownTrigger : MonoBehaviour
{
    public TownManager townText;
    public string townA;
    public string townB;

    private string lastTown = "";

    void Awake()
    {
        townText = FindObjectOfType<TownManager>();
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        string result = (lastTown == townA) ? townB : townA;
        lastTown = result;

        townText.SetTownName(result);
    }
    
}
