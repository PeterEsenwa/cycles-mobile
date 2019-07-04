using Android.OS;
using Cycles.Droid.Services;

namespace Cycles.Droid.Utils
{
    public class RideHandlerBinder : Binder, IGetTimestamp
    {
        public RideHandlerService service;
        public RideHandlerBinder(RideHandlerService service)
        {
            this.service = service;
        }

        public string GetFormattedTimestamp()
        {
            return service?.GetFormattedTimestamp();
        }
    }
}