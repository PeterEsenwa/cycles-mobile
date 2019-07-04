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
using System.IO;
using Android.Content.Res;
using Cycles.Models;
using Cycles.Utils;
using Android.Util;
using Cycles.Droid.Utils;
using GeoJSON.Net;
using Newtonsoft.Json;

[assembly: ExportRenderer(typeof(CyclesMap), typeof(CyclesMapRenderer))]

namespace Cycles.Droid.Renderers
{
    public class CyclesMapRenderer : MapRenderer, GoogleMap.IInfoWindowAdapter
    {
        private List<CustomPin> _customPins;
        public List<LatLng> Lines;
        public GoogleMap _nativeMap;

        public List<Position> RouteCoordinates { get; private set; }
        public event MapReadyEventHandler MapReady;

        public CyclesMapRenderer(Context context) : base(context)
        {
            MainActivity.LocationAccessChanged += LocationAccessChanged;
            MainActivity.LocationSettingsChanged += LocationSettingsChanged;
        }

        protected override void OnElementChanged(Xamarin.Forms.Platform.Android.ElementChangedEventArgs<Map> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                NativeMap.InfoWindowClick -= OnInfoWindowClick;
            }

            if (e.NewElement == null) return;

            var formsMap = (CyclesMap) e.NewElement;
            RouteCoordinates = formsMap.RouteCoordinates;
            Lines = formsMap.Lines;
            formsMap.RoutesListUpdated += FormsMap_RoutesListUpdated;
            _customPins = formsMap.CustomPins;
            Control.GetMapAsync(this);
        }

        //public void LoadRoutes(OverviewPolyline overview_polyline)
        //{
        //    lines = DirectionsMethods.DecodePolyline(overview_polyline.points);
        //    FormsMap_RoutesListUpdated(this, EventArgs.Empty);
        //}

        private void FormsMap_RoutesListUpdated(object sender, EventArgs e)
        {
            if (NativeMap == null) return;
            var polylineOptions = new PolylineOptions()
                .InvokeColor(0x66FF0000)
                .InvokeWidth(6);

            foreach (LatLng line in Lines)
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

//            var latLng = new LatLng(Communities.LagosLatitude, Communities.LagosLongitude);
//            CameraPosition.Builder builder = CameraPosition.InvokeBuilder();
//            builder.Target(latLng);
//            builder.Zoom(15);
//            CameraPosition cameraPosition = builder.Build();
//            CameraUpdate cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);
//            map.MoveCamera(cameraUpdate);

            base.OnMapReady(map);
            MapReady?.Invoke(this);
            LocationAccessChanged(MainActivity.IsLocationAccessGranted);
            LocationSettingsChanged(MainActivity.IsLocationEnabled);
            NativeMap.UiSettings.ZoomControlsEnabled = false;

            var cuPolygonOptions = new PolygonOptions();
            cuPolygonOptions.InvokeStrokeColor(Android.Graphics.Color.Argb(255, 219, 62, 68));
            cuPolygonOptions.InvokeStrokeWidth(15.0f);
            foreach (Community cyclesCommunity in Communities.CyclesCommunities)
            {
                cyclesCommunity.PolygonCoordinates.ForEach(lng => { cuPolygonOptions.Add(lng); });
                NativeMap.AddPolygon(cuPolygonOptions).Tag = cyclesCommunity.ShortName;
            }
        }

        private void LocationAccessChanged(bool value)
        {
            if (NativeMap == null) return;
            if (!MainActivity.IsLocationEnabled) return;
            NativeMap.MyLocationEnabled = value;
            NativeMap.UiSettings.MyLocationButtonEnabled = false;
        }

        private void LocationSettingsChanged(bool value)
        {
            if (NativeMap == null) return;
            if (!MainActivity.IsLocationAccessGranted) return;
            NativeMap.MyLocationEnabled = value;
            NativeMap.UiSettings.MyLocationButtonEnabled = false;
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

        private CustomPin GetCustomPin(Marker annotation)
        {
            var position = new Position(annotation.Position.Latitude, annotation.Position.Longitude);
            foreach (CustomPin pin in _customPins)
            {
                if (pin.Position == position)
                {
                    return pin;
                }
            }

            return null;
        }

        public void AnimateCamera(CameraUpdate cameraUpdate)
        {
            if (cameraUpdate != null)
                NativeMap?.AnimateCamera(cameraUpdate);
        }
    }

    public delegate void MapReadyEventHandler(CyclesMapRenderer sender);
}