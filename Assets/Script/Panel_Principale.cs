using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.XR;
using static DBClass;
using System.Collections.Concurrent;
using System.IO.Compression;


public class Panel_Principale : MonoBehaviour
{



    [Serializable]
    public class SyncZipResponse
    {
        public bool success;
        public string file;
        public string url;
        public long size;
    }


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

    public Slider downloadSlider;
    public Text downloadText;

    public Slider dbSlider;
    public Text dbText;

    // Un solo HttpClient riutilizzato per tutta l'app (evita l'esaurimento dei socket)
    private static readonly HttpClient _httpClient = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(120)
    };

    private GameObject _btnComune;
    private int _lingua_selezionata = 1;
    private string _istat = "";
    private string queryString = "";
    private DBClass _DBClass;
    private List<GameObject> POIGo = new List<GameObject>();
    public GameObject _logo;
    private bool _rotate = false;

    private Button _currentButton;
    private GameObject _currentDownloadIcon;

    // Start is called before the first frame update
    private void Start()
    {
        _lingua_selezionata =
            PlayerPrefs.GetInt("lingua_selezionata");

        _istat = "";

        GameObject sqliteObject =
            GameObject.FindWithTag("SQLite");

        if (sqliteObject == null)
        {
            Debug.LogError("[START] Oggetto SQLite non trovato.");
            return;
        }

        _DBClass =
            sqliteObject.GetComponent<DBClass>();

        if (_DBClass == null)
        {
            Debug.LogError("[START] DBClass non trovato.");
            return;
        }

        _btnComune =
            (GameObject)Resources.Load("Button_comune");

        txtLuoghi.text =
            _lingua_selezionata == 1
                ? "Luoghi"
                : "Places";

        txtItinerari.text =
            _lingua_selezionata == 1
                ? "Itinerari"
                : "Itineraries";

        txtPuntiDiInteresse.text =
            _lingua_selezionata == 1
                ? "Punti di interesse"
                : "Points of interest";


        // ==========================================================
        // COSTRUZIONE ID SINP
        // ==========================================================

        List<int> _id_sinp =
            new List<int>();

        _id_sinp.Add(0);

        List<COMUNE> comuni =
            _DBClass.GetCOMUNI(
                null,
                null,
                null,
                null,
                null,
                true
            );

        if (comuni != null)
        {
            foreach (COMUNE c in comuni)
            {
                if (c != null && c.id > 0)
                    _id_sinp.Add(c.id);
            }
        }


        // ==========================================================
        // IMPORTANTE
        //
        // Lasciamo terminare completamente la fase iniziale
        // prima di partire con la sincronizzazione.
        // ==========================================================

        if (_id_sinp.Count > 1)
        {
            StartCoroutine(StartSyncDelayed(
                _id_sinp,
                null
            ));
        }


        // ==========================================================
        // APERTURA POI
        // ==========================================================

        if (PlayerPrefs.GetString(
                "apri_direttamente_il_poi_selezionato") != "")
        {
            _panel_principale.SetActive(false);

            PlayerPrefs.SetString(
                "poi_selezionato",
                PlayerPrefs.GetString(
                    "apri_direttamente_il_poi_selezionato"
                )
            );

            _panel_poi.SetActive(true);
        }


        // ==========================================================
        // APERTURA COMUNE
        // ==========================================================

        if (PlayerPrefs.HasKey("comune_selected") &&
            PlayerPrefs.GetInt("comune_selected") > 0)
        {
            var _comune =
                _DBClass.GetCOMUNI(
                    null,
                    null,
                    PlayerPrefs.GetInt("comune_selected")
                );

            if (_comune != null &&
                _comune.Count > 0)
            {
                PlayerPrefs.SetString(
                    "istat",
                    _comune.FirstOrDefault().istat
                );

                _panel_principale.SetActive(false);
                _panel_comune.SetActive(true);
            }
        }
    }

    private IEnumerator StartSyncDelayed(
        List<int> id,
        Button button)
    {
        // Lascia terminare Start(), OnGUI e la costruzione iniziale
        // prima di provare ad acquisire il lock SQLite.
        yield return null;

        yield return null;

        yield return StartCoroutine(
            StartSync(id, button)
        );
    }
    private bool _syncRunning = false;

    public IEnumerator StartSync(List<int> id, Button button)
    {
        if (_syncRunning)
        {
            Debug.LogWarning("[SYNC] Sincronizzazione già in corso.");
            yield break;
        }

        yield return StartCoroutine(
            SyncRoutineInternal(id, button)
        );
        // if (GameObject.FindObjectOfType<ItinerariEventiPOI_AttaccatiAlComune>() != null)
        //     StartCoroutine(GameObject.FindObjectOfType<ItinerariEventiPOI_AttaccatiAlComune>().CaricaEventiComune());
    }


    private IEnumerator SyncRoutineInternal(
        List<int> _id_sinp = null,
        Button button = null)
    {
        if (_id_sinp != null &&
            _id_sinp.Count == 1 &&
            _id_sinp[0] == 0)
        {
            Debug.LogWarning(
                "[SYNC] ID_SINP contiene solamente 0."
            );

            _syncRunning = false;

            yield break;
        }

        _rotate = true;

        Debug.Log("[SYNC] INIT");


        bool singleComune =
            _id_sinp != null &&
            _id_sinp.Count == 1 &&
            _id_sinp[0] != 0;

        if (singleComune)
        {
            downloadSlider.gameObject.SetActive(true);
            dbSlider.gameObject.SetActive(true);

            downloadSlider.minValue = 0f;
            downloadSlider.maxValue = 1f;
            downloadSlider.value = 0f;

            dbSlider.minValue = 0f;
            dbSlider.maxValue = 1f;
            dbSlider.value = 0f;

            downloadText.text =
                "Preparazione download...";

            dbText.text =
                "Preparazione database...";
        }

        string url =
            "https://www.macerataturismo.it/wp-json/rest_api_ws/v1/aggiorna_app_eppoi"
            + "?VERSIONE=" +
            _DBClass.getLastUpdatedFromTable("VERSIONE")
            + "&POI=" +
            _DBClass.getLastUpdatedFromTable("POI")
            + "&COMUNI=" +
            _DBClass.getLastUpdatedFromTable("COMUNI_TEXT")
            + "&PERCORSI=" +
            _DBClass.getLastUpdatedFromTable("PERCORSI");

        if (_id_sinp != null &&
            _id_sinp.Count > 0)
        {
            url +=
                "&ID_SINP=" +
                string.Join(",", _id_sinp);
        }

        url +=
            "&t=" +
            DateTime.UtcNow.Ticks;

        Debug.Log(
            "[SYNC URL] " +
            url);

        string sqlFilePath = null;
        string zipPath = null;

        // ==========================================================
        // CASO 1
        // SINGOLO COMUNE -> JSON -> ZIP -> SQL
        // ==========================================================

        if (singleComune)
        {
            downloadText.text =
                "Richiesta aggiornamento...";

            zipPath =
                Path.Combine(
                    Application.persistentDataPath,
                    "sync_" +
                    DateTime.UtcNow.Ticks +
                    ".zip");

            string zipUrl = null;

            // ======================================================
            // RICHIESTA API
            // ======================================================

            using (UnityWebRequest apiRequest =
                   UnityWebRequest.Get(url))
            {
                apiRequest.timeout = 60;

                yield return apiRequest.SendWebRequest();

#if UNITY_2020_1_OR_NEWER

                if (apiRequest.result !=
                    UnityWebRequest.Result.Success)

#else

            if (apiRequest.isNetworkError ||
                apiRequest.isHttpError)

#endif
                {
                    Debug.LogError(
                        "[SYNC API ERROR] " +
                        apiRequest.error);

                    downloadText.text =
                        "Errore richiesta aggiornamento";

                    EndSync(button);

                    yield break;
                }

                string json =
                    apiRequest.downloadHandler.text;

                Debug.Log(
                    "[SYNC API RESPONSE] " +
                    json);

                SyncZipResponse response =
                    JsonUtility.FromJson<SyncZipResponse>(
                        json);

                if (response == null)
                {
                    Debug.LogError(
                        "[SYNC API] JSON nullo.");

                    downloadText.text =
                        "Risposta server non valida";

                    EndSync(button);

                    yield break;
                }

                if (!response.success)
                {
                    Debug.LogError(
                        "[SYNC API] success=false");

                    downloadText.text =
                        "Aggiornamento non disponibile";

                    EndSync(button);

                    yield break;
                }

                if (string.IsNullOrWhiteSpace(
                    response.url))
                {
                    Debug.LogError(
                        "[SYNC API] URL ZIP mancante.");

                    downloadText.text =
                        "URL database mancante";

                    EndSync(button);

                    yield break;
                }

                zipUrl =
                    response.url;

                Debug.Log(
                    "[SYNC ZIP URL] " +
                    zipUrl);

                if (response.size > 0)
                {
                    Debug.Log(
                        "[SYNC SERVER ZIP SIZE] " +
                        FormatBytes(response.size));
                }
            }

            // ======================================================
            // DOWNLOAD ZIP
            // ======================================================

            downloadText.text =
                "Download database...";

            bool downloadSuccess = false;

            yield return StartCoroutine(
                DownloadZipCoroutine(
                    zipUrl,
                    zipPath,
                    downloadSlider,
                    downloadText,
                    result =>
                    {
                        downloadSuccess = result;
                    }));

            if (!downloadSuccess)
            {
                Debug.LogError(
                    "[SYNC] Download ZIP fallito.");

                EndSync(button);

                yield break;
            }

            if (!File.Exists(zipPath))
            {
                Debug.LogError(
                    "[SYNC] ZIP non trovato.");

                EndSync(button);

                yield break;
            }

            FileInfo zipInfo =
                new FileInfo(zipPath);

            long zipSize =
                zipInfo.Length;

            Debug.Log(
                "[SYNC] ZIP ricevuto: " +
                FormatBytes(zipSize));

            if (zipSize <= 0)
            {
                Debug.LogError(
                    "[SYNC] ZIP vuoto.");

                DeleteFileSafely(
                    zipPath,
                    "[ZIP DELETE]");

                EndSync(button);

                yield break;
            }

            // ======================================================
            // ESTRAZIONE ZIP
            // ======================================================

            downloadText.text =
                "Estrazione database...";

            bool extractionCompleted = false;

            Exception extractionException = null;

            string extractedSqlPath = null;

            string extractionRoot =
                Application.temporaryCachePath;

            Task.Run(() =>
            {
                try
                {
                    extractedSqlPath =
                        ExtractSqlFileFromZip(
                            zipPath,
                            extractionRoot);
                }
                catch (Exception ex)
                {
                    extractionException = ex;
                }

                extractionCompleted = true;
            });

            while (!extractionCompleted)
            {
                yield return null;
            }

            if (extractionException != null)
            {
                Debug.LogError(
                    "[ZIP EXTRACTION ERROR] " +
                    extractionException);

                Debug.LogException(
                    extractionException);

                downloadText.text =
                    "Errore estrazione database";

                DeleteFileSafely(
                    zipPath,
                    "[ZIP DELETE]");

                EndSync(button);

                yield break;
            }

            sqlFilePath =
                extractedSqlPath;

            if (string.IsNullOrWhiteSpace(
                sqlFilePath))
            {
                Debug.LogError(
                    "[SYNC] Percorso SQL vuoto.");

                DeleteFileSafely(
                    zipPath,
                    "[ZIP DELETE]");

                EndSync(button);

                yield break;
            }

            if (!File.Exists(sqlFilePath))
            {
                Debug.LogError(
                    "[SYNC] SQL estratto non trovato: " +
                    sqlFilePath);

                DeleteFileSafely(
                    zipPath,
                    "[ZIP DELETE]");

                EndSync(button);

                yield break;
            }

            FileInfo sqlInfo =
                new FileInfo(sqlFilePath);

            Debug.Log(
                "[SYNC] SQL estratto: " +
                FormatBytes(sqlInfo.Length));

            DeleteFileSafely(
                zipPath,
                "[ZIP DELETE]");

            zipPath = null;
        }
        else
        {
            // ==========================================================
            // CASO 2
            // SERVER -> SQL DIRETTO
            // ==========================================================

            Debug.Log(
                "[SYNC] DOWNLOAD SQL DIRETTO");

            string sqlTempPath =
                Path.Combine(
                    Application.temporaryCachePath,
                    "sync_" +
                    DateTime.UtcNow.Ticks +
                    ".sql");

            using (UnityWebRequest req =
                   UnityWebRequest.Get(url))
            {
                req.timeout = 600;

                yield return req.SendWebRequest();

#if UNITY_2020_1_OR_NEWER

                if (req.result !=
                    UnityWebRequest.Result.Success)

#else

            if (req.isNetworkError ||
                req.isHttpError)

#endif
                {
                    Debug.LogError(
                        "[SYNC SQL DOWNLOAD ERROR] " +
                        req.error);

                    EndSync(button);

                    yield break;
                }

                if (req.downloadHandler == null)
                {
                    Debug.LogError(
                        "[SYNC] DownloadHandler SQL nullo.");

                    EndSync(button);

                    yield break;
                }

                File.WriteAllText(
                    sqlTempPath,
                    req.downloadHandler.text,
                    Encoding.UTF8);

                sqlFilePath =
                    sqlTempPath;
            }

            if (string.IsNullOrWhiteSpace(
                sqlFilePath) ||
                !File.Exists(sqlFilePath))
            {
                Debug.LogError(
                    "[SYNC] FILE SQL NON CREATO");

                EndSync(button);

                yield break;
            }
        }

        // ==========================================================
        // CONTROLLO FILE SQL
        // ==========================================================

        if (string.IsNullOrWhiteSpace(
            sqlFilePath))
        {
            Debug.LogError(
                "[SYNC] Percorso SQL nullo.");

            EndSync(button);

            yield break;
        }

        if (!File.Exists(sqlFilePath))
        {
            Debug.LogError(
                "[SYNC] File SQL non trovato: " +
                sqlFilePath);

            EndSync(button);

            yield break;
        }

        // ==========================================================
        // APPLICAZIONE SQL
        // ==========================================================

        dbText.text =
            "Applicazione database...";

        dbSlider.value =
            0f;

        bool sqlCompleted = false;

        Exception sqlException = null;

        try
        {
            ExecuteSqlFileStreaming(
                sqlFilePath,
                progress =>
                {
                    if (singleComune)
                    {
                        dbSlider.value =
                            Mathf.Clamp01(progress);
                    }
                });

            sqlCompleted = true;
        }
        catch (Exception ex)
        {
            sqlException = ex;
        }

        if (!sqlCompleted ||
            sqlException != null)
        {
            Debug.LogError(
                "[SYNC] ERRORE APPLICAZIONE SQL");

            if (sqlException != null)
            {
                Debug.LogException(
                    sqlException);
            }

            dbText.text =
                "Errore aggiornamento database";

            DeleteFileSafely(
                sqlFilePath,
                "[SQL DELETE]");

            EndSync(button);

            yield break;
        }

        // ==========================================================
        // AGGIORNAMENTO COMPLETATO
        // ==========================================================

        DeleteFileSafely(
            sqlFilePath,
            "[SQL DELETE]");

        sqlFilePath = null;

        if (singleComune)
        {
            dbSlider.value =
                1f;

            downloadSlider.value =
                1f;

            dbText.text =
                "Database aggiornato";

            downloadText.text =
                "Aggiornamento completato";
        }

        Debug.Log(
            "[SYNC] COMPLETE");

        EndSync(button);
    }

    private IEnumerator DownloadZipCoroutine(
     string url,
     string destinationFile,
     Slider slider,
     Text txt,
     Action<bool> completedCallback)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            Debug.LogError(
                "[ZIP DOWNLOAD] URL vuoto.");

            txt.text =
                "URL download non valido";

            completedCallback?.Invoke(false);

            yield break;
        }

        if (string.IsNullOrWhiteSpace(
            destinationFile))
        {
            Debug.LogError(
                "[ZIP DOWNLOAD] Destinazione vuota.");

            txt.text =
                "Percorso download non valido";

            completedCallback?.Invoke(false);

            yield break;
        }

        string directory =
            Path.GetDirectoryName(
                destinationFile);

        if (!string.IsNullOrWhiteSpace(directory) &&
            !Directory.Exists(directory))
        {
            try
            {
                Directory.CreateDirectory(
                    directory);
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    "[ZIP DOWNLOAD] Errore creazione directory: " +
                    ex);

                txt.text =
                    "Errore spazio temporaneo";

                completedCallback?.Invoke(false);

                yield break;
            }
        }

        DeleteFileSafely(
            destinationFile,
            "[ZIP OLD FILE DELETE]");

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;

        txt.text =
            "Connessione al server...";

        using (UnityWebRequest req =
               UnityWebRequest.Get(url))
        {
            // Nessun timeout per download molto grandi.
            req.timeout = 0;

            DownloadHandlerFile handler =
                new DownloadHandlerFile(
                    destinationFile);

            handler.removeFileOnAbort =
                false;

            req.downloadHandler =
                handler;

            UnityWebRequestAsyncOperation operation =
                req.SendWebRequest();

            while (!operation.isDone)
            {
                float progress =
                    req.downloadProgress;

                if (progress >= 0f)
                {
                    slider.value =
                        Mathf.Clamp01(progress);
                }

                ulong downloaded =
                    req.downloadedBytes;

                string contentLength =
                    req.GetResponseHeader(
                        "Content-Length");

                ulong total = 0;

                bool hasTotal =
                    !string.IsNullOrWhiteSpace(
                        contentLength) &&
                    ulong.TryParse(
                        contentLength,
                        out total) &&
                    total > 0;

                if (hasTotal)
                {
                    txt.text =
                        "Download " +
                        (progress * 100f)
                            .ToString("0") +
                        "%  " +
                        FormatBytes(downloaded) +
                        " / " +
                        FormatBytes(total);
                }
                else
                {
                    txt.text =
                        "Download " +
                        (progress * 100f)
                            .ToString("0") +
                        "%  " +
                        FormatBytes(downloaded);
                }

                yield return null;
            }

#if UNITY_2020_1_OR_NEWER

            if (req.result !=
                UnityWebRequest.Result.Success)

#else

        if (req.isNetworkError ||
            req.isHttpError)

#endif
            {
                Debug.LogError(
                    "[ZIP DOWNLOAD ERROR] " +
                    req.error);

                txt.text =
                    "Errore download";

                DeleteFileSafely(
                    destinationFile,
                    "[ZIP PARTIAL DELETE]");

                completedCallback?.Invoke(false);

                yield break;
            }
        }

        if (!File.Exists(destinationFile))
        {
            Debug.LogError(
                "[ZIP DOWNLOAD] File non trovato dopo il download.");

            txt.text =
                "File ZIP non trovato";

            completedCallback?.Invoke(false);

            yield break;
        }

        FileInfo info =
            new FileInfo(destinationFile);

        if (info.Length <= 0)
        {
            Debug.LogError(
                "[ZIP DOWNLOAD] File ZIP vuoto.");

            txt.text =
                "File ZIP vuoto";

            DeleteFileSafely(
                destinationFile,
                "[ZIP EMPTY DELETE]");

            completedCallback?.Invoke(false);

            yield break;
        }

        slider.value =
            1f;

        txt.text =
            "Download completato - " +
            FormatBytes(info.Length);

        Debug.Log(
            "[ZIP DOWNLOAD COMPLETED] " +
            FormatBytes(info.Length));

        completedCallback?.Invoke(true);
    }
    private string ExtractSqlFileFromZip(
    string zipPath,
    string tempFolderRoot)
    {
        if (string.IsNullOrWhiteSpace(zipPath))
        {
            throw new ArgumentException(
                "zipPath vuoto.");
        }

        if (string.IsNullOrWhiteSpace(
            tempFolderRoot))
        {
            throw new ArgumentException(
                "tempFolderRoot vuoto.");
        }

        if (!File.Exists(zipPath))
        {
            throw new FileNotFoundException(
                "ZIP non trovato.",
                zipPath);
        }

        FileInfo zipInfo =
            new FileInfo(zipPath);

        if (zipInfo.Length <= 0)
        {
            throw new InvalidDataException(
                "ZIP vuoto.");
        }

        string extractFolder =
            Path.Combine(
                tempFolderRoot,
                "sync_extract_" +
                Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(
            extractFolder);

        Debug.Log(
            "[ZIP] Estrazione in: " +
            extractFolder);

        try
        {
            ZipFile.ExtractToDirectory(
                zipPath,
                extractFolder);

            string[] sqlFiles =
                Directory.GetFiles(
                    extractFolder,
                    "*.sql",
                    SearchOption.AllDirectories);

            if (sqlFiles == null ||
                sqlFiles.Length == 0)
            {
                throw new FileNotFoundException(
                    "Nessun file .sql trovato nello ZIP.");
            }

            string sqlFile =
                sqlFiles
                    .OrderByDescending(
                        file =>
                        new FileInfo(file).Length)
                    .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(
                sqlFile))
            {
                throw new FileNotFoundException(
                    "File SQL non valido.");
            }

            FileInfo sqlInfo =
                new FileInfo(sqlFile);

            if (sqlInfo.Length <= 0)
            {
                throw new InvalidDataException(
                    "File SQL vuoto.");
            }

            Debug.Log(
                "[ZIP] SQL trovato: " +
                sqlFile);

            Debug.Log(
                "[ZIP] SQL SIZE: " +
                FormatBytes(sqlInfo.Length));

            return sqlFile;
        }
        catch
        {
            try
            {
                if (Directory.Exists(
                    extractFolder))
                {
                    Directory.Delete(
                        extractFolder,
                        true);
                }
            }
            catch (Exception cleanupEx)
            {
                Debug.LogWarning(
                    "[ZIP CLEANUP ERROR] " +
                    cleanupEx.Message);
            }

            throw;
        }
    }





    private void ExecuteSqlFileStreaming(
      string sqlFile,
      Action<float> progressCallback = null)
    {
        Debug.Log("[DB] STREAM START");

        if (string.IsNullOrWhiteSpace(sqlFile))
            throw new ArgumentException(
                "Percorso SQL vuoto.",
                nameof(sqlFile));

        if (!File.Exists(sqlFile))
            throw new FileNotFoundException(
                "File SQL non trovato.",
                sqlFile);

        FileInfo sqlInfo = new FileInfo(sqlFile);

        long sqlFileSize = sqlInfo.Length;

        if (sqlFileSize <= 0)
            throw new InvalidDataException(
                "Il file SQL è vuoto.");

        Debug.Log(
            "[DB] SQL SIZE: " +
            FormatBytes(sqlFileSize));

        long executed = 0;
        long bytesRead = 0;

        bool syncStarted = false;

        try
        {
            // ========================================================
            // TRANSAZIONE
            // ========================================================

            Debug.Log("[DB] BEGIN SYNC");

            _DBClass.BeginSync();

            syncStarted = true;

            Debug.Log("[DB] BEGIN SYNC OK");


            // ========================================================
            // STREAM FILE
            // ========================================================

            using (FileStream fileStream =
                   new FileStream(
                       sqlFile,
                       FileMode.Open,
                       FileAccess.Read,
                       FileShare.Read,
                       1024 * 1024,
                       FileOptions.SequentialScan))
            {
                // Buffer di lettura.
                // 64 KB è sufficiente e non crea grossi picchi di memoria.
                byte[] buffer = new byte[64 * 1024];

                // Query corrente.
                StringBuilder queryBuffer =
                    new StringBuilder(4096);

                int bytesReadNow;

                // Stato del separatore ;--
                int separatorState = 0;


                // ====================================================
                // LETTURA A BLOCCHI
                // ====================================================

                while ((bytesReadNow =
                        fileStream.Read(
                            buffer,
                            0,
                            buffer.Length)) > 0)
                {
                    bytesRead += bytesReadNow;


                    // ================================================
                    // PROCESSA I BYTE
                    // ================================================

                    for (int i = 0; i < bytesReadNow; i++)
                    {
                        char c = (char)buffer[i];


                        // =================================================
                        // RICONOSCIMENTO SEPARATORE ;--
                        //
                        // ;  -> stato 1
                        // -  -> stato 2
                        // -  -> separatore completo
                        // =================================================

                        if (separatorState == 0)
                        {
                            if (c == ';')
                            {
                                separatorState = 1;
                            }
                            else
                            {
                                queryBuffer.Append(c);
                            }
                        }
                        else if (separatorState == 1)
                        {
                            if (c == '-')
                            {
                                separatorState = 2;
                            }
                            else
                            {
                                // Il ; non faceva parte del separatore.
                                queryBuffer.Append(';');

                                if (c == ';')
                                {
                                    separatorState = 1;
                                }
                                else
                                {
                                    queryBuffer.Append(c);
                                    separatorState = 0;
                                }
                            }
                        }
                        else // separatorState == 2
                        {
                            if (c == '-')
                            {
                                // =========================================
                                // SEPARATORE TROVATO
                                // =========================================

                                separatorState = 0;

                                string sql =
                                    queryBuffer
                                        .ToString()
                                        .Trim();

                                queryBuffer.Clear();


                                // =========================================
                                // ESEGUI QUERY
                                // =========================================

                                if (sql.Length > 0)
                                {
                                    try
                                    {
                                        _DBClass.ExecSqlInTransaction(sql);

                                        executed++;

                                        // Log delle prime 5 query
                                        if (executed <= 5)
                                        {
                                            Debug.Log(
                                                "[DB] QUERY #" +
                                                executed +
                                                ": " +
                                                sql.Substring(
                                                    0,
                                                    Math.Min(
                                                        sql.Length,
                                                        500)));
                                        }
                                    }
                                    catch (Exception queryEx)
                                    {
                                        Debug.LogError(
                                            "[DB] ERRORE QUERY #" +
                                            (executed + 1));

                                        Debug.LogError(
                                            "[DB] QUERY:");

                                        Debug.LogError(sql);

                                        Debug.LogError(
                                            "[DB] EXCEPTION:");

                                        Debug.LogError(queryEx);

                                        throw;
                                    }


                                    // =====================================
                                    // PROGRESSO
                                    // =====================================

                                    if (executed % 100 == 0)
                                    {
                                        float progress =
                                            Mathf.Clamp01(
                                                (float)(
                                                    (double)bytesRead /
                                                    sqlFileSize));

                                        progressCallback?.Invoke(
                                            progress);

                                        Debug.Log(
                                            "[DB] EXEC " +
                                            executed +
                                            " | Progress " +
                                            (
                                                (double)bytesRead /
                                                sqlFileSize *
                                                100.0
                                            ).ToString("0.0") +
                                            "%");
                                    }


                                    // =====================================
                                    // GC
                                    // =====================================

                                    // NON fare Resources.UnloadUnusedAssets()
                                    // durante l'importazione.
                                    //
                                    // È molto pesante e non serve per le
                                    // stringhe .NET che stiamo gestendo.

                                    if (executed % 1000 == 0)
                                    {
                                        GC.Collect(
                                            0,
                                            GCCollectionMode.Optimized);

                                        Debug.Log(
                                            "[DB] MEMORY CLEANUP - QUERY " +
                                            executed);
                                    }
                                }
                            }
                            else
                            {
                                // Avevamo trovato ";-"
                                // ma il carattere successivo non è '-'.

                                queryBuffer.Append(';');
                                queryBuffer.Append('-');

                                if (c == ';')
                                {
                                    separatorState = 1;
                                }
                                else
                                {
                                    queryBuffer.Append(c);
                                    separatorState = 0;
                                }
                            }
                        }
                    }
                }


                // ====================================================
                // FINE FILE
                // ====================================================

                // Se siamo rimasti nello stato 1:
                if (separatorState == 1)
                {
                    queryBuffer.Append(';');
                }
                else if (separatorState == 2)
                {
                    queryBuffer.Append(';');
                    queryBuffer.Append('-');
                }


                // ====================================================
                // QUERY FINALE
                // ====================================================

                string remainingSql =
                    queryBuffer
                        .ToString()
                        .Trim();

                if (remainingSql.Length > 0)
                {
                    Debug.Log(
                        "[DB] QUERY FINALE SENZA ;--");

                    try
                    {
                        _DBClass.ExecSqlInTransaction(
                            remainingSql);

                        executed++;
                    }
                    catch (Exception queryEx)
                    {
                        Debug.LogError(
                            "[DB] ERRORE QUERY FINALE");

                        Debug.LogError(remainingSql);

                        Debug.LogError(queryEx);

                        throw;
                    }
                }
            }


            // ========================================================
            // FILE COMPLETAMENTE LETTO
            // ========================================================

            Debug.Log(
                "[DB] SQL LETTO COMPLETAMENTE.");

            Debug.Log(
                "[DB] QUERY ESEGUITE: " +
                executed);

            Debug.Log(
                "[DB] COMMIT DATABASE");


            // ========================================================
            // COMMIT
            // ========================================================

            _DBClass.EndSync();

            syncStarted = false;

            progressCallback?.Invoke(1f);

            Debug.Log(
                "[DB] STREAM END - SQL eseguiti: " +
                executed);
        }
        catch (Exception ex)
        {
            Debug.LogError(
                "[DB ERROR] Query eseguite prima dell'errore: " +
                executed);

            Debug.LogError(
                "[DB ERROR] " +
                ex);


            // ========================================================
            // CHIUSURA TRANSAZIONE
            // ========================================================

            if (syncStarted)
            {
                try
                {
                    Debug.Log(
                        "[DB] CHIUSURA SYNC DOPO ERRORE");

                    _DBClass.EndSync();

                    syncStarted = false;
                }
                catch (Exception syncEx)
                {
                    Debug.LogError(
                        "[DB SYNC CLOSE ERROR] " +
                        syncEx);
                }
            }

            throw;
        }
    }



    private void DeleteFileSafely(
    string filePath,
    string logPrefix)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return;

        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);

                Debug.Log(
                    logPrefix +
                    " " +
                    filePath);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning(
                logPrefix +
                " Impossibile eliminare il file: " +
                ex.Message);
        }
    }

    private long GetAvailableFreeSpace(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
                return -1;

            string fullPath =
                Path.GetFullPath(path);

            string root =
                Path.GetPathRoot(fullPath);

            if (string.IsNullOrWhiteSpace(root))
                return -1;

            DriveInfo drive =
                new DriveInfo(root);

            if (!drive.IsReady)
                return -1;

            return drive.AvailableFreeSpace;
        }
        catch (Exception ex)
        {
            Debug.LogWarning(
                "[STORAGE] Impossibile leggere spazio libero: " +
                ex.Message
            );

            return -1;
        }
    }

    private string FormatBytes(ulong bytes)
    {
        const double KB = 1024.0;
        const double MB = KB * 1024.0;
        const double GB = MB * 1024.0;

        if (bytes >= (ulong)GB)
        {
            return (bytes / GB)
                .ToString("0.00") +
                " GB";
        }

        if (bytes >= (ulong)MB)
        {
            return (bytes / MB)
                .ToString("0.00") +
                " MB";
        }

        if (bytes >= (ulong)KB)
        {
            return (bytes / KB)
                .ToString("0.00") +
                " KB";
        }

        return bytes + " B";
    }


    private string FormatBytes(long bytes)
    {
        if (bytes <= 0)
            return "0 B";

        return FormatBytes(
            (ulong)bytes);
    }
    public void EndSync(Button button)
    {
        _rotate = false;

        downloadSlider.gameObject.SetActive(false);
        dbSlider.gameObject.SetActive(false);

        // ==========================================================
        // IMPORTANTE:
        // la sincronizzazione è terminata PRIMA di eseguire
        // nuove query SQLite.
        // ==========================================================

        _syncRunning = false;

        if (button == null)
            return;

        try
        {
            Transform istatTransform =
                button.transform.Find("Istat");

            if (istatTransform == null)
            {
                Debug.LogWarning(
                    "[SYNC END] Istat non trovato nel button."
                );

                return;
            }

            Text istatText =
                istatTransform.GetComponent<Text>();

            if (istatText == null)
            {
                Debug.LogWarning(
                    "[SYNC END] Text Istat non trovato."
                );

                return;
            }

            string Istat =
                istatText.text;

            if (_DBClass == null)
            {
                GameObject sqliteObject =
                    GameObject.FindWithTag("SQLite");

                if (sqliteObject != null)
                {
                    _DBClass =
                        sqliteObject.GetComponent<DBClass>();
                }
            }

            if (_DBClass == null)
            {
                Debug.LogError(
                    "[SYNC END] DBClass non disponibile."
                );

                return;
            }

            List<DBClass.COMUNE> c =
                _DBClass.GetCOMUNI(
                    Istat,
                    null,
                    null,
                    null,
                    null
                );

            if (c != null &&
                c.Count > 0)
            {
                int n_poi =
                    _DBClass.getPOI_Count(
                        null,
                        c[0].id,
                        null,
                        null,
                        null
                    );

                if (n_poi > 0)
                {
                    _currentButton =
                        button;

                    _currentDownloadIcon =
                        _currentButton.transform
                            .Find("ImmagineDownload")
                            ?.gameObject;

                    var img =
                        button.transform.Find(
                            "ImmagineDownload"
                        );

                    if (img != null)
                        img.gameObject.SetActive(false);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(
                "[SYNC END] Errore aggiornamento UI: " +
                ex
            );
        }
    }

    void Update()
    {

        var scale = _logo.transform.localScale;
        if (_rotate)
        {
            _logo.transform.Rotate(0, 0, -0.4f);
        }
        else
        {
            if (_logo.transform.rotation != quaternion.identity)
                _logo.transform.rotation = quaternion.identity;
        }

        if ((PlayerPrefs.GetString("percorso_selezionato") != "" || PlayerPrefs.GetString("poi_selezionato") != ""))
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
        // Durante la sincronizzazione NON eseguire query SQLite.
        // Potrebbero mantenere un reader/connection aperto e impedire
        // a BeginSync() di ottenere il lock esclusivo.
        if (_syncRunning)
            return;

        if (!_towrite)
            return;

        _towrite = false;

        if (POIGo.Count > 0)
        {
            foreach (var ap in POIGo)
            {
                if (ap != null)
                    Destroy(ap);
            }

            POIGo.Clear();
        }

        if (_DBClass == null)
        {
            GameObject sqliteObject = GameObject.FindWithTag("SQLite");

            if (sqliteObject == null)
            {
                Debug.LogError("[GUI] Oggetto SQLite non trovato.");
                return;
            }

            _DBClass = sqliteObject.GetComponent<DBClass>();

            if (_DBClass == null)
            {
                Debug.LogError("[GUI] DBClass non trovato.");
                return;
            }
        }

        var comuni = _DBClass.GetCOMUNI(
            string.Empty,
            filtro_nome.text,
            null,
            true
        );

        StartCoroutine(BuildComuniList(comuni));
    }
    private IEnumerator BuildComuniList(List<COMUNE> comuni)
    {
        const int batchSize = 20;

        if (comuni == null)
            yield break;

        // Se è partita una sincronizzazione, non continuare
        // a lavorare sulla lista proveniente dal database.
        if (_syncRunning)
            yield break;

        for (int i = 0; i < comuni.Count; i++)
        {
            COMUNE comune = comuni[i];

            var btn = Instantiate(_btnComune);

            var pos = btn.transform.position;

            btn.transform.position = new Vector3(
                pos.x,
                (-1 * (pos.y + (737 * i))),
                pos.z);

            btn.transform.localScale = Vector3.one;

            foreach (var text in btn.GetComponentsInChildren<Text>(true))
            {
                switch (text.name)
                {
                    case "NomeComune":

                        text.text = comune.nome_comune;

                        if (!string.IsNullOrEmpty(comune.provincia))
                            text.text += $" ({comune.provincia})";

                        break;

                    case "TestoBreveComune":

                        text.text = comune.descrizioneBreve();

                        break;

                    case "Istat":

                        text.text = comune.istat;

                        break;
                }
            }

            foreach (var image in btn.GetComponentsInChildren<Image>(true))
            {
                if (image.name != "ImmagineComune")
                    continue;

                if (comune.Listimages != null &&
                    comune.Listimages.Count > 0 &&
                    comune.Listimages[0].image != null &&
                    comune.Listimages[0].image.Length > 0)
                {
                    image.sprite = _DBClass.getSpriteFromByteArray(
                        comune.Listimages[0].image);
                }
            }

            btn.SetActive(true);
            btn.transform.SetParent(content_list_comuni.transform, false);

            POIGo.Add(btn);

            if (i % batchSize == 0)
                yield return null;
        }

        content_list_comuni.GetComponent<RectTransform>().sizeDelta =
            new Vector2(0, (737 * comuni.Count) + 325);
    }
    public void OpenStoreForUpdate()
    {
        string appId = "com.task.poiqui"; // Es: com.azienda.gioco
        string appleId = "6759608710"; // Es: 123456789 (solo numeri)

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
