using Android.App;
using Android.Content;
using Android.Gms.Location;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Cycles.Droid.Renderers;
using System;
using System.Collections.Generic;

namespace Cycles.Droid.Services
{
    [Service(Name = "com.Cycles.RideHandlerService")]
    public class RideHandlerService : Service, IGetTimestamp
    {
        private static readonly string TAG = typeof(RideHandlerService).FullName;
        public const string RIDE_CHANNEL = "com.Kinesys.Cycles.RideNotification";
        private UtcTimestamper timestamper;
        private bool isStarted;
        private Handler handler;
        private Action runnable;
        private Notification notification;
        private RideHandlerBinder Binder;
        private MapPageRenderer mapView;
        //List<Location> RideLocations;
        LocationManager locationManager;

        public override void OnCreate()
        {
            base.OnCreate();
            locationManager = (LocationManager)GetSystemService(LocationService);
            timestamper = new UtcTimestamper();
            handler = new Handler();
            runnable = new Action(() =>
            {
                if (timestamper == null)
                {
                    Log.Wtf(TAG, "Why isn't there a Timestamper initialized?");
                }
                else
                {
                    string msg = timestamper.GetFormattedTimestamp();
                    Log.Debug(TAG, msg);
                    //UpdateNotification();
                    Intent i = new Intent(Constants.NOTIFICATION_BROADCAST_ACTION);
                    i.PutExtra(Constants.BROADCAST_MESSAGE_KEY, msg);
                    Android.Support.V4.Content.LocalBroadcastManager.GetInstance(this).SendBroadcast(i);

                    //if (timestamper.ShouldGetLocation())
                    //{
                    //    //LocationRequest mLocationRequest = new LocationRequest();
                    //    //mLocationRequest.SetInterval(10000);
                    //    //mLocationRequest.SetFastestInterval(5000);
                    //    //mLocationRequest.SetPriority(LocationRequest.PriorityHighAccuracy);
                    //    Location location = mLocationRequest.
                    //}

                    handler.PostDelayed(runnable, Constants.DELAY_BETWEEN_LOG_MESSAGES);
                }
            });
        }

        public override void OnDestroy()
        {
            Binder = null;
            // Stop the handler.
            handler.RemoveCallbacks(runnable);

            // Remove the notification from the status bar.
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.Cancel(Constants.SERVICE_RUNNING_NOTIFICATION_ID);

            timestamper = null;
            isStarted = false;

            base.OnDestroy();
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            if (intent.Action.Equals(Constants.ACTION_START_SERVICE))
            {
                if (!isStarted)
                {
                    RegisterForegroundService();
                    handler.PostDelayed(runnable, Constants.DELAY_BETWEEN_LOG_MESSAGES);
                    isStarted = true;
                }
            }
            else if (intent.Action.Equals(Constants.ACTION_STOP_SERVICE))
            {
                Log.Info(TAG, "OnStartCommand: The service is stopping.");
                timestamper = null;
                StopForeground(true);
                StopSelf();
                isStarted = false;
                this.OnDestroy();

            }
            else if (intent.Action.Equals(Constants.ACTION_RESTART_TIMER))
            {
                Log.Info(TAG, "OnStartCommand: Restarting the timer.");
                timestamper.Restart();

            }

            return StartCommandResult.StickyCompatibility;
        }
        private void UpdateNotification()
        {
            string chanName = GetString(Resource.String.noti_chan_ride);
            var importance = NotificationImportance.High;
            NotificationChannel chan = new NotificationChannel(RIDE_CHANNEL, chanName, importance);
            chan.EnableVibration(true);
            chan.LockscreenVisibility = NotificationVisibility.Public;
            NotificationManager notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(chan);

            notification = notification = new Notification.Builder(this, RIDE_CHANNEL)
                .SetOnlyAlertOnce(true)
                .SetSmallIcon(Resource.Drawable.find_bike)
                .SetContentTitle("Ride ongoing")
                .SetContentIntent(BuildIntentToShowMainActivity())
                //.SetContentText(GetFormattedTimestamp())
                .SetContentText("Remember to lock the bike when you are done")
                .SetOngoing(true)
                .SetAutoCancel(false)
                .Build();

            NotificationManager mNotificationManager = (NotificationManager)GetSystemService(Context.NotificationService);
            mNotificationManager.Notify(Constants.SERVICE_RUNNING_NOTIFICATION_ID, notification);
        }
        private void RegisterForegroundService()
        {
            //var notification = new Notification.Builder(this)
            //    .SetContentTitle(Resources.GetString(Resource.String.app_name))
            //    .SetContentText(Resources.GetString(Resource.String.notification_text))
            //    .SetSmallIcon(Resource.Drawable.ic_stat_name)
            //    .SetContentIntent(BuildIntentToShowMainActivity())
            //    .SetOngoing(true)
            //    .Build();

            string chanName = GetString(Resource.String.noti_chan_ride);
            var importance = NotificationImportance.High;
            NotificationChannel chan = new NotificationChannel(RIDE_CHANNEL, chanName, importance);
            chan.EnableVibration(true);
            chan.LockscreenVisibility = NotificationVisibility.Public;
            NotificationManager notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(chan);

            notification = notification = new Notification.Builder(this, RIDE_CHANNEL)
                .SetOnlyAlertOnce(true)
                .SetSmallIcon(Resource.Drawable.find_bike)
                .SetContentTitle("Ride ongoing")
                .SetContentIntent(BuildIntentToShowMainActivity())
                //.SetContentText(GetFormattedTimestamp())
                .SetContentText("Remember to lock the bike when you are done")
                .SetOngoing(true)
                .SetAutoCancel(false)
                .Build();


            NotificationManager mNotificationManager = (NotificationManager)GetSystemService(Context.NotificationService);
            mNotificationManager.Notify(Constants.SERVICE_RUNNING_NOTIFICATION_ID, notification);


            // Enlist this instance of the service as a foreground service
            StartForeground(Constants.SERVICE_RUNNING_NOTIFICATION_ID, notification);
        }

        public override IBinder OnBind(Intent intent)
        {
            this.Binder = new RideHandlerBinder(this);
            return this.Binder;
        }

        public override bool OnUnbind(Intent intent)
        {
            return base.OnUnbind(intent);
        }

        public string GetFormattedTimestamp()
        {
            return timestamper?.GetFormattedTimestamp();
        }

        private PendingIntent BuildIntentToShowMainActivity()
        {
            var notificationIntent = new Intent(this, typeof(MainActivity));
            notificationIntent.SetAction(Constants.ACTION_MAIN_ACTIVITY);
            notificationIntent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTask);
            notificationIntent.PutExtra(Constants.SERVICE_STARTED_KEY, true);

            var pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, PendingIntentFlags.UpdateCurrent);
            return pendingIntent;
        }

        public void SetActivity(MapPageRenderer renderer)
        {
            this.mapView = renderer;
        }
    }

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