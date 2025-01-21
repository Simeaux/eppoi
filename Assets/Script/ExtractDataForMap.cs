using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DBClass;

public class ExtractDataForMap : MonoBehaviour
{
    private List<POI> PoiList;
    private List<PERCORSO> PercorsoList;
    private List<TAPPEXPERCORSI> TappeXPercorsi;
    private DBClass _DBClass;
    // Start is called before the first frame update
    public void getstart()
    {
        _DBClass = GameObject.FindWithTag("SQLite").GetComponent<DBClass>();
        Debug.Log("Extract");
        PoiList = _DBClass.getPOI();
        PercorsoList = _DBClass.GetPERCORSO(null, null, true);
        TappeXPercorsi = _DBClass.getTAPPEXPERCORSI(null, null, null);
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
        return TappeXPercorsi;
    }
    public void setTappeXPercorsiList(List<TAPPEXPERCORSI> _TappeXPercorsi)
    {
        TappeXPercorsi = new List<TAPPEXPERCORSI>();
        TappeXPercorsi = _TappeXPercorsi;
    }
}
