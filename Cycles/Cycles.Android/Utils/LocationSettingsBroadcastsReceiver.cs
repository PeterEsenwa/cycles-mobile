using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Cycles.Droid.Utils
{
    [BroadcastReceiver(Enabled = true, Exported = false)]
    public class LocationSettingsBroadcastsReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.P)
            {
                MainActivity.IsLocationEnabled = LocationManager.FromContext(context).IsLocationEnabled;
            }
            else if (Build.VERSION.SdkInt <= BuildVersionCodes.P)
            {
                var mode = Settings.Secure.GetInt(context.ContentResolver, "location_mode", (int)SecurityLocationMode.Off);
                MainActivity.IsLocationEnabled = mode != (int) SecurityLocationMode.Off;
            }
        }
    }
}