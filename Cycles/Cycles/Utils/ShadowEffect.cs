using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Cycles.Utils
{
    public class ShadowEffect : RoutingEffect
    {
        public float Radius { get; set; }

        public Color Color { get; set; }

        public float DistanceX { get; set; }

        public float DistanceY { get; set; }

        public ShadowEffect() : base("Custom.EntryShadowEffect")
        {
        }
    }
}
