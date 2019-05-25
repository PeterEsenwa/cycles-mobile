using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
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

[assembly: ExportRenderer(typeof(CustomBarcodeScanner), typeof(BarcodeScannerRenderer))]

namespace Cycles.Droid.Renderers
{
    sealed class BarcodeScannerRenderer : PageRenderer
    {
        private ConstraintLayout MainLayout { get; set; }
        const string TAG = "BarcodeTracker";
        private CameraSource MCameraSource { get; set; }
        private bool _previouslyScanned { get; set; }
        private CameraSourcePreview CameraSourcePreview { get; set; }
        private GraphicOverlay MGraphicOverlay { get; set; }
        public Android.Hardware.Camera CoreCamera { get; private set; }
        AlertDialog.Builder alertDialog { get; set; }
        public bool IsFlashOn { get; set; }
        public BarcodeScannerRenderer(Context context) : base(context)
        {
            MainLayout =
                (ConstraintLayout)LayoutInflater.FromContext(context).Inflate(Resource.Layout.BarcodeTracker, null);
            AddView(MainLayout);
            CameraSourcePreview = MainLayout.FindViewById<CameraSourcePreview>(Resource.Id.preview);
            //ControlsBox = MainLayout.FindViewById<Android.Widget.RelativeLayout>(Resource.Id.controls_overlay);
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
                foreach (var field in fields)
                {
                    if (field.Type.CanonicalName.Equals("android.hardware.camera", StringComparison.OrdinalIgnoreCase))
                    {
                        field.Accessible = true;
                        var camera = field.Get(javaCam);
                        CoreCamera = (Android.Hardware.Camera)camera;
                    }
                }
            }

            if (!IsFlashOn)
            {
                var prams = CoreCamera.GetParameters();
                prams.FlashMode = Android.Hardware.Camera.Parameters.FlashModeTorch;
                CoreCamera.SetParameters(prams);
                IsFlashOn = true;
            }
            else
            {
                var prams = CoreCamera.GetParameters();
                prams.FlashMode = Android.Hardware.Camera.Parameters.FlashModeOff;
                CoreCamera.SetParameters(prams);
                IsFlashOn = false;
            }
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            base.OnLayout(changed, l, t, r, b);

            if (MainLayout != null)
            {
                int width = r - l;
                int height = b - t;
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
                Crashlytics.Crashlytics.LogException(Java.Lang.Throwable.FromException(e));
            }
        }

        /**
         * Factory for creating a face tracker to be associated with a new face.  The multiprocessor
         * uses this factory to create face trackers as needed -- one for each individual.
         */
        private class GraphicBarcodeTrackerFactory : Java.Lang.Object, MultiProcessor.IFactory
        {
            BarcodeScannerRenderer parent;
            public GraphicBarcodeTrackerFactory(GraphicOverlay overlay, BarcodeScannerRenderer parent) : base()
            {
                this.parent = parent;
                Overlay = overlay;
            }

            private GraphicOverlay Overlay { get; set; }

            public Tracker Create(Java.Lang.Object item)
            {
                return new GraphicBarcodeTracker(parent);
            }
        }

        /**
         *Tracker for each detected barcode. This maintains a face graphic within the app's
         * associated face overlay.
         */
        public class GraphicBarcodeTracker : Tracker
        {
            BarcodeScannerRenderer parent;

            public GraphicBarcodeTracker(BarcodeScannerRenderer parent)
            {
                this.parent = parent;
            }

            /**
* Start tracking the detected face instance within the face overlay.
*/
            public override void OnNewItem(int idValue, Java.Lang.Object item)
            {
                var id = idValue;
            }

            /**
            * Update the position/characteristics of the face within the overlay.
            */
            public override void OnUpdate(Detector.Detections detections, Java.Lang.Object item)
            {
                //Video barcode DisplayValue
                const string videoDisplayVal = "http://download.jimicloud.com/webDown/mibike?no=7551008104";
                var barcode = item.JavaCast<Barcode>();
                var stringValue = barcode.DisplayValue;

                if (!parent._previouslyScanned)
                {
                    MessagingCenter.Send<GraphicBarcodeTracker, string>(this, "Barcode Scanned", stringValue);
                    MessagingCenter.Send(this, "Close Scanner");
                    parent._previouslyScanned = true;
                }

                //.Make(parent.MainLayout, "To start ride click Unlock. You are on PAYG", Snackbar.LengthLong)
                //.SetAction("Unlock",
                //    v => { })
                //.Show();
                //if (videoDisplayVal.Equals(stringValue) && !_previouslyScanned)
                //{
                //    //MessagingCenter.Send(this, "Close Scanner");

                //    //AView layout = FindViewById(Android.Resource.Id.Content);
                //    Snackbar
                //        .Make(parent.MainLayout, "To start ride click Unlock. You are on PAYG", Snackbar.LengthLong)
                //        .SetAction("Unlock",
                //            v => { })
                //        .Show();


                //}
            }
        }
    }
}