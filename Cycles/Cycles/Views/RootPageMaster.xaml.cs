﻿using Cycles.Utils;
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
    public partial class RootPageMaster : ContentPage
    {
        public ListView ListView;

        public RootPageMaster()
        {
            InitializeComponent();

            BindingContext = new RootPageMasterViewModel();
            ListView = MenuItemsListView;
        }

        public class RootPageMasterViewModel : INotifyPropertyChanged
        {
            public ObservableCollection<RootPageMenuItem> MenuItems { get; set; }

            public RootPageMasterViewModel()
            {
                MenuItems = new ObservableCollection<RootPageMenuItem>(new[]
                {
                    new RootPageMenuItem { Id = 0, Title = "Home", TargetType = typeof(MapPage), ImageUrl="house_icon" },
                    new RootPageMenuItem { Id = 1, Title = "Dashboard", TargetType = typeof(Dashboard), ImageUrl="bar_chart"  },
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
                if (PropertyChanged == null)
                    return;

                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion
        }
    }
}