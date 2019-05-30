using Cycles.Droid;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Cycles.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RootPage
    {
        private Page PreviousPage { get; set; }

        public RootPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            MasterPage.MenuItemsListView.ItemSelected += ListView_ItemSelected;

            MessagingCenter.Subscribe<MainActivity>(this, "openMenu", (sender) => { IsPresented = true; });
        }

        private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (!(e.SelectedItem is RootPageMenuItem item))
                return;

            if (PreviousPage != null && PreviousPage.Title.Equals(item.Title))
            {
                IsPresented = false;
            }
            else
            {
                PreviousPage = (Page) Activator.CreateInstance(item.TargetType);
                PreviousPage.Title = item.Title;
                NavDetailPage = new NavigationPage(PreviousPage);
                NavigationPage.SetHasNavigationBar(NavDetailPage, false);
                Detail = NavDetailPage;
                IsPresented = false;
            }


//            MasterPage.ListView.SelectedItem = null;
        }

        private NavigationPage NavDetailPage { get; set; }

        protected override void OnAppearing()
        {
            if (MasterPage.MenuItemsListView.SelectedItem == null)
            {
                MasterPage.MenuItemsListView.SelectedItem = RootPageMaster.RootMasterViewModel.MenuItems[0];
            }

            base.OnAppearing();
        }

    }
}