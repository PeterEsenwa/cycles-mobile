using System;
using Xamarin.Essentials;

namespace Cycles.Utils
{
    class FontHandler
    {
        public static bool firstPageLoad = true;
        public static string lastOrientation;

        public static void AdjustFontSizes(double difference)
        {
            double oldLargeFS = (double)Xamarin.Forms.Application.Current.Resources["FontSizeLarge"];
            double oldMediumFS = (double)Xamarin.Forms.Application.Current.Resources["FontSizeMedium"];
            double oldMediumAltFS = (double)Xamarin.Forms.Application.Current.Resources["FontSizeMediumAlt"];
            double oldBigFS = (double)Xamarin.Forms.Application.Current.Resources["FontSizeBig"];
            double oldSmallFS = (double)Xamarin.Forms.Application.Current.Resources["FontSizeSmall"];
            double oldTinyFS = (double)Xamarin.Forms.Application.Current.Resources["FontSizeTiny"];
            double oldVeryTinyFS = (double)Xamarin.Forms.Application.Current.Resources["FontSizeVeryTiny"];
            double oldMarginTop = (double)Xamarin.Forms.Application.Current.Resources["DottedPathMarginTop"];
            Xamarin.Forms.Application.Current.Resources["FontSizeLarge"] = oldLargeFS * difference;
            Xamarin.Forms.Application.Current.Resources["FontSizeMedium"] = oldMediumFS * difference;
            Xamarin.Forms.Application.Current.Resources["FontSizeMediumAlt"] = oldMediumAltFS * difference;
            Xamarin.Forms.Application.Current.Resources["FontSizeBig"] = oldBigFS * difference;
            Xamarin.Forms.Application.Current.Resources["FontSizeSmall"] = oldSmallFS * difference;
            Xamarin.Forms.Application.Current.Resources["FontSizeTiny"] = oldTinyFS * difference;
            Xamarin.Forms.Application.Current.Resources["FontSizeVeryTiny"] = oldVeryTinyFS * difference;
            Xamarin.Forms.Application.Current.Resources["DottedPathMarginTop"] = oldMarginTop * difference;
        }
        public static void ScaleFontSizes()
        {
            ScreenMetrics metrics = DeviceDisplay.ScreenMetrics;
            
            if (metrics.Rotation == ScreenRotation.Rotation0 || metrics.Rotation == ScreenRotation.Rotation180)
            {
                if (firstPageLoad)
                {
                    lastOrientation = "potrait";
                    double density = metrics.Density;
                    // Width (in pixels)
                    double width = metrics.Width;

                    // Height (in pixels)
                    double height = metrics.Height;

                    double heightInDp = height / density;
                    double widthInDP = width / density;
                    double diffHeight = Math.Round(heightInDp / 533, 3);
                    double diffWidth = Math.Round(widthInDP / 320, 3);
                    double diff = Math.Round(diffHeight * diffWidth, 2);
                    firstPageLoad = false;
                    AdjustFontSizes(diff);
                }
                else if (!firstPageLoad && lastOrientation == "landscape")
                {
                    AdjustFontSizes(1 / 1.2);
                }
            }
            else if (metrics.Rotation == ScreenRotation.Rotation90 || metrics.Rotation == ScreenRotation.Rotation270)
            {
                if (firstPageLoad)
                {
                    lastOrientation = "landscape";
                    double density = metrics.Density;
                    // Width (in pixels)
                    double width = metrics.Width;

                    // Height (in pixels)
                    double height = metrics.Height;

                    double heightInDp = height / density;
                    double widthInDP = width / density;
                    double diffHeight = Math.Round(heightInDp / 320, 3);
                    double diffWidth = Math.Round(widthInDP / 533, 3);
                    double diff = Math.Round(diffHeight * diffWidth, 2);
                    firstPageLoad = false;
                    AdjustFontSizes(diff * 1.2);
                }
                else
                {
                    AdjustFontSizes(1.2);
                }
            }

        }

    }
}
