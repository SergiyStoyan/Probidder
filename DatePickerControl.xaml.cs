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
        }

        private void DatePickerControl_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            tb.Focus();
            //((DatePicker)sender).FocusOnText();
        }

        TextBox tb;
        readonly string mask = "__/__/__";

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime? dt = SelectedDate;
            if (dt == null)
            {
                if (tb.Text.Length < 1)
                    tb.Text = mask;
                return;
            }
            tb.Text = ((DateTime)dt).ToString("MM/dd/yyyy");
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            e.Handled = true;
            string t = tb.Text;
            if (t.Length >= mask.Length)
                return;
            int p = tb.SelectionStart;
            t = t.Substring(0, p) + Regex.Replace(t.Substring(p), @"/", "");
            for (int i = 2; i < mask.Length; i += 3)
                if (t[i] != '/')
                    t = t.Insert(i, "/");
            tb.Text = t + mask.Substring(t.Length);
            if (p > 0 && t[p - 1] == '/')
                p--;
            tb.SelectionStart = p;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
            string t = tb.Text;
            int p = tb.SelectionStart;
            if (!Regex.IsMatch(t, "_") || p >= mask.Length)
            {
                Console.Beep(5000, 200);
                return;
            }
            if (p == 2 || p == 5)
            {
                if (e.Text == "/")
                    return;
                p++;
            }
            t = t.Substring(0, p) + e.Text + Regex.Replace(t.Substring(p), @"/", "");
            p++;
            for (int i = 2; i < mask.Length; i += 3)
                if (t[i] != '/')
                    t = t.Insert(i, "/");
            tb.Text = t.Substring(0, mask.Length);
            if (p < mask.Length && t[p] == '/')
                p++;
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
            DateTime? dt = calendar_input(tb.Text);
            if (SelectedDate == null && dt == null
                || SelectedDate != null && dt != null && ((DateTime)SelectedDate).Date == ((DateTime)dt).Date && tb.Text.Length != 10)
                DatePicker_SelectedDateChanged(null, null);
            SelectedDate = dt;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (tb.Name != "TextBox")
                tb = (TextBox)sender;
            if (SelectedDate == null)
            {
                if (tb.Text.Length < 1)
                    tb.Text = mask;
                return;
            }
            tb.Text = ((DateTime)SelectedDate).ToString("MM/dd/yy");
            tb.SelectionStart = 0;
            //tb.SelectionLength = 1;
        }
    }

    static public class WpfControlRoutines
    {
        public static void FocusOnText(this DatePicker datePicker)
        {
            if (datePicker == last_focused)
                return;
            last_focused = datePicker;
            Keyboard.Focus(datePicker);
            var eventArgs = new KeyEventArgs(Keyboard.PrimaryDevice,
                                             Keyboard.PrimaryDevice.ActiveSource,
                                             0,
                                             Key.Up);
            eventArgs.RoutedEvent = DatePicker.KeyDownEvent;
            datePicker.RaiseEvent(eventArgs);
        }
        static IInputElement last_focused = null;
    }
}