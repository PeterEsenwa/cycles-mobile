using System;


namespace Cycles.Droid.Services
{
    public class UtcTimestamper : IGetTimestamp
    {
        DateTime startTime;
        private bool wasReset;

        public UtcTimestamper()
        {
            startTime = DateTime.UtcNow;
        }

        public string GetFormattedTimestamp()
        {
            TimeSpan duration = DateTime.UtcNow.Subtract(startTime);
            if (duration.TotalSeconds < 60)
            {
                return $"You've been riding for less than a minute";
            }
            else if (duration.TotalMinutes < 60)
            {
                return $"You've been riding for {duration:mm}minutes";
            }
            else
            {
                return $"You've been riding for {duration:hh}hours";
            }
        }

        public void Restart()
        {
            startTime = DateTime.UtcNow;
            wasReset = true;
        }

        public bool ShouldGetLocation()
        {
            TimeSpan duration = DateTime.UtcNow.Subtract(startTime);
            if (duration.TotalSeconds % 30 == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}