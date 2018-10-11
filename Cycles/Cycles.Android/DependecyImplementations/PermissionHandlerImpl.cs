using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Cycles.Droid.DependecyImplementations;
using Cycles.Utils;
using Xamarin.Forms;
using Android.Views;
using System;

[assembly: Dependency(typeof(PermissionHandlerImpl))]
namespace Cycles.Droid.DependecyImplementations
{
    internal class PermissionHandlerImpl 
    {
        //private readonly object TAG;
        //private readonly string[] PermissionsLocation =
        //{
        //  Manifest.Permission.AccessCoarseLocation,
        //  Manifest.Permission.AccessFineLocation
        //};
        //private const int RequestLocationId = 0;


        //public async Task<bool> IsGrantedAsync(Plugin.Permissions.Abstractions.Permission permission)
        //{
        //    try
        //    {
        //        PermissionStatus status = await CrossPermissions.Current.CheckPermissionStatusAsync(permission);
        //        if (status != PermissionStatus.Granted)
        //        {
        //            if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(permission))
        //            {
        //                Activity activity = CrossCurrentActivity.Current.Activity;
        //                Android.Views.View layout = activity.FindViewById(Android.Resource.Id.Content);
        //                Snackbar
        //                    .Make(layout, "You'll to give Cycles location access to find a bike", Snackbar.LengthLong)
        //                    .SetAction("OK", v => ActivityCompat.RequestPermissions(activity, PermissionsLocation, RequestLocationId))
        //                    .Show();
        //            }

        //            var results = await CrossPermissions.Current.RequestPermissionsAsync(permission);
        //            //Best practice to always check that the key exists
        //            if (results.ContainsKey(permission))
        //                status = results[permission];

        //            return true;
        //        }

        //        if (status == PermissionStatus.Granted)
        //        {
        //            //var results = await CrossGeolocator.Current.GetPositionAsync(10000);
        //            //LabelGeolocation.Text = "Lat: " + results.Latitude + " Long: " + results.Longitude;
        //            Activity activity = CrossCurrentActivity.Current.Activity;
        //            Android.Views.View layout = activity.FindViewById(Android.Resource.Id.Content);
        //            Snackbar
        //                .Make(layout, "You are good to go", Snackbar.LengthLong)
        //                .Show();

        //            return true;
        //        }
        //        else if (status != PermissionStatus.Unknown)
        //        {
        //            Activity activity = CrossCurrentActivity.Current.Activity;
        //            Android.Views.View layout = activity.FindViewById(Android.Resource.Id.Content);
        //            Snackbar
        //                .Make(layout, "IDK", Snackbar.LengthLong)
        //                .Show();

        //            return false;
        //        }
        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        Activity activity = CrossCurrentActivity.Current.Activity;
        //        Android.Views.View layout = activity.FindViewById(Android.Resource.Id.Content);
        //        Snackbar
        //            .Make(layout, ex.Message, Snackbar.LengthLong)
        //            .Show();
        //        return false;
        //    }
        //}
    }
}