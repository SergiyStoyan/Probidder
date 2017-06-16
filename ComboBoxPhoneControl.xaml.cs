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
using System.Collections;

namespace Cliver.Probidder
{
    /// <summary>
    /// Interaction logic for ComboBoxControl.xaml
    /// </summary>
    public partial class ComboBoxPhoneControl : ComboBox
    {
        static ComboBoxPhoneControl()
        {
            List<string> ss = mask.ToCharArray().Distinct().Select(x => Regex.Escape(x.ToString())).ToList();
            mask_r = new Regex("[" + string.Join("", ss) + "]");
            ss.Remove(Regex.Escape("_"));
            mask_separators_r = new Regex("[" + string.Join("", ss) + "]");
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            
            tb0 = this.FindVisualChildrenOfType<TextBox>().Where(x => x.Name == "PART_EditableTextBox").First();
            tb0.TextChanged += tb0_TextChanged;

            tb = this.FindVisualChildrenOfType<TextBox>().Where(x => x.Name == "TextBox").First();
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

            SelectionChanged += ComboBox_SelectionChanged;
            GotKeyboardFocus += ComboBoxControl_GotKeyboardFocus;
            PreviewKeyDown += ComboBoxControl_PreviewKeyDown;
            LostKeyboardFocus += ComboBoxControl_LostFocus;
        }
        
        private void tb0_TextChanged(object sender, TextChangedEventArgs e)
        {
            //tb0.TextChanged -= tb0_TextChanged;
            if (do_not_change_tb_text)
                return;
            ignore_text_change = true;
            tb.Text = apply_mask(tb0.Text);
            ignore_text_change = false;
            old_value = tb.Text;
        }
        bool do_not_change_tb_text = false;

        TextBox tb0;
        string old_value;

        private void ComboBoxControl_LostFocus(object sender, RoutedEventArgs e)
        {
            Text = tb.Text;
            tb.ScrollToHome();
            //if (Regex.IsMatch(tb.Text, @"\d") && Regex.IsMatch(tb.Text, @"_"))
            //{
            //    this.MarkInvalid("error");
            //    return;
            //}
            //if (!IsEditable && (
            //    strip_mask((string)SelectedItem) != strip_mask(tb.Text)
            //    || string.IsNullOrEmpty((string)SelectedItem)) != string.IsNullOrEmpty(strip_mask(tb.Text)
            //    ))
            //{
            //    this.MarkInvalid("Error");
            //}
            //this.MarkValid();
        }

        //protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        //{
            //if (!ignore_OnItemsSourceChanged)
            //{
                //List<string> newValue2 = new List<string>();            
                //foreach (string s in newValue)
                //{
                //    if (string.IsNullOrWhiteSpace(s))
                //        continue;
                //    string v = apply_mask(s);
                //    if (!newValue2.Contains(v))
                //        newValue2.Add(v);
                //}

            //    //base.OnItemsSourceChanged(oldValue, newValue);
            //    ignore_OnItemsSourceChanged = true;
            //    ItemsSource = null;
            //    ItemsSource = newValue2;
            //    ignore_OnItemsSourceChanged = false;
            //    base.OnItemsSourceChanged(oldValue, newValue2);
            //}
        //}
        //bool ignore_OnItemsSourceChanged = false;

        public static IEnumerable<string> GetItemsNormalized(IEnumerable items)
        {
            List<string> is2 = new List<string>();
            foreach (string s in items)
            {
                if (string.IsNullOrWhiteSpace(s))
                    continue;
                string v = apply_mask(s);
                if (!is2.Contains(v))
                    is2.Add(v);
            }
            return is2;
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

        static readonly string mask = "(___) ___-____";
        static readonly Regex mask_separators_r = null;
        static readonly Regex mask_r = null;

        static public string Mask
        {
            get
            {
                return mask;
            }
        }

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

      static  string apply_mask(string t)
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

        string strip_mask(string t)
        {
            if (t == null)
                return null;
            return mask_r.Replace(t, "");
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ignore_text_change = true;
            int p = tb.SelectionStart;
            string s = (string)SelectedItem;
            if (s == null)
            {
                if (IsEditable)
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
            ignore_text_change = false;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            e.Handled = true;
            if (ignore_text_change)
                return;
            do_not_change_tb_text = true;
            string v = strip_mask(tb.Text);
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
                        do_not_change_tb_text = false;
                        return;
                    }
                }
            }
            SelectedItem = null;
            ComboBox_SelectionChanged(null, null);
            do_not_change_tb_text = false;
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
            if(t[p] == '_')
                t = t.Remove(p, 1);
            t = t.Insert(p, e.Text);
            p++;
            tb.Text = apply_mask(t);
            tb.SelectionStart = p;
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
}
