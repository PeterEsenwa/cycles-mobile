using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.Maps;

namespace Cycles.Views
{
    class CyclesMap : Map
    {
        public List<CustomPin> CustomPins { get; set; }
    }
}
