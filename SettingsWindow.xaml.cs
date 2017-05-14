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
    public partial class SettingsWindow : Window
    {
        public static void Open()
        {
            if (lw == null || !lw.IsLoaded)
                lw = new SettingsWindow();
            lw.Show();
        }
        static SettingsWindow lw = null;

        SettingsWindow()
        {
            InitializeComponent();

            Icon = AssemblyRoutines.GetAppIconImageSource();
            
            Closing += delegate (object sender, System.ComponentModel.CancelEventArgs e)
            {
            };

            Closed += delegate
            {
                lw = null;
            };

            DbRefreshPeriodInSecs.Text = Settings.General.DbRefreshPeriodInSecs.ToString();
            DbRefreshRetryPeriodInSecs.Text = Settings.General.DbRefreshRetryPeriodInSecs.ToString();
            NextDbRefreshTime.Value = Settings.General.NextDbRefreshTime;
            
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int secs = int.Parse(DbRefreshPeriodInSecs.Text);
                if (secs <= 0)
                    throw new Exception("Db Refresh Period must be positive.");
                if (secs > Int32.MaxValue)
                    throw new Exception("Db Refresh Period is too big.");
                Settings.General.DbRefreshPeriodInSecs = secs;

                secs = int.Parse(DbRefreshRetryPeriodInSecs.Text);
                if (secs <= 0)
                    throw new Exception("Db Refresh Retry Period must be positive.");
                if (secs > Int32.MaxValue)
                    throw new Exception("Db Refresh Retry Period is too big.");
                Settings.General.DbRefreshRetryPeriodInSecs = secs;

                if (NextDbRefreshTime.Value == null)
                    throw new Exception("Next Db Refresh Time is not set.");
                Settings.General.NextDbRefreshTime = (DateTime)NextDbRefreshTime.Value;

                Settings.General.Save();
                Config.Reload();

                Close();

                //bool running = Service.Running;
                //Service.Running = false;
                //Service.Running = running;
            }
            catch (Exception ex)
            {
                Message.Exclaim(ex.Message);
            }
        }
    }
}