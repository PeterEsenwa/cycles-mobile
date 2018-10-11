using Android.Content;
using Android.Graphics.Drawables;
using Android.Util;
using Cycles.Droid.Renderers;
using Cycles.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(PaddingBorderEntry), typeof(PBEntryRenderer))]
namespace Cycles.Droid.Renderers
{
    internal class PBEntryRenderer : EntryRenderer
    {
        private PaddingBorderEntry _pbEntry;

        public PBEntryRenderer(Context context) : base(context)
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement != null)
                _pbEntry = e.OldElement as PaddingBorderEntry;
            if (e.NewElement != null)
                _pbEntry = e.NewElement as PaddingBorderEntry;
            if (Element != null)
                _pbEntry = Element as PaddingBorderEntry;

            if (Control != null)
            {
                if (_pbEntry.IsCurvedCornersEnabled)
                {
                    // creating gradient drawable for the curved background  
                    var _gradientBackground = new GradientDrawable();
                    _gradientBackground.SetShape(ShapeType.Rectangle);
                    //_gradientBackground.SetAlpha(_pbEntry.BackgroundOpacity);
                    _gradientBackground.SetColor(_pbEntry.Background.ToAndroid());
                    //_gradientBackground.Alpha = _pbEntry.BackgroundOpacity;
                    // set the background of the   
                    if (_pbEntry.HasBorder)
                    {
                        // Thickness of the stroke line 
                        _gradientBackground.SetStroke(_pbEntry.BorderWidth, _pbEntry.BorderColor.ToAndroid());
                    }
                    else
                    {
                        _gradientBackground.SetStroke(0, Color.Transparent.ToAndroid());
                    }

                    // Radius for the curves  
                    _gradientBackground.SetCornerRadius(DpToPixels(Context, Convert.ToSingle(_pbEntry.CornerRadius)));
                
                    Control.SetBackground(_gradientBackground);
                }
                Control.Gravity = Android.Views.GravityFlags.CenterVertical;
                Control.SetPadding((int)_pbEntry.Padding.Left, (int)_pbEntry.Padding.Top, (int)_pbEntry.Padding.Right, (int)_pbEntry.Padding.Bottom);
            }
        }
        public static float DpToPixels(Context context, float valueInDp)
        {
            DisplayMetrics metrics = context.Resources.DisplayMetrics;
            return TypedValue.ApplyDimension(ComplexUnitType.Dip, valueInDp, metrics);
        }
    }
}