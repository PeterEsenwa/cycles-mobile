using System;
using Android.Util;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Cycles
{
    public partial class App : Application
    {
        public static int ScreenHeight;

        public static int ScreenWidth;

        public static float ScreenDensity { get; internal set; }

        public App()
        {
            InitializeComponent();

            MainPage = new Login();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
