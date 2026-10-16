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
    public GameObject ButtonComuni_scaricati;
    public Text NumeroComuni_scaricati;
    public Text PesoComuni_scaricati;
    public Image imgComune_1;
    public Image imgComune_2;
    public Image imgComune_3;

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
            if (PlayerPrefs.GetString("percorso_selezionato") != "" || PlayerPrefs.GetString("poi_selezionato") != "")
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
                    if (ButtonComuni_scaricati != null)
                        ButtonComuni_scaricati.SetActive(false);
                }
                else if (string.IsNullOrEmpty(PlayerPrefs.GetString("istat")))
                {
                    _DBClass = GameObject.FindWithTag("SQLite").GetComponent<DBClass>();
                    CanvasComune.SetActive(false);
                    CanvasPOI.SetActive(false);
                    if (ButtonComuni_scaricati != null)
                    {
                        ButtonComuni_scaricati.SetActive(true);
                    }
                    var _Listcomune = _DBClass.GetCOMUNI(null, null, null, null, null, true);
                    int n_comuni = _Listcomune.Count;
                    string testo = n_comuni.ToString() + " " + (_lingua_selezionata == 1 ? "Comuni offline" : "Offline municipalities");
                    if (NumeroComuni_scaricati != null && NumeroComuni_scaricati.text != testo && imgComune_1 != null && imgComune_2 != null && imgComune_3 != null)
                    {
                        NumeroComuni_scaricati.text = n_comuni.ToString() + " " + (_lingua_selezionata == 1 ? "Comuni offline" : "Offline municipalities");
                        // ---------------------------------------------------------
                        // PESO IMMAGINI
                        // ---------------------------------------------------------

                        long peso_immagini =
                            _DBClass.GetIMMAGINI_PesoTabella();

                        // ---------------------------------------------------------
                        // PESO TESTI
                        // ---------------------------------------------------------

                        long peso_testi =
                            _DBClass.GetTEXT_PesoTabella();
                        if (PesoComuni_scaricati != null)
                        {
                            if (peso_immagini + peso_testi > 0)
                            {
                                PesoComuni_scaricati.gameObject.SetActive(true);
                                PesoComuni_scaricati.text = _DBClass.FormattaDimensione(peso_immagini + peso_testi) + (_lingua_selezionata == 1 ? " sul telefono" : " of data");
                            }
                            else
                            {
                                PesoComuni_scaricati.gameObject.SetActive(false);
                            }
                        }



                        imgComune_1.gameObject.SetActive(false);
                        imgComune_2.gameObject.SetActive(false);
                        imgComune_3.gameObject.SetActive(false);

                        if (n_comuni >= 1)
                        {
                            if (_Listcomune != null && _Listcomune.Count > 0)
                            {
                                if (_Listcomune[0].Listimages != null && _Listcomune[0].Listimages.Count > 0)
                                {
                                    Texture2D texture =
                                        new Texture2D(
                                            2,
                                            2,
                                            TextureFormat.RGBA32,
                                            false
                                        );

                                    bool caricata =
                                        texture.LoadImage(_Listcomune[0].Listimages[0].image);

                                    if (caricata)
                                    {
                                        Sprite sprite =
                                            Sprite.Create(
                                                texture,
                                                new Rect(
                                                    0,
                                                    0,
                                                    texture.width,
                                                    texture.height
                                                ),
                                                new Vector2(0.5f, 0.5f)
                                            );
                                        imgComune_1.sprite = sprite;
                                        imgComune_1.preserveAspect = false;
                                        imgComune_1.gameObject.SetActive(true);
                                    }
                                }
                            }
                            if (n_comuni >= 2)
                            {
                                if (_Listcomune[1].Listimages != null && _Listcomune[1].Listimages.Count > 0)
                                {
                                    Texture2D texture =
                                        new Texture2D(
                                            2,
                                            2,
                                            TextureFormat.RGBA32,
                                            false
                                        );

                                    bool caricata =
                                        texture.LoadImage(_Listcomune[1].Listimages[0].image);

                                    if (caricata)
                                    {
                                        Sprite sprite =
                                            Sprite.Create(
                                                texture,
                                                new Rect(
                                                    0,
                                                    0,
                                                    texture.width,
                                                    texture.height
                                                ),
                                                new Vector2(0.5f, 0.5f)
                                            );
                                        imgComune_2.sprite = sprite;
                                        imgComune_2.preserveAspect = false;
                                        imgComune_2.gameObject.SetActive(true);
                                    }
                                }
                            }
                            if (n_comuni >= 3)
                            {
                                if (_Listcomune[2].Listimages != null && _Listcomune[2].Listimages.Count > 0)
                                {
                                    Texture2D texture =
                                        new Texture2D(
                                            2,
                                            2,
                                            TextureFormat.RGBA32,
                                            false
                                        );

                                    bool caricata =
                                        texture.LoadImage(_Listcomune[2].Listimages[0].image);

                                    if (caricata)
                                    {
                                        Sprite sprite =
                                            Sprite.Create(
                                                texture,
                                                new Rect(
                                                    0,
                                                    0,
                                                    texture.width,
                                                    texture.height
                                                ),
                                                new Vector2(0.5f, 0.5f)
                                            );
                                        imgComune_3.sprite = sprite;
                                        imgComune_3.preserveAspect = false;
                                        imgComune_3.gameObject.SetActive(true);
                                    }
                                }
                            }
                        }
                    }
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
                    while (_Listcomune[0].latitudine > 100)
                        _Listcomune[0].latitudine = _Listcomune[0].latitudine / 10;
                    while (_Listcomune[0].longitudine > 100)
                        _Listcomune[0].longitudine = _Listcomune[0].longitudine / 10;
                    coordinate.text = _Listcomune[0].latitudine.ToString("##.#####").Replace(".", ",") + "°N - " + _Listcomune[0].longitudine.ToString("##.#####").Replace(".", ",") + (_lingua_selezionata == 1 ? "°E" : "°W");
                    altitudine.text = _Listcomune[0].altitudine.ToString("##,###").Replace(",", ".") + (_lingua_selezionata == 1 ? " m.s.l.m." : " m above s.l.");
                    abitanti.text = _Listcomune[0].abitanti.ToString("###,###").Replace(",", ".");

                }
            }
        }
    }
}
