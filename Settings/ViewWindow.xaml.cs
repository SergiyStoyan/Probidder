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

            ContentRendered += delegate
            {
                WpfRoutines.TrimWindowSize(this);
            };

            list.ItemsSource = typeof(Db.Foreclosure).GetProperties().Select(x => new Item
            {
                Show = Settings.View.ShowedColumns.Contains(x.Name),
                Search = Settings.View.SearchedColumns.Contains(x.Name),
                Column = x.Name
            }
            ).ToList();
        }
        class Item
        {
            public bool Show { set; get; }
            public bool Search { set; get; }
            public string Column { set; get; }
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
                Settings.View.SearchedColumns.Clear();
                foreach (Item i in list.ItemsSource)
                {
                    if (i.Show)
                        Settings.View.ShowedColumns.Add(i.Column);
                    if (i.Search)
                        Settings.View.SearchedColumns.Add(i.Column);
                }

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