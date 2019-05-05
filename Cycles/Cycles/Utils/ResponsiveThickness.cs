using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Cycles.Utils
{
    internal class ResponsiveThickness : IMarkupExtension<Thickness>
    {
        public NamedSize Size { get; set; }

        public double Uniform { get; set; } = 0;

        public double Left { get; set; } = 0;

        public double Top { get; set; } = 0;

        public double Right { get; set; } = 0;

        public double Bottom { get; set; } = 0;

        public Thickness ProvideValue(IServiceProvider serviceProvider)
        {
            Thickness respThickness = new Thickness(0, 0, 0, 0);
            if (!Uniform.Equals(0))
            {
                respThickness.Bottom = Uniform / 14 * Device.GetNamedSize(Size, typeof(Label));
                respThickness.Top = Uniform / 14 * Device.GetNamedSize(Size, typeof(Label));
                respThickness.Left = Uniform / 14 * Device.GetNamedSize(Size, typeof(Label));
                respThickness.Right = Uniform / 14 * Device.GetNamedSize(Size, typeof(Label));
            }
            if (!Left.Equals(0))
            {
                respThickness.Left = Left / 14 * Device.GetNamedSize(Size, typeof(Label));
            }
            if (!Right.Equals(0))
            {
                respThickness.Right = Right / 14 * Device.GetNamedSize(Size, typeof(Label));
            }
            if (!Top.Equals(0))
            {
                respThickness.Top = Top / 14 * Device.GetNamedSize(Size, typeof(Label));
            }
            if (!Bottom.Equals(0))
            {
                respThickness.Bottom = Bottom / 14 * Device.GetNamedSize(Size, typeof(Label));
            }

            return respThickness;
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return (this as IMarkupExtension<Thickness>).ProvideValue(serviceProvider);
        }
    }
}
