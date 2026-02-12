using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using static DBClass;



public class Panel_Principale : MonoBehaviour
{
    public GameObject content_list_comuni;
    public Text filtro_nome;
    public GameObject _panel_poi;
    public GameObject _panel_comune;
    public GameObject _panel_principale;
    public GameObject _panel_eventi;
    public Text txtLuoghi;
    public Text txtItinerari;
    public Text txtPuntiDiInteresse;
    public Canvas _aggiorna_app;

    private GameObject _btnComune;
    private int _lingua_selezionata = 1;
    private string _istat = "";
    private string queryString = "";
    private DBClass _DBClass;
    private List<GameObject> POIGo = new List<GameObject>();
    public GameObject _logo;
    private bool _rotate = false;

    // Start is called before the first frame update
    private void Start()
    {
        _lingua_selezionata = PlayerPrefs.GetInt("lingua_selezionata");
        _istat = "";// PlayerPrefs.GetString("istat");
        _DBClass = GameObject.FindWithTag("SQLite").GetComponent<DBClass>();
        _btnComune = (GameObject)Resources.Load("Button_comune");

        txtLuoghi.text = _lingua_selezionata == 1 ? "Luoghi" : "Places";
        txtItinerari.text = _lingua_selezionata == 1 ? "Itinerari" : "Itineraries";
        txtPuntiDiInteresse.text = _lingua_selezionata == 1 ? "Punti di interesse" : "Points of interest";
        _rotate = true;
        _ = CallAsync();
        if (PlayerPrefs.GetString("apri_direttamente_il_poi_selezionato") != "")
        {
            _panel_principale.SetActive(false);
            PlayerPrefs.SetString("poi_selezionato", PlayerPrefs.GetString("apri_direttamente_il_poi_selezionato"));
            _panel_poi.SetActive(true);
        }
        if (PlayerPrefs.HasKey("comune_selected") && PlayerPrefs.GetInt("comune_selected") > 0)
        {
            var _comune = _DBClass.GetCOMUNI(null, null, PlayerPrefs.GetInt("comune_selected"));
            if (_comune != null)
            {
                PlayerPrefs.SetString("istat", _comune.FirstOrDefault().istat);
                _panel_principale.SetActive(false);
                _panel_comune.SetActive(true);
            }
        }
    }

    private async Task CallAsync()
    {

        var client = new HttpClient();
        var dividi_per = ";--";
        //POI
        var versione = _DBClass.getLastUpdatedFromTable("VERSIONE");
        var poi_mod_dte = _DBClass.getLastUpdatedFromTable("POI");
        var comuni_mod_dte = _DBClass.getLastUpdatedFromTable("COMUNI_TEXT");
        var percorsi_mod_dte = _DBClass.getLastUpdatedFromTable("PERCORSI");

        var call = "https://www.macerataturismo.it/wp-json/rest_api_ws/v1/aggiorna_app_eppoi?VERSIONE=" + versione + "&POI=" + poi_mod_dte + "&COMUNI=" + comuni_mod_dte + "&PERCORSI=" + percorsi_mod_dte;
        Debug.Log(call);

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(call),
        };
        using (var response = await client.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            if (body.Contains("_Necessita_aggiornamento_app_eppoi_--"))
            {
                if (Gia_chiesto_aggiornamento.gia_chiesto == 0)
                    _aggiorna_app.gameObject.SetActive(true);
                body = body.Replace("_Necessita_aggiornamento_app_eppoi_--", "");
                Gia_chiesto_aggiornamento.gia_chiesto = 1;
            }
            var body_list = body.Split(dividi_per);
            foreach (var b in body_list)
            {
                _DBClass.exec_sql(b + dividi_per);
            }
            Debug.Log("Done Synch");
            _rotate = false;
            //BtnItaliano.gameObject.SetActive(true);
            //BtnInglese.gameObject.SetActive(true);
        }
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

        if ((PlayerPrefs.GetInt("percorso_selezionato") > 0 || PlayerPrefs.GetString("poi_selezionato") != ""))
        {
            _istat = PlayerPrefs.GetString("istat");
            _panel_poi.SetActive(true);
            _panel_comune.SetActive(false);
            _panel_principale.SetActive(false);
        }
        else if (_istat != PlayerPrefs.GetString("istat") && !string.IsNullOrEmpty(PlayerPrefs.GetString("istat")))
        {
            _istat = PlayerPrefs.GetString("istat");
            _panel_comune.SetActive(true);
            _panel_principale.SetActive(false);
        }
        if (string.IsNullOrEmpty(PlayerPrefs.GetString("istat")))
            _istat = "";// PlayerPrefs.GetString("istat");
        if (queryString != filtro_nome.text)
        {
            queryString = filtro_nome.text;
            _towrite = true;
        }
    }
    private Vector2 scrollPosition = Vector2.zero;
    private bool _towrite = true;
    // Update is called once per frame
    private void OnGUI()
    {
        if (_towrite)
        {
            if (POIGo.Count > 0)
            {
                foreach (var ap in POIGo)
                    ap.Destroy();
                POIGo = new List<GameObject>();
            }
            float h = Screen.height;// - 220;
            GUILayout.BeginVertical(new GUIStyle() { padding = new RectOffset(20, 20, 240, 20) }, GUILayout.MaxHeight(h), GUILayout.Height(h));

            var comuni = _DBClass.GetCOMUNI(string.Empty, filtro_nome.text, null, true);
            var w = Screen.width;
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(0.95f * w), GUILayout.Height(h - 700));
            for (int i = 0; i < comuni.Count; i++)
            {
                COMUNE _comune = comuni[i];
                GUILayout.BeginVertical(GUILayout.Height(h / 5));
                GUILayout.BeginHorizontal(GUILayout.Height(h / 5));
                var btn = Instantiate(_btnComune);
                var pos = btn.transform.position;
                btn.transform.position = new Vector3(pos.x, (-1 * (pos.y + (737 * i))), pos.z);
                btn.transform.localScale = new Vector3(1, 1, 1);
                foreach (var _component in btn.GetComponentsInChildren<Text>())
                {
                    if (_component.name == "NomeComune")
                    {
                        _component.text = _comune.nome_comune;
                        if (!string.IsNullOrEmpty(_comune.provincia))
                            _component.text = $"{_component.text} ({_comune.provincia})";
                    }
                    else if (_component.name == "TestoBreveComune")
                        _component.text = _comune.descrizioneBreve();
                    else if (_component.name == "Istat")
                        _component.text = _comune.istat;
                }
                foreach (var _component in btn.GetComponentsInChildren<Image>())
                {

                    if (_component.name == "ImmagineComune")
                    {
                        byte[] foto = null;
                        if (_comune.Listimages != null && _comune.Listimages.Count > 0 && _comune.Listimages[0].image != null && _comune.Listimages[0].image.Length > 0)
                        {
                            foto = _comune.Listimages[0].image;
                        }
                        _component.sprite = _DBClass.getSpriteFromByteArray(foto);
                    }

                }
                btn.SetActive(true);
                btn.transform.SetParent(content_list_comuni.transform, false);
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                POIGo.Add(btn);
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            _towrite = false;
            content_list_comuni.GetComponent<RectTransform>().sizeDelta = new Vector2(0, (737 * comuni.Count) + 325);
        }
    }
    public void OpenStoreForUpdate()
    {
        string appId = "IL_TUO_BUNDLE_ID"; // Es: com.azienda.gioco
        string appleId = "IL_TUO_APPLE_ID"; // Es: 123456789 (solo numeri)

#if UNITY_ANDROID
        // Apre direttamente l'app Play Store sulla pagina della tua app
        Application.OpenURL("market://details?id=" + appId);
#elif UNITY_IPHONE
        // Apre l'App Store sulla pagina del tuo gioco
        Application.OpenURL("itms-apps://://itunes.apple.com" + appleId);
#else
        Debug.Log("Piattaforma non supportata o URL browser generico");
#endif
    }
}
