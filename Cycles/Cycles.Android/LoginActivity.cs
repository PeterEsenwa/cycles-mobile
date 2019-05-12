using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gms.Tasks;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Firebase;
using Firebase.Analytics;
using Firebase.DynamicLinks;
using Java.Lang;

namespace Cycles.Droid
{
    [Activity(Label = "Login", Theme = "@style/LoginTheme")]
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataSchemes = new string[] { "http", "https" }, DataHosts = new string[] { "cycles.page.link", "cycles.page.link" }, Label = "filter_sign_up_intent", AutoVerify = true)]
    public class LoginActivity : Activity, IOnSuccessListener
    {
        public void OnSuccess(Java.Lang.Object result)
        {
            _ = Log.Debug(Class.ToString(), result.Class.ToString());
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Login);
            FindViewById<ImageButton>(Resource.Id.login_button).Click += LoginToApp;

            // Create your application here
            FirebaseAnalytics mFirebaseAnalytics = FirebaseAnalytics.GetInstance(this);
            FirebaseOptions options = new FirebaseOptions.Builder()
              .SetApiKey("AIzaSyBnw6unIyRQ4XfFZNekTpU7rWumSvv5cnw")
              .SetApplicationId("1:316655980255:android:05c55f9b9a1c0243")
              .Build();
            FirebaseApp firebaseApp = FirebaseApp.Instance ?? FirebaseApp.InitializeApp(ApplicationContext, options);
            Intent intent = Intent;
            string dataString = intent?.DataString;
            Android.Net.Uri data = intent?.Data;

            if (dataString != null && data != null)
            {
                Bundle bundle = new Bundle();
                bundle.PutString(FirebaseAnalytics.Param.ItemId, data.Host);
                bundle.PutString(FirebaseAnalytics.Param.ItemName, data.PathSegments[1]);
                bundle.PutString(FirebaseAnalytics.Param.ContentType, "text");
                mFirebaseAnalytics.LogEvent(FirebaseAnalytics.Event.GenerateLead, bundle);
            }
        }

        private void LoginToApp(object sender, EventArgs e)
        {
            StartActivity(new Intent(this, typeof(MainActivity)));
        }
    }
}