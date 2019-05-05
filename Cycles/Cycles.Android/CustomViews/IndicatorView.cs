using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace Cycles.Droid.CustomViews
{
    public class IndicatorView : ImageView
    {

        private bool _isCurrentPage;
        /**
         * Sets if this indicator's page is the current page in view
         */
        public bool IsCurrentPage {
            get => _isCurrentPage;

            set {
                _isCurrentPage = value;
                if (value == false)
                {
                    (this as ImageView).SetImageResource(Resource.Drawable.other_page_indicator);
                }
                else
                {
                    (this as ImageView).SetImageResource(Resource.Drawable.current_page_indicator);
                    MessagingCenter.Send(this, "SetAsCurrent");
                }
            }
        }

        public IndicatorView(Context context) : base(context)
        {

        }

        public IndicatorView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public IndicatorView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
        }

        public IndicatorView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
        }
    }

    public class IndicatorLayout : Android.Widget.RelativeLayout
    {
        private int _number_of_Indicators;
        // Sets the number of indicators needed
        public int Number_of_Indicators {
            get => _number_of_Indicators;
            set {
                _number_of_Indicators = value;
                CreateIndicators(value);
            }
        }

        private int CurrentIndex { get; set; }

        private void CreateIndicators(int numberOfIndicators)
        {
            LayoutParams layoutParams;
            IndicatorView currentIndicator = new IndicatorView(Context);
            for (int i = 0; i < numberOfIndicators; i++)
            {
                // If first indicator/page
                if (i == 0)
                {
                    layoutParams = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
                    layoutParams.AddRule(LayoutRules.CenterInParent);

                    currentIndicator = new IndicatorView(Context)
                    {
                        IsCurrentPage = true,
                        LayoutParameters = layoutParams,
                        Id = GenerateViewId()
                    };

                    Indicators.Add(currentIndicator);
                    AddView(currentIndicator);
                    currentIndicator.Id = new Random().Next();
                }
                else if (i > 0)
                {
                    layoutParams = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
                    layoutParams.AddRule(LayoutRules.CenterVertical);
                    layoutParams.AddRule(LayoutRules.RightOf, Indicators[i - 1].Id);
                    layoutParams.LeftMargin = 8;
                    IndicatorView otherIndicator = new IndicatorView(Context)
                    {
                        IsCurrentPage = false,
                        LayoutParameters = layoutParams,
                        Id = GenerateViewId()
                    };

                    Indicators.Add(otherIndicator);
                    AddView(otherIndicator);
                }


            }
        }

        public void MoveToPage(int pageIndex, int oldIndex)
        {
            pageIndex -= 1;
            Indicators[CurrentIndex].IsCurrentPage = false;
            foreach (IndicatorView indicator in Indicators)
            {
                if (oldIndex - 1 < pageIndex)
                {
                    int index = Indicators.IndexOf(indicator);
                    if (index != pageIndex)
                    {
                        indicator.IsCurrentPage = false;
                        indicator.Animate().TranslationXBy(-28);
                    }
                    else
                    {
                        Indicators[pageIndex].IsCurrentPage = true;
                        Indicators[pageIndex].Animate().TranslationXBy(-28)
                            .WithEndAction(new Runnable(() =>
                            {
                                (Indicators[pageIndex].LayoutParameters as LayoutParams).AddRule(LayoutRules.CenterInParent);
                            }));
                    }
                }
                else if (oldIndex - 1 > pageIndex)
                {
                    int index = Indicators.IndexOf(indicator);
                    if (index != pageIndex)
                    {
                        indicator.IsCurrentPage = false;
                        indicator.Animate().TranslationXBy(28);
                    }
                    else
                    {
                        Indicators[pageIndex].IsCurrentPage = true;
                        Indicators[pageIndex].Animate().TranslationXBy(28)
                            .WithEndAction(new Runnable(() =>
                            {
                                (Indicators[pageIndex].LayoutParameters as LayoutParams).AddRule(LayoutRules.CenterInParent);
                            }));
                    }
                }

            }
        }

        private ObservableCollection<IndicatorView> Indicators { get; set; } = new ObservableCollection<IndicatorView>();

        public IndicatorLayout(Context context) : base(context)
        {
            MessagingCenter.Subscribe<IndicatorView>(this, "SetAsCurrent", (sender) =>
            {
                CurrentIndex = Indicators.Count != 0 ? Indicators.IndexOf(sender) : 0;
            });
        }

        public IndicatorLayout(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            MessagingCenter.Subscribe<IndicatorView>(this, "SetAsCurrent", (sender) =>
            {
                CurrentIndex = Indicators.Count != 0 ? Indicators.IndexOf(sender) : 0;
            });

        }

        public IndicatorLayout(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            MessagingCenter.Subscribe<IndicatorView>(this, "SetAsCurrent", (sender) =>
            {
                CurrentIndex = Indicators.Count != 0 ? Indicators.IndexOf(sender) : 0;
            });
        }

        public IndicatorLayout(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            MessagingCenter.Subscribe<IndicatorView>(this, "SetAsCurrent", (sender) =>
            {
                CurrentIndex = Indicators.Count != 0 ? Indicators.IndexOf(sender) : 0;
            });
        }
    }
}