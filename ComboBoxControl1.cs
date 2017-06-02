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
    public class ComboBoxControl1 : ComboBox
    {
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            tb = this.FindVisualChildrenOfType<TextBox>().Where(x => x.Name == "PART_EditableTextBox").First();
            tb.TextChanged += tb_TextChanged;
        }
        TextBox tb;

        private void tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            select(tb.SelectionStart, tb.SelectionLength);
        }

        public ComboBoxControl1()
        {       
        }

        void select(int index, int length)
        {
            tb.BeginChange();
            tb.SelectionStart = index;
            tb.SelectionLength = length;
            tb.ScrollToHome();
            tb.EndChange();
        }        
    }
}
