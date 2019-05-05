using Android.Content;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Cycles.Droid.Renderers;
using Cycles.Views;
using Java.Lang;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Xamarin.Forms.Platform.Android.AppCompat;
using Cycles.Utils;

[assembly: ExportRenderer(typeof(TermsAndPrivacy), typeof(TermsAndPrivacyRenderer))]
namespace Cycles.Droid.Renderers
{
    class TermsAndPrivacyRenderer : PageRenderer
    {
        private float toolbarHeight;

        public CoordinatorLayout _androidCoordinatorLayout { get; private set; }

        private AppBarLayout _androidAppBarLayout;
        private MainActivity mainActivity;
        private Toolbar toolbar;
        private ViewPager viewPager;
        private TabLayout tabLayout;

        public TermsAndPrivacyRenderer(Context context) : base(context)
        {
            mainActivity = Context as MainActivity;
            toolbarHeight = TypedValue.ApplyDimension(ComplexUnitType.Dip, 48, Resources.DisplayMetrics);

            _androidCoordinatorLayout = (CoordinatorLayout) mainActivity.LayoutInflater.Inflate(Resource.Layout.TermsAndPrivacy, null);

            _androidAppBarLayout = _androidCoordinatorLayout.FindViewById<AppBarLayout>(Resource.Id.termsAppBar);

            toolbar = _androidAppBarLayout.FindViewById<Toolbar>(Resource.Id.termsToolBar);
            toolbar.FindViewById<Android.Widget.ImageButton>(Resource.Id.gift_button).Visibility = ViewStates.Invisible;

            //if (toolbar.LayoutParameters != null)
            //{
            //    ((AppBarLayout.LayoutParams)toolbar.LayoutParameters).ScrollFlags = AppBarLayout.LayoutParams.ScrollFlagEnterAlways;
            //}

            mainActivity?.SetSupportActionBar(toolbar);
            ActionBar actionBar = mainActivity?.SupportActionBar;

            if (actionBar != null)
            {
                actionBar.SetDisplayHomeAsUpEnabled(true);
                actionBar.SetDisplayShowHomeEnabled(true);
                actionBar.SetDisplayShowTitleEnabled(false);
            }

            viewPager = _androidCoordinatorLayout.FindViewById<ViewPager>(Resource.Id.pager);

            tabLayout = _androidAppBarLayout.FindViewById<TabLayout>(Resource.Id.tabLayout1);
            tabLayout.SetupWithViewPager(viewPager);

            AddView(_androidCoordinatorLayout);

            //mainActivity.SetSupportActionBar(toolbar);
            //ActionBar actionBar = mainActivity.SupportActionBar;

            //actionBar.SetDisplayHomeAsUpEnabled(true);
            //actionBar.SetDisplayShowHomeEnabled(true);
            //actionBar.SetDisplayShowTitleEnabled(false);

        }

        protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
        {
            base.OnElementChanged(e);
            if (e.NewElement != null)
            {
                viewPager.Adapter = new CustomPagerAdapter(this, e.NewElement as TermsAndPrivacy);
              
            }
        }

        protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
        {
            base.OnLayout(changed, left, top, right, bottom);
            if (changed)
            {
                _androidCoordinatorLayout.Layout(left, top, right - left, bottom - top);
            }
        }

        public override void AddView(Android.Views.View child)
        {
            base.AddView(child);
            if (!(child is CoordinatorLayout))
            {
                child.RemoveFromParent();
            }
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
            _androidCoordinatorLayout.Measure(widthMeasureSpec, heightMeasureSpec);
        }
    }

}