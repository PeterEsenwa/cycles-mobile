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

[assembly: ExportRenderer(typeof(Cycles.Views.HowTo), typeof(HowToRenderer))]
namespace Cycles.Droid.Renderers
{
    public class HowToRenderer : PageRenderer
    {
        private float toolbarHeight;
        private LinearLayout _androidLinearLayout;
        private AppBarLayout _androidAppBarLayout;
        private MainActivity mainActivity;
        private Toolbar toolbar;
        private Android.Views.View webView;

        public HowToRenderer(Context context) : base(context)
        {
            mainActivity = Context as MainActivity;

            _androidLinearLayout = new LinearLayout(Context)
            {
                LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent)
            };

            _androidAppBarLayout = (AppBarLayout)LayoutInflater.FromContext(context).Inflate(Resource.Layout.Toolbar, null);

            toolbar = (Toolbar)_androidAppBarLayout.FindViewById(Resource.Id.toolbar);
            toolbar.FindViewById<TextView>(Resource.Id.cycles_text).Text = "How To Cycle";
            toolbar.FindViewById<Android.Widget.ImageButton>(Resource.Id.gift_button).Visibility = ViewStates.Invisible;

            _androidLinearLayout.AddView(_androidAppBarLayout);

            _androidAppBarLayout.LayoutParameters =
                new LinearLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent);

            AddView(_androidLinearLayout);

            mainActivity.SetSupportActionBar(toolbar);
            ActionBar actionBar = mainActivity.SupportActionBar;

            actionBar.SetDisplayHomeAsUpEnabled(true);
            actionBar.Title = "How To Cycle";
            actionBar.SetDisplayShowHomeEnabled(true);
            actionBar.SetDisplayShowTitleEnabled(false);
        }

        public sealed override void AddView(Android.Views.View child)
        {
            if (!(child is LinearLayout))
            {
                child.RemoveFromParent();
                _androidLinearLayout.AddView(child);
                child.LayoutParameters = new LinearLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);
                webView = child;
            }
            else
            {
                base.AddView(child);
            }
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
        {
            base.OnElementChanged(e);
            if (e.NewElement != null)
            {
                toolbarHeight = TypedValue.ApplyDimension(ComplexUnitType.Dip, 48, Resources.DisplayMetrics);
            }
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
                if (webView != null)
                {
                    webView.Measure(msw, msh);
                    webView.Layout(0, (int)toolbarHeight, r - l, b - t);
                }
            }
        }
    }
}