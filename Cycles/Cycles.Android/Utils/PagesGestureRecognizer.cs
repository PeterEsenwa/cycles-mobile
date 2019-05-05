using Android.Support.V4.View;
using Android.Views;

namespace Cycles.Droid.Utils
{
    internal class PagesGestureRecognizer : Java.Lang.Object, View.IOnTouchListener
    {
        private GestureDetectorCompat Detector { get; }

        public PagesGestureRecognizer(GestureDetectorCompat detector)
        {
            Detector = detector;
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            Detector.OnTouchEvent(e);
            return true;
        }
    }
}