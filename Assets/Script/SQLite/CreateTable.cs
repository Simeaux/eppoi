using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ARLocation.UI;
using Mapbox.Json.Linq;
using Mono.Data.Sqlite;


using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static DBClass;
using IDbConnection = System.Data.IDbConnection;

public class CreateTable : MonoBehaviour
{
    private bool _call_all_togeter = false;
    public bool Verbose = false;
    public Slider _slider;
    string conn;
    string sqlQuery;
    IDbConnection dbconn;
    bool _increment_progress;

    // Start is called before the first frame update
    void Start()
    {
        _increment_progress = false;
        /*
        if (_slider != null)
        {
            _slider.maxValue = 0;
            _slider.value = 0;
        }
            */

    }

    //Update è chiamato a ogni frame
    private void Update()
    {
        if (_increment_progress)
        {
            _slider.maxValue = vs_to_call.Count;

            _increment_progress = false;
        }
    }
    // string DATABASE_NAME = "db:\\mydatabase.s3db";
    private bool _reset_db;
    public void CreateDB(bool reset_db, Slider slider)
    {
        getConnection();
        _slider = slider;
        _reset_db = reset_db;



        if (reset_db)
            ReadDataBase();
        else
        {
            CreateATable(reset_db);
            AddData();
        }

    }
    public void RemovePersistent_DB()
    {
        string dbName = "mydatabase.db";
        string sourcePath = Path.Combine(Application.persistentDataPath, dbName);
        if (File.Exists(sourcePath))
        {
            File.Delete(sourcePath);
        }
    }
    public void PersistentToStraming_DB()
    {
        string dbName = "mydatabase.db";
        string sourcePath = Path.Combine(Application.persistentDataPath, dbName);
        if (File.Exists(sourcePath))
        {
            string destinationPath = Path.Combine(Application.streamingAssetsPath, dbName);
            File.Copy(sourcePath, destinationPath, true);
        }
    }

    private void getConnection()
    {
        if (conn == null)
        {
            string dbName = "mydatabase.db";
            string destinationPath = Path.Combine(Application.persistentDataPath, dbName);
            conn = "URI=file:" + destinationPath;
        }
    }
    public void copyDB(Slider loadingBar, Button italiano, Button inglese, Toggle NonChiedereNuovamente)
    {
        string dbName = "mydatabase.db.zip";
        string destinationPath = Path.Combine(Application.persistentDataPath, dbName);
        string sourcePath = Path.Combine(Application.streamingAssetsPath, dbName);
        loadingBar.gameObject.SetActive(true);
        italiano.gameObject.SetActive(false);
        inglese.gameObject.SetActive(false);
        NonChiedereNuovamente.gameObject.SetActive(false);
        //Debug.Log(destinationPath);
        // Check if database already exists in the writable path



        string fileName = "comune_selected.txt";
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

#if UNITY_ANDROID
        Debug.Log("Database - Android");
        StartCoroutine(CopyDatabaseRoutineAndroid(dbName, loadingBar, italiano, inglese, NonChiedereNuovamente));
        StartCoroutine(ReadSettings(fileName));
#else
        string comune_selected_result;
        var copia = false;
        if (!File.Exists(destinationPath.Replace(".zip", "")))
            copia = true;
        else
        {
            FileInfo destinatinInfo = new FileInfo(destinationPath.Replace(".zip", ""));
            FileInfo sourceinInfo = new FileInfo(sourcePath);
            if (destinatinInfo.Length < sourceinInfo.Length)
                copia = true;
        }
        if (copia)
        {
            if (File.Exists(destinationPath))
                RemovePersistent_DB();
            // Fallback per Editor/PC/iOS dove File.Copy funziona
            File.Copy(sourcePath, destinationPath);
            comune_selected_result = File.ReadAllText(filePath);
            int selected_result = 0;
            int.TryParse(comune_selected_result, out selected_result);
            if (selected_result > 0)
            {
                Debug.Log("comune_selected. resetto");
                PlayerPrefs.SetInt("comune_selected", selected_result);
                PlayerPrefs.Save();
            }
            if (destinationPath.Contains(".zip"))
            {
                ZipFile.ExtractToDirectory(destinationPath, Path.Combine(Application.persistentDataPath), true);

                if (File.Exists(destinationPath))
                {
                    File.Delete(destinationPath);
                    Debug.Log("File ZIP rimosso con successo.");
                }
            }
        }
        OnCopyComplete(loadingBar, italiano, inglese, NonChiedereNuovamente);

#endif
        if (destinationPath.Contains(".zip"))
        {
            destinationPath = destinationPath.Replace(".zip", "");
        }
        // Open the database from the NEW writable location
        conn = "URI=file:" + destinationPath;
    }

    private IEnumerator ReadSettings(string fileName)
    {
        string sourcePath = Path.Combine(Application.streamingAssetsPath, fileName);
        string destPath = Path.Combine(Application.persistentDataPath, fileName);
        // Copia solo se il file non esiste già nella cartella persistente
        if (!File.Exists(destPath))
        {
            if (File.Exists(sourcePath))
            {
                using (UnityWebRequest request = UnityWebRequest.Get(sourcePath))
                {
                    yield return request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        File.WriteAllBytes(destPath, request.downloadHandler.data);
                        Debug.Log("File copiato correttamente!");
                    }
                    else
                    {
                        Debug.LogError("Errore copia: " + request.error);
                    }
                }
                // Ora puoi leggerlo
                LeggiFile(destPath);
            }
        }
    }
    private void LeggiFile(string path)
    {
        string comune_selected_result = "";
        if (File.Exists(path))
        {
            comune_selected_result = File.ReadAllText(path);
        }


        int selected_result = 0;
        int.TryParse(comune_selected_result, out selected_result);
        if (selected_result > 0)
        {
            Debug.Log("comune_selected. resetto");
            PlayerPrefs.SetInt("comune_selected", selected_result);
            PlayerPrefs.Save();
        }
    }

    private IEnumerator CopyDatabaseRoutineAndroid(string fileName, Slider loadingBar, Button italiano, Button inglese, Toggle NonChiedereNuovamente)
    {
        string sourcePath = Path.Combine(Application.streamingAssetsPath, fileName);
        string destPath = Path.Combine(Application.persistentDataPath, fileName);

        // 1. Controllo esistenza
        if (File.Exists(destPath.Replace(".zip", "")))
        {
            var copia = false;
            FileInfo destinatinInfo = new FileInfo(destPath.Replace(".zip", ""));
            Debug.Log("Database " + destinatinInfo.Length);
            if (destinatinInfo.Length < 10)
                copia = true;

            if (!copia)
            {
                Debug.Log("Database già presente.");
                OnCopyComplete(loadingBar, italiano, inglese, NonChiedereNuovamente);
                yield break;
            }
        }

        // 2. Operazione di copia
        using (UnityWebRequest request = UnityWebRequest.Get(sourcePath))
        {
            request.downloadHandler = new DownloadHandlerFile(destPath);
            var operation = request.SendWebRequest();
            loadingBar.value = 0;
            while (!operation.isDone)
            {
                if (loadingBar != null)
                {
                    loadingBar.value = request.downloadProgress;
                    loadingBar.gameObject.SetActive(true);
                    italiano.gameObject.SetActive(false);
                    inglese.gameObject.SetActive(false);
                    NonChiedereNuovamente.gameObject.SetActive(false);
                    Debug.Log("Database " + loadingBar.value);
                }

                yield return null;
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Copia riuscita!");
                OnCopyComplete(loadingBar, italiano, inglese, NonChiedereNuovamente);
            }
            else
            {
                Debug.Log($"Errore copia: {request.error}");
            }
        }
        if (destPath.Contains(".zip"))
        {
            ZipFile.ExtractToDirectory(destPath, Path.Combine(Application.persistentDataPath), true);

            if (File.Exists(destPath))
            {
                File.Delete(destPath);
                Debug.Log("File ZIP rimosso con successo.");
            }
        }
    }

    private void OnCopyComplete(Slider loadingBar, Button italiano, Button inglese, Toggle NonChiedereNuovamente)
    {
        // Qui inserisci cosa succede dopo (es: carica la scena successiva)
        Debug.Log("Pronto per aprire il database.");
        loadingBar.gameObject.SetActive(false);
        italiano.gameObject.SetActive(true);
        inglese.gameObject.SetActive(true);
        NonChiedereNuovamente.gameObject.SetActive(true);

    }
    /*
    private void setConnection()
    {
        //conn = "URI=file:mydatabase.db:";
        string dbname = "mydatabase.db";
        string filepath = Application.streamingAssetsPath + "/" + dbname;
        //string filepath = Application.persistentDataPath + "/" + dbname;

        if (!File.Exists(filepath))
        {

            Debug.LogWarning("File " + filepath + " does not exist.Attempting to create from " + Application.dataPath + "!/ assets /" + dbname);
#if UNITY_ANDROID
            var loadDb = new WWW("jar:file://" + Application.dataPath + "!/assets/" + dbname);  // this is the path to your StreamingAssets in android
            while (!loadDb.isDone) { }  // CAREFUL here, for safety reasons you shouldn't let this while loop unattended, place a timer and error check
            // then save to Application.persistentDataPath
            File.WriteAllBytes(filepath, loadDb.bytes);
#elif UNITY_IOS
                 var loadDb = Application.dataPath + "/Raw/" + dbname;  // this is the path to your StreamingAssets in iOS
                // then save to Application.persistentDataPath
                File.Copy(loadDb, filepath);

#else
	var loadDb = Application.dataPath + "/StreamingAssets/" + dbname;  // this is the path to your StreamingAssets in iOS
	// then save to Application.persistentDataPath
	File.Copy(loadDb, filepath);

#endif
        }

        //open db connection

        conn = "URI = file:" +filepath;
        //Debug.Log("Daje1 " + conn);
        if (Verbose)
            Debug.Log("Stablishing connection to: " + conn);

    }
    */
    private void CreateATable(bool reset_db)
    {
        _call_all_togeter = reset_db;
        Debug.Log("Daje2 " + conn);
        using (dbconn = new SqliteConnection(conn))
        {
            Debug.Log("Daje3 " + conn);

            if (Verbose)
                Debug.Log("Stablished connection to: " + conn);

            if (reset_db)
            {
                cancella_tabella("SETTING");
                cancella_tabella("POI");
                cancella_tabella("POI_IMMAGINI");
                cancella_tabella("POI_TEXT");
                cancella_tabella("TIPO_POI");
                cancella_tabella("TIPO_POI_TEXT");
                cancella_tabella("POIXTIPO");
                cancella_tabella("GROUP_TIPO_POI");
                cancella_tabella("GROUP_TIPO_POI_TEXT");
                cancella_tabella("COMUNI");
                cancella_tabella("COMUNI_TEXT");
                cancella_tabella("COMUNI_IMMAGINI");
                cancella_tabella("LINGUE");
                cancella_tabella("PERCORSI");
                cancella_tabella("PERCORSI_TEXT");
                cancella_tabella("PERCORSI_IMMAGINI");
                cancella_tabella("TAPPE");
                cancella_tabella("TAPPE_TEXT");
                cancella_tabella("TAPPE_IMMAGINI");
                cancella_tabella("POIXTAPPE");
                cancella_tabella("TAPPEXPERCORSI");
                PlayerPrefs.SetInt("lingua_selezionata", 0);
                PlayerPrefs.SetString("istat", "");
                PlayerPrefs.SetString("poi_selezionato", "");
                PlayerPrefs.SetInt("percorso_selezionato", 0);


                using (var dbcmd = dbconn.CreateCommand())
                {
                    #region creo tabelle
                    dbconn.Open();
                    var loaded_text_file = Resources.Load("createtables") as TextAsset;


                    dbcmd.CommandText = loaded_text_file.text;
                    Debug.Log(loaded_text_file.text);
                    dbcmd.ExecuteNonQuery();
                    dbconn.Close();

                    #endregion

                }
                //Application.Quit();
            }
        }
    }

    void cancella_tabella(string tabella_name)
    {
        bool table_exists = true;

        using (var dbcmd = dbconn.CreateCommand())
        {
            Debug.Log("cancella tabella: " + tabella_name);
            dbconn.Open();
            if (Verbose)
                Debug.Log("Connection opened to: " + conn);
            sqlQuery = $"SELECT count(name) FROM sqlite_master WHERE type='table' AND name='{tabella_name}'";
            dbcmd.CommandText = sqlQuery;
            IDataReader _reader = dbcmd.ExecuteReader();
            while (_reader.Read())
            {
                table_exists = _reader.GetInt32(0) > 0;
            }
            dbconn.Close();
        }
        if (table_exists)
        {
            using (var dbcmd = dbconn.CreateCommand())
            {
                dbconn.Open();
                sqlQuery = $"DROP TABLE '{tabella_name}'";
                dbcmd.CommandText = sqlQuery;
                Debug.Log(sqlQuery);
                dbcmd.ExecuteNonQuery();
                dbconn.Close();
            }
        }
    }
    //Test chiamata webservice
    public static bool WebRequestResultIsError(UnityWebRequest request)
    {
#if UNITY_2020_3_OR_NEWER
        return (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError);
#else
            return (request.isNetworkError || request.isHttpError);
#endif
    }
    /*
    private bool isLoaded = false;
    //private string jsonreturned = string.Empty;
    //this one to wait for response and decode
    private IEnumerator WaitingForJson()
    {
        while (!isLoaded)
            yield return new WaitForSeconds(0.1f);

        LoadJson();
    }
    */

    private List<string> vs_to_call = new List<string>();
    private string _vs = string.Empty;
    private void AddDataParallel()
    {
        if (vs_to_call.Count > 0)
        {

            _slider.maxValue = vs_to_call.Count;
            _slider.value = 0;
            if (_call_all_togeter)
            {
                if (vs_to_call.Count > 0)
                {
                    // versione che chiede 2 json alla volta

                    bool to_call = false;
                    bool to_call_after_loop = false;
                    foreach (string vs in vs_to_call)
                    {
                        _increment_progress = true;
                        _slider.value = _slider.value + 1;
                        Update();
                        //Scene scene = SceneManager.GetActiveScene();
                        //scene.RepaintAll(); //You can try this, it works for me.
                        if (_slider.value % 2 == 0)
                        {
                            //Il numero è pari
                            _vs = _vs + ",";
                            to_call = true;
                            to_call_after_loop = false;
                        }
                        else
                        {
                            int _comune_id = 0;
                            var data = "";
                            if (int.TryParse(vs.Replace("_itinerario", ""), out _comune_id))
                            {
                                var _poi_list = getPOI(1, 0, _comune_id, null, 0, null, null, null, null, null, true);
                                var _data = _poi_list.OrderByDescending(p => p.mod_dte).FirstOrDefault()?.mod_dte.ToString("yyyy-MM-dd hh:mm:ss");
                                if (!string.IsNullOrEmpty(_data))
                                    data = "mod_dte=" + _data + "&";
                                else
                                    data = "";
                            }

                            //Il numero è dispari
                            _vs = "?" + data + "id_sinp=";
                            to_call_after_loop = true;
                        }
                        _vs = _vs + vs;

                        if (to_call)
                        {
                            AddData();
                            to_call = false;
                        }
                    }
                    if (to_call_after_loop)
                    {
                        AddData();
                    }
                }
            }
            else
            {
                //versione che chiede tutti i json insieme
                bool to_call_after_loop = false;
                foreach (string vs in vs_to_call)
                {
                    _slider.value = _slider.value + 1;
                    _increment_progress = true;
                    Update();
                    if (_slider.value > 1)
                    {
                        _vs = _vs + ",";
                    }
                    else
                    {
                        to_call_after_loop = true;
                        var data = "";
                        var _poi_list = getPOI(1, null, null, null, 0, null, null, null, null, null, true);
                        var _data = _poi_list.OrderByDescending(p => p.mod_dte).FirstOrDefault()?.mod_dte.ToString("yyyy-MM-dd hh:mm:ss");
                        if (!string.IsNullOrEmpty(_data))
                            data = "mod_dte=" + _data + "&";


                        //Il numero è dispari
                        _vs = "?" + data + "id_sinp=";
                    }
                    _vs = _vs + vs;

                }
                if (to_call_after_loop)
                {
                    AddData();
                }
            }
        }
        vs_to_call = new List<string>();
    }

    private void ReadDataBase()
    {
        string dbname = "mydatabase.db";
        /*
#if UNITY_IOS
                Debug.Log("ios");
#else

#if UNITY_ANDROID
               string  path = "jar:file://" + Application.dataPath + "!/assets/alphabet.txt";
                 WWW wwwfile = new WWW(path);
                 while (!wwwfile.isDone) { }
                 var filepath = string.Format("{0}/{1}", Application.persistentDataPath, "alphabet.t");
                 File.WriteAllBytes(filepath, wwwfile.bytes);
 
                 StreamReader wr = new StreamReader(filepath);
                     string line;
                     while ((line = wr.ReadLine()) != null)
                     {
                     //your code
                     }
            Debug.Log("android");
#else
            Debug.Log("something else");
#endif


#endif
        //string pathToAssetsFolder = UnityEngine.Application.dataPath.;
        */
        string fileToCopy = "";
#if UNITY_IOS
        fileToCopy = Application.streamingAssetsPath + dbname;
#endif
#if UNITY_ANDROID
        //Debug.Log(Application.streamingAssetsPath);
        //fileToCopy = "jar:file://" + Application.dataPath + "!/assets/"+dbname;
        //fileToCopy = System.IO.Path.Combine(Application.streamingAssetsPath , dbname);
        fileToCopy = Application.streamingAssetsPath + "/" + dbname;
#endif
        string destinationDirectory = Application.persistentDataPath + "/" + dbname;
        FileInfo myFile = new FileInfo(destinationDirectory);
        if (myFile.Exists)
            myFile.Delete();

        File.Copy(fileToCopy, destinationDirectory);
        /*
        string path = "Assets/Script/JSON/dati_per_app.json";
        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(path);
        var risposta = "{\"Risposta\": {\"\": " + reader.ReadToEnd() + "}}";
        LoadJson(risposta);
        */
    }
    /*
    private void ReadDataBase2()
    {
        Parallel.ForEach(vs_to_call, vs =>
        {
            //foreach (string vs in vs_to_call)
            //{
            string path = "Assets/Script/JSON/" + vs + ".json";
            if (File.Exists(path))
            {
                //Read the text from directly from the test.txt file
                StreamReader reader = new StreamReader(path);
                var risposta = "{\"Risposta\": {\"" + vs + "\": " + reader.ReadToEnd() + "}}";
                LoadJson(risposta);
            }
            //}
        });
    }
    */
    private void AddData()
    {

        string vs = _vs;
        if (string.IsNullOrEmpty(vs))
        {
            string mod_dte = string.Empty;
            var _comuni_list = getCOMUNI(1);
            var _data = _comuni_list.OrderByDescending(p => p.mod_dte).FirstOrDefault()?.mod_dte.ToString("yyyy-MM-dd hh:mm:ss");
            vs = "?mod_dte=" + _data;
            foreach (var _comuni in _comuni_list)
            {
                vs_to_call.Add(_comuni.id + "");
                vs_to_call.Add(_comuni.id + "_itinerario");
            }
            AddDataParallel();
        }
        string indirizzo = "https://www.macerataturismo.it/wp-json/rest_api_ws/v1/get_json" + vs;
        Debug.Log(indirizzo);
        var www = UnityWebRequest.PostWwwForm(indirizzo, "");

        www.SendWebRequest();
        while (www.result == UnityWebRequest.Result.InProgress)
            new WaitForSeconds(0.1f);
        if (WebRequestResultIsError(www))
        {
            Debug.Log(www.error);
            Debug.Log(indirizzo);
        }
        else
        {
            // Show results as text
            string jsonreturned = www.downloadHandler.text;
            LoadJson(jsonreturned);
            //_ = MyAsyncFunction();

        }
    }


    private void LoadJson(string jsonreturned)
    {
        Debug.Log("loadJson " + jsonreturned.Length);
        if (jsonreturned != string.Empty)
        {
            //isLoaded = false;
            JObject json = JObject.Parse(jsonreturned);
            foreach (var ap in json)
            {
                if (ap.Key == "Risposta")
                {
                    if (!string.IsNullOrEmpty(ap.Value.ToString()))
                    {
                        try
                        {
                            if (ap.Value.ToString() != "[]")
                            {
                                JObject obj = JObject.Parse(ap.Value.ToString());
                                foreach (var obj1_ in obj)
                                {
                                    Debug.Log("Coune: " + obj1_.Key);
                                    JObject obj1 = JObject.Parse(obj1_.Value.ToString());
                                    foreach (var obj2 in obj1)
                                    {
                                        Debug.Log(obj2.Key);
                                        //JObject obj_dati = JObject.Parse(obj2.Value.ToString());
                                        switch (obj2.Key)
                                        {
                                            case "LINGUE":
                                                add_lingue(obj2.Value);
                                                break;
                                            case "GROUP_TIPO_POI":
                                                add_group_tipo_poi(obj2.Value);
                                                break;
                                            case "GROUP_TIPO_POI_TEXT":
                                                add_group_tipo_poi_text(obj2.Value);
                                                break;
                                            case "TIPO_POI":
                                                add_tipo_poi(obj2.Value);
                                                break;
                                            case "TIPO_POI_TEXT":
                                                add_tipo_poi_text(obj2.Value);
                                                break;
                                            case "COMUNI":
                                                add_comuni(obj2.Value);
                                                break;
                                            case "COMUNI_IMMAGINI":
                                                add_comuni_immagini(obj2.Value);
                                                break;
                                            case "COMUNI_TEXT":
                                                add_comuni_text(obj2.Value);
                                                break;
                                            case "POI":
                                                add_poi(obj2.Value);
                                                break;
                                            case "POI_TEXT":
                                                add_poi_text(obj2.Value);
                                                break;
                                            case "POI_IMMAGINI":
                                                add_poi_immagini(obj2.Value);
                                                break;
                                            case "POIXTIPO":
                                                add_poixtipo(obj2.Value);
                                                break;
                                            case "PERCORSI":
                                                add_percorsi(obj2.Value);
                                                break;
                                            case "PERCORSI_TEXT":
                                                add_percorsi_text(obj2.Value);
                                                break;
                                            case "PERCORSI_IMMAGINI":
                                                add_percorsi_immagini(obj2.Value);
                                                break;
                                            case "TAPPE":
                                                add_tappe(obj2.Value);
                                                break;
                                            case "TAPPE_TEXT":
                                                add_tappe_text(obj2.Value);
                                                break;
                                            case "TAPPE_IMMAGINI":
                                                add_tappe_immagini(obj2.Value);
                                                break;
                                            case "POIXTAPPE":
                                                add_poiXTappe(obj2.Value);
                                                break;
                                            case "TAPPEXPERCORSI":
                                                add_TappeXPercorsi(obj2.Value);
                                                break;
                                            default:
                                                // code block
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.Log(e.Message);
                        }
                    }
                }
            }
        }

    }

    public void add_lingue(JToken obj)
    {

        dbconn.Open();
        using (var dbcmd = dbconn.CreateCommand())
        {
            var _app = getLINGUE();
            foreach (var o in obj)
            {
                if (_app.Where(p => p.id == (double)o["id"]).Count() == 0)
                    sqlQuery = $" insert into LINGUE (id, sigla, lingua, mod_dte, uuid) values (" + o["id"] + ", '" + o["sigla"] + "', '" + o["lingua"] + "', '" + o["mod_dte"] + "', '" + Guid.NewGuid() + "')";
                else
                    sqlQuery = $" update LINGUE set sigla = '" + o["sigla"] + "', lingua = '" + o["lingua"] + "', mod_dte = '" + o["mod_dte"] + "' WHERE id =" + o["id"];
                if (!string.IsNullOrEmpty(sqlQuery))
                {
                    Debug.Log(sqlQuery);
                    dbcmd.CommandText = sqlQuery;
                    dbcmd.ExecuteNonQuery();
                }

            }
        }
        dbconn.Close();
    }
    public void add_group_tipo_poi(JToken obj)
    {

        dbconn.Open();
        using (var dbcmd = dbconn.CreateCommand())
        {
            var _app = getGROUP_TIPO_POI();
            int n = 0;
            foreach (var o in obj)
            {
                if (_app.Where(p => p.id == (double)o["id"]).Count() == 0)
                    sqlQuery = $" insert into GROUP_TIPO_POI (id, indice, value, attivo, mod_dte, uuid) values (" + o["id"] + ", " + n + ", '" + o["value"] + "', '" + o["attivo"] + "', '" + o["mod_dte"] + "', '" + Guid.NewGuid() + "')";
                else
                    sqlQuery = $" update GROUP_TIPO_POI set indice=" + n + ", value= '" + o["value"] + "', attivo= '" + o["attivo"] + "', mod_dte = '" + o["mod_dte"] + "' WHERE id =" + o["id"];
                if (!string.IsNullOrEmpty(sqlQuery))
                {
                    dbcmd.CommandText = sqlQuery;
                    dbcmd.ExecuteNonQuery();
                }
                n++;
            }
        }
        dbconn.Close();
    }
    public void add_group_tipo_poi_text(JToken obj)
    {

        dbconn.Open();
        using (var dbcmd = dbconn.CreateCommand())
        {
            var _app = getGROUP_TIPO_POI_TEXT(1);
            foreach (var t in getGROUP_TIPO_POI_TEXT(2))
                _app.Add(t);
            foreach (var o in obj)
            {
                if (_app.Where(p => p.id == (double)o["id"]).Count() == 0)
                    sqlQuery = $" insert into GROUP_TIPO_POI_TEXT (id, group_tipo_poi_id, descrizione, lingua_id, attivo, mod_dte, uuid) values (" + o["id"] + ", " + o["group_tipo_poi_id"] + ", '" + o["descrizione"].ToString().Replace("'", "’") + "', " + o["lingua_id"] + ", '" + o["attivo"] + "', '" + o["mod_dte"] + "', '" + Guid.NewGuid() + "')";
                else
                    sqlQuery = $" update GROUP_TIPO_POI_TEXT set group_tipo_poi_id = " + o["group_tipo_poi_id"] + ", descrizione = '" + o["descrizione"].ToString().Replace("'", "’") + "', lingua_id = " + o["lingua_id"] + ", attivo =   '" + o["attivo"] + "', mod_dte = '" + o["mod_dte"] + "' WHERE id =" + o["id"];
                if (!string.IsNullOrEmpty(sqlQuery))
                {
                    //Debug.Log(sqlQuery);
                    dbcmd.CommandText = sqlQuery;
                    dbcmd.ExecuteNonQuery();
                }
            }
        }
        dbconn.Close();
    }
    public void add_tipo_poi(JToken obj)
    {

        dbconn.Open();
        using (var dbcmd = dbconn.CreateCommand())
        {
            var _app = getTIPO_POI();
            foreach (var o in obj)
            {
                var group_id = 0;
                if (!string.IsNullOrEmpty(o["group_id"].ToString()))
                    int.TryParse(o["group_id"].ToString(), out group_id);

                if (_app.Where(p => p.id == (double)o["id"]).Count() == 0)
                    sqlQuery = $" insert into TIPO_POI (id, tag, limite_zoom, group_id, attivo, mod_dte, uuid) values (" + o["id"] + ", '" + o["tag"] + "', " + o["limite_zoom"] + ", " + group_id + ", '" + o["attivo"] + "', '" + o["mod_dte"] + "', '" + Guid.NewGuid() + "')";
                else
                    sqlQuery = $" update TIPO_POI set tag = '" + o["tag"] + "', limite_zoom = " + o["limite_zoom"] + ", group_id = " + group_id + ", attivo =  '" + o["attivo"] + "', mod_dte = '" + o["mod_dte"] + "' WHERE id =" + o["id"];
                if (!string.IsNullOrEmpty(sqlQuery))
                {
                    //Debug.Log(sqlQuery);
                    dbcmd.CommandText = sqlQuery;
                    dbcmd.ExecuteNonQuery();
                }
            }
        }
        dbconn.Close();
    }
    public void add_tipo_poi_text(JToken obj)
    {

        dbconn.Open();
        using (var dbcmd = dbconn.CreateCommand())
        {
            var _app = getTIPO_POI_TEXT(1);
            foreach (var t in getTIPO_POI_TEXT(2))
                _app.Add(t);
            foreach (var o in obj)
            {
                if (_app.Where(p => p.id == (double)o["id"]).Count() == 0)
                    sqlQuery = $" insert into TIPO_POI_TEXT (id, tipo_poi_id, descrizione, lingua_id, attivo, mod_dte, uuid) values (" + o["id"] + ", " + o["tipo_poi_id"] + ", '" + o["descrizione"].ToString().Replace("'", "’") + "', " + o["lingua_id"] + ", '" + o["attivo"] + "', '" + o["mod_dte"] + "', '" + Guid.NewGuid() + "')";
                else
                    sqlQuery = $" update TIPO_POI_TEXT set tipo_poi_id = " + o["tipo_poi_id"] + ", descrizione = '" + o["descrizione"].ToString().Replace("'", "’") + "', lingua_id = " + o["lingua_id"] + ", attivo =  '" + o["attivo"] + "', mod_dte = '" + o["mod_dte"] + "' WHERE id =" + o["id"];
                if (!string.IsNullOrEmpty(sqlQuery))
                {
                    //Debug.Log(sqlQuery);
                    dbcmd.CommandText = sqlQuery;
                    dbcmd.ExecuteNonQuery();
                }
            }
        }
        dbconn.Close();
    }
    public void add_comuni(JToken obj)
    {

        dbconn.Open();
        using (var dbcmd = dbconn.CreateCommand())
        {
            var _app = getCOMUNI(1);
            foreach (var o in obj)
            {
                double altitudine = 0;
                double abitanti = 0;
                string _latitudine = "0";
                string _longitudine = "0";
                double latitudine = 0;
                double longitudine = 0;
                double _ap = 0;
                if (double.TryParse(o["latitudine"].ToString(), out _ap))
                {
                    latitudine = _ap;
                    while (latitudine > 100)
                        latitudine = latitudine / 10;
                    _latitudine = latitudine.ToString().Replace(",", ".");
                }
                if (double.TryParse(o["longitudine"].ToString(), out _ap))
                {
                    longitudine = _ap;
                    while (longitudine > 100)
                        longitudine = longitudine / 10;
                    _longitudine = longitudine.ToString().Replace(",", ".");
                }

                if (double.TryParse(o["altitudine"].ToString(), out _ap))
                    altitudine = _ap;
                if (double.TryParse(o["abitanti"].ToString(), out _ap))
                    abitanti = _ap;
                if (_app.Where(p => p.id == (double)o["id"]).Count() == 0)
                    sqlQuery = $" insert into COMUNI (id, istat, nome_comune, provincia, latitudine, longitudine, altitudine, abitanti, attivo, mod_dte, uuid) values (" + o["id"] + ", '" + o["istat"] + "', '" + o["nome_comune"].ToString().Replace("'", "’") + "', '" + o["provincia"] + "', '" + latitudine + "', '" + longitudine + "', '" + altitudine + "', '" + abitanti + "', '" + o["attivo"] + "', '" + o["mod_dte"] + "', '" + Guid.NewGuid() + "')";
                else
                    sqlQuery = $" update COMUNI set istat = '" + o["istat"] + "', nome_comune = '" + o["nome_comune"].ToString().Replace("'", "’") + "', provincia = '" + o["provincia"] + "', latitudine = '" + latitudine + "', longitudine = '" + longitudine + "', altitudine = '" + altitudine + "', abitanti = '" + abitanti + "', attivo = '" + o["attivo"] + "', mod_dte = '" + o["mod_dte"] + "' WHERE id =" + o["id"];
                if (!string.IsNullOrEmpty(sqlQuery))
                {
                    dbcmd.CommandText = sqlQuery;
                    //Debug.Log(sqlQuery);
                    dbcmd.ExecuteNonQuery();

                }
                vs_to_call.Add(o["id"] + "");
                vs_to_call.Add(o["id"] + "_itinerario");
            }
        }
        dbconn.Close();
    }
    public void add_comuni_text(JToken obj)
    {

        dbconn.Open();
        using (var dbcmd = dbconn.CreateCommand())
        {
            var _app = getCOMUNI_TEXT(1);
            foreach (var t in getCOMUNI_TEXT(2))
                _app.Add(t);
            foreach (var o in obj)
            {
                if (_app.Where(p => p.id == (double)o["id"]).Count() == 0)
                    sqlQuery = $" insert into COMUNI_TEXT (id, comune_id, descrizione_breve, descrizione, lingua_id, attivo, mod_dte, uuid) values (" + o["id"] + ", " + o["comune_id"] + ", '" + o["descrizione_breve"].ToString().Replace("'", "’") + "', '" + o["descrizione"].ToString().Replace("'", "’") + "', " + o["lingua_id"] + ", '" + o["attivo"] + "', '" + o["mod_dte"] + "', '" + Guid.NewGuid() + "')";
                else
                    sqlQuery = $" update COMUNI_TEXT set comune_id = " + o["comune_id"] + ", descrizione_breve = '" + o["descrizione_breve"].ToString().Replace("'", "’") + "', descrizione = '" + o["descrizione"].ToString().Replace("'", "’") + "', lingua_id = " + o["lingua_id"] + ", attivo = '" + o["attivo"] + "', mod_dte = '" + o["mod_dte"] + "' WHERE id =" + o["id"];
                if (!string.IsNullOrEmpty(sqlQuery))
                {
                    Debug.Log(sqlQuery);
                    dbcmd.CommandText = sqlQuery;
                    dbcmd.ExecuteNonQuery();
                }
            }
        }
        dbconn.Close();
        if (!_reset_db)
            AddDataParallel();
        //else
        //    ReadDataBase2();
    }
    public void add_poi(JToken obj)
    {

        dbconn.Open();
        using (var dbcmd = dbconn.CreateCommand())
        {
            List<POI> _app = new List<POI>();
            int _searched_comune = 0;
            double c_latitudine = 0;
            double c_longitudine = 0;
            foreach (var o in obj)
            {
                string _latitudine = "0";
                string _longitudine = "0";
                double latitudine = 0;
                double longitudine = 0;
                double _ap = 0;
                int comune_id = 0;
                int.TryParse(o["comune_id"].ToString(), out comune_id);
                if (_app.Count == 0)
                {
                    _app = getPOI(1, 0, comune_id);
                    foreach (var t in getPOI(2, 0, comune_id))
                        _app.Add(t);
                }
                if (o["latitudine"] != null && (double.TryParse(o["latitudine"].ToString(), out _ap)))
                {
                    latitudine = _ap;
                    while (latitudine > 100)
                        latitudine = latitudine / 10;
                    _latitudine = latitudine.ToString().Replace(",", ".");
                }
                if (o["longitudine"] != null && (double.TryParse(o["longitudine"].ToString(), out _ap)))
                {
                    longitudine = _ap;
                    while (longitudine > 100)
                        longitudine = longitudine / 10;
                    _longitudine = longitudine.ToString().Replace(",", ".");
                }
                float _distance = 0;
                if (o["comune_id"] != null)
                {
                    if (_searched_comune != comune_id)
                    {
                        var _c = getCOMUNI(1, null, null, comune_id).FirstOrDefault();
                        if (_c != null)
                        {

                            double c_ap = 0;
                            if (double.TryParse(_c.latitudine.ToString(), out c_ap))
                            {
                                c_latitudine = c_ap;
                                while (c_latitudine > 100)
                                    c_latitudine = c_latitudine / 10;
                            }
                            if (double.TryParse(_c.longitudine.ToString(), out c_ap))
                            {
                                c_longitudine = c_ap;
                                while (c_longitudine > 100)
                                    c_longitudine = c_longitudine / 10;
                            }
                            if (c_latitudine != 0 && c_longitudine != 0 && latitudine != 0 && longitudine != 0)
                                _distance = CalculateDistance((float)c_latitudine, (float)latitudine, (float)c_longitudine, (float)longitudine);
                        }
                        _searched_comune = comune_id;
                    }
                    else
                    {
                        if (c_latitudine != 0 && c_longitudine != 0 && latitudine != 0 && longitudine != 0)
                            _distance = CalculateDistance((float)c_latitudine, (float)latitudine, (float)c_longitudine, (float)longitudine);

                    }
                }
                if (_app.Where(p => p.ID == (double)o["id"]).Count() == 0)
                    sqlQuery = $" insert into POI (id, longitudine, latitudine, nome, webPage, facebook, instagram, telefono, mail, comune_id, distanza_dal_centro, attivo, mod_dte, uuid) values (" + o["id"] + ", " + _longitudine + ", " + _latitudine + ", '" + o["nome"].ToString().Replace("'", "’") + "', '" + o["webPage"] + "', '" + o["facebook"] + "', '" + o["instagram"] + "', '" + o["telefono"] + "', '" + o["mail"] + "',  " + comune_id + ", " + _distance + ", '" + o["attivo"] + "', '" + o["mod_dte"] + "', '" + Guid.NewGuid() + "')";
                else
                    sqlQuery = $" update POI set longitudine = " + _longitudine + ", latitudine = " + _latitudine + ", nome = '" + o["nome"].ToString().Replace("'", "’") + "', webPage = '" + o["webPage"] + "', facebook = '" + o["facebook"] + "', instagram = '" + o["instagram"] + "', telefono = '" + o["telefono"] + "', mail = '" + o["mail"] + "', comune_id = " + comune_id + ", distanza_dal_centro =  " + _distance + ", attivo = '" + o["attivo"] + "', mod_dte = '" + o["mod_dte"] + "' WHERE id =" + o["id"];
                if (!string.IsNullOrEmpty(sqlQuery))
                {
                    //Debug.Log(sqlQuery);
                    dbcmd.CommandText = sqlQuery;
                    dbcmd.ExecuteNonQuery();
                }
            }
        }
        dbconn.Close();
    }
    public void add_poi_text(JToken obj)
    {

        dbconn.Open();
        using (var dbcmd = dbconn.CreateCommand())
        {

            foreach (var o in obj)
            {
                int poi_id = 0;
                int.TryParse(o["poi_id"].ToString(), out poi_id);
                var _app = getPOI_TEXT(1, poi_id);
                foreach (var t in getPOI_TEXT(2, poi_id))
                    _app.Add(t);
                if (_app.Where(p => p.id == (double)o["id"]).Count() == 0)
                    sqlQuery = $" insert into POI_TEXT (id, poi_id, descrizione, lingua_id, attivo, mod_dte, uuid) values (" + o["id"] + ", " + o["poi_id"] + ", '" + o["descrizione"].ToString().Replace("'", "’") + "',  " + o["lingua_id"] + ",  '" + o["attivo"] + "', '" + o["mod_dte"] + "', '" + Guid.NewGuid() + "')";
                else
                    sqlQuery = $" update POI_TEXT set poi_id = " + o["poi_id"] + ", descrizione = '" + o["descrizione"].ToString().Replace("'", "’") + "', lingua_id = " + o["lingua_id"] + ", attivo = '" + o["attivo"] + "', mod_dte = '" + o["mod_dte"] + "' WHERE id =" + o["id"];
                if (!string.IsNullOrEmpty(sqlQuery))
                {
                    //Debug.Log(sqlQuery);
                    dbcmd.CommandText = sqlQuery;
                    dbcmd.ExecuteNonQuery();
                }
            }
        }
        dbconn.Close();
    }

    public void add_poi_immagini(JToken obj)
    {

        dbconn.Open();
        using (var dbcmd = dbconn.CreateCommand())
        {
            foreach (var o in obj)
            {
                long poi_id = Convert.ToInt64(o["poi_id"].ToString());
                var _app = getPOI_IMMAGINI(null, poi_id);
                if (_app.Where(p => p.id == (double)o["id"]).Count() == 0)
                    sqlQuery = $" insert into POI_IMMAGINI (id, poi_id, principale, descrizione, image, attivo, mod_dte, uuid) values (" + o["id"] + ", " + o["poi_id"] + ", " + o["principale"] + ", '" + o["descrizione"].ToString().Replace("'", "’") + "', '" + o["image"] + "', '" + o["attivo"] + "', '" + o["mod_dte"] + "', '" + Guid.NewGuid() + "')";
                else
                    sqlQuery = $" update POI_IMMAGINI set poi_id = " + o["poi_id"] + ", principale = " + o["principale"] + ", descrizione = '" + o["descrizione"].ToString().Replace("'", "’") + "', image = '" + o["image"] + "', attivo = '" + o["attivo"] + "', mod_dte = '" + o["mod_dte"] + "' WHERE id =" + o["id"];
                if (!string.IsNullOrEmpty(sqlQuery))
                {
                    //Debug.Log(sqlQuery);
                    dbcmd.CommandText = sqlQuery;
                    dbcmd.ExecuteNonQuery();
                }
            }
        }
        dbconn.Close();
    }
    // Decode a Base64 string to a string
    public static string DecodeBase64(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;
        Debug.Log(value);
        byte[] valueBytes = System.Convert.FromBase64String(value);
        Debug.Log(valueBytes.Count());

        string ret = System.Text.Encoding.UTF8.GetString(valueBytes);

        Debug.Log(ret);
        return ret;

    }
    public void add_comuni_immagini(JToken obj)
    {

        dbconn.Open();
        using (var dbcmd = dbconn.CreateCommand())
        {
            var _app = getCOMUNI_IMMAGINI();
            foreach (var o in obj)
            {
                string _img = string.Empty;
                if (o["image"] != null)
                {
                    _img = o["image"].ToString();
                }
                if (_app.Where(p => p.id == (double)o["id"]).Count() == 0)
                    sqlQuery = $" insert into COMUNI_IMMAGINI (id, comune_id, principale, descrizione, image, attivo, mod_dte, uuid) values (" + o["id"] + ", " + o["comune_id"] + ", " + o["principale"] + ", '" + o["descrizione"].ToString().Replace("'", "’") + "', '" + _img + "', '" + o["attivo"] + "', '" + o["mod_dte"] + "', '" + Guid.NewGuid() + "')";
                else
                    sqlQuery = $" update COMUNI_IMMAGINI set comune_id = " + o["comune_id"] + ", principale = " + o["principale"] + ", descrizione = '" + o["descrizione"].ToString().Replace("'", "’") + "', image = '" + _img + "', attivo = '" + o["attivo"] + "', mod_dte = '" + o["mod_dte"] + "' WHERE id =" + o["id"];
                if (!string.IsNullOrEmpty(sqlQuery))
                {
                    Debug.Log(sqlQuery);
                    dbcmd.CommandText = sqlQuery;
                    dbcmd.ExecuteNonQuery();
                }
            }
        }
        dbconn.Close();
    }
    public void add_poixtipo(JToken obj)
    {

        dbconn.Open();
        using (var dbcmd = dbconn.CreateCommand())
        {
            foreach (var o in obj)
            {
                int poi_id = 0;
                int.TryParse(o["poi_id"].ToString(), out poi_id);
                var _app = getPOIXTIPO(null, poi_id);
                if (_app.Where(p => p.id == (double)o["id"]).Count() == 0)
                    sqlQuery = $" insert into POIXTIPO (id, poi_id, tipo_id, attivo, mod_dte, uuid) values (" + o["id"] + ", '" + o["poi_id"] + "', " + o["tipo_id"] + ", '" + o["attivo"] + "', '" + o["mod_dte"] + "', '" + Guid.NewGuid() + "')";
                else
                    sqlQuery = $" update POIXTIPO set poi_id = '" + o["poi_id"] + "', tipo_id = " + o["tipo_id"] + ", attivo = '" + o["attivo"] + "', mod_dte = '" + o["mod_dte"] + "' WHERE id =" + o["id"];
                if (!string.IsNullOrEmpty(sqlQuery))
                {
                    //Debug.Log(sqlQuery);
                    dbcmd.CommandText = sqlQuery;
                    dbcmd.ExecuteNonQuery();
                }
            }
        }
        dbconn.Close();
    }

    public void add_percorsi(JToken obj)
    {

        dbconn.Open();
        using (var dbcmd = dbconn.CreateCommand())
        {
            var _app = getPERCORSI();
            foreach (var o in obj)
            {
                var colore = "#FFFF00";
                if (o["tipo_navigazione"].ToString().ToLower() == "iot")
                    colore = "#0007FF";

                if (_app.Where(p => p.id == (double)o["id"]).Count() == 0)
                    sqlQuery = $" insert into PERCORSI (id, tipo_percorso, tipo_navigazione, codice, poi_id, nome_percorso, lingua_id, percorso, attivo, colore, mod_dte, uuid) values (" + o["id"] + ", '" + o["tipo_percorso"] + "', '" + o["tipo_navigazione"] + "', '" + o["codice"] + "', " + o["poi_id"] + ", '" + o["nome_percorso"].ToString().Replace("'", "’") + "', " + o["lingua_id"] + ", '" + o["percorso"].ToString().Replace("'", "’") + "',  '" + o["attivo"] + "', '" + colore + "', '" + o["mod_dte"] + "', '" + Guid.NewGuid() + "')";
                else
                    sqlQuery = $" update PERCORSI set tipo_percorso = '" + o["tipo_percorso"] + "', tipo_navigazione = '" + o["tipo_navigazione"] + "', codice = '" + o["codice"] + "', poi_id = " + o["poi_id"] + ", nome_percorso = '" + o["nome_percorso"].ToString().Replace("'", "’") + "', lingua_id = " + o["lingua_id"] + ", percorso = '" + o["percorso"].ToString().Replace("'", "’") + "', colore = '" + colore + "', attivo = '" + o["attivo"] + "', mod_dte = '" + o["mod_dte"] + "' WHERE id =" + o["id"];
                if (!string.IsNullOrEmpty(sqlQuery))
                {
                    //Debug.Log(sqlQuery);
                    dbcmd.CommandText = sqlQuery;
                    dbcmd.ExecuteNonQuery();
                }
            }
        }
        dbconn.Close();
    }

    public void add_percorsi_text(JToken obj)
    {

        dbconn.Open();
        using (var dbcmd = dbconn.CreateCommand())
        {
            var _app = getPERCORSI_TEXT(1);
            foreach (var t in getPERCORSI_TEXT(2))
                _app.Add(t);
            foreach (var o in obj)
            {
                if (_app.Where(p => p.id == (double)o["id"]).Count() == 0)
                    sqlQuery = $" insert into PERCORSI_TEXT (id, percorso_id, descrizione_breve, descrizione, lingua_id, attivo, mod_dte, uuid) values (" + o["id"] + ", " + o["percorso_id"] + ", '" + o["descrizione_breve"].ToString().Replace("'", "’") + "', '" + o["descrizione"].ToString().Replace("'", "’") + "', " + o["lingua_id"] + ", '" + o["attivo"] + "', '" + o["mod_dte"] + "', '" + Guid.NewGuid() + "')";
                else
                    sqlQuery = $" update PERCORSI_TEXT set percorso_id = " + o["percorso_id"] + ", descrizione_breve = '" + o["descrizione_breve"].ToString().Replace("'", "’") + "', descrizione = '" + o["descrizione"].ToString().Replace("'", "’") + "', lingua_id = " + o["lingua_id"] + ", attivo = '" + o["attivo"] + "', mod_dte = '" + o["mod_dte"] + "' WHERE id =" + o["id"];
                if (!string.IsNullOrEmpty(sqlQuery))
                {
                    //Debug.Log(sqlQuery);
                    dbcmd.CommandText = sqlQuery;
                    dbcmd.ExecuteNonQuery();
                }
            }
        }
        dbconn.Close();
    }

    public void add_percorsi_immagini(JToken obj)
    {

        dbconn.Open();
        using (var dbcmd = dbconn.CreateCommand())
        {
            var _app = getPERCORSI_IMMAGINI();
            foreach (var o in obj)
            {
                if (_app.Where(p => p.id == (double)o["id"]).Count() == 0)
                    sqlQuery = $" insert into PERCORSI_IMMAGINI (id, percorso_id, principale, descrizione, image, attivo, mod_dte, uuid) values (" + o["id"] + ", " + o["percorso_id"] + ", " + o["principale"] + ", '" + o["descrizione"].ToString().Replace("'", "’") + "', '" + o["image"] + "', '" + o["attivo"] + "', '" + o["mod_dte"] + "', '" + Guid.NewGuid() + "')";
                else
                    sqlQuery = $" update PERCORSI_IMMAGINI set percorso_id = " + o["percorso_id"] + ", principale = " + o["principale"] + ", descrizione = '" + o["descrizione"].ToString().Replace("'", "’") + "', image = '" + o["image"] + "', attivo = '" + o["attivo"] + "', mod_dte = '" + o["mod_dte"] + "' WHERE id =" + o["id"];

                if (!string.IsNullOrEmpty(sqlQuery))
                {
                    //Debug.Log(sqlQuery);
                    dbcmd.CommandText = sqlQuery;
                    dbcmd.ExecuteNonQuery();
                }
            }
        }
        dbconn.Close();
    }

    public void add_tappe(JToken obj)
    {

        dbconn.Open();
        using (var dbcmd = dbconn.CreateCommand())
        {
            var _app = getTAPPE(1);
            foreach (var t in getTAPPE(2))
                _app.Add(t);
            foreach (var o in obj)
            {
                string _latitudine = "0";
                string _longitudine = "0";
                double latitudine = 0;
                double longitudine = 0;
                double _ap = 0;
                if (double.TryParse(o["latitudine"].ToString(), out _ap))
                {
                    latitudine = _ap;
                    while (latitudine > 100)
                        latitudine = latitudine / 10;
                    _latitudine = latitudine.ToString().Replace(",", ".");
                }
                if (double.TryParse(o["longitudine"].ToString(), out _ap))
                {
                    longitudine = _ap;
                    while (longitudine > 100)
                        longitudine = longitudine / 10;
                    _longitudine = longitudine.ToString().Replace(",", ".");
                }
                if (_app.Where(p => p.id == (double)o["id"]).Count() == 0)
                    sqlQuery = $" insert into TAPPE (id, nome_tappa, colore, latitudine, longitudine, attivo, mod_dte, uuid) values (" + o["id"] + ", '" + o["nome_tappa"].ToString().Replace("'", "’") + "', '" + o["colore"] + "', " + _latitudine + ", " + _longitudine + ", '" + o["attivo"] + "', '" + o["mod_dte"] + "', '" + Guid.NewGuid() + "')";
                else
                    sqlQuery = $" update TAPPE set nome_tappa = '" + o["nome_tappa"].ToString().Replace("'", "’") + "', colore = '" + o["colore"] + "', latitudine = " + _latitudine + ", longitudine = " + _longitudine + ", attivo = '" + o["attivo"] + "', mod_dte = '" + o["mod_dte"] + "' WHERE id =" + o["id"];
                if (!string.IsNullOrEmpty(sqlQuery))
                {
                    //Debug.Log(sqlQuery);
                    dbcmd.CommandText = sqlQuery;
                    dbcmd.ExecuteNonQuery();
                }
            }
        }
        dbconn.Close();
    }
    public void add_tappe_text(JToken obj)
    {

        dbconn.Open();
        using (var dbcmd = dbconn.CreateCommand())
        {
            var _app = getTAPPE_TEXT(1);
            foreach (var t in getTAPPE_TEXT(2))
                _app.Add(t);
            foreach (var o in obj)
            {
                if (_app.Where(p => p.id == (double)o["id"]).Count() == 0)
                    sqlQuery = $" insert into TAPPE_TEXT (id, tappa_id, descrizione_breve, descrizione, lingua_id, attivo, mod_dte, uuid) values (" + o["id"] + ", " + o["tappa_id"] + ", '" + o["descrizione_breve"].ToString().Replace("'", "’") + "', '" + o["descrizione"].ToString().Replace("'", "’") + "', " + o["lingua_id"] + ", '" + o["attivo"] + "', '" + o["mod_dte"] + "', '" + Guid.NewGuid() + "')";
                else
                    sqlQuery = $" update TAPPE_TEXT set tappa_id = " + o["tappa_id"] + ", descrizione_breve = '" + o["descrizione_breve"].ToString().Replace("'", "’") + "', descrizione = '" + o["descrizione"].ToString().Replace("'", "’") + "', lingua_id = " + o["lingua_id"] + ", attivo = '" + o["attivo"] + "', mod_dte = '" + o["mod_dte"] + "' WHERE id =" + o["id"];
                if (!string.IsNullOrEmpty(sqlQuery))
                {
                    //Debug.Log(sqlQuery);
                    dbcmd.CommandText = sqlQuery;
                    dbcmd.ExecuteNonQuery();
                }
            }
        }
        dbconn.Close();
    }
    public void add_tappe_immagini(JToken obj)
    {

        dbconn.Open();
        using (var dbcmd = dbconn.CreateCommand())
        {
            var _app = getTAPPE_IMMAGINI();
            foreach (var o in obj)
            {
                string _img = string.Empty;
                if (o["image"] != null)
                {
                    _img = o["image"].ToString();
                }

                if (_app.Where(p => p.id == (double)o["id"]).Count() == 0)
                    sqlQuery = $" insert into TAPPE_IMMAGINI (id, tappa_id, image, descrizione, princiale, attivo, mod_dte, uuid) values (" + o["id"] + ", " + o["tappa_id"] + ", '" + _img + "', '" + o["descrizione"].ToString().Replace("'", "’") + "', '" + o["principale"] + "', '" + o["attivo"] + "', '" + o["mod_dte"] + "', '" + Guid.NewGuid() + "')";
                else
                    sqlQuery = $" update TAPPE_IMMAGINI set tappa_id = " + o["tappa_id"] + ", image = '" + _img + "', descrizione = '" + o["descrizione"].ToString().Replace("'", "’") + "', princiale = '" + o["principale"] + "', attivo = '" + o["attivo"] + "', mod_dte = '" + o["mod_dte"] + "' WHERE id =" + o["id"];
                if (!string.IsNullOrEmpty(sqlQuery))
                {
                    Debug.Log(sqlQuery);
                    dbcmd.CommandText = sqlQuery;
                    dbcmd.ExecuteNonQuery();
                }
            }
        }
        dbconn.Close();
    }
    public void add_poiXTappe(JToken obj)
    {

        dbconn.Open();
        using (var dbcmd = dbconn.CreateCommand())
        {
            var _app = getPOIXTAPPE();
            foreach (var o in obj)
            {
                if (_app.Where(p => p.id == (double)o["id"]).Count() == 0)
                    sqlQuery = $" insert into POIXTAPPE (id, poi_id, tappa_id, attivo, mod_dte, uuid) values (" + o["id"] + ", " + o["poi_id"] + ", " + o["tappa_id"] + ", '" + o["attivo"] + "', '" + o["mod_dte"] + "', '" + Guid.NewGuid() + "')";
                else
                    sqlQuery = $" update POIXTAPPE set poi_id = " + o["poi_id"] + ", tappa_id = " + o["tappa_id"] + ", attivo = '" + o["attivo"] + "', mod_dte = '" + o["mod_dte"] + "' WHERE id =" + o["id"];
                if (!string.IsNullOrEmpty(sqlQuery))
                {
                    Debug.Log(sqlQuery);
                    dbcmd.CommandText = sqlQuery;
                    dbcmd.ExecuteNonQuery();
                }
            }
        }
        dbconn.Close();
    }
    public void add_TappeXPercorsi(JToken obj)
    {
        dbconn.Open();
        using (var dbcmd = dbconn.CreateCommand())
        {
            var _app = getTAPPEXPERCORSI();

            foreach (var o in obj)
            {
                if (_app.Where(p => p.id == (double)o["id"]).Count() == 0)
                    sqlQuery = $" insert into TAPPEXPERCORSI (id, tappa_id, percorso_id, ordine, attivo, mod_dte, uuid) values (" + o["id"] + ", " + o["tappa_id"] + ", " + o["percorso_id"] + ", " + o["ordine"] + ", '" + o["attivo"] + "', '" + o["mod_dte"] + "', '" + Guid.NewGuid() + "')";
                else
                    sqlQuery = $" update TAPPEXPERCORSI set tappa_id = " + o["tappa_id"] + ", percorso_id = " + o["percorso_id"] + ", ordine = " + o["ordine"] + ", attivo = '" + o["attivo"] + "', mod_dte = '" + o["mod_dte"] + "' WHERE id =" + o["id"];
                if (!string.IsNullOrEmpty(sqlQuery))
                {
                    Debug.Log(sqlQuery);
                    dbcmd.CommandText = sqlQuery;
                    dbcmd.ExecuteNonQuery();
                }
            }
        }
        dbconn.Close();
    }


    public float CalculateDistance(float lat_1, float lat_2, float long_1, float long_2)
    {
        int R = 6371;

        var lat_rad_1 = Mathf.Deg2Rad * lat_1;
        var lat_rad_2 = Mathf.Deg2Rad * lat_2;
        var d_lat_rad = Mathf.Deg2Rad * (lat_2 - lat_1);
        var d_long_rad = Mathf.Deg2Rad * (long_2 - long_1);

        var a = Mathf.Pow(Mathf.Sin(d_lat_rad / 2), 2) + (Mathf.Pow(Mathf.Sin(d_long_rad / 2), 2) * Mathf.Cos(lat_rad_1) * Mathf.Cos(lat_rad_2));
        var c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
        var total_dist = R * c * 1000; // convert to meters

        return Mathf.Ceil(total_dist);
    }
    private static string SafeSubString(string str, int maxSize)
    {
        if (maxSize <= 0)
            return string.Empty;

        if (str.Length <= maxSize)
            return str;

        return str.Substring(0, maxSize);
    }
    public DateTime getLastUpdatedFromTable(string table)
    {
        DateTime ret = new DateTime();
        getConnection();
        if (Verbose)
            Debug.Log("Stablished connection to: " + conn);

        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open();
            if (Verbose)
                Debug.Log("Connection opened to: " + conn);
            using (var dbcmd = dbconn.CreateCommand())
            {
                sqlQuery = $"select max(mod_dte) from " + table + " where attivo = 'Y'";
                //Debug.Log(sqlQuery);
                dbcmd.CommandText = sqlQuery;
                IDataReader _reader = dbcmd.ExecuteReader();
                while (_reader.Read())
                {
                    ret = _reader.GetDateTime(0);
                }
            }
            dbconn.Close();
        }
        return ret;
    }
    public string getversione()
    {
        string ret = "1";
        getConnection();
        if (Verbose)
            Debug.Log("Stablished connection to: " + conn);

        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open();
            if (Verbose)
                Debug.Log("Connection opened to: " + conn);
            using (var dbcmd = dbconn.CreateCommand())
            {
                sqlQuery = $"select valore from SETTING where tipo = 'versione' AND  attivo = 'Y'";
                //Debug.Log(sqlQuery);
                dbcmd.CommandText = sqlQuery;
                IDataReader _reader = dbcmd.ExecuteReader();
                while (_reader.Read())
                {
                    ret = _reader.IsDBNull(0) ? _reader.GetString(0) : "1";
                }
            }
            dbconn.Close();
        }
        return ret;
    }
    public List<POI> getPOIxMap(string? not_in = "", double[] punti = null, int? comune_id = null)
    {
        Debug.Log("in getPOIxMap" + DateTime.Now);
        List<POI> ret = new List<POI>();
        getConnection();
        if (Verbose)
            Debug.Log("Stablished connection to: " + conn);
        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open();
            if (Verbose)
                Debug.Log("Connection opened to: " + conn);
            using (var dbcmd = dbconn.CreateCommand())
            {

                sqlQuery = $"SELECT  POI.id, POI.longitudine, POI.latitudine ";
                sqlQuery += "FROM POI poi left join COMUNI comuni ON poi.comune_id = COMUNI.id where COMUNI.attivo = 'Y' AND poi.attivo = 'Y' AND poi.longitudine > 0 ";
                if (not_in != "")
                    sqlQuery += $" AND POI.id not in ({not_in})";
                if (comune_id != null && comune_id > 0)
                    sqlQuery += $" AND COMUNI.id = {comune_id}";
                //if (punti != null)
                //    sqlQuery += $" and poi.latitudine BETWEEN {punti[1]} and {punti[3]} and poi.longitudine BETWEEN {punti[0]} and {punti[2]}";
                sqlQuery += $" GROUP BY POI.id ";
                //if (punti == null)
                //    sqlQuery += $" LIMIT 2000";
                //Debug.Log(sqlQuery);
                dbcmd.CommandText = sqlQuery;
                IDataReader _reader = dbcmd.ExecuteReader();
                Debug.Log("fatte query 1 getPOIxMap" + DateTime.Now);
                List<POIXTIPO> _poixtipo = getPOIXTIPO(null, null);//= _reader.GetInt32(1);
                Debug.Log("fatte query 2 getPOIxMap" + DateTime.Now);
                while (_reader.Read())
                {
                    POI poi = new POI();
                    poi.ID = _reader.GetInt64(0);
                    poi.tipoList = _poixtipo.Where(o => o.poi_id == poi.ID).ToList();
                    double lo = _reader.GetFloat(1);
                    while (lo > 99)
                        lo /= 10;
                    poi.longitudine = lo;
                    double la = _reader.GetFloat(2);
                    while (la > 99)
                        la /= 10;
                    poi.latitudine = la;

                    poi.limite_zoom = 5;

                    ret.Add(poi);
                }
            }
            dbconn.Close();
        }
        Debug.Log("out getPOIxMap" + DateTime.Now);
        return ret;
    }

    public List<POI> getPOIGeneralita(long? id = null)
    {
        List<POI> ret = new List<POI>();
        getConnection();
        if (Verbose)
            Debug.Log("Stablished connection to: " + conn);
        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open();
            if (Verbose)
                Debug.Log("Connection opened to: " + conn);
            using (var dbcmd = dbconn.CreateCommand())
            {

                sqlQuery = $"SELECT  POI.id, POI.longitudine, POI.latitudine, nome, webPage, facebook, instagram, telefono, mail, comune_id, nome_comune, provincia FROM POI left join COMUNI on comune_id = COMUNI.id " +
                    $"WHERE POI.attivo = 'Y' ";
                if (id != null && id > 0)
                    sqlQuery += $" AND POI.id = {id}";

                //Debug.Log(sqlQuery);
                dbcmd.CommandText = sqlQuery;
                IDataReader _reader = dbcmd.ExecuteReader();
                while (_reader.Read())
                {
                    POI poi = new POI();
                    poi.ID = _reader.GetInt64(0);
                    double lo = _reader.GetFloat(1);
                    while (lo > 99)
                        lo /= 10;
                    poi.longitudine = lo;
                    double la = _reader.GetFloat(2);
                    while (la > 99)
                        la /= 10;
                    poi.latitudine = la;
                    poi.nome = !_reader.IsDBNull(3) ? Regex.Unescape(_reader.GetString(3)) : "";
                    poi.webPage = !_reader.IsDBNull(4) ? Regex.Unescape(_reader.GetString(4)) : "";
                    poi.facebook = !_reader.IsDBNull(5) ? Regex.Unescape(_reader.GetString(5)) : "";
                    poi.instagram = !_reader.IsDBNull(6) ? Regex.Unescape(_reader.GetString(6)) : "";
                    poi.telefono = !_reader.IsDBNull(7) ? Regex.Unescape(_reader.GetString(7)) : "";
                    poi.mail = !_reader.IsDBNull(8) ? Regex.Unescape(_reader.GetString(8)) : "";

                    poi.comune_id = _reader.GetInt32(9);
                    poi.comune = !_reader.IsDBNull(10) ? Regex.Unescape(_reader.GetString(10)) : "";
                    poi.provincia = !_reader.IsDBNull(11) ? Regex.Unescape(_reader.GetString(11)) : "";
                    ret.Add(poi);
                }

            }
            dbconn.Close();
        }
        return ret;
    }
    public List<POI> getPOI(int lingua_id, long? id = null, int? comune_id = null, string nome = null, int maxrow = 0, float? _latitudine = null, float? _longitudine = null, int? group_tipo_poi = null, int? tipo_poi = null, bool? get_images = null, bool? get_max_date_update = null, int? percorso_id = null, string? uuid = null)
    {
        List<POI> ret = new List<POI>();
        getConnection();
        if (Verbose)
            Debug.Log("Stablished connection to: " + conn);
        string _aggiorna_distanza = "";
        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open();
            if (Verbose)
                Debug.Log("Connection opened to: " + conn);
            using (var dbcmd = dbconn.CreateCommand())
            {

                sqlQuery = $"SELECT  POI.id, POI.longitudine, POI.latitudine, nome, webPage, facebook, instagram, telefono, mail, tag, descrizione, limite_zoom, istat, comune_id, nome_comune, provincia, " +
                    $" (SELECT COUNT(id) from PERCORSI where poi_id = POI.id and PERCORSI.attivo ='Y' and lingua_id = {lingua_id}), " +
                    $"POI.indirizzo, POI.visitabile, POI.distanza_dal_centro, " +
                    //                    $"ABS({SafeSubString(_latitudine.ToString().Replace(".", ""), 7).Replace(",","")} - substr(POI.latitudine, 0,7) + {SafeSubString( _longitudine.ToString().Replace(".", ""), 7).Replace(",", "")} - substr(POI.longitudine, 0,7))  as distance , " +
                    $"ABS({SafeSubString(_latitudine.ToString(), 7).Replace(",", ".")} - CAST(substr(POI.latitudine, 0,7) AS REAL) + {SafeSubString(_longitudine.ToString(), 7).Replace(",", ".")} - CAST(substr(POI.longitudine, 0,7) AS REAL))  as distance , " +
                    $"POI.mod_dte " +
                    "FROM POI " +
                    $"left join POI_TEXT on POI.id = POI_TEXT.poi_id  and POI_TEXT.lingua_id = {lingua_id} " +
                    "left join POIXTIPO on POI.id = POIXTIPO.poi_id " +
                    "left join TIPO_POI on POIXTIPO.tipo_id = TIPO_POI.id " +
                    "left join COMUNI on comune_id = COMUNI.id " +
                    $"WHERE POI.attivo = 'Y' AND COMUNI.attivo = 'Y' ";
                if (id != null && id > 0)
                    sqlQuery += $" AND POI.id = {id}";
                if (comune_id != null && comune_id > 0)
                    sqlQuery += $" AND comune_id= {comune_id}";
                if (!string.IsNullOrEmpty(nome))
                    sqlQuery += $" AND (nome like '%{nome}%' OR COMUNI.nome_comune like '%{nome}%')";
                if (group_tipo_poi != null && group_tipo_poi > 0)
                    sqlQuery += $" AND TIPO_POI.group_id = {group_tipo_poi}";
                if (tipo_poi != null && tipo_poi > 0)
                    sqlQuery += $" AND TIPO_POI.id = {tipo_poi}";
                if (percorso_id != null)
                    sqlQuery += $" and POI.id in (select poi_id from POIXTAPPE where tappa_id in (select tappa_id from TAPPEXPERCORSI where percorso_id = {percorso_id}))";
                if (!string.IsNullOrEmpty(uuid))
                    sqlQuery += $" AND POI.uuid like '%{uuid}%'";
                sqlQuery += $" GROUP BY POI.id ";
                if (get_max_date_update != null && get_max_date_update == true)
                    sqlQuery += $" ORDER BY POI.mod_dte DESC LIMIT 1";
                else
                {
                    sqlQuery += $" ORDER BY distance, distanza_dal_centro";
                    if (maxrow > 0)
                        sqlQuery += $" LIMIT {maxrow}";
                }


                Debug.Log(sqlQuery);
                dbcmd.CommandText = sqlQuery;
                IDataReader _reader = dbcmd.ExecuteReader();
                var _tipo_list = getPOIXTIPO(null, null);
                while (_reader.Read())
                {
                    POI poi = new POI();
                    poi.ID = _reader.GetInt64(0);
                    poi.tipoList = _tipo_list.Where(o => o.poi_id == poi.ID).ToList();//= _reader.GetInt32(1);
                    double lo = _reader.GetFloat(1);
                    while (lo > 99)
                        lo /= 10;
                    poi.longitudine = lo;
                    double la = _reader.GetFloat(2);
                    while (la > 99)
                        la /= 10;
                    poi.latitudine = la;
                    poi.nome = !_reader.IsDBNull(3) ? Regex.Unescape(_reader.GetString(3)) : "";
                    poi._text = getPOI_TEXT(lingua_id, poi.ID);
                    poi.webPage = !_reader.IsDBNull(4) ? Regex.Unescape(_reader.GetString(4)) : "";
                    poi.facebook = !_reader.IsDBNull(5) ? Regex.Unescape(_reader.GetString(5)) : "";
                    poi.instagram = !_reader.IsDBNull(6) ? Regex.Unescape(_reader.GetString(6)) : "";
                    poi.telefono = !_reader.IsDBNull(7) ? Regex.Unescape(_reader.GetString(7)) : "";
                    poi.mail = !_reader.IsDBNull(8) ? Regex.Unescape(_reader.GetString(8)) : "";

                    poi.tag = !_reader.IsDBNull(9) ? Regex.Unescape(_reader.GetString(9)) : "";
                    poi.nome_tipo = !_reader.IsDBNull(10) ? Regex.Unescape(_reader.GetString(10)) : "";
                    /*
                    int limite_zoom = 0;
                    string ap = !_reader.IsDBNull(11) ? Regex.Unescape(_reader.GetString(11) : "";
                    int.TryParse(ap, out limite_zoom);
                    */
                    poi.limite_zoom = !_reader.IsDBNull(11) ? _reader.GetInt32(11) : 0;

                    poi.istat = !_reader.IsDBNull(12) ? Regex.Unescape(_reader.GetString(12)) : "";
                    poi.comune_id = _reader.GetInt32(13);
                    poi.comune = !_reader.IsDBNull(14) ? Regex.Unescape(_reader.GetString(14)) : "";
                    poi.provincia = !_reader.IsDBNull(15) ? Regex.Unescape(_reader.GetString(15)) : "";
                    poi.percorsi_associati = _reader.GetInt32(16) > 0 ? true : false;
                    if (get_images != null && get_images == true)
                        poi._images = getPOI_IMMAGINI(null, poi.ID);
                    poi.indirizzo = !_reader.IsDBNull(17) ? Regex.Unescape(_reader.GetString(17)) : "";
                    poi.visitabile = !_reader.IsDBNull(18) ? Regex.Unescape(_reader.GetString(18)) : "";
                    poi.distanza_dal_centro = !_reader.IsDBNull(19) ? _reader.GetFloat(19) : 0f;
                    if (poi.distanza_dal_centro == 0f)
                    {
                        if (poi.latitudine > 0 && poi.longitudine > 0)
                        {
                            var _comune = getCOMUNI(lingua_id, null, null, poi.comune_id).FirstOrDefault();
                            la = _comune.latitudine;
                            while (la > 99)
                                la /= 10;
                            lo = _comune.longitudine;
                            while (lo > 99)
                                lo /= 10;

                            var _distanza = (float)(CalcolaDistanzaHaversine(la, lo, poi.latitudine, poi.longitudine));
                            _aggiorna_distanza += $"UPDATE POI SET distanza_dal_centro = {(int)_distanza} WHERE id = {poi.ID};";
                            poi.distanza_dal_centro = _distanza;
                        }
                    }
                    //string _date = !_reader.IsDBNull(21) ? Regex.Unescape(_reader.GetString(21) : "";
                    //DateTime.TryParse(_date, out poi.mod_dte);
                    poi.mod_dte = !_reader.IsDBNull(21) ? _reader.GetDateTime(21) : DateTime.MinValue;
                    ret.Add(poi);
                }

            }
            dbconn.Close();
            dbconn.Open();
            if (Verbose)
                Debug.Log("Connection opened to: " + conn);
            using (var dbcmd = dbconn.CreateCommand())
            {
                if (_aggiorna_distanza != "")
                {
                    dbcmd.CommandText = _aggiorna_distanza;
                    IDataReader _updater = dbcmd.ExecuteReader();
                }
            }
            dbconn.Close();
        }
        return ret;
    }
    private double CalcolaDistanzaHaversine(double lat1, double lon1, double lat2, double lon2)
    {
        // Raggio della Terra in metri (approssimativo)
        const double R = 6371000;

        // Converte gradi in radianti
        double radLat1 = lat1 * Math.PI / 180;
        double radLon1 = lon1 * Math.PI / 180;
        double radLat2 = lat2 * Math.PI / 180;
        double radLon2 = lon2 * Math.PI / 180;

        // Differenze
        double dLat = radLat2 - radLat1;
        double dLon = radLon2 - radLon1;

        // Formula Haversine
        double a = Math.Pow(Math.Sin(dLat / 2), 2) +
                   Math.Cos(radLat1) * Math.Cos(radLat2) *
                   Math.Pow(Math.Sin(dLon / 2), 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c; // Distanza in metri
    }
    public int getPOI_Count(int lingua_id, int? id = null, int? comune_id = null, string nome = null, int? group_tipo_poi = null, int? tipo_poi = null)
    {
        int ret = 0;
        getConnection();
        if (Verbose)
            Debug.Log("Stablished connection to: " + conn);

        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open();
            if (Verbose)
                Debug.Log("Connection opened to: " + conn);
            using (var dbcmd = dbconn.CreateCommand())
            {
                sqlQuery = $"SELECT COUNT(POI.id) " +
                    "FROM POI " +
                    $"left join POI_TEXT on POI.id = POI_TEXT.poi_id  and POI_TEXT.lingua_id = {lingua_id} " +
                    "left join POIXTIPO on POI.id = POIXTIPO.poi_id " +
                    "left join TIPO_POI on POIXTIPO.tipo_id = TIPO_POI.id " +
                    "left join COMUNI on comune_id = COMUNI.id " +
                    $"WHERE POI.attivo = 'Y'";
                if (id != null)
                    sqlQuery += $" AND POI.id = {id}";
                if (comune_id != null)
                    sqlQuery += $" AND comune_id= {comune_id}";
                if (!string.IsNullOrEmpty(nome))
                    sqlQuery += $" AND (nome like '%{nome}%' OR COMUNI.nome_comune like '%{nome}%')";
                if (group_tipo_poi != null && group_tipo_poi > 0)
                    sqlQuery += $" AND TIPO_POI.group_id = {group_tipo_poi}";
                if (tipo_poi != null && tipo_poi > 0)
                    sqlQuery += $" AND TIPO_POI.id = {tipo_poi}";
                //Debug.Log(sqlQuery);
                dbcmd.CommandText = sqlQuery;
                IDataReader _reader = dbcmd.ExecuteReader();
                while (_reader.Read())
                {
                    ret = _reader.GetInt32(0);
                }
            }
            dbconn.Close();
        }
        return ret;
    }
    public List<POI_TEXT> getPOI_TEXT(int lingua_id, double? poi_id = null)
    {
        List<POI_TEXT> ret = new List<POI_TEXT>();
        getConnection();
        if (Verbose)
            Debug.Log("Stablished connection to: " + conn);

        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open();
            if (Verbose)
                Debug.Log("Connection opened to: " + conn);
            using (var dbcmd = dbconn.CreateCommand())
            {
                sqlQuery = $"SELECT id, descrizione, descrizione_breve FROM POI_TEXT WHERE attivo = 'Y' ";
                if (poi_id != null && poi_id > 0)
                    sqlQuery += $" AND poi_id = {poi_id}";
                if (lingua_id > 0)
                    sqlQuery += $" AND lingua_id = {lingua_id}";
                dbcmd.CommandText = sqlQuery;
                IDataReader _reader = dbcmd.ExecuteReader();
                while (_reader.Read())
                {
                    POI_TEXT comune = new POI_TEXT();
                    comune.id = _reader.GetInt32(0);
                    comune.descrizione = !_reader.IsDBNull(1) ? Regex.Unescape(_reader.GetString(1)) : "";
                    comune.descrizione_breve = !_reader.IsDBNull(2) ? Regex.Unescape(_reader.GetString(2)) : "";
                    comune.lingua_id = lingua_id;


                    ret.Add(comune);
                }
            }
            dbconn.Close();
        }
        return ret;
    }
    public List<POI_IMMAGINI> getPOI_IMMAGINI(int? id = null, long? poi_id = null, bool? solo_principale = null)
    {
        List<POI_IMMAGINI> ret = new List<POI_IMMAGINI>();
        getConnection();
        if (Verbose)
            Debug.Log("Stablished connection to: " + conn);

        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open();
            if (Verbose)
                Debug.Log("Connection opened to: " + conn);
            using (var dbcmd = dbconn.CreateCommand())
            {
                sqlQuery = $"SELECT id, descrizione, image, principale FROM POI_IMMAGINI WHERE attivo = 'Y' ";
                if (id != null && id > 0)
                    sqlQuery += $" AND id = {id}";
                if (poi_id != null && poi_id > 0)
                    sqlQuery += $" AND poi_id = {poi_id}";
                if (solo_principale.HasValue && solo_principale.Value)
                    sqlQuery += $" AND principale = 1";
                sqlQuery += $" ORDER BY principale DESC ";
                dbcmd.CommandText = sqlQuery;
                IDataReader _reader = dbcmd.ExecuteReader();
                while (_reader.Read())
                {
                    POI_IMMAGINI poi_immagini = new POI_IMMAGINI();
                    poi_immagini.id = _reader.GetInt32(0);
                    poi_immagini.descrizione = !_reader.IsDBNull(1) ? Regex.Unescape(_reader.GetString(1)) : "";
                    if (!string.IsNullOrEmpty(_reader["image"].ToString()))
                        poi_immagini.image = System.Convert.FromBase64String(Regex.Unescape(Encoding.ASCII.GetString((byte[])_reader["image"]))); // ["image"];
                    poi_immagini.principale = (int)_reader["principale"] == 1 ? true : false;


                    ret.Add(poi_immagini);
                }
            }
            dbconn.Close();
        }
        return ret;
    }
    public List<TIPO_POI> getTIPO_POI(string tag = null, int? group_id = null, int? id = null)
    {
        List<TIPO_POI> ret = new List<TIPO_POI>();
        getConnection();
        if (Verbose)
            Debug.Log("Stablished connection to: " + conn);

        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open();
            if (Verbose)
                Debug.Log("Connection opened to: " + conn);
            using (var dbcmd = dbconn.CreateCommand())
            {
                sqlQuery = $"SELECT id, tag, limite_zoom, group_id FROM TIPO_POI WHERE attivo = 'Y' ";
                if (!string.IsNullOrEmpty(tag))
                    sqlQuery += $" AND tag = '{tag}'";
                if (group_id != null && group_id > 0)
                    sqlQuery += $" AND group_id = {group_id}";
                if (id != null && id > 0)
                    sqlQuery += $" AND id = {id}";
                dbcmd.CommandText = sqlQuery;
                IDataReader _reader = dbcmd.ExecuteReader();
                while (_reader.Read())
                {
                    TIPO_POI tipo_poi = new TIPO_POI();
                    tipo_poi.id = _reader.GetInt32(0);
                    tipo_poi.tag = !_reader.IsDBNull(1) ? Regex.Unescape(_reader.GetString(1)) : "";
                    tipo_poi.limite_zoom = _reader.GetInt32(2);
                    tipo_poi.group_id = _reader.GetInt32(3);

                    ret.Add(tipo_poi);
                }
            }
            dbconn.Close();
        }
        return ret;
    }
    public List<TIPO_POI_TEXT> getTIPO_POI_TEXT(int lingua_id, int? id = null)
    {
        List<TIPO_POI_TEXT> ret = new List<TIPO_POI_TEXT>();
        getConnection();
        if (Verbose)
            Debug.Log("Stablished connection to: " + conn);

        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open();
            if (Verbose)
                Debug.Log("Connection opened to: " + conn);
            using (var dbcmd = dbconn.CreateCommand())
            {
                sqlQuery = $"SELECT id, descrizione, lingua_id FROM TIPO_POI_TEXT WHERE lingua_id = {lingua_id}";
                if (id != null)
                    sqlQuery += $" AND id = {id}";
                dbcmd.CommandText = sqlQuery;
                IDataReader _reader = dbcmd.ExecuteReader();
                while (_reader.Read())
                {
                    TIPO_POI_TEXT tipo_poi = new TIPO_POI_TEXT();
                    tipo_poi.id = _reader.GetInt32(0);
                    tipo_poi.descrizione = !_reader.IsDBNull(1) ? Regex.Unescape(_reader.GetString(1)) : "";
                    tipo_poi.lingua_id = _reader.GetInt32(2);

                    ret.Add(tipo_poi);
                }
            }
            dbconn.Close();
        }
        return ret;
    }
    public List<GROUP_TIPO_POI> getGROUP_TIPO_POI(int? id = null)
    {
        List<GROUP_TIPO_POI> ret = new List<GROUP_TIPO_POI>();
        getConnection();
        if (Verbose)
            Debug.Log("Stablished connection to: " + conn);

        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open();
            if (Verbose)
                Debug.Log("Connection opened to: " + conn);
            using (var dbcmd = dbconn.CreateCommand())
            {
                sqlQuery = $"SELECT id, value, indice FROM GROUP_TIPO_POI WHERE attivo = 'Y' ";
                if (id != null)
                    sqlQuery += $" AND id = {id}";
                dbcmd.CommandText = sqlQuery;
                IDataReader _reader = dbcmd.ExecuteReader();
                while (_reader.Read())
                {
                    GROUP_TIPO_POI tipo_poi = new GROUP_TIPO_POI();
                    tipo_poi.id = _reader.GetInt32(0);
                    tipo_poi.value = !_reader.IsDBNull(1) ? Regex.Unescape(_reader.GetString(1)) : "";
                    tipo_poi.indice = !_reader.IsDBNull(2) ? _reader.GetInt32(2) : 0;


                    ret.Add(tipo_poi);
                }
            }
            dbconn.Close();
        }
        return ret;
    }
    public List<GROUP_TIPO_POI_TEXT> getGROUP_TIPO_POI_TEXT(int lingua_id, int? id = null, int? value = null)
    {
        List<GROUP_TIPO_POI_TEXT> ret = new List<GROUP_TIPO_POI_TEXT>();
        getConnection();
        if (Verbose)
            Debug.Log("Stablished connection to: " + conn);

        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open();
            if (Verbose)
                Debug.Log("Connection opened to: " + conn);
            using (var dbcmd = dbconn.CreateCommand())
            {
                sqlQuery = $"SELECT id, descrizione, lingua_id FROM GROUP_TIPO_POI_TEXT  WHERE lingua_id = {lingua_id}";
                if (id != null)
                    sqlQuery += $" AND id = {id}";
                if (value != null)
                    sqlQuery += $" AND group_tipo_poi_id  in (select id from GROUP_TIPO_POI where attivo ='Y' and value = {value})";
                dbcmd.CommandText = sqlQuery;
                IDataReader _reader = dbcmd.ExecuteReader();
                while (_reader.Read())
                {
                    GROUP_TIPO_POI_TEXT tipo_poi = new GROUP_TIPO_POI_TEXT();
                    tipo_poi.id = _reader.GetInt32(0);
                    tipo_poi.descrizione = !_reader.IsDBNull(1) ? Regex.Unescape(_reader.GetString(1)) : "";
                    tipo_poi.lingua_id = _reader.GetInt32(2);

                    ret.Add(tipo_poi);
                }
            }
            dbconn.Close();
        }
        return ret;
    }
    public List<POIXTIPO> getPOIXTIPO(double? id = null, long? poi_id = null, int? tipo_id = null)
    {
        List<POIXTIPO> ret = new List<POIXTIPO>();
        getConnection();
        if (Verbose)
            Debug.Log("Stablished connection to: " + conn);

        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open();
            if (Verbose)
                Debug.Log("Connection opened to: " + conn);

            using (var dbcmd = dbconn.CreateCommand())
            {
                sqlQuery = $"SELECT id, poi_id, tipo_id FROM POIXTIPO WHERE attivo = 'Y' ";
                if (id != null && id > 0)
                    sqlQuery += $" AND id = {id}";
                if (poi_id != null && poi_id > 0)
                    sqlQuery += $" AND poi_id = {poi_id}";
                if (tipo_id != null && tipo_id > 0)
                    sqlQuery += $" AND tipo_id = {tipo_id}";
                dbcmd.CommandText = sqlQuery;
                var list_tipo_poi = getTIPO_POI(null, null, null);
                IDataReader _reader = dbcmd.ExecuteReader();
                while (_reader.Read())
                {
                    POIXTIPO poixtipo = new POIXTIPO();
                    poixtipo.id = _reader.GetInt32(0);
                    poixtipo.poi_id = _reader.GetInt32(1);
                    int tipo_poi = _reader.GetInt32(2);
                    var list = list_tipo_poi.Where(o => o.id == tipo_poi).ToList();
                    if (list != null && list.Count > 0)
                        poixtipo.tipo = list[0];// _reader.GetInt32(2);


                    ret.Add(poixtipo);
                }

            }
            dbconn.Close();
        }
        return ret;
    }
    public List<SETTING> getSETTING(string tipo)
    {
        List<SETTING> ret = new List<SETTING>();
        getConnection();
        if (Verbose)
            Debug.Log("Stablished connection to: " + conn);

        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open();
            if (Verbose)
                Debug.Log("Connection opened to: " + conn);
            using (var dbcmd = dbconn.CreateCommand())
            {
                sqlQuery = $"SELECT * FROM SETTING ";
                if (!string.IsNullOrEmpty(tipo))
                    sqlQuery += $" WHERE tipo = '{tipo}'";
                dbcmd.CommandText = sqlQuery;
                IDataReader _reader = dbcmd.ExecuteReader();
                while (_reader.Read())
                {
                    SETTING setting = new SETTING();
                    setting.id = _reader.GetInt32(0);
                    setting.tipo = !_reader.IsDBNull(1) ? Regex.Unescape(_reader.GetString(1)) : "";
                    setting.valore = !_reader.IsDBNull(2) ? Regex.Unescape(_reader.GetString(2)) : "";

                    ret.Add(setting);
                }
            }
            dbconn.Close();
        }
        return ret;
    }
    public int InsertUpdateSETTING(string tipo, string valore)
    {
        getConnection();
        List<SETTING> ap = getSETTING(tipo);
        if (ap == null || ap.Count == 0)
        {
            sqlQuery = $"INSERT INTO SETTING (id, tipo, valore, attivo, mod_dte, uuid) values(IFNULL((select max(id) + 1 from SETTING), 1), '{tipo}', '{valore}', 'Y', DATETIME(datetime('now', 'localtime')), '{Guid.NewGuid()}')";
        }
        else
        {
            sqlQuery = $"UPDATE SETTING SET valore = '{valore}', mod_dte = DATETIME(datetime('now', 'localtime')) WHERE tipo = '{tipo}'";
        }
        int num_of_row_affected = 0;
        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open();
            if (Verbose)
                Debug.Log("Connection opened to: " + conn);
            using (var dbcmd = dbconn.CreateCommand())
            {
                Debug.Log(sqlQuery);
                dbcmd.CommandText = sqlQuery;
                num_of_row_affected = dbcmd.ExecuteNonQuery();
            }
            dbconn.Close();

        }
        return num_of_row_affected;
    }
    public List<LINGUA> getLINGUE(int? id = null)
    {
        List<LINGUA> ret = new List<LINGUA>();
        getConnection();
        if (Verbose)
            Debug.Log("Stablished connection to: " + conn);

        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open();
            if (Verbose)
                Debug.Log("Connection opened to: " + conn);
            using (var dbcmd = dbconn.CreateCommand())
            {
                sqlQuery = $"SELECT id, sigla, lingua FROM LINGUE WHERE attivo = 'Y' ";
                if (id != null && id > 0)
                    sqlQuery += $" AND id = {id}";
                dbcmd.CommandText = sqlQuery;
                IDataReader _reader = dbcmd.ExecuteReader();
                while (_reader.Read())
                {
                    LINGUA lingua = new LINGUA();
                    lingua.id = _reader.GetInt32(0);
                    lingua.sigla = !_reader.IsDBNull(1) ? Regex.Unescape(_reader.GetString(1)) : "";
                    lingua.lingua = !_reader.IsDBNull(2) ? Regex.Unescape(_reader.GetString(2)) : "";

                    ret.Add(lingua);
                }
            }
            dbconn.Close();
        }
        return ret;
    }
    public List<COMUNE> getCOMUNI(int lingua_id, string istat = null, string nome = null, int? id = null, float? _latitudine = null, float? _longitudine = null)
    {
        List<COMUNE> ret = new List<COMUNE>();
        getConnection();
        if (Verbose)
            Debug.Log("Stablished connection to: " + conn);

        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open();
            if (Verbose)
                Debug.Log("Connection opened to: " + conn);
            using (var dbcmd = dbconn.CreateCommand())
            {
                sqlQuery = $"SELECT id, istat, nome_comune, provincia, latitudine, longitudine, altitudine, abitanti, mod_dte, sito_turistico ";
                if (_latitudine.HasValue && _latitudine.Value > 0)
                    sqlQuery += $", ABS({SafeSubString(_latitudine.Value.ToString().Replace(".", ""), 7).Replace(",", "")} - substr(COMUNI.latitudine, 0,7) + {SafeSubString(_longitudine.Value.ToString().Replace(".", ""), 7).Replace(",", "")} - substr(COMUNI.longitudine, 0,7))  as distance ";
                sqlQuery += $" FROM COMUNI WHERE attivo = 'Y' ";
                if (id != null && id > 0)
                    sqlQuery += $" AND id = {id}";
                if (!string.IsNullOrEmpty(istat))
                    sqlQuery += $" AND istat = '{istat}'";
                if (!string.IsNullOrEmpty(nome))
                    sqlQuery += $" AND nome_comune like '%{nome}%'";
                sqlQuery += " ORDER BY ";
                //if (_latitudine.HasValue && _latitudine.Value > 0)
                //    sqlQuery += " distance ,";
                sqlQuery += " nome_comune";
                dbcmd.CommandText = sqlQuery;
                //Debug.Log(sqlQuery);
                IDataReader _reader = dbcmd.ExecuteReader();
                while (_reader.Read())
                {
                    //Debug.Log("getCOMUNI nel while 1");
                    COMUNE comune = new COMUNE();
                    comune.id = _reader.GetInt32(0);
                    //Debug.Log("getCOMUNI nel while 2");
                    comune.istat = !_reader.IsDBNull(1) ? Regex.Unescape(_reader.GetString(1)) : "";
                    //Debug.Log("getCOMUNI nel while 3");
                    comune.nome_comune = !_reader.IsDBNull(2) ? Regex.Unescape(_reader.GetString(2)) : "";
                    //Debug.Log("getCOMUNI nel while 4");
                    comune.provincia = !_reader.IsDBNull(3) ? Regex.Unescape(_reader.GetString(3)) : "";
                    //Debug.Log("getCOMUNI nel while 5");
                    comune.Listimages = getCOMUNI_IMMAGINI(null, comune.id);
                    //Debug.Log("getCOMUNI nel while 6_1");

                    try
                    {
                        string _s = !_reader.IsDBNull(4) ? Regex.Unescape(_reader.GetString(4)) : "";
                        //Debug.Log("getCOMUNI nel while 6_2");
                        float _l = 0;
                        //Debug.Log("getCOMUNI nel while 6_3");
                        float.TryParse(_s, out _l);
                        //Debug.Log("getCOMUNI nel while 6_4");
                        comune.latitudine = (double)_l;
                    }
                    catch
                    {
                        comune.latitudine = 0;
                    }

                    try
                    {
                        //Debug.Log("getCOMUNI nel while 7_1");
                        string _s = !_reader.IsDBNull(5) ? Regex.Unescape(_reader.GetString(5)) : "";
                        //Debug.Log("getCOMUNI nel while 7_2");
                        float _l = 0;
                        //Debug.Log("getCOMUNI nel while 7_3");
                        float.TryParse(_s, out _l);
                        //Debug.Log("getCOMUNI nel while 7_4");
                        comune.longitudine = (double)_l;
                    }
                    catch
                    {
                        comune.longitudine = 0;
                    }
                    //Debug.Log("getCOMUNI nel while 8");
                    comune.regione = "Marche";
                    //Debug.Log("getCOMUNI nel while 9");
                    comune.altitudine = _reader.GetFloat(6);
                    //Debug.Log("getCOMUNI nel while 10");
                    comune.abitanti = _reader.GetInt32(7);
                    //Debug.Log("getCOMUNI nel while 11");
                    comune.mod_dte = !_reader.IsDBNull(8) ? _reader.GetDateTime(8) : DateTime.MinValue;
                    comune.sito_turistico = !_reader.IsDBNull(9) ? _reader.GetString(9) : "";
                    ret.Add(comune);
                }
            }
            dbconn.Close();
        }
        //Debug.Log("getCOMUNI nel while 12");
        return ret;
    }
    public List<COMUNE_TEXT> getCOMUNI_TEXT(int lingua_id, int? comune_id = null)
    {
        List<COMUNE_TEXT> ret = new List<COMUNE_TEXT>();
        getConnection();
        if (Verbose)
            Debug.Log("Stablished connection to: " + conn);

        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open();
            if (Verbose)
                Debug.Log("Connection opened to: " + conn);
            using (var dbcmd = dbconn.CreateCommand())
            {
                sqlQuery = $"SELECT id, descrizione_breve, descrizione FROM COMUNI_TEXT WHERE attivo = 'Y' ";
                if (comune_id != null && comune_id > 0)
                    sqlQuery += $" AND comune_id = {comune_id}";
                if (lingua_id > 0)
                    sqlQuery += $" AND lingua_id = {lingua_id}";
                dbcmd.CommandText = sqlQuery;
                IDataReader _reader = dbcmd.ExecuteReader();
                while (_reader.Read())
                {
                    COMUNE_TEXT comune = new COMUNE_TEXT();
                    comune.id = _reader.GetInt32(0);
                    comune.descrizione_breve = !_reader.IsDBNull(1) ? Regex.Unescape(_reader.GetString(1)) : "";
                    comune.descrizione = !_reader.IsDBNull(2) ? Regex.Unescape(_reader.GetString(2)) : "";
                    comune.lingua_id = lingua_id;


                    ret.Add(comune);
                }
            }
            dbconn.Close();
        }
        return ret;
    }
    public List<COMUNE_IMMAGINI> getCOMUNI_IMMAGINI(int? id = null, int? comune_id = null, bool? solo_princioale = null)
    {
        List<COMUNE_IMMAGINI> ret = new List<COMUNE_IMMAGINI>();
        getConnection();
        if (Verbose)
            Debug.Log("Stablished connection to: " + conn);

        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open();
            if (Verbose)
                Debug.Log("Connection opened to: " + conn);
            using (var dbcmd = dbconn.CreateCommand())
            {
                sqlQuery = $"SELECT id, descrizione, image, principale FROM COMUNI_IMMAGINI WHERE attivo = 'Y' ";
                if (id != null && id > 0)
                    sqlQuery += $" AND id = {id}";
                if (comune_id != null && comune_id > 0)
                    sqlQuery += $" AND comune_id = {comune_id}";
                if (solo_princioale.HasValue && solo_princioale.Value)
                    sqlQuery += $" AND principale = 1";
                dbcmd.CommandText = sqlQuery;
                IDataReader _reader = dbcmd.ExecuteReader();
                while (_reader.Read())
                {
                    COMUNE_IMMAGINI comune_immagini = new COMUNE_IMMAGINI();
                    comune_immagini.id = _reader.GetInt32(0);
                    comune_immagini.descrizione = !_reader.IsDBNull(1) ? Regex.Unescape(_reader.GetString(1)) : "";
                    if (!string.IsNullOrEmpty(_reader["image"].ToString()))
                        comune_immagini.image = System.Convert.FromBase64String(Encoding.ASCII.GetString((byte[])_reader["image"])); // ["image"];
                    comune_immagini.principale = (int)_reader["principale"] == 1 ? true : false;


                    ret.Add(comune_immagini);
                }
            }
            dbconn.Close();
        }
        return ret;
    }
    public List<PERCORSO> getPERCORSI(int? lingua_id = null, int? id = null, long? poi_id = null, bool? groupedByCodice = null, int? comune_id = null, string nome = null, string tipo_percorso = null, string tipo_navigazione = null, bool? get_images = null)
    {
        List<PERCORSO> ret = new List<PERCORSO>();
        getConnection();
        if (Verbose)
            Debug.Log("Stablished connection to: " + conn);

        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open();
            if (Verbose)
                Debug.Log("Connection opened to: " + conn);
            using (var dbcmd = dbconn.CreateCommand())
            {
                sqlQuery = $"SELECT id, tipo_percorso, tipo_navigazione, nome_percorso, percorso, colore, poi_id, lunghezza, dislivello, " +
                    $"adatto_a, accessibilita, tempo_percorrenza, pendenza";
                sqlQuery += $" FROM PERCORSI WHERE attivo = 'Y' ";
                //if (lingua_id.HasValue && lingua_id > 0)
                //    sqlQuery += $" AND lingua_id = {lingua_id.Value}";
                if (id.HasValue && id > 0)
                    sqlQuery += $" AND id = {id.Value}";
                if (poi_id.HasValue && poi_id > 0)
                    sqlQuery += $" AND poi_id = {poi_id.Value}";
                if (comune_id.HasValue && comune_id > 0)
                    sqlQuery += $" AND poi_id IN( SELECT id from POI where attivo ='Y' and comune_id = {comune_id.Value})";
                if (!string.IsNullOrEmpty(nome))
                    sqlQuery += $" AND nome_percorso like '%{nome}%'";
                if (!string.IsNullOrEmpty(tipo_percorso))
                    sqlQuery += $" AND tipo_percorso = '{tipo_percorso}'";
                if (!string.IsNullOrEmpty(tipo_navigazione))
                    sqlQuery += $" AND tipo_navigazione = '{tipo_navigazione}'";
                if (groupedByCodice.HasValue && groupedByCodice.Value)
                    sqlQuery += $" GROUP BY codice ";
                dbcmd.CommandText = sqlQuery;
                if (Verbose)
                    Debug.Log(sqlQuery);
                IDataReader _reader = dbcmd.ExecuteReader();
                while (_reader.Read())
                {
                    PERCORSO percorso = new PERCORSO();
                    percorso.id = _reader.GetInt32(0);
                    percorso.tipo_percorso = !_reader.IsDBNull(1) ? Regex.Unescape(_reader.GetString(1)) : "";
                    percorso.tipo_navigazione = !_reader.IsDBNull(2) ? Regex.Unescape(_reader.GetString(2)) : "";
                    percorso.nome_percorso = !_reader.IsDBNull(3) ? Regex.Unescape(_reader.GetString(3)) : "";
                    percorso.percorso = !_reader.IsDBNull(4) ? Regex.Unescape(_reader.GetString(4)) : "";
                    percorso.colore = !_reader.IsDBNull(5) ? Regex.Unescape(_reader.GetString(5)) : "";
                    percorso.poi_id = _reader.GetInt32(6);
                    percorso.lunghezza = !_reader.IsDBNull(7) ? Regex.Unescape(_reader.GetString(7)) : "";
                    percorso.dislivello = !_reader.IsDBNull(8) ? Regex.Unescape(_reader.GetString(8)) : "";
                    percorso.adatto_a = !_reader.IsDBNull(9) ? Regex.Unescape(_reader.GetString(9)) : "";
                    percorso.accessibilita = !_reader.IsDBNull(10) ? Regex.Unescape(_reader.GetString(10)) : "";
                    percorso.tempo_percorrenza = !_reader.IsDBNull(11) ? Regex.Unescape(_reader.GetString(11)) : "";
                    percorso.pendenza = !_reader.IsDBNull(12) ? Regex.Unescape(_reader.GetString(12)) : "";
                    if (get_images != null && get_images == true)
                        percorso.Listimages = getPERCORSI_IMMAGINI(null, percorso.id);
                    if (lingua_id.HasValue)
                        percorso.descrizione = getPERCORSI_TEXT(lingua_id.Value, percorso.id);
                    ret.Add(percorso);
                }
            }
            dbconn.Close();
        }
        return ret;
    }
    public List<PERCORSO_TEXT> getPERCORSI_TEXT(int lingua_id, int? percorso_id = null)
    {
        List<PERCORSO_TEXT> ret = new List<PERCORSO_TEXT>();
        getConnection();
        if (Verbose)
            Debug.Log("Stablished connection to: " + conn);

        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open();
            if (Verbose)
                Debug.Log("Connection opened to: " + conn);
            using (var dbcmd = dbconn.CreateCommand())
            {
                sqlQuery = $"SELECT id, descrizione_breve, descrizione FROM PERCORSI_TEXT WHERE attivo = 'Y' ";
                if (percorso_id != null && percorso_id > 0)
                    sqlQuery += $" AND percorso_id = {percorso_id}";
                if (lingua_id > 0)
                    sqlQuery += $" AND lingua_id = {lingua_id}";
                dbcmd.CommandText = sqlQuery;
                IDataReader _reader = dbcmd.ExecuteReader();
                while (_reader.Read())
                {
                    PERCORSO_TEXT comune = new PERCORSO_TEXT();
                    comune.id = _reader.GetInt32(0);
                    comune.descrizione_breve = !_reader.IsDBNull(1) ? Regex.Unescape(_reader.GetString(1)) : "";
                    comune.descrizione = !_reader.IsDBNull(2) ? Regex.Unescape(_reader.GetString(2)) : "";
                    comune.lingua_id = lingua_id;


                    ret.Add(comune);
                }
            }
            dbconn.Close();
        }
        return ret;
    }
    public List<PERCORSO_IMMAGINI> getPERCORSI_IMMAGINI(int? id = null, int? percorso_id = null, bool? solo_princioale = null)
    {
        List<PERCORSO_IMMAGINI> ret = new List<PERCORSO_IMMAGINI>();
        getConnection();
        if (Verbose)
            Debug.Log("Stablished connection to: " + conn);

        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open();
            if (Verbose)
                Debug.Log("Connection opened to: " + conn);
            using (var dbcmd = dbconn.CreateCommand())
            {
                sqlQuery = $"SELECT id, descrizione, image, principale FROM PERCORSI_IMMAGINI WHERE attivo = 'Y' ";
                if (id != null && id > 0)
                    sqlQuery += $" AND id = {id}";
                if (percorso_id != null && percorso_id > 0)
                    sqlQuery += $" AND percorso_id = {percorso_id}";
                if (solo_princioale.HasValue && solo_princioale.Value)
                    sqlQuery += $" AND principale = 1";
                dbcmd.CommandText = sqlQuery;
                IDataReader _reader = dbcmd.ExecuteReader();
                while (_reader.Read())
                {
                    PERCORSO_IMMAGINI percorsi_immagini = new PERCORSO_IMMAGINI();
                    percorsi_immagini.id = _reader.GetInt32(0);
                    percorsi_immagini.descrizione = !_reader.IsDBNull(1) ? Regex.Unescape(_reader.GetString(1)) : "";
                    percorsi_immagini.image = System.Convert.FromBase64String(Regex.Unescape(Encoding.ASCII.GetString((byte[])_reader["image"]))); // ["image"];
                    percorsi_immagini.principale = (int)_reader["principale"] == 1 ? true : false;



                    ret.Add(percorsi_immagini);
                }
            }
            dbconn.Close();
        }
        return ret;
    }
    public List<POIXTAPPE> getPOIXTAPPE(int? id = null, long? poi_id = null, int? tappa_id = null, int? percorso_id = null)
    {
        List<POIXTAPPE> ret = new List<POIXTAPPE>();
        getConnection();
        if (Verbose)
            Debug.Log("Stablished connection to: " + conn);

        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open();
            if (Verbose)
                Debug.Log("Connection opened to: " + conn);
            using (var dbcmd = dbconn.CreateCommand())
            {
                sqlQuery = $"SELECT id, poi_id, tappa_id FROM POIXTAPPE WHERE attivo = 'Y' ";
                if (id != null && id > 0)
                    sqlQuery += $" AND id = {id}";
                if (poi_id != null && poi_id > 0)
                    sqlQuery += $" AND poi_id = {poi_id}";
                if (tappa_id != null && tappa_id > 0)
                    sqlQuery += $" AND tappa_id = {tappa_id}";
                if (percorso_id != null && percorso_id > 0)
                    sqlQuery += $" and tappa_id in (select tappa_id from TAPPEXPERCORSI where percorso_id = {percorso_id})";
                dbcmd.CommandText = sqlQuery;
                IDataReader _reader = dbcmd.ExecuteReader();
                while (_reader.Read())
                {
                    POIXTAPPE poixtappe = new POIXTAPPE();
                    poixtappe.id = _reader.GetInt32(0);
                    poixtappe.poi_id = _reader.GetInt32(1);
                    poixtappe.tappa_id = _reader.GetInt32(2);
                    ret.Add(poixtappe);
                }
            }
            dbconn.Close();
        }
        return ret;
    }
    public List<TAPPE> getTAPPE(int lingua_id, int? id = null)
    {
        List<TAPPE> ret = new List<TAPPE>();
        getConnection();
        if (Verbose)
            Debug.Log("Stablished connection to: " + conn);

        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open();
            if (Verbose)
                Debug.Log("Connection opened to: " + conn);
            using (var dbcmd = dbconn.CreateCommand())
            {
                sqlQuery = $"SELECT id, nome_tappa, colore, latitudine, longitudine FROM TAPPE WHERE attivo = 'Y' ";
                if (id != null && id > 0)
                    sqlQuery += $" AND id = {id}";
                dbcmd.CommandText = sqlQuery;
                IDataReader _reader = dbcmd.ExecuteReader();
                while (_reader.Read())
                {
                    TAPPE tappa = new TAPPE();
                    tappa.id = _reader.GetInt32(0);
                    tappa.nome_tappa = !_reader.IsDBNull(1) ? Regex.Unescape(_reader.GetString(1)) : "";
                    tappa.colore = !_reader.IsDBNull(2) ? Regex.Unescape(_reader.GetString(2)) : "";

                    double lo = _reader.GetFloat(3);
                    while (lo > 99)
                        lo /= 10;
                    tappa.latitudine = lo;
                    double la = _reader.GetFloat(4);
                    while (la > 99)
                        la /= 10;
                    tappa.longitudine = la;




                    tappa.tappe_text = getTAPPE_TEXT(lingua_id, null, tappa.id);
                    ret.Add(tappa);
                }
            }
            dbconn.Close();
        }
        return ret;
    }
    public List<TAPPE_TEXT> getTAPPE_TEXT(int lingua_id, int? id = null, int? tappa_id = null)
    {
        List<TAPPE_TEXT> ret = new List<TAPPE_TEXT>();
        getConnection();
        if (Verbose)
            Debug.Log("Stablished connection to: " + conn);

        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open();
            if (Verbose)
                Debug.Log("Connection opened to: " + conn);
            using (var dbcmd = dbconn.CreateCommand())
            {
                sqlQuery = $"SELECT id, descrizione, lingua_id, tappa_id FROM TAPPE_TEXT WHERE lingua_id = {lingua_id}";
                if (id != null)
                    sqlQuery += $" AND id = {id}";
                if (tappa_id != null)
                    sqlQuery += $" AND tappa_id = {tappa_id}";
                dbcmd.CommandText = sqlQuery;
                IDataReader _reader = dbcmd.ExecuteReader();
                while (_reader.Read())
                {
                    TAPPE_TEXT tipo_poi = new TAPPE_TEXT();
                    tipo_poi.id = _reader.GetInt32(0);
                    tipo_poi.descrizione = !_reader.IsDBNull(1) ? Regex.Unescape(_reader.GetString(1)) : "";
                    tipo_poi.lingua_id = _reader.GetInt32(2);
                    tipo_poi.tappa_id = _reader.GetInt32(3);

                    ret.Add(tipo_poi);
                }
            }
            dbconn.Close();
        }
        return ret;
    }
    public List<TAPPE_IMMAGINI> getTAPPE_IMMAGINI(int? id = null, int? tappa_id = null)
    {
        List<TAPPE_IMMAGINI> ret = new List<TAPPE_IMMAGINI>();
        getConnection();
        if (Verbose)
            Debug.Log("Stablished connection to: " + conn);

        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open();
            if (Verbose)
                Debug.Log("Connection opened to: " + conn);
            using (var dbcmd = dbconn.CreateCommand())
            {
                sqlQuery = $"SELECT id, tappa_id, image, descrizione FROM TAPPE_IMMAGINI ";
                string _where = string.Empty;
                if (id != null)
                    _where += $" id = {id}";
                if (tappa_id != null)
                {
                    if (!string.IsNullOrEmpty(_where))
                        _where += $" AND ";
                    _where += $"tappa_id = {tappa_id}";
                }
                if (!string.IsNullOrEmpty(_where))
                    sqlQuery += " WHERE " + _where;
                dbcmd.CommandText = sqlQuery;
                IDataReader _reader = dbcmd.ExecuteReader();
                while (_reader.Read())
                {
                    TAPPE_IMMAGINI tipo_poi = new TAPPE_IMMAGINI();
                    tipo_poi.id = _reader.GetInt32(0);
                    tipo_poi.tappa_id = _reader.GetInt32(1);
                    tipo_poi.image = (byte[])_reader["image"];
                    tipo_poi.descrizione = !_reader.IsDBNull(3) ? Regex.Unescape(_reader.GetString(3)) : "";

                    ret.Add(tipo_poi);
                }
            }
            dbconn.Close();
        }
        return ret;
    }


    public List<TAPPEXPERCORSI> getTAPPEXPERCORSI(int? id = null, int? tappa_id = null, int? percorso_id = null)
    {
        List<TAPPEXPERCORSI> ret = new List<TAPPEXPERCORSI>();
        getConnection();
        if (Verbose)
            Debug.Log("Stablished connection to: " + conn);

        using (dbconn = new SqliteConnection(conn))
        {
            dbconn.Open();
            if (Verbose)
                Debug.Log("Connection opened to: " + conn);
            using (var dbcmd = dbconn.CreateCommand())
            {
                sqlQuery = $"SELECT id, tappa_id, percorso_id, ordine FROM TAPPEXPERCORSI WHERE attivo = 'Y' ";
                if (id != null && id > 0)
                    sqlQuery += $" AND id = {id}";
                if (tappa_id != null && tappa_id > 0)
                    sqlQuery += $" AND tappa_id = {tappa_id}";
                if (percorso_id != null && percorso_id > 0)
                    sqlQuery += $" AND percorso_id = {percorso_id}";

                if (percorso_id != null && percorso_id > 0)
                    sqlQuery += $" ORDER BY ordine";
                dbcmd.CommandText = sqlQuery;
                IDataReader _reader = dbcmd.ExecuteReader();
                while (_reader.Read())
                {
                    TAPPEXPERCORSI tappaxpercorsi = new TAPPEXPERCORSI();
                    tappaxpercorsi.id = _reader.GetInt32(0);
                    tappaxpercorsi.tappa_id = _reader.GetInt32(1);
                    tappaxpercorsi.percorso_id = _reader.GetInt32(2);
                    tappaxpercorsi.ordine = _reader.GetInt32(3);
                    ret.Add(tappaxpercorsi);
                }
            }
            dbconn.Close();
        }
        return ret;
    }
    public void decompressImage(string nome_tabella, Texture2D tex, int id)
    {
        tex.Compress(false);
        Texture2D decopmpresseTex = tex.DeCompress();
        var bytes = decopmpresseTex.EncodeToPNG();
        setImageTOTable(nome_tabella, bytes, id);
    }
    public void setImageTOTable(string nome_tabella, byte[] arr, int id)
    {
        getConnection();
        SqliteConnection dbconn = new SqliteConnection(conn);
        SqliteCommand sqlQuery = new SqliteCommand($"UPDATE {nome_tabella} set image = @image, mod_dte = DATETIME(datetime('now', 'localtime')) WHERE id = {id}", dbconn);

        sqlQuery.Parameters.Add(new SqliteParameter()
        {
            ParameterName = "@image",
            Value = arr,
            DbType = System.Data.DbType.Binary
        });
        dbconn.Open();
        sqlQuery.ExecuteNonQuery();
        dbconn.Close();

    }
    public void exec_sql(string sql)
    {
        sql = sql.Replace('"', ' ');
        getConnection();
        SqliteConnection dbconn = new SqliteConnection(conn);
        SqliteCommand sqlQuery = new SqliteCommand(sql, dbconn);
        dbconn.Open();
        try
        {

            // Debug.Log("-----------"+sql);
            sqlQuery.ExecuteNonQuery();
        }
        catch (SqliteException ex)
        {
            Debug.Log("ERRORE QUERY: " + sql + "-----" + ex.ErrorCode);
        }
        dbconn.Close();
    }
}
public static class ExtensionMethod
{
    public static Texture2D DeCompress(this Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
                    source.width,
                    source.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }
}