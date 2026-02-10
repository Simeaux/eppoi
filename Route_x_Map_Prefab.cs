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
using Mapbox.Directions;
using System.Numerics;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(LineRenderer))]
public class Route_x_Map_Prefab : MonoBehaviour
{
    public AbstractMap _map;
    public List<Vector2d> Waypoints; // I tuoi punti Lat/Lon
    private LineRenderer _line;

    void Awake()
    {
        _line = GetComponent<LineRenderer>();
    }

    public void Enable()
    {
        // Ti iscrivi all'evento di aggiornamento della mappa
        if (_map != null) _map.OnUpdated += UpdatePath;
    }

    void OnDisable()
    {
        // Importante: disiscriviti quando l'oggetto viene rimosso
        if (_map != null) _map.OnUpdated -= UpdatePath;
    }
    public void UpdatePath()
    {
        if (Waypoints == null || Waypoints.Count() < 2) return;
        _line.positionCount = Waypoints.Count();

        for (int i = 0; i < Waypoints.Count(); i++)
        {
            // Parametro 'true' per proiettare la linea sul terreno
            Vector3 worldPos = _map.GeoToWorldPosition(Waypoints[i], true);

            // Alziamo leggermente la linea (Y offset) per evitare che sfarfalleggi sul suolo
            worldPos.y += 0.5f;

            _line.SetPosition(i, worldPos);
        }
    }
}
