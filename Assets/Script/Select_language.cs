using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using ARLocation;
using ARLocation.UI;
using ARLocation.Utils;
using Mapbox.Json.Linq;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Networking;
using UnityEngine.UI;
using static DBClass;
//using static TouchScript.Behaviors.Cursors.UI.GradientTexture;

public class Select_language : MonoBehaviour
{
    public Button BtnItaliano;
    public Button BtnInglese;
    public Button BtnCancella_DB;
    public Toggle conferma;
    public Slider slider;

    public GameObject _logo;
    public GameObject panel_select_language;
    public GameObject panel_benvenuto;

    public Text txtNonChiedereNuovamente;
    public Toggle NonChiedereNuovamente;

    private int _lingua_selezionata = 1;

    private DBClass _DBClass;

    private bool _rotate = false;
    // Start is called before the first frame update
    void Start()
    {
        //Setto il parametro del comune selezionato all'istat di Serrapetrona
        _DBClass = GameObject.FindWithTag("SQLite").GetComponent<DBClass>();
        try
        {
            if (_DBClass.getSetting_LinguaSelezionata() > 0)
            {
                PlayerPrefs.SetInt("lingua_selezionata", _DBClass.getSetting_LinguaSelezionata());
                goOn();
            }
        }
        catch
        {
            //if (slider != null)
            //    _DBClass.CreateDB(slider);
        }
        _lingua_selezionata = PlayerPrefs.GetInt("lingua_selezionata");
        _DBClass.copyDB(slider, BtnItaliano, BtnInglese, NonChiedereNuovamente);
        txtNonChiedereNuovamente.text = _lingua_selezionata == 1 ? "Non chiedere nuovamente" : "Don't ask again";
        if (PlayerPrefs.GetString("apri_direttamente_il_poi_selezionato") != "")
        {
            panel_select_language.SetActive(false);
            panel_benvenuto.SetActive(true);
        }
    }

    private void OnEnable()
    {
        BtnItaliano.onClick.AddListener(onSelectLanguageItaliano);
        BtnInglese.onClick.AddListener(onSelectLanguageInglese);
        BtnCancella_DB.onClick.AddListener(onCancellaDB);
    }
    private void OnDisable()
    {
        BtnItaliano.onClick.RemoveListener(onSelectLanguageItaliano);
        BtnInglese.onClick.RemoveListener(onSelectLanguageInglese);
        BtnCancella_DB.onClick.RemoveListener(onCancellaDB);
    }
    private void onSelectLanguageItaliano()
    {
        PlayerPrefs.SetInt("lingua_selezionata", 1);
        if (conferma.isOn)
            _DBClass.setSetting_LinguaSelezionata(1);
        goOn();
    }
    private void onSelectLanguageInglese()
    {
        PlayerPrefs.SetInt("lingua_selezionata", 2);
        if (conferma.isOn)
            _DBClass.setSetting_LinguaSelezionata(2);
        goOn();
    }
    private void onCancellaDB()
    {
        _DBClass.RemovePersistent_DB(slider, BtnItaliano, BtnInglese, NonChiedereNuovamente);
        slider.gameObject.SetActive(true);
        BtnItaliano.gameObject.SetActive(false);
        BtnInglese.gameObject.SetActive(false);
        NonChiedereNuovamente.gameObject.SetActive(false);
        _DBClass.copyDB(slider, BtnItaliano, BtnInglese, NonChiedereNuovamente);
    }
    private void goOn()
    {
        panel_select_language.SetActive(!panel_select_language.activeSelf);
        panel_benvenuto.SetActive(!panel_select_language.activeSelf);
    }
    private void Update()
    {
        var scale = _logo.transform.localScale;
        if (_rotate)
        {
            /*
            if (scale.x > 0.5f)
            {
                var _x = scale.x -0.02f;
                _logo.transform.localScale = new Vector3(_x, _x, _x);
            }
            */
            _logo.transform.Rotate(0, 0, -0.4f);
        }
        else
        {
            /*
            if(scale.x < 1f)
            {
                var _x = scale.x + 0.02f;
                _logo.transform.localScale = new Vector3(_x, _x, _x);
            }
            */
            if (_logo.transform.rotation != new Quaternion(0, 0, 0, 0))
                _logo.transform.rotation = new Quaternion(0, 0, 0, 0);
        }
    }
}
