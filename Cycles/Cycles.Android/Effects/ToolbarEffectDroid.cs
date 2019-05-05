using Android.Content;
using Android.Views;
using Cycles.Droid.Effects;
using Cycles.Utils;
using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Toolbar = Android.Support.V7.Widget.Toolbar;

[assembly: ResolutionGroupName("Custom")]
[assembly: ExportEffect(typeof(ToolbarEffectDroid), "ToolbarEffect")]
namespace Cycles.Droid.Effects
{
    internal class ToolbarEffectDroid : PlatformEffect
    {
        protected override void OnAttached()
        {
            try
            {
                if (Container != null)
                {
                    UpdateControl();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        protected override void OnDetached()
        {

        }

        private void UpdateControl()
        {
            var effect = (ToolbarEffect)Element.Effects.FirstOrDefault(e => e is ToolbarEffect);
            if (effect != null && Container != null)
            {
                try
                {
                    Context context = Container.Context;
                    MainActivity mainActivity = (context as MainActivity);
                    Toolbar toolbar = (Toolbar)LayoutInflater.FromContext(context).Inflate(Resource.Layout.Toolbar, null);
                    toolbar.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                    
                    (Container as ViewGroup).AddView(toolbar);
                    toolbar.Layout(0, 0, 700, 332);
                    //toolbar.BringToFront();
                    //mainActivity.SetSupportActionBar(toolbar);
                    //mainActivity.SupportActionBar.Show();
                    for (int i = 0; i < Container.ChildCount; i++)
                    {
                        var foundChild = Container.GetChildAt(i);
                        var type = foundChild.GetType();
                    }
                    int height = toolbar.Height;

                    //Android.Support.V7.App.ActionBar actionBar = mainActivity.SupportActionBar;
                    
                    //actionBar.SetDisplayHomeAsUpEnabled(true);
                    //actionBar.SetDisplayShowTitleEnabled(false);
                    //actionBar.SetHomeAsUpIndicator(Resource.Drawable.abc_ic_ab_back_material);


                    //Container.AddView(toolbar);
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }
    }
}