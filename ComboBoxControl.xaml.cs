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
using System.Reflection;

namespace Cliver.Foreclosures
{
    /// <summary>
    /// Interaction logic for ComboBoxControl.xaml
    /// </summary>
    public partial class ComboBoxControl : ComboBox/*, INotifyPropertyChanged*/
    {
        //public event PropertyChangedEventHandler PropertyChanged;
        //protected void NotifyPropertyChanged(string propertyName)
        //{
        //    if (PropertyChanged != null)
        //    {
        //        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        //    }
        //}

        //protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        //{
        //    base.OnPropertyChanged(e);
        //}

        static ComboBoxControl()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(ComboBoxControl), new FrameworkPropertyMetadata(typeof(ComboBoxControl)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            tb0 = this.FindVisualChildrenOfType<TextBox>().Where(x => x.Name == "PART_EditableTextBox").First();
            tb0.TextChanged += tb0_TextChanged;

            tb = this.FindVisualChildrenOfType<TextBox>().Where(x => x.Name == "TextBox").First();
            //tb = this.FindVisualChildrenOfType<TextBox>().First();
            tb.PreviewTextInput += TextBox_PreviewTextInput;
            tb.TextChanged += TextBox_TextChanged;
            //tb.KeyDown += TextBox_PreviewKeyDown;
            //tb.LostFocus += TextBox_LostFocus;
            //tb.GotKeyboardFocus += TextBox_GotKeyboardFocus;
        }

        private void tb0_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ignore_text_change)
                return;
            ignore_text_change = true;
            tb.Text = tb0.Text;
            ignore_text_change = false;
            old_value = tb.Text;
        }

        TextBox tb0;
        string old_value;

        //public event PropertyChangedEventHandler TextChanged;
        //public string Text
        //{
        //    get { return (string)GetValue(TextProperty); }
        //    set { SetValue(TextProperty, value);
        //        old_value = value;
        //    }
        //}
        //string old_value;
        //public static DependencyProperty TextProperty = DependencyProperty.Register("Text2", typeof(string), typeof(ComboBoxControl),
        //    new FrameworkPropertyMetadata(
        //        string.Empty,
        //        FrameworkPropertyMetadataOptions.AffectsRender,
        //        OnTextChanged
        //        )
        //    );
        //private static void OnTextChanged(DependencyObject control, DependencyPropertyChangedEventArgs eventArgs)
        //{
        //    var c = (ComboBoxControl)control;
        //    c.Text = (string)eventArgs.NewValue;
        //}

        public ComboBoxControl()
        {
            InitializeComponent();

            Loaded += delegate
              {
              };

            GotKeyboardFocus += ComboBoxControl_GotKeyboardFocus;
            PreviewKeyDown += ComboBoxControl_PreviewKeyDown;
            SelectionChanged += ComboBox_SelectionChanged;
            LostKeyboardFocus += ComboBoxControl_LostKeyboardFocus;
        }

        private void ComboBoxControl_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Text = tb.Text;
            tb.ScrollToHome();

            if (!IsEditable &&
                ((string)SelectedItem != tb.Text
                || string.IsNullOrEmpty((string)SelectedItem) != string.IsNullOrEmpty(tb.Text))
                )
            {
                this.MarkInvalid("Error");
            }
            this.MarkValid();

            //var dpd = DependencyPropertyDescriptor.FromProperty(TextProperty, GetType());
            //var dpd = DependencyPropertyDescriptor.FromName("Text", GetType(), GetType());
            //dpd.AddValueChanged(this)
            //DependencyProperty dp = dpd.DependencyProperty;
            //dp.ValidateValueCallback?.Invoke(tb.Text);
            //dp.AddValueChanged(this.tb, GridIsAvailableChanged);            

            //OnPropertyChanged(new DependencyPropertyChangedEventArgs(TextProperty, old_value, tb.Text));
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
            ignore_text_change = true;
            int p = tb.SelectionStart;
            string s = (string)SelectedItem;
            if (s == null)
            {
                if (IsEditable)
                    select(p, 0);
                else
                    tb.Text = "";
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
            ignore_text_change = false;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            e.Handled = true;
            if (ignore_text_change)
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
        bool ignore_text_change = false;

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
        }        
    }
}
