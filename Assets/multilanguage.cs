using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class multilanguage : MonoBehaviour
{
    public Text label;
    public string testo_tradotto;

    // Start is called before the first frame update
    void Start()
    {
        var _lingua_selezionata = PlayerPrefs.GetInt("lingua_selezionata");
        if (_lingua_selezionata > 1)
            label.text = testo_tradotto;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
