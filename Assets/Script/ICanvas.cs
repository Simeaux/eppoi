using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ARLocation;
using ARLocation.MapboxRoutes;
using ARLocation.Utils;
using Mapbox.Unity.Map;
using UnityEngine;
using UnityEngine.UI;
using static DBClass;

using UnityEditor;
using Mapbox.Unity.MeshGeneration.Data;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using static TouchScript.Behaviors.Cursors.UI.GradientTexture;
using TMPro;
using static ItinerariEventiPOI_AttaccatiAlComune;
using UnityEngine.SceneManagement;

public class ICanvas : MonoBehaviour
{
    [System.Serializable]
    public class ElementsData
    {
        public UnityEngine.UI.Text lon;
        public UnityEngine.UI.Text lat;
        public UnityEngine.UI.Text zoom;
        public Button Btnmore_zoom;
        public Button Btnless_zoom;
        public Button Btnfilter;
        public Button Btnremovefilter;
        //public GameObject panelFilter;
        //public GameObject panelZoom;
        //public GameObject panelInfo;
    }
    public bool search_filterPOI = false;
    public Font _font;
    public ElementsData Elements;
    public GameObject PanelMAP;
    public GameObject PanelPOI;
    public AbstractMap abstractMap;

    public Toggle _togglePOI;
    public UnityEngine.UI.Text gruppo_tipo_poi;
    public Toggle _toggleitinerari;
    public Toggle _bici;
    public Toggle _piedi;
    public Toggle _AR;
    public Toggle _IOT;
    public UnityEngine.UI.Text QueryText;
    public Dropdown _grid_comune;
    public GameObject content_list_oggetti;
    public GameObject panel_all_White;
    public Button Cerca;
    private List<GameObject> POIGo = new List<GameObject>();


    private DBClass _DBClass;
    private GameObject _btnIEP;
    private GameObject _btnSeeMore;
    private int _lingua_selezionata;
    private int _NumberOfItemsToShow = 10;
    private ExtractDataForMap _extract;
    private List<IEP> Results = new List<IEP>();
    
    private void Start()
    {
        _DBClass = GameObject.FindWithTag("SQLite").GetComponent<DBClass>();
        _btnIEP  = (GameObject)Resources.Load("Button_Itinerari_Eventi_POI");
        _btnSeeMore = (GameObject)Resources.Load("Button_SeeMore");
        _lingua_selezionata = PlayerPrefs.GetInt("lingua_selezionata");
        _extract = GameObject.FindWithTag("SQLite").GetComponent<ExtractDataForMap>();
        ItinerariEventiPOI_AttaccatiAlComune _ap = new ItinerariEventiPOI_AttaccatiAlComune();
        
        foreach (var _component in Cerca.GetComponentsInChildren<UnityEngine.UI.Text>())
        {
            if (_component.name == "Text")
                _component.text = _lingua_selezionata == 1 ? "Cerca" : "Search";
        }
        _togglePOI.GetComponentInChildren<UnityEngine.UI.Text>().text = _lingua_selezionata == 1 ? "Solo Punti di interesse" : "Only Points of interest";
        _toggleitinerari.GetComponentInChildren<UnityEngine.UI.Text>().text = _lingua_selezionata == 1 ? "Solo itinerari" : "Only Itineraries";
       // Elements.Btnremovefilter.GetComponentInChildren<UnityEngine.UI.Text>().text = _lingua_selezionata == 1 ? "Mostra nella mappa" : "Show on map";
    }
    // Start is called before the first frame update
    private void Awake()
    {
        Elements.lat.text = 43.256187.ToString();
        Elements.lon.text = 13.008713.ToString();
        Elements.zoom.text = "15";
        /*
        Elements.Btnmore_zoom.gameObject.SetActive(true);
        Elements.Btnless_zoom.gameObject.SetActive(true);
        */
        Debug.Log("Enable!!!");
        if (PlayerPrefs.GetInt("show_grid_poi") == 1 && !panel_all_White.activeSelf)
        {
            panel_all_White.SetActive(true);
            search_filterPOI = true;
        }
    }
    private List<int> _comuni_id = new List<int>();

    private void OnEnable()
    {
        ItinerariEventiPOI_AttaccatiAlComune _ap = new ItinerariEventiPOI_AttaccatiAlComune();
        _DBClass = GameObject.FindWithTag("SQLite").GetComponent<DBClass>();
            
        Elements.Btnmore_zoom.onClick.AddListener(OnBtnmore_zoom);
        Elements.Btnless_zoom.onClick.AddListener(OnBtnless_zoom);
        Elements.Btnfilter.onClick.AddListener(OnBtnShowHidefilter);
        Elements.Btnremovefilter.onClick.AddListener(OnBtnShowHidefilter);

        _comuni_id = new List<int>();
        _lingua_selezionata = PlayerPrefs.GetInt("lingua_selezionata");
        string testo = (_lingua_selezionata == 1) ? "Seleziona un comune" : "Select a municipality";
        List<Dropdown.OptionData> _list_comuni = new List<Dropdown.OptionData>
        {
            new Dropdown.OptionData() { text = testo}
        };
        _comuni_id.Add(0);
        foreach (var _comune in _DBClass.GetCOMUNI(string.Empty, string.Empty, null))
        {
            _list_comuni.Add(new Dropdown.OptionData() { text = _comune.nome_comune + $" ({_comune.provincia})" });
            _comuni_id.Add(_comune.id);
        }
        _grid_comune.GetComponent<Dropdown>().options = _list_comuni;


        if (PlayerPrefs.GetInt("show_grid_poi") == 1 && !search_filterPOI)
            search_filterPOI = !search_filterPOI;

    }

    private void OnDisable()
    {

        Elements.Btnmore_zoom.onClick.RemoveListener(OnBtnmore_zoom);
        Elements.Btnless_zoom.onClick.RemoveListener(OnBtnless_zoom);
        Elements.Btnfilter.onClick.RemoveListener(OnBtnShowHidefilter);
        Elements.Btnremovefilter.onClick.RemoveListener(OnBtnShowHidefilter);
    }
    public void IncrementNumberOfRowToShow()
    {
        search_filterPOI = true;
        _NumberOfItemsToShow += 10;
    }
    public void ResetNumberOfRowToShow()
    {
        _NumberOfItemsToShow = 10;
    }
    // Update is called once per frame

    private void OnBtnmore_zoom()
    {
        Debug.Log("Bottone More zoom premuto");

        double x = 0;
        if (double.TryParse(Elements.zoom.text, out x))
            x += 1;
        Elements.zoom.text = x.ToString();
    }
    private void OnBtnless_zoom()
    {
        Debug.Log("Bottone Less zoom premuto");

        double x = 0;
        if (double.TryParse(Elements.zoom.text, out x))
            x -= 1;
        Elements.zoom.text = x.ToString();
    }

    public void OnBtnShowHidefilter()
    {
        search_filterPOI = !search_filterPOI;
        panel_all_White.SetActive(!panel_all_White.activeSelf);
        // Create a temporary reference to the current scene.
        Scene currentScene = SceneManager.GetActiveScene();
        // Retrieve the name of this scene.
        string sceneName = currentScene.name;
        if (sceneName == "Map")
        {
            Elements.Btnfilter.gameObject.SetActive(!panel_all_White.activeSelf);
        }
        
        PlayerPrefs.SetInt("show_grid_poi", panel_all_White.activeSelf ? 1 : 0);
        
    }
    
    private Vector2 scrollPosition = Vector2.zero;
    
    private Vector3 MouseDownPosition = Vector3.zero;
    private void Update()
    {
        if (PlayerPrefs.GetInt("show_grid_poi") == 2)
        {
            search_filterPOI = false;
            panel_all_White.SetActive(false);
            if(PlayerPrefs.GetInt("poi_selezionato") > 0)
                PanelPOI.GetComponent<DetailPOIManager>().OpenDetailAtID(PlayerPrefs.GetInt("poi_selezionato").ToString(), true, false, 3);
            else if (PlayerPrefs.GetInt("percorso_selezionato") > 0)
                    PanelPOI.GetComponent<DetailPOIManager>().OpenDetailAtID(PlayerPrefs.GetInt("percorso_selezionato").ToString(), true, false, 1);
            PanelMAP.SetActive(false);
        }
        if (PlayerPrefs.GetInt("show_grid_poi") == 0)
        {
            search_filterPOI = false;
            panel_all_White.SetActive(false);
        }
        if (PlayerPrefs.GetInt("show_grid_poi") == 1 && !panel_all_White.activeSelf)
        {
            panel_all_White.SetActive(!panel_all_White.activeSelf);
        }
        //se il filtro per nome è attivo non muovo la mappa e non clicco sui POI, ma posso fare lo scroll nella scrollview
        if (panel_all_White.activeSelf)
        {
            if (Input.GetMouseButtonDown(0))
            {
                    MouseDownPosition = Input.mousePosition;
            }
            if (Input.GetMouseButton(0))
            {
                var delta = Input.mousePosition - MouseDownPosition;
                scrollPosition = new Vector2(scrollPosition.x + delta.x, scrollPosition.y + delta.y);
                MouseDownPosition = Input.mousePosition;

            }

            if (Input.GetMouseButtonUp(0))
            {
                MouseDownPosition = Input.mousePosition;
            }

        }
    }
    #region style

    private GUIStyle _textStyle;
    GUIStyle textStyle()
    {
        if (_textStyle == null)
        {
            _textStyle = new GUIStyle(GUI.skin.label);
            _textStyle.fontSize = 68;
            _textStyle.font = _font;
            _textStyle.fontStyle = FontStyle.Bold;
            _textStyle.normal.textColor = Color.black;
            _textStyle.normal.background = MakeBackgroundTexture(100, 100, new Color32(255, 255, 255, 255));
        }

        return _textStyle;
    }
    private GUIStyle _textFieldStyle;
    GUIStyle textFieldStyle()
    {
        if (_textFieldStyle == null)
        {
            _textFieldStyle = new GUIStyle(GUI.skin.textField);
            _textFieldStyle.font = _font;
            _textFieldStyle.fontSize = 48;
            _textFieldStyle.normal.textColor = Color.black;
            _textFieldStyle.normal.background = MakeBackgroundTexture(100, 100, new Color32(200, 200, 200, 255));
        }
        return _textFieldStyle;
    }

    private GUIStyle _errorLabelStyle;
    GUIStyle errorLabelSytle()
    {
        if (_errorLabelStyle == null)
        {
            _errorLabelStyle = new GUIStyle(GUI.skin.label);
            _errorLabelStyle.fontSize = 24;
            _errorLabelStyle.font = _font;
            _errorLabelStyle.fontStyle = FontStyle.Bold;
            _errorLabelStyle.normal.textColor = Color.red;
            _errorLabelStyle.normal.background = MakeBackgroundTexture(100, 100, new Color32(255, 255, 255, 255));

        }

        return _errorLabelStyle;
    }
    private Texture2D MakeBackgroundTexture(int width, int height, Color color)
    {
        Color[] pixels = new Color[width * height];

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }

        Texture2D backgroundTexture = new Texture2D(width, height);

        backgroundTexture.SetPixels(pixels);
        backgroundTexture.Apply();

        return backgroundTexture;
    }

    private GUIStyle _buttonStyle;
    GUIStyle buttonStyle()
    {
        if (_buttonStyle == null)
        {
            _buttonStyle = new GUIStyle(GUI.skin.button);
            _buttonStyle.fontSize = 50;
            _buttonStyle.font = _font;
            _buttonStyle.normal.textColor = Color.black;
            _buttonStyle.normal.background = MakeBackgroundTexture(100, 100, new Color32(255, 255, 255, 255));

        }

        return _buttonStyle;
    }
    #endregion
    private void OnGUI()
    {
        
        if (search_filterPOI)
        {
            float h = Screen.height;// - 220;

            var w = Screen.width;

            if (POIGo.Count > 0)
            {
                foreach (var ap in POIGo)
                    ap.Destroy();
                POIGo = new List<GameObject>();
            }
            
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(0.95f * w), GUILayout.Height(h));
            int _i = 0;
            

            for (_i = 0; _i < Results.Count && _i < _NumberOfItemsToShow; _i++)
            {
                IEP _iep = Results[_i];
                GUILayout.BeginVertical(GUILayout.Height(h / 5));
                GUILayout.BeginHorizontal(GUILayout.Height(h / 5));
                var btn = Instantiate(_btnIEP);
                var pos = btn.transform.position;
                btn.transform.position = new Vector3(pos.x, pos.y - (580 * _i), pos.z);
                btn.transform.localScale = new Vector3(1, 1, 1);
                foreach (var _component in btn.GetComponentsInChildren<UnityEngine.UI.Text>())
                {
                    if (_component.name == "Text")
                        _component.text = _iep.testo;
                    else if (_component.name == "TipoItinerario")
                        _component.text = $"{_iep.tipo_poi}";
                    else if (_component.name == "TipoPOI")
                    {
                        _component.text = $"{_iep.tipo_poi}";
                        foreach (var _component2 in _component.GetComponentsInChildren<UnityEngine.UI.Image>())
                        {
                            if (_component2.name == "ImagePOI")
                            {
                                var _gruppo = _DBClass.getGROUP_TIPO_POI(_iep.immagine_poi).FirstOrDefault();
                                if (_gruppo != null)
                                {
                                    if (_gruppo.value == "accoglienza-e-ricettivita")
                                        _component2.sprite = Resources.Load<Sprite>("Icone/manifatturiero");
                                    if (_gruppo.value == "enogastronomico")
                                        _component2.sprite = Resources.Load<Sprite>("Icone/enogastronomico");
                                    if (_gruppo.value == "naturalistico")
                                        _component2.sprite = Resources.Load<Sprite>("Icone/naturalistico");
                                    if (_gruppo.value == "religioso")
                                        _component2.sprite = Resources.Load<Sprite>("Icone/religioso_spirituale");
                                    if (_gruppo.value == "storico-artistico")
                                        _component2.sprite = Resources.Load<Sprite>("Icone/storico_artistico");
                                    if (_gruppo.value == "tempo-libero-e-sport")
                                        _component2.sprite = Resources.Load<Sprite>("Icone/sensoriale");
                                }
                            }
                        }
                    }
                    else if (_component.name == "tipologia")
                        _component.text = $"{_iep.tipo}";
                    else if (_component.name == "id")
                        _component.text = $"{_iep.tipo};{_iep.id}";
                    else if (_component.name == "Distanza")
                        _component.text = _component.text.Replace("{0}", _iep.distance.ToString("0.##"));
                    else if (_component.name == "NomeComune")
                        _component.text = _iep.nome_comune;
                    else if (_component.name == "DescrizioneBreve")
                        _component.text = _iep.descrizione_breve;
                    else if (_component.name == "Lunghezza")
                    {
                        if (!string.IsNullOrEmpty(_iep.lunghezza))
                            _component.text = _component.text.Replace("{0}", "{0} km");
                        _component.text = _component.text.Replace("{0}", _iep.lunghezza);

                        _component.text = _component.text.Replace("{1}", _iep.dislivello);
                    }
                }
                foreach (var _component in btn.GetComponentsInChildren<UnityEngine.UI.Image>())
                {
                    if (_component.name == "Image_background_scritte")
                    {
                        Color c = Color.black;
                        Color _c = Color.black;
                        Debug.Log(_iep.immagine_poi);
                        var _gruppo = _DBClass.getGROUP_TIPO_POI(_iep.immagine_poi).FirstOrDefault();
                        if (_gruppo != null)
                        {
                            //4456A3 - blu
                            //009366 - verde
                            //E8531E - arancione
                            if (_gruppo.value == "accoglienza-e-ricettivita")
                                if (ColorUtility.TryParseHtmlString("#009366", out _c))
                                    c = _c;
                            if (_gruppo.value == "enogastronomico")
                                if (ColorUtility.TryParseHtmlString("#009366", out _c))
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
                                if (ColorUtility.TryParseHtmlString("#E8531E", out _c))
                                    c = _c;
                            if (_gruppo.value == "tempo-libero-e-sport")
                                if (ColorUtility.TryParseHtmlString("#E8531E", out _c))
                                    c = _c;
                            /*
                            if (ColorUtility.TryParseHtmlString("#E8531E", out _c))
                                c = _c;
                            if(_iep.immagine_poi > 0 && _iep.immagine_poi < 4)
                            {
                                if (ColorUtility.TryParseHtmlString("#009366", out _c))
                                    c = _c;
                            }
                            else if(_iep.immagine_poi > 0 && _iep.immagine_poi < 6)
                            {
                                if (ColorUtility.TryParseHtmlString("#4456A3", out _c))
                                    c = _c;
                            }
                            */
                        }
                        _component.color = c;
                    }
                    if (_component.name == "Image")
                    {
                        byte[] foto = null;
                        if (_iep.immagine != null && _iep.immagine != null && _iep.immagine.Length > 0)
                            foto = _iep.immagine;
                        _component.sprite = _DBClass.getSpriteFromByteArray(foto);
                    }
                    if (_component.name == "Image_tipo_percorso")
                    {
                        if (_iep.tipo == 1)
                        {
                            _component.gameObject.SetActive(true);
                            if (!string.IsNullOrEmpty(_iep.tipo_percorso) && _iep.tipo_percorso != "Bici")
                            {
                                _component.sprite = Resources.Load<Sprite>("Icone/camminata_dx");
                            }
                            else
                            {
                                _component.sprite = Resources.Load<Sprite>("Icone/bicicletta");
                            }
                        }
                        else
                            _component.gameObject.SetActive(false);
                    }
                    if (_component.name == "Image_tipo_navigazione")
                    {
                        if (_iep.tipo == 1)
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


            if (_i < Results.Count - 1)
            {
                GUILayout.BeginVertical(GUILayout.Height(h / 5));
                GUILayout.BeginHorizontal(GUILayout.Height(h / 5));
                var btn = Instantiate(_btnSeeMore);
                var pos = btn.transform.position;
                btn.transform.position = new Vector3(pos.x, pos.y - (580 * _i), pos.z);
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
            float delta = (580 * (Results.Count < _NumberOfItemsToShow ? Results.Count : _NumberOfItemsToShow + 1));
            content_list_oggetti.GetComponent<RectTransform>().sizeDelta = new Vector2(0, delta);

            search_filterPOI = false;
        }
    }

    
    private int _gruppo_tipo_poi = -1;
    private int _tipo_poi = -1;
    public void search()
    {
        ResetNumberOfRowToShow();
        StartCoroutine(_DBClass.GetLatLonUsingGPS());
        
        List<DBClass.POI> POIList = new List<DBClass.POI>();
        int? comune_id = null;
        if (_grid_comune.value > 0)
            comune_id = _comuni_id[_grid_comune.value];
        List<POI> _POI = new List<POI>();
        if ((_togglePOI.isOn) || (!_togglePOI.isOn && !_toggleitinerari.isOn))
        {
            int c = -1;
            if (int.TryParse(gruppo_tipo_poi.text, out c))
            {
                if (c != _gruppo_tipo_poi)
                {
                    _gruppo_tipo_poi = c;
                }
            }
            _POI = _DBClass.getPOI(null, comune_id, QueryText.text, 0, _gruppo_tipo_poi, _tipo_poi);
        }
        float old_zoom = abstractMap.Zoom;
        
        var posizione_attuale = abstractMap.GeoToWorldPosition(new Mapbox.Utils.Vector2d(_DBClass._latitudine, _DBClass._longitudine));

        foreach (POI _p in _POI)
        {
            if (_p.tipoList != null && _p.tipoList.Count > 0)
            {
                var posizione_POI = abstractMap.GeoToWorldPosition(_DBClass.VectorFromLonLat(_p.longitudine, _p.latitudine));
                //Debug.Log($"{l.Longitude} ; {l.Latitude} - {_p.longitudine} ; {_p.latitudine}");
                _p.distanza_aria = ARLocation.MathUtils.HorizontalDistance(posizione_POI, posizione_attuale) * old_zoom;
                POIList.Add(_p);

            }
        }
        
        //Results = POIList;//.OrderBy(o => o.distanza_aria).ToList();
        abstractMap.SetZoom(old_zoom);
        search_filterPOI = true;

        
        _extract.setPOIList(POIList);
        var _listPercorsi = new List<PERCORSO>();
        if ((_toggleitinerari.isOn)||(!_toggleitinerari.isOn && !_togglePOI.isOn))
        {
            string tipo_percorso = "";
            if (_bici.isOn)
                tipo_percorso = "Bici";
            if (_piedi.isOn)
                tipo_percorso = "Piedi";
            string tipo_navigazione = "";
            if (_AR.isOn)
                tipo_navigazione = "ar-vr";
            if (_IOT.isOn)
                tipo_navigazione = "IOT";
            _listPercorsi = _DBClass.GetPERCORSO(null, null, true, comune_id, QueryText.text, tipo_percorso, tipo_navigazione);
        }
        ItinerariEventiPOI_AttaccatiAlComune _ap = new ItinerariEventiPOI_AttaccatiAlComune();
        Results = _ap.ClassiToIEP(POIList, _listPercorsi, false);
        _extract.setPercorsoList(_listPercorsi);
        GameObject.FindObjectOfType<FreeMap>().Ricalcola_Centro(true);
    }
    
}
