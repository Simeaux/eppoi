using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonPOI : MonoBehaviour
{
    public Text _distanza;
    private int _lingua_selezionata = 1;

    private void Start()
    {
        _lingua_selezionata = PlayerPrefs.GetInt("lingua_selezionata");
        _distanza.text = _distanza.text.Replace("km da te", "km from you");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
