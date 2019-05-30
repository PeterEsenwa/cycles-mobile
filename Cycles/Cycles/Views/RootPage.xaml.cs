using Cycles.Droid;
using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Cycles.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RootPage
    {
        public RootPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            MasterPage.ListView.ItemSelected += ListView_ItemSelected;
            MessagingCenter.Subscribe<MainActivity>(this, "openMenu", (sender) =>
            {
                IsPresented = true;
            });
        }

        private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as RootPageMenuItem;
            if (item == null)
                return;


            var page = (Page)Activator.CreateInstance(item.TargetType);
            page.Title = item.Title;

            NavigationPage detailPage = new NavigationPage(page)
            {
            };
            NavigationPage.SetHasNavigationBar(detailPage, false);
            Detail = detailPage;
            IsPresented = false;

            MasterPage.ListView.SelectedItem = null;
        }

        protected override void OnAppearing()
        {
            MasterPage.ListView.SelectedItem = new RootPageMaster.RootPageMasterViewModel().MenuItems[0];
            base.OnAppearing();
        }
    }
}