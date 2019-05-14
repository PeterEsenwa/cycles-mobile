using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Support.V4.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Cycles.Droid.Utils;
using Firebase.Auth;
using Android.Gms.Tasks;
using Java.Lang;
using Firebase;

namespace Cycles.Droid
{
    public class EmailSignUpFragment : DialogFragment, IOnSuccessListener, IOnFailureListener, IOnCompleteListener
    {
        public View EmailSignUpLayout { get; private set; }


        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            Dialog.Window.RequestFeature(WindowFeatures.NoTitle);
            EmailSignUpLayout = inflater.Inflate(Resource.Layout.EmailSignupLayout, container, false);

            Button emailSignUpGoBtn = EmailSignUpLayout.FindViewById<Button>(Resource.Id.emailSignUpGoBtn);
            EditText emailSignUpEditText = EmailSignUpLayout?.FindViewById<EditText>(Resource.Id.emailEditText);

            emailSignUpEditText.AddTextChangedListener(new EmailSignUpTextWatcher(emailSignUpGoBtn));
            emailSignUpGoBtn.Click += EmailSignUpGo_Clicked;

            return EmailSignUpLayout;
        }

        public void OnFailure(Java.Lang.Exception e)
        {
            Log.Error(Tag, "Error occured during email sending from Firebase", e);
            Crashlytics.Crashlytics.LogException(e);
        }

        public void OnComplete(Task task)
        {
            if (task.IsSuccessful)
            {
                var res = task.Result;
            }
        }
        public void OnSuccess(Java.Lang.Object result)
        {
            Activity.SupportFragmentManager.BeginTransaction().Commit();
        }

        private void EmailSignUpGo_Clicked(object sender, EventArgs e)
        {
            try
            {
                FirebaseOptions options = new FirebaseOptions.Builder()
                .SetApiKey("AIzaSyBnw6unIyRQ4XfFZNekTpU7rWumSvv5cnw")
                .SetApplicationId("1:316655980255:android:05c55f9b9a1c0243")
                .Build();
                FirebaseApp App = FirebaseApp.Instance ?? FirebaseApp.InitializeApp(Activity, options);

                ActionCodeSettings actionCodeSettings = ActionCodeSettings.NewBuilder()
                      .SetUrl("https://cycles.page.link/tc4X")
                      .SetHandleCodeInApp(true)
                      .SetAndroidPackageName(Activity.PackageName, true, null)
                      .Build();
                
                FirebaseAuth auth = FirebaseAuth.GetInstance(App);
                if (auth.CurrentUser != null)
                {
                    auth.SignOut();
                }

                

                var email = EmailSignUpLayout?.FindViewById<EditText>(Resource.Id.emailEditText).Text;
                auth.SendSignInLinkToEmail(email, actionCodeSettings)
                    .AddOnSuccessListener(this)
                    .AddOnCompleteListener(this)
                    .AddOnFailureListener(this);
            }
            catch (System.Exception ex)
            {
                Log.Error(Tag, "Error occured", ex);
                Crashlytics.Crashlytics.LogException(Throwable.FromException(ex));
            }
        }

        class EmailSignUpTextWatcher : EmailTextWatcher
        {
            public Button EmailSignUpBtn { get; private set; }
            public EmailSignUpTextWatcher(Button emailSignUpBtn)
            {
                EmailSignUpBtn = emailSignUpBtn ?? throw new ArgumentNullException(nameof(emailSignUpBtn));
            }

            public override void OnTextFilled(bool isValid)
            {
                EmailSignUpBtn.Enabled = isValid;
            }
        }
    }

}