using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.CanvasScaler;

public class GestioneTab : MonoBehaviour
{
    public Text selected_tab;
    public List<Button> _btn_List;
    public bool _check_per_CanvasPOI = true;
    public ReadForMe _readForMe;
    private int _lingua_selezionata = 1;
    private DBClass _DBClass;


    // Start is called before the first frame update
    void Start()
    {

        _lingua_selezionata = PlayerPrefs.GetInt("lingua_selezionata");
        _DBClass = GameObject.FindWithTag("SQLite").GetComponent<DBClass>();
        if (_lingua_selezionata == 2)
        {
            foreach (var _btn in _btn_List)
            {
                foreach (var _cmp in _btn.GetComponentsInChildren<Text>())
                {
                    if (_cmp.name == "Text")
                    {

                        switch (_cmp.text)
                        {
                            case "LUOGHI":
                                _cmp.text = "PLACES";
                                break;
                            case "ITINERARI":
                                _cmp.text = "ITINERARIES";
                                break;
                            case "PUNTI DI INTERESSE":
                                _cmp.text = "POINTS OF INTEREST";
                                break;
                            case "DESCRIZIONE":
                                _cmp.text = "DESCRIPTION";
                                break;
                            case "EVENTI":
                                _cmp.text = "EVENTS";
                                break;
                        }
                    }
                }
            }
        }
    }

    private void Update()
    {
        if (_check_per_CanvasPOI)
        {

            if (PlayerPrefs.GetString("percorso_selezionato") != "")
            {
                var app = long.Parse(PlayerPrefs.GetString("percorso_selezionato"));
                if (app < 0)
                    app = app * -1;
                if (_DBClass.getPOIXTAPPE(null, null, null, app).Count > 0)
                {
                    _btn_List[2].gameObject.SetActive(true);
                    if (!_btn_List[2].enabled)
                        _btn_List[2].enabled = true;
                }
                else
                    _btn_List[2].enabled = true;
            }
            else if (PlayerPrefs.GetString("poi_selezionato") != "")
            {
                if (_DBClass.getPOIXTAPPE(null, int.Parse(PlayerPrefs.GetString("poi_selezionato")), null, null).Count > 0)
                {
                    _btn_List[1].gameObject.SetActive(true);
                    if (!_btn_List[1].enabled)
                        _btn_List[1].enabled = true;
                }
                else
                    _btn_List[1].enabled = false;
            }
            if (PlayerPrefs.GetString("percorso_selezionato") != "" && _btn_List[1].gameObject.activeSelf)
            {
                selectes_btn_index(0);
                _btn_List[1].gameObject.SetActive(false);
            }
            else if (!(PlayerPrefs.GetString("percorso_selezionato") != "") && !_btn_List[1].gameObject.activeSelf)
            {
                _btn_List[1].gameObject.SetActive(true);
                /*
                if (!_btn_List[1].enabled)
                    _btn_List[1].enabled = true;
                    */
            }
            if (PlayerPrefs.GetString("percorso_selezionato") != "" && !_btn_List[2].gameObject.activeSelf)
            {
                _btn_List[2].gameObject.SetActive(true);
                /*
                if (!_btn_List[2].enabled)
                    _btn_List[2].enabled = true;
                    */
            }
            else if (!(PlayerPrefs.GetString("percorso_selezionato") != "") && _btn_List[2].gameObject.activeSelf)
                _btn_List[2].gameObject.SetActive(false);
        }
    }
    public void selectes_btn_index(int i)
    {
        if (selected_tab != null)
            selected_tab.text = $"{i}";
        int app_i = 0;
        if (_btn_List != null)
        {

            foreach (var _btn in _btn_List)
            {
                if (app_i == i)
                    _btn.GetComponent<BtnTabClick>().SetCliccked(true);
                else
                    _btn.GetComponent<BtnTabClick>().SetCliccked(false);
                //if (!_btn.gameObject.activeSelf)
                //    _btn.gameObject.SetActive(true);
                app_i++;
            }
        }
        PlayerPrefs.SetInt($"show_comuni", 0);
        PlayerPrefs.SetInt($"show_itinerari", 0);
        PlayerPrefs.SetInt($"show_poi", 0);
        PlayerPrefs.SetInt($"show_eventi", 0);

        //luoghi/descrizione
        if (i == 0)
        {
            //Debug.Log("comuni");
            PlayerPrefs.SetInt($"show_comuni", 1);
        }
        //itinerari
        if (i == 1)
        {
            //Debug.Log("itinerari");
            PlayerPrefs.SetInt($"show_itinerari", 1);
        }
        //
        if (i == 2)
        {
            //Debug.Log("poi");
            PlayerPrefs.SetInt($"show_poi", 1);
        }
        if (i == 3)
        {
            //Debug.Log("poi");
            PlayerPrefs.SetInt($"show_eventi", 1);
        }
        if (_readForMe != null)
        {
            _readForMe.StopSpeak();
            _readForMe.OpenCloseReader(false);
        }
    }

}
