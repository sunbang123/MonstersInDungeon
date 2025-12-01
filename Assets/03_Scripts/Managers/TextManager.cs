using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TextManager : MonoBehaviour
{
    public Text dasa;
    public GameObject obFade;
    int dasaCount = 0;
    // Start is called before the first frame update
    List<Dictionary<string, object>> data ;

    public Animator witch;

    void Start()
    {
        StartCoroutine("FadeAction");
        data = CSVReader.Read("story");
        dasa.text = data[dasaCount]["이름"] + " : " + data[dasaCount]["대사"];
        dasaCount++;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            if(dasaCount<data.Count)
            {
                dasa.text = data[dasaCount]["이름"] + " : " + data[dasaCount]["대사"];
                witch.SetTrigger("tell");
                dasaCount++;
            }
            else
            {
                StartCoroutine("FadeAction2");
                
            }            
        }

    }
}
