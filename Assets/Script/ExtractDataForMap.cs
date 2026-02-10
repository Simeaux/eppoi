using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DBClass;

public class ExtractDataForMap : MonoBehaviour
{
    private List<POI> PoiList;
    private List<PERCORSO> PercorsoList;
    private List<TAPPEXPERCORSI> TappeXPercorsiList;
    private DBClass _DBClass;
    // Start is called before the first frame update
    public void getstart()
    {
        _DBClass = GameObject.FindWithTag("SQLite").GetComponent<DBClass>();
        Debug.Log("Extract");
        //PoiList = _DBClass.getPOI(null, null, null, 100, null, null, false);
        PoiList = _DBClass.getPOIxMap("");
        PercorsoList = _DBClass.GetPERCORSO(null, null, true);
        TappeXPercorsiList = _DBClass.getTAPPEXPERCORSI(null, null, null);
    }

    public void aggiornaPoiList(double[] punti = null)
    {
        string not_in = "";
        /*foreach (var _in in PoiList)
        {
            if (not_in != "")
                not_in = not_in + ", ";
            not_in = not_in + _in.ID;
        }
        */
        setPOIList(new List<POI>());
        setPOIList(_DBClass.getPOIxMap(not_in, punti));
        
    }
    public List<POI> getPoiList()
    {
        if (PoiList == null)
            PoiList = new List<POI>();
        return PoiList;
    }
    public void setPOIList(List<POI> _poilist)
    {
        PoiList = new List<POI>();
        PoiList = _poilist;
    }
    public List<PERCORSO> getPercorsoList()
    {
        return PercorsoList;
    }
    public void setPercorsoList(List<PERCORSO> _percorsolist)
    {
        PercorsoList = new List<PERCORSO>();
        PercorsoList = _percorsolist;
    }
    public List<TAPPEXPERCORSI> getTappeXPercorsiList()
    {
        return TappeXPercorsiList;
    }
    public void setTappeXPercorsiList(List<TAPPEXPERCORSI> _TappeXPercorsi)
    {
        TappeXPercorsiList = new List<TAPPEXPERCORSI>();
        TappeXPercorsiList = _TappeXPercorsi;
    }
}
