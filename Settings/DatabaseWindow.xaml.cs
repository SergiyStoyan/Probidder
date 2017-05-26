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
    public partial class DatabaseWindow : Window
    {
        public static void OpenDialog()
        {
            var w = new DatabaseWindow();
            System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(w);
            w.ShowDialog();
        }

        DatabaseWindow()
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

            Loaded += delegate
            {
                //SizeToContent = SizeToContent.Manual;
            };

            if (Settings.Database.RefreshPeriodInSecs > 0)
            {
                RefreshPeriodInDays.Text = ((float)Settings.Database.RefreshPeriodInSecs / (24 * 60 * 60)).ToString();
                DoRefresh.IsChecked = true;
            }
            else
                DoRefresh.IsChecked = false;
            DoRefresh_Checked(null, null);

            if (Settings.Database.RefreshRetryPeriodInSecs > 0)
            {
                RefreshRetryPeriodInSecs.Text = Settings.Database.RefreshRetryPeriodInSecs.ToString();
                DoRefreshRetry.IsChecked = true;
            }
            else
                DoRefreshRetry.IsChecked = false;
            DoRefreshRetry_Checked(null, null);

            NextRefreshTime.Value = Settings.Database.NextRefreshTime;
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DoRefresh.IsChecked == true)
                {
                    float days = float.Parse(RefreshPeriodInDays.Text);
                    int secs = (int)(days * 24 * 60 * 60);
                    if (secs <= 0)
                        throw new Exception("Db Refresh Period must be positive.");
                    if (secs > Int32.MaxValue)
                        throw new Exception("Db Refresh Period is too big.");
                    Settings.Database.RefreshPeriodInSecs = secs;
                }
                else
                    Settings.Database.RefreshPeriodInSecs = -1;

                if (DoRefreshRetry.IsChecked == true)
                {
                    int secs = int.Parse(RefreshRetryPeriodInSecs.Text);
                    if (secs <= 0)
                        throw new Exception("Db Refresh Retry Period must be positive.");
                    if (secs > Int32.MaxValue)
                        throw new Exception("Db Refresh Retry Period is too big.");
                    Settings.Database.RefreshRetryPeriodInSecs = secs;
                }
                else
                    Settings.Database.RefreshRetryPeriodInSecs = -1;

                if (NextRefreshTime.Value == null)
                    throw new Exception("Next Db Refresh Time is not set.");
                Settings.Database.NextRefreshTime = (DateTime)NextRefreshTime.Value;

                Settings.Database.Save();
                Config.Reload();

                Close();
            }
            catch (Exception ex)
            {
                Message.Exclaim(ex.Message);
            }
        }

        private void DoRefresh_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized)
                return;
            lDbRefreshPeriodInDays.Visibility = DoRefresh.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            RefreshPeriodInDays.Visibility = DoRefresh.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            DoRefreshRetry.Visibility = DoRefresh.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            DoRefreshRetry_Checked(null, null);
            lNextDbRefreshTime.Visibility = DoRefresh.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            NextRefreshTime.Visibility = DoRefresh.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private void DoRefreshRetry_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized)
                return;
            lDbRefreshRetryPeriodInSecs.Visibility = DoRefresh.IsChecked == true && DoRefreshRetry.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            RefreshRetryPeriodInSecs.Visibility = DoRefresh.IsChecked == true && DoRefreshRetry.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private void DoRefresh_Unchecked(object sender, RoutedEventArgs e)
        {
            DoRefresh_Checked(sender, e);
        }

        private void DoRefreshRetry_Unchecked(object sender, RoutedEventArgs e)
        {
            DoRefreshRetry_Checked(sender, e);
        }
    }
}