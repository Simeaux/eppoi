using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SearchPoi : MonoBehaviour
{
    public Toggle t_Accoglienza_e_Ricettivita;
    public Toggle t_Enogastronomico;
    public Toggle t_Manifatturiero;
    public Toggle t_Naturalistico;
    public Toggle t_Religioso;
    public Toggle t_Storico_Artistico;
    public Toggle t_Tempo_libero_e_sport;
    public Toggle t_Varie;
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
            List<DBClass.GROUP_TIPO_POI> _group_tipo_poi = _DBClass.getGROUP_TIPO_POI();
            if (t_Accoglienza_e_Ricettivita.isOn)
            {
                var ap = _group_tipo_poi.Find(p => p.value == "accoglienza-e-ricettivita");
                if (ap != null)
                    t = ap.id.ToString();
                var text = t_Accoglienza_e_Ricettivita.GetComponentInChildren<Text>();
                if (text != null)
                    testo = text.text;
            }
            if (t_Enogastronomico.isOn)
            {
                var ap = _group_tipo_poi.Find(p => p.value == "enogastronomico");
                if (ap != null)
                    t = ap.id.ToString();
                var text = t_Enogastronomico.GetComponentInChildren<Text>();
                if (text != null)
                    testo = text.text;
            }
            if (t_Manifatturiero.isOn)
            {
                var ap = _group_tipo_poi.Find(p => p.value == "manifatturiero");
                if (ap != null)
                    t = ap.id.ToString();
                var text = t_Manifatturiero.GetComponentInChildren<Text>();
                if (text != null)
                    testo = text.text;
            }
            if (t_Naturalistico.isOn)
            {
                var ap = _group_tipo_poi.Find(p => p.value == "naturalistico");
                if (ap != null)
                    t = ap.id.ToString();

                var text = t_Naturalistico.GetComponentInChildren<Text>();
                if (text != null)
                    testo = text.text;
            }
            if (t_Religioso.isOn)
            {
                var ap = _group_tipo_poi.Find(p => p.value == "religioso");
                if (ap != null)
                    t = ap.id.ToString();
                var text = t_Religioso.GetComponentInChildren<Text>();
                if (text != null)
                    testo = text.text;
            }
            if (t_Storico_Artistico.isOn)
            {
                var ap = _group_tipo_poi.Find(p => p.value == "storico-artistico");
                if (ap != null)
                    t = ap.id.ToString();
                var text = t_Storico_Artistico.GetComponentInChildren<Text>();
                if (text != null)
                    testo = text.text;
            }
            if (t_Tempo_libero_e_sport.isOn)
            {
                var ap = _group_tipo_poi.Find(p => p.value == "tempo-libero-e-sport");
                if (ap != null)
                    t = ap.id.ToString();
                var text = t_Tempo_libero_e_sport.GetComponentInChildren<Text>();
                if (text != null)
                    testo = text.text;
            }
            if (t_Varie.isOn)
            {
                var ap = _group_tipo_poi.Find(p => p.value == "varie");
                if (ap != null)
                    t = ap.id.ToString();
                var text = t_Varie.GetComponentInChildren<Text>();
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
