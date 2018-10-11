using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace Cycles
{
    public partial class MainPage : ContentPage
    {
        private ExceptionDispatchInfo capturedException;

        public MainPage()
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
            }
        }

        public Timer progressticker { get; private set; }

        protected override async void OnAppearing()
        {

            base.OnAppearing();
            if (capturedException != null)
            {
                bool action = await DisplayAlert("Alert", "You need to allow Location access to the app", "Go", "Close app");
                capturedException.Throw();
            }
            try
            {
                GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromMilliseconds(10000));
                Location location = await Geolocation.GetLocationAsync(request);

                if (location != null)
                {
                    await progressBar.ProgressTo(1, 250, Easing.CubicInOut);
                    progressBar.IsVisible = false;
                    map.MoveToRegion(MapSpan.FromCenterAndRadius(new Position( location.Latitude, location.Longitude),
                                             Distance.FromKilometers(.1)));
                    map.IsVisible = true;
                }
                else
                {
                    bool retry = await DisplayAlert("Network Issue", "We couldn't your location. Please check your network", "Ok", "Retry");
                    if (retry)
                    {
                        location = await Geolocation.GetLastKnownLocationAsync();
                        map.IsVisible = true;
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
