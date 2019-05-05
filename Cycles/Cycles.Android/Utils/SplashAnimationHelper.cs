using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Cycles.Droid.Utils
{
    public static class SplashAnimationHelper
    {
        public static void AnimateViewOut(View view)
        {
            view.Animate()
                .Alpha(0)
                .SetDuration(300)
                .WithEndAction(new Java.Lang.Runnable(() => {
                    view.Visibility = ViewStates.Visible;
                }))
                .Start();
        }

        public static void AnimateViewIn(View view)
        {
            view.Animate()
                .Alpha(255)
                .SetDuration(300)
                .WithStartAction(new Java.Lang.Runnable(() => {
                    view.Visibility = ViewStates.Visible;
                }))
                .Start();
        }

        public static void AnimateViewIn(View view, Java.Lang.Runnable continueAction)
        {
            view.Animate()
                .Alpha(255)
                .SetDuration(300)
                .WithStartAction(new Java.Lang.Runnable(() => {
                    view.Visibility = ViewStates.Visible;
                }))
                .WithEndAction(continueAction)
                .Start();
        }
    }
}