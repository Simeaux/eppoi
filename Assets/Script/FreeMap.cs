using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using Mapbox.Map;




using System.IO;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using Mapbox.Unity.Map.Interfaces;
using Mapbox.Unity.Utilities;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using ARLocation.MapboxRoutes;
using static UnityEngine.XR.ARSubsystems.XRCpuImage;
using static DBClass;
using System.Linq;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using ARLocation;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using static ARLocation.MapboxRoutes.CustomRoute;
using UnityEngine.UIElements;
using System.Drawing;
using System.Globalization;

public class FreeMap : MonoBehaviour
{
    public string accessToken = "pk.eyJ1Ijoic2ltZWF1eCIsImEiOiJjbHlzaHV5ajQwNTI1MnFzY3Rjazlxc3gyIn0.qZJs_X3xqoqnIWS2Otlyhg";
    public Text lon;
    public Text lat;
    public Text zoom;
    public Text Dx;
    public Text Dy;
    public GameObject PanelMAP;
    public GameObject PanelPOI;
    public GameObject PanelGrid;
    public Canvas canvasPoi;
    public AbstractMap abstractMap;
    //public GameObject _markerPrefab;

    private ChangeScene _changeScene;



    private double centerLongitude; //Cambia la longitudine del centro dell'area che serve
    private double centerLatitude; //Cambia la latitudine del centro dell'area che serve

    private double selectedzoom; //Cambia la latitudine del centro dell'area che serve

    public enum style { Light, Dark, Streets, Outdoors, Satellite, SatelliteStreets };
    public style mapStyle = style.Streets;
    public enum resolution { low = 1, high = 2 };
    public resolution mapResolution = resolution.high;
    public double[] boundingBox; //[lon(min), lat(min), lon(max), lat(max)]

    public GameObject GOImhere;

    public Material MinimapLineMaterial;
    public GameObject[] _gameobject;
    public GameObject GOObject;

    private string[] styleStr = new string[] { "light-v10", "dark-v10", "streets-v11", "outdoors-v11", "satellite-v9", "satellite-streets-v11" };
    //private string url = "";
    //private bool updateMap = true;
    //private bool ResetMap = false;
    private Material mapMaterial;
    private Vector2 screenResolution;
    private double mapWidthMeter;
    private double mapHeightMeter;
    private int mapWidthPx = 1280;
    private int mapHeightPx = 1280;
    private double planeToCameraDistance;

    //Variabili da tenere
    private string accessTokenLast = "pk.eyJ1Ijoic2ltZWF1eCIsImEiOiJjbHlzaHV5ajQwNTI1MnFzY3Rjazlxc3gyIn0.qZJs_X3xqoqnIWS2Otlyhg";
    private double centerLongitudeLast = 43.144215900691755; //Cambia la longitudine del centro dell'area che serve
    private double centerLatitudeLast = 13.196735039064533; //Cambia la latitudine del centro dell'area che serve
    private double selectedzoomLast = 12.78; //Cambia la latitudine del centro dell'area che serve
    private style mapStyleLast = style.Streets;
    private resolution mapResolutionLast = resolution.high;


    public float _spawnScale = 0.1F;

    float clicktime;
    Ray ray;
    RaycastHit hit;
    Transform clickPoint;
    private DBClass _DBClass;
    private ExtractDataForMap _extract;
    // Start is called before the first frame update
    void Start()
    {
        lat.text = 43.144215900691755.ToString();
        lon.text = 13.196735039064533.ToString();
        FindLatLon();
        zoom.text = "15";
        

        double.TryParse(lon.text, out centerLongitude);
        double.TryParse(lat.text, out centerLatitude);
        double.TryParse(zoom.text, out selectedzoom);
        centerLongitudeLast = centerLongitude;
        centerLatitudeLast = centerLatitude;
        selectedzoomLast = selectedzoom;
        planeToCameraDistance = Vector3.Distance(abstractMap.transform.position, Camera.main.transform.position);
        screenResolution = new Vector2(Screen.width, Screen.height);
        MatchPlaneToScreenSize();
        //if (abstractMap.GetComponent<MeshRenderer>() == null)
        //{
        //    abstractMap.AddComponent<MeshRenderer>();
        //}
        //mapMaterial = new Material(Shader.Find("Unlit/Texture"));
        //map.GetComponent<MeshRenderer>().material = mapMaterial;
        POIGo = new List<GameObject>();
        _DBClass = GameObject.FindWithTag("SQLite").GetComponent<DBClass>();
        _extract = GameObject.FindWithTag("SQLite").GetComponent<ExtractDataForMap>();
        _extract.getstart();
        StartCoroutine(_DBClass.GetLatLonUsingGPS());
        GetMapbox();
        //OnBtnHere();
    }
    public IEnumerable FindLatLon()
    {
        // Check if the user has location service enabled.
        if (!Input.location.isEnabledByUser)
            Debug.Log("Location not enabled on device or app does not have permission to access location");

        // Starts the location service.
        Input.location.Start();

        // Waits until the location service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // If the service didn't initialize in 20 seconds this cancels location service use.
        if (maxWait < 1)
        {
            Debug.Log("Timed out");
            yield break;
        }

        // If the connection failed this cancels location service use.
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("Unable to determine device location");
            yield break;
        }
        else
        {
            // If the connection succeeded, this retrieves the device's current location and displays it in the Console window.
            Debug.Log("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
            lon.text = Input.location.lastData.longitude.ToString();
            lat.text = Input.location.lastData.latitude.ToString();
        }

        // Stops the location service if there is no need to query location updates continuously.
        Input.location.Stop();
    }


    //Update è chiamato a ogni frame
    private void Update()
    {
        /*
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("SpostaCentro_freemap")))
        {
            var ap = PlayerPrefs.GetString("SpostaCentro_freemap").Split('_');
            if (ap != null && ap.Length > 0)
            {
                _long = double.Parse(ap[0]);
                _lati = double.Parse(ap[1]);
                SpostaCentro(_lati, _long);
                PlayerPrefs.SetString("SpostaCentro_freemap", "");
            }
        }
        */
        //se il filtro per nome è attivo non muovo la mappa e non clicco sui POI
        if (!PanelGrid.activeSelf)
        {
            bool click = false;

            if (Input.GetMouseButtonDown(0))
            {
                clicktime = Time.time;
            }
            if (Input.touchCount == 2)
            {
                var touchZero = Input.GetTouch(0);
                var touchOne = Input.GetTouch(1);

                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                float prevMagniture = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float currentMagniture = (touchZero.position - touchOne.position).magnitude;

                float difference = currentMagniture - prevMagniture;

                pinch_zoom(difference * 0.001f);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                var clickup = Time.time;
                //Debug.Log(clickup - clicktime);
                if (clickup - clicktime < 0.15f)
                {
                    click = true;
                }
            }

            if (click)
            {
                Debug.Log("FreeMap CLiccked!!");
                // Bit shift the index of the layer (8) to get a bit mask
                int layerMask = 1 << 8;

                // This would cast rays only against colliders in layer 8.
                // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
                layerMask = ~layerMask;
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
                {
                    print(Input.mousePosition);

                    // da quì ho il tag del gameobject cliccato e posso aprire il dettaglio o chiedere se vuole arrivarci
                    var p = hit.transform.gameObject as GameObject;
                    if (p.CompareTag("Close_Detail"))
                        PanelPOI.GetComponent<DetailPOIManager>().CloseDetailPOI();
                    else
                    {
                        if(!string.IsNullOrEmpty(p.name))
                            PlayerPrefs.SetInt("poi_selezionato", int.Parse(p.name));
                        PanelPOI.GetComponent<DetailPOIManager>().OpenDetailAtID(p.name, false, false, 3);
                        PanelMAP.SetActive(false);
                        GameObject.FindObjectOfType<ItinerariEventiPOI_AttaccatiAlComune>().Reload();
                    }

                }

            }
            else
            {
                if (!canvasPoi.enabled)
                {
                    double.TryParse(zoom.text, out selectedzoom);
                    if (Dx.text != "0" || Dy.text != "0" || selectedzoom != selectedzoomLast)
                    {

                        Ricalcola_Centro(true);
                        if (selectedzoomLast != selectedzoom)
                            selectedzoomLast = selectedzoom;
                    }
                }

            }
        }
    }

    void pinch_zoom (float increment)
    {
        //Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - increment, 5, 20);
        double.TryParse(zoom.text, out selectedzoom);
        zoom.text = (selectedzoom + increment).ToString();
    }
    
    public void Ricalcola_Centro(bool needredrawMap)
    {
        boundingBox = GetRectMinMaxLonLat(centerLongitude, centerLatitude, mapWidthMeter, mapHeightMeter);
        var maxX = Screen.width;
        double deltax = 0;
        double.TryParse(Dx.text, out deltax);
        double calcolo_x = (((boundingBox[2] - boundingBox[0]) / maxX) * deltax) * Math.Pow(2, (19 - selectedzoom));
        var maxY = Screen.height;
        double deltay = 0;
        double.TryParse(Dy.text, out deltay);
        double calcolo_y = (((boundingBox[3] - boundingBox[1]) / Screen.height) * deltay) * Math.Pow( 2, (19 - selectedzoom));
        centerLongitude = (centerLongitude - calcolo_x);
        centerLatitude = (centerLatitude - calcolo_y);
        lon.text = centerLongitude.ToString();
        lat.text = centerLatitude.ToString();
        double.TryParse(zoom.text, out selectedzoom);
        //HP 2: sposto semopllicemente il centro della mappa
        //abstractMap.SetCenterLatitudeLongitude(new Vector2d(centerLatitude, centerLongitude));
        //Debug.Log($"{centerLatitude} {centerLongitude} {selectedzoom}");
        abstractMap.UpdateMap(new Vector2d(centerLatitude, centerLongitude), ((float)selectedzoom));
        //abstractMap.
        Dx.text = "-1";
        Dy.text = "-1";
        
        //double valore = 2.575;
        //GOObject.transform.Translate((float)(deltax / valore), 0,  (float)(deltay / valore));
        if(needredrawMap)
            DrawCustomRoute();

    }

    private void GetMapbox()
    {
        /*
        boundingBox = GetRectMinMaxLonLat(centerLongitude, centerLatitude, mapWidthMeter, mapHeightMeter);
        double x = (boundingBox[1] + boundingBox[3]) / 2;
        double y = (boundingBox[0] + boundingBox[2]) / 2;
        //url = "https://api.mapbox.com/styles/v1/mapbox/" + styleStr[(int)mapStyle] + "/static/" + ((boundingBox[1] + boundingBox[3]) / 2).ToString().Replace(",", ".") + "," + ((boundingBox[0] + boundingBox[2]) / 2).ToString().Replace(",", ".") + "," + (selectedzoom).ToString().Replace(",", ".") + ",0/" + mapWidthPx + "x" + mapHeightPx + "?access_token=" + accessToken;
        url = "https://api.mapbox.com/directions/v5/mapbox/cycling/13.202615%2C43.146635%3B13.191017%2C43.150953%3B13.189631%2C43.151366%3B13.192672%2C43.174763%3B13.19089%2C43.175303%3B13.188591%2C43.175868?alternatives=true&continue_straight=true&geometries=geojson&language=en&overview=full&steps=true&access_token=" + accessToken;
        Debug.Log(url);
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("WWW ERROR: " + www.error);
            Debug.Log(url);
            //Resetto tutto allo stato di partenza
            accessToken = accessTokenLast;
            centerLongitude = centerLongitudeLast;
            centerLatitude = centerLatitudeLast;
            mapStyle = mapStyleLast;
            mapResolution = mapResolutionLast;
           // updateMap = true;
           // ResetMap = true;
        }
        else
        {
        */
            //Debug.Log("WWW OK!!!");
            //Destroy(gameObject.GetComponent<MeshRenderer>().material.GetTexture("_MainTex"));
            //gameObject.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", ((DownloadHandlerTexture)www.downloadHandler).texture);
            //abstractMap.SetLoadingTexture(((DownloadHandlerTexture)www.downloadHandler).texture);
            abstractMap.UpdateMap(new Vector2d(centerLatitude, centerLongitude), (float)selectedzoom);
            /*
            _markerPrefab.layer = 99;
            foreach (GameObject g in FindObjectsOfType(typeof(GameObject)))
            {
                if (g.layer == 99)
                    Destroy(g);
            }
            */
            //abstractMap.UpdateMap((float)selectedzoom);



            /*
            Vector2d position = new Vector2d(x, y);

            GameObject go = FindObjectOfType<GameObject>();
            var instance = Instantiate(go, abstractMap.transform);
            instance.transform.localPosition = abstractMap.GeoToWorldPosition(position, true);
            instance.transform.localScale = new Vector3(.2f, .2f, .2f);
            */
            ////////

            //Aggiorno le variabili *Last per tenere traccia dei cambiamenti
            accessTokenLast = accessToken;
            centerLongitudeLast = centerLongitude;
            centerLatitudeLast = centerLatitude;
            selectedzoomLast = selectedzoom;
            mapStyleLast = mapStyle;
            mapResolutionLast = mapResolution;
           // updateMap = true;
           // ResetMap = false;
        //}
        Dx.text = "0";
        Dy.text = "0";

        DrawCustomRoute();
    }



    //Set the scale of plane to match the screen size
    private void MatchPlaneToScreenSize()
    {
        double planeHeightScale = (2.0 * Math.Tan(0.5f * Camera.main.fieldOfView * (Math.PI / 180)) * planeToCameraDistance) / 10.0; //Radians = (Math.PI / 180) * degrees. Default plane is 10 units in x and z
        double planeWidthScale = planeHeightScale * Camera.main.aspect;
        //abstractMap.transform.localScale = new Vector3((float)planeWidthScale, 1, (float)planeHeightScale);
        abstractMap.transform.localScale = new Vector3(1, 1, 1);
        mapWidthMeter = planeWidthScale * 10.0; // Prendo buono che ogni unità in unity è 1 metro nel mondo reale. Il plane è impostato si 100 unità in x e z
        mapHeightMeter = planeHeightScale * 10.0; // Prendo buono che ogni unità in unity è 1 metro nel mondo reale. Il plane è impostato si 100 unità in x e z
        //Set map width and height in pixel based on view aspec ratio
        if (Camera.main.aspect > 1) //Width is bigger than height
        {
            mapWidthPx = 1280; //Mapbox width should be a number between 1 and 1280 pixels.
            mapHeightPx = (int)Math.Round(mapWidthPx / Camera.main.aspect); //Height is proportional to to view aspect ratio
        }
        else //Height is bigger than width
        {
            mapHeightPx = 1280; //Mapbox height should be a number between 1 and 1280 pixels.
            mapWidthPx = (int)Math.Round(mapWidthPx * Camera.main.aspect); //Width is proportional to to view aspect ratio
        }
    }


    //Return map bounding box [minLon, minLat, maxLon, maxLat] using center (lon, lat) in decimal degree, and map width and height
    private double[] GetRectMinMaxLonLat(double centerLon, double centerLat, double width, double height)
    {
        double distance = Math.Sqrt(Math.Pow(height / 2.0, 2) + Math.Pow(width / 2.0, 2)); //Ipotenusa dei 2 punti
        double topRightBearing = Math.Atan((width / 2.0) / (height / 2.0)); // Bearing in radian
        double bottonLeftBearing = 3.14159f + topRightBearing; //Math.PI + topRightBearing;
        double[] bottonLeft = GetPointLonLat(centerLon, centerLat, distance, bottonLeftBearing);
        double[] topRight = GetPointLonLat(centerLon, centerLat, distance, topRightBearing);
        return new double[] { bottonLeft[0], bottonLeft[1], topRight[0], topRight[1] };
    }


    private double[] GetPointLonLat(double startLonDegree, double startLatDegree, double distance, double bearingRadian)
    {
        double earthRadious = 6378100.000; //In metri
        double startLatRadians = startLatDegree * (Math.PI / 180); // Radians ) Degree * (PI /180)
        double startLonRadians = startLonDegree * (Math.PI / 180); // Radians ) Degree * (PI /180)
        double targetLatRadians = Math.Asin(Math.Sin(startLatRadians) * Math.Cos(distance / earthRadious) + Math.Cos(startLatRadians) * Math.Sin(distance / earthRadious) * Math.Cos(bearingRadian));
        double targetLonRadians = startLonRadians + Math.Atan2(Math.Sin(bearingRadian) * Math.Sin(distance / earthRadious) * Math.Cos(startLatRadians), Math.Cos(distance / earthRadious) - Math.Sin(startLatRadians) * Math.Sin(targetLatRadians));
        return new double[] { targetLonRadians * (180.0 / Math.PI), targetLatRadians * (180.0 / Math.PI) };
    }
    private List<GameObject> minimapRouteGo;
    private List<GameObject> POIGo = new List<GameObject>();

    public static bool Between(double num, double min, double max)
    {
        return min < num && num < max;
    }

    
    private UnityEngine.Color FromHex(string hex)
    {
        if (hex.StartsWith("#"))
            hex = hex.Substring(1);

        var h = NumberStyles.HexNumber;

        var r = int.Parse(hex.Substring(0, 2), h);
        var g = int.Parse(hex.Substring(2, 2), h);
        var b = int.Parse(hex.Substring(4, 2), h);
        var a = 255;

        if (hex.Length == 8)
        {
            a = int.Parse(hex.Substring(6, 2), h);
        }

        return new UnityEngine.Color(r, g, b, a);
    }
    private void DrawCustomRoute()
    {
        if (_extract == null)
        {
            Debug.Log("ricreo ExtractDataForMap");
            _extract = GameObject.FindWithTag("SQLite").GetComponent<ExtractDataForMap>();
            _extract.getstart();
        }
        //inserisco i POI dinamicamente leggendoli dal DB
        double delta_zoom = 40;
        var tmp_boundingBox = GetRectMinMaxLonLat(centerLongitude, centerLatitude, (mapWidthMeter * delta_zoom), (mapHeightMeter * delta_zoom));

        if (minimapRouteGo != null)
        {
            foreach (var _minimapRouteGo in minimapRouteGo)
                _minimapRouteGo.Destroy();
        }


        minimapRouteGo = new List<GameObject>();
        if (POIGo.Count > 0)
        {
            foreach (var ap in POIGo)
                ap.Destroy();
            POIGo = new List<GameObject>();
        }
        
       


        string ID_selected = "";
        if(GameObject.FindObjectOfType<Panel_POI>() != null)
            ID_selected = GameObject.FindObjectOfType<Panel_POI>().id_selected.text;
        //Questo array serve per non inserire gli oggetti doppi
        List<int> already_inserted = new List<int>();
        foreach (POI _p in _extract.getPoiList().Where(o=> Between(o.longitudine, tmp_boundingBox[0], tmp_boundingBox[2]) && Between(o.latitudine, tmp_boundingBox[1], tmp_boundingBox[3]) || (!string.IsNullOrEmpty(ID_selected) && ID_selected == o.ID.ToString())))
        {
            if (_p != null)
            {
                //"43.25659609773222,13.00896889545388"
                foreach (var _tipo in _p.tipoList)
                {
                    if (_tipo.tipo != null)
                    {
                        if (!already_inserted.Contains(_p.ID))
                        {
                            already_inserted.Add(_p.ID);
                            if (_p.limite_zoom <= (int)selectedzoom)
                            {
                                var _go = _gameobject[_tipo.tipo.group_id];// GameObject.FindGameObjectsWithTag(_p.tag).FirstOrDefault();
                                if (_go != null)
                                {
                                    var apgo = Instantiate(_go, abstractMap.GeoToWorldPosition(_DBClass.VectorFromLonLat(_p.longitudine, _p.latitudine), true), Quaternion.identity);
                                    apgo.tag = _p.tag;
                                    apgo.name = _p.ID.ToString();
                                    apgo.transform.Rotate(180, 0, 0);
                                    if (!string.IsNullOrEmpty(ID_selected) && ID_selected == apgo.name)
                                    {
                                        var x = apgo.transform.localScale.x;
                                        apgo.transform.localScale = new Vector3(x * 1.2f, x * 1.2f, x * 1.2f);

                                    }
                                    apgo.transform.SetParent(GOObject.transform, false);
                                    POIGo.Add(apgo);
                                }
                            }

                        }
                    }
                }
            }
        }
        try
        {
            //Location l = ARLocationManager.Instance.GetLocationForWorldPosition(Camera.main.transform.position);
            //var imhere = Instantiate(GOImhere, abstractMap.GeoToWorldPosition(new Vector2d(l.Latitude, l.Longitude), true), Quaternion.identity);
            var imhere = Instantiate(GOImhere, abstractMap.GeoToWorldPosition(new Vector2d(_DBClass._latitudine, _DBClass._longitudine), true), Quaternion.identity);
            imhere.transform.Rotate(90, 0, 0);
            //apgo.tag = _p.tag;
            imhere.transform.SetParent(GOObject.transform, false);
            POIGo.Add(imhere);
        }
        catch
        {

        }
        finally
        { }
        int i = 0;
        List<PERCORSO> percorsi = new List<PERCORSO>();

        if(already_inserted != null && already_inserted.Count > 0)
            percorsi = _extract?.getPercorsoList()?.Where(p => already_inserted.Contains(p.poi_id)).ToList<PERCORSO>();
        else
            percorsi = _extract?.getPercorsoList();
        foreach (var customroute in _DBClass.GetCustomRoute(percorsi?.ToList()))
        {
            
            var res = new RouteResponse();

            res.routes = new List<Route> { customroute.ToMapboxRoute() };
            res.waypoints = customroute.GetWaypoints();

            var geo = res.routes[0].geometry;
            var vertices = new List<Vector3>();
            var indices = new List<int>();

            var worldPositions = new List<Vector2>();
            //Vector3 firstPosition = Vector3.zero;
            foreach (var p in geo.coordinates)
            {
                // Mapbox.Unity.Utilities.Conversions.GeoToWorldPosition
                var pos = abstractMap.GeoToWorldPosition(new Vector2d(p.Latitude, p.Longitude), true);

                worldPositions.Add(new Vector2(pos.x, pos.z));
            }



            //questa parte serve per fare il percorso nella mappa piccola
            var _minimapRouteGo = new GameObject($"percorso_{percorsi.ToList()[i].id}");
            _minimapRouteGo.layer = 6;



            var mesh = _minimapRouteGo.AddComponent<MeshFilter>().mesh;

            var lineWidth = 2 * Mathf.Pow(2.0f, abstractMap.Zoom - 18);
            if (lineWidth < 0.5)
                lineWidth = 0.5f;
            LineBuilder.BuildLineMesh(worldPositions, mesh, lineWidth);

            var meshRenderer = _minimapRouteGo.AddComponent<MeshRenderer>();
            Material newmaterial = new Material(MinimapLineMaterial);
            if (!string.IsNullOrEmpty(customroute.Color))
                newmaterial.SetColor("_Color", FromHex(customroute.Color));
            meshRenderer.sharedMaterial = newmaterial;
            _minimapRouteGo.transform.SetParent(GOObject.transform, false);
            minimapRouteGo.Add(_minimapRouteGo);

            i++;
        }
    }

    
    public void OnBtnHere()
    {
        //Location l = ARLocationManager.Instance.GetLocationForWorldPosition(Camera.main.transform.position);
        //Location l = ARLocationProvider.Instance.CurrentLocation.ToLocation();
        StartCoroutine(_DBClass.GetLatLonUsingGPS());

        SpostaCentro(_DBClass._latitudine, _DBClass._longitudine);
    }
    public void SpostaCentro(double latitudine, double longitudine)
    {
        centerLatitude = latitudine;
        centerLongitude = longitudine;
        Ricalcola_Centro(true);
    }
}
