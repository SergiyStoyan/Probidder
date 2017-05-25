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
            {
                tb = this.FindChildrenOfType<TextBox>().First();
                //tb.DataContext = null;
                //var g = tb.GetBindingExpression(TextBox.TextProperty);
                //Binding b = new Binding("SelectedDate");
                //b.Mode = BindingMode.OneWay;
                //b.StringFormat = "MM/dd/yyyy";
                //tb.SetBinding(TextBox.TextProperty, b);
                //tb.PreviewTextInput += TextBox_PreviewTextInput;
                //tb.TextChanged += TextBox_TextChanged;
                //tb.LostFocus += TextBox_LostFocus;
                //tb.GotFocus += TextBox_GotFocus;
                //tb.Text = mask;
            };
        }

        private void DatePickerControl_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox_LostFocus(null, null);
        }

        private void DatePickerControl_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
        }

        TextBox tb;
        readonly string mask = "__/__/__";

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime? dt = SelectedDate;
            if (dt == null)
            {
                tb.Text = mask;
                return;
            }
            tb.Text = ((DateTime)dt).ToString("MM/dd/yyyy");
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            e.Handled = true;
            if (tb.Text.Length < mask.Length)
            {
                int p = tb.SelectionStart;
                string s = mask.Substring(p, mask.Length - tb.Text.Length);
                tb.Text = tb.Text.Insert(p, s);
                tb.SelectionStart = p;
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
            if (tb.SelectionStart >= mask.Length)
                return;
            int p = tb.SelectionStart;
            if (p == 2 || p == 5)
            {
                if (e.Text == "/")
                    return;
                p++;
            }
            if (Regex.IsMatch(e.Text, @"[^\d]"))
                return;
            string t = tb.Text.Remove(p, 1);
            tb.Text = t.Insert(p, e.Text);
            if (p == 1 || p == 4)
                p++;
            p++;
            tb.SelectionStart = p;
            tb.SelectionLength = 1;
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
            SelectedDate = calendar_input(tb.Text);
            DatePicker_SelectedDateChanged(null, null);
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (tb.Name != "TextBox")
                tb = (TextBox)sender;
            if (SelectedDate == null)
            {
                tb.Text = mask;
                return;
            }
            tb.Text = ((DateTime)SelectedDate).ToString("MM/dd/yy");
            tb.SelectionStart = 0;
            tb.SelectionLength = 1;
        }
    }
}