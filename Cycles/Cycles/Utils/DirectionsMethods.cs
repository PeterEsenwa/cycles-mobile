using Android.Gms.Maps.Model;
using Cycles.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Cycles.Utils
{
    public static class DirectionsMethods
    {
        private static HttpClient httpClient = new HttpClient { MaxResponseContentBufferSize = 100000 };
        private static string apiUrl;

        public static double GetDistance(Directions directions)
        {
            if (directions != null)
            {
                double distance = directions.Routes.Sum(r => r.legs.Sum(l => l.distance.value));
                if (distance == 0)
                {
                    throw new Exception("Google cannot find road route");
                }
                return distance;
            }
            throw new Exception("Directions object cannot be null");
        }


        public static List<LatLng> DecodePolyline(string encodedPoints)
        {
            if (string.IsNullOrWhiteSpace(encodedPoints))
            {
                return null;
            }

            int index = 0;
            var polylineChars = encodedPoints.ToCharArray();
            var poly = new List<LatLng>();
            int currentLat = 0;
            int currentLng = 0;
            int next5Bits;

            while (index < polylineChars.Length)
            {
                // calculate next latitude
                int sum = 0;
                int shifter = 0;

                do
                {
                    next5Bits = polylineChars[index++] - 63;
                    sum |= (next5Bits & 31) << shifter;
                    shifter += 5;
                }
                while (next5Bits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length)
                {
                    break;
                }

                currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                // calculate next longitude
                sum = 0;
                shifter = 0;

                do
                {
                    next5Bits = polylineChars[index++] - 63;
                    sum |= (next5Bits & 31) << shifter;
                    shifter += 5;
                }
                while (next5Bits >= 32 && index < polylineChars.Length);

                if (index >= polylineChars.Length && next5Bits >= 32)
                {
                    break;
                }

                currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

                var mLatLng = new LatLng(Convert.ToDouble(currentLat) / 100000.0, Convert.ToDouble(currentLng) / 100000.0);
                poly.Add(mLatLng);
            }

            return poly;
        }

        public async static Task<Directions> GetDirectionsInfo(double startLatitude, double endLatitude, double startLongitude, double endLongitude)
        {
            try
            {
                apiUrl = string.Format(URLS.GoogleDirectionAPI,
                    startLatitude, startLongitude,
                    endLatitude.ToString(),
                    endLongitude.ToString()
                    );
                //WebRequest request = HttpWebRequest.Create(apiUrl);
                HttpResponseMessage response = await httpClient.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    JObject respBody = JObject.Parse(await response.Content.ReadAsStringAsync());
                    Directions responseData = JsonConvert.DeserializeObject<Directions>(respBody.Value<JObject>().ToString());
                    return responseData;
                }
                else
                {
                    throw new Exception("Unable to get location from google");
                }

            }
            catch (Exception ex)
            {
                //int R = 6371;
                //Double dLat = (endLatitude - startLatitude).ToRadian();
                //Double dLon = (endLongitude - startLongitude).ToRadian();
                //var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                //        Math.Cos(startLatitude.ToRadian()) * Math.Cos(endLatitude.ToRadian()) *
                //        Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
                //var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                //var d = R * c;
                //return d * 1000;
                return null;
            }

        }



        public static Double ToRadian(this Double number)
        {
            return (number * Math.PI / 180);
        }
    }
}
