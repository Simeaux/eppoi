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
        Timeout = TimeSpan.FromSeconds(30)
    };

    private GameObject _btnComune;
    private int _lingua_selezionata = 1;
    private string _istat = "";
    private string queryString = "";
    private DBClass _DBClass;
    private List<GameObject> POIGo = new List<GameObject>();
    public GameObject _logo;
    private bool _rotate = false;

    private readonly BlockingCollection<List<string>> _sqlBatches =
        new BlockingCollection<List<string>>(200);

    private Thread _dbThread;

    private long _parsedQueries = 0;
    private long _executedQueries = 0;

    private readonly BlockingCollection<string> _sqlQueue =
        new BlockingCollection<string>(1000);

    private bool _streamFinished = false;

    private float _frameTime;
    private Button _currentButton;
    private GameObject _currentDownloadIcon;

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

        // Avvio del download come COROUTINE: cosi' lo spinner gira e la UI resta viva.
        List<int> _id_sinp = new List<int>();
        _id_sinp.Add(0);
        _DBClass.GetCOMUNI(null, null, null, null, null, true).ForEach(c => _id_sinp.Add(c.id));
        if (_id_sinp.Count > 0)
            StartCoroutine(StartSync(_id_sinp, null));

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

    private bool _syncRunning = false;

    public IEnumerator StartSync(List<int> id, Button button)
    {
        if (_syncRunning) yield break;

        yield return StartCoroutine(SyncRoutine(id, button));
    }


    private void StartDBWorker()
    {
        _dbThread = new Thread(() =>
        {
            Debug.Log("[DB THREAD] START");

            _DBClass.BeginSync();

            try
            {
                foreach (var batch in _sqlBatches.GetConsumingEnumerable())
                {
                    if (batch == null || batch.Count == 0)
                        continue;

                    _DBClass.BeginTransactionFast();

                    foreach (var sql in batch)
                    {
                        _DBClass.ExecSqlInTransaction(sql);

                        Interlocked.Increment(ref _executedQueries);
                    }

                    _DBClass.EndTransactionFast();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }

            _DBClass.EndSync();

            Debug.Log(
                $"[DB THREAD] END - Executed {Interlocked.Read(ref _executedQueries)}");
        });

        _dbThread.IsBackground = true;
        _dbThread.Start();
    }

    private IEnumerator StreamParser(
    Stream stream,
    long totalBytes,
    Slider downloadSlider,
    Text downloadText)
    {
        Debug.Log("[PARSER] START");

        byte[] buffer = new byte[65536];

        StringBuilder sb = new StringBuilder(1024 * 128);

        long totalRead = 0;

        List<string> batch = new List<string>(500);

        while (true)
        {
            var readTask = stream.ReadAsync(buffer, 0, buffer.Length);

            yield return new WaitUntil(() => readTask.IsCompleted);

            int read = readTask.Result;

            if (read <= 0)
                break;

            totalRead += read;

            sb.Append(
                Encoding.UTF8.GetString(
                    buffer,
                    0,
                    read));

            string current = sb.ToString();

            int idx;

            while ((idx = current.IndexOf(";--", StringComparison.Ordinal)) >= 0)
            {
                string sql = current.Substring(0, idx);

                sql = sql.Trim();

                if (sql.Length > 0)
                {
                    batch.Add(sql);

                    Interlocked.Increment(ref _parsedQueries);

                    if (batch.Count >= 500)
                    {
                        _sqlBatches.Add(batch);

                        batch = new List<string>(500);
                    }
                }

                current = current.Substring(idx + 3);
            }

            sb.Clear();
            sb.Append(current);

            if (totalBytes > 0)
            {
                float progress =
                    Mathf.Clamp01(
                        (float)totalRead / totalBytes);

                downloadSlider.value = progress;

                downloadText.text =
                    $"Download {(totalRead / 1024 / 1024)} MB / {(totalBytes / 1024 / 1024)} MB";
            }

            yield return null;
        }

        if (sb.Length > 0)
        {
            string sql = sb.ToString().Trim();

            if (sql.Length > 0)
            {
                batch.Add(sql);

                Interlocked.Increment(ref _parsedQueries);
            }
        }

        if (batch.Count > 0)
        {
            _sqlBatches.Add(batch);
        }

        Debug.Log(
            $"[PARSER] END Parsed={Interlocked.Read(ref _parsedQueries)}");
    }

    // Chiamala anche per il download "su richiesta": StartCoroutine(SyncRoutine(idSinp));
    public IEnumerator SyncRoutine(List<int> _id_sinp = null, Button button = null)
    {
        if (_id_sinp != null && _id_sinp.Count() == 1 && _id_sinp.FirstOrDefault() == 0)
        {
            yield break;
        }
        _syncRunning = true;

        Debug.Log("[SYNC] INIT");

        _rotate = true;

        bool singleComune =
            _id_sinp != null &&
            _id_sinp.Count == 1 &&
            _id_sinp[0] != 0;

        if (singleComune)
        {
            downloadSlider.gameObject.SetActive(true);
            dbSlider.gameObject.SetActive(true);

            downloadSlider.value = 0;
            dbSlider.value = 0;
        }

        string url =
            "https://www.macerataturismo.it/wp-json/rest_api_ws/v1/aggiorna_app_eppoi"
            + "?VERSIONE=" + _DBClass.getLastUpdatedFromTable("VERSIONE")
            + "&POI=" + _DBClass.getLastUpdatedFromTable("POI")
            + "&COMUNI=" + _DBClass.getLastUpdatedFromTable("COMUNI_TEXT")
            + "&PERCORSI=" + _DBClass.getLastUpdatedFromTable("PERCORSI");

        if (_id_sinp != null && _id_sinp.Count > 0)
            url += "&ID_SINP=" + string.Join(",", _id_sinp);

        url += "&t=" + DateTime.UtcNow.Ticks;

        Debug.Log("[SYNC URL] " + url);

        string sqlScript = null;

        if (singleComune)
        {
            //--------------------------------------------------
            // CASO 1: SERVER RESTITUISCE ZIP
            //--------------------------------------------------

            string zipPath =
                Path.Combine(
                    Application.persistentDataPath,
                    $"sync_{DateTime.UtcNow.Ticks}.zip");

            yield return StartCoroutine(
                DownloadZipCoroutine(
                    url,
                    zipPath,
                    downloadSlider,
                    downloadText));

            if (!File.Exists(zipPath))
            {
                Debug.LogError("[SYNC] ZIP NON TROVATO");

                EndSync(button);
                yield break;
            }

            bool completed = false;
            Exception workerException = null;
            string tempFolderRoot = Application.temporaryCachePath;
            Task.Run(() =>
            {
                try
                {
                    sqlScript = ExtractSqlFromZip(
                        zipPath,
                        tempFolderRoot);

                    if (string.IsNullOrWhiteSpace(sqlScript))
                        throw new Exception("SQL vuoto");

                    completed = true;
                }
                catch (Exception ex)
                {
                    workerException = ex;
                    completed = true;
                }
            });

            while (!completed)
                yield return null;

            try
            {
                File.Delete(zipPath);
            }
            catch
            {
            }

            if (workerException != null)
            {
                Debug.LogException(workerException);

                EndSync(button);
                yield break;
            }
        }
        else
        {
            //--------------------------------------------------
            // CASO 2: SERVER RESTITUISCE SQL DIRETTO
            //--------------------------------------------------

            Debug.Log("[SYNC] DOWNLOAD SQL");

            using (UnityWebRequest req = UnityWebRequest.Get(url))
            {
                req.SendWebRequest();

                while (!req.isDone)
                {
                    yield return null;
                }

#if UNITY_2020_1_OR_NEWER
                if (req.result != UnityWebRequest.Result.Success)
#else
            if (req.isNetworkError || req.isHttpError)
#endif
                {
                    Debug.LogError(req.error);

                    EndSync(button);
                    yield break;
                }

                sqlScript = req.downloadHandler.text;
                sqlScript = sqlScript.Trim();

                if (sqlScript.StartsWith("\""))
                    sqlScript = sqlScript.Substring(1);

                if (sqlScript.EndsWith("\""))
                    sqlScript = sqlScript.Substring(0, sqlScript.Length - 1);
            }

            if (string.IsNullOrWhiteSpace(sqlScript))
            {
                Debug.LogError("[SYNC] SQL VUOTO");

                EndSync(button);
                yield break;
            }
        }

        //--------------------------------------------------
        // ESECUZIONE SCRIPT SQL
        //--------------------------------------------------

        dbText.text = "Applicazione script...";
        dbSlider.value = 0.5f;

        bool dbCompleted = false;
        Exception dbException = null;

        Task.Run(() =>
        {
            try
            {
                ExecuteFullScriptInSingleTransaction(sqlScript);

                dbCompleted = true;
            }
            catch (Exception ex)
            {
                dbException = ex;

                dbCompleted = true;
            }
        });

        while (!dbCompleted)
            yield return null;

        if (dbException != null)
        {
            Debug.LogException(dbException);

            EndSync(button);
            yield break;
        }

        dbSlider.value = 1f;
        downloadSlider.value = 1f;

        Debug.Log("[SYNC] COMPLETE");

        EndSync(button);
    }


    private IEnumerator DownloadZipCoroutine(
        string url,
        string destinationFile,
        Slider slider,
        Text txt)
    {
        using (UnityWebRequest req =
               UnityWebRequest.Get(url))
        {
            req.downloadHandler =
                new DownloadHandlerFile(destinationFile);

            req.SendWebRequest();

            while (!req.isDone)
            {
                slider.value = req.downloadProgress;

                txt.text =
                    $"Download {(req.downloadProgress * 100f):0}%";

                yield return null;
            }

#if UNITY_2020_1_OR_NEWER
            if (req.result != UnityWebRequest.Result.Success)
#else
        if (req.isNetworkError || req.isHttpError)
#endif
            {
                Debug.LogError(req.error);
            }
        }
    }



    private string ExtractSqlFromZip(
    string zipPath,
    string tempFolderRoot)
    {
        string extractFolder =
            Path.Combine(
                tempFolderRoot,
                "sync_extract_" + Guid.NewGuid());

        Directory.CreateDirectory(extractFolder);

        ZipFile.ExtractToDirectory(
            zipPath,
            extractFolder);

        string sqlFile =
            Directory.GetFiles(
                extractFolder,
                "*",
                SearchOption.AllDirectories)
            .FirstOrDefault();

        if (string.IsNullOrEmpty(sqlFile))
            throw new Exception("Nessun file trovato nello zip");

        string sql =
            File.ReadAllText(
                sqlFile,
                Encoding.UTF8);

        try
        {
            Directory.Delete(extractFolder, true);
        }
        catch
        {
        }

        return sql;
    }



    private void ExecuteFullScriptInSingleTransaction(string sqlScript)
    {
        Debug.Log("[DB] START");

        string[] commands =
            sqlScript.Split(
                new[] { ";--" },
                StringSplitOptions.RemoveEmptyEntries);

        Debug.Log($"[DB] COMMANDS = {commands.Length}");

        _DBClass.BeginSync();

        try
        {
            int count = 0;

            foreach (string command in commands)
            {
                string sql = command.Trim();

                if (string.IsNullOrWhiteSpace(sql))
                    continue;

                _DBClass.ExecSqlInTransaction(sql);

                count++;

                if ((count % 1000) == 0)
                {
                    Debug.Log($"[DB] EXECUTED {count}");
                }
            }
        }
        finally
        {
            _DBClass.EndSync();
        }

        Debug.Log("[DB] END");
    }




    public void EndSync(Button button)
    {
        _rotate = false;

        downloadSlider.gameObject.SetActive(false);
        dbSlider.gameObject.SetActive(false);

        if (button == null)
        {
            _syncRunning = false;
            return;
        }


        var Istat = button.transform.Find("Istat").GetComponent<Text>().text;
        _DBClass = GameObject.FindWithTag("SQLite").GetComponent<DBClass>();
        List<DBClass.COMUNE> c = _DBClass.GetCOMUNI(Istat, null, null, null, null);
        if (c != null && c.Count > 0)
        {
            int n_poi = _DBClass.getPOI_Count(null, c[0].id, null, null, null);
            if (n_poi > 0)
            {
                if (button != null)
                {
                    _currentButton = button;

                    if (_currentButton != null)
                    {
                        _currentDownloadIcon =
                            _currentButton.transform
                                .Find("ImmagineDownload")
                                ?.gameObject;
                    }

                    var img =
                        button.transform.Find("ImmagineDownload");

                    if (img != null)
                        img.gameObject.SetActive(false);
                }
            }
        }
        _syncRunning = false;
    }

    void Update()
    {
        _frameTime = Time.unscaledDeltaTime * 1000f;

        if (_frameTime > 20f)
        {
            Debug.LogWarning($"[FRAME SLOW] {_frameTime}ms");
        }

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

        var comuni =
            _DBClass.GetCOMUNI(
                string.Empty,
                filtro_nome.text,
                null,
                true);

        StartCoroutine(BuildComuniList(comuni));
    }
    private IEnumerator BuildComuniList(List<COMUNE> comuni)
    {
        const int batchSize = 20;

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
