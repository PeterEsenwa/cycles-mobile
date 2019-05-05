using Android.Text;
using Android.Widget;
using Java.Lang;

namespace Cycles.Droid.Utils
{
    public class VerifyTextWatcher : Object, ITextWatcher
    {
        public EditText EditText { get; }
        public EditText NextEditText { get; }

        public VerifyTextWatcher(EditText editText, EditText nextEditText)
        {
            EditText = editText;
            NextEditText = nextEditText;
        }

        public void AfterTextChanged(IEditable s)
        {
            if (EditText.Text.Length != 0)
            {
                EditText.ClearFocus();
                NextEditText.RequestFocus();
                NextEditText.Enabled = true;
            }
        }

        public void BeforeTextChanged(ICharSequence s, int start, int count, int after)
        {

        }

        public void OnTextChanged(ICharSequence s, int start, int before, int count)
        {
        }

    }
}