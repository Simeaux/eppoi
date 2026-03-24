using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace ARLocation.MapboxRoutes
{
    [CustomEditor(typeof(CustomRoute))]
    public class CustomRouteInspector : Editor
    {
        private List<Vector3> pointsCache = null;

        private void OnEnable()
        {
            AddOnSceneGUIDelegate(OnSceneGuiDelegate);
            Tools.hidden = true;
        }

        void OnDisable()
        {
            RemoveOnSceneGUIDelegate(OnSceneGuiDelegate);
            Tools.hidden = false;
        }


#if UNITY_2019_1_OR_NEWER
        private void AddOnSceneGUIDelegate(Action<SceneView> del)
        {
            SceneView.duringSceneGui += del; // sceneView => OnSceneGUI();
        }
#else
        private void AddOnSceneGUIDelegate(SceneView.OnSceneFunc del)
        {
            SceneView.onSceneGUIDelegate += del;
        }
#endif

#if UNITY_2019_1_OR_NEWER
        private void RemoveOnSceneGUIDelegate(Action<SceneView> del)
        {
            SceneView.duringSceneGui -= del; // sceneView => OnSceneGUI();
        }
#else
        private void RemoveOnSceneGUIDelegate(SceneView.OnSceneFunc del)
        {
            SceneView.onSceneGUIDelegate -= del;
        }
#endif

        private void OnSceneGuiDelegate(SceneView sceneview)
        {
            OnSceneGUI();
        }

        void DrawHandles(CustomRoute customRoute)
        {
            var viewScale = customRoute.SceneViewScale;
            var effScale = (1.0f + Mathf.Cos(viewScale * Mathf.PI / 2 - Mathf.PI));
            var s = new Vector3(effScale, 1.0f, effScale);

            for (var i = 0; i < pointsCache.Count; i++)
            {
                var p = pointsCache[i];
                var isStep = customRoute.Points[i].IsStep;
                var isTappa = customRoute.Points[i].IsTappa;
                Handles.color = isStep ? Color.red : Color.blue;
                Handles.color = isTappa ? Color.yellowGreen : Handles.color;
                Handles.SphereHandleCap(0, Vector3.Scale(p, s), Quaternion.identity, 4.0f, EventType.Repaint);

                if (i > 0)
                {
                    Handles.color = Color.green;
                    Handles.DrawLine(Vector3.Scale(s, pointsCache[i - 1]), Vector3.Scale(s, pointsCache[i]));
                }
            }
        }

        void DrawOnSceneGui(CustomRoute customRoute)
        {
            Handles.BeginGUI();

            GUILayout.BeginArea(new Rect(20, 20, 200, 200));

            var rect = EditorGUILayout.BeginVertical();
            GUI.color = new Color(1, 1, 1, 0.4f);
            GUI.Box(rect, GUIContent.none);

            GUI.color = Color.white;

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Custom Route");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            var style = new GUIStyle
            {
                margin = new RectOffset(0, 0, 4, 200)
            };

            GUILayout.BeginHorizontal(style);
            GUI.backgroundColor = new Color(0.2f, 0.5f, 0.92f);

            GUILayout.Label("View Scale: ", GUILayout.Width(80.0f));

            var sceneViewScale = customRoute.SceneViewScale;

            var newViewScale = GUILayout.HorizontalSlider(sceneViewScale, 0.01f, 1.0f);

            if (Math.Abs(newViewScale - sceneViewScale) > 0.000001f)
            {
                sceneViewScale = newViewScale;
                serializedObject.ApplyModifiedProperties();
            }

            customRoute.SceneViewScale = sceneViewScale;

            GUILayout.Label(sceneViewScale.ToString("0.00"), GUILayout.Width(32.0f));

            GUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            GUILayout.EndArea();
            Handles.EndGUI();
        }

        void OnSceneGUI()
        {
            var customRoute = (CustomRoute)target;

            if (customRoute == null || customRoute.Points.Count < 2)
            {
                return;
            }

            if (customRoute.IsDirty || pointsCache == null)
            {
                pointsCache = new List<Vector3>();

                for (var i = 0; i < customRoute.Points.Count; i++)
                {
                    var position = Location.GetGameObjectPositionForLocation(
                            null,
                            new Vector3(),
                            customRoute.Points[0].Location,
                            customRoute.Points[i].Location,
                            true
                            );

                    position.y = 0;
                    pointsCache.Add(position);
                }

                customRoute.IsDirty = false;
            }

            DrawOnSceneGui(customRoute);
            DrawHandles(customRoute);
        }

        [MenuItem("Assets/AR+GPS/Custom Route From KML", false)]
        private static void KmlMenuClick()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);

            var reader = new System.IO.StreamReader(path);
            var contents = reader.ReadToEnd();

            Debug.Log(contents);

            reader.Close();

            var xml = new System.Xml.XmlDocument();
            xml.LoadXml(contents);

            var kmlNode = xml["kml"];
            if (kmlNode == null)
            {
                Debug.LogError("Erorr parsing xml file!");
            }

            var documentNode = kmlNode["Document"];
            if (documentNode == null)
            {
                Debug.LogError("Erorr parsing xml file!");
            }

            var placemarkNodeList = documentNode.GetElementsByTagName("Placemark");
            var customRoute = ScriptableObject.CreateInstance<MapboxRoutes.CustomRoute>();

            for (var i = 0; i < placemarkNodeList.Count; i++)
            {
                var placemarkNode = placemarkNodeList[i];
                var name = placemarkNode["name"]?.Name;

                var PointNode = placemarkNode["Point"];
                var lineStringNode = placemarkNode["LineString"];
                if (lineStringNode != null)
                {
                    var coordinatesNode = lineStringNode["coordinates"];
                    if (coordinatesNode != null)
                    {
                        // var txt = coordinatesNode.Value.TrimStart();
                        var txt = coordinatesNode.InnerText.TrimStart().TrimEnd();
                        var split = txt.Split(new char[] { ',', ' ' });
                        foreach (var s in split)
                        {
                            //Debug.Log($":{s}:");
                        }


                        customRoute.Points = new List<CustomRoute.Point>();
                        for (var k = 0; k < split.Length; k += 3)
                        {
                            var lonString = split[k];
                            var latString = split[k + 1];

                            double lat, lon;
                            if (!double.TryParse(lonString, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out lon))
                            {
                                Debug.LogError("Failed to parse float number");
                                return;
                            }

                            if (!double.TryParse(latString, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out lat))
                            {
                                Debug.LogError("Failed to parse float number");
                                return;
                            }

                            var location = new Location(lat, lon);
                            var point = new MapboxRoutes.CustomRoute.Point();
                            point.Location = location;
                            point.IsStep = true;
                            //point.Name = "pluto";
                            //point.Instruction = "Pippo";
                            customRoute.Points.Add(point);
                        }

                        //customRoute.Points[0].IsStep = true;
                        //customRoute.Points[customRoute.Points.Count - 1].IsStep = true;

                    }
                }
                else if (PointNode != null)
                {
                    var coordinatesNode = PointNode["coordinates"];
                    double longitude = 0;
                    double latitude = 0;

                    if (coordinatesNode != null)
                    {
                        // var txt = coordinatesNode.Value.TrimStart();
                        var txt = coordinatesNode.InnerText.TrimStart().TrimEnd();
                        var split = txt.Split(new char[] { ',', ' ' });
                        for (var k = 0; k < split.Length; k += 3)
                        {
                            var lonString = split[k];
                            var latString = split[k + 1];

                            if (!double.TryParse(lonString, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out longitude))
                            {
                                Debug.LogError("Failed to parse float number");
                                return;
                            }

                            if (!double.TryParse(latString, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out latitude))
                            {
                                Debug.LogError("Failed to parse float number");
                                return;
                            }
                        }
                    }
                    var nameNode = placemarkNode["name"];
                    var descriptionNode = placemarkNode["description"];

                    var min_distance = 999999999.99;
                    int num = 0;
                    int indice = -1;

                    foreach (var point in customRoute.Points)
                    {
                        if (point.Location != null && point.Location.Latitude > 0 && point.Location.Longitude > 0)
                        {
                            var distanza = distance(latitude, longitude, point.Location.Latitude, point.Location.Longitude, 'K');
                            if (distanza < min_distance)
                            {
                                min_distance = distanza;
                                indice = num;
                            }
                        }
                        num++;
                    }
                    if (indice != -1)
                    {
                        if (nameNode != null)
                        {
                            if (nameNode.InnerText.Contains("(t)"))
                                customRoute.Points[indice].IsTappa = true;
                            customRoute.Points[indice].Name = nameNode.InnerText.Replace("(t)", "");
                        }
                        if (descriptionNode != null)
                            customRoute.Points[indice].Instruction = descriptionNode.InnerText;
                    }
                }
            }
            var dirPath = System.IO.Path.GetDirectoryName(path);
            var baseName = System.IO.Path.GetFileNameWithoutExtension(path);
            var filename = System.IO.Path.Combine(dirPath, baseName + ".asset");
            AssetDatabase.CreateAsset(customRoute, filename);


        }

        [MenuItem("Assets/AR+GPS/Custom Route From KML", true)]
        private static bool KmlMenuClickValidator()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            var ext = System.IO.Path.GetExtension(path);

            return ext.ToLower(System.Globalization.CultureInfo.InvariantCulture) == ".kml";
        }




        private static double distance(double lat1, double lon1, double lat2, double lon2, char unit)
        {
            if ((lat1 == lat2) && (lon1 == lon2))
            {
                return 0;
            }
            else
            {
                double theta = lon1 - lon2;
                double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));
                dist = Math.Acos(dist);
                dist = rad2deg(dist);
                dist = dist * 60 * 1.1515;
                if (unit == 'K')
                {
                    dist = dist * 1.609344;
                }
                else if (unit == 'N')
                {
                    dist = dist * 0.8684;
                }
                return (dist);
            }
        }

        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //::  This function converts decimal degrees to radians             :::
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        private static double deg2rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //::  This function converts radians to decimal degrees             :::
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        private static double rad2deg(double rad)
        {
            return (rad / Math.PI * 180.0);
        }




    }
}
