using System;
using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Util;
using Android.Views;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Xamarin;
using Xamarin.Forms;

namespace Cycles.Droid
{
    [Activity(Label = "Cycles", Theme = "@style/MainTheme", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private readonly string[] PermissionsLocation =
        {
          Manifest.Permission.AccessCoarseLocation,
          Manifest.Permission.AccessFineLocation
        };
        private const int RequestLocationId = 0;

        public bool CanProceed { get; set; }

        App app;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Forms.Init(this, savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            AppCenter.Start("4b376e42-98b2-47fd-af73-7a84453954f9",
                    typeof(Analytics), typeof(Crashes));
            AppCenter.Start("4b376e42-98b2-47fd-af73-7a84453954f9", typeof(Analytics), typeof(Crashes));

            DisplayMetrics dm = new DisplayMetrics();
            IWindowManager windowManager = GetSystemService(WindowService).JavaCast<IWindowManager>();

            windowManager.DefaultDisplay.GetRealMetrics(dm);
            App.ScreenHeight = (int)Math.Ceiling(dm.Ydpi);
            App.ScreenWidth = (int)Math.Ceiling(dm.Xdpi);
            App.ScreenDensity = dm.Density;
            //CrossCurrentActivity.Current.Init(this, savedInstanceState);
            FormsMaps.Init(this, savedInstanceState);
            app = new App();
            CheckPermissions(app);
        }

        private void CheckPermissions(App app)
        {
            foreach (string permission in PermissionsLocation)
            {
                if (ContextCompat.CheckSelfPermission(this, permission) != (int)Permission.Granted)
                {
                    // Camera permission is not granted. If necessary display rationale & request.
                    CanProceed = false;
                    break;
                }
                else
                {
                    // We have permission, go ahead and use the camera.
                    CanProceed = true;
                }
            }

            if (CanProceed)
            {
                LoadApplication(app);
            }
            else
            {
                if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.AccessFineLocation))
                {
                    // Provide an additional rationale to the user if the permission was not granted
                    // and the user would benefit from additional context for the use of the permission.
                    // For example if the user has previously denied the permission.
                    //var requiredPermissions = new String[] { Manifest.Permission.AccessFineLocation };
                    //Activity activity = CrossCurrentActivity.Current.Activity;
                    Android.Views.View layout = FindViewById(Android.Resource.Id.Content);
                    Snackbar
                        .Make(layout, "You need to allow Location access to the app", Snackbar.LengthLong)
                        .SetAction("OK", v => ActivityCompat.RequestPermissions(this, PermissionsLocation, RequestLocationId))
                        .Show();
                }
                else
                {
                    ActivityCompat.RequestPermissions(this, PermissionsLocation, RequestLocationId);
                }
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            if (requestCode == RequestLocationId)
            {
                foreach (Permission result in grantResults)
                {
                    if (result != Permission.Granted)
                    {
                        Android.Views.View layout = FindViewById(Android.Resource.Id.Content);
                        Snackbar
                            .Make(layout, "You need to allow Location access to the app", Snackbar.LengthShort)
                            .SetAction("OK", v => ActivityCompat.RequestPermissions(this, PermissionsLocation, RequestLocationId))
                            .Show();
                    }
                    else
                    {
                        CheckPermissions(app);
                    }
                }
            }
            else
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            }
        }
    }
}
