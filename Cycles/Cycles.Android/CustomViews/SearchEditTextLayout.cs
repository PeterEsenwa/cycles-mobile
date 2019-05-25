using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Animation;
using Android.Bluetooth;
using Android.Content;
using Android.Util;
using Android.Text;
using Android.Views;
using Android.Widget;
using static Android.Views.View;

namespace Cycles.Droid.CustomViews
{
    public class SearchEditTextLayout : FrameLayout
    {
        private const float EXPAND_MARGIN_FRACTION_START = 0.8f;
        private const int ANIMATION_DURATION = 200;

        private IOnKeyListener mPreImeKeyListener;
        private int? mTopMargin;
        private int? mBottomMargin;
        private int? mLeftMargin;
        private int? mRightMargin;

        private float mCollapsedElevation;

        /* Subclass-visible for testing */
        protected bool mIsExpanded = false;
        protected bool mIsFadedOut = false;
        private LinearLayout mCollapsed;
        private View mExpanded;
        private EditText mSearchView;
        private View mSearchIcon;
        private View mCollapsedSearchBox;
        private View mVoiceSearchButtonView;
        private View mOverflowButtonView;
        private View mBackButtonView;
        private View mExpandedSearchBox;
        private View mClearButtonView;
        private ValueAnimator mAnimator;
        private Callback mCallback;

        public SearchEditTextLayout(Context context) : base(context)
        {
        }

        public SearchEditTextLayout(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public interface Callback
        {
            void onBackButtonClicked();
            void onSearchViewClicked();
        }

        public void setPreImeKeyListener(IOnKeyListener listener)
        {
            mPreImeKeyListener = listener;
        }

        public void setCallback(Callback listener)
        {
            mCallback = listener;
        }

        protected override void OnFinishInflate()
        {
            var parameters = (MarginLayoutParams)LayoutParameters;
            mTopMargin = parameters?.TopMargin;
            mBottomMargin = parameters?.BottomMargin;
            mLeftMargin = parameters?.LeftMargin;
            mRightMargin = parameters?.RightMargin;

            mCollapsedElevation = Elevation;

            mCollapsed = (LinearLayout)FindViewById(Resource.Id.search_box_collapsed);
//            mExpanded = FindViewById(Resource.Id.search_box_expanded);
//            mSearchView = (EditText)mExpanded.FindViewById(Resource.Id.search_view);

            base.OnFinishInflate();
        }
    }
}