using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static DBClass;

namespace ARLocation.MapboxRoutes.SampleProject
{
    public class MenuController : MonoBehaviour
    {
        public enum LineType
        {
            Route,
            NextTarget
        }
        public bool custom_route = false;
        public string MapboxToken = "pk.eyJ1IjoiZG1iZm0iLCJhIjoiY2tyYW9hdGMwNGt6dTJ2bzhieDg3NGJxNyJ9.qaQsMUbyu4iARFe0XB2SWg";
        public GameObject ARSession;
        public GameObject ARSessionOrigin;
        public GameObject RouteContainer;
        public Camera Camera;
        public Camera MapboxMapCamera;
        public MapboxRoute MapboxRoute;
        public AbstractRouteRenderer RoutePathRenderer;
        public AbstractRouteRenderer NextTargetPathRenderer;
        public Texture RenderTexture;
        public Mapbox.Unity.Map.AbstractMap Map;
        [Range(100, 10000)]
        public int MapSize = 512;
        public DirectionsFactory DirectionsFactory;
        public int MinimapLayer;
        public Material MinimapLineMaterial;
        public float BaseLineWidth = 20;
        public float MinimapStepSize = 0.5f;
        public GameObject[] _gameobject1234;
        public GameObject GoTappa;
        

        private AbstractRouteRenderer currentPathRenderer => s.LineType == LineType.Route ? RoutePathRenderer : NextTargetPathRenderer;
        private DBClass _DBClass;
        private List<POI> _POI = new List<POI>();
        private List<TAPPE> _TAPPE = new List<TAPPE>();
        public LineType PathRendererType
        {
            get => s.LineType;
            set
            {
                if (value != s.LineType)
                {
                    currentPathRenderer.enabled = false;
                    s.LineType = value;
                    currentPathRenderer.enabled = true;

                    if (s.View == View.Route)
                    {
                        MapboxRoute.RoutePathRenderer = currentPathRenderer;
                    }
                }
            }
        }

        enum View
        {
            SearchMenu,
            Route,
        }

        [System.Serializable]
        private class State
        {
            //SMO
            //public string QueryText = "Esanatoglia";
            public string QueryText = "";
            public List<GeocodingFeature> Results = new List<GeocodingFeature>();
            //SMO
            //public View View = View.Route;
            public View View = View.SearchMenu;

            public Location destination;
            public LineType LineType = LineType.NextTarget;
            public string ErrorMessage;
        }

        private State s = new State();

        private GUIStyle _textStyle;
        GUIStyle textStyle()
        {
            if (_textStyle == null)
            {
                _textStyle = new GUIStyle(GUI.skin.label);
                _textStyle.fontSize = 48;
                _textStyle.fontStyle = FontStyle.Bold;
            }

            return _textStyle;
        }

        private GUIStyle _textFieldStyle;
        GUIStyle textFieldStyle()
        {
            if (_textFieldStyle == null)
            {
                _textFieldStyle = new GUIStyle(GUI.skin.textField);
                _textFieldStyle.fontSize = 48;
            }
            return _textFieldStyle;
        }

        private GUIStyle _errorLabelStyle;
        GUIStyle errorLabelSytle()
        {
            if (_errorLabelStyle == null)
            {
                _errorLabelStyle = new GUIStyle(GUI.skin.label);
                _errorLabelStyle.fontSize = 24;
                _errorLabelStyle.fontStyle = FontStyle.Bold;
                _errorLabelStyle.normal.textColor = Color.red;
            }

            return _errorLabelStyle;
        }


        private GUIStyle _buttonStyle;
        GUIStyle buttonStyle()
        {
            if (_buttonStyle == null)
            {
                _buttonStyle = new GUIStyle(GUI.skin.button);
                _buttonStyle.fontSize = 48;
            }

            return _buttonStyle;
        }

        void Awake()
        {
            // MapboxMapCamera.gameObject.SetActive(false);
            // Map.SetCenterLatitudeLongitude()
        }
        //private float _long;
        //private float _lati;
        void Start()
        {
            NextTargetPathRenderer.enabled = false;
            RoutePathRenderer.enabled = false;
            ARLocationProvider.Instance.OnEnabled.AddListener(onLocationEnabled);
            Map.OnUpdated += OnMapRedrawn;
            //SMO
            //s.ErrorMessage = null;
            //StartCoroutine(search());
            s.View = View.Route;
            _DBClass = GameObject.FindWithTag("SQLite").GetComponent<DBClass>();
        }

        private void OnMapRedrawn()
        {
            // Debug.Log("OnMapRedrawn");
            if (currentResponse != null)
            {
                buildMinimapRoute(currentResponse);
            }
        }

        private void onLocationEnabled(Location location)
        {
            location = ARLocationManager.Instance.GetLocationForWorldPosition(Camera.main.transform.position);
            // Map.SetZoom(18);
            Map.UpdateMap();
        }

        void OnEnable()
        {
            Debug.Log("Enable!!!!!!!!");
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            // ARLocationProvider.Instance.OnEnabled.RemoveListener(onLocationEnabled);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"Scene Loaded: {scene.name}");
        }

        void drawMap()
        {
            
            var tw = RenderTexture.width;
            var th = RenderTexture.height;

            var scale = MapSize / th;
            var newWidth = scale * tw;
            var x = Screen.width / 2 - newWidth / 2;
            float border;
            if (x < 0)
            {
                border = -x;
            }
            else
            {
                border = 0;
            }

            //disegna la minimap
            GUI.DrawTexture(new Rect(x, Screen.height - MapSize, newWidth, MapSize), RenderTexture, ScaleMode.ScaleAndCrop);
            //disegna la riga divisoria
            GUI.DrawTexture(new Rect(0, Screen.height - MapSize - 20, Screen.width, 20), separatorTexture, ScaleMode.StretchToFill, false);
            //aggiunge lo slider per lo zoom
            //var newZoom = GUI.HorizontalSlider(new Rect(0, Screen.height - 100, Screen.width, 200), Map.Zoom, 4, 20);
            float newZoom = Map.Zoom;


            // Create style for a button
            GUIStyle myButtonStyle = new GUIStyle(GUI.skin.button);
            myButtonStyle.fontSize = 50;
            // Load and set Font
            Font myFont = (Font)Resources.Load("Font/Poppins", typeof(Font));
            myButtonStyle.font = myFont;
            // Set color for selected and unselected buttons
            myButtonStyle.normal.textColor = Color.black;
            myButtonStyle.hover.textColor = Color.black;
            myButtonStyle.normal.background = Texture2D.whiteTexture;
            myButtonStyle.hover.background = Texture2D.whiteTexture;
            //520 is the height of map when is in minimum height
            if (MapSize > 600)
            {
                if (GUI.Button(new Rect(Screen.width - 200, Screen.height - ((520 / 2) + 150), 100, 100), "+", myButtonStyle))
                    newZoom += 1f;
                if (GUI.Button(new Rect(Screen.width - 200, Screen.height - ((520 / 2) - 50), 100, 100), "-", myButtonStyle))
                    newZoom -= 1f;
            }

            if (newZoom != Map.Zoom)
            {
                Map.SetZoom(newZoom);
                Map.UpdateMap();
                // buildMinimapRoute(currentResponse);
            }
        }

        void OnGUI()
        {
            if (s.View == View.Route)
            {
                drawMap();
                return;
            }
            if (custom_route)
                StartCoroutine(search());
            else
            {
                float h = Screen.height - MapSize;
                GUILayout.BeginVertical(new GUIStyle() { padding = new RectOffset(20, 20, 120, 20) }, GUILayout.MaxHeight(h), GUILayout.Height(h));

                var w = Screen.width;

                GUILayout.BeginVertical(GUILayout.MaxHeight(300));
                GUILayout.Label("Location Search", textStyle());
                GUILayout.BeginHorizontal(GUILayout.MaxHeight(100), GUILayout.MinHeight(100));
                s.QueryText = GUILayout.TextField(s.QueryText, textFieldStyle(), GUILayout.MinWidth(0.8f * w), GUILayout.MaxWidth(0.8f * w));

                if (GUILayout.Button("OK", buttonStyle(), GUILayout.MinWidth(0.15f * w), GUILayout.MaxWidth(0.15f * w)))
                {
                    s.ErrorMessage = null;
                    StartCoroutine(search());
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.BeginVertical();

                if (s.ErrorMessage != null)
                {
                    GUILayout.Label(s.ErrorMessage, errorLabelSytle());
                }

                foreach (var r in s.Results)
                {
                    if (GUILayout.Button(r.place_name, new GUIStyle(buttonStyle()) { alignment = TextAnchor.MiddleLeft, fontSize = 24, fixedHeight = 0.05f * Screen.height }))
                    {
                        StartRoute(r.geometry.coordinates[0]);
                    }
                }


                GUILayout.EndVertical();
                // GUILayout.Label(RenderTexture);
                GUILayout.EndVertical();
                // GUILayout.Label(RenderTexture, GUILayout.Height(mapSize));
            }
            drawMap();
        }

        private Texture2D _separatorTexture;
        private Texture2D separatorTexture
        {
            get
            {
                if (_separatorTexture == null)
                {
                    _separatorTexture = new Texture2D(1, 1);
                    _separatorTexture.SetPixel(0, 0, new Color(0.15f, 0.15f, 0.15f));
                    _separatorTexture.Apply();
                }

                return _separatorTexture;
            }
        }

        public void StartRoute(Location dest)
        {
            s.destination = dest;

            if (ARLocationProvider.Instance.IsEnabled)
            {
                loadRoute(ARLocationProvider.Instance.CurrentLocation.ToLocation());
            }
            else
            {
                ARLocationProvider.Instance.OnEnabled.AddListener(loadRoute);
            }
        }

        public void EndRoute()
        {
            custom_route = false;
            ARLocationProvider.Instance.OnEnabled.RemoveListener(loadRoute);
            ARSession.SetActive(false);
            ARSessionOrigin.SetActive(false);
            RouteContainer.SetActive(false);
            Camera.gameObject.SetActive(true);
            s.View = View.SearchMenu;
        }

        private void loadRoute(Location _)
        {
            if (s.destination != null)
            {
                var lang = PlayerPrefs.GetInt("lingua_selezionata") == 1 ? MapboxApiLanguage.Italian : MapboxApiLanguage.English_UK;//  MapboxRoute.Settings.Language;
                var api = new MapboxApi(MapboxToken, lang);
                var loader = new RouteLoader(api);
               
                StartCoroutine(
                        loader.LoadRoute(
                            //new RouteWaypoint { Type = RouteWaypointType.Location, Location = start },
                            new RouteWaypoint { Type = RouteWaypointType.UserLocation },
                            new RouteWaypoint { Type = RouteWaypointType.Location, Location = s.destination },
                            (err, res) =>
                            {
                                if (err != null)
                                {
                                    s.ErrorMessage = err;
                                    s.Results = new List<GeocodingFeature>();
                                    return;
                                }

                                ARSession.SetActive(true);
                                ARSessionOrigin.SetActive(true);
                                RouteContainer.SetActive(true);
                                Camera.gameObject.SetActive(false);
                                s.View = View.Route;
                                
                                currentPathRenderer.enabled = true;
                                MapboxRoute.RoutePathRenderer = currentPathRenderer;
                                MapboxRoute.BuildRoute(res);
                                currentResponse = res;
                                buildMinimapRoute(res);
                            }));
            }
        }

        private GameObject minimapRouteGo;
        private RouteResponse currentResponse;
        private List<GameObject> POIGo = new List<GameObject>();

        public void CustomRoute(RouteResponse res, List<POI> poiList, List<TAPPE>tappeList)
        {
            custom_route = true;
            ARSession.SetActive(true);
            ARSessionOrigin.SetActive(true);
            RouteContainer.SetActive(true);
            Camera.gameObject.SetActive(false);
            s.View = View.Route;

            currentPathRenderer.enabled = true;
            MapboxRoute.RoutePathRenderer = currentPathRenderer;
            MapboxRoute.BuildRoute(res);
            currentResponse = res;
            buildMinimapRoute(res);
            _POI = poiList;
            _TAPPE = tappeList;
        }

        private void buildMinimapRoute(RouteResponse res)
        {
            var geo = res.routes[0].geometry;
            var vertices = new List<Vector3>();
            var indices = new List<int>();

            var worldPositions = new List<Vector2>();
            //Vector3 firstPosition = Vector3.zero;
            foreach (var p in geo.coordinates)
            {
                /* var pos = Mapbox.Unity.Utilities.Conversions.GeoToWorldPosition(
                        p.Latitude,
                        p.Longitude,
                        Map.CenterMercator,
                        Map.WorldRelativeScale
                        ); */

                // Mapbox.Unity.Utilities.Conversions.GeoToWorldPosition
                var pos = Map.GeoToWorldPosition(new Mapbox.Utils.Vector2d(p.Latitude, p.Longitude), true);
                //if (firstPosition == Vector3.zero)
                //    firstPosition = new Vector3(pos.x, pos.z); ;
                worldPositions.Add(new Vector2(pos.x, pos.z));
                // worldPositions.Add(new Vector2((float)pos.x, (float)pos.y));
            }

            if (minimapRouteGo != null)
            {
                minimapRouteGo.Destroy();
            }
            if (POIGo.Count > 0)
            {
                foreach (var ap in POIGo)
                    ap.Destroy();
            }
            //questa parte serve per fare il percorso nella mappa piccola
            minimapRouteGo = new GameObject("minimap route game object");
            minimapRouteGo.layer = MinimapLayer;



            var mesh = minimapRouteGo.AddComponent<MeshFilter>().mesh;

            var lineWidth = BaseLineWidth * Mathf.Pow(2.0f, Map.Zoom - 18);
            LineBuilder.BuildLineMesh(worldPositions, mesh, lineWidth);

            var meshRenderer = minimapRouteGo.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = MinimapLineMaterial;



            
            //Debug.Log($"sono :{_POI.Count()}");
            List<long> already_inserted = new List<long>();
            foreach (DBClass.POI _p in _POI)
            {
                //"43.25659609773222,13.00896889545388"
                if (_p != null)
                {
                    foreach (var _tipo in _p.tipoList)
                    {
                        if (!already_inserted.Contains(_p.ID))
                        {
                            already_inserted.Add(_p.ID);
                            var _go = _gameobject1234[_tipo.tipo.group_id];// GameObject.FindGameObjectsWithTag(_p.tag).FirstOrDefault();
                            if (_go != null)
                            {
                                //Debug.Log($"POI :{_p.nome} lon:{_p.longitudine} lat:{_p.latitudine}");
                                var apgo = Instantiate(_go, Map.GeoToWorldPosition(_DBClass.VectorFromLonLat(_p.longitudine, _p.latitudine), true), Quaternion.identity);
                                apgo.tag = _p.tag;
                                apgo.name = _p.ID.ToString();
                                apgo.transform.Rotate(90, 0, 0);
                                apgo.transform.localPosition = new Vector3(
                    apgo.transform.position.x,
                    10,
                    apgo.transform.position.z);
                                if (MapSize > 512)
                                    apgo.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                                POIGo.Add(apgo);
                            }
                        }
                    }
                }
            }
            
            foreach (DBClass.TAPPE _t in _TAPPE)
            {
                //"43.25659609773222,13.00896889545388"
                if (_t != null)
                {
                    var _go = GoTappa;
                    if (_go != null)
                    {
                        Debug.Log("Caricate tappe n." + _TAPPE.Count);
                        //Debug.Log($"POI :{_p.nome} lon:{_p.longitudine} lat:{_p.latitudine}");
                        var apgo = Instantiate(_go, Map.GeoToWorldPosition(_DBClass.VectorFromLonLat(_t.longitudine, _t.latitudine), true), Quaternion.identity);
                        //apgo.tag = _t.tag;
                        apgo.name = _t.id.ToString();
                        //apgo.transform.Rotate(90, 0, 0);
                        apgo.transform.localPosition = new Vector3(
            apgo.transform.position.x,
            10,
            apgo.transform.position.z);
                        if (MapSize > 512)
                            apgo.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                        //apgo.transform.Rotate(90, 90, 90);
                        foreach (var ap in apgo.GetComponentsInChildren<TestoTappa>())
                        {
                            ap.SetText(_t.nome_tappa);
                        }
                        POIGo.Add(apgo);
                    }
                }
            }

        }

        IEnumerator search()
        {
            var lang = PlayerPrefs.GetInt("lingua_selezionata") == 1 ? MapboxApiLanguage.Italian : MapboxApiLanguage.English_UK;//  MapboxRoute.Settings.Language;
            Debug.Log("--SMO:Lingua selezionata:" + lang);
            var api = new MapboxApi(MapboxToken, lang);

            yield return api.QueryLocal(s.QueryText, true);

            if (api.ErrorMessage != null)
            {
                s.ErrorMessage = api.ErrorMessage;
                s.Results = new List<GeocodingFeature>();
            }
            else
            {
                s.Results = api.QueryLocalResult.features;
            }
        }
        
        Vector3 lastCameraPos;
        void Update()
        {
            MapboxMapCamera.gameObject.SetActive(true);
            if (s.View == View.Route)
            {
                var cameraPos = Camera.main.transform.position;

                var arLocationRootAngle = ARLocationManager.Instance.gameObject.transform.localEulerAngles.y;
                var cameraAngle = Camera.main.transform.localEulerAngles.y;
                var mapAngle = cameraAngle - arLocationRootAngle;

                MapboxMapCamera.transform.eulerAngles = new Vector3(90, mapAngle, 0);
                //Debug.Log("bbb differenza " + (cameraPos - lastCameraPos).magnitude);
                if ((cameraPos - lastCameraPos).magnitude < MinimapStepSize)
                {
                    return;
                }

                lastCameraPos = cameraPos;

                //Debug.Log("bbbb " + cameraPos);
                var location = ARLocationManager.Instance.GetLocationForWorldPosition(cameraPos);
                //Debug.Log("bbbb " + location.Longitude + " " + location.Latitude);
                Map.SetCenterLatitudeLongitude(new Mapbox.Utils.Vector2d(location.Latitude, location.Longitude));
                //    Map.SetCenterLatitudeLongitude(new Mapbox.Utils.Vector2d(location.Latitude, location.Longitude));
                Map.UpdateMap();
                //StartCoroutine(_DBClass.GetLatLonUsingGPS());
                //Map.UpdateMap(new Mapbox.Utils.Vector2d(_DBClass._latitudine, _DBClass._longitudine));
            }
            else
            {
                MapboxMapCamera.transform.eulerAngles = new Vector3(90, 0, 0);
            }

        }
    }
}
