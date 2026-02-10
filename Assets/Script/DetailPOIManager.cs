using System;
using System.Collections.Generic;
using TMPro;
using TS.PageSlider.Demo;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using static ARLocation.MapboxRoutes.SampleProject.ArMenuController;
using static DBClass;

public class DetailPOIManager : MonoBehaviour, IPointerClickHandler
{

    public Canvas DetailPOI;
    public Text ID_selected;
    public Button portami_la;
    public POIData Pois;
    //public GameObject PanelDownOfPanlMap;

    public bool _from_search_filter = false;




    private double _latitudine_scelta;
    private double _longitudine_scelta;
    private PERCORSO _percorso_associato;
    private DBClass _DBClass;
    private int _lingua_selezionata = 1;
    private Color lightgray = new Color(0.8f, 0.8f, 0.8f, 1.0f);

    private void Start()
    {
        _DBClass = GameObject.FindWithTag("SQLite").GetComponent<DBClass>();
        _lingua_selezionata = PlayerPrefs.GetInt("lingua_selezionata");
    }
    private void OnDisable()
    {
        Pois.immagine.gameObject.SetActive(true);
        Pois.scrollview_list_img.Clear();
        Pois.scrollview.transform.localPosition = new Vector3(0, 0, 0);
    }
    private class DettaglioItinerario
    {
        public string label;
        public string value;
    }
    public void OpenDetailAtID(string _ID, bool from_search_filter, bool from_comune, int tipo = 3)
    {
        _lingua_selezionata = PlayerPrefs.GetInt("lingua_selezionata");

        Pois.scrollview_list_img.Clear();

        if (GameObject.FindObjectOfType<GestioneTab>() != null)
        {
            GameObject.FindObjectOfType<GestioneTab>().selectes_btn_index(0);
        }
        if (GameObject.FindObjectOfType<ItinerariEventiPOI_AttaccatiAlComune>() != null)
            GameObject.FindObjectOfType<ItinerariEventiPOI_AttaccatiAlComune>().BtnIDescrizioneClicked();
        _DBClass = GameObject.FindWithTag("SQLite").GetComponent<DBClass>();
        
        _from_search_filter = from_search_filter;
        //Debug.Log($"Arrivo qui con id: {ID} tipo: {tipo}");
        if (PlayerPrefs.GetInt("show_grid_poi") == 2)
            PlayerPrefs.SetInt("show_grid_poi", 1);
        if (!DetailPOI.enabled || from_comune)
        {
            if (_ID  != "")
            {
                Pois.descrizione.text = string.Empty;
                Pois.scrollview_list_img.gameObject.SetActive(true);
                //POI
                if (tipo == 3)
                {
                    long lID;
                    if (long.TryParse(_ID, out lID))
                    {
                        List<POI> _POI = _DBClass.getPOI(lID, null, null, 0, null, null, true);
                        if (Pois.dettaglioItinerario != null)
                            Pois.dettaglioItinerario.SetActive(false);
                        if (Pois.Btn_percorso_associato != null)
                            Pois.Btn_percorso_associato.gameObject.SetActive(false);
                        foreach (DBClass.POI _p in _POI)
                        {
                            if (_p != null)
                            {

                                ID_selected.text = _ID;
                                DetailPOI.enabled = true;
                                if (portami_la != null)
                                    portami_la.gameObject.SetActive(true);
                                //if (PanelDownOfPanlMap != null)
                                //    PanelDownOfPanlMap.SetActive(false);

                                Pois.immagine.gameObject.SetActive(true);
                                float _dimensione_foto = 1240;
                                if (_p._images != null && _p._images.Count > 0)
                                {
                                    Pois.descrizione.text = "\n\n\n\n\n\n\n";

                                    Debug.Log($"foto da visualizzare {_p._images.Count}");
                                    int _i = 0;
                                    foreach (var _img in _p._images)
                                    {

                                        var goApp = Instantiate(Pois.immagine);
                                        goApp.gameObject.SetActive(true);
                                        var pos = Pois.immagine.transform.position;
                                        goApp.transform.position = new Vector3(0 + (_dimensione_foto * _i), -400, pos.z);
                                        goApp.transform.localScale = new Vector3(1, 1, 1);
                                        //descrizione dell'immagine
                                        if (!string.IsNullOrEmpty(_img.descrizione) && goApp.GetComponentsInChildren<Text>() != null)
                                        {
                                            foreach (var aptext in goApp.GetComponentsInChildren<Text>())
                                            {
                                                if (aptext.name == "descrizione")
                                                    aptext.text = _img.descrizione;
                                                if (aptext.name == "count" && _p._images.Count > 1)
                                                    aptext.text = (_i + 1) + "/" + _p._images.Count;
                                            }
                                        }
                                        goApp._Image = _DBClass.getSpriteFromByteArray(_img.image);
                                        Pois.scrollview_list_img.AddPage((RectTransform)goApp.transform);
                                        _i++;
                                    }
                                    //float delta = 0;
                                    //if(_i > 1)
                                    //    delta = (_dimensione_foto * (_i - 1));
                                    //Pois.content_list_img.GetComponent<RectTransform>().sizeDelta = new Vector2(delta, 0);
                                    Pois.immagine.gameObject.SetActive(false);
                                    Pois.scrollview_list_img.transform.SetParent(Pois.descrizione.transform, false);


                                }
                                else
                                {
                                    //var texture = Resources.Load<Texture2D>("Foto/no_images");
                                    //texture.Apply();
                                    //Pois.immagine._Image = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
                                    //Pois.content_list_img.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
                                    //Pois.scrollview_list_img.transform.SetParent(Pois.descrizione.transform, false);
                                    Pois.scrollview_list_img.gameObject.SetActive(false);
                                }
                                //Location loc = new Location(_p.latitudine, _p.longitudine);
                                if (Pois.name != null)
                                    Pois.name.text = _p.nome;
                                if (Pois.nameComune != null)
                                {
                                    Pois.nameComune.text = $"{_p.comune}";
                                }
                                if (Pois.tipo != null)
                                    Pois.tipo.text = _p.tipo_list_descrizione();
                                if (Pois.descrizione != null)
                                {
                                    var breve = _p.descrizione_breve();
                                    var lunga = _p.descrizione();
                                    if (!string.IsNullOrEmpty(breve))
                                        Pois.descrizione.text += $"<i>{breve}</i>\n\n\n\n";
                                    if (!string.IsNullOrEmpty(lunga))
                                        Pois.descrizione.text += lunga;
                                }
                                Pois.img_webpage.color = Color.black;
                                Pois.img_facebook.color = Color.black;
                                Pois.img_instagramm.color = Color.black;
                                Pois.img_telefono.color = Color.black;
                                Pois.img_mail.color = Color.black;

                                Pois.webpage.enabled = true;
                                Pois.facebook.enabled = true;
                                Pois.instagramm.enabled = true;
                                Pois.telefono.enabled = true;
                                Pois.mail.enabled = true;


                                if (!string.IsNullOrEmpty(_p.webPage))
                                    Pois.webpage.text = _p.webPage;
                                else
                                {
                                    Pois.webpage.text = string.Empty;
                                    Pois.img_webpage.color = lightgray;
                                    Pois.webpage.enabled = false;
                                }
                                if (!string.IsNullOrEmpty(_p.facebook))
                                    Pois.facebook.text = _p.facebook;
                                else
                                {
                                    Pois.facebook.text = string.Empty;
                                    Pois.img_facebook.color = lightgray;
                                    Pois.facebook.enabled = false;
                                }
                                if (!string.IsNullOrEmpty(_p.instagram))
                                    Pois.instagramm.text = _p.instagram;
                                else
                                {
                                    Pois.instagramm.text = string.Empty;
                                    Pois.img_instagramm.color = lightgray;
                                    Pois.instagramm.enabled = false;
                                }
                                if (!string.IsNullOrEmpty(_p.telefono))
                                    Pois.telefono.text = _p.telefono;
                                else
                                {
                                    Pois.telefono.text = string.Empty;
                                    Pois.img_telefono.color = lightgray;
                                    Pois.telefono.enabled = false;
                                }
                                if (!string.IsNullOrEmpty(_p.mail))
                                    Pois.mail.text = _p.mail;
                                else
                                {
                                    Pois.mail.text = string.Empty;
                                    Pois.img_mail.color = lightgray;
                                    Pois.mail.enabled = false;
                                }
                                foreach (var _poixtappe in _DBClass.getPOIXTAPPE(null, _p.ID, null))
                                {
                                    var ListTappexPercorsi = _DBClass.getTAPPEXPERCORSI(null, _poixtappe.tappa_id);

                                    if (Pois.percorso_associato != null || (ListTappexPercorsi != null && ListTappexPercorsi.Count > 0))
                                    {

                                        if (_p.percorsi_associati)
                                        {
                                            Debug.Log("percorso associato tramite GetPERCORSO");
                                            List<PERCORSO> _PERCORSOList = _DBClass.GetPERCORSO(null, lID);
                                            if (_PERCORSOList != null && _PERCORSOList.Count > 0)
                                            {
                                                _percorso_associato = _PERCORSOList[0];
                                            }
                                        }
                                        else if (ListTappexPercorsi.Count > 0)
                                        {
                                            Debug.Log("percorso associato tramite getPOIXTAPPE e getTAPPEXPERCORSI");
                                            var _percorso = _DBClass.GetPERCORSO(ListTappexPercorsi[0].id);
                                            if (_percorso != null && _percorso.Count > 0)
                                                _percorso_associato = _percorso[0];
                                        }
                                        else
                                        {
                                            Debug.Log("percorso non associato 2");
                                        }
                                    }
                                    else
                                    {
                                        Debug.Log("percorso non associato 1");
                                    }
                                }
                                _latitudine_scelta = _p.longitudine;
                                _longitudine_scelta = _p.latitudine;
                                DetailPOI.gameObject.SetActive(true);
                                var ap = Pois.scrollview.GetComponent<RectTransform>().offsetMin;
                                ap.y = 208;
                                Pois.scrollview.GetComponent<RectTransform>().offsetMin = ap;
                            }
                        }
                    }
                }
                else if (tipo == 1)
                {
                    int ID = int.Parse(_ID);
                    var _pxtList = _DBClass.getPOIXTAPPE(null, null, null, ID);
                    var _poiList = _DBClass.getPOI(null, null, null, 0, null, null, null, null, ID);
                    List<PERCORSO> _PERCORSO = _DBClass.GetPERCORSO(ID);
                    foreach (DBClass.PERCORSO _p in _PERCORSO)
                    {
                        if (_p != null)
                        {
                            ID_selected.text = _ID;
                            DetailPOI.enabled = true;
                            portami_la.gameObject.SetActive(true);
                            //if (PanelDownOfPanlMap != null)
                            //    PanelDownOfPanlMap.SetActive(false);

                                Pois.immagine.gameObject.SetActive(true);

                            float _dimensione_foto = 1240;
                            if (_p.Listimages != null && _p.Listimages.Count > 0)
                            {

                                Pois.descrizione.text = "\n\n\n\n\n\n\n";

                                Debug.Log($"foto da visualizzare {_p.Listimages.Count}");
                                int _i = 0;
                                foreach (var _img in _p.Listimages)
                                {

                                    var goApp = Instantiate(Pois.immagine);
                                    goApp.gameObject.SetActive(true);
                                    var pos = Pois.immagine.transform.position;
                                    goApp.transform.position = new Vector3(0 + (_dimensione_foto * _i), -400, pos.z);
                                    goApp.transform.localScale = new Vector3(1, 1, 1);
                                    //descrizione dell'immagine
                                    if (!string.IsNullOrEmpty(_img.descrizione) && goApp.GetComponentsInChildren<Text>() != null)
                                    {
                                        foreach (var aptext in goApp.GetComponentsInChildren<Text>())
                                        {
                                            if (aptext.name == "descrizione")
                                                aptext.text = _img.descrizione;
                                            if (aptext.name == "count" && _p.Listimages.Count > 1)
                                                aptext.text = (_i + 1) + "/" + _p.Listimages.Count;
                                        }
                                    }
                                    //goApp.transform.SetParent(Pois.content_list_img.transform, false);
                                    goApp._Image = _DBClass.getSpriteFromByteArray(_img.image);
                                    Pois.scrollview_list_img.AddPage((RectTransform)goApp.transform);

                                    _i++;
                                }
                                //float delta = 0;
                                //if (_i > 1)
                                //    delta = (_dimensione_foto * (_i - 1));
                                //Pois.content_list_img.GetComponent<RectTransform>().sizeDelta = new Vector2(delta, 0);
                                Pois.immagine.gameObject.SetActive(false);
                                Pois.scrollview_list_img.transform.SetParent(Pois.descrizione.transform, false);
                            }
                            else
                            {
                                //var texture = Resources.Load<Texture2D>("Foto/no_images");
                                //texture.Apply();
                                //Pois.immagine._Image = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
                                //Pois.content_list_img.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
                                //Pois.scrollview_list_img.transform.SetParent(Pois.descrizione.transform, false);
                                Pois.scrollview_list_img.gameObject.SetActive(false);
                            }
                            


                            //Location loc = new Location(_p.latitudine, _p.longitudine);
                            Pois.name.text = _p.nome_percorso;
                            Pois.nameComune.text = "";
                            Pois.tipo.text = _lingua_selezionata == 1 ? "Itinerario": "Itinerary";
                            if(_p.descrizione != null && _p.descrizione.Count > 0)
                                Pois.descrizione.text += _p.descrizione[0].descrizione + "\n\n\n\n";
                            // Aggiungo le tappe
                            bool tappe = false;
                            foreach( var txp in _DBClass.getTAPPEXPERCORSI(null, null, _p.id))
                            {
                                foreach(var _t in  _DBClass.getTAPPE(_lingua_selezionata, txp.tappa_id))
                                {
                                    Pois.descrizione.text += "<br><b><sprite name=\"tappa\"><color=#E8531E>" + _t.nome_tappa + "</color></b>"+ "\n";
                                    if (_t.tappe_text != null && _t.tappe_text.Count > 0)
                                    {
                                        tappe = true;
                                        TAPPE_TEXT _tt = _t.tappe_text[0];
                                        if (!string.IsNullOrEmpty(_tt.descrizione_breve))
                                            Pois.descrizione.text +=  "<i>" + _tt.descrizione_breve + "</i>" + "\n";
                                        if (!string.IsNullOrEmpty(_tt.descrizione))
                                            Pois.descrizione.text += _tt.descrizione + "\n";
                                        foreach(var _pxt in _pxtList.FindAll(p=> p.tappa_id == _t.id))
                                        {
                                            foreach(var _poi in _poiList.FindAll(p=> p.ID == _pxt.id))
                                            {
                                                if (!string.IsNullOrEmpty(_poi.nome))
                                                {
                                                    Pois.descrizione.text += "<br><link=\""+ _poi.ID +"\"><sprite name=\"poi\"><color=#E8531E><b>" + _poi.nome + "</b></color></link><br>";
                                                    if (!string.IsNullOrEmpty(_poi.descrizione_breve()))
                                                        Pois.descrizione.text += "<i>" + _poi.descrizione_breve() + "</i>" + "\n";
                                                    if (!string.IsNullOrEmpty(_poi.descrizione()))
                                                        Pois.descrizione.text += _poi.descrizione() + "\n";
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if(tappe)
                                Pois.descrizione.text += "\n\n\n\n";
                            Pois.descrizione.text = _DBClass.pulisciHTML(Pois.descrizione.text);
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


                            /*if (!string.IsNullOrEmpty(_p.webPage))
                                Pois.webpage.text = _p.webPage;
                            else
                            {*/
                                Pois.webpage.text = string.Empty;
                                Pois.img_webpage.color = lightgray;
                                Pois.webpage.enabled = false;
                            /*}
                            if (!string.IsNullOrEmpty(_p.facebook))
                                Pois.facebook.text = _p.facebook;
                            else
                            {*/
                                Pois.facebook.text = string.Empty;
                                Pois.img_facebook.color = lightgray;
                                Pois.facebook.enabled = false;
                            /*}
                            if (!string.IsNullOrEmpty(_p.instagram))
                                Pois.instagramm.text = _p.instagram;
                            else
                            {*/
                                Pois.instagramm.text = string.Empty;
                                Pois.img_instagramm.color = lightgray;
                                Pois.instagramm.enabled = false;
                            /*}
                            if (!string.IsNullOrEmpty(_p.telefono))
                                Pois.telefono.text = _p.telefono;
                            else
                            {*/
                                Pois.telefono.text = string.Empty;
                                Pois.img_telefono.color = lightgray;
                                Pois.telefono.enabled = false;
                            /*}
                            if (!string.IsNullOrEmpty(_p.mail))
                                Pois.mail.text = _p.mail;
                            else
                            {*/
                                Pois.mail.text = string.Empty;
                                Pois.img_mail.color = lightgray;
                                Pois.mail.enabled = false;
                            //}

                            Color _c = new Color();
                            if (ColorUtility.TryParseHtmlString("#E8531E", out _c))
                                Pois.img_percorso_associato.color = _c;

                            Pois.percorso_associato.enabled = true;
                            //Pois.percorso_associato.text = _p.nome_percorso;
                            /*
                            if (_p.percorsi_associati)
                            {
                                List<PERCORSO> _PERCORSOList = _DBClass.GetPERCORSO(null, ID);
                                if (_PERCORSOList != null && _PERCORSOList.Count > 0)
                                {
                                    _percorso_associato = _PERCORSOList[0];
                                    Pois.percorso_associato.text = _percorso_associato.nome_percorso;
                                }
                            }
                            else
                            {
                                Pois.percorso_associato.text = string.Empty;
                                Pois.img_percorso_associato.color = lightgray;
                                Pois.percorso_associato.enabled = false;
                            }
                            
                            _latitudine_scelta = _p.longitudine;
                            _longitudine_scelta = _p.latitudine;
                            */
                            Pois.Btn_percorso_associato.gameObject.SetActive(true);
                            //Pois.show_percorso_associato.text = string.Empty;
                            if (string.IsNullOrEmpty(_p.percorso))
                            {
                                //Pois.percorso_associato.text = string.Empty;
                                Pois.img_percorso_associato.color = lightgray;
                                //Pois.percorso_associato.enabled = false;
                            }
                            DetailPOI.gameObject.SetActive(true);
                            var ap = Pois.scrollview.GetComponent<RectTransform>().offsetMin;
                            ap.y = 200;
                            Pois.scrollview.GetComponent<RectTransform>().offsetMin = ap;

                            if (Pois.dettaglioItinerario != null)
                            {
                                Pois.dettaglioItinerario.SetActive(true);
                                Pois.dettaglioItinerario.transform.SetParent(Pois.descrizione.transform, false);
                                //Pois.dettaglioItinerario.transform.position = Vector3.zero;
                                //Debug.Log((Pois.dettaglioItinerario.transform.localPosition.y));
                                //Debug.Log((Pois.dettaglioItinerario.transform.localPosition.y - 2.0));
                                int tab = 150;
                                int altezza_pageslider = -402;
//#if UNITY_IOS
//			                    altezza_pageslider = -802;
//#endif

                                Pois.dettaglioItinerario.transform.localPosition = new Vector3(0, 0 + tab, 0);// .Translate(new Vector3(0, -1 * (Pois.dettaglioItinerario.transform.localPosition.y), 0));
                                if (_p.Listimages != null && _p.Listimages.Count != 0)
                                //    Pois.dettaglioItinerario.transform.localPosition = new Vector3(0, 0, 0);// .Translate(new Vector3(0, -2, 0));
                                //else
                                    Pois.dettaglioItinerario.transform.localPosition = new Vector3(0, altezza_pageslider + tab, 0);// Pois.dettaglioItinerario.transform.position = new Vector3(0, -802, 0);

                                int indice_dettaglio = 0;

                                List<DettaglioItinerario> dettagli_da_scrivere = new List<DettaglioItinerario>();
                                string t = "";
                                if (!string.IsNullOrEmpty(_p.tipo_percorso))
                                    t = _p.tipo_percorso;
                                if (!string.IsNullOrEmpty(_p.tipo_navigazione))
                                {
                                    if (!string.IsNullOrEmpty(t))
                                        t += ", ";
                                    t += _p.tipo_navigazione;
                                }
                                if(!string.IsNullOrEmpty(t))
                                    dettagli_da_scrivere.Add(new DettaglioItinerario() { label = _lingua_selezionata == 1 ? "Tipologia" : "Typology", value = t });

                                t = "";
                                if (!string.IsNullOrEmpty(_p.dislivello))
                                    t = _p.dislivello;
                                if (!string.IsNullOrEmpty(_p.lunghezza))
                                {
                                    if (!string.IsNullOrEmpty(t))
                                        t += ", ";
                                    //t += _lingua_selezionata == 1 ? "lunghezza " : "length ";
                                    t += _p.lunghezza + " km";
                                }
                                if (!string.IsNullOrEmpty(t))
                                    dettagli_da_scrivere.Add(new DettaglioItinerario() { label = _lingua_selezionata == 1 ? "Difficoltà" : "Difficulty", value = t });

                                t = "";
                                if (!string.IsNullOrEmpty(_p.pendenza))
                                    t = _p.pendenza;
                                if (!string.IsNullOrEmpty(t))
                                    dettagli_da_scrivere.Add(new DettaglioItinerario() { label = _lingua_selezionata == 1 ? "Pendenza max" : "Max slope", value = t });

                                t = "";
                                if (!string.IsNullOrEmpty(_p.adatto_a))
                                    t = _p.adatto_a;
                                if (!string.IsNullOrEmpty(t))
                                    dettagli_da_scrivere.Add(new DettaglioItinerario() { label = _lingua_selezionata == 1 ? "Adatto a" : "Suitable for", value = t });

                                t = "";
                                if (!string.IsNullOrEmpty(_p.accessibilita))
                                    t = _p.accessibilita;
                                if (!string.IsNullOrEmpty(t))
                                    dettagli_da_scrivere.Add(new DettaglioItinerario() { label = _lingua_selezionata == 1 ? "Accessibilità" : "Accessibility", value = t });

                                t = "";
                                if (!string.IsNullOrEmpty(_p.tempo_percorrenza))
                                    t = _p.tempo_percorrenza;
                                if (!string.IsNullOrEmpty(t))
                                    dettagli_da_scrivere.Add(new DettaglioItinerario() { label = _lingua_selezionata == 1 ? "Tempo stimato" : "Travel time", value = t });

                                t = "";
                                //if (!string.IsNullOrEmpty(_p.tempo_percorrenza))
                                var poi = _DBClass.getPOI(_p.poi_id);
                                if(poi != null)
                                    t = $"{(Math.Sqrt((Math.Pow(poi[0].longitudine - _DBClass._longitudine, 2) + Math.Pow(poi[0].latitudine - _DBClass._latitudine, 2))) * 100).ToString("0.##")} km";

                                //"100 km";
                                if (!string.IsNullOrEmpty(t))
                                    dettagli_da_scrivere.Add(new DettaglioItinerario() { label = _lingua_selezionata == 1 ? "Distanza da te" : "Distance from you", value = t });
                                foreach (var aptext in Pois.ItemsItinerari)
                                    aptext.gameObject.SetActive(false);
                                foreach (var aptext in Pois.ItemsItinerari)
                                {

                                    if (indice_dettaglio < dettagli_da_scrivere.Count)
                                    {
                                        if (aptext.name == $"{(indice_dettaglio + 1)}")
                                        {
                                            aptext.gameObject.SetActive(true);
                                            foreach (var ap_inside in aptext.GetComponentsInChildren<Text>())
                                            {
                                                if (ap_inside.name == "label")
                                                {
                                                    ap_inside.text = dettagli_da_scrivere[indice_dettaglio].label;
                                                }
                                                else if (ap_inside.name == "value")
                                                {
                                                    ap_inside.text = dettagli_da_scrivere[indice_dettaglio].value;
                                                }
                                            }
                                            Pois.descrizione.text = "\n\n" + Pois.descrizione.text;
                                        }
                                    }
                                    indice_dettaglio++;
                                }
                                //Pois.descrizione.text = "\n\n\n\n\n\n\n\n\n\n"+ Pois.descrizione.text;
                            }
                        }
                    }
                }
            }
        }
    }
    
    public void CloseDetailPOI()
    {
        if (PlayerPrefs.GetInt("show_grid_poi") == 2)
            PlayerPrefs.SetInt("show_grid_poi", 1);
        // Create a temporary reference to the current scene.
        // Retrieve the name of this scene.
        if (SceneManager.GetActiveScene().name == "Map")
        {
            if (GameObject.FindObjectOfType<ICanvas>() != null)
            {
                GameObject.FindObjectOfType<ICanvas>().search_filterPOI = _from_search_filter;
                GameObject.FindObjectOfType<ICanvas>().panel_all_White.SetActive(_from_search_filter);
            }
        }
        /*
        else
        {
            ChangeScene cs = new ChangeScene();
            cs.Load_Map();
        }
        */
        //if(PanelDownOfPanlMap != null)
        //    PanelDownOfPanlMap.SetActive(true);
        DetailPOI.enabled = false;
        DetailPOI.gameObject.SetActive(false);
        portami_la.gameObject.SetActive(false);
    }
    public void Mostra_nella_mappa()
    {
        Debug.Log("Mostra_nella_mappa");
        //if (SceneManager.GetActiveScene().name == "Menu")
        //{
            PlayerPrefs.SetString("id_select", ID_selected.text);
            PlayerPrefs.SetString("show_on_freemap", "1");
            PlayerPrefs.SetString("SpostaCentro_freemap", _longitudine_scelta.ToString() + "_" + _latitudine_scelta.ToString());
        //}
        CloseDetailPOI();
        PlayerPrefs.SetInt("show_grid_poi", 0);
        
        if (GameObject.FindObjectOfType<ICanvas>() != null)
            GameObject.FindObjectOfType<ICanvas>().search_filterPOI = false;
        //if (PanelDownOfPanlMap != null)
        //    PanelDownOfPanlMap.SetActive(true);

        DetailPOI.enabled = false;
        if (GameObject.FindObjectOfType<Panel_POI>().isActiveAndEnabled)
            GameObject.FindObjectOfType<Panel_POI>().id_selected.text = ID_selected.text;
        if (GameObject.FindObjectOfType<FreeMap>() != null)
            GameObject.FindObjectOfType<FreeMap>().SpostaCentro(_longitudine_scelta, _latitudine_scelta);
    }
    public void set_ID_selected_NULL()
    {
        if (!string.IsNullOrEmpty(ID_selected.text))
        {
            ID_selected.text = string.Empty;
            GameObject.FindObjectOfType<FreeMap>().Ricalcola_Centro(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // First, get the index of the link clicked. Each of the links in the text has its own index.
        var linkIndex = TMP_TextUtilities.FindIntersectingLink(Pois.descrizione, Input.mousePosition, null);
        if (linkIndex >= 0)
        {
            // As the order of the links can vary easily (e.g. because of multi-language support),
            // you need to get the ID assigned to the links instead of using the index as a base for our decisions.
            // you need the LinkInfo array from the textInfo member of the TextMesh Pro object for that.
            var linkId = Pois.descrizione.textInfo.linkInfo[linkIndex].GetLinkID();

            // Now finally you have the ID in hand to decide what to do. Don't forget,
            // you don't need to make it act like an actual link, instead of opening a web page,
            // any kind of functions can be called.

            Debug.Log($"URL clicked: linkInfo[{linkIndex}].id={linkId}");

            // Let's see that web page!
            //Application.OpenURL(url);
            PlayerPrefs.SetInt("poi_selezionato_collegato_ad_un_percorso", int.Parse(linkId));
        }
    }
}
