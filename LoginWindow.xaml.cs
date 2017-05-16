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
    public partial class LoginWindow : Window
    {
        public static void OpenDialog()
        {
            var w = new LoginWindow();
            System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(w);
            w.ShowDialog();
        }

        LoginWindow()
        {
            InitializeComponent();

            Icon = AssemblyRoutines.GetAppIconImageSource();

            Closing += delegate (object sender, System.ComponentModel.CancelEventArgs e)
            {
            };

            Closed += delegate
            {
            };

            UserName.Text = Settings.General.UserName;
            if (!string.IsNullOrWhiteSpace(Settings.General.EncryptedPassword))
                Password.Password = Settings.General.Decrypt(Settings.General.EncryptedPassword);
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(UserName.Text))
                    throw new Exception("User Name is not set.");
                Settings.General.UserName = UserName.Text;
                if (string.IsNullOrWhiteSpace(Password.Password))
                    throw new Exception("Password is not set.");
                Settings.General.EncryptedPassword = Settings.General.Encrypt(Password.Password);

                Settings.General.Save();
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