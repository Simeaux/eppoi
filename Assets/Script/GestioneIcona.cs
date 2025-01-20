using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GestioneIcona : MonoBehaviour
{

    public Text id;
    public Text tipologia;
    public GameObject distanza;
    public GameObject Lunghezza;
    public GameObject immagine_tipo_percorso;
    public GameObject immagine_tipo_navigazione;
    public GameObject tipo_Itinerario;
    public GameObject tipo_POI;
    public GameObject immagine_tipo_poi;
    private int _lingua_selezionata = 1;


    // Start is called before the first frame update
    void Start()
    {
        _lingua_selezionata = PlayerPrefs.GetInt("lingua_selezionata");
        if (_lingua_selezionata == 2)
        {
            Debug.Log("distanza");
            foreach (var _cmp in distanza.GetComponentsInChildren<Text>())
            {
                Debug.Log(_cmp.name);
                if (_cmp.name == "Distanza")
                    _cmp.text = _cmp.text.Replace( "km dal centro", "km from the center");
            }
            

        }
        //se è un poi
        if (tipologia.text == "3")
        {
            //immagine_tipo_percorso.SetActive(false);
            //immagine_tipo_navigazione.SetActive(false);
            distanza.SetActive(true);
            Lunghezza.SetActive(false);
            tipo_Itinerario.SetActive(false);
            tipo_POI.SetActive(true);
        }
        else if (tipologia.text == "2") //evento
        {
            //immagine_tipo_percorso.SetActive(false);
            //immagine_tipo_navigazione.SetActive(false);
            distanza.SetActive(false);
            Lunghezza.SetActive(false);
            tipo_Itinerario.SetActive(false);
            tipo_POI.SetActive(false);
        }
        else // itinerario
        {
            //immagine_tipo_percorso.SetActive(true);
            //immagine_tipo_navigazione.SetActive(true);
            distanza.SetActive(false);
            Lunghezza.SetActive(true);
            tipo_Itinerario.SetActive(true);
            tipo_POI.SetActive(false);
        }
    }
}
