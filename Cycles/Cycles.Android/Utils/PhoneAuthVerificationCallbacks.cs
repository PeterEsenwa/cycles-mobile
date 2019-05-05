using Android.App;
using Android.Util;
using Firebase;
using Firebase.Auth;
using System;

namespace Cycles.Droid.Utils
{
    internal class PhoneAuthVerificationCallbacks : PhoneAuthProvider.OnVerificationStateChangedCallbacks
    {
        public event EventHandler CodeSent;
        public string VerificationId { get; private set; }
        public PhoneAuthProvider.ForceResendingToken ResendingToken { get; private set; }
        public event EventHandler VerificationCompleted;

        public override void OnVerificationCompleted(PhoneAuthCredential credential)
        {
            Log.Debug("OnVerificationCompleted", "Worked");
            VerificationCompleted?.Invoke(this, new PhoneAuthCompletedEventArgs(credential));
        }

        public override void OnVerificationFailed(FirebaseException exception)
        {
            Log.Debug("OnVerificationFailed", exception.GetType().ToString());
            Log.Debug("OnVerificationFailed", exception.Class.ToString());
        }

        public override void OnCodeSent(string verificationId, PhoneAuthProvider.ForceResendingToken forceResendingToken)
        {
            base.OnCodeSent(verificationId, forceResendingToken);
            VerificationId = verificationId;
            ResendingToken = forceResendingToken;
            CodeSent?.Invoke(this, EventArgs.Empty);
        }
    }

    class PhoneAuthCompletedEventArgs : EventArgs
    {
        public PhoneAuthCompletedEventArgs(PhoneAuthCredential credential)
        {
            Credential = credential ?? throw new ArgumentNullException(nameof(credential));
        }

        public PhoneAuthCredential Credential { get; }
    }
}