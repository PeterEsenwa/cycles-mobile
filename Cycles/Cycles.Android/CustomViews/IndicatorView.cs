using Android.Content;
using Android.Util;
using Android.Widget;
using Xamarin.Forms;

namespace Cycles.Droid.CustomViews
{
    public class IndicatorView : ImageView
    {
        private IndicatorLayout ParentLayout { get; set; }
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
            ParentLayout = new IndicatorLayout(context);
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
}