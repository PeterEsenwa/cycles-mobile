using System;
using Android.Views;
using Xamarin.Forms;

namespace Cycles.Views
{
    internal class SizedButton : Button
    {
        public SizedButton() : base()
        {
            
        }

        private static BindableProperty IsRoundProperty { get; } = BindableProperty.Create(
              "IsRound",
              typeof(bool),
              typeof(SizedButton),
              false,
              propertyChanged: (bindable, oldvalue, newvalue) =>
              {
                  ((SizedButton)bindable).InvalidateMeasure();
              }
        );

        public bool IsRound {
            get { return (bool)GetValue(IsRoundProperty); }
            set { SetValue(IsRoundProperty, value); }
        }

        public int Radius {
            get { return (int)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        private static BindableProperty RadiusProperty { get; } = BindableProperty.Create(
              "Radius",
              typeof(int),
              typeof(SizedButton),
              0,
              propertyChanged: (bindable, oldvalue, newvalue) =>
              {
                  ((SizedButton)bindable).InvalidateMeasure();
              }
        );

        //public Thickness Padding {
        //    get { return (Thickness)GetValue(PaddingProperty); }
        //    set { SetValue(PaddingProperty, value); }
        //}

        //private static BindableProperty PaddingProperty { get; } = BindableProperty.Create(
        //  "Padding",
        //  typeof(Thickness),
        //  typeof(SizedButton),
        //  new Thickness(5),
        //  propertyChanged: (bindable, oldvalue, newvalue) =>
        //  {
        //      ((SizedButton)bindable).InvalidateMeasure();
        //  });

        //public static explicit operator SizedButton(Android.Views.View v)
        //{
        //    throw new NotImplementedException();
        //}

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
