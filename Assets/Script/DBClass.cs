using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static UnityEngine.XR.ARSubsystems.XRCpuImage;
//using static TouchScript.Behaviors.Cursors.UI.GradientTexture;
using ARLocation;
using ARLocation.MapboxRoutes;
using System.Text.RegularExpressions;
using System.Linq;
using Mapbox.Utils;
using static DBClass;
using UnityEngine.UI;
using System.Threading.Tasks;

public class DBClass : MonoBehaviour
{
    private static GameObject _createTable;
    private static CreateTable createTable;
    private void Start()
    {
        Application.targetFrameRate = 60; // O 120 per schermi moderni
        StartCoroutine(GetLatLonUsingGPS());
    }
    private void Awake()
    {
        _createTable = new GameObject("Cool GameObject made from Code");
        createTable = _createTable.AddComponent<CreateTable>();
        StartCoroutine(GetLatLonUsingGPS());
    }
    public class POI
    {
        public long ID;
        public string indirizzo;
        public string visitabile;
        public string tag;
        public List<POIXTIPO> tipoList;
        public double longitudine;
        public double latitudine;
        public float distanza_dal_centro;
        public string icona;
        public string nome;
        public List<POI_TEXT> _text;
        public List<POI_IMMAGINI> _images;
        public string webPage;
        public string facebook;
        public string instagram;
        public string telefono;
        public string mail;
        public string nome_tipo;
        public int limite_zoom;
        public string istat;
        public int comune_id;
        public string comune;
        public string provincia;
        public bool percorsi_associati;
        public double distanza_aria;
        public DateTime mod_dte;

        public string descrizione()
        {

            var ap = createTable.getPOI_TEXT(PlayerPrefs.GetInt("lingua_selezionata"), this.ID);
            if (ap == null)
                ap = createTable.getPOI_TEXT(1, this.ID);
            return ap.FirstOrDefault()?.descrizione;
        }
        public string descrizione_breve()
        {

            var ap = createTable.getPOI_TEXT(PlayerPrefs.GetInt("lingua_selezionata"), this.ID);
            if (ap == null)
                ap = createTable.getPOI_TEXT(1, this.ID);
            return ap.FirstOrDefault()?.descrizione_breve;
        }
        public string tipo_list_descrizione()
        {
            string ret = "";

            var _lingua_selezionata = PlayerPrefs.GetInt("lingua_selezionata");
            foreach (var ap in this.tipoList)
            {
                if (ap.tipo != null)
                {
                    var _ap = createTable.getTIPO_POI_TEXT(_lingua_selezionata, ap.tipo.id);
                    if (_ap != null && _ap.Count > 0 && !ret.Contains(_ap[0].descrizione))
                    {
                        if (!string.IsNullOrEmpty(ret))
                            ret = ret + ", ";
                        ret = ret + _ap[0].descrizione;
                    }
                }
            }
            return ret;
        }
    }
    public class POI_TEXT
    {
        public int id;
        public string descrizione;
        public string descrizione_breve;
        public int lingua_id;
    }
    public class POI_IMMAGINI
    {
        public int id;
        public byte[] image;
        public string descrizione;
        public bool principale;
    }
    public class POIXTIPO
    {
        public int id;
        public int poi_id;
        public TIPO_POI tipo;
    }
    public class TIPO_POI
    {
        public int id;
        public int group_id;
        public string tag;
        public int limite_zoom;
    }
    public class TIPO_POI_TEXT
    {
        public int id;
        public string descrizione;
        public int lingua_id;
    }
    public class GROUP_TIPO_POI
    {
        public int id;
        public int indice;
        public string value;
    }
    public class GROUP_TIPO_POI_TEXT
    {
        public int id;
        public string descrizione;
        public int lingua_id;
    }
    public class LINGUA
    {
        public int id;
        public string sigla;
        public string lingua;
    }
    public class PERCORSO
    {
        public long id;
        public string tipo_percorso;
        public string tipo_navigazione;
        public string nome_percorso;
        public string lunghezza;
        public string dislivello;
        public int lingua;
        public string percorso;
        public string colore;
        public string adatto_a;
        public string accessibilita;
        public string tempo_percorrenza;
        public string pendenza;
        public long poi_id;
        public List<PERCORSO_IMMAGINI> Listimages;
        public List<PERCORSO_TEXT> descrizione;
    }
    public class PERCORSO_TEXT
    {
        public int id;
        public string descrizione_breve;
        public string descrizione;
        public int lingua_id;
    }
    public class PERCORSO_IMMAGINI
    {
        public long id;
        public byte[] image;
        public string descrizione;
        public bool principale;
    }
    public class TAPPE
    {
        public long id;
        public string nome_tappa;
        public string colore;
        public double latitudine;
        public double longitudine;
        public List<TAPPE_TEXT> tappe_text;
    }
    public class TAPPE_TEXT
    {
        public int id;
        public int tappa_id;
        public string descrizione_breve;
        public string descrizione;
        public int lingua_id;
    }
    public class TAPPE_IMMAGINI
    {
        public int id;
        public int tappa_id;
        public byte[] image;
        public string descrizione;
        public bool principale;
    }
    public class POIXTAPPE
    {
        public int id;
        public long poi_id;
        public long tappa_id;
    }
    public class TAPPEXPERCORSI
    {
        public long id;
        public long tappa_id;
        public long percorso_id;
        public int ordine;
    }
    public class COMUNE
    {
        public int id;
        public string istat;
        public string nome_comune;
        public string provincia;
        public string regione;

        public double latitudine;
        public double longitudine;
        public float altitudine;
        public int abitanti;
        public string sito_turistico;
        public List<COMUNE_IMMAGINI> Listimages;




        public string descrizione()
        {
            string ret = string.Empty;

            var ap = createTable.getCOMUNI_TEXT(PlayerPrefs.GetInt("lingua_selezionata"), this.id);
            if (ap == null)
                ap = createTable.getCOMUNI_TEXT(1, this.id);
            if (ap != null && ap.Count > 0)
                ret = ap.FirstOrDefault().descrizione.Replace("`", "'");
            return ret;
        }
        public string descrizioneBreve()
        {
            string ret = string.Empty;

            var ap = createTable.getCOMUNI_TEXT(PlayerPrefs.GetInt("lingua_selezionata"), this.id);
            if (ap == null)
                ap = createTable.getCOMUNI_TEXT(1, this.id);
            if (ap != null && ap.Count > 0)
                ret = ap.FirstOrDefault().descrizione_breve;
            return ret;

        }
        public DateTime mod_dte;
    }
    public class COMUNE_TEXT
    {
        public int id;
        public string descrizione_breve;
        public string descrizione;
        public int lingua_id;
    }
    public class COMUNE_IMMAGINI
    {
        public int id;
        public byte[] image;
        public string descrizione;
        public bool principale;
    }

    public class SETTING
    {
        public int id;
        public string tipo;
        public string valore;
    }

    public string getLastUpdatedFromTable(string table)
    {
        var ret = "";
        if (table != "VERSIONE")
        {
            DateTime p = createTable.getLastUpdatedFromTable(table);
            ret = p.ToString("yyyy-MM-ddTHH:mm:ss");
        }
        else
            ret = createTable.getversione();
        return ret;
    }
    public List<POI> getPOIxMap(string? not_in = "", double[] punti = null)
    {
        StartCoroutine(GetLatLonUsingGPS());
        int? comune_id = null;
        if (PlayerPrefs.HasKey("comune_selected") && PlayerPrefs.GetInt("comune_selected") > 0)
            comune_id = PlayerPrefs.GetInt("comune_selected");
        var ap = createTable.getPOIxMap(not_in, punti, comune_id);
        return ap;
    }
    public List<POI> getPOIGeneralita(long? id = null)
    {
        var ap = createTable.getPOIGeneralita(id);
        return ap;
    }
    public List<POI> getPOI(long? id = null, int? comune_id = null, string nome = null, int maxrow = 0, int? group_tipo_poi = null, int? tipo_poi = null, bool? get_images = null, bool? get_max_date_update = null, long? percorso_id = null, string? uuid = null)
    {
        StartCoroutine(GetLatLonUsingGPS());
        if (PlayerPrefs.HasKey("comune_selected") && PlayerPrefs.GetInt("comune_selected") > 0)
            comune_id = PlayerPrefs.GetInt("comune_selected");

        var ap = createTable.getPOI(PlayerPrefs.GetInt("lingua_selezionata"), id, comune_id, nome, maxrow, _latitudine, _longitudine, group_tipo_poi, tipo_poi, get_images, get_max_date_update, percorso_id, uuid);

        return ap;
    }
    public int getPOI_Count(int? id = null, int? comune_id = null, string nome = null, int? group_tipo_poi = null, int? tipo_poi = null)
    {
        //Debug.Log(DateTime.Now);

        var ap = createTable.getPOI_Count(PlayerPrefs.GetInt("lingua_selezionata"), id, comune_id, nome, group_tipo_poi, tipo_poi);
        //Debug.Log(DateTime.Now);
        return ap;
    }
    public List<POI_TEXT> getPOI_TEXT(int? poi_id = null)
    {

        return createTable.getPOI_TEXT(PlayerPrefs.GetInt("lingua_selezionata"), poi_id);
    }

    public List<TIPO_POI> getTIPO_POI(string tag = null, int? group_id = null)
    {

        return createTable.getTIPO_POI(tag, group_id);
    }

    public List<TIPO_POI_TEXT> getTIPO_POI_TEXT(int? id = null)
    {

        return createTable.getTIPO_POI_TEXT(PlayerPrefs.GetInt("lingua_selezionata"), id);
    }

    public List<SETTING> getSETTING(string tipo)
    {

        return createTable.getSETTING(tipo);
    }
    public int SetSetting(string tipo, string valore)
    {

        return createTable.InsertUpdateSETTING(tipo, valore);
    }

    //lingua selezionata
    public int getSetting_LinguaSelezionata()
    {
        int ret = 0;
        var app_settings = getSETTING("lingua_selezionata");
        if (app_settings != null && app_settings.Count > 0)
        {
            if (int.TryParse(app_settings[0].valore, out ret))
            {
                PlayerPrefs.SetInt("lingua_selezionata", ret);
            }
        }

        return ret;
    }
    public void setSetting_LinguaSelezionata(int lingua_selezionata)
    {
        SetSetting("lingua_selezionata", lingua_selezionata.ToString());
        PlayerPrefs.SetInt("lingua_selezionata", lingua_selezionata);
    }

    public List<PERCORSO> GetPERCORSO(long? id = null, long? poi_id = null, bool? groupedByCodice = null, int? comune_id = null, string nome = null, string tipo_percorso = null, string tipo_navigazione = null, bool? get_images = null)
    {
        if (PlayerPrefs.HasKey("comune_selected") && PlayerPrefs.GetInt("comune_selected") > 0)
            comune_id = PlayerPrefs.GetInt("comune_selected");
        return createTable.getPERCORSI(PlayerPrefs.GetInt("lingua_selezionata"), id, poi_id, groupedByCodice, comune_id, nome, tipo_percorso, tipo_navigazione, get_images);
    }
    public List<PERCORSO_IMMAGINI> getPERCORSI_IMMAGINI(int? id = null, long? percorso_id = null)
    {

        return createTable.getPERCORSI_IMMAGINI(id, percorso_id);
    }
    public List<COMUNE> GetCOMUNI(string istat, string nome = null, int? id = null, bool? checkuserposition = null, bool? check_all = null)
    {
        if (check_all == null || check_all == false)
        {
            if (PlayerPrefs.HasKey("comune_selected") && PlayerPrefs.GetInt("comune_selected") > 0)
                id = PlayerPrefs.GetInt("comune_selected");
        }
        //Debug.Log(checkuserposition);
        if (checkuserposition.HasValue && checkuserposition.Value)
        {
            StartCoroutine(GetLatLonUsingGPS());
        }
        return createTable.getCOMUNI(PlayerPrefs.GetInt("lingua_selezionata"), istat, nome, id, (float?)_latitudine, (float?)_longitudine);
    }
    public List<COMUNE_IMMAGINI> getCOMUNI_IMMAGINI(int? id = null, int? comune_id = null)
    {

        return createTable.getCOMUNI_IMMAGINI(id, comune_id);
    }
    public List<POI_IMMAGINI> getPOI_IMMAGINI(int? id = null, long? poi_id = null, bool? solo_principale = null)
    {

        return createTable.getPOI_IMMAGINI(id, poi_id, solo_principale);
    }
    public List<POIXTAPPE> getPOIXTAPPE(int? id = null, long? poi_id = null, long? tappa_id = null, long? percorso_id = null)
    {
        if (createTable == null)
            Awake();
        return createTable.getPOIXTAPPE(id, poi_id, tappa_id, percorso_id);
    }

    public List<TAPPEXPERCORSI> getTAPPEXPERCORSI(int? id = null, long? tappa_id = null, long? percorso_id = null)
    {

        return createTable.getTAPPEXPERCORSI(id, tappa_id, percorso_id);
    }

    public List<TAPPE> getTAPPE(int lingua_id, long? id = null)
    {

        return createTable.getTAPPE(lingua_id, id);
    }
    public List<GROUP_TIPO_POI> getGROUP_TIPO_POI(int? id = null)
    {

        return createTable.getGROUP_TIPO_POI(id);
    }
    public List<GROUP_TIPO_POI_TEXT> getGROUP_TIPO_POI_TEXT(int? id = null, int? value = null)
    {

        return createTable.getGROUP_TIPO_POI_TEXT(PlayerPrefs.GetInt("lingua_selezionata"), id, value);
    }


    public void CreateDB(Slider slider)
    {
        _createTable = new GameObject("Cool GameObject made from Code");
        createTable = _createTable.AddComponent<CreateTable>();
        createTable.CreateDB(true, slider);
    }
    public void RemovePersistent_DB(Slider slider, Slider sliderchunk, Button italiano, Button inglese, Toggle NonChiedereNuovamente, Canvas Canvas_DB_Corrotto, Canvas Canvas_Errore_connessione, Canvas Canvas_Manca_Spazio, Canvas Canvas_Prompt_Download, Text Testo_Info_Download, Button Bottone_Conferma, Button Bottone_Annulla)
    {
        _createTable = new GameObject("Cool GameObject made from Code");
        createTable = _createTable.AddComponent<CreateTable>();
        createTable.RemovePersistent_DB();
        createTable.copyDB(slider, sliderchunk, italiano, inglese, NonChiedereNuovamente, Canvas_DB_Corrotto, Canvas_Errore_connessione, Canvas_Manca_Spazio, Canvas_Prompt_Download, Testo_Info_Download, Bottone_Conferma, Bottone_Annulla);
    }
    public void PersistentToStraming_DB()
    {
        _createTable = new GameObject("Cool GameObject made from Code");
        createTable = _createTable.AddComponent<CreateTable>();
        createTable.PersistentToStraming_DB();
    }
    public void UpdateDB(Slider slider)
    {
        _createTable = new GameObject("Cool GameObject made from Code");
        createTable = _createTable.AddComponent<CreateTable>();
        createTable.CreateDB(false, slider);
    }


    public List<CustomRoute> ListOfCustomRoute(long? id = null, int? poi_id = null, bool? groupedByCodice = null)
    {
        //costruisco un Custom route
        return GetCustomRoute(GetPERCORSO(id, poi_id, groupedByCodice));
    }

    public List<CustomRoute> GetCustomRoute(List<PERCORSO> _PERCORSOList)
    {
        List<CustomRoute> _ret = new List<CustomRoute>();
        if (_PERCORSOList != null)
        {
            foreach (var _PERCORSO in _PERCORSOList)
            {
                if (_PERCORSO.percorso.Contains("Points:"))
                {
                    CustomRoute ret = (CustomRoute)ScriptableObject.CreateInstance(typeof(CustomRoute));
                    //                CustomRoute ret = new CustomRoute();
                    ret.Name = "tempCustomRoute";
                    ret.Points = new List<CustomRoute.Point>();
                    ret.Color = _PERCORSO.colore;
                    List<string> PointSection = Regex.Split(_PERCORSO.percorso, "Points:").ToList();
                    List<string> ListOfLocations = Regex.Split(PointSection[1], "- Location:").ToList();

                    foreach (var ListOfPoint in ListOfLocations)
                    {
                        CustomRoute.Point _point = new CustomRoute.Point();
                        double _latitude = 0;
                        double _longitude = 0;
                        double _altitude = 0;
                        int _altitudeMode = 0;
                        string _label = "";
                        foreach (var ListOfElements in Regex.Split(ListOfPoint, "\n"))
                        {
                            if (ListOfElements.Contains("IsStep:"))
                            {
                                _point.IsStep = false;
                                var isStep = Regex.Split(ListOfElements, "IsStep:");
                                if (isStep[1].Contains("1"))
                                    _point.IsStep = true;
                            }
                            if (ListOfElements.Contains("IsTappa:"))
                            {
                                _point.IsTappa = false;
                                var isTappa = Regex.Split(ListOfElements, "IsTappa:");
                                if (isTappa[1].Contains("1"))
                                    _point.IsTappa = true;
                            }
                            if (ListOfElements.Contains("Name:"))
                            {
                                _point.Name = "";
                                var _Name = Regex.Split(ListOfElements, "Name:");
                                _point.Name = _Name[1];
                            }
                            if (ListOfElements.Contains("Instruction:"))
                            {
                                _point.Instruction = "";
                                var Instruction = Regex.Split(ListOfElements, "Instruction:");
                                _point.Instruction = Instruction[1];
                            }
                            if (ListOfElements.Contains("Latitude:"))
                            {
                                var Latitude = Regex.Split(ListOfElements, "Latitude:");
                                double la = double.Parse(Latitude[1].Replace(".", "").Replace(",", ""));
                                while (la > 99)
                                    la /= 10;
                                _latitude = la;
                            }
                            if (ListOfElements.Contains("Longitude:"))
                            {
                                var Longitude = Regex.Split(ListOfElements, "Longitude:");
                                double lo = double.Parse(Longitude[1].Replace(".", "").Replace(",", ""));
                                while (lo > 99)
                                    lo /= 10;
                                _longitude = lo;
                            }
                            if (ListOfElements.Contains("Altitude:"))
                            {
                                var Altitude = Regex.Split(ListOfElements, "Altitude:");
                                _altitude = double.Parse(Altitude[1]);
                            }
                            if (ListOfElements.Contains("AltitudeMode:"))
                            {
                                var AltitudeMode = Regex.Split(ListOfElements, "AltitudeMode:");
                                _altitudeMode = int.Parse(AltitudeMode[1]);
                            }
                            if (ListOfElements.Contains("Label:"))
                            {
                                var Label = Regex.Split(ListOfElements, "Label:");
                                _label = Label[1];
                            }
                        }
                        if (_latitude != 0 && _longitude != 0)
                        {
                            Location _loc = new Location(_latitude, _longitude, _altitude);
                            _loc.AltitudeMode = (AltitudeMode)_altitudeMode;
                            _loc.Label = _label;
                            _point.Location = _loc;
                            ret.Points.Add(_point);
                        }
                    }
                    _ret.Add(ret);
                }
            }
        }

        return _ret;
    }
    public Sprite getSpriteFromByteArray(byte[] bytes)
    {
        var tex = new Texture2D(1, 1); // note that the size is overridden
        if (bytes != null && bytes.Length > 0)
            tex.LoadImage(bytes);
        else
            tex = Resources.Load<Texture2D>("Foto/no_images");
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(tex.width / 2, tex.height / 2));
    }
    /*
    public Sprite getImage(byte[] arr)
    {
        Texture2D texture = new Texture2D(1, 1);
        if (arr != null && arr.Length > 0)
            texture.LoadImage(arr, true);
        else
            texture = Resources.Load<Texture2D>("Foto/no_images");
        //texture.Apply();
        return Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
    }
    */
    public Vector2d VectorFromLonLat(double longitudine, double latitudine)
    {
        return new Vector2d(latitudine, longitudine);
    }


    public void setImageTOComuni(byte[] arr, int comune_id)
    {

        createTable.setImageTOTable("COMUNI_IMMAGINI", arr, comune_id);
    }
    public void setImageTOPOI(byte[] arr, int poi_id)
    {

        createTable.setImageTOTable("POI_IMMAGINI", arr, poi_id);
    }
    public void setImageTOPERCORSI(byte[] arr, int percorso_id)
    {

        createTable.setImageTOTable("PERCORSI_IMMAGINI", arr, percorso_id);
    }

    public void exec_sql(string sql)
    {
        createTable.exec_sql(sql);
    }

    public double _longitudine;
    public double _latitudine;

    public IEnumerator GetLatLonUsingGPS()
    {
        //Input.location.Start();
        int maxWait = 5;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        //Debug.Log("waiting before getting lat and lon");

        // Access granted and location value could be retrieve
        _longitudine = Input.location.lastData.longitude;
        _latitudine = Input.location.lastData.latitude;

        PlayerPrefs.SetString("_latitudine", _latitudine.ToString());
        PlayerPrefs.SetString("_longitudine", _longitudine.ToString());
        //AddLocation(latitude, longitude);

        if (Input.location.status == LocationServiceStatus.Stopped && _latitudine == 0)
        {
            if (PlayerPrefs.HasKey("_longitudine"))
                double.TryParse(PlayerPrefs.GetString("_longitudine"), out _longitudine);
            else
                _longitudine = 13.0091695785522;

            if (PlayerPrefs.HasKey("_latitudine"))
                double.TryParse(PlayerPrefs.GetString("_latitudine"), out _latitudine);
            else
                _latitudine = 43.2534828186035;

        }
        //        Debug.Log(Input.location.status + "  lat:" + _latitudine + "  long:" + _longitudine);
        //Stop retrieving location
        //Input.location.Stop();
    }

    public string pulisciHTML(string testo)
    {
        return createTable.pulisciHTML(testo);
    }
    public void copyDB(Slider loadingBar, Slider loadingChunkBar, Button italiano, Button inglese, Toggle NonChiedereNuovamente, Canvas Canvas_DB_Corrotto, Canvas Canvas_Errore_connessione, Canvas Canvas_Manca_Spazio, Canvas Canvas_Prompt_Download, Text Testo_Info_Download, Button Bottone_Conferma, Button Bottone_Annulla)
    {
        _createTable = new GameObject("Cool GameObject made from Code");
        createTable = _createTable.AddComponent<CreateTable>();
        createTable.copyDB(loadingBar, loadingChunkBar, italiano, inglese, NonChiedereNuovamente, Canvas_DB_Corrotto, Canvas_Errore_connessione, Canvas_Manca_Spazio, Canvas_Prompt_Download, Testo_Info_Download, Bottone_Conferma, Bottone_Annulla);
    }
    public float CalculateDistance(double lat_1, double lat_2, double long_1, double long_2)
    {
        return createTable.CalculateDistance((float)lat_1, (float)lat_2, (float)long_1, (float)long_2);
    }
}

