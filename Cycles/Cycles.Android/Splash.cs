using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Auth.Api;
using Android.Gms.Auth.Api.SignIn;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Preferences;
using Android.Support.Constraints;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Util;
using Android.Views;
using Android.Widget;
using Cycles.Droid.CustomViews;
using Cycles.Droid.Utils;
using Firebase;
using Firebase.Auth;

using Java.Util.Concurrent;

using System;
using System.Collections.Generic;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using static Cycles.Droid.Utils.SplashAnimationHelper;
using static Android.Gms.Common.Apis.GoogleApiClient;
using Android.Gms.Tasks;
using Firebase.Analytics;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace Cycles.Droid
{

    [Activity(Label = "Cycles", Icon = "@mipmap/icon", Theme = "@style/Splash",
        MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]

    public class Splash : FragmentActivity, IOnConnectionFailedListener, IFacebookCallback, IOnSuccessListener
    {
        #region CONSTANTS

        private const string TAG = "SplashActivity";
        private const int RC_SIGN_IN = 9001;
        private const int VERIFICATION_TIMEOUT = 30;
        private const string FIRST_RUN = "first_run";
        private const int REQUEST_LOCATION_ID = 0;

        #endregion

        #region Firebase Properties
        private FirebaseApp App { get; set; }
        private FirebaseAuth FirebaseAuth { get; set; }
        #endregion

        #region Google Properties
        private GoogleApiClient MGoogleApiClient { get; set; }
        #endregion

        #region Views and ViewGroups
        public ImageButton NextButton { get; set; }
        private Button SkipButton { get; set; }
        private PhoneVerificationDialog Dialog { get; set; }
        public ImageView FastFitPath { get; private set; }
        public Drawable FastFitInitialDrawable { get; private set; }
        public RelativeLayout SplashBaseLayout { get; private set; }
        #endregion

        private int CurrentPosition { get; set; } = 1;
        private ICallbackManager CallbackManager { get; set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Splash);
            ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(Application.Context);

            //XF.Material.Droid.Material.Init(this, savedInstanceState);
            //Xamarin.Forms.FormsMaterial.Init(this, savedInstanceState);

            Crashlytics.Crashlytics.HandleManagedExceptions();

            AppCenter.Start("4b376e42-98b2-47fd-af73-7a84453954f9", typeof(Analytics), typeof(Crashes));

            App = FirebaseApp.Instance ?? FirebaseApp.InitializeApp(ApplicationContext);
            FirebaseAuth = FirebaseAuth.GetInstance(App);
            FirebaseAnalytics.GetInstance(this);
            //FirebaseDynamicLinks.Instance.GetDynamicLink(Intent).AddOnSuccessListener(this);

            bool firstRun = sharedPreferences.Contains(FIRST_RUN);

            if (Intent.HasExtra("intent_activity") && Intent.GetStringExtra("intent_activity").Equals("LoginActivity"))
            {
                SplashInit();
                DoSplashSwipe(SwipeDirection.Forward);
                DoSplashSwipe(SwipeDirection.Forward);
            }
            else if (FirebaseAuth.CurrentUser != null && firstRun)
            {
                StartActivity(new Intent(this, typeof(MainActivity)));
                Finish();
            }
            else if (!firstRun)
            {
                sharedPreferences.Edit().PutBoolean(FIRST_RUN, false).Apply();
                SplashInit();
            }
            else if (firstRun)
            {
                StartActivity(new Intent(this, typeof(LoginActivity)));
            }
        }

        private void SplashInit()
        {
            NextButton = FindViewById<ImageButton>(Resource.Id.next_button);
            SkipButton = FindViewById<Button>(Resource.Id.skip_button);
            SkipButton.Click += SkipToEnd; ;
            NextButton.Click += NextButton_Click;

            FindViewById<TextView>(Resource.Id.goto_login).Click += Goto_LoginPage;
            ConstraintLayout pagesHolder = FindViewById<ConstraintLayout>(Resource.Id.pages_holder);
            PagesGestureListener gestureListener = new PagesGestureListener(this);
            GestureDetectorCompat gestureDetector = new GestureDetectorCompat(this, gestureListener);

            pagesHolder.SetOnTouchListener(new PagesGestureRecognizer(gestureDetector));
            FindViewById<ImageView>(Resource.Id.number_signup).Click += PhoneImageViewBtn_Click;
            FindViewById<ImageView>(Resource.Id.google_signup).Click += GoogleSignup_Click;
            FindViewById<ImageView>(Resource.Id.facebook_signup).Click += FacebookSignup_Click;
            FindViewById<ImageView>(Resource.Id.email_signup).Click += EmailLayout_Click;
            SplashBaseLayout = FindViewById<RelativeLayout>(Resource.Id.base_layout);

            IndicatorLayout indicators = FindViewById<IndicatorLayout>(Resource.Id.indicatorLayout);
            indicators.NumberOfIndicators = 3;

            FastFitPath = FindViewById<ImageView>(Resource.Id.fast_fit_path_imageview);
            FastFitInitialDrawable = FastFitPath.Drawable;
        }

        #region Click Handlers
        private void Goto_LoginPage(object sender, EventArgs e)
        {
            StartActivity(new Intent(this, typeof(LoginActivity)));
            Finish();
        }

        private void SkipToEnd(object sender, EventArgs e)
        {
            while (CurrentPosition < 3)
            {
                DoSplashSwipe(SwipeDirection.Forward);
            }
        }

        private void FacebookSignup_Click(object sender, EventArgs e)
        {
            CallbackManager = CallbackManagerFactory.Create();

            LoginManager.Instance.RegisterCallback(CallbackManager, this);

            LoginManager.Instance.LogInWithReadPermissions(this, new List<string>() { "public_profile", "user_friends" });

            IsLogin_InProgress(true);
        }

        private void GoogleSignup_Click(object sender, EventArgs e)
        {
            GoogleSignInOptions gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                .RequestIdToken("316655980255-fudv9pg2bcs7s89eevadgo82nrj0t83v.apps.googleusercontent.com")
                    .RequestEmail()
                    .Build();
            if (MGoogleApiClient == null)
            {
                MGoogleApiClient = new Builder(this)
                    .EnableAutoManage(this /* FragmentActivity */, this /* OnConnectionFailedListener */)
                    .AddApi(Auth.GOOGLE_SIGN_IN_API, gso)
                    .Build();
            }
            Intent signInIntent = Auth.GoogleSignInApi.GetSignInIntent(MGoogleApiClient);
            StartActivityForResult(signInIntent, RC_SIGN_IN);

            IsLogin_InProgress(true);
        }

        private void PhoneImageViewBtn_Click(object sender, EventArgs e)
        {
            var phoneNumberDialogHandler = new EventHandler<DialogClickEventArgs>((alertDialog, clicked) =>
            {
                var phoneNumberAlertDialog = alertDialog as AlertDialog;
                Button btnClicked = phoneNumberAlertDialog?.GetButton(clicked.Which);

                if (btnClicked?.Text != GetString(Resource.String.send_code)) return;

                var phoneNumber = phoneNumberAlertDialog?.FindViewById<EditText>(Resource.Id.phoneNumber);
                var mCallBacks = new PhoneAuthVerificationCallbacks();

                mCallBacks.CodeSent += MCallBacks_CodeSent;
                mCallBacks.VerificationCompleted += MCallBacks_VerificationCompleted;
                PhoneAuthProvider.GetInstance(FirebaseAuth).VerifyPhoneNumber(phoneNumber?.Text, VERIFICATION_TIMEOUT, TimeUnit.Seconds, this, mCallBacks);
            });

            AlertDialog phoneNumberAlert = new AlertDialog.Builder(this)
                .SetTitle(GetString(Resource.String.input_phone_number))
                .SetPositiveButton(GetString(Resource.String.send_code), phoneNumberDialogHandler)
                .SetView(LayoutInflater.Inflate(Resource.Layout.PhoneNumberDialog, null))
                .Show();

            EditText phoneNumberInput = phoneNumberAlert.FindViewById<EditText>(Resource.Id.phoneNumber);
            //phoneNumberInput.AddTextChangedListener(new PhoneNumberFormattingTextWatcher("NG"));
            phoneNumberInput.AddTextChangedListener(new PhoneTextWatcher(phoneNumberInput));
        }

        private void EmailLayout_Click(object sender, EventArgs e)
        {
            EmailSignUpFragment emailSignUp = new EmailSignUpFragment();
            emailSignUp.Show(SupportFragmentManager, "Email_Sign_Up_Dialog");
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            DoSplashSwipe(SwipeDirection.Forward);
        }
        #endregion

        #region Phone number Verification Callbacks

        private async void MCallBacks_VerificationCompleted(object sender, EventArgs e)
        {
            IsLogin_InProgress(true);
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }
            else
            {
                PhoneAuthCompletedEventArgs verifiedEvent = (PhoneAuthCompletedEventArgs)e;
                if (Dialog != null)
                {
                    Dialog.Dismiss();
                    Dialog.Dispose();
                }
                IAuthResult signinResult = await FirebaseAuth.SignInWithCredentialAsync(verifiedEvent.Credential);
                if (signinResult != null && signinResult.User != null)
                {
                    FirebaseUser user = signinResult.User;
                    Intent mainactivityIntent = new Intent(this, typeof(MainActivity));
                    StartActivity(mainactivityIntent);
                    IsLogin_InProgress(false);
                    Finish();
                }
                else
                {
                    Toast.MakeText(this, "Authentication failed.", ToastLength.Long).Show();
                    IsLogin_InProgress(false);
                }
            }

        }

        private void MCallBacks_CodeSent(object sender, EventArgs e)
        {
            Dialog = new PhoneVerificationDialog(VERIFICATION_TIMEOUT);

            Android.Support.V4.App.FragmentTransaction ft = SupportFragmentManager.BeginTransaction();

            Dialog.Show(ft, "tatattta");
        }
        #endregion

        public void DoSplashSwipe(SwipeDirection direction)
        {
            int oldPage = CurrentPosition;
            if (CurrentPosition >= 3)
            {
                return;
            }
            int newPage = CurrentPosition += EnumExtension.GetValue(direction);

            //Forward swipe
            if (direction.Equals(SwipeDirection.Forward))
            {
                switch (oldPage)
                {
                    case 1 when newPage == 2:
                    {
                        var vectorDrawable = FastFitPath.Drawable;
                        if (vectorDrawable != FastFitInitialDrawable)
                        {
                            FastFitPath.SetImageDrawable(FastFitInitialDrawable);
                        }
                        (FastFitPath.Drawable as AnimatedVectorDrawable)?.Start();
                        FindViewById<IndicatorLayout>(Resource.Id.indicatorLayout).MoveToPage(newPage, oldPage);
                        break;
                    }

                    case 2 when newPage == 3:
                    {
                        AnimateViewOut(SkipButton);
                        AnimateViewOut(NextButton);
                        AnimateViewOut(FastFitPath);

                        RelativeLayout socialLayout = FindViewById<RelativeLayout>(Resource.Id.social_options);
                        TextView createAccount = FindViewById<TextView>(Resource.Id.call_to_create);

                        var finalInAnimation = new Java.Lang.Runnable(() =>
                        {
                            TextView haveAccount = FindViewById<TextView>(Resource.Id.have_an_account);
                            TextView loginText = FindViewById<TextView>(Resource.Id.goto_login);

                            AnimateViewIn(loginText);
                            AnimateViewIn(haveAccount);

                        });

                        AnimateViewIn(createAccount);
                        AnimateViewIn(socialLayout, finalInAnimation);

                        IndicatorLayout indicators = FindViewById<IndicatorLayout>(Resource.Id.indicatorLayout);
                        indicators.MoveToPage(newPage, oldPage);
                        break;
                    }
                }
            }
            else if (direction.Equals(SwipeDirection.Backward))
            {
                if (oldPage == 2 && newPage == 1)
                {
                    FastFitPath.SetImageDrawable(GetDrawable(Resource.Drawable.fit_fast_animvector));
                    ((AnimatedVectorDrawable)FastFitPath.Drawable)?.Start();
                    FindViewById<IndicatorLayout>(Resource.Id.indicatorLayout).MoveToPage(newPage, oldPage);
                }
            }
        }

        public enum SwipeDirection
        {
            Forward = +1,
            Backward = -1
        }

        public override void OnBackPressed() { }

        public void OnConnectionFailed(ConnectionResult result)
        {
            Snackbar.Make(SplashBaseLayout, Resource.String.connection_failed, Snackbar.LengthLong);
        }

        #region Google Auth with Firebase
        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            Log.Debug(TAG, "onActivityResult:" + requestCode + ":" + resultCode + ":" + data);
            if (CallbackManager != null)
            {
                CallbackManager.OnActivityResult(requestCode, (int)resultCode, data);
            }
            if (requestCode == RC_SIGN_IN)
            {
                IsLogin_InProgress(false);
                GoogleSignInResult result = Auth.GoogleSignInApi.GetSignInResultFromIntent(data);
                if (result.IsSuccess)
                {
                    GoogleSignInAccount userAccount = result.SignInAccount;
                    var success = await FirebaseAuthHelper.FirebaseAuthWithGoogle(FirebaseAuth, userAccount);
                    if (success)
                    {
                        Intent mainactivityIntent = new Intent(this, typeof(MainActivity));
                        StartActivity(mainactivityIntent);
                        Finish();
                    }
                }
            }
        }
        #endregion

        #region Facebook Auth area
        private void IsLogin_InProgress(bool isTrue)
        {
            if (isTrue)
            {
                ProgressBar progressBar = FindViewById<ProgressBar>(Resource.Id.indeterminateBar);
                progressBar.Visibility = ViewStates.Visible;
                //MProgressBar.StartAnimation();
            }
            else
            {
                ProgressBar progressBar = FindViewById<ProgressBar>(Resource.Id.indeterminateBar);
                progressBar.Visibility = ViewStates.Invisible;
            }
        }

        public void OnCancel()
        {
            Toast.MakeText(this, "Facebook verification cancelled", ToastLength.Long).Show();
        }

        public void OnError(FacebookException error)
        {
            var fbSignupErrorDialogHandler = new EventHandler<DialogClickEventArgs>((alertDialog, clicked) =>
            {
                AlertDialog phoneNumberAlertDialog = alertDialog as AlertDialog;
                phoneNumberAlertDialog.Dismiss();
                phoneNumberAlertDialog.Dispose();

            });
            AlertDialog fbSignupErrorAlert = new AlertDialog.Builder(this)
                .SetTitle(GetString(Resource.String.fb_signup_error))
                .SetPositiveButton(GetString(Resource.String.ok_text), fbSignupErrorDialogHandler)
                .SetMessage(error.Message)
                .Show();
        }

        public async void OnSuccess(Java.Lang.Object result)
        {
            _ = Log.Debug(Class.ToString(), result.Class.ToString());

            var success = await FirebaseAuthHelper.FirebaseAuthWithFacebook(FirebaseAuth, ((LoginResult)result).AccessToken);
            if (success)
            {
                IsLogin_InProgress(false);
                Toast.MakeText(this, "Authentication successful", ToastLength.Long).Show();
                Intent mainactivityIntent = new Intent(this, typeof(MainActivity));
                StartActivity(mainactivityIntent);
                Finish();
            }
            else
            {
                Toast.MakeText(this, "Authentication failed.", ToastLength.Long).Show();
                IsLogin_InProgress(false);
            }
        }
        #endregion
    }
}