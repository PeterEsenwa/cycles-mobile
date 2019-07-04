using Cycles.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Cycles.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RootPageMaster
    {
//        public ListView ListView;

        public RootPageMaster()
        {
            InitializeComponent();

            RootMasterViewModel = new RootPageMasterViewModel();
            BindingContext = RootMasterViewModel;
//            ListView = MenuItemsListView;
        }

        public static RootPageMasterViewModel RootMasterViewModel { get; private set; }

        public class RootPageMasterViewModel : INotifyPropertyChanged
        {
            public ObservableCollection<RootPageMenuItem> MenuItems { get; }

            public RootPageMasterViewModel()
            {
                MenuItems = new ObservableCollection<RootPageMenuItem>(new[]
                {
                    new RootPageMenuItem { Id = 0, Title = "Map", TargetType = typeof(MapPage), ImageUrl="map_icon" },
                    new RootPageMenuItem { Id = 1, Title = "Dashboard", TargetType = typeof(Dashboard), ImageUrl="dashboard_icon"  },
                    //new RootPageMenuItem { Id = 2, Title = "Profile" , TargetType = typeof(RootPageDetail) },
                    //new RootPageMenuItem { Id = 3, Title = "Plans & Membership" , TargetType = typeof(RootPageDetail) },
                    new RootPageMenuItem { Id = 4, Title = "How to Use", TargetType = typeof(HowTo), ImageUrl="how_to_use"  },
                    new RootPageMenuItem { Id = 5, Title = "Terms & Privacy", TargetType = typeof(TermsAndPrivacy), ImageUrl="terms_icon"  },
                    new RootPageMenuItem { Id = 6, Title = "Settings", TargetType = typeof(RootPageDetail), ImageUrl="settings_icon"  },
                });
            }

            #region INotifyPropertyChanged Implementation
            public event PropertyChangedEventHandler PropertyChanged;
            void OnPropertyChanged([CallerMemberName] string propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion
        }
    }
}