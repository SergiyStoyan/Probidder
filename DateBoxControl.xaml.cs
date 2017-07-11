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

namespace Cliver.Probidder
{
    public partial class DateBoxControl : TextBox
    {
        static DateBoxControl()
        {
            List<string> ss = mask.ToCharArray().Distinct().Select(x => Regex.Escape(x.ToString())).ToList();
            mask_r = new Regex("[" + string.Join("", ss) + "]");
            ss.Remove(Regex.Escape("_"));
            mask_separators_r = new Regex("[" + string.Join("", ss) + "]");
        }

        public DateBoxControl()
        {
            InitializeComponent();

            PreviewTextInput += TextBox_PreviewTextInput;
            LostKeyboardFocus += TextBox_LostKeyboardFocus;
            GotFocus += TextBox_GotFocus;
        }

        private void TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            selected_date = ParseText(Text);
            selectedDateChanged();
        }
        DateTime? selected_date = null;

        private void selectedDateChanged()
        {
            ignore_text_change = true;
            int p = SelectionStart;
            DateTime? dt = selected_date;
            if (dt == null)
            {
                Text = apply_mask(Text);
                select(p, 0);
            }
            else
            {
                if (IsKeyboardFocused)
                    Text = ((DateTime)dt).ToString("MM/dd/yy");
                else
                    Text = ((DateTime)dt).ToString("MM/dd/yyyy");
                //select(p, tb.Text.Length);
            }
            ignore_text_change = false;
        }

        static readonly string mask = "__/__/__";
        static readonly Regex mask_separators_r = null;
        static readonly Regex mask_r = null;

        public static string GetMaskedString(DateTime? dt = null)
        {
            if (dt == null)
                return mask;
            return ((DateTime)dt).ToString("MM/dd/yyyy");
        }

        static string apply_mask(string t)
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

        static string strip_separators(string t)
        {
            if (t == null)
                return null;
            return mask_separators_r.Replace(t, "");
        }

        static string strip_mask(string t)
        {
            if (t == null)
                return null;
            return mask_r.Replace(t, "");
        }

        void select(int index, int length)
        {
            BeginChange();
            SelectionStart = index;
            SelectionLength = length;
            ScrollToHome();
            EndChange();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (selected_date == null)
                Text = apply_mask(Text);
            else
                Text = ((DateTime)selected_date).ToString("MM/dd/yy");
            select(0, Text.Length);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            e.Handled = true;
            if (ignore_text_change)
                return;
            DateTime? td = ParseText(Text);
            if (selected_date != td)
                selected_date = td;
            selectedDateChanged();
        }
        bool ignore_text_change = false;

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
            string t = Text;
            int p = SelectionStart;
            t = t.Remove(p, SelectionLength);
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
            if (t[p] == '_')
                t = t.Remove(p, 1);
            t = t.Insert(p, e.Text);
            p++;
            Text = apply_mask(t);
            SelectionStart = p;
        }

        public static DateTime? ParseText(string text)
        {
            DateTime? dt;
            if (!texts2DateTime.TryGetValue(text, out dt))
            {
                dt = _ParseText(text);
                texts2DateTime[text] = dt;
            }
            return dt;
        }
        static Dictionary<string, DateTime?> texts2DateTime = new Dictionary<string, DateTime?>();
        static DateTime? _ParseText(string text)
        {
            Match m = Regex.Match(text, @"^(\d{2})/(\d{2})/(\d{2}(\d{2})?)$");
            if (!m.Success)
                return null;
            try
            {
                int y = int.Parse(m.Groups[3].Value);
                if (y < 30)
                    y += 2000;
                else if (y < 100)
                    y += 1900;
                return new DateTime(y, int.Parse(m.Groups[1].Value), int.Parse(m.Groups[2].Value));
            }
            catch
            {
                return null;
            }
        }
    }
}