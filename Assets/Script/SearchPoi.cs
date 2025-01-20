using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SearchPoi : MonoBehaviour
{
    public Toggle t_Enogastronomia;
    public Toggle t_Manifatturiero;
    public Toggle t_Naturalistico;
    public Toggle t_Religioso;
    public Toggle t_Sensoriale;
    public Toggle t_Storico;
    public Toggle t_Artistico;
    public Text GruppoTipoPoi;
    public Text Label;


    private bool _changed = false;

    private int _lingua_selezionata;
    private DBClass _DBClass;
    
    // Start is called before the first frame update
    void Start()
    {
        _lingua_selezionata = PlayerPrefs.GetInt("lingua_selezionata");
        _DBClass = GameObject.FindWithTag("SQLite").GetComponent<DBClass>();

    }

    // Update is called once per frame
    void Update()
    {
        if (_changed)
        {
            string t = "0";
            string testo = string.Empty;
            if (t_Enogastronomia.isOn)
            {
                t = "1";
                var text  = t_Enogastronomia.GetComponentInChildren<Text>();
                if(text != null)
                    testo = text.text;
            }
            if (t_Manifatturiero.isOn)
            { 
                t = "2";
                var text = t_Manifatturiero.GetComponentInChildren<Text>();
                if (text != null)
                    testo = text.text;
            }
            if (t_Naturalistico.isOn)
            {
                t = "3";
                var text = t_Naturalistico.GetComponentInChildren<Text>();
                if (text != null)
                    testo = text.text;
            }
            if (t_Religioso.isOn)
            {
                t = "4";
                var text = t_Religioso.GetComponentInChildren<Text>();
                if (text != null)
                    testo = text.text;
            }
            if (t_Sensoriale.isOn)
            {
                t = "5";
                var text = t_Sensoriale.GetComponentInChildren<Text>();
                if (text != null)
                    testo = text.text;
            }
            if (t_Storico.isOn)
            {
                t = "6";
                var text = t_Storico.GetComponentInChildren<Text>();
                if (text != null)
                    testo = text.text;
            }
            if (t_Artistico.isOn)
            {
                t = "7";
                var text = t_Artistico.GetComponentInChildren<Text>();
                if (text != null)
                    testo = text.text;
            }

            if (GruppoTipoPoi.text != t)
            {
                GruppoTipoPoi.text = t;
                Label.text = testo;
            }
        }
    }
    public void change()
    {
        _changed = true;
    }
}
