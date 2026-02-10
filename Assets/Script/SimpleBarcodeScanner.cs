using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vuforia;
using static DBClass;

public class SimpleBarcodeScanner : MonoBehaviour
{
    public ChangeScene change;
    private DBClass _DBClass;
    BarcodeBehaviour mBarcodeBehaviour;
    void Start()
    {
        mBarcodeBehaviour = GetComponent<BarcodeBehaviour>();
        _DBClass = GameObject.FindWithTag("SQLite").GetComponent<DBClass>();

    }
    // Update is called once per frame
    void Update()
    {
        if (mBarcodeBehaviour != null && mBarcodeBehaviour.InstanceData != null)
        {
            var url = mBarcodeBehaviour.InstanceData.Text;
            var arr_slug = url.Split("/");
            var slug = "";
            foreach (var app in arr_slug)
            {
                if (app != "")
                    slug = app;
            }
            if (arr_slug.Last() != "")
            {
                List<POI> _POI = _DBClass.getPOI(null, null, null, 0, null, null, false, false, null, slug);
                if (_POI != null && _POI.Count() > 0)
                {
                    POI _poi = _POI.FirstOrDefault();
                    PlayerPrefs.SetString("poi_selezionato", _poi.ID.ToString());
                    PlayerPrefs.SetString("istat", _poi.istat);
                    PlayerPrefs.SetString("apri_direttamente_il_poi_selezionato", _poi.ID.ToString());
                    change.Load_Menu_Without_reset_Player();
                    mBarcodeBehaviour = null;
                    Debug.Log("Ho trovato il poi e devo aprirlo!!");
                }
                else
                {
                    Application.OpenURL(url);
                }
            }
            else
            {
                Application.OpenURL(url);
            }
        }

    }
}