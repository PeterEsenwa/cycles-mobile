using Xamarin.Forms;

namespace Cycles.Views
{
    internal class PaddingBorderEntry : Entry
    {
        private static BindableProperty BackgroundProperty { get; } = BindableProperty.Create(
          "Background",
          typeof(Color),
          typeof(PaddingBorderEntry),
          Color.Transparent,
          propertyChanged: (bindable, oldvalue, newvalue) =>
          {
              ((PaddingBorderEntry)bindable).InvalidateMeasure();
          });

        public Color Background {
            get { return (Color)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        private static BindableProperty BorderColorProperty { get; } = BindableProperty.Create(
          "BorderColor",
          typeof(Color),
          typeof(PaddingBorderEntry),
          Color.Transparent,
          propertyChanged: (bindable, oldvalue, newvalue) =>
          {
              ((PaddingBorderEntry)bindable).InvalidateMeasure();
          });

        public Color BorderColor {
            get { return (Color)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }

        private static BindableProperty BorderWidthProperty { get; } = BindableProperty.Create(
            "BorderWidth",
            typeof(int),
            typeof(PaddingBorderEntry),
            0,
            propertyChanged: (bindable, oldvalue, newvalue) =>
            {
                ((PaddingBorderEntry)bindable).InvalidateMeasure();
            });

        public int BorderWidth {
            get { return (int)GetValue(BorderWidthProperty); }
            set { SetValue(BorderWidthProperty, value); }
        }

        private static BindableProperty BackgroundOpacityProperty { get; } = BindableProperty.Create(
            "BackgroundOpacity",
            typeof(int),
            typeof(PaddingBorderEntry),
            0,
            propertyChanged: (bindable, oldvalue, newvalue) =>
            {
                ((PaddingBorderEntry)bindable).InvalidateMeasure();
            });

        public int BackgroundOpacity {
            get { return (int)GetValue(BackgroundOpacityProperty); }
            set { SetValue(BackgroundOpacityProperty, value); }
        }

        private static BindableProperty CornerRadiusProperty { get; } = BindableProperty.Create(
            "CornerRadius",
            typeof(double),
            typeof(PaddingBorderEntry),
            0.0,
            propertyChanged: (bindable, oldvalue, newvalue) =>
            {
                ((PaddingBorderEntry)bindable).InvalidateMeasure();
            });

        public double CornerRadius {
            get { return (double)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        #region Has Border property
        private static BindableProperty HasBorderProperty { get; } = BindableProperty.Create(
            "HasBorder",
            typeof(bool),
            typeof(PaddingBorderEntry),
            false,
            propertyChanged: (bindable, oldvalue, newvalue) =>
            {
                ((PaddingBorderEntry)bindable).InvalidateMeasure();
            }
        );

        public bool HasBorder {
            get { return (bool)GetValue(HasBorderProperty); }
            set { SetValue(HasBorderProperty, value); }
        }

        #endregion

        public PaddingBorderEntry()
        {

        }

        #region Has Curved Corners?
        public bool IsCurvedCornersEnabled {
            get { return (bool)GetValue(IsCurvedCornersEnabledProperty); }
            set { SetValue(IsCurvedCornersEnabledProperty, value); }
        }

        private static BindableProperty IsCurvedCornersEnabledProperty { get; } = BindableProperty.Create(
          "IsCurvedCornersEnabled",
          typeof(bool),
          typeof(PaddingBorderEntry),
          true,
          propertyChanged: (bindable, oldvalue, newvalue) =>
          {
              ((PaddingBorderEntry)bindable).InvalidateMeasure();
          });
        #endregion

        #region Set thickness of Padding
        public Thickness Padding {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        private static BindableProperty PaddingProperty { get; } = BindableProperty.Create(
          "Padding",
          typeof(Thickness),
          typeof(PaddingBorderEntry),
          new Thickness(5),
          propertyChanged: (bindable, oldvalue, newvalue) =>
          {
              ((PaddingBorderEntry)bindable).InvalidateMeasure();
          });
        #endregion

        private static BindableProperty FontSizeFactorProperty { get; } = BindableProperty.Create(
          "FontSizeFactor",
          typeof(double),
          typeof(PaddingBorderEntry),
          1.0,
          propertyChanged: OnFontSizeFactorChanged);

        public double FontSizeFactor {
            get { return (double)GetValue(FontSizeFactorProperty); }
            set { SetValue(FontSizeFactorProperty, value); }
        }

        private static void OnFontSizeFactorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((PaddingBorderEntry)bindable).OnFontSizeChangedImpl();
        }

        private static BindableProperty NamedFontSizeProperty { get; } = BindableProperty.Create(
          "NamedFontSize",
          typeof(NamedSize),
          typeof(PaddingBorderEntry),
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
            ((PaddingBorderEntry)bindable).OnFontSizeChangedImpl();
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
            double namedSize = Device.GetNamedSize(NamedFontSize, typeof(Entry));
            FontSize = FontSizeFactor * namedSize * density;
            InvalidateMeasure();
        }

        private static int BorderWidthByPlatform()
        {
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    return 1;
                case Device.Android:
                    return 2;
                case Device.UWP:
                    return 2;
                default:
                    return 2;
            }
        }
    }
}
