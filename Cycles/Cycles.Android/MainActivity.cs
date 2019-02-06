using System;
using Android;
using Android.App;
using Android.Content.PM;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
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
        public const long UPDATE_INTERVAL_IN_MILLISECONDS = 10000;
        public const long FASTEST_UPDATE_INTERVAL_IN_MILLISECONDS = UPDATE_INTERVAL_IN_MILLISECONDS / 2;
        private readonly string[] PermissionsLocation =
        {
          Manifest.Permission.AccessCoarseLocation,
          Manifest.Permission.AccessFineLocation
        };
        private const int RequestLocationId = 0;

        public bool CanProceed { get; set; }

        public static Window window;
        public static MainActivity activity;
        App app;

        protected override async void OnStart()
        {
            window = Window;
            activity = this;
            base.OnStart();
            await CheckLocation(this);
        }

        private static async System.Threading.Tasks.Task CheckLocation(Activity currentActivity)
        {
            try
            {
                GoogleApiClient
                    googleApiClient = new GoogleApiClient.Builder(currentActivity)
                        .AddApi(LocationServices.API)
                        .Build();

                googleApiClient.Connect();

                LocationRequest
                    locationRequest = LocationRequest.Create()
                        .SetPriority(LocationRequest.PriorityHighAccuracy)
                        .SetInterval(UPDATE_INTERVAL_IN_MILLISECONDS)
                        .SetFastestInterval(FASTEST_UPDATE_INTERVAL_IN_MILLISECONDS);

                LocationSettingsRequest.Builder
                    locationSettingsRequestBuilder = new LocationSettingsRequest.Builder()
                        .AddLocationRequest(locationRequest);

                locationSettingsRequestBuilder.SetAlwaysShow(true);

                LocationSettingsResult
                    locationSettingsResult = await LocationServices.SettingsApi.CheckLocationSettingsAsync(
                        googleApiClient, locationSettingsRequestBuilder.Build());

                if (locationSettingsResult.Status.StatusCode == LocationSettingsStatusCodes.ResolutionRequired)
                {
                    locationSettingsResult.Status.StartResolutionForResult(currentActivity, 0);
                }
            }
            catch (Exception exception)
            {
                // Log exception
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Forms.Init(this, savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
           
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
            string lastPermission = "";
            foreach (string permission in PermissionsLocation)
            {
                if (ContextCompat.CheckSelfPermission(this, permission) != (int)Permission.Granted)
                {
                    // Camera permission is not granted. If necessary display rationale & request.
                    CanProceed = false;
                    lastPermission = permission;
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
                if (ActivityCompat.ShouldShowRequestPermissionRationale(this, lastPermission))
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
                        CanProceed = false;
                        break;
                    }
                }
                if (CanProceed)
                {
                    LoadApplication(app);
                }
                else
                {
                    CheckPermissions(app);
                }
            }
            else
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    MessagingCenter.Send<MainActivity>(this, "openMenu");
                    break;
                default:
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}
