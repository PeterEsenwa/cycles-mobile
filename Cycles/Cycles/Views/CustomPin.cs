using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace Cycles.Views
{
    public class CustomPin : Pin
    {
        //public string Url { get; set; }

        public CustomType PinType { get; set; }

        public enum CustomType
        {
            Park = 1,
            Other = 2
        }

    }
}