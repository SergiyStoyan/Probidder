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

            foreach (string c in Settings.Default.Counties)
                County.Items.Add(c);
            County.SelectedItem = Settings.Default.County;

            foreach (string c in Db.GetValuesFromTable("mortgage_types", "mortgage_type", new Dictionary<string, string>() { }))
                MortgageType.Items.Add(c);
            MortgageType.SelectedItem = Settings.Default.MortgageType;
        }
               
        private void Window_Drop(object sender, DragEventArgs e)
        {
            //error.Visibility = Visibility.Collapsed;
            //if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            //    return;
            //foreach (string file in (string[])e.Data.GetData(DataFormats.FileDrop))
            //    add_attachment(file);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Forward_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            //if (string.IsNullOrWhiteSpace(subject.Text))
            //    throw new Exception("Subject is empty.");
            //if (string.IsNullOrWhiteSpace(this.description.Text))
            //    throw new Exception("Description is empty

            Settings.Default.County = (string)County.SelectedItem;
            Settings.Default.MortgageType = (string)MortgageType.SelectedItem;
            Settings.Default.City = (string)City.SelectedItem;
            Settings.Default.Plaintiff = (string)Plaintiff.SelectedItem;
            Settings.Default.Attorney = (string)Attorney.SelectedItem;
            Settings.Default.AttorneyPhone = (string)AttorneyPhone.SelectedItem;
            Settings.Default.Save();
        }

        private void County_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (string c in Db.GetValuesFromTable("cities", "city", new Dictionary<string, string>() { { "county", (string)County.SelectedItem } }))
                City.Items.Add(c);
            City.SelectedItem = Settings.Default.City;

            foreach (string c in Db.GetValuesFromTable("plaintiffs", "plaintiff", new Dictionary<string, string>() { { "county", (string)County.SelectedItem } }))
                Plaintiff.Items.Add(c);
            Plaintiff.SelectedItem = Settings.Default.Plaintiff;

            foreach (string c in Db.GetValuesFromTable("attorneys", "attorney", new Dictionary<string, string>() { { "county", (string)County.SelectedItem } }))
                Attorney.Items.Add(c);
            Attorney.SelectedItem = Settings.Default.Attorney;

            foreach (string c in Db.GetValuesFromTable("attorney_phones", "attorney_phone", new Dictionary<string, string>() { { "county", (string)County.SelectedItem }, { "attorney", (string)Attorney.SelectedItem } }))
                AttorneyPhone.Items.Add(c);
            AttorneyPhone.SelectedItem = Settings.Default.AttorneyPhone;
        }

        private void City_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (string c in Db.GetZipCodes((string)County.SelectedItem , (string)City.SelectedItem))
                ZipCode.Items.Add(c);
            ZipCode.SelectedItem = Settings.Default.ZipCode;
        }

        private void ZipCode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        void set_foreclosure(Db.Foreclosures.Foreclosure f)
        {
            TYPE_OF_EN.Text = f.TYPE_OF_EN;
            COUNTY.Text = f.COUNTY;
            CASE_N.Text = f.CASE_N;
            FILING_DATE.Text = f.FILING_DATE;
            ENTRY_DATE.Text = f.ENTRY_DATE;
            LENDOR.Text = f.LENDOR;
            ORIGINAL_MTG.Text = f.ORIGINAL_MTG;
            DOCUMENT_N.Text = f.DOCUMENT_N;
            ORIGINAL_I.Text = f.ORIGINAL_I;
            LEGAL_D.Text = f.LEGAL_D;
            ADDRESS.Text = f.ADDRESS;
            CITY.Text = f.CITY;
            ZIP.Text = f.ZIP;
            PIN.Text = f.PIN;
            DATE_OF_CA.Text = f.DATE_OF_CA;
            LAST_PAY_DATE.Text = f.LAST_PAY_DATE;
            BALANCE_DU.Text = f.BALANCE_DU;
            PER_DIEM_I.Text = f.PER_DIEM_I;
            CURRENT_OW.Text = f.CURRENT_OW;
            IS_ORG.IsChecked = f.IS_ORG;
            DECEASED.IsChecked = f.DECEASED;
            OWNER_ROLE.Text = f.OWNER_ROLE;
            OTHER_LIENS.Text = f.OTHER_LIENS;
            ADDL_DEF.Text = f.ADDL_DEF;
            PUB_COMMENTS.Text = f.PUB_COMMENTS;
            INT_COMMENTS.Text = f.INT_COMMENTS;
            ATTY.Text = f.ATTY;
            ATTORNEY_S.Text = f.ATTORNEY_S;
            TYPE_OF_MO.Text = f.TYPE_OF_MO;
            INTEREST_R.Text = f.INTEREST_R;
            PROP_DESC.Text = f.PROP_DESC;
            MONTHLY_PAY.Text = f.MONTHLY_PAY;
            TERM_OF_MTG.Text = f.TERM_OF_MTG;
            DEF_ADDRESS.Text = f.DEF_ADDRESS;
            DEF_PHONE.Text = f.DEF_PHONE;
        }
    }
}
