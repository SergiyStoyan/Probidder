/********************************************************************************************
        Author: Sergey Stoyan
        sergey.stoyan@gmail.com
        http://www.cliversoft.com
********************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace Cliver.Foreclosures
{
    /// <summary>
    /// Interaction logic for DatePickerControl.xaml
    /// </summary>
    public partial class DatePickerControl : DatePicker
    {
        public DatePickerControl()
        {
            InitializeComponent();

            GotKeyboardFocus += DatePickerControl_GotKeyboardFocus;
            LostKeyboardFocus += DatePickerControl_LostKeyboardFocus;

            Loaded += delegate
            {//bulding template is anynchronous so we are waiting when it is finished to get TextBox
                ThreadRoutines.StartTry(() =>
                {
                    TextBox tb1 = (TextBox)SleepRoutines.WaitForObject(() =>
                    {
                        return Dispatcher.Invoke(() =>
 {
     return this.FindVisualChildrenOfType<TextBox>().Where(x => x.Name == "TextBox").FirstOrDefault();
 });
                    }, 1000);
                    if (tb1 != null)
                    {
                        tb = tb1;
                        Dispatcher.Invoke(() =>
                        {
                            DatePicker_SelectedDateChanged(null, null);
                        });
                    }
                });
                tb = this.FindVisualChildrenOfType<TextBox>().Where(x => x.Name == "TextBox").FirstOrDefault();
                if (tb == null)
                    tb = this.FindVisualChildrenOfType<TextBox>().FirstOrDefault();
            };

            List<string> ss = mask.ToCharArray().Distinct().Select(x => Regex.Escape(x.ToString())).ToList();
            mask_r = new Regex("[" + string.Join("", ss) + "]");
            ss.Remove(Regex.Escape("_"));
            mask_separators_r = new Regex("[" + string.Join("", ss) + "]");
        }

        //public override void OnApplyTemplate()
        //{
        //    base.OnApplyTemplate();
        //    tb = this.FindVisualChildrenOfType<TextBox>().Where(x => x.Name == "TextBox").FirstOrDefault();
        //    if (tb == null)
        //        tb = this.FindVisualChildrenOfType<TextBox>().FirstOrDefault();
        //}

        private void DatePickerControl_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            DateTime? dt = calendar_input(tb.Text);
            if (dt == null && Regex.IsMatch(tb.Text, @"\d"))
            {
                this.MarkInvalid("Error");
                return;
            }
            if (SelectedDate != dt)
                base.SelectedDate = dt;
            else
                DatePicker_SelectedDateChanged(null, null);
            this.MarkValid();
        }

        private void DatePickerControl_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            tb.Focus();
            select(0, tb.Text.Length);
        }

        TextBox tb;
        readonly string mask = "__/__/__";
        readonly Regex mask_separators_r = null;
        readonly Regex mask_r = null;

        string apply_mask(string t)
        {
            if (t == null)
                return null;
            string s = strip_separators(t);
            t = mask;
            int j = 0;
            for (int i = 0; i < t.Length; i++)
            {
                if (j >= s.Length)
                    break;
                if (t[i] == '_')
                {
                    t = t.Remove(i, 1);
                    t = t.Insert(i, s[j++].ToString());
                }
            }
            return t;
        }

        string strip_separators(string t)
        {
            if (t == null)
                return null;
            return mask_separators_r.Replace(t, "");
        }

        string strip_mask(string t)
        {
            if (t == null)
                return null;
            return mask_r.Replace(t, "");
        }

        void select(int index, int length)
        {
            tb.BeginChange();
            tb.SelectionStart = index;
            tb.SelectionLength = length;
            tb.ScrollToHome();
            tb.EndChange();
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            selection_setting = true;
            int p = tb.SelectionStart;
            DateTime? dt = SelectedDate;
            if (dt == null)
            {
                if(text_setting)
                    tb.Text = apply_mask(tb.Text);
                else
                    tb.Text = mask;
                select(p, 0);
            }
            else
            {
                tb.Text = ((DateTime)dt).ToString("MM/dd/yyyy");
                select(p, tb.Text.Length);
            }
            selection_setting = false;
        }
        bool selection_setting = false;

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            e.Handled = true;
            if (selection_setting)
                return;
            text_setting = true;
            string t = tb.Text;
            DateTime? dt = calendar_input(t);
            if (SelectedDate != dt)
                SelectedDate = dt;
            else
                DatePicker_SelectedDateChanged(null, null);
            text_setting = false;
        }
        bool text_setting = false;

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
            string t = tb.Text;
            int p = tb.SelectionStart;
            t = t.Remove(p, tb.SelectionLength);
            t = apply_mask(t);
            if (!Regex.IsMatch(t, "_")
                || p >= mask.Length
                || Regex.IsMatch(e.Text, @"[^\d]")
                )
            {
                Console.Beep(5000, 200);
                return;
            }
            while (p < mask.Length && mask_separators_r.IsMatch(t[p].ToString()))
                p++;
            t = t.Remove(p, 1);
            t = t.Insert(p, e.Text);
            p++;
            tb.Text = t;
            tb.SelectionStart = p;
        }

        private DateTime? calendar_input(string text)
        {
            try
            {
                return DateTime.ParseExact(text, "MM/dd/yy", null);
            }
            catch
            {
            }
            try
            {
                return DateTime.ParseExact(text, "MM/dd/yyyy", null);
            }
            catch
            {
            }
            if (text.Length > 6 || Regex.IsMatch(text, @"[^\d]"))
                return null;
            Match m = Regex.Match(text, @"(\d{2})/(\d{2})/(\d{2})");
            if (!m.Success)
                return null;
            try
            {
                int y = int.Parse(m.Groups[3].Value);
                if (y < 30)
                    y += 2000;
                else
                    y += 1900;
                return new DateTime(y, int.Parse(m.Groups[1].Value), int.Parse(m.Groups[2].Value));
            }
            catch
            {
                return null;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            //DateTime? dt = calendar_input(tb.Text);
            //if (SelectedDate == null && dt == null
            //    || SelectedDate != null && dt != null && ((DateTime)SelectedDate).Date == ((DateTime)dt).Date && tb.Text.Length != 10)
            //    DatePicker_SelectedDateChanged(null, null);
            //SelectedDate = dt;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (tb.Name != "TextBox")
                tb = (TextBox)sender;
            if (SelectedDate == null)
                tb.Text = apply_mask(tb.Text);
            else
                tb.Text = ((DateTime)SelectedDate).ToString("MM/dd/yy");
            select(0, tb.Text.Length);
        }
    }

    //static public class WpfControlRoutines
    //{
    //    public static void FocusOnText(this DatePicker datePicker)
    //    {
    //        if (datePicker == last_focused)
    //            return;
    //        last_focused = datePicker;
    //        Keyboard.Focus(datePicker);
    //        var eventArgs = new KeyEventArgs(Keyboard.PrimaryDevice,
    //                                         Keyboard.PrimaryDevice.ActiveSource,
    //                                         0,
    //                                         Key.Up);
    //        eventArgs.RoutedEvent = DatePicker.KeyDownEvent;
    //        datePicker.RaiseEvent(eventArgs);
    //    }
    //    static IInputElement last_focused = null;
    //}
}