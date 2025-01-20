using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static DBClass;

public class ManageCanvasPOI : MonoBehaviour
{
    private ChangeScene _changeScene;
    [System.Serializable]
    public class ElementsData
    {
        public Canvas canvas;
        public Button Btnclose;
        public Button BtnGo;
        public Button BtnShowItinerario;
        public Button BtnGoToItinerario;
        public Button BtnHome;
        public Text ID_selected;
        public float alphaBackground = 0.8f;
        public Image image;
    }
    public ElementsData Elements;
    public GameObject PanelMap;
    public Text selected_tab;
    public GameObject _loaderCanvas;
    public Image _progressBar;
    public GameObject ARMenuCanvas;

    void Start()
    {
        ShowCanvasPOI();
        
        // Create a temporary reference to the current scene.
        Scene currentScene = SceneManager.GetActiveScene();
        // Retrieve the name of this scene.
        string sceneName = currentScene.name;
        if (sceneName == "Map")
            Elements.Btnclose.gameObject.SetActive(true);
        else if (sceneName == "Menu")
            Elements.Btnclose.gameObject.SetActive(false);

        var tempcolor = Elements.image.color;
        tempcolor.a = Elements.alphaBackground;
        Elements.image.color = tempcolor;
        if (string.IsNullOrEmpty(Elements.ID_selected.text))
            HideCanvasPOI();
        if (PlayerPrefs.GetString("show_on_freemap") == "1")
        {
            //DetailPOI.SetActive(false);
            HideCanvasPOI();
            PlayerPrefs.SetString("show_on_freemap", string.Empty);
        }
    }
    void Update()
    {
    }

    public void OnEnable()
    {
        if (Elements.Btnclose != null)
            Elements.Btnclose.onClick.AddListener(HideCanvasPOI);
        if (Elements.BtnGo != null)
            Elements.BtnGo.onClick.AddListener(portami_Qui);
        if (Elements.BtnGoToItinerario != null)
            Elements.BtnGoToItinerario.onClick.AddListener(Route_to_POI);
        if (Elements.BtnShowItinerario != null)
            Elements.BtnShowItinerario.onClick.AddListener(ShowItinerario);
        if (Elements.BtnHome != null)
            Elements.BtnHome.onClick.AddListener(GoToHome);
    }
    public void OnDisable()
    {
        if (Elements.Btnclose != null)
            Elements.Btnclose.onClick.RemoveListener(HideCanvasPOI);
        if (Elements.BtnGo != null)
            Elements.BtnGo.onClick.RemoveListener(portami_Qui);
        if (Elements.BtnGoToItinerario != null)
            Elements.BtnGoToItinerario.onClick.RemoveListener(Route_to_POI);
        if (Elements.BtnShowItinerario != null)
            Elements.BtnShowItinerario.onClick.RemoveListener(ShowItinerario);
        if (Elements.BtnHome != null)
            Elements.BtnHome.onClick.RemoveListener(GoToHome);
    }
    public void HideCanvasPOI()
    {
        PlayerPrefs.SetInt("percorso_selezionato", 0);
        PlayerPrefs.SetInt("evento_selezionato", 0);
        PlayerPrefs.SetInt("poi_selezionato", 0);
        PlayerPrefs.SetString("istat", "");
        selected_tab.text = "0";
        if (PlayerPrefs.GetInt("show_grid_poi") == 2)
            PlayerPrefs.SetInt("show_grid_poi", 1);
        if (GameObject.FindObjectOfType<ICanvas>() != null)
        {
            GameObject.FindObjectOfType<ICanvas>().search_filterPOI = GameObject.FindObjectOfType<DetailPOIManager>()._from_search_filter;
            GameObject.FindObjectOfType<ICanvas>().panel_all_White.SetActive(GameObject.FindObjectOfType<DetailPOIManager>()._from_search_filter);
        }
        // Create a temporary reference to the current scene.
        Scene currentScene = SceneManager.GetActiveScene();
        // Retrieve the name of this scene.
        string sceneName = currentScene.name;
        if (sceneName == "ARRoute")
        {
            if(ARMenuCanvas != null)
                ARMenuCanvas.GetComponent<ARLocation.MapboxRoutes.SampleProject.ArMenuController>().ResetSizeMinimap();
        }
        /*
        if (GameObject.FindObjectOfType<ARLocation.MapboxRoutes.SampleProject.ArMenuController>() != null)
        {
            GameObject.FindObjectOfType<ARLocation.MapboxRoutes.SampleProject.ArMenuController>().ResetSizeMinimap();
        }
        */
        Elements.canvas.enabled = false;
        Elements.canvas.gameObject.SetActive(false);
        if (PanelMap != null)
            PanelMap.SetActive(true);
    }
    public void ShowCanvasPOI()
    {
        Elements.canvas.enabled = true;
        if (PanelMap != null)
            PanelMap.SetActive(false);
    }

    public void portami_Qui()
    {
        if (!string.IsNullOrEmpty(Elements.ID_selected.text))
        {
            List<POI> _POI = GameObject.FindObjectOfType<DBClass>().getPOI(int.Parse(Elements.ID_selected.text));
            if (_POI != null && _POI.Count > 0)
            {
                POI _p = _POI[0];
#if UNITY_ANDROID
            Application.OpenURL($"google.navigation:q={_p.latitudine},{_p.longitudine}");
#elif UNITY_IOS
                Application.OpenURL($"http://maps.apple.com/maps?saddr=Current+Location&daddr={_p.latitudine},{_p.longitudine}");
#else
            Application.OpenURL($"http://maps.google.com/maps?saddr=My+Location&daddr={_p.latitudine},{_p.longitudine}");
#endif
            }
        }
    }
    public void ShowItinerario()
    {
        Debug.Log("ShowItinerario: " + Elements.ID_selected.text);
        List<PERCORSO> _PERCORSO = findPercorso();
        if (_PERCORSO != null && _PERCORSO.Count > 0)
        {
            Elements.ID_selected.text = _PERCORSO[0].id.ToString();
            GameObject.FindObjectOfType<DetailPOIManager>().OpenDetailAtID(Elements.ID_selected.text, false, true, 1);
        }
    }
    List<PERCORSO> findPercorso()
    {
        List<PERCORSO> _PERCORSO = null;
        if (!string.IsNullOrEmpty(Elements.ID_selected.text))
        {
            int _id_selected = int.Parse(Elements.ID_selected.text);
            if (PlayerPrefs.GetInt("percorso_selezionato") > 0)
                _PERCORSO = GameObject.FindObjectOfType<DBClass>().GetPERCORSO(_id_selected);
            //if (PlayerPrefs.GetInt("evento_selezionato")> 0)
            if (PlayerPrefs.GetInt("poi_selezionato") > 0)
            {
                _PERCORSO = GameObject.FindObjectOfType<DBClass>().GetPERCORSO(null, _id_selected);
                if (_PERCORSO == null || _PERCORSO.Count == 0)
                {
                    //cerco se il poi è parte di un percorso nella tabella POIXTAPPE e TAPPEXPERCORSO
                    var list_poixtappe = GameObject.FindObjectOfType<DBClass>().getPOIXTAPPE(null, _id_selected);
                    if (list_poixtappe != null && list_poixtappe.Count > 0)
                    {
                        var list_tappexpercorsi = GameObject.FindObjectOfType<DBClass>().getTAPPEXPERCORSI(null, list_poixtappe[0].tappa_id);
                        if (list_tappexpercorsi != null && list_tappexpercorsi.Count > 0)
                            _PERCORSO = GameObject.FindObjectOfType<DBClass>().GetPERCORSO(list_tappexpercorsi[0].percorso_id);
                    }
                }
            }
        }
        return _PERCORSO;
    }
    public void Route_to_POI()
    {
        if (PlayerPrefs.GetInt("poi_selezionato") > 0)
        {
            PlayerPrefs.SetInt("percorso_selezionato", PlayerPrefs.GetInt("poi_selezionato"));
            PlayerPrefs.SetInt("poi_selezionato", 0);
        }

        Debug.Log("Route_to_POI: " + Elements.ID_selected.text);
        List<PERCORSO> _PERCORSO = findPercorso();
        if (_PERCORSO != null && _PERCORSO.Count > 0 )
        {
            if (!string.IsNullOrEmpty(_PERCORSO[0].percorso))
            {
                Debug.Log("Route_to_POI: " + Elements.ID_selected.text);

                PERCORSO _p = _PERCORSO[0];
                _changeScene = new ChangeScene();
                _changeScene._loaderCanvas = _loaderCanvas;
                _changeScene._progressBar = _progressBar;
                _changeScene.Load_ARRoute(_p.id);
            }
        }
    }
    public void GoToHome()
    {
        _changeScene = new ChangeScene();
        _changeScene.Load_Menu();
    }
}
