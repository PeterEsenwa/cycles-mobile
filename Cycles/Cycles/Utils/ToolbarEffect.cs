using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace Cycles.Utils
{
    public class ToolbarEffect : RoutingEffect
    {
        public ToolbarEffect() : base("Custom.ToolbarEffect")
        {

        }

        protected ToolbarEffect(string effectId) : base(effectId)
        {
        }

        public static readonly BindableProperty HasToolbarProperty =
           BindableProperty.CreateAttached("HasToolbar",
               typeof(bool),
               typeof(ToolbarEffect),
               false,
               propertyChanged: OnHasToolbarChanged);

        #region HasToolbarProperty setter and getter
        public static bool GetHasToolbar(BindableObject view)
        {
            return (bool)view.GetValue(HasToolbarProperty);
        }

        public static void SetHasToolbar(BindableObject view, bool value)
        {
            view.SetValue(HasToolbarProperty, value);
        }
        #endregion

        public static void OnHasToolbarChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (!(bindable is ContentPage view))
            {
                return;
            }

            if ((bool)newValue)
            {
                view.Effects.Add(new ToolbarEffect());
            }
            else
            {
                if (view.Effects.FirstOrDefault(e => e is ToolbarEffect) != null)
                {
                    view.Effects.Remove(view.Effects.FirstOrDefault(e => e is ToolbarEffect));
                }
            }
        }
    }

    public class StringSplitterConverter : IValueConverter
    {
        public StringSplitterConverter()
        {

        }


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string[] values = ((string)value).Split(",", StringSplitOptions.RemoveEmptyEntries);
            int index = int.Parse((string)parameter);
            return values[index];
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "";
        }
        
    }
}
