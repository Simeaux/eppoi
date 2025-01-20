using System.Collections;
using System.Collections.Generic;
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
        if (PlayerPrefs.GetInt("percorso_selezionato_collegato_ad_un_poi") > 0)
        {
            Debug.Log("percorso_selezionato_collegato_ad_un_poi = " + PlayerPrefs.GetInt("percorso_selezionato_collegato_ad_un_poi"));
            PlayerPrefs.SetInt("poi_selezionato", 0);
            PlayerPrefs.SetInt("percorso_selezionato", PlayerPrefs.GetInt("percorso_selezionato_collegato_ad_un_poi"));
            PlayerPrefs.SetInt("percorso_selezionato_collegato_ad_un_poi", 0);
            id_selected.text = PlayerPrefs.GetInt("percorso_selezionato").ToString();
            if (!string.IsNullOrEmpty(id_selected.text))
                _panel_poi.GetComponent<DetailPOIManager>().OpenDetailAtID(id_selected.text, false, true, 1);
        }
        if (PlayerPrefs.GetInt("poi_selezionato_collegato_ad_un_percorso") > 0)
        {
            Debug.Log("poi_selezionato_collegato_ad_un_percorso = " + PlayerPrefs.GetInt("poi_selezionato_collegato_ad_un_percorso"));
            PlayerPrefs.SetInt("poi_selezionato", PlayerPrefs.GetInt("poi_selezionato_collegato_ad_un_percorso"));
            PlayerPrefs.SetInt("percorso_selezionato", 0);
            PlayerPrefs.SetInt("poi_selezionato_collegato_ad_un_percorso", 0);
            id_selected.text = PlayerPrefs.GetInt("poi_selezionato").ToString();
            if (!string.IsNullOrEmpty(id_selected.text))
                _panel_poi.GetComponent<DetailPOIManager>().OpenDetailAtID(id_selected.text, false, true, 3);
        }
    }
    private void OnEnable()
    {
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("id_select")))
        {
            id_selected.text = PlayerPrefs.GetString("id_select");
            PlayerPrefs.SetString("id_select", string.Empty);
        }
        BtnBack.onClick.AddListener(BtnBackCliccked);
        if (PlayerPrefs.GetInt("poi_selezionato") > 0)
        {
            id_selected.text = PlayerPrefs.GetInt("poi_selezionato").ToString();
            if (!string.IsNullOrEmpty(id_selected.text))
                _panel_poi.GetComponent<DetailPOIManager>().OpenDetailAtID(id_selected.text, false, true, 3);
        }
        else if (PlayerPrefs.GetInt("percorso_selezionato") > 0)
        {
            id_selected.text = PlayerPrefs.GetInt("percorso_selezionato").ToString();
            if (!string.IsNullOrEmpty(id_selected.text))
                _panel_poi.GetComponent<DetailPOIManager>().OpenDetailAtID(id_selected.text, false, true, 1);
        }
    }
    private void OnDisable()
    {
        BtnBack.onClick.RemoveListener(BtnBackCliccked);
    }

    private void BtnBackCliccked()
    {
        Debug.Log("BtnBackCliccked");
        PlayerPrefs.SetInt("percorso_selezionato", 0);
        PlayerPrefs.SetInt("evento_selezionato", 0);
        PlayerPrefs.SetInt("poi_selezionato", 0);
        selected_tab.text = "0";
        //PlayerPrefs.SetString("istat", "");
        _panel_poi.SetActive(false);
        if(_panel_comune != null)
            _panel_comune.SetActive(true);
    }
}
