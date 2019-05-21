using Android.Content;
using Cycles.Droid.Renderers;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Widget;
using Cycles.Views;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.Android;
using Android.Views;
using System;
using Cycles.Models;
using Cycles.Utils;
using Android.Util;

[assembly: ExportRenderer(typeof(CyclesMap), typeof(CyclesMapRenderer))]

namespace Cycles.Droid.Renderers
{
    public class CyclesMapRenderer : MapRenderer, GoogleMap.IInfoWindowAdapter
    {
        List<CustomPin> customPins;
        public List<LatLng> lines;
        public GoogleMap nativeMap;

        public List<Position> routeCoordinates { get; private set; }

        public CyclesMapRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(Xamarin.Forms.Platform.Android.ElementChangedEventArgs<Map> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                NativeMap.InfoWindowClick -= OnInfoWindowClick;
            }

            if (e.NewElement != null)
            {
                CyclesMap formsMap = (CyclesMap) e.NewElement;
                routeCoordinates = formsMap.RouteCoordinates;
                lines = formsMap.Lines;
                formsMap.IsShowingUser = false;
                formsMap.RoutesListUpdated += FormsMap_RoutesListUpdated;
                customPins = formsMap.CustomPins;
                Control.GetMapAsync(this);
            }
        }

        //public void LoadRoutes(OverviewPolyline overview_polyline)
        //{
        //    lines = DirectionsMethods.DecodePolyline(overview_polyline.points);
        //    FormsMap_RoutesListUpdated(this, EventArgs.Empty);
        //}

        private void FormsMap_RoutesListUpdated(object sender, EventArgs e)
        {
            if (NativeMap != null)
            {
                var polylineOptions = new PolylineOptions()
                    .InvokeColor(0x66FF0000)
                    .InvokeWidth(6);

                foreach (LatLng line in lines)
                {
                    polylineOptions.Add(line);
                }

                //googleMap.AddPolyline(polylineOptions);


                //var polylineOptions = new PolylineOptions();
                //polylineOptions.InvokeColor(0x66FF0000);

                //foreach (var position in routeCoordinates)
                //{
                //    polylineOptions.Add(new LatLng(position.Latitude, position.Longitude));
                //}

                NativeMap.AddPolyline(polylineOptions);
            }
        }

        protected override void OnMapReady(GoogleMap map)
        {
            var bottom = (int) TypedValue.ApplyDimension(ComplexUnitType.Dip, 8, Resources.DisplayMetrics);
            var left = (int) TypedValue.ApplyDimension(ComplexUnitType.Dip, 8, Resources.DisplayMetrics);
            var right = (int) TypedValue.ApplyDimension(ComplexUnitType.Dip, 8, Resources.DisplayMetrics);
            var top = (int) TypedValue.ApplyDimension(ComplexUnitType.Dip, 48, Resources.DisplayMetrics);
            
            map.SetPadding(left, top, right, bottom);
            map.SetMapStyle(MapStyleOptions.LoadRawResourceStyle(Context, Resource.Raw.my_map_customization));
            map.InfoWindowClick += OnInfoWindowClick;
            map.SetInfoWindowAdapter(this);

            base.OnMapReady(map);
            nativeMap = NativeMap;
            nativeMap.MyLocationEnabled = false;
            nativeMap.UiSettings.ZoomControlsEnabled = false;
            nativeMap.UiSettings.MyLocationButtonEnabled = false;
        }
        
        protected override MarkerOptions CreateMarker(Pin pin)
        {
            var marker = new MarkerOptions();
            marker.SetPosition(new LatLng(pin.Position.Latitude, pin.Position.Longitude));
            marker.SetTitle(pin.Label);
            marker.SetSnippet(pin.Address);
            if (((CustomPin) pin).PinType == CustomPin.CustomType.Park)
            {
                marker.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.bike_park));
            }

            return marker;
        }

        void OnInfoWindowClick(object sender, GoogleMap.InfoWindowClickEventArgs e)
        {
            var customPin = GetCustomPin(e.Marker);
            if (customPin == null)
            {
                throw new Exception("Custom pin not found");
            }

            //if (!string.IsNullOrWhiteSpace(customPin.Url))
            //{
            //    var url = Android.Net.Uri.Parse(customPin.Url);
            //    var intent = new Intent(Intent.ActionView, url);
            //    intent.AddFlags(ActivityFlags.NewTask);
            //    Android.App.Application.Context.StartActivity(intent);
            //}
        }

        public Android.Views.View GetInfoContents(Marker marker)
        {
            if (Android.App.Application.Context.GetSystemService(Context.LayoutInflaterService) is LayoutInflater
                inflater)
            {
                //Android.Views.View view = new Android.Views.View(this);

                //var customPin = GetCustomPin(marker);
                //if (customPin == null)
                //{
                //    throw new Exception("Custom pin not found");
                //}

                //if (customPin.Id.ToString() == "Xamarin")
                //{
                //    //view = inflater.Inflate(Resource.Layout.XamarinMapInfoWindow, null);
                //}
                //else
                //{
                //    //view = inflater.Inflate(Resource.Layout.MapInfoWindow, null);
                //}

                ////var infoTitle = view.FindViewById<TextView>(Resource.Id.InfoWindowTitle);
                ////var infoSubtitle = view.FindViewById<TextView>(Resource.Id.InfoWindowSubtitle);

                //if (infoTitle != null)
                //{
                //    infoTitle.Text = marker.Title;
                //}
                //if (infoSubtitle != null)
                //{
                //    infoSubtitle.Text = marker.Snippet;
                //}

                //return view;
            }

            return null;
        }

        public Android.Views.View GetInfoWindow(Marker marker)
        {
            return null;
        }

        CustomPin GetCustomPin(Marker annotation)
        {
            var position = new Position(annotation.Position.Latitude, annotation.Position.Longitude);
            foreach (var pin in customPins)
            {
                if (pin.Position == position)
                {
                    return pin;
                }
            }

            return null;
        }
    }
}