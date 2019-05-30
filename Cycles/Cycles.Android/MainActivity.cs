using System;
using Android;
using Android.App;
using Android.Content.PM;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Hardware.Camera2;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using Cycles.Droid.Renderers;
using Java.Util;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Rg.Plugins.Popup;
using Rg.Plugins.Popup.Services;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Content.Res;
using Android.Support.V4.Content.Res;
using Java.Lang;
using Xamarin;
using Xamarin.Essentials;
using Xamarin.Forms;
using Device = Xamarin.Forms.Device;
using Exception = System.Exception;
using Math = System.Math;

namespace Cycles.Droid
{
    [Activity(Label = "Cycles", Theme = "@style/MainTheme",
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        //, ShowWhenLocked = true
        AlertDialog.Builder AlertDialog { get; set; }

        private const long HIGH_ACC_INTERVAL_IN_MILLISECONDS = 30000;
        private const long FASTEST_HIGH_ACC_UPDATE_INTERVAL = HIGH_ACC_INTERVAL_IN_MILLISECONDS / 3;

        private const long BAL_ACC_INTERVAL_IN_MILLISECONDS = 10000;
        private const long FASTEST_BAL_ACC_UPDATE_INTERVAL = BAL_ACC_INTERVAL_IN_MILLISECONDS / 2;

        private static readonly SparseIntArray ORIENTATIONS = new SparseIntArray(4);
        private static readonly string TAG = "MLKIT";
        private static readonly string MY_CAMERA_ID = "my_camera_id";
        private static bool _isLocationAccessGranted;
        private static bool _isLocationEnabled;

        private const int REQUEST_CAMERA_ID = 10;
        private const int REQUEST_LOCATION_ID = 0;
        private const int ESSENTIALS_LOCATION_REQUEST_ID = 1;
        private const int REQUEST_TURN_ON_LOCATION_ID = 2;

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

        public static bool IsLocationEnabled
        {
            get => _isLocationEnabled;
            private set
            {
                if (_isLocationEnabled)return;
                LocationEnabledChanged?.Invoke(value);
                _isLocationEnabled = value;
            }
        }

        public static event LocationAccessGrantedEventHandler LocationAccessChanged;

        public delegate void LocationAccessGrantedEventHandler(bool value);

        public static event LocationEnabledEventHandler LocationEnabledChanged;

        public delegate void LocationEnabledEventHandler(bool value);

        private bool IsScanOpen { get; set; }


        private string[] LocationPermissions { get; } =
        {
            Manifest.Permission.AccessCoarseLocation,
            Manifest.Permission.AccessFineLocation
        };

        private App _app;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Popup.Init(this, savedInstanceState);
            Forms.Init(this, savedInstanceState);
            Platform.Init(this, savedInstanceState);
            FormsMaps.Init(this, savedInstanceState);

            if (_app == null)
            {
                _app = new App();
            }

            var dm = new DisplayMetrics();
            var windowManager = GetSystemService(WindowService).JavaCast<IWindowManager>();

            windowManager.DefaultDisplay.GetRealMetrics(dm);
            App.ScreenHeight = (int) Math.Ceiling(dm.Ydpi);
            App.ScreenWidth = (int) Math.Ceiling(dm.Xdpi);
            App.ScreenDensity = dm.Density;
            CheckPermissions();
            await CheckLocationEnabled();

            LoadApplication(_app);
        }

        protected override void OnStart()
        {
            base.OnStart();

            AppCenter.Start("4b376e42-98b2-47fd-af73-7a84453954f9", typeof(Analytics), typeof(Crashes));

            AlertDialog = new AlertDialog.Builder(this);
            MessagingCenter.Subscribe<MapPageRenderer>(this, "Scanner Opened", (mapPage) => { IsScanOpen = true; });
            MessagingCenter.Subscribe<MapPageRenderer>(this, "Remove Lockscreen", (mapPage) =>
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
            MessagingCenter
                .Subscribe<BarcodeScannerRenderer.GraphicBarcodeTracker, string>(this, "Barcode Scanned", (main, s) =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        AlertDialog.SetTitle("Start Ride");
                        AlertDialog.SetMessage("To start ride click Unlock. You are on PAYG");
                        AlertDialog.SetPositiveButton("Unlock", (senderAlert, args) =>
                        {
                            Toast.MakeText(this, "Bike Unlocking", ToastLength.Short).Show();
                            //NativeController.getReadDataUUID();
                        });

                        AlertDialog.SetNegativeButton("Cancel",
                            (senderAlert, args) =>
                            {
                                Toast.MakeText(this, "Ride Cancelled!", ToastLength.Short).Show();
                            });

                        Dialog dialog = AlertDialog.Create();
                        dialog.Show();
                    });
                });
        }

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
                {
                    IsLocationEnabled = true;
                }
            }
            catch (ApiException exception)
            {
                if (exception.StatusCode != CommonStatusCodes.ResolutionRequired)
                {
                    if (exception.StatusCode == LocationSettingsStatusCodes.SettingsChangeUnavailable)
                    {
//                        GetLocationPermissions(Manifest.Permission.AccessFineLocation);
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
                    catch (IntentSender.SendIntentException)
                    {
                        //  Ignore the error.
                    }
                    catch (ClassCastException)
                    {
                        //  Ignore, should be an impossible error.
                    }
                }
            }
            catch (Exception e)
            {
                Crashlytics.Crashlytics.LogException(Throwable.FromException(e));
            }
        }

        private void CheckCamera()
        {
            CameraManager manager = (CameraManager) GetSystemService(CameraService);
        }

        private void CheckPermissions()
        {
            foreach (var permission in LocationPermissions)
            {
                IsLocationAccessGranted =
                    ContextCompat.CheckSelfPermission(this, permission) == (int) Permission.Granted;
                if (IsLocationAccessGranted) continue;
                GetLocationPermissions(permission);
                break;
            }
        }

        private void GetLocationPermissions(string permission)
        {
            if (ActivityCompat.ShouldShowRequestPermissionRationale(this, permission))
            {
                Android.Views.View layout = FindViewById(Android.Resource.Id.Content);
                Snackbar snackbar = Snackbar
                    .Make(layout, "To automatically find your community, grant us location access",
                        Snackbar.LengthIndefinite)
                    .SetAction("Allow",
                        v => ActivityCompat.RequestPermissions(this, LocationPermissions, REQUEST_LOCATION_ID))
                    .SetActionTextColor(
                        ContextCompat.GetColorStateList(this, Resource.Color.permission_snackbar_button));
                snackbar.View.SetBackgroundResource(Resource.Drawable.rounded_bg_r4);
                snackbar.View.BackgroundTintList = ContextCompat.GetColorStateList(this, Resource.Color.cyclesBlue);
                ((FrameLayout.LayoutParams) snackbar.View.LayoutParameters).SetMargins(16, 16, 16, 16);
                snackbar.Show();
            }
            else
            {
                RequestPermissions(LocationPermissions, REQUEST_LOCATION_ID);
            }
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
                await PopupNavigation.Instance.PopAsync(true);
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
                default:
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

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