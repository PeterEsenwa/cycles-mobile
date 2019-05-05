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
    public partial class RootPageDetail : ContentPage
    {
        public RootPageDetail()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            labelToShow.Text = Title + " is coming soon";
        }
    }
}