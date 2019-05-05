using Android.Graphics;
using Cycles.Droid.Effects;
using Cycles.Utils;
using System;
using System.ComponentModel;
using System.Linq;
//using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: Xamarin.Forms.ExportEffect(typeof(ShadowEffectDroid), "ShadowEffect")]
namespace Cycles.Droid.Effects
{
    internal class ShadowEffectDroid : PlatformEffect
    {
        private Android.Views.View control;
        private Android.Views.View container;
        private float radius, translationZ;

        protected override void OnAttached()
        {
            try
            {
                control = Control as Android.Views.View;
                container = Container as Android.Views.View;
                UpdateRadius();
                UpdateTranslationZ();
                UpdateControl();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot set property on attached control. Error: ", ex.Message);
            }
        }

        protected override void OnDetached()
        {
        }

        void UpdateControl()
        {
            var effect = (ShadowEffect)Element.Effects.FirstOrDefault(e => e is ShadowEffect);
            if (effect != null && container != null)
            {
                container.Elevation = radius ;
                container.TranslationZ = translationZ;
            }
            if (effect != null && control != null)
            {
                container.Elevation = radius;
                container.TranslationZ = translationZ;
            }
        }

        void UpdateRadius()
        {
            radius = (float)ShadowEffect.GetRadius(Element);
        }
        
        void UpdateTranslationZ()
        {
            translationZ = (float)ShadowEffect.GetTranslationZ(Element);
        }

        protected override void OnElementPropertyChanged(PropertyChangedEventArgs args)
        {
            if (args.PropertyName == ShadowEffect.RadiusProperty.PropertyName)
            {
                UpdateRadius();
                UpdateControl();
            }
            else if (args.PropertyName == ShadowEffect.TranslationZProperty.PropertyName)
            {
                UpdateTranslationZ();
                UpdateControl();
            }
        }
    }
}