using Cycles.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
namespace Cycles.Utils
{
    public class TermsAndPrivacy : ContentPage
    {
        static BindableProperty TabsSourceProperty { get; } = BindableProperty.Create(
           "TabsSource",
           typeof(ObservableCollection<TabItem>),
           typeof(CustomLabel),
           new ObservableCollection<TabItem>(),
           propertyChanged: null);

        public ObservableCollection<TabItem> TabsSource {
            get { return (ObservableCollection<TabItem>)GetValue(TabsSourceProperty); }
            set { SetValue(TabsSourceProperty, value); }
        }

        public TermsAndPrivacy()
        {
            var terms = new TabItem("https://cycles.com.ng/terms", "Terms");
            var privacy = new TabItem("https://cycles.com.ng/privacy", "Privacy");
            NavigationPage.SetHasNavigationBar(this, false);
            TabsSource = new ObservableCollection<TabItem>()
            {
                terms, privacy
            };
        }
        public class TabItem
        {
            public string Uri { get; private set; }
            public string Title { get; private set; }

            public TabItem()
            {
            }

            public TabItem(string uri, string title)
            {
                Uri = uri;
                Title = title;
            }
        }

    }
}
