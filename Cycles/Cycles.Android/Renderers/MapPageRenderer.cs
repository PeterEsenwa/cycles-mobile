using Android.Content;
using Android.Gms.Maps;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using Cycles.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Maps.Android;
using Xamarin.Forms.Platform.Android;
using AView = Android.Views.View;

[assembly: ExportRenderer(typeof(Cycles.MapPage), typeof(MapPageRenderer))]
namespace Cycles.Droid.Renderers
{
    internal class MapPageRenderer : PageRenderer
    {
        private CoordinatorLayout _androidCoordinatorLayout;
        private LinearLayout LinearLayout;
        private ViewGroup viewGroup;
        private bool firstRun = true;
        private MapRenderer mapView;

        public MapPageRenderer(Context context) : base(context)
        {
            _androidCoordinatorLayout = new CoordinatorLayout(Context);
            _androidCoordinatorLayout.LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);
            LinearLayout = new LinearLayout(Context)
            {
                Orientation = Orientation.Vertical,
                LayoutParameters = new LayoutParams(LayoutParams.MatchParent, 250)
            };
            AddView(_androidCoordinatorLayout);
        }



        protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || Element == null)
            {
                if (Element == null)
                {
                    viewGroup = null;
                    LinearLayout = null;
                    _androidCoordinatorLayout = null;
                }

                return;
            }

            //AddView(_androidCoordinatorLayout,0);
        }

        private void InitView()
        {
            LinearLayout.SetBackgroundColor(Android.Graphics.Color.White);
            _androidCoordinatorLayout.AddView(LinearLayout);
            CoordinatorLayout.LayoutParams parameters = (CoordinatorLayout.LayoutParams)LinearLayout.LayoutParameters;

            BottomSheetBehavior bottomSheetBehavior = new BottomSheetBehavior
            {
                PeekHeight = 150,
                Hideable = false,
            };

            parameters.Behavior = bottomSheetBehavior;
            LinearLayout.LayoutParameters = parameters;
        }

        public override void AddView(AView child)
        {
            child.RemoveFromParent();
            base.AddView(child);
            if (!(child is CoordinatorLayout))
            {
                child.RemoveFromParent();
                LayoutParams layoutParams = new LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent);
                ((ViewGroup)child).LayoutParameters = layoutParams;
                viewGroup = (ViewGroup)child;
                for (int i = 0; i < viewGroup.ChildCount; i++)
                {
                    AView foundChild = viewGroup.GetChildAt(i);
                    if (foundChild is MapRenderer)
                    {
                        mapView = (MapRenderer)foundChild;
                    }
                }
                _androidCoordinatorLayout.AddView(viewGroup);
                InitView();
            }
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            base.OnLayout(changed, l, t, r, b);

            if (_androidCoordinatorLayout != null)
            {
                var msw = MeasureSpec.MakeMeasureSpec(r - l, MeasureSpecMode.Exactly);
                var msh = MeasureSpec.MakeMeasureSpec(b - t, MeasureSpecMode.Exactly);

                _androidCoordinatorLayout.Measure(msw, msh);
                _androidCoordinatorLayout.Layout(0, 0, r - l, b - t);

                int CoordinatorHeightDiff = _androidCoordinatorLayout.Height - 150;
                viewGroup.LayoutParameters.Height = CoordinatorHeightDiff;
                mapView.LayoutParameters.Height = CoordinatorHeightDiff;

                viewGroup.Measure(msw, msh);
                mapView.Measure(msw, msh);

                viewGroup.Layout(0, 0, r - l, CoordinatorHeightDiff - t);
                mapView.Layout(0, 0, r - l, CoordinatorHeightDiff - t);
            }
        }


        //private void SetupCoordinatorLayout()
        //{
        //    List<AView> Children = GetAllChildren(this);
        //    Children.ForEach(
        //        x =>
        //        {
        //            x.RemoveFromParent();
        //            _androidCoordinatorLayout.AddView(x);
        //        });
        //    AddView(_androidCoordinatorLayout);
        //}

        //public List<AView> GetAllChildren(AView v)
        //{
        //    ViewGroup viewGroup = (ViewGroup)v;
        //    List<AView> viewList = new List<AView>();
        //    for (int i = 0; i < viewGroup.ChildCount; i++)
        //    {
        //        AView child = viewGroup.GetChildAt(i);
        //        viewList.Add(child);
        //    }
        //    return viewList;
        //}
    }

}