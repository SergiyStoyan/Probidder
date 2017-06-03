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

namespace Cliver.Foreclosures
{
    public partial class NumberBoxControl : TextBox
    {
        public NumberBoxControl()
        {
            InitializeComponent();

            PreviewTextInput += TextBox_PreviewTextInput;
        }

        public uint MaxFractionChars
        {
            get { return (uint)GetValue(MaxFractionCharsProperty); }
            set { SetValue(MaxFractionCharsProperty, value); }
        }
        
        public static readonly DependencyProperty MaxFractionCharsProperty = DependencyProperty.Register(
                  "MaxFractionChars",
                  typeof(uint),
                  typeof(NumberBoxControl)
              );

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            bool r;
            if (MaxFractionChars > 0)
                r = Regex.IsMatch(e.Text, @"[^\d\.\,]");
            else
                r = Regex.IsMatch(e.Text, @"[^\d\,]");
            if (r)
            {
                Console.Beep(5000, 200);
                e.Handled = true;
            }
        }
    }
}