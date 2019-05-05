using Android.Support.V4.View;
using Cycles.Droid.Renderers;
using Cycles.Views;
using Xamarin.Forms;
using Android.Views;
using Java.Lang;
using Cycles.Utils;
using System.Collections.ObjectModel;
using static Cycles.Utils.TermsAndPrivacy;

[assembly: ExportRenderer(typeof(CustomTabView), typeof(CustomTabViewRenderer))]
namespace Cycles.Droid.Renderers
{
    class CustomPagerAdapter : PagerAdapter
    {
        private TermsAndPrivacyRenderer termsAndPrivacyRenderer;
        ObservableCollection<TabItem> TabCollection;
        public CustomPagerAdapter(TermsAndPrivacyRenderer termsAndPrivacyRenderer, TermsAndPrivacy termsAndPrivacy)
        {
            this.termsAndPrivacyRenderer = termsAndPrivacyRenderer;
            TabCollection = termsAndPrivacy?.TabsSource;
        }

        public override Object InstantiateItem(ViewGroup container, int position)
        {
            TabItem tab = TabCollection?[position];
            if (tab != null)
            {
                Android.Webkit.WebView webView = new Android.Webkit.WebView(termsAndPrivacyRenderer.Context);
                webView.Settings.JavaScriptEnabled = true;
                webView.LoadUrl(tab.Uri);

                container.AddView(webView);
                return webView;
            }
            return base.InstantiateItem(container, position);
        }

        public override ICharSequence GetPageTitleFormatted(int position)
        {
            return new String(TabCollection?[position].Title);
        }

        public override int Count => TabCollection.Count | 0;

        public override bool IsViewFromObject(Android.Views.View view, Object @object)
        {
            return view == @object;
        }
    }
}