using Android.Content;
using Cycles.Droid;
using Cycles.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Support.V7.Widget;
using ButtonRenderer = Xamarin.Forms.Platform.Android.AppCompat.ButtonRenderer;

[assembly: ExportRenderer(typeof(SizedButton), typeof(SizedButtonRenderer))]
namespace Cycles.Droid
{
    internal class SizedButtonRenderer: ButtonRenderer
    {
        private SizedButton _myButton;
        private AppCompatButton _androidButton;
        public SizedButtonRenderer(Context context) : base(context)
        {
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
            }
            else
            {
                _androidButton = CreateNativeControl();
                SetNativeControl(_androidButton);
            }

            if (_myButton != null)
            {
                //_androidButton.SetWidth(_androidButton.Width + ((int)_myButton.Padding.Left + (int)_myButton.Padding.Right));
                _androidButton.SetPadding((int)_myButton.Padding.Left, (int)_myButton.Padding.Top,
                    (int)_myButton.Padding.Right, (int)_myButton.Padding.Bottom);
                _androidButton.SetIncludeFontPadding(false);
            }
            //_androidButton.SetPadding((int) _myButton.Padding.Left, (int) _myButton.Padding.Top,
            //    (int) _myButton.Padding.Right, (int) _myButton.Padding.Bottom);
            _androidButton.SetMinimumHeight(0);
            _androidButton.SetMinimumWidth(0);
        }
    }
}