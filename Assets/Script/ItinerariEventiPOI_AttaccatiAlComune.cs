using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static DBClass;
using WebP;

public class ItinerariEventiPOI_AttaccatiAlComune : MonoBehaviour
{
    public Button BtnIDescrizione;
    public Button BtnItinerari;
    public Button BtnEventi;
    public Button BtnPOI;
    public GameObject panel_list_oggetti;
    public GameObject content_list_oggetti;
    public GameObject _scroll_descrizione;
    public GameObject scrollviewFotoComune;
    public GameObject contentDescrizione;
    public GameObject _panel_link;
    public Text filterName;
    public Toggle manifatturiero;
    public Toggle enogastronomico;
    public Toggle cicloturistico;
    public Toggle naturalistico;
    public Toggle storico_artistico;
    public Toggle ar;
    public Toggle iot;
    public Text gruppo_tipo_poi;
    public bool _check_avanzato = false;
    public Text selected_tab;


    private string _filterName = "";
    private int _gruppo_tipo_poi = -1;
    private int _tipo_poi = -1;
    private bool _manifatturiero = false;
    private bool _enogastronomico = false;
    private bool _cicloturistico = false;
    private bool _naturalistico = false;
    private bool _storico_artistico = false;
    private bool _ar = false;
    private bool _iot = false;
    private int _lingua_selezionata = 1;
    private int _NumberOfItemsToShow = 20;
    private int _TotalRowToExtract = 0;
    private string _istat = "";
    private DBClass _DBClass;
    private COMUNE _selected_comune = new COMUNE();
    private int _tipo_dettaglio_da_vedere;
    private int _OLD_tipo_dettaglio_da_vedere;
    private List<POI> POIList = new List<POI>();
    private List<PERCORSO> PERCORSOList = new List<PERCORSO>();
    private List<IEP> EventiList = new List<IEP>();
    private bool caricamentoEventi = false;
    private bool eventiCaricati = false;
    private Coroutine coroutineEventi;
    private List<GameObject> POIGo = new List<GameObject>();
    private GameObject _btnComune;
    private GameObject _btnComune_SITO;
    private GameObject _btnSeeMore;
    private int immaginiEventiDaScaricare = 0;


    public void IncrementNumberOfRowToShow()
    {
        _tipo_dettaglio_da_vedere = _OLD_tipo_dettaglio_da_vedere;
        _NumberOfItemsToShow += 20;
        Calcola();
    }
    public void ResetNumberOfRowToShow(bool calcola = true)
    {
        _NumberOfItemsToShow = 20;
        _tipo_dettaglio_da_vedere = _OLD_tipo_dettaglio_da_vedere;
        scrollPosition = Vector2.zero;
        if (calcola)
            Calcola();
    }
    private void Start()
    {
        _lingua_selezionata = PlayerPrefs.GetInt("lingua_selezionata");
        ResetNumberOfRowToShow();
        _tipo_dettaglio_da_vedere = 0;

    }
    IEnumerator Wait()
    {
        Debug.Log("This is in the IEnumerator");
        //Waiting will be available here.
        yield return new WaitForSeconds(3);
        Debug.Log("Already wait for 1 second");
    }
    private void Update()
    {
        if (panel_list_oggetti.activeSelf)
        {
            if (filterName != null)
            {
                if (filterName.text != _filterName)
                {
                    StartCoroutine(Wait());
                    _filterName = filterName.text;
                    _tipo_dettaglio_da_vedere = _OLD_tipo_dettaglio_da_vedere;
                    Calcola();
                }
                if (manifatturiero.isOn != _manifatturiero)
                {
                    _manifatturiero = manifatturiero.isOn;
                    _tipo_dettaglio_da_vedere = _OLD_tipo_dettaglio_da_vedere;
                    Calcola();
                }
                if (enogastronomico.isOn != _enogastronomico)
                {
                    _enogastronomico = enogastronomico.isOn;
                    _tipo_dettaglio_da_vedere = _OLD_tipo_dettaglio_da_vedere;
                    Calcola();
                }
                if (cicloturistico.isOn != _cicloturistico)
                {
                    _cicloturistico = cicloturistico.isOn;
                    _tipo_dettaglio_da_vedere = _OLD_tipo_dettaglio_da_vedere;
                    Calcola();
                }
                if (naturalistico.isOn != _naturalistico)
                {
                    _naturalistico = naturalistico.isOn;
                    _tipo_dettaglio_da_vedere = _OLD_tipo_dettaglio_da_vedere;
                    Calcola();
                }
                if (storico_artistico.isOn != _storico_artistico)
                {
                    _storico_artistico = storico_artistico.isOn;
                    _tipo_dettaglio_da_vedere = _OLD_tipo_dettaglio_da_vedere;
                    Calcola();
                }
                if (ar.isOn != _ar)
                {
                    _ar = ar.isOn;
                    _tipo_dettaglio_da_vedere = _OLD_tipo_dettaglio_da_vedere;
                    Calcola();
                }
                if (iot.isOn != _iot)
                {
                    _iot = iot.isOn;
                    _tipo_dettaglio_da_vedere = _OLD_tipo_dettaglio_da_vedere;
                    Calcola();
                }
                int c = -1;
                if (int.TryParse(gruppo_tipo_poi.text, out c))
                {
                    if (c != _gruppo_tipo_poi)
                    {
                        _gruppo_tipo_poi = c;
                        _tipo_dettaglio_da_vedere = _OLD_tipo_dettaglio_da_vedere;
                        Calcola();
                    }
                }
            }
        }
    }
    private void OnEnable()
    {
        // Recuperi il componente ScrollRect dal GameObject
        ScrollRect sr = _scroll_descrizione.GetComponent<ScrollRect>();

        if (sr != null)
        {
            sr.verticalNormalizedPosition = 1f;
        }
        sr = panel_list_oggetti.GetComponent<ScrollRect>();

        if (sr != null)
        {
            sr.verticalNormalizedPosition = 1f;
        }
        BtnIDescrizione.onClick.AddListener(BtnIDescrizioneClicked);
        BtnItinerari.onClick.AddListener(BtnItinerariClicked);
        if (BtnEventi != null)
            BtnEventi.onClick.AddListener(BtnEventiClicked);
        if (BtnPOI != null)
            BtnPOI.onClick.AddListener(BtnPOIClicked);

        _lingua_selezionata = PlayerPrefs.GetInt("lingua_selezionata");
        _istat = PlayerPrefs.GetString("istat");


        _btnComune = (GameObject)Resources.Load("Button_Itinerari_Eventi_POI");
        _btnComune_SITO = (GameObject)Resources.Load("Button_Itinerari_Eventi_POI_simile_al_sito");
        _btnSeeMore = (GameObject)Resources.Load("Button_SeeMore");


        _DBClass = GameObject.FindWithTag("SQLite").GetComponent<DBClass>();
        BtnIDescrizione.GetComponent<BtnTabClick>().SetCliccked(true);
        _tipo_dettaglio_da_vedere = 0;
        Calcola();
        if (!_check_avanzato)
        {
            BtnItinerari.enabled = (PERCORSOList != null && PERCORSOList.Count > 0) ? true : false;
            //if (BtnEventi != null)
            //    BtnEventi.enabled = false;
            if (BtnPOI != null)
                BtnPOI.enabled = (POIList != null && POIList.Count > 0) ? true : false;
        }
        if (selected_tab.text == "0")
        {
            BtnIDescrizione.GetComponent<BtnTabClick>().SetCliccked(true);
            _tipo_dettaglio_da_vedere = 0;
        }
        else
            BtnIDescrizione.GetComponent<BtnTabClick>().SetCliccked(false);
        if (BtnItinerari != null)
        {
            if (selected_tab.text == "1")
            {
                BtnItinerari.GetComponent<BtnTabClick>().SetCliccked(true);
                _tipo_dettaglio_da_vedere = 1;
            }
            else
                BtnItinerari.GetComponent<BtnTabClick>().SetCliccked(false);
        }
        if (BtnEventi != null)
        {
            if (selected_tab.text == "3")
            {
                BtnEventi.GetComponent<BtnTabClick>().SetCliccked(false);
                _tipo_dettaglio_da_vedere = 2;
            }
            else
                BtnEventi.GetComponent<BtnTabClick>().SetCliccked(false);

        }
        if (BtnPOI != null)
        {
            if (selected_tab.text == "2")
            {
                BtnPOI.GetComponent<BtnTabClick>().SetCliccked(true);
                _tipo_dettaglio_da_vedere = 3;
            }
            else
                BtnPOI.GetComponent<BtnTabClick>().SetCliccked(false);
        }
        int _c;
        if (int.TryParse(selected_tab.text, out _c))
        {
            if (GameObject.FindObjectOfType<GestioneTab>() != null)
            {
                GameObject.FindObjectOfType<GestioneTab>().selectes_btn_index(_c);
            }
        }
        if (!eventiCaricati && !caricamentoEventi)
        {
            if (BtnEventi != null)
            {
                BtnEventi.interactable = false;
                BtnEventi.gameObject.SetActive(true);
            }

            StartCoroutine(CaricaEventiComune());
        }

        if (_panel_link != null)
            _panel_link.SetActive(false);
    }
    private void Calcola()
    {
        ScrollRect sr = _scroll_descrizione.GetComponent<ScrollRect>();
        if (sr != null)
            sr.verticalNormalizedPosition = 1f;
        sr = panel_list_oggetti.GetComponent<ScrollRect>();
        if (sr != null)
            sr.verticalNormalizedPosition = 1f;
        _DBClass = GameObject.FindWithTag("SQLite").GetComponent<DBClass>();
        string _poi_selezionato = PlayerPrefs.GetString("poi_selezionato");
        string _percorso_selezionato = PlayerPrefs.GetString("percorso_selezionato");
        //Debug.Log($"ItinerariEventiPOI_calcola poi:{_poi_selezionato} percorso:{_percorso_selezionato}");
        POIList = new List<POI>();
        PERCORSOList = new List<PERCORSO>();
        string tipo_percorso = "";
        if (_manifatturiero)
            tipo_percorso = "artigianale";
        if (_enogastronomico)
            tipo_percorso = "enogastronomico";
        if (_cicloturistico)
            tipo_percorso = "cicloturistico";
        if (_naturalistico)
            tipo_percorso = "naturalistico";
        if (_storico_artistico)
            tipo_percorso = "storico-artistico";
        string tipo_navigazione = "";
        if (_ar)
            tipo_navigazione = "ar-vr";
        if (_iot)
            tipo_navigazione = "iot";
        long _result_poi;
        if (_poi_selezionato != "" && long.TryParse(_poi_selezionato, out _result_poi))
        {
            foreach (var _poiAssociatoApercorsi in _DBClass.GetPERCORSO(null, _result_poi))
            {
                if (!PERCORSOList.Contains(_poiAssociatoApercorsi))
                {
                    PERCORSOList.Add(_poiAssociatoApercorsi);
                }
            }
            foreach (var _poixtappe in _DBClass.getPOIXTAPPE(null, _result_poi))
            {
                foreach (var _tappexpercorsi in _DBClass.getTAPPEXPERCORSI(null, _poixtappe.tappa_id))
                {
                    var _percorso = _DBClass.GetPERCORSO(_tappexpercorsi.percorso_id, null, null, null, _filterName, tipo_percorso, tipo_navigazione).FirstOrDefault();
                    if (PERCORSOList.Where(p => p.id == _percorso.id).Count() == 0)
                    {
                        PERCORSOList.Add(_percorso);
                    }
                }
            }
            _TotalRowToExtract = PERCORSOList.Count;
        }
        else if (_percorso_selezionato != "")
        {
            foreach (var _tappexpercorso in _DBClass.getTAPPEXPERCORSI(null, null, long.Parse(_percorso_selezionato)))
            {
                foreach (var _poixtappe in _DBClass.getPOIXTAPPE(null, null, _tappexpercorso.tappa_id))
                {
                    var _poi = _DBClass.getPOI(_poixtappe.poi_id, null, _filterName, _NumberOfItemsToShow, _gruppo_tipo_poi, _tipo_poi, true).FirstOrDefault();
                    if (POIList.Where(p => p.ID == _poi.ID).Count() == 0)
                        POIList.Add(_poi);
                }
            }
            _TotalRowToExtract = POIList.Count;
        }
        else
        {
            if (!string.IsNullOrEmpty(_istat))
            {
                var _Listcomune = _DBClass.GetCOMUNI(_istat);
                Debug.Log("Passo per il comune");

                if (_Listcomune != null && _Listcomune.Count > 0)
                {
                    foreach (var _selected_comune in _Listcomune)
                    {
                        if (_tipo_dettaglio_da_vedere == 0 || _tipo_dettaglio_da_vedere == 3)
                        {
                            foreach (var pOIs in _DBClass.getPOI(null, _selected_comune.id, _filterName, _NumberOfItemsToShow, _gruppo_tipo_poi, _tipo_poi, false))
                            {
                                if (!POIList.Contains(pOIs))
                                    POIList.Add(pOIs);
                            }
                            _TotalRowToExtract = _DBClass.getPOI_Count(null, _selected_comune.id, _filterName, _gruppo_tipo_poi, _tipo_poi);
                        }
                        else
                            POIList = new List<POI>();
                        if (_tipo_dettaglio_da_vedere == 0 || _tipo_dettaglio_da_vedere == 1)
                        {

                            foreach (var _percorso in _DBClass.GetPERCORSO(null, null, null, _selected_comune.id, _filterName, tipo_percorso, tipo_navigazione, true))
                            {
                                if (!PERCORSOList.Contains(_percorso))
                                    PERCORSOList.Add(_percorso);
                            }
                            _TotalRowToExtract = PERCORSOList.Count;
                        }
                        else
                            PERCORSOList = new List<PERCORSO>();
                    }
                }
            }
            if (_check_avanzato)
            {
                //Debug.Log($"ricalcolo {_filterName} {_tipo_dettaglio_da_vedere}");
                _TotalRowToExtract = 0;
                if (_tipo_dettaglio_da_vedere == 3)
                {
                    foreach (var pOIs in _DBClass.getPOI(null, null, _filterName, _NumberOfItemsToShow, _gruppo_tipo_poi, _tipo_poi, true))
                    {
                        if (!POIList.Contains(pOIs))
                            POIList.Add(pOIs);
                    }
                    _TotalRowToExtract = _DBClass.getPOI_Count(null, null, _filterName, _gruppo_tipo_poi, _tipo_poi);
                }
                if (_tipo_dettaglio_da_vedere == 1)
                {
                    foreach (var _percorso in _DBClass.GetPERCORSO(null, null, null, null, _filterName, tipo_percorso, tipo_navigazione))
                    {
                        if (!PERCORSOList.Contains(_percorso))
                            PERCORSOList.Add(_percorso);
                    }
                    _TotalRowToExtract = PERCORSOList.Count;
                }
            }
        }

    }
    private void OnDisable()
    {
        BtnIDescrizione.onClick.RemoveListener(BtnIDescrizioneClicked);
        BtnItinerari.onClick.RemoveListener(BtnItinerariClicked);
        if (BtnEventi != null)
            BtnEventi.onClick.RemoveListener(BtnEventiClicked);
        if (BtnPOI != null)
            BtnPOI.onClick.RemoveListener(BtnPOIClicked);
        _tipo_dettaglio_da_vedere = 0;
        BtnIDescrizioneClicked();
        ResetNumberOfRowToShow(false);
    }

    public void Reload()
    {
        OnEnable();
    }

    public void BtnIDescrizioneClicked()
    {
        //Debug.Log("vedo la descrizione");
        ScrollRect sr = panel_list_oggetti.GetComponent<ScrollRect>();

        if (sr != null)
        {
            sr.verticalNormalizedPosition = 1f;
        }
        _OLD_tipo_dettaglio_da_vedere = _tipo_dettaglio_da_vedere = 0;
        Transform viewportTransform = panel_list_oggetti.transform.Find("Viewport");

        if (viewportTransform != null)
        {
            RectTransform rtViewport = viewportTransform.GetComponent<RectTransform>();

            if (rtViewport != null)
            {
                // Modifica il Top del Viewport portandolo a 0
                rtViewport.offsetMax = new Vector2(rtViewport.offsetMax.x, -222f);
            }
        }
        ResetNumberOfRowToShow();
    }

    private void BtnItinerariClicked()
    {
        //Debug.Log("vedo gli itinerari");
        ScrollRect sr = panel_list_oggetti.GetComponent<ScrollRect>();

        if (sr != null)
        {
            sr.verticalNormalizedPosition = 1f;
        }
        _OLD_tipo_dettaglio_da_vedere = _tipo_dettaglio_da_vedere = 1;
        Transform viewportTransform = panel_list_oggetti.transform.Find("Viewport");

        if (viewportTransform != null)
        {
            RectTransform rtViewport = viewportTransform.GetComponent<RectTransform>();

            if (rtViewport != null)
            {
                // Modifica il Top del Viewport portandolo a 0
                rtViewport.offsetMax = new Vector2(rtViewport.offsetMax.x, -222f);
            }
        }
        ResetNumberOfRowToShow();
    }
    private void BtnEventiClicked()
    {
        //Debug.Log("vedo gli Eventi");
        ScrollRect sr = panel_list_oggetti.GetComponent<ScrollRect>();

        if (sr != null)
        {
            sr.verticalNormalizedPosition = 1f;
        }
        _OLD_tipo_dettaglio_da_vedere = _tipo_dettaglio_da_vedere = 2;



        Transform viewportTransform = panel_list_oggetti.transform.Find("Viewport");

        if (viewportTransform != null)
        {
            RectTransform rtViewport = viewportTransform.GetComponent<RectTransform>();

            if (rtViewport != null)
            {
                // Modifica il Top del Viewport portandolo a 0
                rtViewport.offsetMax = new Vector2(rtViewport.offsetMax.x, 0f);
            }
        }
        ResetNumberOfRowToShow();
    }
    private void BtnPOIClicked()
    {
        //Debug.Log("vedo i POI");
        ScrollRect sr = panel_list_oggetti.GetComponent<ScrollRect>();

        if (sr != null)
        {
            sr.verticalNormalizedPosition = 1f;
        }
        _OLD_tipo_dettaglio_da_vedere = _tipo_dettaglio_da_vedere = 3;
        Transform viewportTransform = panel_list_oggetti.transform.Find("Viewport");

        if (viewportTransform != null)
        {
            RectTransform rtViewport = viewportTransform.GetComponent<RectTransform>();

            if (rtViewport != null)
            {
                // Modifica il Top del Viewport portandolo a 0
                rtViewport.offsetMax = new Vector2(rtViewport.offsetMax.x, -222f);
            }
        }

        ResetNumberOfRowToShow();
    }
    public class IEP
    {
        public byte[] immagine;
        public string immagine_url;
        public Sprite sprite_immagine;

        public Image uiImage;

        public string nome_comune;
        public string testo;
        public string descrizione_breve;
        public int tipo;
        public string tipo_poi;
        public int immagine_poi;
        public string tipo_percorso;
        public string tipo_navigazione;
        public string lunghezza;
        public string dislivello;
        public long id;
        public bool logo_bici = true;
        public float distance;
        public string colore;
        public string data_orario_inizio;

        public string data_orario_fine;
        public string link;
        public int sinp_id;

    }

    public IEnumerator ScaricaEventi(List<int> _id_sinp = null, List<IEP> risultato = null)
    {
        string indirizzo =
            "https://www.macerataturismo.it/wp-json/rest_api_ws/v1/get_eventi_per_app?";

        //if (_id_sinp > 0)
        //    indirizzo += "&ID_SINP=" + _id_sinp;

        if (_id_sinp != null &&
        _id_sinp.Count > 0)
        {
            indirizzo +=
                "&ID_SINP=" +
                string.Join(",", _id_sinp);
        }

        indirizzo += "&t=" + DateTime.UtcNow.Ticks;


        Debug.Log(indirizzo);


        UnityWebRequest www = UnityWebRequest.Get(indirizzo);


        yield return www.SendWebRequest();


        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error);
            www.Dispose();

            caricamentoEventi = false;

            yield break;
        }


        string jsonreturned = www.downloadHandler.text;


        EventoResponse response =
            JsonConvert.DeserializeObject<EventoResponse>(jsonreturned);



        if (response != null && response.Evento != null)
        {
            foreach (EventoItem evento in response.Evento)
            {
                IEP iep = new IEP();


                iep.id = evento.id;

                iep.nome_comune = evento.title;

                // ATTENZIONE:
                // 2 = evento
                // 3 = POI
                iep.tipo = 2;

                iep.tipo_poi = "EVENTO";


                if (evento.descrizione_breve != null &&
                    evento.descrizione_breve.Count > 0)
                {
                    iep.testo =
                        evento.descrizione_breve[0];
                }


                if (evento.immagine != null &&
                    evento.immagine.Count > 0)
                {
                    if (evento.immagine != null && evento.immagine.Count > 0)
                    {
                        iep.immagine_url = evento.immagine[0];
                        immaginiEventiDaScaricare++;
                        Debug.Log("Immagine evento: " + iep.immagine_url);
                    }
                }

                if (evento.data_orario_inizio != null && evento.data_orario_inizio.Count > 0)
                    iep.data_orario_inizio = evento.data_orario_inizio[0];
                if (evento.data_orario_fine != null && evento.data_orario_fine.Count > 0)
                    iep.data_orario_fine = evento.data_orario_fine[0];
                if (!string.IsNullOrEmpty(evento.link))
                    iep.link = evento.link;
                if (evento.id_sinp != null && evento.id_sinp.Count > 0)
                    iep.sinp_id = int.Parse(evento.id_sinp[0]);

                risultato.Add(iep);
            }
        }


        Debug.Log("Eventi caricati: " + risultato.Count);

        eventiCaricati = true;

        www.Dispose();
    }

    IEnumerator ScaricaImmagineURL(string url, Action<Sprite> callback)
    {
        if (string.IsNullOrEmpty(url))
        {
            callback(null);
            yield break;
        }

        UnityWebRequest www = UnityWebRequest.Get(url);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Errore immagine: " + www.error);
            Debug.LogError("URL: " + url);

            callback(null);
            yield break;
        }

        byte[] data = www.downloadHandler.data;

        string contentType =
            www.GetResponseHeader("Content-Type");

        Debug.Log("Content-Type: " + contentType);
        Debug.Log("Bytes ricevuti: " + data.Length);

        Texture2D texture = null;

        try
        {
            // JPG / PNG
            if (contentType != null &&
                !contentType.ToLower().Contains("webp"))
            {
                texture = new Texture2D(2, 2);

                if (!texture.LoadImage(data))
                {
                    Debug.LogError("LoadImage fallita");
                    callback(null);
                    yield break;
                }
            }
            else
            {
                WebP.Error error;

                texture =
                    Texture2DExt.CreateTexture2DFromWebP(
                        data,
                        false,   // mipmaps
                        false,   // linear
                        out error
                    );
                if (texture == null)
                {
                    Debug.LogError("Decodifica WEBP fallita");
                    callback(null);
                    yield break;
                }
            }

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );

            callback(sprite);
        }
        catch (Exception ex)
        {
            Debug.LogError("Eccezione WEBP: " + ex);
            callback(null);
        }

        immaginiEventiDaScaricare--;

        www.Dispose();
    }

    public IEnumerator CaricaEventiComune()
    {
        caricamentoEventi = true;
        eventiCaricati = false;

        EventiList.Clear();

        if (!string.IsNullOrEmpty(_istat))
        {
            if (BtnEventi != null)
                BtnEventi.interactable = false;


            var comuni = _DBClass.GetCOMUNI(_istat);

            List<int> comuneIds = new List<int>();
            if (comuni != null)
            {
                foreach (var comune in comuni)
                {
                    comuneIds.Add(comune.id);
                }
                yield return StartCoroutine(
                        ScaricaEventi(
                            comuneIds,
                            EventiList
                        )
                    );
            }
        }
        else
        {
            var comuni = _DBClass.GetCOMUNI(null, null, null, null, null, true);

            List<int> comuneIds = new List<int>();
            if (comuni != null)
            {
                foreach (var comune in comuni)
                {
                    comuneIds.Add(comune.id);
                }
                yield return StartCoroutine(
                        ScaricaEventi(
                            comuneIds,
                            EventiList
                        )
                    );
            }
        }


        // Eventi scaricati
        eventiCaricati = true;


        // Ora scarico realmente le immagini
        foreach (IEP evento in EventiList)
        {
            if (!string.IsNullOrEmpty(evento.immagine_url))
            {
                yield return StartCoroutine(
                    ScaricaImmagineURL(
                        evento.immagine_url,
                        sprite =>
                        {
                            evento.sprite_immagine = sprite;
                            Debug.Log("SPRITE EVENTO CARICATO: " + evento.nome_comune + " -> " + (sprite != null));
                        }
                    )
                );
            }
        }


        caricamentoEventi = false;


        // Ora posso abilitare il bottone
        if (BtnEventi != null)
            BtnEventi.interactable = true;

        _TotalRowToExtract = EventiList.Count;
        Debug.Log(
            "EVENTI COMPLETI: " + EventiList.Count
        );
    }

    // Ritorna l'array ordinato per distanza e poi per nome
    public List<IEP> ClassiToIEP(List<POI> _POIs, List<PERCORSO> _PERCORSOs, bool check_tipo_dettaglio_da_vedere)
    {
        if (_DBClass == null)
            _DBClass = GameObject.FindWithTag("SQLite").GetComponent<DBClass>();
        /*StartCoroutine(_DBClass.GetLatLonUsingGPS());
        */

        List<IEP> ret = new List<IEP>();
        _lingua_selezionata = PlayerPrefs.GetInt("lingua_selezionata");

        if ((!check_tipo_dettaglio_da_vedere || _tipo_dettaglio_da_vedere == 3) && _POIs != null && _POIs.Count > 0)
        {
            foreach (var _poi in _POIs)
            {
                if (_poi != null)
                {
                    IEP _IEP = new IEP();
                    _IEP.tipo = 3;
                    _IEP.id = _poi.ID;
                    if (_poi._images != null && _poi._images.Count > 0)
                    {
                        if (_poi._images.Find(match => match.principale) != null)
                            _IEP.immagine = _poi._images.Find(match => match.principale).image;
                        else
                            _IEP.immagine = _poi._images[0].image;
                    }
                    _IEP.testo = _poi.nome;
                    _IEP.logo_bici = false;
                    _IEP.tipo_poi = _poi.tipo_list_descrizione();
                    _IEP.immagine_poi = (_poi.tipoList != null && _poi.tipoList.Count > 0)
                        ? _poi.tipoList[0].tipo.group_id : 0;
                    _IEP.tipo_percorso = "";
                    _IEP.tipo_navigazione = "";
                    _IEP.lunghezza = "";
                    _IEP.dislivello = "";

                    _IEP.nome_comune = _poi.comune + $" ({_poi.provincia})";
                    _IEP.descrizione_breve = _poi.descrizione_breve();
                    _IEP.distance = _DBClass.CalculateDistance(_poi.latitudine, _DBClass._latitudine, _poi.longitudine, _DBClass._longitudine);
                    ret.Add(_IEP);
                }
            }

        }
        /* Eventi */
        /* Eventi */
        if (!check_tipo_dettaglio_da_vedere || _tipo_dettaglio_da_vedere == 2)
        {
            foreach (IEP evento in EventiList)
            {
                ret.Add(evento);
            }
        }
        if ((!check_tipo_dettaglio_da_vedere || _tipo_dettaglio_da_vedere == 1) && _PERCORSOs != null && _PERCORSOs.Count > 0)
        {
            foreach (var _percorso in _PERCORSOs)
            {
                if (_percorso != null)
                {
                    IEP _IEP = new IEP();
                    _IEP.tipo = 1;
                    _IEP.id = _percorso.id;
                    if (_percorso.Listimages != null && _percorso.Listimages.Count > 0)
                    {
                        if (_percorso.Listimages.Find(match => match.principale) != null)
                            _IEP.immagine = _percorso.Listimages.Find(match => match.principale).image;
                        else
                            _IEP.immagine = _percorso.Listimages[0].image;
                    }
                    _IEP.colore = _percorso.colore;
                    _IEP.testo = _percorso.nome_percorso;
                    _IEP.logo_bici = true;
                    _IEP.tipo_poi = _lingua_selezionata == 1 ? "Itinerario" : "Itinerary";
                    _IEP.tipo_percorso = _percorso.tipo_percorso;
                    _IEP.tipo_navigazione = _percorso.tipo_navigazione;
                    _IEP.lunghezza = _percorso.lunghezza;
                    _IEP.dislivello = _percorso.dislivello;
                    if (_percorso.poi_id > 0)
                    {
                        Debug.Log(_percorso.poi_id);
                        var _poi = _DBClass.getPOIGeneralita(_percorso.poi_id).FirstOrDefault();
                        if (_poi != null)
                        {
                            _IEP.nome_comune = _poi.comune + $" ({_poi.provincia})";
                            _IEP.distance = _DBClass.CalculateDistance(_poi.latitudine, _DBClass._latitudine, _poi.longitudine, _DBClass._longitudine);
                        }
                    }
                    else
                    {
                        _IEP.nome_comune = "";
                        _IEP.distance = 0;
                    }
                    if (_percorso.descrizione != null && _percorso.descrizione.Count > 0)
                        _IEP.descrizione_breve = _percorso.descrizione[0].descrizione_breve;
                    else
                        _IEP.descrizione_breve = string.Empty;


                    ret.Add(_IEP);
                }
            }

        }
        return ret.OrderBy(di => di.distance).ThenBy(te => te.testo).ToList();

    }

    private Vector2 scrollPosition = Vector2.zero;
    public void ShowHidePanelLink()
    {
        _panel_link.SetActive(!_panel_link.activeSelf);
    }
    private void OnGUI()
    {
        int altezza_prefab = 450;
        if (_tipo_dettaglio_da_vedere == 2)
            altezza_prefab = 720;
        if (_tipo_dettaglio_da_vedere == 0 && !_scroll_descrizione.activeSelf)
        {
            _scroll_descrizione.SetActive(true);
            panel_list_oggetti.SetActive(false);
        }
        if (_tipo_dettaglio_da_vedere > 0)
        {
            _scroll_descrizione.SetActive(false);
            panel_list_oggetti.SetActive(true);
            if (POIGo.Count > 0)
            {
                foreach (var ap in POIGo)
                    ap.Destroy();
                POIGo = new List<GameObject>();
            }
            Calcola();
            var iep = ClassiToIEP(POIList, PERCORSOList, true);
            iep = iep.OrderBy(p => p.distance).ToList();

            float h = Screen.height;// - 220;
            GUILayout.BeginVertical(new GUIStyle() { padding = new RectOffset(20, 20, 20, 220) }, GUILayout.MaxHeight(h), GUILayout.Height(h - 500));

            var w = Screen.width;
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(0.95f * w), GUILayout.Height(h));
            int _i = 0;
            for (_i = 0; _i < iep.Count && _i < _NumberOfItemsToShow; _i++)
            {
                IEP _iep = iep[_i];
                GUILayout.BeginVertical(GUILayout.Height(h / 5));
                GUILayout.BeginHorizontal(GUILayout.Height(h / 5));
                var btn = Instantiate(_btnComune);
                if (_tipo_dettaglio_da_vedere == 2)
                    btn = Instantiate(_btnComune_SITO);
                var pos = btn.transform.position;
                btn.transform.position = new Vector3(pos.x, pos.y - (altezza_prefab * _i), pos.z);
                btn.transform.localScale = new Vector3(1, 1, 1);
                foreach (var _component in btn.GetComponentsInChildren<TMP_Text>())
                {
                    if (_tipo_dettaglio_da_vedere == 2)
                    {
                        if (_component.name == "Text_data")
                        {

                            DateTime data_i = DateTime.MinValue;
                            DateTime data_f = DateTime.MinValue;
                            bool parsingRiuscito = false;

                            // TENTATIVO 1: Controlla se è un formato data testuale standard
                            string[] formatiAmmessi = new string[] { "yyyy-MM-dd HH:mm:ss,fff", "yyyy-MM-dd HH:mm:ss.fff", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm" };
                            if (DateTime.TryParseExact(_iep.data_orario_inizio, formatiAmmessi, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out data_i) &&
                                DateTime.TryParseExact(_iep.data_orario_fine, formatiAmmessi, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out data_f))
                            {
                                parsingRiuscito = true;
                            }
                            // TENTATIVO 2: Se fallisce, controlla se è un Timestamp Unix (numero)
                            else if (long.TryParse(_iep.data_orario_inizio, out long unixInizio) && long.TryParse(_iep.data_orario_fine, out long unixFine))
                            {
                                data_i = DateTimeOffset.FromUnixTimeSeconds(unixInizio).LocalDateTime;
                                data_f = DateTimeOffset.FromUnixTimeSeconds(unixFine).LocalDateTime;
                                parsingRiuscito = true;
                            }

                            // Se una delle due conversioni è andata a buon fine, componiamo il testo spacchettato
                            // ... (mantenere la parte iniziale di conversione data intatta) ...

                            if (parsingRiuscito)
                            {
                                var culturaItaliana = new System.Globalization.CultureInfo("it-IT");
                                DateTime adesso = DateTime.Now;

                                string prefisso = "";
                                DateTime dataDaMostrare;



                                if (data_i.Date == data_f.Date)
                                {
                                    prefisso = "";
                                    dataDaMostrare = data_i;
                                }
                                else
                                {
                                    if (adesso < data_i)
                                    {
                                        prefisso = "dal\n";
                                        dataDaMostrare = data_i;
                                    }
                                    else
                                    {
                                        prefisso = "fino al\n";
                                        dataDaMostrare = data_f;
                                    }
                                }

                                // Forza l'estrazione del SINGOLO numero del giorno
                                string giorno = dataDaMostrare.Day.ToString();
                                string mese = dataDaMostrare.ToString("MMMM", culturaItaliana);
                                if (!string.IsNullOrEmpty(mese))
                                {
                                    // Prende la prima lettera, la fa maiuscola e attacca il resto della stringa
                                    mese = char.ToUpper(mese[0]) + mese.Substring(1);
                                }
                                // Colore verdino per il giorno (puoi cambiarlo se serve più scuro per lo sfondo bianco)
                                string coloreVerdino = "#3E8E41";

                                // COMPOSIZIONE: Centrato (<align=center>) e in Grassetto (<b>)
                                string testoFinale = $"<align=center><b>{prefisso}<size=120%><color={coloreVerdino}>{giorno}</color></size>\n{mese}</b></align>";

                                _component.text = testoFinale;
                            }
                            else
                            {
                                _component.text = "Data\nNon\nValida";
                            }

                        }
                    }
                }

                foreach (var _component in btn.GetComponentsInChildren<Text>())
                {
                    if (_component.name == "Text")
                        _component.text = _iep.testo;
                    else if (_component.name == "TipoItinerario")
                        _component.text = $"{_iep.tipo_poi}";
                    else if (_component.name == "TipoPOI")
                    {
                        _component.text = $"{_iep.tipo_poi}";
                        foreach (var _component2 in _component.GetComponentsInChildren<Image>())
                        {
                            if (_component2.name == "ImagePOI")
                            {
                                var _gruppo = _DBClass.getGROUP_TIPO_POI(_iep.immagine_poi).FirstOrDefault();
                                if (_gruppo != null)
                                {
                                    if (_gruppo.value == "accoglienza-e-ricettivita")
                                        _component2.sprite = Resources.Load<Sprite>("Icone/ACCOGLIENZA-bianco");
                                    if (_gruppo.value == "enogastronomico")
                                        _component2.sprite = Resources.Load<Sprite>("Icone/enogastronomico");
                                    if (_gruppo.value == "manifatturiero")
                                        _component2.sprite = Resources.Load<Sprite>("Icone/manifatturiero");
                                    if (_gruppo.value == "naturalistico")
                                        _component2.sprite = Resources.Load<Sprite>("Icone/naturalistico");
                                    if (_gruppo.value == "religioso")
                                        _component2.sprite = Resources.Load<Sprite>("Icone/religioso");
                                    if (_gruppo.value == "storico-artistico")
                                        _component2.sprite = Resources.Load<Sprite>("Icone/storico_artistico");
                                    if (_gruppo.value == "tempo-libero-e-sport")
                                        _component2.sprite = Resources.Load<Sprite>("Icone/tempo_libero_e_sport_bianco");
                                    if (_gruppo.value == "varie")
                                        _component2.sprite = Resources.Load<Sprite>("Icone/varie-bianco");
                                }
                            }
                        }
                    }
                    else if (_component.name == "tipologia")
                        _component.text = $"{_tipo_dettaglio_da_vedere}";
                    else if (_component.name == "id")
                    {
                        if (_tipo_dettaglio_da_vedere == 2)
                        {
                            var _comuni = _DBClass.GetCOMUNI(null, null, _iep.sinp_id);
                            _component.text = $"{_comuni.FirstOrDefault()?.sito_turistico}{_iep.link}";
                        }
                        else
                            _component.text = $"{_iep.tipo};{_iep.id}";
                    }
                    else if (_component.name == "Distanza")
                        _component.text = _component.text.Replace("{0}", (_iep.distance / 1000).ToString("0.##"));
                    else if (_component.name == "NomeComune")
                        _component.text = _iep.nome_comune;
                    else if (_component.name == "DescrizioneBreve")
                        _component.text = _iep.descrizione_breve;
                    else if (_component.name == "Lunghezza")
                    {
                        //if (!string.IsNullOrEmpty(_iep.lunghezza) && !_iep.lunghezza.ToLower().Contains("km"))
                        //    _component.text = _component.text.Replace("{0}", "{0} km");
                        _component.text = _component.text.Replace("{0}", _iep.lunghezza);

                        _component.text = _component.text.Replace("{1}", _iep.dislivello);
                    }
                }
                foreach (var _component in btn.GetComponentsInChildren<Image>())
                {

                    if (_component.name == "Image_background_scritte")
                    {
                        Color c = Color.black;
                        Color _c = Color.black;
                        if (_iep.tipo == 3)
                        {
                            Debug.Log(_iep.immagine_poi);
                            var _gruppo = _DBClass.getGROUP_TIPO_POI(_iep.immagine_poi).FirstOrDefault();
                            if (_gruppo != null)
                            {
                                //4456A3 - blu
                                //009366 - verde
                                //E8531E - arancione
                                //C51A1B - rosso
                                if (_gruppo.value == "accoglienza-e-ricettivita")
                                    if (ColorUtility.TryParseHtmlString("#E8531E", out _c))
                                        c = _c;
                                if (_gruppo.value == "enogastronomico")
                                    if (ColorUtility.TryParseHtmlString("#E8531E", out _c))
                                        c = _c;
                                if (_gruppo.value == "manifatturiero")
                                    if (ColorUtility.TryParseHtmlString("#009366", out _c))
                                        c = _c;

                                if (_gruppo.value == "naturalistico")
                                    if (ColorUtility.TryParseHtmlString("#009366", out _c))
                                        c = _c;
                                if (_gruppo.value == "religioso")
                                    if (ColorUtility.TryParseHtmlString("#4456A3", out _c))
                                        c = _c;

                                if (_gruppo.value == "storico-artistico")
                                    if (ColorUtility.TryParseHtmlString("#C51A1B", out _c))
                                        c = _c;
                                if (_gruppo.value == "tempo-libero-e-sport")
                                    if (ColorUtility.TryParseHtmlString("#4456A3", out _c))
                                        c = _c;
                                if (_gruppo.value == "varie")
                                    if (ColorUtility.TryParseHtmlString("#009366", out _c))
                                        c = _c;
                            }
                        }
                        else if (_iep.tipo == 1)
                        {
                            //if (ColorUtility.TryParseHtmlString(_iep.colore, out _c))
                            //    c = _c;
                            if (ColorUtility.TryParseHtmlString("#5c5c5c", out _c))
                                c = _c;

                        }
                        if (_iep.tipo == 2)
                        {
                            if (ColorUtility.TryParseHtmlString("#ffffff", out _c))
                                c = _c;
                        }
                        _component.color = c;
                    }
                    if (_component.name == "Image")
                    {
                        if (_iep.tipo == 2)
                        {
                            // EVENTO
                            if (_iep.sprite_immagine != null)
                            {
                                _component.sprite = _iep.sprite_immagine;
                            }
                        }
                        else
                        {
                            if (_iep.tipo == 3 && _iep.immagine == null)
                            {
                                var _lap = _DBClass.getPOI_IMMAGINI(null, _iep.id, true);
                                POIList[_i]._images = _lap;
                                var _ap = _lap.FirstOrDefault();
                                if (_ap != null)
                                    _iep.immagine = _ap.image;
                            }

                            else if (_iep.tipo == 1 && _iep.immagine == null)
                            {
                                var _lap = _DBClass.getPERCORSI_IMMAGINI(null, (int?)_iep.id);
                                PERCORSOList[_i].Listimages = _lap;
                                var _ap = _lap.FirstOrDefault();
                                if (_ap != null)
                                    _iep.immagine = _ap.image;
                            }
                            byte[] foto = null;
                            if (_iep.immagine != null && _iep.immagine != null && _iep.immagine.Length > 0)
                                foto = _iep.immagine;
                            _component.sprite = _DBClass.getSpriteFromByteArray(foto);
                        }
                        var _ratio_component = _component.GetComponent<AspectRatioFitter>();
                        if (_ratio_component != null)
                        {
                            float ratio = (float)_component.sprite.rect.width / _component.sprite.rect.height;
                            // Aggiorniamo il componente
                            _ratio_component.aspectRatio = ratio;
                        }
                    }
                    if (_component.name == "Image_tipo_percorso")
                    {
                        if (_tipo_dettaglio_da_vedere == 1)
                        {
                            _component.gameObject.SetActive(true);
                            if (!string.IsNullOrEmpty(_iep.tipo_percorso))
                            {
                                if (_iep.tipo_percorso == "artigianale")
                                    _component.sprite = Resources.Load<Sprite>("Icone/manifatturiero");
                                else if (_iep.tipo_percorso == "cicloturistico")
                                    _component.sprite = Resources.Load<Sprite>("Icone/ciclopedonale");
                                else if (_iep.tipo_percorso == "enogastronomico")
                                    _component.sprite = Resources.Load<Sprite>("Icone/enogastronomico");
                                else if (_iep.tipo_percorso == "naturalistico")
                                    _component.sprite = Resources.Load<Sprite>("Icone/naturalistico");
                                else if (_iep.tipo_percorso == "storico-artistico")
                                    _component.sprite = Resources.Load<Sprite>("Icone/storico_artistico");
                            }

                        }
                        else
                            _component.gameObject.SetActive(false);
                    }
                    if (_component.name == "Image_tipo_navigazione")
                    {
                        if (_tipo_dettaglio_da_vedere == 1)
                        {
                            if (!string.IsNullOrEmpty(_iep.tipo_navigazione) && _iep.tipo_navigazione == "ar-vr")
                            {
                                _component.sprite = Resources.Load<Sprite>("Icone/AR");
                            }
                            else
                            {
                                _component.sprite = Resources.Load<Sprite>("Icone/NFC");
                            }
                        }
                        else
                            _component.gameObject.SetActive(false);
                    }

                }
                btn.SetActive(true);
                btn.transform.SetParent(content_list_oggetti.transform, false);
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                POIGo.Add(btn);
            }

            _OLD_tipo_dettaglio_da_vedere = _tipo_dettaglio_da_vedere;
            _tipo_dettaglio_da_vedere = -1;
            Debug.Log($"{_i} +++ {_TotalRowToExtract}");
            if (_i < _TotalRowToExtract - 1)
            {
                GUILayout.BeginVertical(GUILayout.Height(h / 5));
                GUILayout.BeginHorizontal(GUILayout.Height(h / 5));
                var btn = Instantiate(_btnSeeMore);
                var pos = btn.transform.position;
                btn.transform.position = new Vector3(pos.x, pos.y - (altezza_prefab * _i), pos.z);
                btn.transform.localScale = new Vector3(1, 1, 1);
                btn.SetActive(true);
                btn.transform.SetParent(content_list_oggetti.transform, false);
                btn.AddComponent<Button>();
                btn.GetComponent<Button>().onClick.RemoveAllListeners();
                btn.GetComponent<Button>().onClick.AddListener(IncrementNumberOfRowToShow);
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                POIGo.Add(btn);
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            float delta = (altezza_prefab * (_TotalRowToExtract < _NumberOfItemsToShow ? _TotalRowToExtract : iep.Count + 1));
            if (delta > w)
            {
                Debug.Log("1");
                delta -= w;
                content_list_oggetti.GetComponent<RectTransform>().sizeDelta = new Vector2(0, delta + altezza_prefab);
            }
            else
            {
                Debug.Log("2");
                content_list_oggetti.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0 + altezza_prefab);
            }
        }
    }
}



[System.Serializable]
public class EventoResponse
{
    public List<EventoItem> Evento;
}

[System.Serializable]
public class EventoItem
{
    public long id;
    public string post_name;
    public string title;

    public List<string> id_sinp;
    public List<string> genitore_evento;
    public List<string> immagine;
    public List<string> data_orario_inizio;
    public List<string> data_orario_fine;
    public List<string> descrizione_breve;
    public string link;
}