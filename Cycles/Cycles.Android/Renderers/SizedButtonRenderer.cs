using Android.Content;
using Android.Support.V7.Widget;
using Cycles.Droid;
using Cycles.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using static Android.Views.View;
using ButtonRenderer = Xamarin.Forms.Platform.Android.AppCompat.ButtonRenderer;

[assembly: ExportRenderer(typeof(SizedButton), typeof(SizedButtonRenderer))]
namespace Cycles.Droid
{
    internal class SizedButtonRenderer : ButtonRenderer, IOnLayoutChangeListener
    {
        private SizedButton _myButton;
        private AppCompatButton _androidButton;

        public SizedButtonRenderer(Context context) : base(context)
        {
        }

        public void OnLayoutChange(Android.Views.View v, int left, int top, int right, int bottom, int oldLeft, int oldTop, int oldRight, int oldBottom)
        {
            if (v != null)
            {
                if (_myButton.IsRound && _myButton.Radius == 0)
                {
                    //_androidButton.SetHeight((int)_myButton.Height);
                    if (_androidButton.Width > _androidButton.Height)
                    {
                        _androidButton.SetWidth(_androidButton.Height);
                    }
                    else if (_androidButton.Width < _androidButton.Height)
                    {
                        _androidButton.SetHeight(_androidButton.Width);
                    }

                    _myButton.CornerRadius = _androidButton.Width / 2;
                }
                else if (_myButton.IsRound)
                {
                    if (_myButton.Radius < 0)
                    {
                        _myButton.Radius = 0;
                    }
                    if (_myButton.Radius > 100)
                    {
                        _myButton.Radius = 100;
                    }
                    decimal radius = decimal.Divide(_myButton.Radius, 100);
                    radius = _androidButton.Width * radius;
                    _myButton.CornerRadius = (int)radius;
                }
            }
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);
            if (e.OldElement != null)
                _myButton = e.OldElement as SizedButton;
            if (e.NewElement != null)
                _myButton = e.NewElement as SizedButton;
            if (Element != null)
                _myButton = Element as SizedButton;

            if (Control != null)
            {
                _androidButton = Control;
                _androidButton.AddOnLayoutChangeListener(this);
            }
            else
            {
                _androidButton = CreateNativeControl();
                SetNativeControl(_androidButton);
            }

            if (_myButton != null)
            {
                //         _androidButton.SetPadding((int)_myButton.Padding.Left, (int)_myButton.Padding.Top,
                //(int)_myButton.Padding.Right, (int)_myButton.Padding.Bottom);
                //         _androidButton.SetWidth(_androidButton.Width + ((int)_myButton.Padding.Left + (int)_myButton.Padding.Right));


                _androidButton.SetIncludeFontPadding(false);
            }
        //_androidButton.SetPadding((int) _myButton.Padding.Left, (int) _myButton.Padding.Top,
        //    (int) _myButton.Padding.Right, (int) _myButton.Padding.Bottom);
        _androidButton.SetMinimumHeight(0);
            _androidButton.SetMinimumWidth(0);
        }


    }
}