using System;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.OS;
using Android.Support.Design.Button;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Transitions;
using Android.Util;
using Android.Views;
using Android.Widget;
using Cycles;
using Cycles.Droid.Renderers;
using Cycles.Droid.Services;
using Cycles.Droid.Utils;
using Cycles.Views;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using static Android.Support.Design.Widget.AppBarLayout.LayoutParams;
using ActionBar = Android.Support.V7.App.ActionBar;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Application = Xamarin.Forms.Application;
using AView = Android.Views.View;
using Fade = Android.Support.Transitions.Fade;
using ImageButton = Android.Widget.ImageButton;
using ListView = Android.Widget.ListView;
using RelativeLayout = Android.Widget.RelativeLayout;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using TransitionManager = Android.Support.Transitions.TransitionManager;

[assembly: ExportRenderer(typeof(MapPage), typeof(MapPageRenderer))]

namespace Cycles.Droid.Renderers
{
    public sealed class MapPageRenderer : PageRenderer
    {
        public MapPageRenderer(Context context) : base(context)
        {
            MainActivity = Context as MainActivity;
            //MessagingCenter.Subscribe<MainActivity>(this, "Close Scanner",
            //    (sender) => { Application.Current.MainPage = MapPage; });
            FusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(MainActivity);
            AlertBuilder = new AlertDialog.Builder(Context);
            MessagingCenter
                .Subscribe<GraphicBarcodeTracker, string>(this, "Barcode Scanned", (main, s) =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        AlertBuilder.SetTitle("Start Ride");
                        AlertBuilder.SetMessage("To start ride click Unlock. You are on PAYG");
                        AlertBuilder.SetPositiveButton("Unlock", (senderAlert, args) =>
                        {
                            Toast.MakeText(Context, "Bike Unlocking", ToastLength.Short).Show();
                            //NativeController.getReadDataUUID();
                            StartRideHandlerService();
                        });

                        AlertBuilder.SetNegativeButton("Cancel",
                            (senderAlert, args) =>
                            {
                                Toast.MakeText(Context, "Ride Cancelled!", ToastLength.Short).Show();
                            });

                        Dialog dialog = AlertBuilder.Create();
                        dialog.Show();
                    });
                });

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                if (MainActivity?.Window != null)
                {
                    AView decorView = MainActivity.Window.DecorView;
                    decorView.SetFitsSystemWindows(false);

                    MainActivity.Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                }

            //BuildGoogleApiClient();

            _startServiceIntent = new Intent(Context, typeof(RideHandlerService));
            _startServiceIntent.SetAction(Constants.ACTION_START_SERVICE);
            _stopServiceIntent = new Intent(Context, typeof(RideHandlerService));
            _stopServiceIntent.SetAction(Constants.ACTION_STOP_SERVICE);

            #region Initialize _androidCoordinatorLayout with LayoutParams

            AndroidCoordinatorLayout = (CoordinatorLayout)LayoutInflater.FromContext(Context)
                .Inflate(Resource.Layout.MapWithCoordinator, null);
            AndroidCoordinatorLayout.SetBackgroundColor(Color.Transparent.ToAndroid());
            InfoTextView = AndroidCoordinatorLayout.FindViewById<TextView>(Resource.Id.info_textview);

            CommunityEditText = AndroidCoordinatorLayout.FindViewById<EditText>(Resource.Id.current_community);

            ClosestBikeFab =
                AndroidCoordinatorLayout.FindViewById<FloatingActionButton>(Resource.Id.fab_closest_bicycle);

            RefreshMapFab =
                AndroidCoordinatorLayout.FindViewById<FloatingActionButton>(Resource.Id.fab_refresh_map);

            LocateMeFab = AndroidCoordinatorLayout.FindViewById<FloatingActionButton>(Resource.Id.fab_locate_me);

            ScanButton = AndroidCoordinatorLayout.FindViewById<MaterialButton>(Resource.Id.scan_button);
            ScanButton.Click += (sender, args) =>
            {
                //                MessagingCenter.Send(this, "Scanner Opened");
                var bottomSheet = AndroidCoordinatorLayout.FindViewById<LinearLayout>(Resource.Id.bottom_sheet);
                bottomSheet.LayoutParameters.Height = AndroidCoordinatorLayout.Height;
                BottomSheetBehavior.From(bottomSheet).PeekHeight = AndroidCoordinatorLayout.Height;

                IVisualElementRenderer renderer = Platform.CreateRendererWithContext(new CustomBarcodeScanner(), context);
                BottomSheetBehavior.From(bottomSheet).State = BottomSheetBehavior.StateExpanded;
                TransitionManager.BeginDelayedTransition(bottomSheet, new Fade());
                bottomSheet.RemoveView(ScanButton);
                
                bottomSheet.AddView(renderer.View);
            };

            if (!MainActivity.IsLocationAccessGranted || !MainActivity.IsLocationEnabled)
                DisableLocationButtons(ClosestBikeFab, RefreshMapFab, LocateMeFab);

            ClosestBikeFab?.SetVisibility(ViewStates.Gone);
            if (ClosestBikeFab != null) ClosestBikeFab.Click += FindClosestBike_ClickHandler;

            if (LocateMeFab != null) LocateMeFab.Click += LocateMe_ClickHandler;

            if (RefreshMapFab != null) RefreshMapFab.Click += RefreshMap_ClickHandler;

            #endregion

            MainActivity.LocationAccessChanged += MainActivity_LocationAccessChanged;
            MainActivity.LocationSettingsChanged += MainActivityLocationSettingsChanged;

            #region Initialize _androidAppBarLayout with LayoutParams

            //AndroidAppBarLayout = AndroidCoordinatorLayout.FindViewById<AppBarLayout>(Resource.Id.mappage_appbar);
            var toolbar =
                AndroidCoordinatorLayout.FindViewById<Toolbar>(Resource.Id.mappage_toolbar);

            var giftButton = toolbar.FindViewById<ImageButton>(Resource.Id.gift_button);
            giftButton.Click += GiftButton_ClickHandler;

            if (toolbar.LayoutParameters != null)
                ((AppBarLayout.LayoutParams)toolbar.LayoutParameters).ScrollFlags = ScrollFlagEnterAlways;

            MainActivity?.SetSupportActionBar(toolbar);
            ActionBar actionBar = MainActivity?.SupportActionBar;

            if (actionBar != null)
            {
                actionBar.SetDisplayHomeAsUpEnabled(true);
                actionBar.SetDisplayShowTitleEnabled(false);
                actionBar.SetHomeAsUpIndicator(Resource.Drawable.baseline_menu_white_24);
            }

            #endregion

            AddView(AndroidCoordinatorLayout);
        }


        private static void DisableLocationButtons(FloatingActionButton closestBikeFab,
            FloatingActionButton refreshMapFab, FloatingActionButton locateMeFab)
        {
            locateMeFab?.SetVisibility(ViewStates.Gone);
            closestBikeFab?.SetVisibility(ViewStates.Gone);
            if (refreshMapFab?.LayoutParameters == null) return;
            var layoutParameters = (CoordinatorLayout.LayoutParams)refreshMapFab.LayoutParameters;
            layoutParameters.AnchorId = Resource.Id.bottom_sheet;
            layoutParameters.AnchorGravity = (int)(GravityFlags.Top | GravityFlags.End);
        }

        //Handle Location Permission events
        private async void MainActivity_LocationAccessChanged(bool permissionStatus)
        {
            if (MainActivity.IsLocationEnabled && permissionStatus)
                await TurnOnLocationServices(ClosestBikeFab, RefreshMapFab, LocateMeFab);
            else
                TurnOffLocationServices(ClosestBikeFab, RefreshMapFab, LocateMeFab);
        }

        //Handle Location settings On/Off events
        private async void MainActivityLocationSettingsChanged(bool settingStatus)
        {
            if (MainActivity.IsLocationAccessGranted && settingStatus)
                await TurnOnLocationServices(ClosestBikeFab, RefreshMapFab, LocateMeFab);
            else
                TurnOffLocationServices(ClosestBikeFab, RefreshMapFab, LocateMeFab);
        }

        private async Task TurnOnLocationServices(FloatingActionButton closestBikeFab,
            FloatingActionButton refreshMapFab, FloatingActionButton locateMeFab)
        {
            locateMeFab?.SetVisibility(ViewStates.Visible);
            if (refreshMapFab?.LayoutParameters == null) return;
            var layoutParameters = (CoordinatorLayout.LayoutParams)refreshMapFab.LayoutParameters;
            layoutParameters.AnchorId = Resource.Id.fab_locate_me;
            layoutParameters.AnchorGravity = (int)(GravityFlags.Top | GravityFlags.Center);

            if (!_requestingLocationUpdates) await CreateLocationRequest();
            InfoTextView.Text = Resources.GetText(Resource.String.info_getting_your_location);
        }

        private void TurnOffLocationServices(FloatingActionButton closestBikeFab, FloatingActionButton refreshMapFab,
            FloatingActionButton locateMeFab)
        {
            DisableLocationButtons(closestBikeFab, refreshMapFab, locateMeFab);
            InfoTextView.Text = Resources.GetText(Resource.String.info_could_not_find_you);
            CommunityEditText.Text = Resources.GetText(Resource.String.info_no_community_selected);
            if (_requestingLocationUpdates) StopLocationUpdates();
        }


        private async Task CreateLocationRequest()
        {
            _mLocationRequest = new LocationRequest();
            _mLocationRequest.SetInterval(UPDATE_INTERVAL_IN_MILLISECONDS);
            _mLocationRequest.SetFastestInterval(FASTEST_UPDATE_INTERVAL_IN_MILLISECONDS);
            _mLocationRequest.SetPriority(LocationRequest.PriorityHighAccuracy);
            LocationRequestCallback = new LocationCallback();
            LocationRequestCallback.LocationResult += OnLocationRequestResult;
            await FusedLocationProviderClient.RequestLocationUpdatesAsync(_mLocationRequest, LocationRequestCallback);
            _requestingLocationUpdates = true;
        }

        private void StopLocationUpdates()
        {
            if (!_requestingLocationUpdates) return;
            FusedLocationProviderClient.RemoveLocationUpdates(LocationRequestCallback);
            _requestingLocationUpdates = false;
        }

        private void OnLocationRequestResult(object sender, LocationCallbackResultEventArgs e)
        {
            if (e.Result.Locations.Count < 1) return;
            Location accurateLocation = e.Result.Locations.Aggregate((location1, location2) =>
                location1.Accuracy < location2.Accuracy ? location1 : location2);

            if (OldLocation == null) OldLocation = accurateLocation;

            float t = accurateLocation.Time - OldLocation.Time;

            if (accurateLocation.Accuracy < OldLocation.Accuracy || t * 1000 > 300) OldLocation = accurateLocation;

            if (accurateLocation.Accuracy > 30) return;

            OldLocation = accurateLocation;
            var addressResultReceiver = new MapPageRendererAddressResultReceiver(new Handler(), this);
            StartAddressIntentService(addressResultReceiver, accurateLocation);
        }

        private void StartRideHandlerService()
        {
            if (!IsRideServiceStarted)
            {
                Context.StartService(_startServiceIntent);
                if (ServiceConnection == null) ServiceConnection = new RideHandlerServiceConnection(this);

                IsRideServiceStarted = true;
                var serviceToStart = new Intent(Context, typeof(RideHandlerService));
                Context.BindService(serviceToStart, ServiceConnection, Bind.AutoCreate);
            }
            else
            {
                IsRideServiceStarted = false;
                Context.UnbindService(ServiceConnection);
                Context.StopService(_stopServiceIntent);
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

            if (!GoogleApiAvailability.Instance.IsUserResolvableError(queryResult)) return false;
            // Check if there is a way the user can resolve the issue
            var errorString = GoogleApiAvailability.Instance.GetErrorString(queryResult);
            Log.Error("MainActivity", "There is a problem with Google Play Services on this device: {0} - {1}",
                queryResult, errorString);

            // TODO: Alternately, display the error to the user.

            return false;
        }

        private void StartAddressIntentService(IParcelable receiver, IParcelable currentLocation)
        {
            var intent = new Intent(MainActivity, typeof(FetchAddressJobIntentService));
            intent.PutExtra(Constants.RECEIVER, receiver);
            intent.PutExtra(Constants.LOCATION_DATA_EXTRA, currentLocation);
            FetchAddressJobIntentService.EnqueueWork(MainActivity, intent);
        }

        private void RequestCameraPermission()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                MainActivity.RequestPermissions(new[] { Manifest.Permission.Camera }, REQUEST_CAMERA_ID);
        }

        internal void UpdateCommunity(Community community, Location currentLocation)
        {
            try
            {
                var isCommunitySet = community != null;

                if (isCommunitySet && currentLocation != null && !string.IsNullOrWhiteSpace(community.ShortName))
                {
                    InfoTextView.Text = Resources.GetText(Resource.String.info_you_are_in);
                    CommunityEditText.Text = community.ShortName;
                    CommunityEditText.Enabled = true;
                    CommunityEditText.Focusable = false;
                    MoveToLocation(currentLocation);
                }
                else if (isCommunitySet && currentLocation != null && string.IsNullOrWhiteSpace(community.ShortName))
                {
                    InfoTextView.Text = Resources.GetText(Resource.String.info_getting_your_location);
                    CommunityEditText.Text = "None Selected";
                    CommunityEditText.Enabled = false;
                    MoveToLocation(currentLocation);
                }
                else if (!isCommunitySet && currentLocation != null)
                {
                    InfoTextView.Text = Resources.GetText(Resource.String.info_not_in_a_community);
                    CommunityEditText.Text = "Not Available Here";
                    CommunityEditText.Enabled = false;
                    CommunityEditText.Focusable = false;
                    //                    ScanButton.Visibility = ViewStates.Gone;

                    MoveToLocation(currentLocation);
                }
                else
                {
                    InfoTextView.Text = Resources.GetText(Resource.String.info_could_not_find_you);
                    CommunityEditText.Enabled = true;
                    CommunityEditText.Focusable = true;
                    CommunityEditText.FocusableInTouchMode = true;
                    CommunityEditText.Text = "None Selected";
                }
            }
            catch (ObjectDisposedException exception)
            {
                Console.WriteLine(exception);
            }

        }

        #region Feilds

        #region Constants

        private const long UPDATE_INTERVAL_IN_MILLISECONDS = 15000;
        private const long FASTEST_UPDATE_INTERVAL_IN_MILLISECONDS = UPDATE_INTERVAL_IN_MILLISECONDS / 2;
        private const int REQUEST_CAMERA_ID = 10;

        #endregion

        private readonly Intent _startServiceIntent;
        private readonly Intent _stopServiceIntent;

        private LocationRequest _mLocationRequest;

        private bool _requestingLocationUpdates;

        #endregion

        #region Properties

        public static bool IsReload { get; set; }
        private bool IsRideServiceStarted { get; set; }
        private float TotalDistance { get; set; }
        private LocationCallback LocationRequestCallback { get; set; }
        private Location OldLocation { get; set; }
        private AlertDialog.Builder AlertBuilder { get; }
        private RideHandlerServiceConnection ServiceConnection { get; set; }

        private GoogleApiClient MGoogleApiClient { get; set; }
        private FusedLocationProviderClient FusedLocationProviderClient { get; }

        private CoordinatorLayout AndroidCoordinatorLayout { get; set; }
        private ViewGroup MainViewGroup { get; set; }
        public CyclesMapRenderer CyclesMapView { get; private set; }

        private FloatingActionButton LocateMeFab { get; }

        private FloatingActionButton RefreshMapFab { get; }
        private MaterialButton ScanButton { get; set; }
        private FloatingActionButton ClosestBikeFab { get; }
        public TextView InfoTextView { get; }
        public TextView CommunityEditText { get; }
        private MainActivity MainActivity { get; }

        #endregion

        #region Click Handlers

        private static void GiftButton_ClickHandler(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PushAsync(new GiftPopup());
        }

        private async void ScanBarcode_ClickHandler(object sender, EventArgs e)
        {
            if (ContextCompat.CheckSelfPermission(MainActivity, Manifest.Permission.Camera) != (int)Permission.Granted)
            {
                if (ActivityCompat.ShouldShowRequestPermissionRationale(MainActivity, Manifest.Permission.Camera))
                {
                    Snackbar snackbar = Snackbar
                        .Make(AndroidCoordinatorLayout, "Allow access to your phone's camera. Swipe right to dismiss",
                            Snackbar.LengthIndefinite)
                        .SetAction("Allow", v => RequestCameraPermission())
                        .SetActionTextColor(ContextCompat.GetColorStateList(Context,
                            Resource.Color.permission_snackbar_button));
                    snackbar.View.SetBackgroundResource(Resource.Drawable.rounded_bg_r4);

                    int[][] states =
                    {
                        new[] {Android.Resource.Attribute.StateEnabled}
                    };

                    int[] colors =
                    {
                        MainActivity.GetColor(Resource.Color.cyclesBlueLight)
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
                var scanPage = new CustomBarcodeScanner();
                await Application.Current.MainPage.Navigation.PushModalAsync(scanPage);
            }
        }

        private static void RefreshMap_ClickHandler(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        private async void LocateMe_ClickHandler(object sender, EventArgs e)
        {
            if (!MainActivity.IsLocationEnabled || !MainActivity.IsLocationAccessGranted) return;

            Location location = await FusedLocationProviderClient.GetLastLocationAsync();
            MoveToLocation(location);
        }

        public void MoveToLocation(Location location)
        {
            if (location == null) return;

            var latLng = new LatLng(location.Latitude, location.Longitude);
            CameraPosition.Builder builder = CameraPosition.InvokeBuilder();
            builder.Target(latLng);
            builder.Zoom(18);
            CameraPosition cameraPosition = builder.Build();
            CameraUpdate cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);

            CyclesMapView.AnimateCamera(cameraUpdate);
        }

        private void FindClosestBike_ClickHandler(object sender, EventArgs e)
        {
            Toast.MakeText(Context, "You've found a bike?", ToastLength.Long).Show();
        }

        #endregion

        #region Overrides

        protected override async void OnElementChanged(ElementChangedEventArgs<Page> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || Element == null)
            {
                if (Element != null) return;
                MainViewGroup = null;
                AndroidCoordinatorLayout = null;
                StopLocationUpdates();
                return;
            }

            if (e.NewElement == null) return;
            MessagingCenter.Send(this, "Remove Lock-screen");
            if (IsGooglePlayServicesInstalled() && MainActivity.IsLocationAccessGranted &&
                MainActivity.IsLocationEnabled)
                await CreateLocationRequest();
        }

        protected override void Dispose(bool disposing)
        {
            StopLocationUpdates();
            base.Dispose(disposing);
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            base.OnLayout(changed, l, t, r, b);

            if (AndroidCoordinatorLayout == null) return;
            var msw = MeasureSpec.MakeMeasureSpec(r - l, MeasureSpecMode.Exactly);
            var msh = MeasureSpec.MakeMeasureSpec(b - t, MeasureSpecMode.Exactly);
            //
            AndroidCoordinatorLayout?.Measure(msw, msh);
            AndroidCoordinatorLayout?.Layout(0, 0, r - l, b - t);
        }

        public override void AddView(AView child)
        {
            if (child is CoordinatorLayout)
            {
                base.AddView(child);
                return;
            }

            ((ViewGroup)child).LayoutParameters =
                new CoordinatorLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent);
            MainViewGroup = (ViewGroup)child;
            AndroidCoordinatorLayout.FindViewById<RelativeLayout>(Resource.Id.map_holder)
                .AddView(child);

            for (var i = 0; i < MainViewGroup.ChildCount; i++)
            {
                AView foundChild = MainViewGroup.GetChildAt(i);

                if (!(foundChild is CyclesMapRenderer cyclesMapRenderer)) continue;
                CyclesMapView = cyclesMapRenderer;

                CyclesMapView.MapReady += async sender =>
                {
                    if (!MainActivity.IsLocationAccessGranted || !MainActivity.IsLocationEnabled) return;

                    Location lastLocation = await FusedLocationProviderClient.GetLastLocationAsync();

                    if (lastLocation == null) return;
                    var addressResultReceiver = new MapPageRendererAddressResultReceiver(new Handler(), this);
                    StartAddressIntentService(addressResultReceiver, lastLocation);

                    var latLng = new LatLng(lastLocation.Latitude, lastLocation.Longitude);
                    CameraPosition.Builder builder = CameraPosition.InvokeBuilder();
                    builder.Target(latLng);
                    builder.Zoom(18);
                    CameraPosition cameraPosition = builder.Build();
                    CameraUpdate cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);

                    CyclesMapView.AnimateCamera(cameraUpdate);
                };
            }
        }

        #endregion
    }
}