using Xamarin.Forms;

namespace Cycles.Views
{
    internal class SizedButton : Button
    {
        public Thickness Padding {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        private static BindableProperty PaddingProperty { get; } = BindableProperty.Create(
          "Padding",
          typeof(Thickness),
          typeof(SizedButton),
          new Thickness(5),
          propertyChanged: (bindable, oldvalue, newvalue) =>
          {
              ((SizedButton)bindable).InvalidateMeasure();
          });
        private static BindableProperty FontSizeFactorProperty { get; } = BindableProperty.Create(
          "FontSizeFactor",
          typeof(double),
          typeof(SizedButton),
          1.0,
          propertyChanged: OnFontSizeFactorChanged);

        public double FontSizeFactor {
            get { return (double)GetValue(FontSizeFactorProperty); }
            set { SetValue(FontSizeFactorProperty, value); }
        }

        private static void OnFontSizeFactorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((SizedButton)bindable).OnFontSizeChangedImpl();
        }

        private static BindableProperty NamedFontSizeProperty { get; } = BindableProperty.Create(
          "NamedFontSize",
          typeof(NamedSize),
          typeof(SizedButton),
          defaultValue: NamedSize.Large,
          propertyChanged: OnNamedFontSizeChanged);

        // When work wants to kill you
        // Just remember to...

        public NamedSize NamedFontSize {
            get { return (NamedSize)GetValue(NamedFontSizeProperty); }
            set { SetValue(NamedFontSizeProperty, value); }
        }

        private static void OnNamedFontSizeChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((SizedButton)bindable).OnFontSizeChangedImpl();
        }


        public virtual void OnFontSizeChangedImpl()
        {
            double density = (App.ScreenWidth + App.ScreenHeight) / 2;
            density = density / 160;

            density = ((density - 1) / 3) + 1;
            if (Device.Idiom == TargetIdiom.Tablet)
            {
                density = density * 2;
            }
            FontSize = FontSizeFactor * Device.GetNamedSize(NamedFontSize, typeof(Button)) * density;
            InvalidateMeasure();
        }
    }
}
