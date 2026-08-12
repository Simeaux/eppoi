using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPrincipale : MonoBehaviour
{

    private DBClass _DBClass;
    private Canvas _Canvas_Prompt_Download;
    public Panel_Principale pp;
    public Font fontPoppins;
    [Header("Spinner API")]
    public GameObject spinnerPesoComune;
    public Text testoSpinnerPesoComune;




    [System.Serializable]
    public class DatiDownloadZipResponse
    {
        public bool success;
        public int n_poi;
        public int n_poi_immagini;
        public int n_percorsi;
        public string size;
    }

    private IEnumerator RecuperaDatiDownloadZip(
    string idSinp,
    string nomeComune,
    System.Action<bool> onCompleted)
    {
        MostraSpinnerPesoComune(nomeComune);
        string url =
            "https://www.macerataturismo.it/wp-json/rest_api_ws/v1/dati_download_zip?ID_SINP="
            + UnityWebRequest.EscapeURL(idSinp);

        Debug.Log("[API] Chiamata: " + url);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (request.result != UnityWebRequest.Result.Success)
#else
        if (request.isNetworkError || request.isHttpError)
#endif
            {
                Debug.LogError("[API] Errore: " + request.error);
                NascondiSpinnerPesoComune();
                onCompleted?.Invoke(false);
                yield break;
            }

            string json = request.downloadHandler.text;

            Debug.Log("[API] Risposta: " + json);

            DatiDownloadZipResponse dati = null;

            try
            {
                dati = JsonUtility.FromJson<DatiDownloadZipResponse>(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[API] Errore parsing JSON: " + e.Message);

                onCompleted?.Invoke(false);
                yield break;
            }

            if (dati == null)
            {
                Debug.LogError("[API] Risposta JSON non valida.");

                onCompleted?.Invoke(false);
                yield break;
            }

            if (!dati.success)
            {
                Debug.LogError("[API] success=false");
                NascondiSpinnerPesoComune();
                onCompleted?.Invoke(false);
                yield break;
            }

            // =====================================================
            // RECUPERIAMO IL PANEL
            // =====================================================

            if (pp == null)
            {
                pp = GameObject.FindFirstObjectByType<Panel_Principale>();
            }

            if (pp == null)
            {
                Debug.LogError(
                    "[API] Impossibile trovare Panel_Principale."
                );

                onCompleted?.Invoke(false);
                yield break;
            }

            // =====================================================
            // RECUPERIAMO IL CANVAS
            // =====================================================

            Transform canvasTransform =
                pp.gameObject.transform.Find("Canvas_Prompt_Download");

            if (canvasTransform == null)
            {
                Debug.LogError(
                    "[API] Canvas_Prompt_Download non trovato."
                );

                onCompleted?.Invoke(false);
                yield break;
            }

            _Canvas_Prompt_Download =
                canvasTransform.GetComponent<Canvas>();

            if (_Canvas_Prompt_Download == null)
            {
                Debug.LogError(
                    "[API] Canvas_Prompt_Download non contiene un componente Canvas."
                );

                onCompleted?.Invoke(false);
                yield break;
            }

            // =====================================================
            // AGGIORNIAMO I TEXT
            // =====================================================

            Text[] tuttiTesti =
                _Canvas_Prompt_Download.GetComponentsInChildren<Text>(true);

            foreach (Text t in tuttiTesti)
            {
                switch (t.name)
                {
                    case "Testo_Info_Download":
                        t.text = "Scarica " + nomeComune;
                        break;

                    case "poi":
                        t.text = dati.n_poi.ToString() + " audioguide";
                        break;

                    case "poi_immagini":

                        t.text = dati.n_poi_immagini.ToString() + " immagini e schede";

                        break;

                    case "percorsi":

                        t.text = dati.n_percorsi.ToString() + " mappe e itinerari offline";

                        break;

                    case "peso":

                        t.text = "Totale " + dati.size + " MB";

                        break;
                }
            }

            Debug.Log(
                "[API] Dati aggiornati - " +
                "POI: " + dati.n_poi +
                ", Immagini: " + dati.n_poi_immagini +
                ", Percorsi: " + dati.n_percorsi +
                ", Peso: " + dati.size + " MB"
            );
            NascondiSpinnerPesoComune();
            // =====================================================
            // API COMPLETATA CORRETTAMENTE
            // =====================================================

            onCompleted?.Invoke(true);
        }
    }


    private void MostraSpinnerPesoComune(string nomeComune)
    {
        if (spinnerPesoComune == null)
        {
            Debug.LogWarning(
                "[API] SpinnerPesoComune non assegnato."
            );

            return;
        }

        spinnerPesoComune.SetActive(true);

        if (testoSpinnerPesoComune != null)
        {
            testoSpinnerPesoComune.text =
                "Acquisizione informazioni di " +
                nomeComune +
                "...";
        }

        Debug.Log(
            "[API] Spinner mostrato per il comune: " +
            nomeComune
        );
    }

    private void NascondiSpinnerPesoComune()
    {
        if (spinnerPesoComune != null)
        {
            spinnerPesoComune.SetActive(false);
        }

        Debug.Log(
            "[API] Spinner nascosto."
        );
    }
    public void ButtonCliccked(Button button)
    {
        StartCoroutine(ButtonClicckedCoroutine(button));
    }

    private IEnumerator ButtonClicckedCoroutine(Button button)
    {
        // =====================================================
        // RECUPERO ISTAT
        // =====================================================

        Text istatText = button.transform.Find("Istat")?.GetComponent<Text>();

        if (istatText == null)
        {
            Debug.LogError(
                "[MENU] Oggetto Text 'Istat' non trovato."
            );

            yield break;
        }

        string Istat = istatText.text;

        // =====================================================
        // DATABASE
        // =====================================================

        _DBClass =
            GameObject.FindWithTag("SQLite")
            .GetComponent<DBClass>();

        List<DBClass.COMUNE> c =
            _DBClass.GetCOMUNI(
                Istat,
                null,
                null,
                null,
                null
            );

        if (c == null || c.Count == 0)
        {
            Debug.LogError(
                "[MENU] Comune non trovato per ISTAT: " + Istat
            );

            yield break;
        }

        DBClass.COMUNE comune = c[0];

        Debug.Log(
            "[MENU] Comune selezionato: " +
            comune.nome_comune +
            " - ID: " +
            comune.id
        );
        // =====================================================
        // 0. VERIFICA INIZIALE: IL COMUNE HA POI GIÀ PRESENTI?
        // =====================================================

        int poiCount =
            _DBClass.getPOI_Count(
                null,
                null,
                comune.nome_comune,
                null,
                null
            );

        if (poiCount > 0)
        {
            PlayerPrefs.SetString(
                "istat",
                Istat
            );

            PlayerPrefs.SetString(
                "poi_selezionato",
                ""
            );

            PlayerPrefs.SetString(
                "percorso_selezionato",
                ""
            );
        }

        // =====================================================
        // 1. CHIAMATA API
        // =====================================================

        bool apiCompletata = false;
        bool apiSuccess = false;

        yield return StartCoroutine(
            RecuperaDatiDownloadZip(
                comune.id.ToString(),
                comune.nome_comune,
                (success) =>
                {
                    apiSuccess = success;
                    apiCompletata = true;
                }
            )
        );

        // Sicurezza
        if (!apiCompletata || !apiSuccess)
        {
            Debug.LogError(
                "[MENU] Impossibile recuperare i dati del download."
            );

            yield break;
        }

        // =====================================================
        // 2. API COMPLETATA
        //    ADESSO MOSTRIAMO IL CANVAS
        // =====================================================

        if (_Canvas_Prompt_Download == null)
        {
            Debug.LogError(
                "[MENU] Canvas_Prompt_Download non disponibile."
            );

            yield break;
        }

        _Canvas_Prompt_Download.gameObject.SetActive(true);

        Debug.Log(
            "[MENU] Canvas_Prompt_Download mostrato dopo la risposta API."
        );

        // =====================================================
        // 3. CONTROLLO POI GIÀ PRESENTI
        // =====================================================

        int _TotalRowToExtract =
            _DBClass.getPOI_Count(
                null,
                null,
                comune.nome_comune,
                null,
                null
            );

        if (_TotalRowToExtract > 0)
        {
            // Il comune è già presente.
            // Non serve il download.

            PlayerPrefs.SetString(
                "istat",
                Istat
            );

            PlayerPrefs.SetString(
                "poi_selezionato",
                ""
            );

            PlayerPrefs.SetString(
                "percorso_selezionato",
                "");

            _Canvas_Prompt_Download.gameObject.SetActive(false);

            Debug.Log(
                "[MENU] Il comune è già presente nel DB."
            );

            yield break;
        }

        // =====================================================
        // 4. IL COMUNE NON HA POI
        //    ASPETTIAMO LA RISPOSTA DELL'UTENTE
        // =====================================================

        PromptDownloadController promptScript =
            _Canvas_Prompt_Download
            .GetComponent<PromptDownloadController>();

        if (promptScript == null)
        {
            Debug.LogError(
                "[MENU] PromptDownloadController non trovato."
            );

            yield break;
        }

        // Reset del prompt
        promptScript.InizializzaPrompt();

        // Aspettiamo che l'utente prema SI o NO
        yield return new WaitUntil(
            () => promptScript.RispostaRicevuta
        );

        // =====================================================
        // 5. RISPOSTA UTENTE
        // =====================================================

        if (promptScript.RisultatoScelta == false)
        {
            Debug.Log(
                "[MENU] Download annullato dall'utente."
            );

            _Canvas_Prompt_Download.gameObject.SetActive(false);

            yield break;
        }

        // =====================================================
        // 6. UTENTE HA PREMUTO SI
        // =====================================================

        Debug.Log(
            "[MENU] Download confermato dall'utente."
        );

        List<int> idSinp =
            new List<int>
            {
            comune.id
            };

        yield return StartCoroutine(
            pp.StartSync(
                idSinp,
                button
            )
        );

        Debug.Log(
            "[MENU] Download completato con successo!"
        );

        pp.EndSync(button);

        _Canvas_Prompt_Download.gameObject.SetActive(false);

        // =====================================================
        // 7. VERIFICA FINALE
        // =====================================================

        poiCount =
            _DBClass.getPOI_Count(
                null,
                null,
                comune.nome_comune,
                null,
                null
            );

        if (poiCount > 0)
        {
            PlayerPrefs.SetString(
                "istat",
                Istat
            );

            PlayerPrefs.SetString(
                "poi_selezionato",
                ""
            );

            PlayerPrefs.SetString(
                "percorso_selezionato",
                ""
            );
        }
    }

    // Avvia il prompt, attende la risposta dell'utente, esegue il download e apre il comune.
    private IEnumerator DownloadEApriComune(DBClass.COMUNE comune, Button button)
    {

        _Canvas_Prompt_Download = pp.gameObject.transform.Find("Canvas_Prompt_Download").GetComponent<Canvas>();
        // 1. Mostriamo il Canvas di Prompt
        if (_Canvas_Prompt_Download != null)
        {
            _Canvas_Prompt_Download.gameObject.SetActive(true);

            // Otteniamo il componente dello script associato al prompt per leggerne il risultato
            PromptDownloadController promptScript = _Canvas_Prompt_Download.GetComponent<PromptDownloadController>();
            if (promptScript != null)
            {
                // Reset dello stato iniziale prima di attendere
                promptScript.InizializzaPrompt();

                // Blocco yield: la coroutine si ferma qui FINCHÉ l'utente non clicca uno dei due bottoni
                yield return new WaitUntil(() => promptScript.RispostaRicevuta == true);

                // Verifichiamo il risultato scelto dall'utente
                if (promptScript.RisultatoScelta == false)
                {
                    Debug.Log("Download annullato dall'utente.");
                    _Canvas_Prompt_Download.gameObject.SetActive(false); // Chiudiamo il prompt
                    yield break; // Interrompe definitivamente la coroutine senza scaricare nulla
                }
            }
        }

        // 2. Se l'utente ha premuto SI (oppure se il Canvas non era presente), procediamo con il download
        var Istat = button.transform.Find("Istat").GetComponent<Text>().text;
        List<int> idSinp = new List<int> { comune.id };
        yield return StartCoroutine(pp.StartSync(idSinp, button));

        Debug.Log("Download completato con successo!");
        pp.EndSync(button);
        _Canvas_Prompt_Download.gameObject.SetActive(false); // Chiudiamo il prompt
        int poiCount = _DBClass.getPOI_Count(null, null, comune.nome_comune, null, null);
        if (poiCount > 0)
        {
            PlayerPrefs.SetString("istat", Istat);
            PlayerPrefs.SetString("poi_selezionato", "");
            PlayerPrefs.SetString("percorso_selezionato", "");
        }
    }


    public void ButtonClicckedComune(Text text)
    {
        if (text != null && text.text.Contains(";"))
        {
            var listText = text.text.Split(';');
            if (listText != null)
            {
                // Create a temporary reference to the current scene.
                Scene currentScene = SceneManager.GetActiveScene();
                // Retrieve the name of this scene.
                string sceneName = currentScene.name;
                int tipo = int.Parse(listText[0]);


                long id = long.Parse(listText[1]);
                if (tipo == 1 && !(PlayerPrefs.GetString("poi_selezionato") != ""))
                {
                    var _percorso = GameObject.FindObjectOfType<DBClass>().GetPERCORSO(id);
                    if (_percorso != null)
                    {
                        var _poi = GameObject.FindObjectOfType<DBClass>().getPOI(_percorso[0].poi_id);
                        if (_poi != null && _poi.Count > 0)
                        {

                            var _comune = GameObject.FindObjectOfType<DBClass>().GetCOMUNI(null, null, _poi[0].comune_id);
                            if (_comune != null && _comune.Count > 0)
                                PlayerPrefs.SetString("istat", _comune[0].istat);
                        }
                    }

                    PlayerPrefs.SetString("percorso_selezionato", id.ToString());
                    if (sceneName == "Map")
                        PlayerPrefs.SetInt("show_grid_poi", 2);
                }
                else if (tipo == 1 && (PlayerPrefs.GetString("poi_selezionato") != ""))
                {
                    PlayerPrefs.SetString("percorso_selezionato_collegato_ad_un_poi", id.ToString());
                    if (sceneName == "Map")
                        PlayerPrefs.SetInt("show_grid_poi", 2);
                }

                if (tipo == 2)
                {
                    PlayerPrefs.SetString("evento_selezionato", id.ToString());
                    if (sceneName == "Map")
                        PlayerPrefs.SetInt("show_grid_poi", 2);
                }
                if (tipo == 3 && !(PlayerPrefs.GetString("percorso_selezionato") != ""))
                {
                    var _comune = GameObject.FindObjectOfType<DBClass>().GetCOMUNI(null, null, GameObject.FindObjectOfType<DBClass>().getPOI(id)?[0]?.comune_id);
                    if (_comune != null && _comune.Count > 0)
                        PlayerPrefs.SetString("istat", _comune[0].istat);
                    PlayerPrefs.SetString("poi_selezionato", long.Parse(listText[1]).ToString());
                    if (sceneName == "Map")
                        PlayerPrefs.SetInt("show_grid_poi", 2);
                }
                else if (tipo == 3 && (PlayerPrefs.GetString("percorso_selezionato") != ""))
                {
                    PlayerPrefs.SetString("poi_selezionato_collegato_ad_un_percorso", listText[1]);
                    if (sceneName == "Map")
                        PlayerPrefs.SetInt("show_grid_poi", 2);
                }

            }
        }
        else
        {
            Application.OpenURL(text.text);
        }
    }
    public void ButtonClicckedPOI(Text ID)
    {
        PlayerPrefs.SetInt("show_grid_poi", 2);
        PlayerPrefs.SetString("poi_selezionato", ID.text);
    }


    public void ButtonCancellaComune(Text id)
    {
        Debug.Log("-----------------------------------------------------");
        Debug.Log("Cancella comune - id: " + id.text);
        Debug.Log("-----------------------------------------------------");

        // Creiamo il Canvas del messaggio
        GameObject canvasObject = new GameObject("CanvasConfermaCancellazione");

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);

        canvasObject.AddComponent<GraphicRaycaster>();

        // ---------------------------------------------------------
        // SFONDO
        // ---------------------------------------------------------

        GameObject pannello = new GameObject("Pannello");
        pannello.transform.SetParent(canvasObject.transform, false);

        Image pannelloImage = pannello.AddComponent<Image>();
        pannelloImage.color = new Color(0f, 0f, 0f, 0.75f);

        RectTransform pannelloRect =
            pannello.GetComponent<RectTransform>();

        pannelloRect.anchorMin = Vector2.zero;
        pannelloRect.anchorMax = Vector2.one;
        pannelloRect.offsetMin = Vector2.zero;
        pannelloRect.offsetMax = Vector2.zero;

        // ---------------------------------------------------------
        // BOX CENTRALE
        // ---------------------------------------------------------

        GameObject box = new GameObject("BoxConferma");
        box.transform.SetParent(pannello.transform, false);

        Image boxImage = box.AddComponent<Image>();
        boxImage.color = Color.white;

        RectTransform boxRect =
            box.GetComponent<RectTransform>();

        boxRect.anchorMin = new Vector2(0.1f, 0.35f);
        boxRect.anchorMax = new Vector2(0.9f, 0.65f);
        boxRect.offsetMin = Vector2.zero;
        boxRect.offsetMax = Vector2.zero;

        // ---------------------------------------------------------
        // TESTO
        // ---------------------------------------------------------

        GameObject testoObject = new GameObject("TestoConferma");
        testoObject.transform.SetParent(box.transform, false);

        Text testo = testoObject.AddComponent<Text>();

        testo.text =
            "Sei sicuro di voler cancellare questo comune?";

        testo.alignment = TextAnchor.MiddleCenter;
        testo.font = fontPoppins;
        testo.fontSize = 36;
        testo.color = Color.black;

        RectTransform testoRect =
            testo.GetComponent<RectTransform>();

        testoRect.anchorMin = new Vector2(0.05f, 0.45f);
        testoRect.anchorMax = new Vector2(0.95f, 0.95f);
        testoRect.offsetMin = Vector2.zero;
        testoRect.offsetMax = Vector2.zero;

        // ---------------------------------------------------------
        // BOTTONE ANNULLA
        // ---------------------------------------------------------

        GameObject bottoneAnnulla =
            CreaBottone(
                box.transform,
                "ANNULLA",
                new Vector2(0.05f, 0.05f),
                new Vector2(0.45f, 0.35f)
            );

        bottoneAnnulla
            .GetComponent<Button>()
            .onClick.AddListener(() =>
            {
                Destroy(canvasObject);
            });

        // ---------------------------------------------------------
        // BOTTONE CONFERMA
        // ---------------------------------------------------------

        GameObject bottoneConferma =
            CreaBottone(
                box.transform,
                "CONFERMA",
                new Vector2(0.55f, 0.05f),
                new Vector2(0.95f, 0.35f)
            );

        bottoneConferma
            .GetComponent<Button>()
            .onClick.AddListener(() =>
            {
                Debug.Log(
                    "Cancellazione confermata. ID: " +
                    id.text
                );
                _DBClass = GameObject.FindWithTag("SQLite").GetComponent<DBClass>();
                _DBClass.DeleteComuneById(int.Parse(id.text));
                // Aggiorna la lista dei comuni

                CanvasStatistiche statistiche =
                    FindFirstObjectByType<CanvasStatistiche>();

                if (statistiche != null)
                {
                    statistiche.AggiornaListaComuni();
                    statistiche.AggiornaPesoApp();
                }
                else
                {
                    Debug.LogWarning(
                        "[MENU] CanvasStatistiche non trovato."
                    );
                }
                Destroy(canvasObject);
            });
    }

    private GameObject CreaBottone(
    Transform parent,
    string testoBottone,
    Vector2 anchorMin,
    Vector2 anchorMax)
    {
        GameObject bottone =
            new GameObject(testoBottone);

        bottone.transform.SetParent(
            parent,
            false
        );

        Image image =
            bottone.AddComponent<Image>();

        image.color =
            new Color(
                0.85f,
                0.85f,
                0.85f
            );

        Button button =
            bottone.AddComponent<Button>();

        RectTransform rect =
            bottone.GetComponent<RectTransform>();

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // ---------------------------------------------------------
        // TESTO BOTTONE
        // ---------------------------------------------------------

        GameObject testoObject =
            new GameObject("Text");

        testoObject.transform.SetParent(
            bottone.transform,
            false
        );

        Text testo =
            testoObject.AddComponent<Text>();

        testo.text =
            testoBottone;

        testo.font = fontPoppins;

        testo.fontSize = 28;
        testo.alignment =
            TextAnchor.MiddleCenter;

        testo.color =
            Color.black;

        RectTransform testoRect =
            testo.GetComponent<RectTransform>();

        testoRect.anchorMin = Vector2.zero;
        testoRect.anchorMax = Vector2.one;
        testoRect.offsetMin = Vector2.zero;
        testoRect.offsetMax = Vector2.zero;

        return bottone;
    }
}
