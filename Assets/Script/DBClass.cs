using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static UnityEngine.XR.ARSubsystems.XRCpuImage;
using static TouchScript.Behaviors.Cursors.UI.GradientTexture;
using ARLocation;
using ARLocation.MapboxRoutes;
using System.Text.RegularExpressions;
using System.Linq;
using Mapbox.Utils;
using static DBClass;

public class DBClass : MonoBehaviour
{

    private void Start()
    {
        StartCoroutine(GetLatLonUsingGPS());
    }
    public class POI
    {
        public int ID;
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

        public string descrizione()
        {
            CreateTable ct = ScriptableObject.CreateInstance<CreateTable>();
            var ap = ct.getPOI_TEXT(PlayerPrefs.GetInt("lingua_selezionata"), this.ID);
            if (ap == null)
                ap = ct.getPOI_TEXT(1, this.ID);
            return ap.FirstOrDefault()?.descrizione;
        }
        public string descrizione_breve()
        {
            CreateTable ct = ScriptableObject.CreateInstance<CreateTable>();
            var ap = ct.getPOI_TEXT(PlayerPrefs.GetInt("lingua_selezionata"), this.ID);
            if (ap == null)
                ap = ct.getPOI_TEXT(1, this.ID);
            return ap.FirstOrDefault()?.descrizione_breve;
        }
        public string tipo_list_descrizione()
        {
            string ret = "";
            CreateTable ct = ScriptableObject.CreateInstance<CreateTable>();
            var _lingua_selezionata = PlayerPrefs.GetInt("lingua_selezionata");
            foreach (var ap in this.tipoList)
            {
                if (!string.IsNullOrEmpty(ret))
                    ret = ret + ", ";
                var _ap = ct.getTIPO_POI_TEXT(_lingua_selezionata, ap.tipo.id);
                if (_ap != null && _ap.Count > 0)
                    ret = ret + _ap[0].descrizione;
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
        public int value;
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
        public int id;
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
        public int poi_id;
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
        public int id;
        public byte[] image;
        public string descrizione;
        public bool principale;
    }
    public class TAPPE
    {
        public int id;
        public string nome_tappa;
        public string colore;
        public double latitudine;
        public double longitudine;
    }
    public class TAPPE_TEXT
    {
        public int id;
        public string descrizione_breve;
        public string descrizione;
        public int lingua_id;
    }
    public class TAPPE_IMMAGINI
    {
        public int id;
        public byte[] image;
        public string descrizione;
        public bool principale;
    }
    public class POIXTAPPE
    {
        public int id;
        public int poi_id;
        public int tappa_id;
    }
    public class TAPPEXPERCORSI
    {
        public int id;
        public int tappa_id;
        public int percorso_id;
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
        public double altitudine;
        public int abitanti;
        public List<COMUNE_IMMAGINI> Listimages;




        public string descrizione()
        {
            string ret = string.Empty;
            CreateTable ct = ScriptableObject.CreateInstance<CreateTable>();
            var ap = ct.getCOMUNI_TEXT(PlayerPrefs.GetInt("lingua_selezionata"), this.id);
            if (ap == null)
                ap = ct.getCOMUNI_TEXT(1, this.id);
            if(ap != null && ap.Count > 0)
                ret = ap.FirstOrDefault().descrizione;
            return ret;
        }
        public string descrizioneBreve()
        {
            string ret = string.Empty;
            CreateTable ct = ScriptableObject.CreateInstance<CreateTable>();
            var ap = ct.getCOMUNI_TEXT(PlayerPrefs.GetInt("lingua_selezionata"), this.id);
            if (ap == null)
                ap = ct.getCOMUNI_TEXT(1, this.id);
            if (ap != null && ap.Count > 0)
                ret = ap.FirstOrDefault().descrizione_breve;
            return ret;

        }
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

    public List<POI> getPOI(int? id = null, int? comune_id = null, string nome = null, int maxrow = 0, int? group_tipo_poi = null, int? tipo_poi = null)
    {
        //Debug.Log(DateTime.Now);
        CreateTable ct = ScriptableObject.CreateInstance<CreateTable>();
        StartCoroutine(GetLatLonUsingGPS());
        var ap = ct.getPOI(PlayerPrefs.GetInt("lingua_selezionata"), id, comune_id, nome, maxrow, (float?)_latitudine, (float?)_longitudine, group_tipo_poi, tipo_poi);
        //Debug.Log(DateTime.Now);
        return ap;
    }
    public int getPOI_Count(int? id = null, int? comune_id = null, string nome = null, int? group_tipo_poi = null, int? tipo_poi = null)
    {
        //Debug.Log(DateTime.Now);
        CreateTable ct = ScriptableObject.CreateInstance<CreateTable>();
        var ap = ct.getPOI_Count(PlayerPrefs.GetInt("lingua_selezionata"), id, comune_id, nome, group_tipo_poi, tipo_poi);
        //Debug.Log(DateTime.Now);
        return ap;
    }
    public List<POI_TEXT> getPOI_TEXT(int? poi_id = null)
    {
        CreateTable ct = ScriptableObject.CreateInstance<CreateTable>();
        return ct.getPOI_TEXT(PlayerPrefs.GetInt("lingua_selezionata"), poi_id);
    }

    public List<TIPO_POI> getTIPO_POI(int? id = null, int? group_id = null)
    {
        CreateTable ct = ScriptableObject.CreateInstance<CreateTable>();
        return ct.getTIPO_POI(id, group_id);
    }

    public List<TIPO_POI_TEXT> getTIPO_POI_TEXT(int? id = null)
    {
        CreateTable ct = ScriptableObject.CreateInstance<CreateTable>();
        return ct.getTIPO_POI_TEXT(PlayerPrefs.GetInt("lingua_selezionata"), id);
    }

    public List<SETTING> getSETTING(string tipo)
    {
        CreateTable ct = ScriptableObject.CreateInstance<CreateTable>();
        return ct.getSETTING(tipo);
    }
    public int SetSetting(string tipo, string valore)
    {
        CreateTable ct = ScriptableObject.CreateInstance<CreateTable>();
        return ct.InsertUpdateSETTING(tipo, valore);
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

    public List<PERCORSO> GetPERCORSO(int? id = null, int? poi_id = null, bool? groupedByCodice = null, int? comune_id = null, string nome = null, string tipo_percorso = null, string tipo_navigazione = null)
    {
        CreateTable ct = ScriptableObject.CreateInstance<CreateTable>();
        return ct.getPERCORSI(PlayerPrefs.GetInt("lingua_selezionata"), id, poi_id, groupedByCodice, comune_id, nome, tipo_percorso, tipo_navigazione);
    }
    public List<PERCORSO_IMMAGINI> getPERCORSI_IMMAGINI(int? id = null, int? percorso_id = null)
    {
        CreateTable ct = ScriptableObject.CreateInstance<CreateTable>();
        return ct.getPERCORSI_IMMAGINI(id, percorso_id);
    }
    public List<COMUNE> GetCOMUNI(string istat, string nome = null, int? id = null, bool? checkuserposition = null)
    {
        CreateTable ct = ScriptableObject.CreateInstance<CreateTable>();
        //Debug.Log(checkuserposition);
        if (checkuserposition.HasValue && checkuserposition.Value)
        {
            StartCoroutine(GetLatLonUsingGPS());
        }
        return ct.getCOMUNI(PlayerPrefs.GetInt("lingua_selezionata"), istat, nome, id, (float?)_latitudine, (float?)_longitudine);
    }
    public List<COMUNE_IMMAGINI> getCOMUNI_IMMAGINI(int? id = null, int? comune_id = null)
    {
        CreateTable ct = ScriptableObject.CreateInstance<CreateTable>();
        return ct.getCOMUNI_IMMAGINI(id, comune_id);
    }
    public List<POI_IMMAGINI> getPOI_IMMAGINI(int? id = null, int? poi_id = null)
    {
        CreateTable ct = ScriptableObject.CreateInstance<CreateTable>();
        return ct.getPOI_IMMAGINI(id, poi_id);
    }
    public List<POIXTAPPE> getPOIXTAPPE(int? id = null, int? poi_id = null, int? tappa_id = null)
    {
        CreateTable ct = ScriptableObject.CreateInstance<CreateTable>();
        return ct.getPOIXTAPPE(id, poi_id, tappa_id);
    }

    public List<TAPPEXPERCORSI> getTAPPEXPERCORSI(int? id = null, int? tappa_id = null, int? percorso_id = null)
    {
        CreateTable ct = ScriptableObject.CreateInstance<CreateTable>();
        return ct.getTAPPEXPERCORSI(id, tappa_id, percorso_id);
    }


    public List<GROUP_TIPO_POI> getGROUP_TIPO_POI(int? id = null)
    {
        CreateTable ct = ScriptableObject.CreateInstance<CreateTable>();
        return ct.getGROUP_TIPO_POI(id);
    }
    public List<GROUP_TIPO_POI_TEXT> getGROUP_TIPO_POI_TEXT(int? id = null, int? value = null)
    {
        CreateTable ct = ScriptableObject.CreateInstance<CreateTable>();
        return ct.getGROUP_TIPO_POI_TEXT(PlayerPrefs.GetInt("lingua_selezionata"), id, value);
    }


    public void CreateDB(bool reset_db)
    {
        CreateTable ct = ScriptableObject.CreateInstance<CreateTable>();
        ct.CreateDB(reset_db);
    }
    

    public List<CustomRoute> ListOfCustomRoute(int? id = null, int? poi_id = null, bool? groupedByCodice = null)
    {
        //costruisco un Custom route
        return GetCustomRoute(GetPERCORSO(id, poi_id, groupedByCodice));
    }

    public List<CustomRoute> GetCustomRoute(List<PERCORSO> _PERCORSOList)
    {
        List<CustomRoute> _ret = new List<CustomRoute>();
        if(_PERCORSOList != null)
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
        CreateTable ct = ScriptableObject.CreateInstance<CreateTable>();
        ct.setImageTOTable("COMUNI_IMMAGINI", arr, comune_id);
    }
    public void setImageTOPOI(byte[] arr, int poi_id)
    {
        CreateTable ct = ScriptableObject.CreateInstance<CreateTable>();
        ct.setImageTOTable("POI_IMMAGINI", arr, poi_id);
    }
    public void setImageTOPERCORSI(byte[] arr, int percorso_id)
    {
        CreateTable ct = ScriptableObject.CreateInstance<CreateTable>();
        ct.setImageTOTable("PERCORSI_IMMAGINI", arr, percorso_id);
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

        //AddLocation(latitude, longitude);
        
        if (Input.location.status == LocationServiceStatus.Stopped && _latitudine == 0)
        {
            _latitudine = 43.2534828186035 ;
            _longitudine = 13.0091695785522;
        }
//        Debug.Log(Input.location.status + "  lat:" + _latitudine + "  long:" + _longitudine);
        //Stop retrieving location
        //Input.location.Stop();
    }


}

