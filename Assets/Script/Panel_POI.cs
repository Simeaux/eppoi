using System;
using System.Collections;
using System.Collections.Generic;
using Mapbox.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Panel_POI : MonoBehaviour
{
    public Button BtnBack;
    public GameObject _panel_poi;
    public GameObject _panel_comune;
    public Text id_selected;
    public Text selected_tab;
    public Text txtDescrizione;
    public Text txtItinerari;
    public Text txtPuntiDiInteresse;
    public Text txtBtnRealtàAumentata;

    private int _lingua_selezionata = 1;


    private void Awake()
    {
    }
    IEnumerator LoadSceneInvisible(string sceneToLoad)
    {
        // 1. Carica la scena in modo asincrono
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);

        // 2. Aspetta che la scena sia completamente caricata
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 3. Ottieni la scena appena caricata
        Scene loadedScene = SceneManager.GetSceneByName(sceneToLoad);

        // 4. Trova il root object e disattivalo
        if (loadedScene.isLoaded)
        {
            GameObject[] rootObjects = loadedScene.GetRootGameObjects();
            foreach (GameObject rootObj in rootObjects)
            {
                if ((rootObj.name == "CanvasContainer") || (rootObj.name == "Main Camera"))
                {
                    rootObj.SetActive(false);
                }
                else if ((rootObj.name == "CameraXItinerari"))
                {
                    rootObj.SetActive(true);
                }
                else if ((rootObj.name == "MapController"))
                {
                    double _x = 0;
                    double _y = 0;
                    Vector2d _min = Vector2d.zero;
                    Vector2d _max = Vector2d.zero;
                    var _freemap = rootObj.GetComponent<FreeMap>();
                    foreach (Transform child in rootObj.transform)
                    {
                        if (child.name == "GoObject")
                        {
                            foreach (Transform _obj in child.transform)
                            {
                                if (_obj.name.Contains("percorso_"))
                                {
                                    var _ap = _obj.GetComponent<Obj_x_Map_Prefab>();

                                    foreach (var _points in _ap.Waypoints)
                                    {
                                        if (_min.x == 0)
                                            _min = _points;
                                        if (_min.x > _points.x)
                                            _min.x = _points.x;
                                        if (_min.y > _points.y)
                                            _min.y = _points.y;
                                        if (_max.x == 0)
                                            _max = _points;
                                        if (_max.x < _points.x)
                                            _max.x = _points.x;
                                        if (_max.y < _points.y)
                                            _max.y = _points.y;
                                        _x += _points.x;
                                        _y += _points.y;
                                    }
                                    _x = _x / _ap.Waypoints.Count;
                                    _y = _y / _ap.Waypoints.Count;
                                    Debug.Log("Trovato figlio: " + child.name);
                                    // Esempio: distruggili o mettili in una lista
                                }
                            }
                        }
                        _freemap.zoom.text = CalcolaZoom(_min, _max).ToString();
                        _freemap.SpostaCentro(_x, _y);
                        //_freemap.Ricalcola_Centro(true);
                    }
                }
            }
        }
    }
    private float CalcolaZoom(Vector2d p1, Vector2d p2)
    {
        // Differenza di latitudine e longitudine
        double latDiff = Math.Abs(p1.x - p2.x);
        double lonDiff = Math.Abs(p1.y - p2.y);

        // Zoom basato sulla differenza maggiore (per far stare tutto nello schermo)
        double maxDiff = Math.Max(latDiff, lonDiff);

        if (maxDiff <= 0) return 18; // Se i punti sono uguali, zoom massimo

        // Formula semplificata per calcolare lo zoom logaritmico
        // 360 è l'estensione del mondo, il resto dipende dalla proiezione di Mercatore
        float zoom = (float)(Math.Log(360.0 / maxDiff) / Math.Log(2));

        return Mathf.Clamp(zoom, 0, 20);
    }
    private void Start()
    {
        _lingua_selezionata = PlayerPrefs.GetInt("lingua_selezionata");

        txtDescrizione.text = _lingua_selezionata == 1 ? "Descrizione" : "Description";
        txtItinerari.text = _lingua_selezionata == 1 ? "Itinerari" : "Itineraries";
        txtPuntiDiInteresse.text = _lingua_selezionata == 1 ? "Punti di interesse" : "Points of interest";
        txtBtnRealtàAumentata.text = _lingua_selezionata == 1 ? "Realtà aumentata" : "Augmented reality";

        // Create a temporary reference to the current scene.
        Scene currentScene = SceneManager.GetActiveScene();
        // Retrieve the name of this scene.
        string sceneName = currentScene.name;
        if (sceneName == "Map")
            BtnBack.gameObject.SetActive(false);
        else if (sceneName == "Menu")
            BtnBack.gameObject.SetActive(true);
    }
    private void Update()
    {
        if (PlayerPrefs.GetString("percorso_selezionato_collegato_ad_un_poi") != "")
        {
            // Verifica se è già carica, altrimenti la aggiunge
            if (!PlayerPrefs.HasKey("loaded_map") && !SceneManager.GetSceneByName("Map").isLoaded)
            {
                PlayerPrefs.SetInt("loaded_map", 1);
                if (SceneManager.GetActiveScene().name == "Menu")
                    StartCoroutine(LoadSceneInvisible("Map"));
            }

            Debug.Log("percorso_selezionato_collegato_ad_un_poi = " + PlayerPrefs.GetString("percorso_selezionato_collegato_ad_un_poi"));
            PlayerPrefs.SetString("poi_selezionato", (long.Parse(PlayerPrefs.GetString("poi_selezionato")) * -1).ToString());
            PlayerPrefs.SetString("percorso_selezionato", PlayerPrefs.GetString("percorso_selezionato_collegato_ad_un_poi"));
            PlayerPrefs.SetString("percorso_selezionato_collegato_ad_un_poi", "");
            id_selected.text = PlayerPrefs.GetString("percorso_selezionato");
            if (!string.IsNullOrEmpty(id_selected.text))
                _panel_poi.GetComponent<DetailPOIManager>().OpenDetailAtID(id_selected.text, false, true, 1);
        }
        if (PlayerPrefs.GetString("poi_selezionato_collegato_ad_un_percorso") != "")
        {

            if (SceneManager.GetSceneByName("Map").isLoaded)
            {
                SceneManager.UnloadSceneAsync("Map");
            }
            Debug.Log("poi_selezionato_collegato_ad_un_percorso = " + PlayerPrefs.GetString("poi_selezionato_collegato_ad_un_percorso"));
            PlayerPrefs.SetString("poi_selezionato", PlayerPrefs.GetString("poi_selezionato_collegato_ad_un_percorso"));
            PlayerPrefs.SetString("percorso_selezionato", (long.Parse(PlayerPrefs.GetString("percorso_selezionato")) * -1).ToString());
            PlayerPrefs.SetString("poi_selezionato_collegato_ad_un_percorso", "");
            id_selected.text = PlayerPrefs.GetString("poi_selezionato");
            if (!string.IsNullOrEmpty(id_selected.text))
                _panel_poi.GetComponent<DetailPOIManager>().OpenDetailAtID(id_selected.text, false, true, 3);
        }
    }

    private void OnEnable()
    {

        BtnBack.onClick.AddListener(BtnBackCliccked);
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("id_select")))
        {
            id_selected.text = PlayerPrefs.GetString("id_select");
            PlayerPrefs.SetString("id_select", string.Empty);
        }
        //BtnBack.onClick.AddListener(BtnBackCliccked);
        if (PlayerPrefs.GetString("poi_selezionato") != "")
        {
            // Verifica se è già carica, cadsomai la distrugge
            if (SceneManager.GetSceneByName("Map").isLoaded)
            {
                SceneManager.UnloadSceneAsync("Map");
            }
            PlayerPrefs.SetString("apri_direttamente_il_poi_selezionato", "");
            id_selected.text = PlayerPrefs.GetString("poi_selezionato");
            if (!string.IsNullOrEmpty(id_selected.text))
                _panel_poi.GetComponent<DetailPOIManager>().OpenDetailAtID(id_selected.text, false, true, 3);
        }
        else if (PlayerPrefs.GetString("percorso_selezionato") != "")
        {

            // Verifica se è già carica, altrimenti la aggiunge
            if (!PlayerPrefs.HasKey("loaded_map") && !SceneManager.GetSceneByName("Map").isLoaded)
            {
                PlayerPrefs.SetInt("loaded_map", 1);
                if (SceneManager.GetActiveScene().name == "Menu")
                    StartCoroutine(LoadSceneInvisible("Map"));
            }
            id_selected.text = PlayerPrefs.GetString("percorso_selezionato");
            if (!string.IsNullOrEmpty(id_selected.text))
                _panel_poi.GetComponent<DetailPOIManager>().OpenDetailAtID(id_selected.text, false, true, 1);
        }
    }
    private void OnDisable()
    {
        BtnBack.onClick.RemoveListener(BtnBackCliccked);

    }
    void OnDestroy()
    {
        // Verifica se è già carica, cadsomai la distrugge
        if (SceneManager.GetSceneByName("Map").isLoaded)
        {
            SceneManager.UnloadSceneAsync("Map");
        }
        PlayerPrefs.DeleteKey("loaded_map");
        PlayerPrefs.Save(); // Forza il salvataggio immediato su disco
    }
    private void BtnBackCliccked()
    {
        Debug.Log("BtnBackCliccked");
        if (PlayerPrefs.GetString("percorso_selezionato").Contains("-"))
            PlayerPrefs.SetString("percorso_selezionato", PlayerPrefs.GetString("percorso_selezionato").Replace("-", ""));
        else
            PlayerPrefs.SetString("percorso_selezionato", "");

        if (PlayerPrefs.GetString("evento_selezionato") != "")
            PlayerPrefs.SetString("evento_selezionato", (long.Parse(PlayerPrefs.GetString("evento_selezionato")) * -1).ToString());
        else
            PlayerPrefs.SetString("evento_selezionato", "");
        long _app;
        if (long.TryParse(PlayerPrefs.GetString("poi_selezionato"), out _app))
        {
            if (_app < 0)
                PlayerPrefs.SetString("poi_selezionato", (long.Parse(PlayerPrefs.GetString("poi_selezionato")) * -1).ToString());
            else
                PlayerPrefs.SetString("poi_selezionato", "");
        }

        selected_tab.text = "0";
        //PlayerPrefs.SetString("istat", "");
        _panel_poi.SetActive(false);
        if (_panel_comune != null)
            _panel_comune.SetActive(true);
    }
}
