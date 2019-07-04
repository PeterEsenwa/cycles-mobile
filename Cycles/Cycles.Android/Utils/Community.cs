using System.Collections.Generic;
using System.Linq;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Java.Interop;
using Java.Lang;
using static Cycles.Droid.Utils.Communities;

namespace Cycles.Droid.Utils
{
    public class Community : Object, IParcelable, IParcelableCreator
    {
        private Community(Parcel parcel)
        {
            Address = string.Empty;
            ShortName = string.Empty;
            PolygonCoordinates = new List<LatLng>();
            Address = parcel.ReadString();
            ShortName = parcel.ReadString();
            Latitude = parcel.ReadDouble();
            Longitude = parcel.ReadDouble();
            PolygonCoordinates = new List<LatLng>(1000);
            parcel.ReadList(PolygonCoordinates, Class.FromType(typeof(LatLng)).ClassLoader);
        }

        public string Address { get; internal set; }
        public string ShortName { get; internal set; }
        public double Latitude { get; internal set; }
        public double Longitude { get; internal set; }
        public List<LatLng> PolygonCoordinates { get; internal set; }

        public Community()
        {
            Address = string.Empty;
            ShortName = string.Empty;
            PolygonCoordinates = new List<LatLng>();
        }


        #region Parcelable Implementation

        public int DescribeContents()
        {
            return 0;
        }

        public void WriteToParcel(Parcel dest, [GeneratedEnum] ParcelableWriteFlags flags)
        {
            dest.WriteString(Address);
            dest.WriteString(ShortName);
            dest.WriteDouble(Latitude);
            dest.WriteDouble(Longitude);
            dest.WriteList(PolygonCoordinates);
        }

        public Object CreateFromParcel(Parcel source)
        {
            return new Community(source);
        }

        public Object[] NewArray(int size)
        {
            return new Object[size];
        }

        [ExportField("CREATOR")]
        public static Community Creator()
        {
            return new Community();
        }

        #endregion

        public Location GetLocation()
        {
            return new Location(LocationManager.GpsProvider)
            {
                Latitude = Latitude,
                Longitude = Longitude
            };
        }

        public static Community IsInCyclesCommunity(LatLng pointLatLng)
        {
            var builder = new LatLngBounds.Builder();
            if (CyclesCommunities == null) return null;
            foreach (Community community in CyclesCommunities)
            {
                if (community == null) continue;
                foreach (LatLng latLngSet in community.PolygonCoordinates)
                    if (latLngSet != null)
                        builder.Include(latLngSet);

                LatLngBounds polygonBounds = builder.Build();
                if (polygonBounds.Contains(pointLatLng))
                    return community;
            }

            return null;
        }

        public static Community IsInCyclesCommunity(Location location)
        {
            var builder = new LatLngBounds.Builder();
            if (CyclesCommunities == null) return null;
            if (CyclesCommunitiesPolygons != null && CyclesCommunitiesPolygons.Any())
            {
                foreach (LatLngBounds polygonBounds in CyclesCommunitiesPolygons)
                {
                    if (polygonBounds.Contains(new LatLng(location.Latitude, location.Longitude)))
                        return CyclesCommunities[CyclesCommunitiesPolygons.IndexOf(polygonBounds)];
                }
            }
            else
            {
                foreach (Community community in CyclesCommunities)
                {
                    if (community == null) continue;
                    foreach (LatLng latLngSet in community.PolygonCoordinates)
                        if (latLngSet != null)
                            builder.Include(latLngSet);

                    LatLngBounds polygonBounds = builder.Build();
                    CyclesCommunitiesPolygons?.Add(polygonBounds);
                    if (polygonBounds.Contains(new LatLng(location.Latitude, location.Longitude)))
                        return community;
                }
            }


            return null;
        }
    }
}