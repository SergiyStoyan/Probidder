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
using System.ComponentModel;

namespace Cliver.Probidder
{
    public partial class DatePickerControl : DatePicker
    {
        static DatePickerControl()
        {
            List<string> ss = mask.ToCharArray().Distinct().Select(x => Regex.Escape(x.ToString())).ToList();
            mask_r = new Regex("[" + string.Join("", ss) + "]");
            ss.Remove(Regex.Escape("_"));
            mask_separators_r = new Regex("[" + string.Join("", ss) + "]");
        }

        public DatePickerControl()
        {
            InitializeComponent();

            GotKeyboardFocus += DatePickerControl_GotKeyboardFocus;
            LostKeyboardFocus += DatePickerControl_LostKeyboardFocus;
            SelectedDateChanged += DatePicker_SelectedDateChanged;
            PreviewGotKeyboardFocus += delegate
            {
            };

            Loaded += delegate
            {
                tb0 = this.FindVisualChildrenOfType<TextBox>().Where(x => x.Name == "PART_TextBox").FirstOrDefault();
                
                tb.PreviewTextInput += TextBox_PreviewTextInput;
                tb.TextChanged += TextBox_TextChanged;
                tb.GotFocus += TextBox_GotFocus;
                //tb.Focus();
                //this.FocusOnText();
            };
        }

        TextBox tb0;
        TextBox tb
        {
            get
            {
                if (_tb == null)
                {//bulding template is anynchronous so we are waiting when it is finished to get TextBox                    
                    _tb = (TextBox)SleepRoutines.WaitForObject(() =>
                    {
                        return this.FindVisualChildrenOfType<TextBox>().Where(x => x.Name == "TextBox").FirstOrDefault();
                    }, 1000);
                    if (_tb != null)
                    {
                        //Dispatcher.Invoke(() =>
                        //{
                        //    DatePicker_SelectedDateChanged(null, null);
                        //});
                    }
                }
                return _tb;
            }
        }
        TextBox _tb;

        //public override void OnApplyTemplate()
        //{
        //    base.OnApplyTemplate();
        //    tb = this.FindVisualChildrenOfType<TextBox>().Where(x => x.Name == "TextBox").FirstOrDefault();
        //    if (tb == null)
        //        tb = this.FindVisualChildrenOfType<TextBox>().FirstOrDefault();
        //}

        public string Text2 { get { return (string)GetValue(Text2Property); }
            set
            {
                if (Text2 == value)
                    return;
                DateTime? dt = ParseText(value);
                if (text2_dt == dt && dt != null)
                    return;
                SetValue(Text2Property, value);
            }
        }
        public static DependencyProperty Text2Property = DependencyProperty.Register("Text2", typeof(string), typeof(DatePickerControl),
            new FrameworkPropertyMetadata(
                mask,
                FrameworkPropertyMetadataOptions.AffectsRender,
                OnText2Changed
                )
            );
        private static void OnText2Changed(DependencyObject control, DependencyPropertyChangedEventArgs eventArgs)
        {
            var c = (DatePickerControl)control;
            c.tb.Text = (string)eventArgs.NewValue;
            c.text2_dt = ParseText((string)eventArgs.NewValue);
        }
        DateTime? text2_dt = null;

        public void Reset()
        {
            tb.Text = mask;
        }

        private void DatePickerControl_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            DatePicker_SelectedDateChanged(null, null);
        }

        private void DatePickerControl_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            tb.Focus();
            select(0, tb.Text.Length);
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
            tb.BeginChange();
            tb.SelectionStart = index;
            tb.SelectionLength = length;
            tb.ScrollToHome();
            tb.EndChange();
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ignore_text_change = true;
            int p = tb.SelectionStart;
            DateTime? dt = SelectedDate;
            if (dt == null)
            {
                tb.Text = apply_mask(tb.Text);
                select(p, 0);
            }
            else
            {
                if (tb.IsKeyboardFocused)
                    tb.Text = ((DateTime)dt).ToString("MM/dd/yy");
                else
                    tb.Text = ((DateTime)dt).ToString("MM/dd/yyyy");
                //select(p, tb.Text.Length);
            }
            Text2 = tb.Text;
            ignore_text_change = false;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            e.Handled = true;
            if (ignore_text_change)
                return;
            DateTime? td = ParseText(tb.Text);
            if (SelectedDate != td)
                SelectedDate = td;
            else
                DatePicker_SelectedDateChanged(null, null);
        }
        bool ignore_text_change = false;
        
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
            if (t[p] == '_')
                t = t.Remove(p, 1);
            t = t.Insert(p, e.Text);
            p++;
            tb.Text = apply_mask(t);
            tb.SelectionStart = p;
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

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SelectedDate == null)
                tb.Text = apply_mask(tb.Text);
            else
                tb.Text = ((DateTime)SelectedDate).ToString("MM/dd/yy");
            select(0, tb.Text.Length);
        }

        new public Brush Background
        {
            get
            {
                return tb.Background;
            }
            set
            {
                tb.Background = value;
            }
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