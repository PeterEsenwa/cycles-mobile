using Android.Text;
using Android.Widget;
using Java.Lang;

namespace Cycles.Droid.Utils
{
    internal class PhoneTextWatcher : Object, ITextWatcher
    {
        private EditText InputEditText { get; set; }

        public PhoneTextWatcher(EditText editText)
        {
            InputEditText = editText;
        }

        public void AfterTextChanged(IEditable s)
        {
            //throw new NotImplementedException();
        }

        public void BeforeTextChanged(ICharSequence s, int start, int count, int after)
        {
            //throw new NotImplementedException();
        }

        public void OnTextChanged(ICharSequence s, int start, int before, int count)
        {
            string text = InputEditText.Text.ToString();
            int textLength = InputEditText.Text.Length;
            if (text.EndsWith("-") || text.EndsWith(" ") || text.EndsWith(")") || text.EndsWith(")"))
                return;
            if (textLength >= 1 && textLength <= 4)
            {
                if (textLength == 1 && text.Contains("+"))
                {
                    InputEditText.SetText(new StringBuilder(text).Replace(text.Length - 1, 1, "(234) ").ToString(), TextView.BufferType.Normal);
                    InputEditText.SetSelection(InputEditText.Text.Length);
                }
                else if (textLength >= 3 && text.Contains("234") && !text.Contains("("))
                {
                    InputEditText.SetText(new StringBuilder(text).Replace(0, text.Length, "(234) ").ToString(), TextView.BufferType.Normal);
                    InputEditText.SetSelection(InputEditText.Text.Length);
                }
                else if (textLength >= 3 && !text.Contains("("))
                {
                    InputEditText.SetText(new StringBuilder(text).Insert(0, "(234) ").ToString(), TextView.BufferType.Normal);
                    InputEditText.SetSelection(InputEditText.Text.Length);
                }
            }
            else if (textLength == 5)
            {
                if (!text.Contains(")"))
                {
                    InputEditText.SetText(new StringBuilder(text).Insert(text.Length - 1, ")").ToString(), TextView.BufferType.Normal);
                    InputEditText.SetSelection(InputEditText.Text.Length);
                }
            }
            else if (textLength == 6)
            {
                InputEditText.SetText(new StringBuilder(text).Insert(text.Length - 1, " ").ToString(), TextView.BufferType.Normal);
                InputEditText.SetSelection(InputEditText.Text.Length);
            }
            else if (textLength == 11)
            {
                if (!text.Contains("-"))
                {
                    InputEditText.SetText(new StringBuilder(text).Insert(text.Length - 1, "-").ToString(), TextView.BufferType.Normal);
                    InputEditText.SetSelection(InputEditText.Text.Length);
                }
            }
            else if (textLength == 15)
            {
                if (text.Contains("-"))
                {
                    InputEditText.SetText(new StringBuilder(text).Insert(text.Length - 1, "-").ToString(), TextView.BufferType.Normal);
                    InputEditText.SetSelection(InputEditText.Text.Length);
                }
            }
        }

    }
}