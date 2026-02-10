using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Unity.Location;
using Mapbox.Utils;
using System.Numerics;

public class PlayerPositioner : MonoBehaviour
{
    public AbstractMap _map;
    public GameObject _marker; // Il tuo GOImhere


    void Update()
    {
        // Verifica di sicurezza: se la factory non è pronta, non fare nulla
        if (LocationProviderFactory.Instance == null) return;

        var provider = LocationProviderFactory.Instance.DefaultLocationProvider;
        if (provider == null) return;

        // Recupera le coordinate attuali
        Vector2d latLon = provider.CurrentLocation.LatitudeLongitude;

        // Se le coordinate sono (0,0), probabilmente il GPS sta ancora caricando
        if (latLon.x != 0 && latLon.y != 0)
        {
            // Converti in World Position di Unity
            UnityEngine.Vector3 worldPos = _map.GeoToWorldPosition(latLon, true);

            // Posiziona il marker
            _marker.transform.position = worldPos;



            var imhere = Instantiate(_marker, _map.GeoToWorldPosition(new Vector2d(worldPos.x, worldPos.z), true), UnityEngine.Quaternion.identity);
            Obj_x_Map_Prefab _ap = imhere.GetComponentInChildren<Obj_x_Map_Prefab>();
            if (_ap != null)
            {
                _ap._map = _map;
                _ap._latLong = new Vector2d(worldPos.x, worldPos.y);
                _ap.Enable();
                _ap.UpdatePosition();
            }

            imhere.transform.Rotate(90, 0, 0);
        }
    }
}
