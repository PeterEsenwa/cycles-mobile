using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;

namespace Cycles.Droid
{
    [Activity(Label = "Cycles", Icon = "@mipmap/icon", Theme = "@style/Splash", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class Splash : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            StartActivity(new Intent(this, typeof(MainActivity)));
            Finish();
            // Create your application here
        }


    }
}