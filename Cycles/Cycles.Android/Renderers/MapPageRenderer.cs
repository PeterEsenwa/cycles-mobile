using Android;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Cycles.Droid.Renderers;
using Cycles.Droid.Services;
using Rg.Plugins.Popup.Services;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

using static Android.Support.Design.Widget.AppBarLayout.LayoutParams;
using AView = Android.Views.View;
using Button = Android.Widget.Button;
using Color = Xamarin.Forms.Color;
using ImageButton = Android.Widget.ImageButton;

[assembly: ExportRenderer(typeof(Cycles.MapPage), typeof(MapPageRenderer))]

namespace Cycles.Droid.Renderers
{
    public class MapPageRenderer : PageRenderer, GoogleApiClient.IConnectionCallbacks,
        GoogleApiClient.IOnConnectionFailedListener, Android.Gms.Location.ILocationListener
    {
        private const string TAG = "location-settings";
        protected const int RequestCheckSettings = 0x1;

        public static bool IsReload { get; set; }
        private CoordinatorLayout AndroidCoordinatorLayout { get; set; }
        //private AppBarLayout AndroidAppBarLayout { get; set; }

        private ViewGroup MainViewGroup { get; set; }
        private float TotalDist { get; set; }
        private Location OldLocation { get; set; }

        private bool IsStarted { get; set; }
        private readonly Intent startServiceIntent;
        private Intent stopServiceIntent;
        private RideHandlerServiceConnection ServiceConnection { get; set; }

        private GoogleApiClient mGoogleApiClient { get; set; }

        private LocationRequest mLocationRequest;
        //private LocationSettingsRequest mLocationSettingsRequest;

        FusedLocationProviderClient fusedLocationProviderClient;

        private static CyclesMapRenderer MyMapView { get; set; }
        private const long UPDATE_INTERVAL_IN_MILLISECONDS = 10000;
        private const long FASTEST_UPDATE_INTERVAL_IN_MILLISECONDS = UPDATE_INTERVAL_IN_MILLISECONDS / 2;

        //private MapPage MapPage { get; set; }
        public MainActivity mainActivity { get; private set; }
        private const int REQUEST_CAMERA_ID  = 10;

        public MapPageRenderer(Context context) : base(context)
        {
            mainActivity = (Context as MainActivity);

            //MessagingCenter.Subscribe<MainActivity>(this, "Close Scanner",
            //    (sender) => { Application.Current.MainPage = MapPage; });
            fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(mainActivity);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                if (mainActivity?.Window != null)
                {
                    AView decorView = mainActivity.Window.DecorView;
                    decorView.SetFitsSystemWindows(false);

                    mainActivity.Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                }
            }

            //BuildGoogleApiClient();

            startServiceIntent = new Intent(Context, typeof(RideHandlerService));
            startServiceIntent.SetAction(Constants.ACTION_START_SERVICE);
            stopServiceIntent = new Intent(Context, typeof(RideHandlerService));
            stopServiceIntent.SetAction(Constants.ACTION_STOP_SERVICE);

            #region Initialize _androidCoordinatorLayout with LayoutParams

            AndroidCoordinatorLayout = (CoordinatorLayout)LayoutInflater.FromContext(Context)
                .Inflate(Resource.Layout.MapWithCoordinator, null);
            AndroidCoordinatorLayout.SetBackgroundColor(Color.Transparent.ToAndroid());
            var scanBarcode = AndroidCoordinatorLayout.FindViewById<Button>(Resource.Id.scan_barcode);

            var closestBikeFab = AndroidCoordinatorLayout.FindViewById<FloatingActionButton>(Resource.Id.fab_closest_bicycle);
            var locateMeFab = AndroidCoordinatorLayout.FindViewById<FloatingActionButton>(Resource.Id.fab_locate_me);
            var refreshMapFab = AndroidCoordinatorLayout.FindViewById<FloatingActionButton>(Resource.Id.fab_refresh_map);
            if (closestBikeFab != null)
            {
                closestBikeFab.Click += FindClosestBike;
            }

            if (scanBarcode != null)
            {
                scanBarcode.Click += ScanBarcode_Click;
            }

            if (locateMeFab != null)
            {
                locateMeFab.Click += LocateMe;
            }

            if (refreshMapFab != null)
            {
                refreshMapFab.Click += RefreshMap;
            }

            #endregion

            #region Initialize _androidAppBarLayout with LayoutParams

            //AndroidAppBarLayout = AndroidCoordinatorLayout.FindViewById<AppBarLayout>(Resource.Id.mappage_appbar);
            var toolbar = AndroidCoordinatorLayout.FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.mappage_toolbar);

            var giftButton = toolbar.FindViewById<ImageButton>(Resource.Id.gift_button);
            giftButton.Click += GiftButton_Click;

            if (toolbar.LayoutParameters != null)
            {
                ((AppBarLayout.LayoutParams)toolbar.LayoutParameters).ScrollFlags = ScrollFlagEnterAlways;
            }

            mainActivity?.SetSupportActionBar(toolbar);
            ActionBar actionBar = mainActivity?.SupportActionBar;

            if (actionBar != null)
            {
                actionBar.SetDisplayHomeAsUpEnabled(true);
                actionBar.SetDisplayShowTitleEnabled(false);
                actionBar.SetHomeAsUpIndicator(Resource.Drawable.baseline_menu_white_24);
            }

            #endregion

            AddView(AndroidCoordinatorLayout);
        }

        private void GiftButton_Click(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PushAsync(new Views.GiftPopup(), true);
        }

        private void ScanBarcode_Click(object sender, EventArgs e)
        {
            if (ContextCompat.CheckSelfPermission(mainActivity, Manifest.Permission.Camera) != (int)Permission.Granted)
            {
                if (ActivityCompat.ShouldShowRequestPermissionRationale(mainActivity, Manifest.Permission.Camera))
                {
                    AView layout = FindViewById(Android.Resource.Id.Content);
                    Snackbar snackbar = Snackbar
                        .Make(AndroidCoordinatorLayout, "Allow access to your phone's camera. Swipe right to dismiss", Snackbar.LengthIndefinite)
                        .SetAction("Allow", v => RequestCameraPermission())
                        .SetActionTextColor(ContextCompat.GetColorStateList(Context, Resource.Color.permission_snackbar_button));
                    snackbar.View.SetBackgroundResource(Resource.Drawable.rounded_bg_r4);

                    int[][] states = new int[][] {
                        new int[] { Android.Resource.Attribute.StateEnabled}
                    };

                    int[] colors = new int[] {
                        mainActivity.GetColor(Resource.Color.cyclesBlueLight)
                    };

                    snackbar.View.BackgroundTintList = new ColorStateList(states, colors);
                    ((CoordinatorLayout.LayoutParams)snackbar.View.LayoutParameters).SetMargins(16, 32, 16, 32);
                    snackbar.Show();
                }
                else
                {
                    RequestCameraPermission();
                }
            }
            else
            {
                MessagingCenter.Send(this, "Scanner Opened");
            }
        }

        private void RequestCameraPermission()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                mainActivity.RequestPermissions(new string[] { Manifest.Permission.Camera }, REQUEST_CAMERA_ID);
            }
        }

        private void RefreshMap(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        private async void LocateMe(object sender, EventArgs e)
        {
            Location location = await fusedLocationProviderClient.GetLastLocationAsync();
            var latLng = new LatLng(location.Latitude, location.Longitude);
            CameraPosition.Builder builder = CameraPosition.InvokeBuilder();
            builder.Target(latLng);
            builder.Zoom(18);
            builder.Bearing(location.Bearing);

            CameraPosition cameraPosition = builder.Build();

            CameraUpdate cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);

            MyMapView.nativeMap.AnimateCamera(cameraUpdate);
        }

        private void FindClosestBike(object sender, EventArgs e)
        {
            Toast.MakeText(Context, "You've found a bike?", ToastLength.Long).Show();
        }

        private void BuildGoogleApiClient()
        {
            Log.Info(TAG, "Building GoogleApiClient");
            mGoogleApiClient = new GoogleApiClient.Builder(Context)
                .AddConnectionCallbacks(this)
                .AddOnConnectionFailedListener(this)
                .AddApi(LocationServices.API)
                .Build();
        }

        private async Task CreateLocationRequest()
        {
            mLocationRequest = new LocationRequest();
            mLocationRequest.SetInterval(UPDATE_INTERVAL_IN_MILLISECONDS);
            mLocationRequest.SetFastestInterval(FASTEST_UPDATE_INTERVAL_IN_MILLISECONDS);
            mLocationRequest.SetPriority(LocationRequest.PriorityHighAccuracy);
            var callback = new LocationCallback();
            callback.LocationResult += OnLocationResult;
            await fusedLocationProviderClient.RequestLocationUpdatesAsync(mLocationRequest, callback);
        }

        private void OnLocationResult(object sender, LocationCallbackResultEventArgs e)
        {
            if (e.Result.Locations.Count >= 1)
            {
                OldLocation = e.Result.Locations[0];
            }
        }

        private bool IsGooglePlayServicesInstalled()
        {
            var queryResult = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(Context);
            if (queryResult == ConnectionResult.Success)
            {
                Log.Info("MainActivity", "Google Play Services is installed on this device.");
                return true;
            }

            if (GoogleApiAvailability.Instance.IsUserResolvableError(queryResult))
            {
                // Check if there is a way the user can resolve the issue
                var errorString = GoogleApiAvailability.Instance.GetErrorString(queryResult);
                Log.Error("MainActivity", "There is a problem with Google Play Services on this device: {0} - {1}",
                    queryResult, errorString);

                // Alternately, display the error to the user.
            }

            return false;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || Element == null)
            {
                if (Element != null) return;
                MainViewGroup = null;
                AndroidCoordinatorLayout = null;
                //AndroidAppBarLayout = null;

                return;
            }

            if (e.NewElement != null)
            {
                MessagingCenter.Send(this, "Remove Lockscreen");
            }
        }

        private void StartRideHandlerService(object sender, EventArgs e)
        {
            if (!IsStarted)
            {
                Context.StartService(startServiceIntent);
                if (ServiceConnection == null)
                {
                    ServiceConnection = new RideHandlerServiceConnection(this);
                }

                IsStarted = true;
                var serviceToStart = new Intent(Context, typeof(RideHandlerService));
                Context.BindService(serviceToStart, ServiceConnection, Bind.AutoCreate);
            }
            else
            {
                OldLocation = null;
                //locationManager.RemoveUpdates(this);
                IsStarted = false;
                Context.UnbindService(ServiceConnection);
                Context.StopService(stopServiceIntent);
            }
        }

        public override void AddView(AView child)
        {
            child.RemoveFromParent();
            base.AddView(child);
            if (!(child is CoordinatorLayout))
            {
                child.RemoveFromParent();
                ((ViewGroup)child).LayoutParameters =
                    new CoordinatorLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);
                MainViewGroup = (ViewGroup)child;
                AndroidCoordinatorLayout.FindViewById<Android.Widget.RelativeLayout>(Resource.Id.map_holder).AddView(child);
                for (var i = 0; i < MainViewGroup.ChildCount; i++)
                {
                    AView foundChild = MainViewGroup.GetChildAt(i);
                    if (foundChild is CyclesMapRenderer cyclesMapRenderer)
                    {
                        MyMapView = cyclesMapRenderer;
                    }
                    if (IsGooglePlayServicesInstalled())
                    {
                        //await CreateLocationRequest();
                    }
                }

                AndroidCoordinatorLayout.FindViewById(Resource.Id.fabs_holder).BringToFront();
                AndroidCoordinatorLayout.FindViewById(Resource.Id.scan_barcode).BringToFront();
            }
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            base.OnLayout(changed, l, t, r, b);

            if (AndroidCoordinatorLayout != null)
            {
                var msw = MeasureSpec.MakeMeasureSpec(r - l, MeasureSpecMode.Exactly);
                var msh = MeasureSpec.MakeMeasureSpec(b - t, MeasureSpecMode.Exactly);

                AndroidCoordinatorLayout?.Measure(msw, msh);
                AndroidCoordinatorLayout?.Layout(0, 0, r - l, b - t);
            }
        }

        public void OnConnected(Bundle connectionHint)
        {
            Log.Info(TAG, "Connected to GoogleApiClient");
        }

        public void OnConnectionSuspended(int cause)
        {
            Log.Info(TAG, "Connection suspended");
        }

        public void OnConnectionFailed(ConnectionResult result)
        {
            Log.Info(TAG, "Connection failed: ConnectionResult.getErrorCode() = " + result.ErrorCode);
        }

        public void OnLocationChanged(Location location)
        {
            if (OldLocation == null)
            {
                OldLocation = location;
            }
            else
            {
                if (OldLocation.Accuracy < location.Accuracy)
                {
                    TotalDist += OldLocation.DistanceTo(location);
                    OldLocation = location;
                }
            }
        }


    }
}