using System.Collections.Generic;
using Android.Content;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Util;
using Cycles.Droid.Renderers;
using Cycles.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Toolbar = Android.Support.V7.Widget.Toolbar;
[assembly: ExportRenderer(typeof(CustomTabView), typeof(CustomTabViewRenderer))]
namespace Cycles.Droid.Renderers
{
    class CustomTabViewRenderer : Xamarin.Forms.Platform.Android.AppCompat.ViewRenderer<CustomTabView, ViewPager>
    {
        TabLayout tabLayout;
        private MainActivity mainActivity;
        private Toolbar toolbar;
        public int TabCount { get; set; }
        public IList<Xamarin.Forms.View> Children { get; private set; }

        //public LinearLayout ll;
        private ViewPager viewPager;

        public CustomTabViewRenderer(Context context) : base(context)
        {
            mainActivity = Context as MainActivity;
            viewPager = (ViewPager) mainActivity.LayoutInflater.Inflate(Resource.Layout.TermsAndPrivacy, null);
            tabLayout = viewPager.FindViewById<TabLayout>(Resource.Id.tabLayout1);
            tabLayout.SetupWithViewPager(viewPager);
            /**
            //toolbarHeight = TypedValue.ApplyDimension(ComplexUnitType.Dip, Resource.Attribute.actionBarSize, Resources.DisplayMetrics);

            //ll = new LinearLayout(Context);
            //ll.Orientation = Orientation.Vertical;

            //toolbar = (Toolbar)mainActivity.LayoutInflater.Inflate(Resource.Layout.Toolbar, this, false);
            //toolbar.FindViewById<TextView>(Resource.Id.cycles_text).Gravity = GravityFlags.Left | GravityFlags.CenterVertical;
            //toolbar.FindViewById<TextView>(Resource.Id.cycles_text).Text = "Dashboard";
            //toolbar.FindViewById<Android.Widget.ImageButton>(Resource.Id.gift_button).SetImageResource(Resource.Drawable.edit_pencil);

            //layout = toolbar?.FindViewById<TabLayout>(Resource.Id.tabsLayout);

            //ViewGroup.AddView(ll, 0);
            //ViewGroup.AddView(toolbar);
            //toolbar.LayoutParameters.Height = (int)toolbarHeight;
            //toolbar.LayoutParameters.Width = LayoutParams.MatchParent;
            //ll.SetBackgroundColor(Android.Graphics.Color.Green);
            //_androidAppBarLayout.LayoutParameters.Height = LayoutParams.WrapContent;
            //_androidAppBarLayout.LayoutParameters.Width = LayoutParams.MatchParent;
    */

        }


        protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
        {
            base.OnLayout(changed, left, top, right, bottom);
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<CustomTabView> e)
        {
            base.OnElementChanged(e);
            if (e.NewElement != null)
            {
                TabCount = e.NewElement.Children.Count;
                Children = e.NewElement.Children;
                SetNativeControl(viewPager);
                //viewPager.Adapter = new CustomPagerAdapter(this);
            }

            if (e.OldElement != null)
            {

            }
        }

        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            /**
            if (e.PropertyName == "Renderer")
            {
                //ll.AddView(toolbar);
                //layout = (TabLayout)ViewGroup.GetChildAt(1);
                for (int i = 0; i < ViewGroup.ChildCount; i++)
                {
                    var child = ViewGroup.GetChildAt(i);
                    //child.Visibility = ViewStates.Gone;
                    //if (child is TabLayout)
                    //{
                    //    ((TabLayout)child).RemoveFromParent();
                    //    ((TabLayout)child).Visibility = ViewStates.Gone;
                    //}
                    if (child is ViewPager)
                    {
                        viewPager = (ViewPager)child;
                        viewPager.RemoveFromParent();
                        //ll.AddView(viewPager);
                        viewPager.Visibility = ViewStates.Gone;
                    }
                }

                //layout.SetupWithViewPager(viewPager);


                //LayoutParams lp2 = toolbar.LayoutParameters;
                //int h = lp.Height;

                //toolbar.AddView(layout, 1);
                //layout.Visibility = ViewStates.Gone;
                //mainActivity.SetSupportActionBar(toolbar);
                //ActionBar actionBar = mainActivity.SupportActionBar;

                //actionBar.SetDisplayHomeAsUpEnabled(true);
                //actionBar.SetDisplayShowHomeEnabled(true);
                //actionBar.SetDisplayShowTitleEnabled(false);
                */

            //}
        }
    }
}