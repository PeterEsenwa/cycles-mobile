using Cycles.Views;
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
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
                        Device.BeginInvokeOnMainThread(() => progressBar.ProgressTo(progressBar.Progress + 0.025, 250, Easing.Linear));
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

        private async System.Threading.Tasks.Task PrepareMap()
        {
            try
            {
                GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromMilliseconds(10000));
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
                    bool retry = await DisplayAlert("Network Issue", "We couldn't get your location. Please check your network", "Ok", "Retry");
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
    }
}
