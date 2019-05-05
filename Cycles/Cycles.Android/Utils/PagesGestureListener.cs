using Android.Views;
using Android.Widget;

using System;

namespace Cycles.Droid.Utils
{
    internal class PagesGestureListener : GestureDetector.SimpleOnGestureListener
    {
        private readonly ImageButton NextButton;
        private readonly Splash splash;
        public PagesGestureListener(Splash splash)
        {
            NextButton = splash.NextButton;
            this.splash = splash;
        }

        private const int SWIPE_MIN_DISTANCE = 120;
        private const int SWIPE_MAX_OFF_PATH = 250;
        private const int SWIPE_THRESHOLD_VELOCITY = 200;

        public override bool OnDown(MotionEvent e)
        {
            return base.OnDown(e);
        }

        public override bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            try
            {
                if (Math.Abs(e1.GetY() - e2.GetY()) > SWIPE_MAX_OFF_PATH)
                {
                    return false;
                }
                if (e1.GetX() - e2.GetX() > SWIPE_MIN_DISTANCE
                        && Math.Abs(velocityX) > SWIPE_THRESHOLD_VELOCITY)
                {
                    NextButton.CallOnClick();
                }
                else if (e2.GetX() - e1.GetX() > SWIPE_MIN_DISTANCE
                        && Math.Abs(velocityX) > SWIPE_THRESHOLD_VELOCITY)
                {
                    splash.DoSplashSwipe(Splash.SwipeDirection.Backward);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Crashlytics.Crashlytics.LogException(Java.Lang.Throwable.FromException(e));
            }
            return base.OnFling(e1, e2, velocityX, velocityY);
        }

    }
}