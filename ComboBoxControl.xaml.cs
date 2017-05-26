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

            SelectionChanged += ComboBoxControl_SelectionChanged;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var tbs = this.FindVisualChildrenOfType<TextBox>();
            tb = tbs.First();
            tb.PreviewTextInput += TextBox_PreviewTextInput;
            tb.TextChanged += TextBox_TextChanged;
            //if (grid != null)
            //{
            //    grid.ColumnDefinitions.Add(new ColumnDefinition());
            //    Button button = new Button();
            //    button.Content = "test";
            //    button.SetValue(Grid.ColumnProperty, 2);
            //    grid.Children.Add(button);
            //}
        }
        TextBox tb = null;
        readonly string mask = "__/__/__";

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //DateTime? dt = SelectedDate;
            //if (dt == null)
            //{
            //    if (tb.Text.Length < 1)
            //        tb.Text = mask;
            //    return;
            //}
            //tb.Text = ((DateTime)dt).ToString("MM/dd/yyyy");
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            e.Handled = true;
            //string t = tb.Text;
            //if (t.Length >= mask.Length)
            //    return;
            //int p = tb.SelectionStart;
            //t = t.Substring(0, p) + Regex.Replace(t.Substring(p), @"/", "");
            //for (int i = 2; i < mask.Length; i += 3)
            //    if (t[i] != '/')
            //        t = t.Insert(i, "/");
            //tb.Text = t + mask.Substring(t.Length);
            //tb.SelectionStart = p;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
            tb.Text = "()";
            SelectedIndex = 1;
            //string t = tb.Text;
            //int p = tb.SelectionStart;
            //if (!Regex.IsMatch(t, "_") || p >= mask.Length)
            //{
            //    Console.Beep(5000, 200);
            //    return;
            //}
            //if (p == 2 || p == 5)
            //{
            //    if (e.Text == "/")
            //        return;
            //    p++;
            //}
            //t = t.Substring(0, p) + e.Text + Regex.Replace(t.Substring(p), @"/", "");
            //p++;
            //for (int i = 2; i < mask.Length; i += 3)
            //    if (t[i] != '/')
            //        t = t.Insert(i, "/");
            //tb.Text = t.Substring(0, mask.Length);
            //tb.SelectionStart = p;
        }

        private string phone_input(string text)
        {
            return null;
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

        private void ComboBoxControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//make text left alignment
            ComboBox cb = (ComboBox)sender;
            TextBox tb = cb.GetVisualChild<TextBox>();
            if (tb == null)//can be so due to asynchronous building
                return;
            if (e.AddedItems.Count < 1)
                return;
            ThreadRoutines.StartTry(() => {
                DateTime end = DateTime.Now.AddMilliseconds(200);
                while (end > DateTime.Now)
                {
                    System.Threading.Thread.Sleep(20);
                    tb.Dispatcher.Invoke(() =>
                    {
                        tb.Select(0, tb.Text.Length);
                        tb.ScrollToHome();
                    });
                }
            });
        }
    }
}
