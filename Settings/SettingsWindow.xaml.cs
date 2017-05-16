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
            if (w == null || !w.IsLoaded)
            {
                w = new SettingsWindow();
                System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(w);
            }
            w.Show();
        }
        static SettingsWindow w = null;

        SettingsWindow()
        {
            InitializeComponent();

            Icon = AssemblyRoutines.GetAppIconImageSource();

            Closing += delegate (object sender, System.ComponentModel.CancelEventArgs e)
            {
            };

            Closed += delegate
            {
                w = null;
            };

            //Dictionary<string, int> texts2period = new Dictionary<string, int> {
            //    { "1 day", 24 * 60 * 60 },
            //    { "3 days", 3 * 24 * 60 * 60 },
            //    { "7 days", 7*24 * 60 * 60 },
            //    { "Never", -1 }
            //};
            if (Settings.General.DbRefreshPeriodInSecs > 0)
            {
                DbRefreshPeriodInDays.Text = ((float)Settings.General.DbRefreshPeriodInSecs / (24 * 60 * 60)).ToString();
                DoRefresh.IsChecked = true;
            }
            else
                DoRefresh.IsChecked = false;
            DoRefresh_Checked(null, null);

            if (Settings.General.DbRefreshRetryPeriodInSecs > 0)
            {
                DbRefreshRetryPeriodInSecs.Text = Settings.General.DbRefreshRetryPeriodInSecs.ToString();
                DoRefreshRetry.IsChecked = true;
            }
            else
                DoRefreshRetry.IsChecked = false;
            DoRefreshRetry_Checked(null, null);

            NextDbRefreshTime.Value = Settings.General.NextDbRefreshTime;

            UserName.Text = Settings.General.UserName;
            if (!string.IsNullOrWhiteSpace(Settings.General.EncryptedPassword))
                Password.Password = Settings.General.Decrypt(Settings.General.EncryptedPassword);

            County.Items.Clear();
            foreach (string c in Db.GetCounties())
                County.Items.Add(c);
            County.SelectedItem = Settings.General.County;
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
                    float days = float.Parse(DbRefreshPeriodInDays.Text);
                    int secs = (int)(days * 24 * 60 * 60);
                    if (secs <= 0)
                        throw new Exception("Db Refresh Period must be positive.");
                    if (secs > Int32.MaxValue)
                        throw new Exception("Db Refresh Period is too big.");
                    Settings.General.DbRefreshPeriodInSecs = secs;
                }
                else
                    Settings.General.DbRefreshPeriodInSecs = -1;

                if (DoRefreshRetry.IsChecked == true)
                {
                    int secs = int.Parse(DbRefreshRetryPeriodInSecs.Text);
                    if (secs <= 0)
                        throw new Exception("Db Refresh Retry Period must be positive.");
                    if (secs > Int32.MaxValue)
                        throw new Exception("Db Refresh Retry Period is too big.");
                    Settings.General.DbRefreshRetryPeriodInSecs = secs;
                }
                else
                    Settings.General.DbRefreshRetryPeriodInSecs = -1;

                if (NextDbRefreshTime.Value == null)
                    throw new Exception("Next Db Refresh Time is not set.");
                Settings.General.NextDbRefreshTime = (DateTime)NextDbRefreshTime.Value;

                if (string.IsNullOrWhiteSpace(UserName.Text))
                    throw new Exception("User Name is not set.");
                Settings.General.UserName = UserName.Text;
                if (string.IsNullOrWhiteSpace(Password.Password))
                    throw new Exception("Password is not set.");
                Settings.General.EncryptedPassword = Settings.General.Encrypt(Password.Password);

                if (County.SelectedItem == null)
                    throw new Exception("No county chosen.");
                Settings.General.County = County.SelectedItem.ToString();

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

        private void DoRefresh_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized)
                return;
            lDbRefreshPeriodInDays.Visibility = DoRefresh.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            DbRefreshPeriodInDays.Visibility = DoRefresh.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            DoRefreshRetry.Visibility = DoRefresh.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            DoRefreshRetry_Checked(null, null);
            lNextDbRefreshTime.Visibility = DoRefresh.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            NextDbRefreshTime.Visibility = DoRefresh.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private void DoRefreshRetry_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized)
                return;
            lDbRefreshRetryPeriodInSecs.Visibility = DoRefresh.IsChecked == true && DoRefreshRetry.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            DbRefreshRetryPeriodInSecs.Visibility = DoRefresh.IsChecked == true && DoRefreshRetry.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
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