using System.Linq;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class MyWebController : MonoBehaviour
{

    private string url = "https://www.google.it";
    private WebViewObject webViewObject;
    private RectTransform rectTransform;
    private DBClass _DBClass;
    [TextArea(5, 10)] // Ti permette di scrivere più righe nell'Inspector
    private string customCSS = "#wrapper { display: none !important; } #titolo { display: none !important; } #breadcrumb { display: none !important; } footer { display: none !important; }";


    // OnEnable viene chiamato ogni volta che il pannello (o il padre) diventa attivo
    void OnEnable()
    {
        _DBClass = GameObject.FindWithTag("SQLite").GetComponent<DBClass>();
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();

        // Se il WebView non esiste ancora, crealo
        if (webViewObject == null)
        {
            InitWebView();
        }
        else
        {
            // Se esiste già, rendilo semplicemente visibile
            webViewObject.SetVisibility(true);
            UpdateMargins();
        }
    }

    public void InjectCustomCSS()
    {
        if (webViewObject == null || string.IsNullOrEmpty(customCSS)) return;

        // Rimuove eventuali ritorni a capo per evitare errori nella stringa JS
        string cleanedCSS = customCSS.Replace("\n", "").Replace("\r", "");

        string js = $@"
        var style = document.createElement('style');
        style.type = 'text/css';
        style.innerHTML = '{cleanedCSS}';
        document.getElementsByTagName('head')[0].appendChild(style);
    ";

        webViewObject.EvaluateJS(js);
    }

    void InitWebView()
    {
        webViewObject = (new GameObject("WebView_Instance")).AddComponent<WebViewObject>();
        /*
        webViewObject.Init(
            ld: (msg) =>
            {
                // Mostra il webview solo se il pannello è ancora attivo nel momento in cui finisce di caricare
                if (this.gameObject.activeInHierarchy)
                    webViewObject.SetVisibility(true);
            }
        );*/
        webViewObject.Init(
            ld: (msg) =>
            {
                if (this.gameObject.activeInHierarchy)
                {
                    webViewObject.SetVisibility(true);

                    InjectCustomCSS(); // <-- Aggiunto qui
                }
            }
        );
        UpdateMargins();
        Debug.Log(PlayerPrefs.GetString("istat"));
        var _comune = _DBClass.GetCOMUNI(PlayerPrefs.GetString("istat"));
        if (_comune != null)
            url = _comune.FirstOrDefault().sito_turistico + "/eventi/";
        webViewObject.LoadURL(url);
    }



    // --- NAVIGAZIONE ---
    public void OnClickBack()
    {
        if (webViewObject != null) webViewObject.GoBack();
    }

    public void OnClickForward()
    {
        if (webViewObject != null) webViewObject.GoForward();
    }

    public void OnClickReload()
    {
        if (webViewObject != null) webViewObject.Reload();
    }

    // --- ZOOM & SCROLL ---
    public void SetZoom(float zoom)
    {
        if (webViewObject == null) return;
        string js = $@"document.body.style.zoom = '{zoom}'; 
                       document.body.style.webkitTransform = 'scale({zoom})';
                       document.body.style.webkitTransformOrigin = '0 0';";
        webViewObject.EvaluateJS(js);
    }

    public void ScrollToTop()
    {
        if (webViewObject != null) webViewObject.EvaluateJS("window.scrollTo(0, 0);");
    }

    // --- GESTIONE LAYOUT ---
    public void UpdateMargins()
    {
        if (webViewObject == null) return;

        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        // Calcolo margini basato sui bordi dello schermo
        int left = (int)corners[0].x;
        int bottom = (int)corners[0].y;
        int right = Screen.width - (int)corners[2].x;
        int top = Screen.height - (int)corners[2].y;

        webViewObject.SetMargins(left, top, right, bottom);
    }

    void Update()
    {
        // Aggiorna se il pannello viene spostato/ridimensionato
        if (transform.hasChanged && webViewObject != null)
        {
            UpdateMargins();
            transform.hasChanged = false;
        }

        // Tasto Back fisico di Android
        if (Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Escape))
        {
            OnClickBack();
        }
    }

    void OnDisable()
    {
        if (webViewObject != null)
        {
            webViewObject.SetVisibility(false);
            OnDestroy();
        }
    }

    void OnDestroy()
    {
        if (webViewObject != null) Destroy(webViewObject.gameObject);
    }

}
