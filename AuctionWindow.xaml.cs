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
    public partial class AuctionWindow : Window
    {
        public AuctionWindow()
        {
            InitializeComponent();

            Icon = AssemblyRoutines.GetAppIconImageSource();

            //WindowStartupLocation = WindowStartupLocation.CenterScreen;

            //string temp_dir = Path.GetTempPath() + "\\" + ProgramRoutines.GetAppName();
            //DateTime delete_time = DateTime.Now.AddDays(-3);
            //foreach (FileInfo fi in (new DirectoryInfo(temp_dir)).GetFiles())
            //    if (fi.LastWriteTime < delete_time)
            //        try
            //        {
            //            fi.Delete();
            //        }
            //        catch { }

           // HttpClientHandler handler = new HttpClientHandler();
            //handler.Credentials = new System.Net.NetworkCredential(Settings.General.ZendeskUser, Settings.General.ZendeskPassword);
            //http_client = new HttpClient(handler);

            Closing += delegate (object sender, System.ComponentModel.CancelEventArgs e)
            {
                //if (Message.YesNo("Posting the ticket is in progress. Do you want to cancel it?"))
                //{
                //    create_ticket_t = null;
                //    http_client.CancelPendingRequests();
                //    Log.Main.Inform("Cancelling...");
                //}
                //e.Cancel = true;
            };

            Closed += delegate
            {
                //http_client.CancelPendingRequests();
            };

            foreach (string c in Settings.General.Counties)
                County.Items.Add(c);
            County.SelectedItem = Settings.General.County;
        }
               
        private void Window_Drop(object sender, DragEventArgs e)
        {
            //error.Visibility = Visibility.Collapsed;
            //if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            //    return;
            //foreach (string file in (string[])e.Data.GetData(DataFormats.FileDrop))
            //    add_attachment(file);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            //if (string.IsNullOrWhiteSpace(subject.Text))
            //    throw new Exception("Subject is empty.");
            //if (string.IsNullOrWhiteSpace(this.description.Text))
            //    throw new Exception("Description is empty

        }

        private void County_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (string c in Db.GetValuesFromTable("cities", "city", new Dictionary<string, string>() { { "county", (string)County.SelectedItem }}))
                City.Items.Add(c);
            City.SelectedItem = Settings.General.City;
        }

        private void City_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (string c in Db.GetZipCodes((string)County.SelectedItem , (string)City.SelectedItem))
                ZipCode.Items.Add(c);
            ZipCode.SelectedItem = Settings.General.ZipCode;
        }

        private void ZipCode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        void fill_cities()
        {
        }
    }
}
