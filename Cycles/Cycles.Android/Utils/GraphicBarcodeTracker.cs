using Android.Gms.Vision;
using Android.Gms.Vision.Barcodes;
using Android.Runtime;
using Cycles.Droid.Renderers;
using Xamarin.Forms;

namespace Cycles.Droid.Utils
{
    /**
     *Tracker for each detected barcode. This maintains a face graphic within the app's
     * associated face overlay.
     */
    internal class GraphicBarcodeTracker : Tracker
    {
        private readonly BarcodeScannerRenderer _parent;

        public GraphicBarcodeTracker(BarcodeScannerRenderer parent)
        {
            _parent = parent;
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
            const string barcodeMibikeNo = "http://download.jimicloud.com/webDown/mibike?no=7551008104";
            var barcode = item.JavaCast<Barcode>();
            var stringValue = barcode.DisplayValue;

            if (_parent.PreviouslyScanned) return;
            MessagingCenter.Send<GraphicBarcodeTracker, string>(this, "Barcode Scanned", stringValue);
            MessagingCenter.Send(this, "Close Scanner");
            _parent.PreviouslyScanned = true;

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