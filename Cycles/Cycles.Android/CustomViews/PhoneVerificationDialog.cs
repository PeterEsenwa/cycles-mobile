using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Support.V4.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Support.V7.Widget;
using Android.Widget;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Cycles.Droid.Utils;

namespace Cycles.Droid.CustomViews
{
    [Android.App.Activity(Theme = "@style/Splash")]
    class PhoneVerificationDialog : DialogFragment, View.IOnClickListener
    {
        private readonly int verificationTimeout;

        public VerificationCountDownTimer Timer { get; private set; }

        public PhoneVerificationDialog(int verificationTimeout)
        {
            this.verificationTimeout = verificationTimeout * 1000;
        }

        public void OnClick(View v)
        {
            Dismiss();
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetStyle(StyleNormal, Resource.Style.PhoneVerificationDialogStyle);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            View view = inflater.Inflate(Resource.Layout.PhoneVerification, container, true);
            Toolbar toolbar = view.FindViewById<Toolbar>(Resource.Id.toolbar);
            TextView textTimer = view.FindViewById<TextView>(Resource.Id.countdownTimer);
            textTimer.Text = "00:" + verificationTimeout.ToString();
            toolbar.SetNavigationOnClickListener(this);
            Timer = new VerificationCountDownTimer(textTimer, verificationTimeout, 1000);

            return view;
        }

        private void Timer_FinishedCount(object sender, EventArgs e)
        {
            EditText firstInput = View.FindViewById<EditText>(Resource.Id.number_one);
            EditText secondInput = View.FindViewById<EditText>(Resource.Id.number_two);
            EditText thirdInput = View.FindViewById<EditText>(Resource.Id.number_three);
            EditText fourthInput = View.FindViewById<EditText>(Resource.Id.number_four);
            EditText fifthInput = View.FindViewById<EditText>(Resource.Id.number_five);
            EditText sixthInput = View.FindViewById<EditText>(Resource.Id.number_six);
            firstInput.AddTextChangedListener(new VerifyTextWatcher(firstInput, secondInput));
            secondInput.AddTextChangedListener(new VerifyTextWatcher(secondInput, thirdInput));
            thirdInput.AddTextChangedListener(new VerifyTextWatcher(thirdInput, fourthInput));
            fourthInput.AddTextChangedListener(new VerifyTextWatcher(fourthInput, fifthInput));
            fifthInput.AddTextChangedListener(new VerifyTextWatcher(fifthInput, sixthInput));
            firstInput.Enabled = true;
        }

        public override void OnStart()
        {
            base.OnStart();

            if (Dialog != null)
            {
                int width = ViewGroup.LayoutParams.MatchParent;
                int height = ViewGroup.LayoutParams.MatchParent;
                Dialog.Window.SetLayout(width, height);
                Timer.FinishedCount += Timer_FinishedCount;
                Timer.Start();
            }
        }

        public override void Dismiss()
        {
            Timer.Cancel();
            Timer.Dispose();
            base.Dismiss();
        }
    }
}