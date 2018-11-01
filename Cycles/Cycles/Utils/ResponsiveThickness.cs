using System;
using System.Collections.Generic;
using System.Text;
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
            if (!Uniform.Equals(0))
            {
                double uniformDouble = (Uniform / 14) * Device.GetNamedSize(Size, typeof(Label));
                return new Thickness(uniformDouble);
            }
            else if (!Left.Equals(0) || !Right.Equals(0) || !Top.Equals(0) || !Bottom.Equals(0))
            {
                double leftDouble = (Left / 14) * Device.GetNamedSize(Size, typeof(Label));
                double rightDouble = (Right / 14) * Device.GetNamedSize(Size, typeof(Label));
                double topDouble = (Top / 14) * Device.GetNamedSize(Size, typeof(Label));
                double bottomDouble = (Bottom / 14) * Device.GetNamedSize(Size, typeof(Label));
                return new Thickness(leftDouble, topDouble, rightDouble, bottomDouble);
            }
            return new Thickness(0);
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return (this as IMarkupExtension<Thickness>).ProvideValue(serviceProvider);
        }
    }
}
