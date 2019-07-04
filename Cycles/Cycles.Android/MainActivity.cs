using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Util;
using Android.Views;
using Android.Widget;

using Cycles.Droid.Renderers;
using Cycles.Droid.Utils;

using Java.Lang;

using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

using Rg.Plugins.Popup;
using Rg.Plugins.Popup.Services;

using System;
using System.Threading.Tasks;

using Xamarin;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

using Exception = System.Exception;
using Math = System.Math;
using Platform = Xamarin.Essentials.Platform;
using View = Android.Views.View;

namespace Cycles.Droid
{
    [Activity(Label = "Cycles", Theme = "@style/MainTheme",
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : FormsAppCompatActivity
    {
        private static readonly SparseIntArray Orientations = new SparseIntArray(4);
        private static bool _isLocationAccessGranted;
        private static bool _isLocationEnabled;

        private App _app;
        private bool IsScanOpen { get; set; }

        #region Constant Fields

        private const long HIGH_ACC_INTERVAL_IN_MILLISECONDS = 30000;
        private const long FASTEST_HIGH_ACC_UPDATE_INTERVAL = HIGH_ACC_INTERVAL_IN_MILLISECONDS / 3;

        private const long BAL_ACC_INTERVAL_IN_MILLISECONDS = 10000;
        private const long FASTEST_BAL_ACC_UPDATE_INTERVAL = BAL_ACC_INTERVAL_IN_MILLISECONDS / 2;

        private const int REQUEST_CAMERA_ID = 10;
        private const int REQUEST_LOCATION_ID = 0;
        private const int ESSENTIALS_LOCATION_REQUEST_ID = 1;
        private const int REQUEST_TURN_ON_LOCATION_ID = 2;

        private const string TAG = "MLKIT";
        private const string MY_CAMERA_ID = "my_camera_id";

        #endregion

        #region Overrides

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Popup.Init(this, savedInstanceState);
            Forms.Init(this, savedInstanceState);
            Platform.Init(this, savedInstanceState);
            FormsMaps.Init(this, savedInstanceState);

            Receiver = new LocationSettingsBroadcastsReceiver();

            AppCenter.Start("4b376e42-98b2-47fd-af73-7a84453954f9", typeof(Analytics), typeof(Crashes));

            var dm = new DisplayMetrics();
            var windowManager = GetSystemService(WindowService).JavaCast<IWindowManager>();

            windowManager.DefaultDisplay.GetRealMetrics(dm);
            App.ScreenHeight = (int)Math.Ceiling(dm.Ydpi);
            App.ScreenWidth = (int)Math.Ceiling(dm.Xdpi);
            App.ScreenDensity = dm.Density;

            if (_app == null) _app = new App();

            LoadApplication(_app);
        }

        protected override async void OnStart()
        {
            base.OnStart();
            await CheckLocationEnabled();
            CheckPermissions();
        }

        protected override void OnPause()
        {
            base.OnPause();
            UnregisterReceiver(Receiver);
            MessagingCenter.Unsubscribe<MapPageRenderer>(this, "Scanner Opened");
            MessagingCenter.Unsubscribe<MapPageRenderer>(this, "Remove Lock-screen");
        }

        protected override void OnResume()
        {
            base.OnResume();

            RegisterReceiver(Receiver, new IntentFilter(LocationManager.ModeChangedAction));

            MessagingCenter.Subscribe<MapPageRenderer>(this, "Scanner Opened", mapPage =>
            {
                IsScanOpen = true;
            });
            MessagingCenter.Subscribe<MapPageRenderer>(this, "Remove Lock-screen", mapPage =>
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.OMr1)
                {
                    SetShowWhenLocked(true);
                    Window.AddFlags(WindowManagerFlags.KeepScreenOn);
                    Window.AddFlags(WindowManagerFlags.AllowLockWhileScreenOn);
                }
                else
                {
                    Window.AddFlags(WindowManagerFlags.ShowWhenLocked);
                    Window.AddFlags(WindowManagerFlags.KeepScreenOn);
                    Window.AddFlags(WindowManagerFlags.AllowLockWhileScreenOn);
                }
            });
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            switch (requestCode)
            {
                case REQUEST_TURN_ON_LOCATION_ID:
                    switch (resultCode)
                    {
                        case Result.Ok:
                            IsLocationEnabled = true;
                            break;
                        case Result.Canceled:
                            IsLocationEnabled = false;
                            break;
                        case Result.FirstUser:
                            IsLocationEnabled = false;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(resultCode), resultCode, null);
                    }

                    break;
                default:
                    base.OnActivityResult(requestCode, resultCode, data);
                    break;
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
            [GeneratedEnum] Permission[] grantResults)
        {
            switch (requestCode)
            {
                case REQUEST_CAMERA_ID when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                    IsScanOpen = true;
                    MessagingCenter.Send(this, "Scanner Opened");
                    break;
                case ESSENTIALS_LOCATION_REQUEST_ID when grantResults.Length > 0:
                    break;
                case REQUEST_LOCATION_ID when grantResults.Length > 0:
                    foreach (Permission result in grantResults) IsLocationAccessGranted = result == Permission.Granted;
                    break;
                default:
                    base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                    break;
            }
        }

        public override async void OnBackPressed()
        {
            if (Popup.SendBackPressed(base.OnBackPressed))
            {
                await PopupNavigation.Instance.PopAsync();
            }
            else
            {
                if (!IsScanOpen) return;

                IsScanOpen = false;
                MessagingCenter.Send(this, "Close Scanner");
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    MessagingCenter.Send(this, "openMenu");
                    //Xamarin.Forms.Application.Current.MainPage = new Dashboard();
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }


        #endregion

        #region Functions

        private async Task CheckLocationEnabled()
        {
            try
            {
                LocationRequest
                    highAccuracyRequest = LocationRequest.Create()
                        .SetPriority(LocationRequest.PriorityHighAccuracy)
                        .SetInterval(HIGH_ACC_INTERVAL_IN_MILLISECONDS)
                        .SetFastestInterval(FASTEST_HIGH_ACC_UPDATE_INTERVAL);

                LocationRequest
                    balancedRequest = LocationRequest.Create()
                        .SetPriority(LocationRequest.PriorityBalancedPowerAccuracy)
                        .SetInterval(BAL_ACC_INTERVAL_IN_MILLISECONDS)
                        .SetFastestInterval(FASTEST_BAL_ACC_UPDATE_INTERVAL);

                LocationSettingsRequest.Builder locationSettingsRequestBuilder =
                    new LocationSettingsRequest.Builder()
                        .AddLocationRequest(highAccuracyRequest)
                        .AddLocationRequest(balancedRequest)
                        .SetAlwaysShow(true);

                LocationSettingsResponse locationSettingsResult =
                    await LocationServices.GetSettingsClient(this)
                        .CheckLocationSettingsAsync(locationSettingsRequestBuilder.Build());

                if (locationSettingsResult.LocationSettingsStates.IsLocationPresent &&
                    locationSettingsResult.LocationSettingsStates.IsLocationUsable)
                    IsLocationEnabled = true;
                else
                    IsLocationEnabled = false;
            }
            catch (ApiException exception)
            {
                if (exception.StatusCode != CommonStatusCodes.ResolutionRequired)
                {
                    if (exception.StatusCode == LocationSettingsStatusCodes.SettingsChangeUnavailable)
                    {
                        //  GetLocationPermissions(Manifest.Permission.AccessFineLocation);
                    }
                }
                else
                {
                    //  Location settings are not satisfied. But could be fixed by showing the
                    //  user a dialog.
                    try
                    {
                        var resolvable = exception as ResolvableApiException;
                        resolvable?.StartResolutionForResult(this, REQUEST_TURN_ON_LOCATION_ID);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
            catch (Exception e)
            {
//                Crashlytics.Crashlytics.LogException(Throwable.FromException(e));
            }
        }

        private void CheckPermissions()
        {
            foreach (var permission in LocationPermissions)
            {
                IsLocationAccessGranted =
                    ContextCompat.CheckSelfPermission(this, permission) == (int)Permission.Granted;
                if (IsLocationAccessGranted) continue;
                GetLocationPermissions(permission);
                break;
            }
        }

        private void GetLocationPermissions(string permission)
        {
            if (ActivityCompat.ShouldShowRequestPermissionRationale(this, permission))
            {
                View layout = FindViewById(Android.Resource.Id.Content);
                Snackbar snackbar = Snackbar
                    .Make(layout, "To automatically find your community, grant us location access",
                        Snackbar.LengthIndefinite)
                    .SetAction("Allow",
                        v => ActivityCompat.RequestPermissions(this, LocationPermissions, REQUEST_LOCATION_ID))
                    .SetActionTextColor(
                        ContextCompat.GetColorStateList(this, Resource.Color.permission_snackbar_button));
                snackbar.View.SetBackgroundResource(Resource.Drawable.rounded_bg_r4);
                snackbar.View.BackgroundTintList = ContextCompat.GetColorStateList(this, Resource.Color.cyclesBlue);
                ((FrameLayout.LayoutParams)snackbar.View.LayoutParameters).SetMargins(16, 16, 16, 16);
                snackbar.Show();
            }
            else
            {
                RequestPermissions(LocationPermissions, REQUEST_LOCATION_ID);
            }
        }

        #endregion

        #region Location related props, fields, events and delegates

        public static bool IsLocationAccessGranted
        {
            get => _isLocationAccessGranted;
            private set
            {
                if (_isLocationAccessGranted == value) return;
                LocationAccessChanged?.Invoke(value);
                _isLocationAccessGranted = value;
            }
        }

        public static bool IsLocationEnabled {
            get => _isLocationEnabled;
            internal set {
                if (_isLocationEnabled == value) return;
                LocationSettingsChanged?.Invoke(value);
                _isLocationEnabled = value;
            }
        }

        private string[] LocationPermissions { get; } =
        {
            Manifest.Permission.AccessCoarseLocation,
            Manifest.Permission.AccessFineLocation
        };

        private LocationSettingsBroadcastsReceiver Receiver { get; set; }

        public delegate void LocationAccessEventHandler(bool value);

        public delegate void LocationSettingsEventHandler(bool value);

        public static event LocationAccessEventHandler LocationAccessChanged;

        public static event LocationSettingsEventHandler LocationSettingsChanged;

        #endregion

        //[RequiresApi(Api = (int)BuildVersionCodes.Lollipop)]
        //private void ImageFromMediaImage(Image mediaImage, int rotation)
        //{
        //    // [START image_from_media_image]
        //    FirebaseVisionImage image = FirebaseVisionImage.FromMediaImage(mediaImage, rotation);
        //    // [END image_from_media_image]
        //}

        //[RequiresApi(Api = (int)BuildVersionCodes.Lollipop)]
        //private void GetCompensation(Activity activity, Context context)
        //{
        //    try
        //    {
        //        Rotation = GetRotationCompensation(MY_CAMERA_ID, activity, context);
        //    }
        //    catch (CameraAccessException cameraAccessException)
        //    {
        //        Console.WriteLine(cameraAccessException);
        //        throw;
        //    }

        //    // Get the ID of the camera using CameraManager. Then:
        //}

        //private int Rotation { get; set; }

        //[RequiresApi(Api = (int)BuildVersionCodes.Lollipop)]
        //private int GetRotationCompensation(string cameraId, Activity activity, Context context)
        //{
        //    // Get the device's current rotation relative to its "native" orientation.
        //    // Then, from the ORIENTATIONS table, look up the angle the image must be
        //    // rotated to compensate for the device's rotation.
        //    SurfaceOrientation deviceRotation = activity.WindowManager.DefaultDisplay.Rotation;
        //    int rotationCompensation = ORIENTATIONS.Get((int)deviceRotation);

        //    // On most devices, the sensor orientation is 90 degrees, but for some
        //    // devices it is 270 degrees. For devices with a sensor orientation of
        //    // 270, rotate the image an additional 180 ((270 + 270) % 360) degrees.
        //    var cameraManager = (CameraManager)context.GetSystemService(CameraService);
        //    var sensorOrientation = (int)cameraManager.GetCameraCharacteristics(cameraId)
        //        .Get(CameraCharacteristics.SensorOrientation);
        //    rotationCompensation = (rotationCompensation + sensorOrientation + 270) % 360;

        //    // Return the corresponding FirebaseVisionImageMetadata rotation value.
        //    int result;
        //    switch (rotationCompensation)
        //    {
        //        case 0:
        //            result = FirebaseVisionImageMetadata.Rotation0;
        //            break;
        //        case 90:
        //            result = FirebaseVisionImageMetadata.Rotation90;
        //            break;
        //        case 180:
        //            result = FirebaseVisionImageMetadata.Rotation180;
        //            break;
        //        case 270:
        //            result = FirebaseVisionImageMetadata.Rotation270;
        //            break;
        //        default:
        //            result = FirebaseVisionImageMetadata.Rotation0;
        //            Log.Error(TAG, "Bad rotation value: " + rotationCompensation);
        //            break;
        //    }

        //    return result;
        //}
    }
}