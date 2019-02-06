using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Preferences;
using Android.Support.V4.Content;
using Android.Widget;

namespace Cycles.Droid
{
    [Activity(Label = "Cycles", Icon = "@mipmap/icon", Theme = "@style/Splash", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class Splash : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        //private AnimationDrawable splashDrawable;
        //private LayerDrawable splashDrawable;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(this);
            if (!sharedPreferences.GetBoolean("first_run", false))
            {
                SetContentView(Resource.Layout.Splash);
            }
            else
            {
                StartActivity(new Intent(this, typeof(MainActivity)));
                Finish();
            }
            //splashLayout.Background.SetVisible(false, false);
            //splashLayout.Background.SetAlpha(0);



            RelativeLayout splashLayout = FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);



        }

        public bool HasNavBar(Android.Content.Res.Resources resources)
        {
            int id = resources.GetIdentifier("config_showNavigationBar", "bool", "android");
            return id > 0 && resources.GetBoolean(id);
        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);
            //splashDrawable.Start();
        }
        public override void OnBackPressed() { }
    }









    //splashDrawable = (AnimationDrawable)ContextCompat.GetDrawable(this, Resource.Drawable.splash_screen);
    //RelativeLayout asteroidImage = FindViewById<RelativeLayout>(Resource.Id.relativeLayout1);
    //ImageView asteroidImage = FindViewById<ImageView>(Resource.Id.new_image);
    //asteroidImage.SetBackgroundResource(Resource.Drawable.splash_screen);
    //BitmapDrawable cyclesLogo = (BitmapDrawable)splashDrawable.GetDrawable(1);
    //cyclesLogo.SetAlpha(0);
    //Button asteroidButton = FindViewById<Button>(Resource.Id.new_button);
    //asteroidButton.Click += (sender, e) =>
    //{
    //    splashDrawable.Start();
    //};
    //ImageView cycles_logo = (ImageView)FindViewById(Resource.Id.cycles_logo);
    //Window.SetNavigationBarColor(Color.White);
    //if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
    //{
    //    View decorView = Window.DecorView;
    //    decorView.SetFitsSystemWindows(false);

    //    var uiOptions = (int)decorView.SystemUiVisibility;
    //    var newUiOptions = uiOptions;

    //    //window.AddFlags(WindowManagerFlags.TranslucentStatus);
    //    Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
    //    //window.SetStatusBarColor(Color.FromHex("#99d7282f").ToAndroid());
    //    //newUiOptions |= (int)SystemUiFlags.LayoutFullscreen;
    //    newUiOptions |= (int)SystemUiFlags.LightNavigationBar;
    //    //newUiOptions |= (int)SystemUiFlags.LayoutStable;

    //    decorView.SystemUiVisibility = (StatusBarVisibility)newUiOptions;
    //}

    //cyclesLogo.Gravity = Android.Views.GravityFlags.NoGravity;
    ////cyclesLogo.
    //int no = splashDrawable.NumberOfLayers;
    //var h = cyclesLogo.IntrinsicHeight;
    ////var foo = splashDrawable.GetLayerInsetTop(1);
    //splashDrawable.SetLayerInset(1, -20, -10, 0, 20);
    //var foo = splashDrawable.GetDrawable(0);
    ////var cyclesLogo = (BitmapDrawable)splashDrawable.GetDrawable(1);
    ////cyclesLogo.Gravity = Android.Views.GravityFlags.Top | Android.Views.GravityFlags.Start;
    ////foo.SetVisible(false, false);
    ////cyclesLogo.SetVisible(false, false);
    //ObjectAnimator animator = ObjectAnimator.OfInt(cyclesLogo, "Alpha", 255, 0);
    //ObjectAnimator animator2 = ObjectAnimator.OfInt(foo, "alpha", 255, 0);
    //animator2.SetDuration(1000);
    //animator2.Start();
    // Create your application here
}