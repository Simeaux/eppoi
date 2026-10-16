using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using TS.PageSlider;
using TS.PageSlider.Demo;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static DBClass;

public class Panel_Comune : MonoBehaviour
{
    public TMPro.TMP_Text descrizione;
    public GameObject content_list_img;
    public PageSlider scrollviewFotoComune;
    public SliderPage foto_comune;
    public UnityEngine.UI.Button backBtn;
    public GameObject _panelComune;
    public GameObject _panelPrincipale;
    public GameObject _panelPOI;
    public Text selected_tab;
    public Text txtDescrizione;
    public Text txtItinerari;
    public Text txtPuntiDiInteresse;
    public Text txtEventi;
    public Text txtNotizie;


    public GameObject _scrollDesccrizione;
    public GameObject _scrollOggetti;

    private int _lingua_selezionata = 1;
    private string _istat = "";
    private DBClass _DBClass;
    private COMUNE _selected_comune = new COMUNE();
    private List<SliderPage> GOList = new List<SliderPage>();

    private void Start()
    {
        _lingua_selezionata = PlayerPrefs.GetInt("lingua_selezionata");

        txtDescrizione.text = _lingua_selezionata == 1 ? "Descrizione" : "Description";
        txtItinerari.text = _lingua_selezionata == 1 ? "Itinerari" : "Itineraries";
        txtPuntiDiInteresse.text = _lingua_selezionata == 1 ? "Punti di interesse" : "Points of interest";
        txtEventi.text = _lingua_selezionata == 1 ? "Eventi" : "Events";
        txtNotizie.text = _lingua_selezionata == 1 ? "Notizie" : "News";
    }
    private void Update()
    {

        if ((PlayerPrefs.GetString("poi_selezionato") != "" || PlayerPrefs.GetString("percorso_selezionato") != "") && !_panelPOI.activeSelf)
        {
            _panelPOI.SetActive(true);
            _panelComune.SetActive(false);

        }
    }
    private void OnGUI()
    {

        //GUI.DrawTexture(new Rect(200, 20, 400, 400), texture, ScaleMode.ScaleToFit, true, 1f);
    }
    private void OnEnable()
    {
        // Verifica se è già carica, cadsomai la distrugge
        if (SceneManager.GetSceneByName("Map").isLoaded)
        {
            SceneManager.UnloadSceneAsync("Map");
        }
        _DBClass = GameObject.FindWithTag("SQLite").GetComponent<DBClass>();


        ///
        backBtn.onClick.AddListener(backBtnPressed);

        _lingua_selezionata = PlayerPrefs.GetInt("lingua_selezionata");
        _istat = PlayerPrefs.GetString("istat");

        var _Listcomune = _DBClass.GetCOMUNI(_istat);
        descrizione.text = string.Empty;
        scrollviewFotoComune.gameObject.SetActive(true);




        if (_Listcomune != null && _Listcomune.Count > 0)
        {
            var _comune = _Listcomune[0];
            scrollviewFotoComune.transform.SetParent(descrizione.transform, false);
            _selected_comune = _comune;


            var _panelLink = scrollviewFotoComune.gameObject.transform.Find("PanelLink");
            if (_panelLink != null)
            {
                _panelLink.gameObject.SetActive(false);
            }
            var _panelItinerario = scrollviewFotoComune.gameObject.transform.Find("PanelItinerario");
            if (_panelItinerario != null)
                _panelItinerario.gameObject.SetActive(false);
            var _panelComune = scrollviewFotoComune.gameObject.transform.Find("PanelComune");
            if (_panelComune != null)
            {
                _panelComune.gameObject.SetActive(true);
                RectTransform rect = _panelComune.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchoredPosition = new Vector2(0, -430f);
                }
                var _oggetticomune = _panelComune.gameObject.transform.Find("regione");
                if (_comune.regione != null && !string.IsNullOrEmpty(_comune.regione))
                {
                    if (_oggetticomune != null)
                    {
                        Text _textLabel = _oggetticomune.gameObject.transform.Find("Label").GetComponent<Text>();
                        if (_textLabel != null)
                            _textLabel.text = _lingua_selezionata == 1 ? "REGIONE" : "REGION";
                        Text _textTitolo = _oggetticomune.gameObject.transform.Find("Titolo").GetComponent<Text>();
                        if (_textTitolo != null)
                            _textTitolo.text = _comune.regione;
                    }
                }
                _oggetticomune = _panelComune.gameObject.transform.Find("provincia");
                if (_comune.provincia != null && !string.IsNullOrEmpty(_comune.provincia))
                {
                    if (_oggetticomune != null)
                    {
                        Text _textLabel = _oggetticomune.gameObject.transform.Find("Label").GetComponent<Text>();
                        if (_textLabel != null)
                            _textLabel.text = _lingua_selezionata == 1 ? "PROVINCIA" : "PROVINCE";
                        Text _textTitolo = _oggetticomune.gameObject.transform.Find("Titolo").GetComponent<Text>();
                        if (_textTitolo != null)
                            _textTitolo.text = _comune.provincia;
                    }
                }
                _oggetticomune = _panelComune.gameObject.transform.Find("coordinate");
                if (_comune.latitudine > 0)
                {
                    if (_oggetticomune != null)
                    {
                        Text _textLabel = _oggetticomune.gameObject.transform.Find("Label").GetComponent<Text>();
                        if (_textLabel != null)
                            _textLabel.text = _lingua_selezionata == 1 ? "COORDINATE" : "COORDINATES";
                        Text _textTitolo = _oggetticomune.gameObject.transform.Find("Titolo").GetComponent<Text>();

                        while (_comune.latitudine > 100)
                            _comune.latitudine = _comune.latitudine / 10;
                        while (_comune.longitudine > 100)
                            _comune.longitudine = _comune.longitudine / 10;
                        var _descrizione = _comune.latitudine.ToString("##.#####").Replace(".", ",") + "°N - " + _comune.longitudine.ToString("##.#####").Replace(".", ",") + (_lingua_selezionata == 1 ? "°E" : "°W");

                        if (_textTitolo != null)
                            _textTitolo.text = _descrizione;
                    }
                }
                _oggetticomune = _panelComune.gameObject.transform.Find("altitudine");
                if (_comune.altitudine > 0)
                {
                    if (_oggetticomune != null)
                    {
                        Text _textLabel = _oggetticomune.gameObject.transform.Find("Label").GetComponent<Text>();
                        if (_textLabel != null)
                            _textLabel.text = _lingua_selezionata == 1 ? "ALTITUDINE" : "ALTITUDE";
                        Text _textTitolo = _oggetticomune.gameObject.transform.Find("Titolo").GetComponent<Text>();
                        if (_textTitolo != null)
                            _textTitolo.text = _comune.altitudine.ToString("##,###").Replace(",", ".") + (_lingua_selezionata == 1 ? " m.s.l.m." : " m above s.l.");
                    }
                }
                _oggetticomune = _panelComune.gameObject.transform.Find("abitanti");
                if (_comune.altitudine > 0)
                {
                    if (_oggetticomune != null)
                    {
                        Text _textLabel = _oggetticomune.gameObject.transform.Find("Label").GetComponent<Text>();
                        if (_textLabel != null)
                            _textLabel.text = _lingua_selezionata == 1 ? "ABITANTI" : "INHABITANTS";
                        Text _textTitolo = _oggetticomune.gameObject.transform.Find("Titolo").GetComponent<Text>();
                        if (_textTitolo != null)
                            _textTitolo.text = _comune.abitanti.ToString("###,###").Replace(",", ".");
                    }
                }

            }

            if (_comune.Listimages != null && _comune.Listimages.Count > 0 && _comune.Listimages[0].image != null && _comune.Listimages[0].image.Length > 0)
            {
                descrizione.text = "\n\n\n\n\n\n\n\n\n\n\n\n\n";

                foto_comune.gameObject.SetActive(true);
                float _dimensione_foto = 1240;
                if (GOList != null && GOList.Count > 0)
                {
                    Debug.Log($"foto da distruggere {GOList.Count}");
                    foreach (var _g in GOList)
                        _g.gameObject.Destroy();
                    GOList = new List<SliderPage>();
                }
                int _i = 0;
                foreach (var _img in _comune.Listimages)
                {
                    /*
                    var page = Instantiate(foto_comune);
                    page.sprite = _DBClass.getSpriteFromByteArray(_img.image);

                    scrollviewFotoComune.AddPage((RectTransform)page.transform);
                    */


                    var goApp = Instantiate(foto_comune);
                    goApp.gameObject.SetActive(true);
                    var pos = foto_comune.transform.position;
                    goApp.transform.position = new Vector3(0 + (_dimensione_foto * _i), -400, pos.z);
                    //descrizione dell'immagine
                    if (!string.IsNullOrEmpty(_img.descrizione) && goApp.GetComponentsInChildren<Text>() != null)
                    {
                        foreach (var ap in goApp.GetComponentsInChildren<Text>())
                        {
                            if (ap.name == "descrizione")
                                ap.text = _img.descrizione;
                            if (ap.name == "count" && _comune.Listimages.Count > 1)
                                ap.text = (_i + 1) + "/" + _comune.Listimages.Count;
                        }
                    }
                    goApp.transform.localScale = new Vector3(1, 1, 1);
                    //goApp.transform.SetParent(content_list_img.transform, false);
                    goApp._Image = _DBClass.getSpriteFromByteArray(_img.image);
                    scrollviewFotoComune.AddPage((RectTransform)goApp.transform);
                    GOList.Add(goApp);
                    _i++;
                }
                //float delta = 0;
                //if (_i > 1)
                //    delta = (_dimensione_foto * (_i - 1));
                //content_list_img.GetComponent<RectTransform>().sizeDelta = new Vector2(delta, 0);
                //foto_comune.gameObject.SetActive(false);
            }
            else
            {
                scrollviewFotoComune.gameObject.SetActive(false);
            }
            descrizione.text += _comune.descrizione();

        }
        _scrollDesccrizione.SetActive(false);
        _scrollOggetti.SetActive(true);
    }
    private void OnDisable()
    {
        backBtn.onClick.RemoveListener(backBtnPressed);
        if (GOList != null && GOList.Count > 0)
        {
            Debug.Log($"foto da distruggere {GOList.Count}");
            foreach (var _g in GOList)
                _g.gameObject.Destroy();
            GOList = new List<SliderPage>();
        }
        foto_comune.gameObject.SetActive(true);
        scrollviewFotoComune.Clear();

    }
    public void backBtnPressed()
    {
        PlayerPrefs.SetString("istat", "");
        PlayerPrefs.SetString("poi_selezionato", "");
        PlayerPrefs.SetString("percorso_selezionato", "");
        selected_tab.text = "0";
        _panelComune.SetActive(false);
        _panelPrincipale.SetActive(true);
    }

}
