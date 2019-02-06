using Android.Content;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Graphics.Drawables;
using Android.Locations;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Cycles.Droid.Renderers;
using Cycles.Droid.Services;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Maps.Android;
using Xamarin.Forms.Platform.Android;
using static Cycles.Droid.MainActivity;
using AButton = Android.Widget.Button;
using AView = Android.Views.View;

[assembly: ExportRenderer(typeof(Cycles.MapPage), typeof(MapPageRenderer))]
namespace Cycles.Droid.Renderers
{
    public class MapPageRenderer : PageRenderer, GoogleApiClient.IConnectionCallbacks,
        GoogleApiClient.IOnConnectionFailedListener, Android.Gms.Location.ILocationListener
    {
        protected const string TAG = "location-settings";
        protected const int REQUEST_CHECK_SETTINGS = 0x1;

        private CoordinatorLayout _androidCoordinatorLayout;
        private AppBarLayout _androidAppBarLayout;
        private int bottomSheetHeight;
        private LinearLayout _androidLinearLayout;
        private AButton StartRideBtn;
        private AButton FindClosestBike;
        private ViewGroup viewGroup;
        private MapRenderer mapView;

        private TextView DistanceCalcTextView;
        private float totatlDist;
        private Location oldLocation;

        private bool isStarted = false;
        private Intent startServiceIntent;
        private Intent stopServiceIntent;
        private RideHandlerServiceConnection serviceConnection;
        //LocationManager locationManager;
        private int peekHeight;
        private GoogleApiClient mGoogleApiClient;
        private LocationRequest mLocationRequest;
        private LocationSettingsRequest mLocationSettingsRequest;

        public const long UPDATE_INTERVAL_IN_MILLISECONDS = 10000;
        public const long FASTEST_UPDATE_INTERVAL_IN_MILLISECONDS = UPDATE_INTERVAL_IN_MILLISECONDS / 2;


        //public void SendCustomClicked()
        //{
        //    EventHandler eventHandler = this.FoundClosestBike;
        //    eventHandler?.Invoke((object)this, EventArgs.Empty);
        //}

        public MapPageRenderer(Context context) : base(context)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                AView decorView = window.DecorView;
                decorView.SetFitsSystemWindows(false);

                var uiOptions = (int)decorView.SystemUiVisibility;
                var newUiOptions = uiOptions;

                //window.AddFlags(WindowManagerFlags.TranslucentStatus);
                window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                //window.SetStatusBarColor(Color.FromHex("#99d7282f").ToAndroid());
                newUiOptions |= (int)SystemUiFlags.LayoutFullscreen;
                newUiOptions |= (int)SystemUiFlags.LightNavigationBar;
                newUiOptions |= (int)SystemUiFlags.LayoutStable;

                //decorView.SystemUiVisibility = (StatusBarVisibility)newUiOptions;
            }
            //newUiOptions |= (int)SystemUiFlags.Immersive;


            BuildGoogleApiClient();
            CreateLocationRequest();
            BuildLocationSettingsRequest();

            startServiceIntent = new Intent(Context, typeof(RideHandlerService));
            startServiceIntent.SetAction(Constants.ACTION_START_SERVICE);
            stopServiceIntent = new Intent(Context, typeof(RideHandlerService));
            stopServiceIntent.SetAction(Constants.ACTION_STOP_SERVICE);

            Color findBikeColor = (Color)Application.Current.Resources["ButtonBGColorNormalRed"];
            Color NearBlackTxtColor = (Color)Application.Current.Resources["TextColorNearBlack"];

            #region Initialize _androidCoordinatorLayout with LayoutParams

            _androidCoordinatorLayout = new CoordinatorLayout(Context)
            {
                LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent)
            };
            _androidCoordinatorLayout.SetBackgroundColor(Color.Transparent.ToAndroid());

            #endregion

            #region Initialize _androidAppBarLayout with LayoutParams

            _androidAppBarLayout = new AppBarLayout(Context);
            _androidCoordinatorLayout.SetBackgroundColor(Android.Graphics.Color.ParseColor("#FFFFFF"));

            AppBarLayout.LayoutParams layoutParams1 = new AppBarLayout.LayoutParams(LayoutParams.MatchParent, (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 48, Resources.DisplayMetrics));
            layoutParams1.ScrollFlags = AppBarLayout.LayoutParams.ScrollFlagEnterAlways;

            Android.Support.V7.Widget.Toolbar toolbar = new Android.Support.V7.Widget.Toolbar(Context)
            {
                LayoutParameters = layoutParams1
            };

            //AppBarLayout.LayoutParams @params = (AppBarLayout.LayoutParams)toolbar.LayoutParameters;
            //@params.ScrollFlags = AppBarLayout.LayoutParams.ScrollFlagEnterAlways;
            //toolbar.LayoutParameters = @params;

            Android.Support.V7.Widget.Toolbar.LayoutParams textviewLayoutParams = new Android.Support.V7.Widget.Toolbar.LayoutParams(LayoutParams.WrapContent, LayoutParams.WrapContent)
            {
                Gravity = (int)GravityFlags.CenterHorizontal
            };
            TextView titleTextView = new TextView(Context)
            {
                Text = "Cycles",
                LayoutParameters = textviewLayoutParams,
                TextSize = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 10, base.Resources.DisplayMetrics)
            };

            titleTextView.SetTextColor(Android.Graphics.Color.White);

            toolbar.AddView(titleTextView);
            toolbar.SetBackgroundColor(Android.Graphics.Color.ParseColor("#AC2026"));
            //toolbar.SetTitleTextColor(Android.Graphics.Color.ParseColor("#FFFFFF"));

            activity.SetSupportActionBar(toolbar);


            ActionBar actionBar = activity.SupportActionBar;

            actionBar.SetDisplayHomeAsUpEnabled(true);
            actionBar.SetDisplayShowTitleEnabled(false);

            actionBar.SetHomeAsUpIndicator(Resource.Drawable.baseline_menu_white_24);
            _androidAppBarLayout.AddView(toolbar);

            #endregion
            _androidAppBarLayout.LayoutParameters = new AppBarLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent);
            _androidAppBarLayout.Elevation = 9;
            _androidCoordinatorLayout.AddView(_androidAppBarLayout);

            #region Initialize _androidLinearLayout (for use as BottomSheet)

            bottomSheetHeight = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 250, Resources.DisplayMetrics);
            _androidLinearLayout = new LinearLayout(Context)
            {
                Orientation = Orientation.Vertical,
                LayoutParameters = new LayoutParams(LayoutParams.MatchParent, bottomSheetHeight),
                Elevation = 10
            };

            _androidLinearLayout.SetPadding(12, 0, 12, 0);
            _androidLinearLayout.SetBackgroundResource(Resource.Drawable.coordinator_background);

            #endregion

            #region Create (topContainer) : LinearLayout to hold top content of the BottomSheet

            LinearLayout topContainer = new LinearLayout(Context)
            {
                Orientation = Orientation.Horizontal,
                LayoutParameters = new LayoutParams(LayoutParams.MatchParent, (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 80, Resources.DisplayMetrics)),
            };

            topContainer.SetGravity(GravityFlags.Center);
            topContainer.SetBackgroundColor(Color.Transparent.ToAndroid());

            #endregion

            #region Create (descriptionHolder) : LinearLayout to hold descriptive text about current location/area

            LinearLayout descriptionHolder = new LinearLayout(Context) { Orientation = Orientation.Vertical };

            LinearLayout.LayoutParams layoutParams = new LinearLayout.LayoutParams(LayoutParams.WrapContent, LayoutParams.WrapContent)
            {
                RightMargin = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 32, base.Resources.DisplayMetrics)
            };
            descriptionHolder.LayoutParameters = layoutParams;
            descriptionHolder.SetBackgroundColor(Color.Transparent.ToAndroid());

            #endregion

            #region Create TextViews for area info

            TextView communityTxt = new TextView(Context) { Text = "Community:", TextSize = 18, Gravity = GravityFlags.Top };
            TextView placeTxt = new TextView(Context) { Text = "Covenant University" };

            communityTxt.SetIncludeFontPadding(false);
            communityTxt.SetTextColor(NearBlackTxtColor.ToAndroid());
            placeTxt.SetTextColor(NearBlackTxtColor.ToAndroid());

            #endregion

            #region Initialize StartRideBtn

            StartRideBtn = new AButton(Context)
            {
                Text = "Start Ride"
            };

            #endregion

            #region Initialize FindClosestBike Btn

            FindClosestBike = new AButton(Context)
            {
                Text = "Closest Bike",
                Elevation = 8,
                TextSize = 14,
            };
            Drawable leftIconClosestBike = Context.GetDrawable(Resource.Drawable.find_bike_white);
            FindClosestBike.CompoundDrawablePadding = 8;
            FindClosestBike.SetCompoundDrawablesWithIntrinsicBounds(leftIconClosestBike, null, null, null);
            FindClosestBike.Background.SetColorFilter(findBikeColor.ToAndroid(), Android.Graphics.PorterDuff.Mode.Multiply);
            FindClosestBike.SetTextColor(Android.Graphics.Color.White);

            #endregion

            DistanceCalcTextView = new TextView(Context) { Text = "Distance shows here" };

            //activity.OnOptionsItemSelected = 

            descriptionHolder.AddView(communityTxt);
            descriptionHolder.AddView(placeTxt);

            topContainer.AddView(descriptionHolder);
            topContainer.AddView(FindClosestBike);
            _androidLinearLayout.AddView(topContainer);
            _androidLinearLayout.AddView(StartRideBtn);
            _androidLinearLayout.AddView(DistanceCalcTextView);


            //LinearLayout.AddView(StartRideBtn);
            AddView(_androidCoordinatorLayout);

        }



        protected void BuildGoogleApiClient()
        {
            Log.Info(TAG, "Building GoogleApiClient");
            mGoogleApiClient = new GoogleApiClient.Builder(Context)
                .AddConnectionCallbacks(this)
                .AddOnConnectionFailedListener(this)
                .AddApi(LocationServices.API)
                .Build();
        }

        protected void CreateLocationRequest()
        {
            mLocationRequest = new LocationRequest();
            mLocationRequest.SetInterval(UPDATE_INTERVAL_IN_MILLISECONDS);
            mLocationRequest.SetFastestInterval(FASTEST_UPDATE_INTERVAL_IN_MILLISECONDS);
            mLocationRequest.SetPriority(LocationRequest.PriorityHighAccuracy);
        }

        protected void BuildLocationSettingsRequest()
        {
            LocationSettingsRequest.Builder builder = new LocationSettingsRequest.Builder();
            builder.AddLocationRequest(mLocationRequest);
            mLocationSettingsRequest = builder.Build();
        }

        protected async Task StartLocationUpdates()
        {
            await LocationServices.FusedLocationApi.RequestLocationUpdates(
                mGoogleApiClient,
                mLocationRequest,
                this
            );
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || Element == null)
            {
                if (Element == null)
                {
                    viewGroup = null;
                    _androidLinearLayout = null;
                    _androidCoordinatorLayout = null;
                    _androidAppBarLayout = null;
                }

                return;
            }

            StartRideBtn.Click += StartRideHandlerService;
            FindClosestBike.Click += ((MapPage)Element).SizedButton_Clicked;
        }

        private void StartRideHandlerService(object sender, EventArgs e)
        {

            if (!isStarted)
            {
                StartRideBtn.Text = "Stop Ride";
                Context.StartService(startServiceIntent);
                if (serviceConnection == null)
                {
                    serviceConnection = new RideHandlerServiceConnection(this);
                }
                isStarted = true;
                Intent serviceToStart = new Intent(Context, typeof(RideHandlerService));
                Context.BindService(serviceToStart, serviceConnection, Bind.AutoCreate);
            }
            else
            {
                oldLocation = null;
                //locationManager.RemoveUpdates(this);
                isStarted = false;
                Context.UnbindService(serviceConnection);
                Context.StopService(stopServiceIntent);
                StartRideBtn.Text = "Start Ride";
            }

        }

        private void InitView()
        {
            _androidCoordinatorLayout.AddView(_androidLinearLayout);
            CoordinatorLayout.LayoutParams parameters = (CoordinatorLayout.LayoutParams)_androidLinearLayout.LayoutParameters;

            peekHeight = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 80, Resources.DisplayMetrics);

            BottomSheetBehavior bottomSheetBehavior = new BottomSheetBehavior
            {
                PeekHeight = 0,
                Hideable = false,
                State = BottomSheetBehavior.StateHidden
            };

            int appbarHeight = _androidCoordinatorLayout.Height - peekHeight;

            parameters.Behavior = bottomSheetBehavior;
            _androidLinearLayout.LayoutParameters = parameters;
        }

        public override void AddView(AView child)
        {
            child.RemoveFromParent();
            base.AddView(child);
            if (!(child is CoordinatorLayout))
            {
                child.RemoveFromParent();

                CoordinatorLayout.LayoutParams layoutParams = new CoordinatorLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);
                //layoutParams.ScrollFlags = AppBarLayout.LayoutParams.ScrollFlagEnterAlways;

                ((ViewGroup)child).LayoutParameters = layoutParams;
                viewGroup = (ViewGroup)child;
                for (int i = 0; i < viewGroup.ChildCount; i++)
                {
                    AView foundChild = viewGroup.GetChildAt(i);
                    if (foundChild is MapRenderer)
                    {
                        mapView = (MapRenderer)foundChild;
                    }
                }
                //AppBarLayout.LayoutParams @params = (AppBarLayout.LayoutParams)viewGroup.LayoutParameters;
                //@params.ScrollFlags = AppBarLayout.LayoutParams.ScrollFlagEnterAlways;
                //viewGroup.LayoutParameters = @params;

                _androidCoordinatorLayout.AddView(viewGroup);
                InitView();
            }
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            base.OnLayout(changed, l, t, r, b);

            if (_androidCoordinatorLayout != null)
            {
                var msw = MeasureSpec.MakeMeasureSpec(r - l, MeasureSpecMode.Exactly);
                var msh = MeasureSpec.MakeMeasureSpec(b - t, MeasureSpecMode.Exactly);

                _androidCoordinatorLayout.Measure(msw, msh);
                _androidCoordinatorLayout.Layout(0, 0, r - l, b - t);

                int CoordinatorHeightDiff = _androidCoordinatorLayout.Height - peekHeight;
                if (_androidAppBarLayout.Parent == null)
                {
                    //_androidAppBarLayout.LayoutParameters = new AppBarLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);
                    //_androidCoordinatorLayout.AddView(_androidAppBarLayout);
                }
                //if (mapView.Height > CoordinatorHeightDiff || mapView.Height <= 0)
                //{
                //    mapView.LayoutParameters.Height = CoordinatorHeightDiff;
                //    mapView.Layout(0, 0, r - l, CoordinatorHeightDiff - t);
                //}

                //viewGroup.Layout(0, 0, r - l, CoordinatorHeightDiff - t);
            }
        }

        public void OnConnected(Bundle connectionHint)
        {
            Log.Info(TAG, "Connected to GoogleApiClient");
            DistanceCalcTextView.Text = "Connected to GoogleApiClient";
        }

        public void OnConnectionSuspended(int cause)
        {
            Log.Info(TAG, "Connection suspended");
            DistanceCalcTextView.Text = "Connect suspended";
        }

        public void OnConnectionFailed(ConnectionResult result)
        {
            Log.Info(TAG, "Connection failed: ConnectionResult.getErrorCode() = " + result.ErrorCode);
            DistanceCalcTextView.Text = "Connection failed: ConnectionResult.getErrorCode() = " + result.ErrorCode;
        }

        public void OnLocationChanged(Location location)
        {
            if (oldLocation == null)
            {
                oldLocation = location;
                DistanceCalcTextView.Text = "Distance shows here";
            }
            else
            {
                if (oldLocation.Accuracy < location.Accuracy)
                {
                    totatlDist = totatlDist + oldLocation.DistanceTo(location);
                    oldLocation = location;
                    DistanceCalcTextView.Text = "Distance traveled: " + Math.Floor(totatlDist);
                }
            }

        }
    }

}