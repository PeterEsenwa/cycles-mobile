using Android.Gms.Vision;
using Cycles.Droid.CustomViews;
using Cycles.Droid.Renderers;

namespace Cycles.Droid.Utils
{
    /**
     * Factory for creating a face tracker to be associated with a new face.  The multiprocessor
     * uses this factory to create face trackers as needed -- one for each individual.
     */
    internal class GraphicBarcodeTrackerFactory : Java.Lang.Object, MultiProcessor.IFactory
    {
        readonly BarcodeScannerRenderer _parent;
        public GraphicBarcodeTrackerFactory(GraphicOverlay overlay, BarcodeScannerRenderer parent) : base()
        {
            _parent = parent;
            Overlay = overlay;
        }

        private GraphicOverlay Overlay { get; set; }

        public Tracker Create(Java.Lang.Object item)
        {
            return new GraphicBarcodeTracker(_parent);
        }
    }
}