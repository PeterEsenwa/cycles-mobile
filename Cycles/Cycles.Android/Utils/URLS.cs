namespace Cycles.Droid.Utils
{
    internal class URLS
    {
        public static string GoogleDirectionAPI { get; set; } = "https://maps.googleapis.com/maps/api/directions/json?origin={0},{1}&destination={2},{3}&sensor=false&mode=walking&key=AIzaSyD4k7vC9LxSiWSBYdQxhrb3UUmqrPkQJ3A";
        public static string GoogleDirectionAPIDriving { get; set; } = "https://maps.googleapis.com/maps/api/directions/json?origin={0},{1}&destination={2},{3}&sensor=false&mode=driving&key=AIzaSyD4k7vC9LxSiWSBYdQxhrb3UUmqrPkQJ3A";
    }
}