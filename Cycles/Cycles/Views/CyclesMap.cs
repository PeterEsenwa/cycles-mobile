using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.Maps;

namespace Cycles.Views
{
    class CyclesMap : Map
    {
        public List<Position> RouteCoordinates { get; set; }
        public List<CustomPin> CustomPins { get; set; }

        public CyclesMap()
        {
            RouteCoordinates = new List<Position>();
        }

        public CyclesMap(List<Position> routeCoordinates, List<CustomPin> customPins)
        {
            RouteCoordinates = routeCoordinates;
            CustomPins = customPins;
        }
    }
}
