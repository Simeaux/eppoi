using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Panel_Header : MonoBehaviour
{
    public GameObject CanvasComune;
    public GameObject CanvasPOI;
    public Text nome_comune;
    public Text lblregione;
    public Text lblprovincia;
    public Text lblcoordinate;
    public Text lblaltitudine;
    public Text lblabitanti;
    public Text regione;
    public Text provincia;
    public Text coordinate;
    public Text altitudine;
    public Text abitanti;
    public InputField txtSearch;
    public bool only_blank = false;

    private int _lingua_selezionata = 1;
    private DBClass _DBClass;

    private void Start()
    {
        //CanvasSearch.SetActive(!only_blank);
        _lingua_selezionata = PlayerPrefs.GetInt("lingua_selezionata");
        lblregione.text = _lingua_selezionata == 1 ? "Regione" : "Region";
        lblprovincia.text = _lingua_selezionata == 1 ? "Provincia" : "Province";
        lblcoordinate.text = _lingua_selezionata == 1 ? "Coordinate" : "Coordinates";
        lblaltitudine.text = _lingua_selezionata == 1 ? "Altitudine" : "Altitude";
        lblabitanti.text = _lingua_selezionata == 1 ? "Abitanti" : "Inhabitants";
    }
    private void Update()
    {
        if (!only_blank)
        {
            if (PlayerPrefs.GetInt("percorso_selezionato") > 0 || PlayerPrefs.GetInt("poi_selezionato") > 0)
            {
                if (!CanvasPOI.activeSelf)
                {
                    CanvasComune.SetActive(false);
                    CanvasPOI.SetActive(true);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(PlayerPrefs.GetString("istat")) && !CanvasComune.activeSelf)
                {
                    CanvasComune.SetActive(true);
                    CanvasPOI.SetActive(false);
                }
                if (string.IsNullOrEmpty(PlayerPrefs.GetString("istat")))
                {
                    CanvasComune.SetActive(false);
                    CanvasPOI.SetActive(false);
                }
            }
        }
    }

    private void OnEnable()
    {
        if (!only_blank)
        {
            _DBClass = GameObject.FindWithTag("SQLite").GetComponent<DBClass>();
            if (!string.IsNullOrEmpty(PlayerPrefs.GetString("istat")))
            {
                var _Listcomune = _DBClass.GetCOMUNI(PlayerPrefs.GetString("istat"));
                if (_Listcomune != null && _Listcomune.Count > 0)
                {
                    nome_comune.text = _Listcomune[0].nome_comune;
                    regione.text = _Listcomune[0].regione;
                    provincia.text = _Listcomune[0].provincia;
                    coordinate.text = _Listcomune[0].latitudine.ToString("##.#####").Replace(".", ",") + "°N - " + _Listcomune[0].longitudine.ToString("##.#####").Replace(".", ",") + (_lingua_selezionata == 1 ? "°E" : "°W");
                    altitudine.text = _Listcomune[0].altitudine.ToString("##,###").Replace(",", ".") + (_lingua_selezionata == 1 ? " m.s.l.m." : " m above s.l.");
                    abitanti.text = _Listcomune[0].abitanti.ToString("###,###").Replace(",", ".");

                }
            }
        }
    }
}
