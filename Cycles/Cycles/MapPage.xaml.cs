using Cycles.Droid.Renderers;
using Cycles.Utils;
using Cycles.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Cycles.Droid;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using NetworkAccess = Xamarin.Essentials.NetworkAccess;

namespace Cycles
{
    public partial class MapPage : ContentPage
    {
        private static readonly string TAG = typeof(Droid.MainActivity).FullName;

        public MapPage()
        {
            try
            {
                InitializeComponent();
#if __ANDROID__
                NavigationPage.SetHasNavigationBar(this, false);
#endif
#if __IOS__
                StackLayout NavStack = new StackLayout()
                {
                    Children = {
                        new Label() { Text = "Cycles" }
                    }
                };
                NavigationPage.SetTitleView(this, NavStack);
#endif
                Device.StartTimer(TimeSpan.FromSeconds(.5), () =>
                {
                    if (progressBar.Progress < 1)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                            progressBar.ProgressTo(progressBar.Progress + 0.005, 500, Easing.Linear));
                        return true;
                    }

                    return false;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Crashlytics.Crashlytics.LogException(Java.Lang.Throwable.FromException(ex));
            }

            BindingContext = this;

            CustomPin pin = new CustomPin
            {
                Type = PinType.Place,
                PinType = CustomPin.CustomType.Park,
                Position = new Position(6.672219, 3.161639),
                Label = "Cycles Point @Cafe 2",
                Address = "Cafeteria 2, Goodness Rd, Canaan Land, Ota",
                Id = "P2"
            };
            CustomPin pin2 = new CustomPin
            {
                Type = PinType.Place,
                PinType = CustomPin.CustomType.Park,
                Position = new Position(6.67369, 3.15922),
                Label = "Cycles Point @CST",
                Address = "College of Science and Tech, CU, Canaan Land, Ota",
                Id = "P3"
            };
            map.CustomPins = new List<CustomPin> { pin, pin2 };
            map.Pins.Add(pin);
            map.Pins.Add(pin2);

            MessagingCenter.Subscribe<MapPageRenderer>(this, "Scanner Opened", async (mapPage) =>
            {
                var scanPage = new CustomBarcodeScanner();
                await Navigation.PushModalAsync(scanPage);
            });
            MessagingCenter.Subscribe<MainActivity>(this, "Scanner Opened", async (mapPage) =>
            {
                var scanPage = new CustomBarcodeScanner();
                await Navigation.PushModalAsync(scanPage);
            });
            MessagingCenter.Subscribe<MainActivity>(this, "Close Scanner", async (sender) =>
            {
                if (Navigation.NavigationStack.Count > 0)
                {
                    await Navigation.PopModalAsync();
                }
            });
            MessagingCenter.Subscribe<BarcodeScannerRenderer.GraphicBarcodeTracker>(this, "Close Scanner", async (sender) =>
            {
                if (Navigation.ModalStack.Count > 0)
                {
                    await Navigation.PopModalAsync(); 
                }
            });
        }

        protected override async void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            await PrepareMap();
        }

        private bool IsRideOngoing { get; set; }
        private bool IsCalculatingDist { get; set; } = false;
        private Image TempImage { get; set; } = new Image();

        private async Task PrepareMap()
        {
            try
            {
                GeolocationRequest request =
                    new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(45));


                Location location = await Geolocation.GetLocationAsync(request);
                Debug.WriteLine(location?.ToString() ??
                                "no location**********************************************************************************************");

                if (location != null)
                {
                    await progressBar.ProgressTo(1, 250, Easing.CubicInOut);
                    progressBar.IsVisible = false;
                    map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(location.Latitude, location.Longitude),
                        Distance.FromKilometers(.1)));
                    map.IsVisible = true;
                }
                else
                {
                    bool retry = await DisplayAlert("Network Issue",
                        "We couldn't get your location. Please check your network", "RETRY", "USE LAST KNOWN");
                    if (retry)
                    {
                        await PrepareMap();
                    }
                    else
                    {
                        location = await Geolocation.GetLastKnownLocationAsync();
                        if (location != null)
                        {
                            await progressBar.ProgressTo(1, 250, Easing.CubicInOut);
                            progressBar.IsVisible = false;
                            map.MoveToRegion(MapSpan.FromCenterAndRadius(
                                new Position(location.Latitude, location.Longitude),
                                Distance.FromKilometers(.1)));
                            map.IsVisible = true;
                        }
                        else
                        {
                            await PrepareMap();
                        }
                    }
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                bool action = await DisplayAlert("Something went wrong", "You need to allow Location access to the app",
                    "Go", "Close app");
                Crashlytics.Crashlytics.LogException(Java.Lang.Throwable.FromException(fnsEx));
                // Handle not supported on device exception
            }
            catch (PermissionException pEx)
            {
                bool action = await DisplayAlert("Alert", "You need to allow Location access to the app", "Go",
                    "Close app");
                Crashlytics.Crashlytics.LogException(Java.Lang.Throwable.FromException(pEx));
                // Handle permission exception
            }
            catch (Exception ex)
            {
                bool action = await DisplayAlert("Unknown", "You need to allow Location access to the app", "Go",
                    "Close app");
                Crashlytics.Crashlytics.LogException(Java.Lang.Throwable.FromException(ex));
                // Unable to get location
            }
        }

        public async void SizedButton_Clicked(object sender, EventArgs e)
        {
            //JsonValue value = JsonValue.Parse(@"{ ""name"":""Prince Charming"", ...");
            //JsonObject result = value as JsonObject;
            Location startLocation = await Geolocation.GetLastKnownLocationAsync();
            NetworkAccess current = Connectivity.NetworkAccess;
            if (current == NetworkAccess.Internet)
            {
                double shortestDistance = 0;
                Models.Directions directions = new Models.Directions();
                Location neareatPark = new Location();
                foreach (Pin pin in map.Pins)
                {
                    Location endLocation = new Location(pin.Position.Latitude, pin.Position.Longitude);
                    Models.Directions tempDirections = await DirectionsMethods.GetDirectionsInfo(startLocation.Latitude,
                        endLocation.Latitude, startLocation.Longitude, endLocation.Longitude);
                    if (tempDirections != null)
                    {
                        double tempDistance = DirectionsMethods.GetDistance(tempDirections);
                        if (shortestDistance == 0 || shortestDistance > tempDistance)
                        {
                            neareatPark = endLocation;
                            shortestDistance = tempDistance;
                            directions = tempDirections;
                        }
                    }
                }

                //map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(neareatPark.Latitude, neareatPark.Longitude),
                //                                Distance.FromKilometers(.1)));
                //foreach (Models.Route route in directions.Routes)
                //{
                //    foreach (Models.Leg leg in route.legs)
                //    {
                //        map.LoadRoutes(leg.steps);
                //    }
                //}
                foreach (Models.Route route in directions.Routes)
                {
                    map.LoadRoutes(route.overview_polyline);
                }
            }
        }

        public void StartRide_Clicked(object sender, EventArgs e)
        {
            SizedButton rideBtn = ((SizedButton)sender);
            if (!IsRideOngoing)
            {
                IsRideOngoing = true;
                TempImage.Source = rideBtn.Image;
                rideBtn.Image = null;
                rideBtn.Text = "Starting...";
                int seconds = 0;
                Task incrementTask = Task.Run(() =>
                {
                    Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                    {
                        if ((IsRideOngoing && !IsCalculatingDist) || (!IsRideOngoing && IsCalculatingDist))
                        {
                            seconds = seconds + 1;
                            // Ensure that seconds is less than TimeSpan.MaxValue.TotalSeconds to avoid an exception
                            TimeSpan time = TimeSpan.FromSeconds(seconds);

                            //here backslash is must to tell that colon is
                            //not the part of format, it just a character that we want in output
                            string str = time.ToString(@"hh\:mm\:ss");
                            rideBtn.Text = str;
                            return true;
                        }

                        rideBtn.Image = (FileImageSource)TempImage.Source;
                        return false;
                    });
                });
            }
            else
            {
                IsRideOngoing = false;
            }
        }

        //private string GET(string url)
        //{
        //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        //    try
        //    {
        //        WebResponse response = request.GetResponse();
        //        using (Stream responseStream = response.GetResponseStream())
        //        {
        //            StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8);
        //            return reader.ReadToEnd();
        //        }
        //    }
        //    catch (WebException ex)
        //    {
        //        WebResponse errorResponse = ex.Response;
        //        using (Stream responseStream = errorResponse.GetResponseStream())
        //        {
        //            StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.GetEncoding("utf-8"));
        //            String errorText = reader.ReadToEnd();
        //            // log errorText
        //        }
        //        throw ex;
        //    }
        //}
    }
}