using Cycles.Droid.Effects;
using Cycles.Utils;
using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ResolutionGroupName("Custom")]
[assembly: ExportEffect(typeof(EntryShadowEffect), "EntryShadowEffect")]
namespace Cycles.Droid.Effects
{
    internal class EntryShadowEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            try
            {
                var control = Control as FormsEditText;
                var effect = (ShadowEffect)Element.Effects.FirstOrDefault(e => e is ShadowEffect);
                if (effect != null)
                {
                    float radius = effect.Radius;
                    float distanceX = effect.DistanceX;
                    float distanceY = effect.DistanceY;
                    Android.Graphics.Color color = effect.Color.ToAndroid();
                    //control.SetShadowLayer(radius, distanceX, distanceY, color);
                    control.Elevation = (int)distanceY;
                    control.TranslationZ = (int)radius;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot set property on attached control. Error: ", ex.Message);
            }
        }

        protected override void OnDetached()
        {

        }
    }
}