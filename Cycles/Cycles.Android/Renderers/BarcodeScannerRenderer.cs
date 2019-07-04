using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Content;
using Android.Gms.Vision;
using Android.Gms.Vision.Barcodes;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX;
//using Android.Hardware.Camera2.CameraCharacteristics;
//using Android.Hardware.Camera2.CameraDevice;
//using Android.Hardware.Camera2.CameraManager;
//using Android.Hardware.Camera2.CameraMetadata;
//using Android.Hardware.Camera2.CaptureRequest;
using Android.Hardware.Camera2;
using Cycles.Droid.CustomViews;
using Cycles.Droid.Renderers;
using Cycles.Views;
using Xamarin.Forms;
using Android.Support.Constraints;
using Xamarin.Forms.Platform.Android;
using Application = Android.App.Application;
using Color = Android.Graphics.Color;
using Android.Support.Design.Widget;
using Android.Content.PM;
using Xamarin.Essentials;
using Android.Hardware;
using Cycles.Droid.Utils;
using Java.Lang.Reflect;
using Camera = Android.Hardware.Camera;
using ImageButton = Android.Widget.ImageButton;
using Object = Java.Lang.Object;

[assembly: ExportRenderer(typeof(CustomBarcodeScanner), typeof(BarcodeScannerRenderer))]

namespace Cycles.Droid.Renderers
{
    internal sealed class BarcodeScannerRenderer : PageRenderer
    {
        private ConstraintLayout MainLayout { get; set; }
        private const string TAG = "BarcodeTracker";
        private CameraSource MCameraSource { get; set; }
        internal bool PreviouslyScanned { get; set; }
        private ImageButton CloseScannerButton { get; set; }
        private CameraSourcePreview CameraSourcePreview { get; set; }
        private GraphicOverlay MGraphicOverlay { get; set; }
        private Camera CoreCamera { get; set; }
        private bool IsFlashOn { get; set; }
        public BarcodeScannerRenderer(Context context) : base(context)
        {
            MainLayout =
                (ConstraintLayout)LayoutInflater.FromContext(context).Inflate(Resource.Layout.BarcodeTracker, null);
            AddView(MainLayout);
            CameraSourcePreview = MainLayout.FindViewById<CameraSourcePreview>(Resource.Id.preview);
            CloseScannerButton = MainLayout.FindViewById<ImageButton>(Resource.Id.closeScannerBtn);
            CloseScannerButton.Click += async delegate
            {
                await Xamarin.Forms.Application.Current.MainPage.Navigation.PopModalAsync();
            };

            MGraphicOverlay = MainLayout.FindViewById<GraphicOverlay>(Resource.Id.faceOverlay);

            BarcodeDetector detector = new BarcodeDetector.Builder(Application.Context)
                .Build();
            detector.SetProcessor(
                new MultiProcessor.Builder(new GraphicBarcodeTrackerFactory(MGraphicOverlay, this)).Build());

            if (!detector.IsOperational)
            {
                // Note: The first time that an app using barcode API is installed on a device, GMS will
                // download a native library to the device in order to do detection.  Usually this
                // completes before the app is run for the first time.  But if that download has not yet
                // completed, then the above call will not detect any barcodes.
                //
                // IsOperational can be used to check if the required native library is currently
                // available.  The detector will automatically become operational once the library
                // download completes on device.
                Android.Util.Log.Warn(TAG, "Barcode detector dependencies are not yet available.");
            }
            MCameraSource = new CameraSource.Builder(Application.Context, detector)
                .SetAutoFocusEnabled(true)
                .SetFacing(Android.Gms.Vision.CameraFacing.Back)
                .SetRequestedFps(15.0f)
                .Build();

            var torchFab = FindViewById<FloatingActionButton>(Resource.Id.fab_torchlight);
            var hasFlashlight = Context.PackageManager.HasSystemFeature(PackageManager.FeatureCameraFlash);
            if (!hasFlashlight)
            {
                torchFab.Hide();
            }
            else
            {
                torchFab.Click += TorchFab_Click;
            }
            StartCameraSource();
        }

        private void TorchFab_Click(object sender, EventArgs e)
        {
            if (CoreCamera == null)
            {
                var javaCam = MCameraSource.JavaCast<Java.Lang.Object>();
                var fields = javaCam.Class.GetDeclaredFields();
                foreach (Field field in fields)
                {
                    if (!field.Type.CanonicalName.Equals("android.hardware.camera", StringComparison.OrdinalIgnoreCase))
                        continue;
                    field.Accessible = true;
                    Object camera = field.Get(javaCam);
                    CoreCamera = (Camera)camera;
                }
            }

            if (!IsFlashOn)
            {
                if (CoreCamera != null)
                {
                    Camera.Parameters prams = CoreCamera.GetParameters();
                    prams.FlashMode = Camera.Parameters.FlashModeTorch;
                    CoreCamera.SetParameters(prams);
                }

                IsFlashOn = true;
            }
            else
            {
                Camera.Parameters prams = CoreCamera.GetParameters();
                prams.FlashMode = Camera.Parameters.FlashModeOff;
                CoreCamera.SetParameters(prams);
                IsFlashOn = false;
            }
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            base.OnLayout(changed, l, t, r, b);

            if (MainLayout != null)
            {
                var width = r - l;
                var height = b - t;
                var msw = MeasureSpec.MakeMeasureSpec(width, MeasureSpecMode.Exactly);
                var msh = MeasureSpec.MakeMeasureSpec(height, MeasureSpecMode.Exactly);

                MainLayout.Measure(msw, msh);
                MainLayout.Layout(0, 0, width, height);

                CameraSourcePreview.Layout(0, 0, width, height);

                //ControlsBox.Layout(0, 0, width, );
            }
            base.OnLayout(changed, l, t, r, b);
        }

        /**
          * Starts or restarts the camera source, if it exists.  If the camera source doesn't exist yet
          * (e.g., because onResume was called before the camera source was created), this will be called
          * again when the camera source is created.
          */
        private void StartCameraSource()
        {
            try
            {
                CameraSourcePreview.Start(MCameraSource, MGraphicOverlay);
            }
            catch (Exception e)
            {
                Android.Util.Log.Error(TAG, "Unable to start camera source.", e);
                MCameraSource.Release();
                MGraphicOverlay = null;
//                Crashlytics.Crashlytics.LogException(Java.Lang.Throwable.FromException(e));
            }
        }

    }
}