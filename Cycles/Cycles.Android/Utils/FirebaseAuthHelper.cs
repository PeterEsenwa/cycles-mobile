using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Auth.Api.SignIn;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Firebase;
using Firebase.Auth;
using Xamarin.Essentials;
using Xamarin.Facebook;

namespace Cycles.Droid.Utils
{
    class FirebaseAuthHelper
    {
        public static async Task<bool> FirebaseAuthWithFacebook(FirebaseAuth firebaseAuth, AccessToken fbUserToken)
        {
            try
            {
                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    using (AuthCredential credential = FacebookAuthProvider.GetCredential(fbUserToken.Token))
                    {
                        using (IAuthResult result = await firebaseAuth.SignInWithCredentialAsync(credential))
                        {
                            return result != null && result.User != null;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Crashlytics.Crashlytics.LogException(Java.Lang.Throwable.FromException(e));
            }
            return false;
        }

        public static async Task<bool> FirebaseAuthWithGoogle(FirebaseAuth firebaseAuth, GoogleSignInAccount acct)
        {
            try
            {
                if (Connectivity.NetworkAccess == NetworkAccess.Internet)
                {
                    AuthCredential credential = GoogleAuthProvider.GetCredential(acct.IdToken, null);
                    IAuthResult result = await firebaseAuth.SignInWithCredentialAsync(credential);
                    if (result != null && result.User != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (FirebaseNetworkException)
            {
                throw;
            }
            return false;
        }

    }
}