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

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            
            //tb = this.FindVisualChildrenOfType<TextBox>().Where(x => x.Name == "TextBox").First();
            tb = this.FindVisualChildrenOfType<TextBox>().First();
            tb.PreviewTextInput += TextBox_PreviewTextInput;
            tb.TextChanged += TextBox_TextChanged;
            //tb.KeyDown += TextBox_PreviewKeyDown;
            //tb.LostFocus += TextBox_LostFocus;
            //tb.GotKeyboardFocus += TextBox_GotKeyboardFocus;
        }

        public ComboBoxControl()
        {
            InitializeComponent();

            Loaded += delegate
              {
              };

            GotKeyboardFocus += ComboBoxControl_GotKeyboardFocus;
            PreviewKeyDown += ComboBoxControl_PreviewKeyDown;
            LostKeyboardFocus += ComboBoxControl_LostFocus;
        }

        private void ComboBoxControl_LostFocus(object sender, RoutedEventArgs e)
        {
            tb.ScrollToHome();
            //if (Regex.IsMatch(tb.Text, @"\d") && SelectedItem == null)
            //{
            //    this.MarkInvalid("error");
            //    return;
            //}
            //this.MarkValid();
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
                //e.Handled = true;
                //if (tb.SelectionStart > 0)
                //{
                //    tb.SelectionStart = tb.SelectionStart - 1;
                //    tb.SelectionLength += 1;
                //}
                delete_clicked = true;
                return;
            }
        }
        bool delete_clicked = false;

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

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selection_setting = true;
            int p = tb.SelectionStart;
            string s = (string)SelectedItem;
            if (s == null)
            {
                select(p, 0);
                //this.MarkValid();
            }
            else
            {
                string t = tb.Text.Substring(0, p);
                tb.Text = s;
                if (tb.Text.StartsWith(t, StringComparison.InvariantCultureIgnoreCase))
                    select(p, tb.Text.Length - p);
                else
                    select(p, 0);
                //this.MarkValid();
            }
            selection_setting = false;
        }
        bool selection_setting = false;

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            e.Handled = true;
            if (selection_setting)
                return;
            string t = tb.Text;
            //Items.Filter = new Predicate<object>((object o) => {
            //    return ((string)o).StartsWith(t, StringComparison.InvariantCultureIgnoreCase);
            //});
            if (t.Length > 0 && !delete_clicked)
            {
                foreach (string i in Items)
                {
                    if (string.IsNullOrEmpty(i))
                        continue;                    
                    if (i.StartsWith(t, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if ((string)SelectedItem == i)
                            ComboBox_SelectionChanged(null, null);
                        SelectedItem = i;
                        return;
                    }
                }
            }
            SelectedItem = null;
            ComboBox_SelectionChanged(null, null);
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
        }        
    }
}
