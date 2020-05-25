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

namespace Cliver.Probidder
{
    /// <summary>
    /// Interaction logic for ForeclosuresControl.xaml
    /// </summary>
    public partial class ForeclosuresControl : DataGrid
    {
        public ForeclosuresControl()
        {
            InitializeComponent();
        }

        public delegate void OnOpenClick(object sender, RoutedEventArgs e);
        public event OnOpenClick OpenClick;
        private void open_Click(object sender, RoutedEventArgs e)
        {
            OpenClick?.Invoke(sender, e);
        }

        public delegate void OnDeleteClick(object sender, RoutedEventArgs e);
        public event OnDeleteClick DeleteClick;
        private void delete_Click(object sender, RoutedEventArgs e)
        {
            DeleteClick?.Invoke(sender, e);
        }

        protected override void OnCanExecuteBeginEdit(System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }
    }
}
