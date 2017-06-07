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
//using System.Windows.Shapes;
using System.Net.Http;
using System.Net;
using System.IO;
using System.Management;
using System.Threading;

namespace Cliver.Foreclosures
{
    public partial class LocationWindow : Window
    {
        public static void OpenDialog()
        {
            var w = new LocationWindow();
            System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(w);
            w.ShowDialog();
        }

        LocationWindow()
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

            set(Settings.Location);
        }

        void set(Settings.LocationSettings s)
        {
            County.Items.Clear();
            foreach (Db.County c in (new Db.Counties()).GetAll())
                County.Items.Add(c.county);
            County.SelectedItem = s.County;
        }

        private void reset_Click(object sender, RoutedEventArgs e)
        {
            set(Settings.Location.GetResetInstance<Settings.LocationSettings>());
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (County.SelectedItem == null)
                    throw new Exception("No county chosen.");
                Settings.Location.County = County.SelectedItem.ToString();

                Settings.Location.Save();
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