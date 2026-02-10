using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SearchItinerari : MonoBehaviour
{
    public Text txtRadioPercorso;
    public Text txtRadioNavigazione;
    // Start is called before the first frame update
    void Start()
    {
        var _lingua_selezionata = PlayerPrefs.GetInt("lingua_selezionata");
        txtRadioPercorso.text = _lingua_selezionata == 1 ? "Itinerario" : "Path";
        txtRadioNavigazione.text = _lingua_selezionata == 1 ? "Tecnologia" : "Technology";
    }
}
