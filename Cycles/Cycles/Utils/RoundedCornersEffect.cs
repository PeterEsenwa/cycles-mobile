using System.Linq;
using Xamarin.Forms;

namespace Cycles.Utils
{
    public static class RoundCorners
    {
        public class RoundCornersEffect : RoutingEffect
        {
            public RoundCornersEffect() : base("Custom.RoundedCornersEffectDroid")
            {
            }


        }
        public static readonly BindableProperty CornerRadiusProperty =
   BindableProperty.CreateAttached("CornerRadius", typeof(double), typeof(RoundCorners), 0.0, propertyChanged: OnCornerRadiusChanged);

        //public static BindableProperty CornerRadiusProperty { get; set; } = BindableProperty.CreateAttached(
        //  "CornerRadius",
        //  typeof(double),
        //  typeof(RoundCorners),
        //  0,
        //  propertyChanged: OnCornerRadiusChanged);

        public static double GetCornerRadius(BindableObject view) =>
            (double)view.GetValue(CornerRadiusProperty);

        public static void SetCornerRadius(BindableObject view, double value) =>
            view.SetValue(CornerRadiusProperty, value);

        private static void OnCornerRadiusChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (!(bindable is View view))
                return;

            var cornerRadius = (double)newValue;
            var effect = view.Effects.OfType<RoundCornersEffect>().FirstOrDefault();

            if (cornerRadius > 0 && effect == null)
                view.Effects.Add(new RoundCornersEffect());

            if (cornerRadius == 0 && effect != null)
                view.Effects.Remove(effect);
        }
    }

}
