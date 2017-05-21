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
//using System.Windows.Shapes;
using System.Net.Http;
using System.Net;
using System.IO;
using System.Management;
using System.Threading;
using System.Reflection;

namespace Cliver.Foreclosures
{
    public partial class ViewWindow : Window
    {
        public static void OpenDialog()
        {
            var w = new ViewWindow();
            System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(w);
            w.ShowDialog();
        }

        ViewWindow()
        {
            InitializeComponent();

            Icon = AssemblyRoutines.GetAppIconImageSource();

            Closing += delegate (object sender, System.ComponentModel.CancelEventArgs e)
            {
            };

            Closed += delegate
            {
            };

            Columns.Items.Clear();
            foreach (PropertyInfo pi in typeof(Db.Foreclosure).GetProperties())
            {
                CheckBox i = new CheckBox();
                i.Content = pi.Name;
                i.IsChecked = Settings.View.ShowedColumns.Contains(pi.Name);
                Columns.Items.Add(i);
            }
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Settings.View.ShowedColumns.Clear();
                foreach (CheckBox i in Columns.Items)
                    if (i.IsChecked == true)
                        Settings.View.ShowedColumns.Add(i.Content.ToString());

                Settings.View.Save();
                Config.Reload();

                Close();
            }
            catch (Exception ex)
            {
                Message.Exclaim(ex.Message);
            }
        }
    }
}