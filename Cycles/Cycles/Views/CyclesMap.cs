using Android.Gms.Maps.Model;
using Cycles.Models;
using Cycles.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.Maps;

namespace Cycles.Views
{
    class CyclesMap : Map
    {
        public List<Position> RouteCoordinates { get; set; }
        public List<LatLng> Lines { get; set; }
        public List<CustomPin> CustomPins { get; set; }

        public event EventHandler RoutesListUpdated;
        public CyclesMap()
        {
            RouteCoordinates = new List<Position>();
            Lines = new List<LatLng>();
        }

        public CyclesMap(List<Position> routeCoordinates, List<CustomPin> customPins)
        {
            RouteCoordinates = routeCoordinates;
            CustomPins = customPins;
        }

        public void LoadRoutes(List<Step> steps)
        {
            foreach (Step step in steps)
            {
                RouteCoordinates.Add(new Position(step.start_location.lat, step.start_location.lng));
                RouteCoordinates.Add(new Position(step.end_location.lat, step.end_location.lng));
                RoutesListUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        public void LoadRoutes(OverviewPolyline overview_polyline)
        {
            foreach (LatLng line in DirectionsMethods.DecodePolyline(overview_polyline.points))
            {
                Lines.Add(line);
            }
            RoutesListUpdated?.Invoke(this, EventArgs.Empty);
        }
    }
}
