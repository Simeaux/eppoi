using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace ARLocation.MapboxRoutes
{
    [CreateAssetMenu(fileName = "CustomRoute", menuName = "AR+GPS/Route")]
    public class CustomRoute : ScriptableObject
    {
        [System.Serializable]
        public class Point
        {
            [Tooltip("The geographical location of this route point.")]
            public Location Location;

            [Tooltip("If true, this point is considered a \"Step\" in thre route. A route \"Step\" is a point of the route where"
                    + "a meneuver is expected to happen, e.g., \"TurnRight\".")]
            public bool IsStep;
            [Tooltip("Indica se il punto è una tappa")]
            public bool IsTappa;

            [Tooltip("The name of the point (optional)")]
            public string Name;

            [Tooltip("The instruction to the user informing of the maneuver to be exectured (in case \"IsStep\" is true).")]
            public string Instruction;
        }

        [Tooltip("The name of the custom route.")]
        public string Name;

        [Tooltip("A list of point defining the custom route.")]
        public List<Point> Points = new List<Point>() { new Point { }, new Point { } };

        [HideInInspector]
        public float SceneViewScale = 1.0f;

        public string Color;

        private bool isDirty = true;
        public bool IsDirty
        {
            get => isDirty;
            set { isDirty = value; }
        }

        public void OnValidate()
        {
            isDirty = true;

            if (Points.Count == 0)
            {
                Points.Add(new Point { });
                Points.Add(new Point { });
            }

            if (Points.Count == 1)
            {
                Points.Add(new Point { });
            }

            if (Points.Count > 0)
            {
                Points[0].IsStep = true;
                Points[Points.Count - 1].IsStep = true;
            }
        }

        public List<Waypoint> GetWaypoints()
        {
            var result = new List<Waypoint>();

            if (Points.Count < 2)
            {
                return result;
            }

            result.Add(new Waypoint { location = Points[0].Location, name = Points[0].Name });
            result.Add(new Waypoint { location = Points[Points.Count - 1].Location, name = Points[Points.Count - 1].Name });

            return result;
        }

        public Route ToMapboxRoute()
        {
            var route = new Route { };
            var leg = new Route.RouteLeg();
            leg.steps = new List<Route.Step>();

            route.geometry = new Route.Geometry();
            route.legs = new List<Route.RouteLeg> { leg };
            route.name = Name;
            int _lingua_selezionata = 1;

            _lingua_selezionata = PlayerPrefs.GetInt("lingua_selezionata");

            int _numero_tappa = 1;
            int _numero_tappe = Points.Where(p => p.IsTappa).Count();
            foreach (var p in Points)
            {
                route.geometry.coordinates.Add(p.Location.Clone());

                if (p.IsStep)
                {
                    var step = new Route.Step();
                    step.geometry = new Route.Geometry();
                    step.geometry.coordinates.Add(p.Location.Clone());
                    step.name = string.Empty;

                    if (_numero_tappa > _numero_tappe)
                    {
                        if (_lingua_selezionata == 1)
                            step.name = "Procedi verso fine itinerario";
                        else
                            step.name = "Proceed towards the end of the itinerary";
                    }
                    else if (!p.IsTappa)
                    {
                        if (_lingua_selezionata == 1)
                            step.name = "Procedi verso la tappa " + _numero_tappa.ToString();
                        else
                            step.name = "Proceed to stage " + _numero_tappa.ToString();
                    }
                    if (p.IsTappa)
                        _numero_tappa++;

                    step.maneuver = new Route.Maneuver();
                    step.maneuver.location = p.Location.Clone();
                    var t = p.Instruction;
                    if (t.Contains(";"))
                    {
                        var t_split = t.Split(";");
                        if (_lingua_selezionata == 1)
                            t = t_split[0];
                        else
                            t = t_split[1] ?? t_split[0];
                    }
                    step.maneuver.instruction = string.IsNullOrWhiteSpace(t) ? "" : t;
                    step.maneuver.isTappa = p.IsTappa;

                    leg.steps.Add(step);
                }
            }

            return route;
        }
        public Route _ToMapboxRoute()
        {
            var route = new Route { };
            var leg = new Route.RouteLeg();
            leg.steps = new List<Route.Step>();

            route.geometry = new Route.Geometry();
            route.legs = new List<Route.RouteLeg> { leg };
            route.name = Name;
            int _lingua_selezionata = 1;

            _lingua_selezionata = PlayerPrefs.GetInt("lingua_selezionata");

            int _numero_tappa = 0;
            foreach (var p in Points)
            {
                route.geometry.coordinates.Add(p.Location.Clone());

                if (p.IsStep)
                {
                    if (p.IsTappa)
                        _numero_tappa++;
                    var step = new Route.Step();
                    step.geometry = new Route.Geometry();
                    step.geometry.coordinates.Add(p.Location.Clone());
                    step.name = "Procedi verso la tappa " + _numero_tappa.ToString();

                    step.maneuver = new Route.Maneuver();
                    step.maneuver.location = p.Location.Clone();
                    var t = p.Instruction;
                    if (t.Contains(";"))
                    {
                        var t_split = t.Split(";");
                        if (_lingua_selezionata == 1)
                            t = t_split[0];
                        else
                            t = t_split[1] ?? t_split[0];
                    }
                    step.maneuver.instruction = t;

                    leg.steps.Add(step);
                }
            }

            return route;
        }
    }
}
