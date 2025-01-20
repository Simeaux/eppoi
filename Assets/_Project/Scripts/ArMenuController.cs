using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ARLocation.MapboxRoutes;
using Mapbox.Directions;
using static UnityEngine.XR.ARSubsystems.XRCpuImage;
using System.Collections;
using static DBClass;
using Mapbox.Map;

using static ARLocation.MapboxRoutes.SampleProject.ArMenuController;
using TS.PageSlider;
using TS.PageSlider.Demo;
using DigitsNFCToolkit;

namespace ARLocation.MapboxRoutes.SampleProject
{
    #region Tween
    public class Tween
    {
        Vector3 start;
        Vector3 end;
        Vector3 current;
        float speed;
        float t;

        public Vector3 Position => current;

        public Tween(Vector3 startPos, Vector3 endPos, float tweenSpeed = 1)
        {
            start = startPos;
            end = endPos;
            speed = tweenSpeed;
        }

        public bool Update()
        {
            current = start * (1 - t) + end * t;

            t += Time.deltaTime * speed;

            if (t > 1)
            {
                return true;
            }

            return false;
        }
    }

    public class TweenRectTransform
    {
        public RectTransform Rt;
        public Vector3 PositionStart;
        public Vector3 PositionEnd;
        public Quaternion RotationStart;
        public Quaternion RotationEnd;

        public TweenRectTransform(RectTransform rectTransform, Vector3 targetPosition, Quaternion targetRotation)
        {
            Rt = rectTransform;
            PositionStart = rectTransform.position;
            RotationStart = rectTransform.rotation;
            PositionEnd = targetPosition;
            RotationEnd = targetRotation;
        }
    }

    public class TweenRectTransformGroup
    {
        public enum EaseFunc
        {
            Linear,
            EaseOutBack,
            EaseInCubic,
        }

        public List<TweenRectTransform> Elements = new List<TweenRectTransform>();
        float speed;
        float t;
        Func<float, float, float, float> easeFunc;

        public TweenRectTransformGroup(float speed, EaseFunc easeFuncType)
        {
            this.speed = speed;

            switch (easeFuncType)
            {
                case EaseFunc.EaseOutBack:
                    this.easeFunc = EaseOutBack;
                    break;

                case EaseFunc.EaseInCubic:
                    this.easeFunc = EaseInCubic;
                    break;

                case EaseFunc.Linear:
                    this.easeFunc = EaseLinear;
                    break;
            }
        }

        public Vector3 ease(Vector3 start, Vector3 end, float t)
        {
            var x = easeFunc(start.x, end.x, t);
            var y = easeFunc(start.y, end.y, t);
            var z = easeFunc(start.z, end.z, t);

            return new Vector3(x, y, z);
        }

        public bool Update()
        {
            foreach (var e in Elements)
            {
                e.Rt.position = ease(e.PositionStart, e.PositionEnd, t); //e.PositionStart * (1 - t) + e.PositionEnd * t;
                e.Rt.rotation = Quaternion.Lerp(e.RotationStart, e.RotationEnd, t);
            }

            t += speed * Time.deltaTime;

            if (t > 1)
            {
                foreach (var e in Elements)
                {
                    e.Rt.position = e.PositionEnd;
                    e.Rt.rotation = e.RotationEnd;
                }

                return true;
            }

            return false;
        }

        public static float EaseOutBack(float start, float end, float value)
        {
            float s = 1.70158f;
            end -= start;
            value = (value) - 1;
            return end * ((value) * value * ((s + 1) * value + s) + 1) + start;
        }

        public static Vector3 EaseOutBack(Vector3 start, Vector3 end, float t)
        {
            float x = EaseOutBack(start.x, end.x, t);
            float y = EaseOutBack(start.y, end.y, t);
            float z = EaseOutBack(start.z, end.z, t);

            return new Vector3(x, y, z);
        }

        public static float EaseInCubic(float start, float end, float value)
        {
            end -= start;
            return end * value * value * value + start;
        }

        public static float EaseLinear(float start, float end, float value)
        {
            return start * (1 - value) * end * value;
        }

    }

    #endregion
    #region ARMenuController
    public class ArMenuController : MonoBehaviour
    {
        private Color lightgray = new Color(0.8f, 0.8f, 0.8f, 1.0f);
        private DBClass _DBClass;
        public enum StateType
        {
            Closed,
            Open,
            OpenTransition,
            CloseTransition
        }

        [System.Serializable]
        public class StateData
        {
            public StateType CurrentStateNavigazione = StateType.Closed;
            //public StateType CurrentStatePercorsi = StateType.Closed;
            public TweenRectTransformGroup tweenGroupNavigazione;
            //public TweenRectTransformGroup tweenGroupPercorsi;
        }

        [System.Serializable]
        public class ElementsData
        {
            public Text LabelText;

            public Button BtnToggle;
            public Button BtnNext;
            public Button BtnPrev;
            public Button BtnRestart;
            public Button BtnLineRender;
            public Button BtnResizeMinimap;
            public Button BtnNFC;
            public Button BtnClose;

            //public Button BtnExit;
            //public Button BtnPercorsi;
            //public Button BtnPercorso1;
            //public Button BtnPercorso2;


            public Text LabelNext;
            public Text LabelPrev;
            public Text LabelRestart;
            public Text LabelTargetRender;

            //public Text LabelSearch;
            //public Text LabelPercorsi;
            //public Text LabelPercorso1;
            //public Text LabelPercorso2;

            public RectTransform TargetNext;
            public RectTransform TargetPrev;
            public RectTransform TargetRestart;
            public RectTransform TargetLineRender;
            //public RectTransform TargetExit;

            //public RectTransform TargetPercorso1;
            //public RectTransform TargetPercorso2;
        }

        [System.Serializable]
        public class SettingsData
        {
            public MapboxRoute MapboxRoute;
            public MenuController MenuController;
            public float TransitionSpeed = 2.0f;
        }

        [System.Serializable]
        public class POIData
        {
            public SliderPage immagine;
            public GameObject content_list_img;
            public PageSlider scrollview_list_img;
            public Text tipo;
            public Text name;
            public Text nameComune;
            public GameObject scrollview;
            public TMPro.TMP_Text descrizione;
            public GameObject dettaglioItinerario;
            public List<GameObject> ItemsItinerari;
            public TMPro.TMP_Text webpage;
            public Image img_webpage;
            public TMPro.TMP_Text facebook;
            public Image img_facebook;
            public TMPro.TMP_Text instagramm;
            public Image img_instagramm;
            public TMPro.TMP_Text telefono;
            public Image img_telefono;
            public TMPro.TMP_Text mail;
            public Image img_mail;
            public Button Btn_percorso_associato;
            public Text percorso_associato;
            public Image img_percorso_associato;
        }
        public SettingsData Settings;
        public ElementsData Elements;
        public POIData Pois;
        public Canvas DetailPOI;
        private StateData s = new StateData();

        public void Awake()
        {
            s = new StateData();

            showOnlyToggleButtonNavigazione();
            //showOnlyToggleButtonPercorsi();
        }

        void showOnlyToggleButtonNavigazione()
        {
            //Elements.BtnExit.gameObject.SetActive(false);
            Elements.BtnLineRender.gameObject.SetActive(false);
            Elements.BtnNext.gameObject.SetActive(false);
            Elements.BtnPrev.gameObject.SetActive(false);
            Elements.BtnRestart.gameObject.SetActive(false);
            Elements.BtnToggle.gameObject.SetActive(true);

        }
        //void showOnlyToggleButtonPercorsi()
        //{
        //    Elements.BtnPercorso1.gameObject.SetActive(false);
        //    Elements.BtnPercorso2.gameObject.SetActive(false);
        //    Elements.BtnPercorsi.gameObject.SetActive(true);
        //
        //}

        void showAllButtonsNavigazione()
        {
            //Elements.BtnExit.gameObject.SetActive(true);
            Elements.BtnLineRender.gameObject.SetActive(true);
            Elements.BtnNext.gameObject.SetActive(true);
            Elements.BtnPrev.gameObject.SetActive(true);
            Elements.BtnRestart.gameObject.SetActive(true);
            Elements.BtnToggle.gameObject.SetActive(true);

        }
        //void showAllButtonsPercorsi()
        //{
        //    Elements.BtnPercorso1.gameObject.SetActive(true);
        //    Elements.BtnPercorso2.gameObject.SetActive(true);
        //    Elements.BtnPercorsi.gameObject.SetActive(true);
        //
        //}

        public void OnEnable()
        {
            Elements.BtnToggle.onClick.AddListener(OnTogglePress);
            Elements.BtnNext.onClick.AddListener(OnNextPress);
            Elements.BtnPrev.onClick.AddListener(OnPrevPress);
            Elements.BtnRestart.onClick.AddListener(OnRestartPress);
            //Elements.BtnExit.onClick.AddListener(OnSearchPress);
            Elements.BtnLineRender.onClick.AddListener(OnLineRenderPress);
            //Elements.BtnPercorsi.onClick.AddListener(OnPercorsiPress);
            //Elements.BtnPercorso1.onClick.AddListener(OnPercorso1Press);
            //Elements.BtnPercorso2.onClick.AddListener(OnPercorso2Press);

            updateLineRenderButtonLabel();
        }

        public void OnDisable()
        {
            Elements.BtnToggle.onClick.RemoveListener(OnTogglePress);
            Elements.BtnNext.onClick.RemoveListener(OnNextPress);
            Elements.BtnPrev.onClick.RemoveListener(OnPrevPress);
            Elements.BtnRestart.onClick.RemoveListener(OnRestartPress);
            //Elements.BtnExit.onClick.RemoveListener(OnSearchPress);
            Elements.BtnLineRender.onClick.RemoveListener(OnLineRenderPress);
            //Elements.BtnPercorsi.onClick.RemoveListener(OnPercorsiPress);
            //Elements.BtnPercorso1.onClick.RemoveListener(OnPercorso1Press);
            //Elements.BtnPercorso2.onClick.RemoveListener(OnPercorso2Press);
        }

        private void updateLineRenderButtonLabel()
        {
            var mc = Settings.MenuController;

            if (mc.PathRendererType == MenuController.LineType.Route)
            {
                Elements.LabelTargetRender.text = "Percorso";
            }
            else
            {
                Elements.LabelTargetRender.text = "Linea al bersaglio";
            }

        }


        public void OnLineRenderPress()
        {
            var mc = Settings.MenuController;

            if (mc.PathRendererType == MenuController.LineType.Route)
            {
                mc.PathRendererType = MenuController.LineType.NextTarget;
                Elements.LabelTargetRender.text = "Linea al bersaglio";
            }
            else
            {
                mc.PathRendererType = MenuController.LineType.Route;
                Elements.LabelTargetRender.text = "Percorso";
            }
        }


        private void OnSearchPress()
        {
            Debug.Log("OnSearchPress");
            Settings.MenuController.EndRoute();
        }

        public void OnRestartPress()
        {
            Debug.Log("OnRestartPress");
            Settings.MapboxRoute.ClosestTarget();
        }

        private void OnPrevPress()
        {
            Debug.Log("OnPrevPress");
            Settings.MapboxRoute.PrevTarget();
        }

        private void OnNextPress()
        {
            Debug.Log("OnNextPress");
            Settings.MapboxRoute.NextTarget();
        }

        private void OnTogglePress()
        {
            Debug.Log("OnTogglePress");
            //if (s.CurrentStatePercorsi == StateType.Open)
            //{
            //    closeMenuPercorsi();
            //}
            toggleMenuNavigazione();
        }


        /*
        private void OnPercorsiPress()
        {
            Debug.Log("Percorsi Pressed.");
            if (s.CurrentStateNavigazione == StateType.Open)
            {
                closeMenuNavigazione();
            }
            toggleMenuPercorsi();
        }
        
        private void OnPercorso1Press()
        {
            Percorso_N_Go("0");
            OnPercorsiPress();
        }

        private void OnPercorso2Press()
        {
            Percorso_N_Go("1");
            OnPercorsiPress();
        }
        */
        private void Percorso_N_Go(int n)
        {
            Elements.LabelText.text = n.ToString();
            reloadRoute(n);
        }
        private void reloadRoute(int n)
        {
            
            if (n >= 0)
            {
                var ListOfRoute = _DBClass.ListOfCustomRoute(n, null, null);
                Debug.Log($"ArMenuController percorso id:{n}");
                if (ListOfRoute != null && ListOfRoute.Count > 0)
                {
                    Debug.Log($"ArMenuController customroute:{ListOfRoute[0]}");

                    Settings.MapboxRoute.Settings.RouteSettings.CustomRoute = ListOfRoute[0];
                    //var route = Settings.MapboxRoute.Settings.RouteSettings.CustomRoute[app];
                    MapboxRoute mr = (MapboxRoute)Settings.MapboxRoute.GetComponent(typeof(MapboxRoute));
                    mr.LoadCustomRoute(ListOfRoute[0]);
                    mr.ReloadRoute();

                    var res = new RouteResponse();
                    res.routes = new List<Route>();
                    foreach (var route in ListOfRoute)
                    {
                        res.routes.Add(route.ToMapboxRoute());
                        if(res.waypoints == null)
                            res.waypoints = route.GetWaypoints();
                    }
                    List<POI> _poi = new List<POI>();
                    List<int> _comuni = new List<int>();
                    // prendo in considerazione solo le tappe collegate ai comuni compresi nel percorso
                    foreach (var _tappexperxorso in _DBClass.getTAPPEXPERCORSI(null, null, n))
                    {
                        if (_tappexperxorso != null)
                        {
                            var _poixtappa = _DBClass.getPOIXTAPPE(null, null, _tappexperxorso.tappa_id);
                            if (_poixtappa != null && _poixtappa.Count > 0)
                            {
                                foreach (var _ppp in _poixtappa)
                                {
                                    foreach (var _poi_selezionati in _DBClass.getPOI(_ppp.poi_id))
                                    {
                                        if (!_comuni.Contains(_poi_selezionati.comune_id))
                                        {
                                            _comuni.Add(_poi_selezionati.comune_id);
                                            foreach (var _PoiPerComune in _DBClass.getPOI(null, _poi_selezionati.comune_id))
                                            {
                                                if (!_poi.Contains(_PoiPerComune))
                                                    _poi.Add(_PoiPerComune);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    Settings.MenuController.CustomRoute(res, _poi);
                }

            }
           /*
            var route = Settings.MapboxRoute.Settings.RouteSettings.CustomRoute;
            MapboxRoute mr = (MapboxRoute)Settings.MapboxRoute.GetComponent(typeof(MapboxRoute));
            mr.LoadCustomRoute(route);
            mr.ReloadRoute();

            var res = new RouteResponse();
            res.routes = new List<Route> { route.ToMapboxRoute() };
            res.waypoints = route.GetWaypoints();

            Settings.MenuController.CustomRoute(res);
            */
        }

        void toggleMenuNavigazione()
        {
            if (s.CurrentStateNavigazione == StateType.Closed)
            {
                openMenuNavigazione();
            }
            else if (s.CurrentStateNavigazione == StateType.Open)
            {
                closeMenuNavigazione();
            }
        }
        /*
        void toggleMenuPercorsi()
        {
            if (s.CurrentStatePercorsi == StateType.Closed)
            {
                openMenuPercorsi();
            }
            else if (s.CurrentStatePercorsi == StateType.Open)
            {
                closeMenuPercorsi();
            }
        }
        */
        void openMenuNavigazione()
        {
            switch (s.CurrentStateNavigazione)
            {
                case StateType.Closed:

                    showAllButtonsNavigazione();

                    s.tweenGroupNavigazione = new TweenRectTransformGroup(Settings.TransitionSpeed, TweenRectTransformGroup.EaseFunc.EaseInCubic);

                    s.tweenGroupNavigazione.Elements.Add(new TweenRectTransform(Elements.BtnNext.GetComponent<RectTransform>(), Elements.TargetNext.position, Quaternion.identity));
                    s.tweenGroupNavigazione.Elements.Add(new TweenRectTransform(Elements.BtnPrev.GetComponent<RectTransform>(), Elements.TargetPrev.position, Quaternion.identity));
                    s.tweenGroupNavigazione.Elements.Add(new TweenRectTransform(Elements.BtnRestart.GetComponent<RectTransform>(), Elements.TargetRestart.position, Quaternion.identity));
                    //s.tweenGroupNavigazione.Elements.Add(new TweenRectTransform(Elements.BtnExit.GetComponent<RectTransform>(), Elements.TargetExit.position, Quaternion.identity));
                    s.tweenGroupNavigazione.Elements.Add(new TweenRectTransform(Elements.BtnLineRender.GetComponent<RectTransform>(), Elements.TargetLineRender.position, Quaternion.identity));

                    s.tweenGroupNavigazione.Elements.Add(new TweenRectTransform(Elements.BtnToggle.GetComponent<RectTransform>(), Elements.BtnToggle.GetComponent<RectTransform>().position, Quaternion.Euler(0, 0, 180)));

                    //s.BtnNextTween = new Tween(Elements.BtnNext.GetComponent<RectTransform>().position, Elements.TargetNext.position, Settings.TransitionSpeed);
                    //s.BtnPrevTween = new Tween(Elements.BtnPrev.GetComponent<RectTransform>().position, Elements.TargetPrev.position, Settings.TransitionSpeed);
                    //s.BtnRestartTween = new Tween(Elements.BtnRestart.GetComponent<RectTransform>().position, Elements.TargetRestart.position, Settings.TransitionSpeed);
                    //s.BtnExitTween = new Tween(Elements.BtnExit.GetComponent<RectTransform>().position, Elements.TargetExit.position, Settings.TransitionSpeed);

                    Elements.BtnToggle.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 180);

                    s.CurrentStateNavigazione = StateType.OpenTransition;

                    break;
            }
        }
        /*
        void openMenuPercorsi()
        {
            switch (s.CurrentStatePercorsi)
            {
                case StateType.Closed:

                    showAllButtonsPercorsi();

                    s.tweenGroupPercorsi = new TweenRectTransformGroup(Settings.TransitionSpeed, TweenRectTransformGroup.EaseFunc.EaseInCubic);

                    s.tweenGroupPercorsi.Elements.Add(new TweenRectTransform(Elements.BtnPercorso1.GetComponent<RectTransform>(), Elements.TargetPercorso1.position, Quaternion.identity));
                    s.tweenGroupPercorsi.Elements.Add(new TweenRectTransform(Elements.BtnPercorso2.GetComponent<RectTransform>(), Elements.TargetPercorso2.position, Quaternion.identity));

                    //s.BtnNextTween = new Tween(Elements.BtnNext.GetComponent<RectTransform>().position, Elements.TargetNext.position, Settings.TransitionSpeed);
                    //s.BtnPrevTween = new Tween(Elements.BtnPrev.GetComponent<RectTransform>().position, Elements.TargetPrev.position, Settings.TransitionSpeed);
                    //s.BtnRestartTween = new Tween(Elements.BtnRestart.GetComponent<RectTransform>().position, Elements.TargetRestart.position, Settings.TransitionSpeed);
                    //s.BtnExitTween = new Tween(Elements.BtnExit.GetComponent<RectTransform>().position, Elements.TargetExit.position, Settings.TransitionSpeed);

                    Elements.BtnPercorsi.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 180);

                    s.CurrentStatePercorsi = StateType.OpenTransition;

                    break;
            }
        }
        */
        void closeMenuNavigazione()
        {
            switch (s.CurrentStateNavigazione)
            {
                case StateType.Open:

                    s.tweenGroupNavigazione = new TweenRectTransformGroup(Settings.TransitionSpeed, TweenRectTransformGroup.EaseFunc.EaseInCubic);

                    var togglerRt = Elements.BtnToggle.GetComponent<RectTransform>();

                    s.tweenGroupNavigazione.Elements.Add(new TweenRectTransform(Elements.BtnNext.GetComponent<RectTransform>(), togglerRt.position, Quaternion.identity));
                    s.tweenGroupNavigazione.Elements.Add(new TweenRectTransform(Elements.BtnPrev.GetComponent<RectTransform>(), togglerRt.position, Quaternion.identity));
                    s.tweenGroupNavigazione.Elements.Add(new TweenRectTransform(Elements.BtnRestart.GetComponent<RectTransform>(), togglerRt.position, Quaternion.identity));
                    //s.tweenGroupNavigazione.Elements.Add(new TweenRectTransform(Elements.BtnExit.GetComponent<RectTransform>(), togglerRt.position, Quaternion.identity));
                    s.tweenGroupNavigazione.Elements.Add(new TweenRectTransform(Elements.BtnLineRender.GetComponent<RectTransform>(), togglerRt.position, Quaternion.identity));

                    s.tweenGroupNavigazione.Elements.Add(new TweenRectTransform(Elements.BtnToggle.GetComponent<RectTransform>(), Elements.BtnToggle.GetComponent<RectTransform>().position, Quaternion.Euler(0, 0, 0)));

                    //Elements.BtnToggle.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);

                    s.CurrentStateNavigazione = StateType.CloseTransition;

                    var pos = Elements.BtnToggle.GetComponent<RectTransform>().position;
                    Elements.BtnNext.GetComponent<RectTransform>().position = pos;
                    Elements.BtnPrev.GetComponent<RectTransform>().position = pos;
                    Elements.BtnRestart.GetComponent<RectTransform>().position = pos;
                    Elements.BtnLineRender.GetComponent<RectTransform>().position = pos;
                    //Elements.BtnExit.GetComponent<RectTransform>().position = pos;
                    Elements.BtnToggle.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);
                    s.CurrentStateNavigazione = StateType.Closed;
                    showOnlyToggleButtonNavigazione();

                    break;
            }
        }
        /*
        void closeMenuPercorsi()
        {
            switch (s.CurrentStatePercorsi)
            {
                case StateType.Open:

                    s.tweenGroupPercorsi = new TweenRectTransformGroup(Settings.TransitionSpeed, TweenRectTransformGroup.EaseFunc.EaseInCubic);

                    var togglerRt = Elements.BtnPercorsi.GetComponent<RectTransform>();

                    s.tweenGroupPercorsi.Elements.Add(new TweenRectTransform(Elements.BtnPercorso1.GetComponent<RectTransform>(), togglerRt.position, Quaternion.identity));
                    s.tweenGroupPercorsi.Elements.Add(new TweenRectTransform(Elements.BtnPercorso2.GetComponent<RectTransform>(), togglerRt.position, Quaternion.identity));

                    s.tweenGroupPercorsi.Elements.Add(new TweenRectTransform(Elements.BtnPercorsi.GetComponent<RectTransform>(), Elements.BtnPercorsi.GetComponent<RectTransform>().position, Quaternion.Euler(0, 0, 0)));

                    //Elements.BtnToggle.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);

                    s.CurrentStatePercorsi = StateType.CloseTransition;

                    //showOnlyToggleButton();
                    //var pos = Elements.BtnToggle.GetComponent<RectTransform>().position;
                    //Elements.BtnNext.GetComponent<RectTransform>().position = pos;
                    //Elements.BtnPrev.GetComponent<RectTransform>().position = pos;
                    //Elements.BtnRestart.GetComponent<RectTransform>().position = pos;
                    //Elements.BtnExit.GetComponent<RectTransform>().position = pos;
                    //Elements.BtnToggle.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, 0);
                    //s.CurrentState = StateType.Closed;
                    break;
            }
        }
        */
        void Start()
        {
            _DBClass = GameObject.FindWithTag("SQLite").GetComponent<DBClass>();
            Elements.BtnResizeMinimap.onClick.AddListener(ResizeMinimapPress);
            int percorso = PlayerPrefs.GetInt("percorso");
            if (percorso > 0)
            {
                var _percorsoList = _DBClass.GetPERCORSO(percorso);
                if(_percorsoList != null && _percorsoList.Count > 0)
                {
                    var _percorso = _percorsoList[0];
                    if (_percorso.tipo_navigazione == "IOT")
                    {
                        Elements.BtnResizeMinimap.gameObject.SetActive(false);
                        Elements.BtnNFC.gameObject.SetActive(true);
                        ResizeMinimapPress();
                        Elements.BtnClose.gameObject.SetActive(true);
                    }
                    else
                        Elements.BtnNFC.gameObject.SetActive(false);
                }
                Percorso_N_Go(percorso);
            }
            else
            {
                string _ID = PlayerPrefs.GetString("ID");
                int ID = 0;
                ID = int.Parse(_ID);

                if (ID > 0)
                {
                    List<POI> _POI = _DBClass.getPOI(ID);
                    foreach (DBClass.POI _p in _POI)
                    {
                        if (_p.ID.CompareTo(ID) == 0)
                        {
                            Location loc = new Location(_p.longitudine, _p.latitudine);
                            Settings.MenuController.StartRoute(loc);
                            byte[] app;
                            if (_p._images.Find(match => match.principale) != null)
                                app = _p._images.Find(match => match.principale).image;
                            else
                                app = _p._images[0].image;
                            Sprite sp = null;
                            if (app != null)
                            {
                                Debug.Log("OK caricamento immagine!!!");
                                //var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(tex.width / 2, tex.height / 2));
                                sp = _DBClass.getSpriteFromByteArray(app);
                            }
                            else
                                Debug.Log("Errore caricamento immagine!!!");

                            if (sp != null)
                                Pois.immagine._Image = sp;
                            Pois.name.text = _p.nome;
                            Pois.descrizione.text = "\n\n\n\n\n\n\n";

                            Pois.descrizione.text += _p.descrizione();
                            Pois.img_webpage.color = Color.white;
                            Pois.img_facebook.color = Color.white;
                            Pois.img_instagramm.color = Color.white;
                            Pois.img_telefono.color = Color.white;
                            Pois.img_mail.color = Color.white;

                            Pois.webpage.enabled = true;
                            Pois.facebook.enabled = true;
                            Pois.instagramm.enabled = true;
                            Pois.telefono.enabled = true;
                            Pois.mail.enabled = true;


                            if (!string.IsNullOrEmpty(_p.webPage))
                                Pois.webpage.text = _p.webPage;
                            else
                            {
                                Pois.img_webpage.color = lightgray;
                                Pois.webpage.enabled = false;
                            }
                            if (!string.IsNullOrEmpty(_p.facebook))
                                Pois.facebook.text = _p.facebook;
                            else
                            {
                                Pois.img_facebook.color = lightgray;
                                Pois.facebook.enabled = false;
                            }
                            if (!string.IsNullOrEmpty(_p.instagram))
                                Pois.instagramm.text = _p.instagram;
                            else
                            {
                                Pois.img_instagramm.color = lightgray;
                                Pois.instagramm.enabled = false;
                            }
                            if (!string.IsNullOrEmpty(_p.telefono))
                                Pois.telefono.text = _p.telefono;
                            else
                            {
                                Pois.img_telefono.color = lightgray;
                                Pois.telefono.enabled = false;
                            }
                            if (!string.IsNullOrEmpty(_p.mail))
                                Pois.mail.text = _p.mail;
                            else
                            {
                                Pois.img_mail.color = lightgray;
                                Pois.mail.enabled = false;
                            }
                            Debug.Log("In ARROUT con il tag: " + ID);
                            continue;
                        }
                    }
                    DetailPOI.gameObject.SetActive(false);
                }
            }
            
        }

        void showButtonLabelsNavigazione()
        {
            Elements.LabelNext.gameObject.SetActive(true);
            Elements.LabelPrev.gameObject.SetActive(true);
            Elements.LabelRestart.gameObject.SetActive(true);
            //Elements.LabelSearch.gameObject.SetActive(true);
            Elements.LabelTargetRender.gameObject.SetActive(true);
        }

        void hideButtonLabelsNavigazione()
        {
            Elements.LabelNext.gameObject.SetActive(false);
            Elements.LabelPrev.gameObject.SetActive(false);
            Elements.LabelRestart.gameObject.SetActive(false);
            Elements.LabelTargetRender.gameObject.SetActive(false);

        }
        /*
        void showButtonLabelsPercorsi()
        {
            Elements.LabelPercorsi.gameObject.SetActive(true);
            Elements.LabelPercorso1.gameObject.SetActive(true);
            Elements.LabelPercorso2.gameObject.SetActive(true);
        }

        void hideButtonLabelsPercorsi()
        {
            Elements.LabelPercorsi.gameObject.SetActive(false);
            Elements.LabelPercorso1.gameObject.SetActive(false);
            Elements.LabelPercorso2.gameObject.SetActive(false);

        }
        */


        void Update()
        {
            switch (s.CurrentStateNavigazione)
            {
                case StateType.OpenTransition:
                    if (s.tweenGroupNavigazione.Update())
                    {
                        showButtonLabelsNavigazione();
                        s.CurrentStateNavigazione = StateType.Open;
                    }

                    break;

                case StateType.CloseTransition:
                    if (s.tweenGroupNavigazione.Update())
                    {
                        hideButtonLabelsNavigazione();
                        showOnlyToggleButtonNavigazione();
                        s.CurrentStateNavigazione = StateType.Closed;
                    }
                    break;
            }
            /*
            switch (s.CurrentStatePercorsi)
            {
                case StateType.OpenTransition:
                    if (s.tweenGroupPercorsi.Update())
                    {
                        showButtonLabelsPercorsi();
                        s.CurrentStatePercorsi = StateType.Open;
                    }

                    break;

                case StateType.CloseTransition:
                    if (s.tweenGroupPercorsi.Update())
                    {
                        hideButtonLabelsPercorsi();
                        showOnlyToggleButtonPercorsi();
                        s.CurrentStatePercorsi = StateType.Closed;
                    }
                    break;
            }
            */
        }
        private float default_zoom = 0;
        void ResizeMinimapPress()
        {
            if (Settings.MenuController.MapSize == 512)
            {
                default_zoom = Settings.MenuController.Map.Zoom;
                Settings.MenuController.Map.SetZoom(Settings.MenuController.Map.Zoom - 4.0f);
                Settings.MenuController.MapSize = Screen.height - 220;
                Settings.MenuController.Map.UpdateMap();
                Elements.BtnClose.gameObject.SetActive(false);
            }
            else
            {
                Settings.MenuController.Map.SetZoom(Settings.MenuController.Map.Zoom);
                ResetSizeMinimap();
            }
        }
        public void ResetSizeMinimap()
        {
            Settings.MenuController.Map.SetZoom(18.0f);
            Settings.MenuController.MapSize = 512;
            Settings.MenuController.Map.UpdateMap();
            Elements.BtnClose.gameObject.SetActive(true);
        }
        public void ZeroSizeMinimap()
        {
            Settings.MenuController.MapSize = 0;
            Settings.MenuController.Map.UpdateMap();
            Elements.BtnClose.gameObject.SetActive(true);
        }
    }

    #endregion
}
