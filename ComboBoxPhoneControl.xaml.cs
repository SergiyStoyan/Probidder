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
    public partial class ComboBoxPhoneControl : ComboBox
    {
        static ComboBoxPhoneControl()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(ComboBoxControl), new FrameworkPropertyMetadata(typeof(ComboBoxControl)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            SelectionChanged += ComboBox_SelectionChanged;

            //tb = this.FindVisualChildrenOfType<TextBox>().Where(x => x.Name == "TextBox").First();
            tb = this.FindVisualChildrenOfType<TextBox>().First();
            tb.Text = mask;
            tb.PreviewTextInput += TextBox_PreviewTextInput;
            tb.TextChanged += TextBox_TextChanged;
            //tb.KeyDown += TextBox_PreviewKeyDown;
            //tb.LostFocus += TextBox_LostFocus;
            //tb.GotFocus += TextBox_GotFocus;
        }

        public ComboBoxPhoneControl()
        {
            InitializeComponent();

            Loaded += delegate
              {
              };

            GotKeyboardFocus += ComboBoxControl_GotKeyboardFocus;
            PreviewKeyDown += ComboBoxControl_PreviewKeyDown;
            LostKeyboardFocus += ComboBoxControl_LostFocus;

            List<string> ss = mask.ToCharArray().Distinct().Select(x => Regex.Escape(x.ToString())).ToList();
            mask_r = new Regex("[" + string.Join("", ss) + "]");
            ss.Remove(Regex.Escape("_"));
            mask_separators_r = new Regex("[" + string.Join("", ss) + "]");
        }

        private void ComboBoxControl_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Regex.IsMatch(tb.Text, @"\d") && Regex.IsMatch(tb.Text, @"_"))
            {
                this.MarkInvalid("error");
                return;
            }
            this.MarkValid();
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
            delete_clicked = false;
            if (e.Key == Key.Up)
            {
                e.Handled = true;
                if (SelectedIndex <= 0)
                    SelectedIndex = Items.Count - 1;
                else
                    SelectedIndex = SelectedIndex - 1;
                select(0, tb.Text.Length);
                return;
            }
            if (e.Key == Key.Down)
            {
                e.Handled = true;
                if (SelectedIndex < 0 || SelectedIndex >= Items.Count - 1)
                    SelectedIndex = 0;
                else
                    SelectedIndex = SelectedIndex + 1;
                select(0, tb.Text.Length);
                return;
            }
            if (e.Key == Key.Delete)
            {
                delete_clicked = true;
                return;
            }
            if (e.Key == Key.Back)
            {
                delete_clicked = true;
                return;
            }
        }
        bool delete_clicked = false;

        readonly string mask = "(___) ___-____";
        readonly Regex mask_separators_r = null;
        readonly Regex mask_r = null;

        private void ComboBoxControl_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (tb == null)
                return;
            tb.Focus();
            select(0, tb.Text.Length);
        }
        
        TextBox tb = null;

        void select(int index, int length)
        {
            tb.BeginChange();
            tb.SelectionStart = index;
            tb.SelectionLength = length;
            tb.ScrollToHome();
            tb.EndChange();
        }

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
            selection_setting = true;
            int p = tb.SelectionStart;
            string s = (string)SelectedItem;
            if (s == null)
            {
                if (text_setting)
                {
                    tb.Text = apply_mask(tb.Text);
                    select(p, 0);
                }
                else
                    tb.Text = mask;
            }
            else
            {
                string t = tb.Text.Substring(0, p);
                tb.Text = s;
                if (tb.Text.StartsWith(t, StringComparison.InvariantCultureIgnoreCase))
                    select(p, tb.Text.Length - p);
                else
                    select(p, 0);
            }
            //this.MarkValid();
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
            //if (t.Length > mask.Length)
            //    return;
            string v = strip_mask(t);
            if (v.Length > 0 && !delete_clicked)
            {
                foreach (string i in Items)
                {
                    if (string.IsNullOrEmpty(i))
                        continue;
                    if (strip_mask(i).StartsWith(v, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if ((string)SelectedItem == i)
                            ComboBox_SelectionChanged(null, null);
                        SelectedItem = i;
                        text_setting = false;
                        return;
                    }
                }
            }
            SelectedItem = null;
            ComboBox_SelectionChanged(null, null);
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
    }
}
