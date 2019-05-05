using Android.Content;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Cycles.Droid.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Toolbar = Android.Support.V7.Widget.Toolbar;

[assembly: ExportRenderer(typeof(Cycles.Views.Dashboard), typeof(DashboardRenderer))]
namespace Cycles.Droid.Renderers
{
    public class DashboardRenderer : PageRenderer
    {
        private LinearLayout _androidLinearLayout;
        private float toolbarHeight;
        private AppBarLayout _androidAppBarLayout;
        private MainActivity mainActivity;
        private Toolbar toolbar;
        private Android.Views.View scrollView;

        public DashboardRenderer(Context context) : base(context)
        {
            mainActivity = Context as MainActivity;

            _androidLinearLayout = new LinearLayout(Context)
            {
                LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent)
            };

            toolbarHeight = TypedValue.ApplyDimension(ComplexUnitType.Dip, 48, Resources.DisplayMetrics);
            _androidAppBarLayout = new AppBarLayout(Context)
            {
                LayoutParameters = new LinearLayout.LayoutParams(LayoutParams.MatchParent, (int)toolbarHeight)
            };

            toolbar = (Toolbar)LayoutInflater.FromContext(context).Inflate(Resource.Layout.Toolbar, null);
            toolbar.FindViewById<TextView>(Resource.Id.cycles_text).Gravity = GravityFlags.Center;
            toolbar.FindViewById<TextView>(Resource.Id.cycles_text).Text = "Dashboard";
            toolbar.FindViewById<Android.Widget.ImageButton>(Resource.Id.gift_button).SetImageResource(Resource.Drawable.edit_pencil);
            _androidAppBarLayout.AddView(toolbar);
            
            ((AppBarLayout.LayoutParams) toolbar.LayoutParameters).Height = LayoutParams.WrapContent;
            ((AppBarLayout.LayoutParams) toolbar.LayoutParameters).Width = LayoutParams.MatchParent;
            
            _androidLinearLayout.AddView(_androidAppBarLayout);
            AddView(_androidLinearLayout);

            mainActivity.SetSupportActionBar(toolbar);
            ActionBar actionBar = mainActivity.SupportActionBar;

            actionBar.SetDisplayHomeAsUpEnabled(true);
            actionBar.SetDisplayShowHomeEnabled(true);
            actionBar.SetDisplayShowTitleEnabled(false);
        }

        public override void AddView(Android.Views.View child)
        {
            base.AddView(child);
            if (!(child is LinearLayout))
            {
                child.RemoveFromParent();
                _androidLinearLayout.AddView(child);
                child.LayoutParameters = new LinearLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);
                scrollView = child;
            }
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
        {
            base.OnElementChanged(e);
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            base.OnLayout(changed, l, t, r, b);

            if (_androidLinearLayout != null)
            {
                var msw = MeasureSpec.MakeMeasureSpec(r - l, MeasureSpecMode.Exactly);
                var msh = MeasureSpec.MakeMeasureSpec(b - t, MeasureSpecMode.Exactly);

                _androidLinearLayout.Measure(msw, msh);
                _androidLinearLayout.Layout(0, 0, r - l, b - t);
                if (scrollView != null)
                {
                    scrollView.Measure(msw, msh);
                    scrollView.Layout(0, (int)toolbarHeight, r - l, b - t);
                }
            }
        }
    }
}