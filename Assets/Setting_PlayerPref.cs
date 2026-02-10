using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class Setting_PlayerPref : MonoBehaviour
{



    private DBClass _DBClass;
    public Dropdown _grid_comune;

    private List<int> _comuni_id;
    public ChangeScene change;

    // Start is called before the first frame update
    void Start()
    {
        _DBClass = GameObject.FindWithTag("SQLite").GetComponent<DBClass>();
        _comuni_id = new List<int>();
        var _lingua_selezionata = PlayerPrefs.GetInt("lingua_selezionata");
        string testo = (_lingua_selezionata == 1) ? "Tutti i comuni" : "All municipalities";
        List<Dropdown.OptionData> _list_comuni = new List<Dropdown.OptionData>
        {
            new Dropdown.OptionData() { text = testo}
        };
        _comuni_id.Add(0);
        var _comune_selected = 0;
        if (PlayerPrefs.HasKey("comune_selected"))
            _comune_selected = PlayerPrefs.GetInt("comune_selected");
        int i = 1;
        int _selected_i = 0;
        foreach (var _comune in _DBClass.GetCOMUNI(string.Empty, string.Empty, null, null, true))
        {
            _list_comuni.Add(new Dropdown.OptionData() { text = _comune.nome_comune + $" ({_comune.provincia})" });
            _comuni_id.Add(_comune.id);
            if (_comune.id == _comune_selected)
                _selected_i = i;
            i++;
        }
        _grid_comune.GetComponent<Dropdown>().options = _list_comuni;
        _grid_comune.value = _selected_i;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void saveSetting()
    {
        var _old_comune_selected = 0;
        if (PlayerPrefs.HasKey("comune_selected"))
        {
            _old_comune_selected = PlayerPrefs.GetInt("comune_selected");
        }
        Debug.Log("savesetting " + _old_comune_selected + " ___ " + _comuni_id[_grid_comune.value]);
        if (_old_comune_selected != _comuni_id[_grid_comune.value])
        {
            Debug.Log("savesetting2 " + _grid_comune.value);
            Debug.Log("comune_selected. modifico" + _grid_comune.value);
            //if (_grid_comune.value == 0)
            //    PlayerPrefs.DeleteKey("comune_selected");
            //else
            PlayerPrefs.SetInt("comune_selected", _comuni_id[_grid_comune.value]);
            PlayerPrefs.Save();
            change.Load_Menu();
        }
    }
}
