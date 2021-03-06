﻿using System;
using Android.Util;
using Cycles.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Cycles
{
    public partial class App
    {
        public static int ScreenHeight { get; set; }

        public static int ScreenWidth { get; set; }

        public static float ScreenDensity { get; internal set; }

        public App()
        {
            InitializeComponent();
            MainPage = new RootPage();
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
