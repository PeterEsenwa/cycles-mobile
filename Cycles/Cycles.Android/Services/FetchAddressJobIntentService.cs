using System;
using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using Cycles.Droid.Utils;
using Java.Lang;
using Exception = System.Exception;

namespace Cycles.Droid.Services
{
    [Service(Exported = false, Permission = "android.permission.BIND_JOB_SERVICE")]
    public class FetchAddressJobIntentService : JobIntentService
    {
        private const int TAG = 87;
        private const string FETCH_ADDRESS_JIS = "FetchAddress_JIS";

        private ResultReceiver _receiver;

        public static void EnqueueWork(Context context, Intent work)
        {
            Class cls = Class.FromType(typeof(FetchAddressJobIntentService));
            try
            {
                EnqueueWork(context, cls, TAG, work);
            }
            catch (Exception ex)
            {
                Console.WriteLine("FETCH_ADDRESS_JIS Exception: {0}", ex.Message);
            }
        }

        protected override void OnHandleWork(Intent intent)
        {
            _receiver = intent.GetParcelableExtra(Constants.RECEIVER) as ResultReceiver;

            if (_receiver == null)
            {
                Log.Wtf(FETCH_ADDRESS_JIS, "No receiver received. There is nowhere to send the results.");
                return;
            }

            var location = (Location) intent.GetParcelableExtra(Constants.LOCATION_DATA_EXTRA);

            string errorMessage;
            if (location == null)
            {
                errorMessage = GetString(Resource.String.info_could_not_find_you);
                Log.Wtf(FETCH_ADDRESS_JIS, errorMessage);
                DeliverResultToReceiver(Result.FirstUser, errorMessage);
                return;
            }

            if (location.Accuracy > 35)
            {
                errorMessage = GetString(Resource.String.info_getting_your_location);
                Log.Wtf(FETCH_ADDRESS_JIS, errorMessage);
                DeliverResultToReceiver(Result.FirstUser + 1, null, location);
                return;
            }

            try
            {
                #region Old Code

                //                var addresses = new List<Address>(geocoder.GetFromLocation(location.Latitude, location.Longitude, 5));
                //                if (addresses.Count == 0)
                //                {
                //                    if (string.IsNullOrEmpty(errorMessage))
                //                    {
                //                        errorMessage = GetString(Resource.String.no_address_found);
                //                        Log.Error(FETCH_ADDRESS_JIS, errorMessage);
                //                    }
                //
                //                    DeliverResultToReceiver(Result.FirstUser, location);
                //                }
                //                else
                //                {
                //                    var addressFragments = new List<string>();
                //                    foreach (Address address in addresses)
                //                    {
                //                        if (address == null) continue;
                //                        for (var i = 0; i <= address.MaxAddressLineIndex; i++)
                //                        {
                //                            addressFragments.Add(address.GetAddressLine(i));
                //                        }
                //                    }
                //
                //                    Log.Info(FETCH_ADDRESS_JIS, GetString(Resource.String.address_found));
                //                    DeliverResultToReceiver(Result.Canceled, location);
                //                }

                #endregion

                Community getCyclesCommunity = Community.IsInCyclesCommunity(location);
                DeliverResultToReceiver(Result.Canceled, getCyclesCommunity, location);
            }

            #region Old Code

            //            catch (IOException ioException)
//            {
//                errorMessage = GetString(Resource.String.info_could_not_find_you);
//                Log.Error(FETCH_ADDRESS_JIS, errorMessage, ioException);
//                DeliverResultToReceiver(Result.FirstUser, errorMessage);
//            }
//            catch (IllegalArgumentException illegalArgumentException)
//            {
//                errorMessage = GetString(Resource.String.info_could_not_find_you);
//                Log.Error(FETCH_ADDRESS_JIS,
//                    $"{errorMessage}. Latitude = {location.Latitude}, Longitude = {location.Longitude}",
//                    illegalArgumentException);
//                DeliverResultToReceiver(Result.FirstUser, errorMessage);
//            }

            #endregion

            catch (Exception e)
            {
                errorMessage = GetString(Resource.String.info_could_not_find_you);
                Log.Error(FETCH_ADDRESS_JIS,
                    $"{errorMessage}. Latitude = {location.Latitude}, Longitude = {location.Longitude}",
                    e);
                DeliverResultToReceiver(Result.FirstUser, errorMessage);
            }
        }

        private void DeliverResultToReceiver(Result resultCode, string message)
        {
            var bundle = new Bundle();
            bundle.PutString(Constants.RESULT_DATA_KEY, message);
            _receiver.Send(resultCode, bundle);
        }

        private void DeliverResultToReceiver(Result resultCode, Community community, Location location)
        {
            var bundle = new Bundle();
            bundle.PutParcelable(Constants.RESULT_DATA_KEY, community);
            bundle.PutParcelable(Constants.RESULT_DATA_KEY2, location);
            _receiver.Send(resultCode, bundle);
        }
    }
}