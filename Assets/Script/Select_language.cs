using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using ARLocation;
using UnityEngine;
using UnityEngine.UI;
using static DBClass;
using static TouchScript.Behaviors.Cursors.UI.GradientTexture;

public class Select_language : MonoBehaviour
{
    public Button BtnItaliano;
    public Button BtnInglese;
    public Toggle conferma;

    public GameObject panel_select_language;
    public GameObject panel_benvenuto;

    public Text txtNonChiedereNuovamente;

    private int _lingua_selezionata = 1;

    private DBClass _DBClass;

    // Start is called before the first frame update
    void Start()
    {
        //Setto il parametro del comune selezionato all'istat di Serrapetrona
        //      PlayerPrefs.SetString("istat", "043051");
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
            _DBClass.CreateDB(true);
        }
        _lingua_selezionata = PlayerPrefs.GetInt("lingua_selezionata");

        txtNonChiedereNuovamente.text = _lingua_selezionata == 1 ? "Non chiedere nuovamente" : "Don't ask again";
    }

    private void OnEnable()
    {
        BtnItaliano.onClick.AddListener(onSelectLanguageItaliano);
        BtnInglese.onClick.AddListener(onSelectLanguageInglese);
    }
    private void OnDisable()
    {
        BtnItaliano.onClick.RemoveListener(onSelectLanguageItaliano);
        BtnInglese.onClick.RemoveListener(onSelectLanguageInglese);
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
    private void goOn()
    {
        panel_select_language.SetActive(!panel_select_language.activeSelf);
        panel_benvenuto.SetActive(!panel_select_language.activeSelf);
    }
}
