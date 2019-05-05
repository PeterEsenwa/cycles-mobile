using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Vision;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Xamarin.Essentials;

namespace Cycles.Droid.CustomViews
{
    public class CameraSourcePreview : LinearLayout
    {
        const string TAG = "CameraSourcePreview";

        Context mContext;
        SurfaceView mSurfaceView;
        bool mStartRequested;
        protected bool SurfaceAvailable { get; set; }
        CameraSource MCameraSource { get; set; }
        GraphicOverlay mOverlay;

        public CameraSourcePreview(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            mContext = context;
            mStartRequested = false;
            SurfaceAvailable = false;

            mSurfaceView = new SurfaceView(context);
            mSurfaceView.Holder.AddCallback(new SurfaceCallback(this));
            AddView(mSurfaceView, 0);
        }

        public void Start(CameraSource cameraSource)
        {
            if (cameraSource == null)
                Stop();

            MCameraSource = cameraSource;

            if (MCameraSource != null)
            {
                mStartRequested = true;
                StartIfReady();
            }
        }

        public void Start(CameraSource cameraSource, GraphicOverlay overlay)
        {
            mOverlay = overlay;
            Start(cameraSource);
        }

        public void Stop()
        {
            MCameraSource?.Stop();
        }

        public void Release()
        {
            if (MCameraSource != null)
            {
                MCameraSource.Release();
                MCameraSource = null;
            }
        }

        private void StartIfReady()
        {
            if (mStartRequested && SurfaceAvailable)
            {
                MCameraSource.Start(mSurfaceView.Holder);
                if (mOverlay != null)
                {
                    var size = MCameraSource.PreviewSize;
                    var min = Math.Min(size.Width, size.Height);
                    var max = Math.Max(size.Width, size.Height);
                    if (IsPortraitMode())
                    {
                        // Swap width and height sizes when in portrait, since it will be rotated by
                        // 90 degrees
                        mOverlay.SetCameraInfo(min, max, MCameraSource.CameraFacing);
                    }
                    else
                    {
                        mOverlay.SetCameraInfo(max, min, MCameraSource.CameraFacing);
                    }

                    mOverlay.Clear();
                }

                mStartRequested = false;
            }
        }

        private class SurfaceCallback : Java.Lang.Object, ISurfaceHolderCallback
        {
            public SurfaceCallback(CameraSourcePreview parent)
            {
                Parent = parent;
            }

            public CameraSourcePreview Parent { get; private set; }

            public void SurfaceCreated(ISurfaceHolder surface)
            {
                Parent.SurfaceAvailable = true;
                try
                {
                    Parent.StartIfReady();
                }
                catch (Exception ex)
                {
                    Log.Error(TAG, "Could not start camera source.", ex);
                    Crashlytics.Crashlytics.LogException(Java.Lang.Throwable.FromException(ex));
                }
            }

            public void SurfaceDestroyed(ISurfaceHolder surface)
            {
                Parent.SurfaceAvailable = false;
            }

            public void SurfaceChanged(ISurfaceHolder holder, Android.Graphics.Format format, int width, int height)
            {
            }
        }
        protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
        {
            //var mainDisplayInfo = DeviceDisplay.MainDisplayInfo;
            //int width = (int) mainDisplayInfo.Width;
            //int height = (int) mainDisplayInfo.Height;

            //if (mCameraSource != null)
            //{
            //    var size = mCameraSource.PreviewSize;
            //    if (size != null)
            //    {
            //        width = size.Width;
            //        height = size.Height;
            //    }
            //}

            var layoutWidth = right - left;
            var layoutHeight = bottom - top;

            var height = (int)(layoutHeight / 1.5);
            // Computes height and width for potentially doing fit width.
            //int childWidth = layoutWidth;
            //int childHeight = (int)((layoutWidth / (float)width) * height);
            //int childHeight = childWidth;
            // If height is too tall using fit width, does fit height instead.
            //if (childHeight > layoutHeight)
            //{
            //    childHeight = layoutHeight;
            //    //childWidth = (int)((layoutHeight / (float)height) * width);
            //}
            //Height = ((int)height / 2) - 64;

            for (int i = 0; i < ChildCount; ++i)
            {
                View child = GetChildAt(i);
                if (child is SurfaceView)
                {
                    child.Layout(0, 0, layoutWidth, height);
                }
                else if (child is GraphicOverlay)
                {
                    child.Layout(0, 0, 0, 0);
                }
                
            }

            try
            {
                StartIfReady();
                //ControlsBox = FindViewById<RelativeLayout>(Resource.Id.controls_overlay);
     
            }
            catch (Exception ex)
            {
                Log.Error(TAG, "Could not start camera source.", ex);
                Crashlytics.Crashlytics.LogException(Java.Lang.Throwable.FromException(ex));
            }
        }

        bool IsPortraitMode()
        {
            var orientation = mContext.Resources.Configuration.Orientation;
            if (orientation == Android.Content.Res.Orientation.Landscape)
                return false;
            if (orientation == Android.Content.Res.Orientation.Portrait)
                return true;

            Android.Util.Log.Debug(TAG, "isPortraitMode returning false by default");
            return false;
        }
    }

    public class GraphicOverlay : View
    {
        object mLock = new object();
        int mPreviewWidth;
        float mWidthScaleFactor = 2.0f;
        int mPreviewHeight;
        float mHeightScaleFactor = 2.0f;
        CameraFacing mFacing = CameraFacing.Back;
        List<Graphic> mGraphics = new List<Graphic>();

        /**
     * Base class for a custom graphics object to be rendered within the graphic overlay.  Subclass
     * this and implement the {@link Graphic#draw(Canvas)} method to define the
     * graphics element.  Add instances to the overlay using {@link GraphicOverlay#add(Graphic)}.
     */
        public abstract class Graphic
        {
            private GraphicOverlay mOverlay;

            public Graphic(GraphicOverlay overlay)
            {
                mOverlay = overlay;
            }

            /**
             * Draw the graphic on the supplied canvas.  Drawing should use the following methods to
             * convert to view coordinates for the graphics that are drawn:
             * <ol>
             * <li>{@link Graphic#scaleX(float)} and {@link Graphic#scaleY(float)} adjust the size of
             * the supplied value from the preview scale to the view scale.</li>
             * <li>{@link Graphic#translateX(float)} and {@link Graphic#translateY(float)} adjust the
             * coordinate from the preview's coordinate system to the view coordinate system.</li>
             * </ol>
             *
             * @param canvas drawing canvas
             */
            public abstract void Draw(Canvas canvas);

            /**
         * Adjusts a horizontal value of the supplied value from the preview scale to the view
         * scale.
         */
            public float ScaleX(float horizontal)
            {
                return horizontal * mOverlay.mWidthScaleFactor;
            }

            /**
         * Adjusts a vertical value of the supplied value from the preview scale to the view scale.
         */
            public float ScaleY(float vertical)
            {
                return vertical * mOverlay.mHeightScaleFactor;
            }

            /**
         * Adjusts the x coordinate from the preview's coordinate system to the view coordinate
         * system.
         */
            public float TranslateX(float x)
            {
                if (mOverlay.mFacing == CameraFacing.Front)
                {
                    return mOverlay.Width - ScaleX(x);
                }

                return ScaleX(x);
            }

            /**
             * Adjusts the y coordinate from the preview's coordinate system to the view coordinate
             * system.
             */
            public float TranslateY(float y)
            {
                return ScaleY(y);
            }

            public void PostInvalidate()
            {
                mOverlay.PostInvalidate();
            }
        }

        public GraphicOverlay(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        /**
         * Removes all graphics from the overlay.
         */
        public void Clear()
        {
            lock (mLock)
            {
                mGraphics.Clear();
            }

            PostInvalidate();
        }

        /**
         * Adds a graphic to the overlay.
         */
        public void Add(Graphic graphic)
        {
            lock (mLock)
            {
                mGraphics.Add(graphic);
            }

            PostInvalidate();
        }

        /**
         * Removes a graphic from the overlay.
         */
        public void Remove(Graphic graphic)
        {
            lock (mLock)
            {
                mGraphics.Remove(graphic);
                mGraphics.Clear();
            }

            PostInvalidate();
        }

        /**
         * Sets the camera attributes for size and facing direction, which informs how to transform
         * image coordinates later.
         */
        public void SetCameraInfo(int previewWidth, int previewHeight, CameraFacing facing)
        {
            lock (mLock)
            {
                mPreviewWidth = previewWidth;
                mPreviewHeight = previewHeight;
                mFacing = facing;
            }

            PostInvalidate();
        }

        /**
         * Draws the overlay with its associated graphic objects.
         */

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            lock (mLock)
            {
                if ((mPreviewWidth != 0) && (mPreviewHeight != 0))
                {
                    mWidthScaleFactor = (float)canvas.Width / (float)mPreviewWidth;
                    mHeightScaleFactor = (float)canvas.Height / (float)mPreviewHeight;
                }

                foreach (Graphic graphic in mGraphics)
                {
                    graphic.Draw(canvas);
                }
            }
        }
    }
}