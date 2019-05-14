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
using Cycles.Droid.Renderers;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System;
using Android.Content;
using Android.Hardware.Camera2;
using Android.Support.Annotation;
using Firebase;
using Firebase.ML.Vision.Common;
using Xamarin;
using Xamarin.Forms;
using Application = Xamarin.Forms.Application;
using Image = Android.Media.Image;
using Android.Widget;
using Device = Xamarin.Forms.Device;
using Rg.Plugins.Popup;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using System.Threading.Tasks;

namespace Cycles.Droid
{
    [Activity(Label = "Cycles", Theme = "@style/MainTheme",
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        //, ShowWhenLocked = true
        AlertDialog.Builder AlertDialog { get; set; }
        private const long UPDATE_INTERVAL_IN_MILLISECONDS = 10000;
        private static readonly SparseIntArray ORIENTATIONS = new SparseIntArray(4);
        private static readonly string TAG = "MLKIT";
        private static readonly string MY_CAMERA_ID = "my_camera_id";
        private const long FASTEST_UPDATE_INTERVAL_IN_MILLISECONDS = UPDATE_INTERVAL_IN_MILLISECONDS / 2;
        private bool IsScanOpen { get; set; }


        private string[] LocationPermissions { get; } =
        {
            Manifest.Permission.AccessCoarseLocation,
            Manifest.Permission.AccessFineLocation
        };

        private const int REQUEST_LOCATION_ID = 0;
        private App app;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Popup.Init(this, savedInstanceState);
            Forms.Init(this, savedInstanceState);
            Platform.Init(this, savedInstanceState);
            FormsMaps.Init(this, savedInstanceState);

            if (app == null)
            {
                app = new App();
            }
            LoadApplication(app);
        }

        protected override void OnStart()
        {
            base.OnStart();

            AppCenter.Start("4b376e42-98b2-47fd-af73-7a84453954f9", typeof(Analytics), typeof(Crashes));

            var dm = new DisplayMetrics();
            var windowManager = GetSystemService(WindowService).JavaCast<IWindowManager>();

            windowManager.DefaultDisplay.GetRealMetrics(dm);
            App.ScreenHeight = (int)Math.Ceiling(dm.Ydpi);
            App.ScreenWidth = (int)Math.Ceiling(dm.Xdpi);
            App.ScreenDensity = dm.Density;

            AlertDialog = new AlertDialog.Builder(this);
            MessagingCenter.Subscribe<MapPageRenderer>(this, "Scanner Opened", (mapPage) =>
            {
                IsScanOpen = true;
            });
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
            MessagingCenter.Subscribe<BarcodeScannerRenderer.GraphicBarcodeTracker, string>(this, "Barcode Scanned", (main, s) =>
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

                    AlertDialog.SetNegativeButton("Cancel", (senderAlert, args) =>
                    {
                        Toast.MakeText(this, "Ride Cancelled!", ToastLength.Short).Show();
                    });

                    Dialog dialog = AlertDialog.Create();
                    dialog.Show();
                });

            });

        }

        private async Task CheckLocation(Activity currentActivity)
        {
            try
            {
                GoogleApiClient
                    googleApiClient = new GoogleApiClient.Builder(this)
                        .AddApi(LocationServices.API)
                        .Build();

                googleApiClient.Connect();

                LocationRequest
                    locationRequest = LocationRequest.Create()
                        .SetPriority(LocationRequest.PriorityHighAccuracy)
                        .SetInterval(UPDATE_INTERVAL_IN_MILLISECONDS)
                        .SetFastestInterval(FASTEST_UPDATE_INTERVAL_IN_MILLISECONDS);

                LocationSettingsRequest.Builder locationSettingsRequestBuilder =
                    new LocationSettingsRequest.Builder().AddLocationRequest(locationRequest);

                locationSettingsRequestBuilder.SetAlwaysShow(true);

                LocationSettingsResult locationSettingsResult =
                    await LocationServices.SettingsApi.CheckLocationSettingsAsync(googleApiClient, locationSettingsRequestBuilder.Build());

                if (locationSettingsResult.Status.StatusCode == CommonStatusCodes.ResolutionRequired)
                {
                    locationSettingsResult.Status.StartResolutionForResult(currentActivity, 0);
                }
            }
            catch (Exception e)
            {
                Crashlytics.Crashlytics.LogException(Java.Lang.Throwable.FromException(e));
                // Log exception
            }
        }

        private void CheckCamera()
        {
            CameraManager manager = (CameraManager)GetSystemService(CameraService);
        }

        private void CheckPermissions()
        {
            foreach (var permission in LocationPermissions)
            {
                bool isGranted = ContextCompat.CheckSelfPermission(this, permission) != (int)Permission.Granted;
                if (isGranted)
                {
                    if (ActivityCompat.ShouldShowRequestPermissionRationale(this, permission))
                    {
                        Android.Views.View layout = FindViewById(Android.Resource.Id.Content);
                        Snackbar snackbar = Snackbar
                            .Make(layout, "This app needs Location access to work properly", Snackbar.LengthIndefinite)
                            .SetAction("Allow", v => ActivityCompat.RequestPermissions(this, LocationPermissions, REQUEST_LOCATION_ID))
                            .SetActionTextColor(ContextCompat.GetColorStateList(this, Resource.Color.permission_snackbar_button));
                        snackbar.View.SetBackgroundResource(Resource.Color.cyclesBlack);
                        //snackbar.View
                        ((FrameLayout.LayoutParams)snackbar.View.LayoutParameters).SetMargins(16, 16, 16, 16);
                        snackbar.Show();
                    }
                    else
                    {
                        RequestPermissions(LocationPermissions, REQUEST_LOCATION_ID);
                    }
                }
            }
        }

        public async override void OnBackPressed()
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