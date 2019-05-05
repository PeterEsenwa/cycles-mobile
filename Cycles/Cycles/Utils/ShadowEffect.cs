using System.Linq;
using Xamarin.Forms;

namespace Cycles.Utils
{
    public class ShadowEffect : RoutingEffect
    {
        public ShadowEffect() : base("Custom.ShadowEffect")
        {
        }

        protected ShadowEffect(string effectId) : base(effectId)
        {
        }

        public static readonly BindableProperty HasShadowProperty =
            BindableProperty.CreateAttached("HasShadow",
                typeof(bool),
                typeof(ShadowEffect),
                false,
                propertyChanged: OnHasShadowChanged);

        public static readonly BindableProperty RadiusProperty =
            BindableProperty.CreateAttached("Radius",
                typeof(double),
                typeof(ShadowEffect),
                4.0);

        public static readonly BindableProperty TranslationZProperty =
         BindableProperty.CreateAttached("TranslationZ",
             typeof(double),
             typeof(ShadowEffect),
             4.0);

        #region HasShadowProperty setter and getter
        public static bool GetHasShadow(BindableObject view)
        {
            return (bool)view.GetValue(HasShadowProperty);
        }

        public static void SetHasShadow(BindableObject view, bool value)
        {
            view.SetValue(HasShadowProperty, value);
        }
        #endregion

        #region RadiusProperty setter and getter
        public static double GetRadius(BindableObject view)
        {
            return (double)view.GetValue(RadiusProperty);
        }

        public static void SetRadius(BindableObject view, double value)
        {
            view.SetValue(RadiusProperty, value);
        }
        #endregion

        #region TranslationZProperty setter and getter
        public static double GetTranslationZ(BindableObject view)
        {
            return (double)view.GetValue(TranslationZProperty);
        }

        public static void SetTranslationZ(BindableObject view, double value)
        {
            view.SetValue(TranslationZProperty, value);
        }
        #endregion
        
        static void OnHasShadowChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (!(bindable is View view))
            {
                return;
            }

            if ((bool)newValue)
            {
                view.Effects.Add(new ShadowEffect());
            }
            else
            {
                if (view.Effects.FirstOrDefault(e => e is ShadowEffect) != null)
                {
                    view.Effects.Remove(view.Effects.FirstOrDefault(e => e is ShadowEffect));
                }
            }
        }

        //private static void OnCornerRadiusChanged(BindableObject bindable, object oldValue, object newValue)
        //{
        //    if (!(bindable is View view))
        //        return;

        //    var cornerRadius = (double)newValue;
        //    var effect = view.Effects.OfType<ShadowEffect>().FirstOrDefault();

        //    if (cornerRadius > 0 && effect == null)
        //        view.Effects.Add(new ShadowEffect());

        //    if (cornerRadius == 0 && effect != null)
        //        view.Effects.Remove(effect);
        //}
        
    }
}
