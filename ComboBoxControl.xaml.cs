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

namespace Cliver.Foreclosures
{
    /// <summary>
    /// Interaction logic for ComboBoxControl.xaml
    /// </summary>
    public partial class ComboBoxControl : ComboBox
    {
        public ComboBoxControl()
        {
            InitializeComponent();

            SelectionChanged += ComboBoxControl_SelectionChanged;

            
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

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {

        }
    }
}
