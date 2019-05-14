using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Widget;
using Java.Lang;

namespace Cycles.Droid.Utils
{
    abstract class EmailTextWatcher : Java.Lang.Object, ITextWatcher
    {
        private dynamic DynamicInput { get; set; }

        public abstract void OnTextFilled(bool isValid);

        public EmailTextWatcher(dynamic dynamicInput)
        {
            DynamicInput = dynamicInput ?? throw new ArgumentNullException(nameof(dynamicInput));
        }

        protected EmailTextWatcher()
        {
        }

        public void AfterTextChanged(IEditable s)
        {
            Console.WriteLine(Class + " AfterTextChanged fired");
        }

        public void BeforeTextChanged(ICharSequence s, int start, int count, int after)
        {
            Console.WriteLine(Class + " BeforeTextChanged fired");
        }

        public void OnTextChanged(ICharSequence s, int start, int before, int count)
        {
            if (DynamicInput == null)
            {
                switch (s)
                {
                    case null:
                    {
                        OnTextFilled(false);
                        break;
                    }

                    default:
                    {
                        if (Android.Util.Patterns.EmailAddress.Matcher(s).Matches())
                        {
                            OnTextFilled(true);
                        }
                        else
                        {
                            OnTextFilled(false);
                        }
                        break;
                    }

                }
            }

        }
    }
}