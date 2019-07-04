using System.Collections.Generic;
using Android.Gms.Maps.Model;

namespace Cycles.Droid.Utils
{
    internal class Communities
    {
        public static readonly List<Community> CyclesCommunities;
        public static readonly double LagosLatitude = 6.5244;
        public static readonly double LagosLongitude = 3.3792;
        static Communities()
        {
            CyclesCommunities = GetCommunities();
        }

        public static List<LatLngBounds> CyclesCommunitiesPolygons { get; set; } = new List<LatLngBounds>();

        private static List<Community> GetCommunities()
        {
            var communities = new List<Community>
            {
                new Community
                {
                    Address = "Covenant University, KM 10 Idiroko Rd, Ota, Ogun State",
                    Latitude = 6.6726447,
                    Longitude = 3.161213,
                    ShortName = "Covenant University",
                    PolygonCoordinates =
                    {
                        new LatLng(6.6727702, 3.1511776),
                        new LatLng(6.6678753, 3.153107),
                        new LatLng(6.6671008, 3.1584151),
                        new LatLng(6.6646683, 3.1583751),
                        new LatLng(6.6638839, 3.1618439),
                        new LatLng(6.6644895, 3.1649128),
                        new LatLng(6.6662271, 3.1650528),
                        new LatLng(6.6673789, 3.1631934),
                        new LatLng(6.6674693, 3.1631977),
                        new LatLng(6.668199, 3.1632325),
                        new LatLng(6.6710524, 3.1633687),
                        new LatLng(6.6711518, 3.1633734),
                        new LatLng(6.6713242, 3.1633573),
                        new LatLng(6.6785184, 3.1626833),
                        new LatLng(6.6783311, 3.1603488),
                        new LatLng(6.6752325, 3.1602644),
                        new LatLng(6.6749744, 3.1582552),
                        new LatLng(6.6731971, 3.1525272),
                        new LatLng(6.6727702, 3.1511776)
                    }
                }
            };
            return communities;
        }
    }
}