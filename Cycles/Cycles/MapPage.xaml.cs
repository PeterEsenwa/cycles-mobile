using Cycles.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace Cycles
{
    public partial class MapPage : ContentPage
    {
        private ExceptionDispatchInfo capturedException;
        public MapPage()
        {
            try
            {
                InitializeComponent();
                Device.StartTimer(TimeSpan.FromSeconds(.25), () =>
                {
                    if (progressBar.Progress < 1)
                    {
                        Device.BeginInvokeOnMainThread(() => progressBar.ProgressTo(progressBar.Progress + 0.0025, 250, Easing.Linear));
                        return true;
                    }
                    return false;
                });
            }
            catch (Exception ex)
            {
                capturedException = ExceptionDispatchInfo.Capture(ex);
                Console.WriteLine(ex.Message);
                capturedException.Throw();
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
            map.CustomPins = new List<CustomPin> { pin };
            map.Pins.Add(pin);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (capturedException != null)
            {
                bool action = await DisplayAlert("Alert", "You need to allow Location access to the app", "Go", "Close app");
                capturedException.Throw();
            }
            await PrepareMap();

        }
        public bool IsRideOngoing { get; set; } = false;
        public bool IsCalculatingDist { get; set; } = false;
        private async Task PrepareMap()
        {
            try
            {
                GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromMilliseconds(30000));
                Location location = await Geolocation.GetLocationAsync(request);

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
                    bool retry = await DisplayAlert("Network Issue", "We couldn't get your location. Please check your network", "RETRY", "CLOSE APP");
                    if (retry)
                    {
                        await PrepareMap();
                    }
                    else
                    {
                        Application.Current.Quit();
                    }
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                bool action = await DisplayAlert("Not Correct", "You need to allow Location access to the app", "Go", "Close app");
                // Handle not supported on device exception
            }
            catch (PermissionException pEx)
            {
                bool action = await DisplayAlert("Alert", "You need to allow Location access to the app", "Go", "Close app");
                // Handle permission exception
            }
            catch (Exception ex)
            {
                bool action = await DisplayAlert("Unknown", "You need to allow Location access to the app", "Go", "Close app");
                // Unable to get location
            }
        }

        private async void SizedButton_Clicked(object sender, EventArgs e)
        {
            //JsonValue value = JsonValue.Parse(@"{ ""name"":""Prince Charming"", ...");
            //JsonObject result = value as JsonObject;
            Location location = await Geolocation.GetLastKnownLocationAsync();
            double shortestDistance = 0;
            Location neareatPark = new Location();
            foreach (Pin pin in map.Pins)
            {
                Location locationEnd = new Location(pin.Position.Latitude, pin.Position.Longitude);
                double tempDistance = Location.CalculateDistance(location, locationEnd, DistanceUnits.Kilometers);
                if (shortestDistance == 0)
                {
                    neareatPark = locationEnd;
                }
                else
                {
                    neareatPark = (tempDistance < shortestDistance) ? locationEnd : neareatPark;
                    shortestDistance = (tempDistance < shortestDistance) ? tempDistance : shortestDistance;
                }
            }
            map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(neareatPark.Latitude, neareatPark.Longitude),
                                            Distance.FromKilometers(.1)));
        }

        private Location startLocation;
        private Location endLocation;
        private double TotalDistance = 0;
        private Image tempImage = new Image();
        private async void StartRide_Clicked(object sender, EventArgs e)
        {
            //JsonValue value = JsonValue.Parse(@"{ ""name"":""Prince Charming"", ...");
            //JsonObject result = value as JsonObject;
            //double shortestDistance = 0;
            //Location neareatPark = new Location();
            //foreach (Pin pin in map.Pins)
            //{
            //    Location locationEnd = new Location(pin.Position.Latitude, pin.Position.Longitude);
            //    double tempDistance = Location.CalculateDistance(location, locationEnd, DistanceUnits.Kilometers);
            //    if (shortestDistance == 0)
            //    {
            //        neareatPark = locationEnd;
            //    }
            //    else
            //    {
            //        neareatPark = (tempDistance < shortestDistance) ? locationEnd : neareatPark;
            //        shortestDistance = (tempDistance < shortestDistance) ? tempDistance : shortestDistance;
            //    }
            //}
            //map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(neareatPark.Latitude, neareatPark.Longitude),
            //                                Distance.FromKilometers(.1)));

            SizedButton rideBtn = ((SizedButton)sender);
            if (!IsRideOngoing)
            {
                IsRideOngoing = true;
                tempImage.Source = rideBtn.Image;
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
                        rideBtn.Image = (FileImageSource)tempImage.Source;
                        return false;
                    });
                });

                
            }
            else
            {
                IsRideOngoing = false;
            }

        }

        private string GET(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            try
            {
                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.UTF8);
                    return reader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                WebResponse errorResponse = ex.Response;
                using (Stream responseStream = errorResponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, System.Text.Encoding.GetEncoding("utf-8"));
                    String errorText = reader.ReadToEnd();
                    // log errorText
                }
                throw ex;
            }
        }
    }
}
