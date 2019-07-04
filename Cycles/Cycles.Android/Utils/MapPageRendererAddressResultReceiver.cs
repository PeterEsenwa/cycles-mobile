using System;
using Android.Locations;
using Android.OS;
using Cycles.Droid.Renderers;
using Cycles.Droid.Services;

namespace Cycles.Droid.Utils
{
    public class MapPageRendererAddressResultReceiver : ResultReceiver
    {
        private MapPageRenderer MapPage { get; }

        protected override void OnReceiveResult(int resultCode, Bundle resultData)
        {
            try
            {
                if (MapPage == null) return;
                switch (resultCode)
                {
                    case Constants.SUCCESS_RESULT:
                    {
                        var community = resultData.GetParcelable(Constants.RESULT_DATA_KEY) as Community;
                        var currentLocation = resultData.GetParcelable(Constants.RESULT_DATA_KEY2) as Location;
                        MapPage.UpdateCommunity(community, currentLocation);
                        break;
                    }

                    case Constants.FAILURE_RESULT + 1:
                    {
                        var location = resultData.GetParcelable(Constants.RESULT_DATA_KEY) as Location;
                        MapPage.UpdateCommunity(new Community(), location);
                        break;
                    }


                    case Constants.FAILURE_RESULT:
                    {
                        MapPage.UpdateCommunity(null, null);
                        break;
                    }
                }
            }
            catch (ObjectDisposedException objectDisposedException)
            {
                Console.WriteLine(objectDisposedException.Message);
            }
        }

        public MapPageRendererAddressResultReceiver(Handler handler, MapPageRenderer mapPage) : base(handler)
        {
            MapPage = mapPage;
        }
    }
}