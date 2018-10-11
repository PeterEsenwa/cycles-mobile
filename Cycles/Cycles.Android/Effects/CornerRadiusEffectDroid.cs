using Android.Graphics;
using Android.Views;
using Cycles.Droid.Effects;
using Cycles.Utils;
using System;
using System.ComponentModel;
using System.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportEffect(typeof(RoundedCornersEffectDroid), "RoundedCornersEffectDroid")]
namespace Cycles.Droid.Effects
{
    internal class RoundedCornersEffectDroid : PlatformEffect
    {
        protected override void OnAttached()
        {
            PrepareContainer();
            SetCornerRadius();
        }

        protected override void OnDetached()
        {
            Container.OutlineProvider = ViewOutlineProvider.Background;
        }

        protected override void OnElementPropertyChanged(PropertyChangedEventArgs args)
        {
            if (args.PropertyName == RoundCorners.CornerRadiusProperty.PropertyName)
                SetCornerRadius();
        }

        private void PrepareContainer()
        {
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop)
            {
                Container.ClipToOutline = true;
            }
            else
            {
                // Do things the pre-Lollipop way
            }
            
        }

        private void SetCornerRadius()
        {
            var cornerRadius = RoundCorners.GetCornerRadius(Element) * GetDensity();
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop)
            {
                Container.OutlineProvider = new RoundedOutlineProvider((float)cornerRadius);
            }
            else
            {
                // Do things the pre-Lollipop way
            }
        }

        private static float GetDensity()
        {
            var metrics = DeviceDisplay.ScreenMetrics;
            return (float)metrics.Density;
        }

        private class RoundedOutlineProvider : ViewOutlineProvider
        {
            private readonly float _radius;
             

            public RoundedOutlineProvider(float radius)
            {
                _radius = radius;
            }

            public override void GetOutline(Android.Views.View view, Outline outline)
            {
                outline?.SetRoundRect(0, 0, view.Width, view.Height, _radius);
            }
        }
    }
}