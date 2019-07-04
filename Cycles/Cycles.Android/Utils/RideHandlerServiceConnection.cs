using System;
using Android.Content;
using Android.OS;
using Cycles.Droid.Renderers;
using Cycles.Droid.Services;

namespace Cycles.Droid.Utils
{
    public class RideHandlerServiceConnection : Java.Lang.Object, IServiceConnection, IGetTimestamp
    {
        static readonly string TAG = typeof(RideHandlerServiceConnection).FullName;

        MapPageRenderer Renderer;
        RideHandlerService rideHandler;

        public RideHandlerServiceConnection(MapPageRenderer renderer)
        {
            IsConnected = false;
            Binder = null;
            Renderer = renderer;
        }

        public bool IsConnected { get; private set; }
        public RideHandlerBinder Binder { get; private set; }

        public string GetFormattedTimestamp()
        {
            if (!IsConnected)
            {
                return null;
            }

            return Binder?.GetFormattedTimestamp();
        }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            Binder = service as RideHandlerBinder;
            rideHandler = Binder.service;
            rideHandler.SetActivity(Renderer);
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            throw new NotImplementedException();
        }
    }
}