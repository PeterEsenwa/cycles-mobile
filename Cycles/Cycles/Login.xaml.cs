using System;
using Android;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.Content;
using Cycles.Utils;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Cycles
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Login : InitPage
    {
        public Login()
        {
            InitializeComponent();
            BindingContext = this;
            double oldMarginTop = (double)Application.Current.Resources["DottedPathMarginTop"];
            DottedPathMargin = new Thickness(0, oldMarginTop, 0, 0);
            DottedPath.Margin = DottedPathMargin;
        }
        public Thickness DottedPathMargin { get; set; }

        private void Login_btn_Clicked(object sender, EventArgs e)
        {
            //if ((int)Build.VERSION.SdkInt < 23)
            //{

            //}
            //else
            //{

            //}
            try
            {
                Application.Current.MainPage = new MainPage();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}