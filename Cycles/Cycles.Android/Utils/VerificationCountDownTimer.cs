using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using System;

namespace Cycles.Droid.Utils
{
    internal class VerificationCountDownTimer : CountDownTimer
    {
        private readonly int verificationTimeout;
        private readonly int interval;
        private TextView timerTextView;
        public event EventHandler FinishedCount;

        public VerificationCountDownTimer(TextView timerTextView, int verificationTimeout, int interval) : base(verificationTimeout, interval)
        {
            this.timerTextView = timerTextView ?? throw new ArgumentNullException(nameof(timerTextView));
            this.verificationTimeout = verificationTimeout;
            this.interval = interval;
        }

        public override void OnFinish()
        {
            FinishedCount?.Invoke(this, EventArgs.Empty);
        }

        public override void OnTick(long millisUntilFinished)
        {
            string secondsLeft = (millisUntilFinished / 1000).ToString();
            secondsLeft = secondsLeft.Length == 1 ? "0" + secondsLeft : secondsLeft;
            timerTextView.SetText("00:" + secondsLeft, TextView.BufferType.Normal);
        }
    }

}