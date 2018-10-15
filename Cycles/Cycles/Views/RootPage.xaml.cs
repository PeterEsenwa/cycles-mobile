using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Cycles.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RootPage : MasterDetailPage
    {
        public RootPage()
        {
            InitializeComponent();
            MasterPage.ListView.ItemSelected += ListView_ItemSelected;
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
                BarBackgroundColor = Color.Red
            };
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