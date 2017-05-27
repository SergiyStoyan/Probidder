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
    /// Interaction logic for ComboBoxControl.xaml
    /// </summary>
    public partial class ComboBoxControl : ComboBox
    {
        static ComboBoxControl()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(ComboBoxControl), new FrameworkPropertyMetadata(typeof(ComboBoxControl)));
        }

        public ComboBoxControl()
        {
            InitializeComponent();
            
            Loaded += delegate
              {
              };

            GotKeyboardFocus += ComboBoxControl_GotKeyboardFocus;
            PreviewKeyDown += ComboBoxControl_PreviewKeyDown;
            LostFocus += ComboBoxControl_LostFocus;

            List<string> ss = mask.ToCharArray().Distinct().Select(x => Regex.Escape(x.ToString())).ToList();
            mask_r = new Regex("[" + string.Join("", ss) + "]");
            ss.Remove(Regex.Escape("_"));
            mask_separators_r = new Regex("[" + string.Join("", ss) + "]");
        }

        private void ComboBoxControl_LostFocus(object sender, RoutedEventArgs e)
        {
            //var b = GetBindingExpression(TextProperty);
            //string t = apply_mask(tb.Text);
            //if (Regex.IsMatch(t, @"_") && Regex.IsMatch(t, @"\d"))
            //{
            //    ValidationError ve = new ValidationError(new BogusValidationRule(), b);
            //    ve.ErrorContent = "Error";
            //    Validation.MarkInvalid(b, ve);
            //}
            //Validation.ClearInvalid(b);
        }

        public IEnumerable<string> ItemsSourceNomalized
        {
            set
            {
                List<string> vs = new List<string>();
                foreach (string s in value)
                {
                    if (string.IsNullOrWhiteSpace(s))
                        continue;
                    string v = apply_mask(s);
                    if (!vs.Contains(v))
                        vs.Add(v);
                }
                ItemsSource = vs;
                tb.Text = mask;
            }
        }

        private void ComboBoxControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                e.Handled = true;
                if (SelectedIndex <= 0)
                    SelectedIndex = Items.Count - 1;
                else
                    SelectedIndex = SelectedIndex - 1;
                return;
            }
            if (e.Key == Key.Down)
            {
                e.Handled = true;
                if (SelectedIndex < 0 || SelectedIndex >= Items.Count - 1)
                    SelectedIndex = 0;
                else
                    SelectedIndex = SelectedIndex + 1;
                return;
            }
        }

        readonly string mask = "(___) ___-____";
        readonly Regex mask_separators_r = null;
        readonly Regex mask_r = null;

        private void ComboBoxControl_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (tb != null)
                tb.Focus();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            //hidden_tb = this.FindVisualChildrenOfType<TextBox>().Where(x=>x.Name== "PART_EditableTextBox").First();
            //hidden_tb.Visibility = Visibility.Collapsed;

            //tb = this.FindVisualChildrenOfType<TextBox>().Where(x => x.Name == "TextBox").First();
            tb = this.FindVisualChildrenOfType<TextBox>().First();
            tb.Text = mask;
            tb.PreviewTextInput += TextBox_PreviewTextInput;
            tb.TextChanged += TextBox_TextChanged;
            tb.KeyDown += TextBox_PreviewKeyDown;
            tb.LostFocus += TextBox_LostFocus;
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
        }

        //TextBox hidden_tb = null;
        TextBox tb = null;

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

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string s = (string)SelectedItem;
            if (s == null)
            {
                if (tb.Text.Length < 1)
                    tb.Text = mask;
                return;
            }
            int p = tb.SelectionStart;
            string t = tb.Text.Substring(0, p);
            tb.Text = apply_mask(s);
            tb.SelectionStart = p;
            if (tb.Text.StartsWith(t))
                tb.SelectionLength = tb.Text.Length - p;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (ignore_text_change)
            //    return;

            e.Handled = true;
            string t = tb.Text;
            if (t.Length > mask.Length)
                return;

            //ignore_text_change = true;

            string v = strip_mask(t);
            if (v.Length > 0)
            {
                foreach (string i in Items)
                    if (strip_mask(i).StartsWith(v, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if ((string)SelectedItem == i)
                            ComboBox_SelectionChanged(null, null);
                        SelectedItem = i;
                        return;
                    }
            }
            SelectedItem = null;
            int p = tb.SelectionStart;            
            tb.Text = apply_mask(t);
            //tb.ScrollToHome();
            tb.SelectionStart = p;

            //ignore_text_change = false;
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
            t = t.Remove(p, 1);
            t = t.Insert(p, e.Text);
            p++;
            tb.Text = t;
            tb.SelectionStart = p;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            //if (tb.Name != "TextBox")
            //    tb = (TextBox)sender;
            //if (SelectedDate == null)
            //{
            //    if (tb.Text.Length < 1)
            //        tb.Text = mask;
            //    return;
            //}
            //tb.Text = ((DateTime)SelectedDate).ToString("MM/dd/yy");
            //tb.SelectionStart = 0;
            //tb.SelectionLength = 1;
        }
    }
    //public class BogusValidationRule : ValidationRule
    //{
    //    public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
    //    {
    //        return new ValidationResult(false, "value cannot be empty.");
    //    }
    //}
}
