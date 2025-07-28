using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static DBClass;

public class Panel_benvenuto : MonoBehaviour
{
    public Text benvenuto;
    public Text txt_NonMostrareNuovamente;
    public Toggle toggle_NonMostrareNuovamente;
    public TMP_Text descrizione;
    public Button BtnAvanti;

    public GameObject panel_benvenuto;
    public GameObject panel_principale;

    private DBClass _DBClass;
    private string _saltaBenvenuto = "salta_benvenuto";
    private int _lingua_selezionata = 1;
    private string _istat = "";
    // Start is called before the first frame update
    private void Start()
    {
        _lingua_selezionata = PlayerPrefs.GetInt("lingua_selezionata");
        _istat = PlayerPrefs.GetString("istat");
        _DBClass = GameObject.FindWithTag("SQLite").GetComponent<DBClass>();
        var set = _DBClass.getSETTING(_saltaBenvenuto);
        if(set != null && set.Count > 0 && set[0].valore == "1")
        {
            goOn();
        }
    
        descrizione.text = string.Empty;
        benvenuto.text = _lingua_selezionata == 1 ? "Benvenuto" : "Welcome";
        txt_NonMostrareNuovamente.text = _lingua_selezionata == 1 ? "Non mostrare nuovamente" : "Do not show again";
        BtnAvanti.gameObject.GetComponentsInChildren<Text>()[0].text = _lingua_selezionata == 1 ? "Avanti" : "Skip";

        
        if (!string.IsNullOrEmpty(_istat))
        {
            var _ListComuni = _DBClass.GetCOMUNI(_istat, string.Empty, null);
            if(_ListComuni!= null && _ListComuni.Count > 0)
            {
                COMUNE _comune = _ListComuni[0];
                if(_comune != null)
                {
                    if (_lingua_selezionata == 1)
                    {
                        benvenuto.text += $" a ";
                    }
                    else
                    {
                        benvenuto.text += $" to ";
                    }
                    benvenuto.text += $"{_comune.nome_comune}";
                    descrizione.text = _comune.descrizione();
                }
            }
        }
        if(string.IsNullOrEmpty(descrizione.text))
        {
            descrizione.text = "!!!Descrizione di default!!!";
        }
    }
    private void OnEnable()
    {
        BtnAvanti.onClick.AddListener(onAvanti);
    }
    public void OnDisable()
    {
        BtnAvanti.onClick.RemoveListener(onAvanti);
    }
    private void onAvanti()
    {
        if (toggle_NonMostrareNuovamente.isOn)
        {
            _DBClass.SetSetting(_saltaBenvenuto, "1");
        }
        goOn();
    }
    private void goOn()
    {
        GameObject.FindObjectOfType<ChangeScene>().ResetPlayer();
        panel_benvenuto.SetActive(!panel_benvenuto.activeSelf);
        panel_principale.SetActive(!panel_benvenuto.activeSelf);
    }
    
}
