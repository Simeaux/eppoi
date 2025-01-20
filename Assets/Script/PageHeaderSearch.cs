using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageHeaderSearch : MonoBehaviour
{
    public GameObject searchItinerari;
    public GameObject searchpoi;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerPrefs.GetInt("show_itinerari") == 1 && !searchItinerari.activeSelf)
        {
            searchItinerari.SetActive(!searchItinerari.activeSelf);
        }
        else if (PlayerPrefs.GetInt("show_itinerari") != 1 && searchItinerari.activeSelf)
        {
            searchItinerari.SetActive(!searchItinerari.activeSelf);
        }
        if (PlayerPrefs.GetInt("show_poi") == 1 && !searchpoi.activeSelf)
        {
            searchpoi.SetActive(!searchpoi.activeSelf);
        }
        else if (PlayerPrefs.GetInt("show_poi") != 1 && searchpoi.activeSelf)
        {
            searchpoi.SetActive(!searchpoi.activeSelf);
        }
    }
}
