using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Cycles.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Dashboard : ContentPage
    {
        private ObservableCollection<RideInfo> Rides { get; set; }
        public Dashboard()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            Rides = new ObservableCollection<RideInfo>()
            {
                new RideInfo(
                    "NG Hub From Facebook, Montgomery Rd, Sabo yaba 100001, Lagos",
                    "Rosabon Financial Services, 32 Montgomery Rd, Sabo yaba 100001, Lagos",
                    new DateTime(2019,2,25,14,30,10),
                    new DateTime(2019,2,25,14,40,28),
                    10,
                    19,
                    140),
                new RideInfo(
                    "Rosabon Financial Services, 32 Montgomery Rd, Sabo yaba 100001, Lagos",
                    "NG Hub From Facebook, Montgomery Rd, Sabo yaba 100001, Lagos",
                    new DateTime(2019,2,21,14,32,16),
                    new DateTime(2019,2,21,14,42,29),
                    8,
                    19,
                    140),
                new RideInfo(
                    "Rosabon Financial Services, 32 Montgomery Rd, Sabo yaba 100001, Lagos",
                    "NG Hub From Facebook, Montgomery Rd, Sabo yaba 100001, Lagos",
                    new DateTime(2019,2,21,14,32,16),
                    new DateTime(2019,2,21,14,42,29),
                    7,
                    19,
                    140),
            };
            RidesHistory.ItemsSource = Rides;
        }
        public class RideInfo : IEquatable<RideInfo>
        {
            public string To { get; set; }
            public string From { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public double TimeSpent { get; set; }
            public double Calories { get; set; }
            public decimal Cost { get; set; }

            public RideInfo()
            {
            }

            public RideInfo(string to, string from, DateTime startTime, DateTime endTime, double timeSpent, double calories, decimal cost)
            {
                To = to ?? throw new ArgumentNullException(nameof(to));
                From = from ?? throw new ArgumentNullException(nameof(from));
                StartTime = startTime;
                EndTime = endTime;
                TimeSpent = timeSpent;
                Calories = calories;
                Cost = cost;
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as RideInfo);
            }

            public bool Equals(RideInfo other)
            {
                return other != null &&
                       To == other.To &&
                       From == other.From &&
                       StartTime == other.StartTime &&
                       EndTime == other.EndTime &&
                       TimeSpent == other.TimeSpent &&
                       Calories == other.Calories &&
                       Cost == other.Cost;
            }

            public override int GetHashCode()
            {
                var hashCode = -1245731201;
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(To);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(From);
                hashCode = hashCode * -1521134295 + StartTime.GetHashCode();
                hashCode = hashCode * -1521134295 + EndTime.GetHashCode();
                hashCode = hashCode * -1521134295 + TimeSpent.GetHashCode();
                hashCode = hashCode * -1521134295 + Calories.GetHashCode();
                hashCode = hashCode * -1521134295 + Cost.GetHashCode();
                return hashCode;
            }

            public static bool operator ==(RideInfo info1, RideInfo info2)
            {
                return EqualityComparer<RideInfo>.Default.Equals(info1, info2);
            }

            public static bool operator !=(RideInfo info1, RideInfo info2)
            {
                return !(info1 == info2);
            }
        }

       
    }

}