using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static DBClass;

public class PanelFooter : MonoBehaviour
{
    public Button btnHome;
    public Button btnMap;
    public Button btnNavigatore;
    public Button btnAudio;
    public Button btnInfo;
    public Text txtHome;
    public Text txtMap;
    public Text txtNavigatore;
    public Text txtSearch;
    public Text txtAudio;
    public Text txtInfo;
    public ChangeScene change;


    private int _lingua_selezionata = 1;
    private Color lightgray = new Color(0.8f, 0.8f, 0.8f, 1.0f);

    // Start is called before the first frame update
    void Start()
    {
        _lingua_selezionata = PlayerPrefs.GetInt("lingua_selezionata");
        if (_lingua_selezionata == 2)
        {
            if (txtHome != null)
                txtHome.text = "Home";
            if (txtMap != null)
                txtMap.text = "Map";
            if (txtNavigatore != null)
                txtNavigatore.text = "Start Navigation";
            if (txtAudio != null)
                txtAudio.text = "Play Audio";
            if (txtInfo != null)
                txtInfo.text = "Info";
            if (txtSearch != null)
                txtSearch.text = "Search";

        }
    }
    private void OnEnable()
    {
        btnHome.onClick.AddListener(btnHomeCliccked);
        btnMap.onClick.AddListener(btnMapCliccked);
        btnNavigatore.onClick.AddListener(btnNavigatoreCliccked);
        btnInfo.onClick.AddListener(btnInfoCliccked);
        //btnSearch.onClick.AddListener(btnSearchCliccked);


    }
    private void OnDisable()
    {
        btnHome.onClick.RemoveListener(btnHomeCliccked);
        btnMap.onClick.RemoveListener(btnMapCliccked);
        btnNavigatore.onClick.RemoveListener(btnNavigatoreCliccked);
        btnInfo.onClick.RemoveListener(btnInfoCliccked);
        //btnSearch.onClick.RemoveListener(btnSearchCliccked);

    }

    // Update is called once per frame
    void Update()
    {
        if (string.IsNullOrEmpty(PlayerPrefs.GetString("istat")) && !(PlayerPrefs.GetString("poi_selezionato") != "") && btnNavigatore.enabled)
            btnNavigatore.enabled = false;
        else if ((!string.IsNullOrEmpty(PlayerPrefs.GetString("istat")) || PlayerPrefs.GetString("poi_selezionato") != "") && !btnNavigatore.enabled)
            btnNavigatore.enabled = true;

        if (!btnNavigatore.enabled && btnNavigatore.gameObject.GetComponentInChildren<Image>().color != lightgray)
        {
            btnNavigatore.gameObject.GetComponentInChildren<Image>().color = lightgray;
            txtNavigatore.color = lightgray;
        }
        else if (btnNavigatore.enabled && btnNavigatore.gameObject.GetComponentInChildren<Image>().color != Color.black)
        {
            btnNavigatore.gameObject.GetComponentInChildren<Image>().color = Color.black;
            txtNavigatore.color = Color.black;
        }
        if (txtMap != null)
        {
            string label_map = "Mappa";
            if (_lingua_selezionata == 2)
            {
                label_map = "Map";
            }
            if (SceneManager.GetActiveScene().name == "Map")
            {
                if (_lingua_selezionata == 1)
                    label_map = "Dove sono";
                else
                    label_map = "I'm Here";
            }
            if (label_map != txtMap.text)
                txtMap.text = label_map;
        }
    }

    private void btnHomeCliccked()
    {
        change.Load_Menu();
        //GameObject.FindObjectOfType<ChangeScene>().Load_Menu();
        //gameObject.AddComponent<ChangeScene>().Load_Menu();
    }

    private void btnMapCliccked()
    {
        if (SceneManager.GetActiveScene().name == "Map")
        {
            if (PlayerPrefs.GetString("poi_selezionato") != "")
            {
                GameObject.FindObjectOfType<DetailPOIManager>().ID_selected.text = PlayerPrefs.GetString("poi_selezionato");
                //GameObject.FindObjectOfType<DetailPOIManager>().Mostra_nella_mappa();
                GameObject.FindObjectOfType<ManageCanvasPOI>().HideCanvasPOI();
            }
            else
            {
                btnImHereCliccked();
            }
        }
        else
            change.Load_Map();
        //GameObject.FindObjectOfType<ChangeScene>().Load_Map();
        //gameObject.AddComponent<ChangeScene>().Load_Map();


    }
    private void btnInfoCliccked()
    {
        change.Load_QR();
    }

    private void btnNavigatoreCliccked()
    {
        if (PlayerPrefs.GetString("poi_selezionato") != "")
        {
            // navigo verso il poi
            var _POI = GameObject.FindObjectOfType<DBClass>().getPOI(long.Parse(PlayerPrefs.GetString("poi_selezionato")));
            if (_POI != null && _POI.Count > 0)
            {
                POI _selected_poi = _POI[0];
#if UNITY_ANDROID
                Application.OpenURL($"google.navigation:q={_selected_poi.latitudine},{_selected_poi.longitudine}");
#elif UNITY_IOS
                Application.OpenURL($"http://maps.apple.com/maps?saddr=Current+Location&daddr={_selected_poi.latitudine},{_selected_poi.longitudine}");
#else
            Application.OpenURL($"http://maps.google.com/maps?saddr=My+Location&daddr={_selected_poi.latitudine},{_selected_poi.longitudine}");
#endif
            }
        }
        else if (PlayerPrefs.GetString("percorso_selezionato") != "")
        {
            // navigo verso il poi
            var _POIXTAPPE = GameObject.FindObjectOfType<DBClass>().getPOIXTAPPE(null, null, null, long.Parse(PlayerPrefs.GetString("percorso_selezionato")));
            if (_POIXTAPPE != null && _POIXTAPPE.Count > 0)
            {
                var _POI = GameObject.FindObjectOfType<DBClass>().getPOI(_POIXTAPPE[0].poi_id);
                if (_POI != null && _POI.Count > 0)
                {
                    POI _selected_poi = _POI[0];
#if UNITY_ANDROID
                    Application.OpenURL($"google.navigation:q={_selected_poi.latitudine},{_selected_poi.longitudine}");
#elif UNITY_IOS
                    Application.OpenURL($"http://maps.apple.com/maps?saddr=Current+Location&daddr={_selected_poi.latitudine},{_selected_poi.longitudine}");
#else
                    Application.OpenURL($"http://maps.google.com/maps?saddr=My+Location&daddr={_selected_poi.latitudine},{_selected_poi.longitudine}");
#endif
                }
            }
        }
        else if (!string.IsNullOrEmpty(PlayerPrefs.GetString("istat")))
        {
            //navigo verso il comune
            var _COMUNE = GameObject.FindObjectOfType<DBClass>().GetCOMUNI(PlayerPrefs.GetString("istat"));
            if (_COMUNE != null && _COMUNE.Count > 0)
            {
                var _selected_comune = _COMUNE[0];
#if UNITY_ANDROID
                Application.OpenURL($"google.navigation:q={_selected_comune.nome_comune}");
#elif UNITY_IOS
                Application.OpenURL($"http://maps.apple.com/maps?saddr=Current+Location&daddr={_selected_comune.nome_comune}");
#else
            Application.OpenURL($"http://maps.google.com/maps?saddr=My+Location&daddr={_selected_comune.nome_comune}");
#endif
            }
        }
    }

    //private void btnSearchCliccked()
    //{ }
    private void btnImHereCliccked()
    {
        if (GameObject.FindObjectOfType<FreeMap>())
            GameObject.FindObjectOfType<FreeMap>().OnBtnHere();
    }
}
