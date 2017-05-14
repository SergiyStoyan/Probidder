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
        public AuctionWindow(int? foreclosure_id = null)
        {
            InitializeComponent();
            
            Icon = AssemblyRoutines.GetAppIconImageSource();

            this.foreclosure_id = foreclosure_id;
            if (foreclosure_id != null)
            {
                fields.IsEnabled = false;
                Save.Visibility = Visibility.Collapsed;
                Delete.Visibility = Visibility.Collapsed;
                Edit.Visibility = Visibility.Visible;
            }

            COUNTY.Items.Clear();
            foreach (string c in Db.GetCounties())
                COUNTY.Items.Add(c);

            TYPE_OF_MO.Items.Clear();
            foreach (string c in Db.GetValuesFromTable("mortgage_types", "mortgage_type", new Dictionary<string, string>() { }))
                TYPE_OF_MO.Items.Add(c);

            PROP_DESC.Items.Clear();
            foreach (string c in Db.GetPropertyCodes())
                PROP_DESC.Items.Add(c);

            OWNER_ROLE.Items.Clear();
            foreach (string c in Db.GetOwnerRoles())
                OWNER_ROLE.Items.Add(c);

            if (foreclosure_id != null)
                set_foreclosure(foreclosures.GetById((int)foreclosure_id));
            else
                set_foreclosure(null);

            Closed += delegate
            {
                foreclosures.Dispose();
            };
        }
        int? foreclosure_id = null;
        Db.Foreclosures foreclosures = new Db.Foreclosures();

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (!Message.YesNo("The entry is about deletion. Are you sure to proceed?"))
                return;
            if (foreclosure_id != null)
            {
                foreclosures.Delete((int)foreclosure_id);
                ListWindow.ItemDeleted((int)foreclosure_id);
            }
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (fields.IsEnabled)
                Close();
            else
            {
                Edit.Visibility = Visibility.Visible;
                Delete.Visibility = Visibility.Collapsed;
                Save.Visibility = Visibility.Collapsed;
                fields.IsEnabled = false;
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            Edit.Visibility = Visibility.Collapsed;
            Save.Visibility = Visibility.Visible;
            Delete.Visibility = Visibility.Visible;
            fields.IsEnabled = true;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            //if (string.IsNullOrWhiteSpace(subject.Text))
            //    throw new Exception("Subject is empty.");
            //if (string.IsNullOrWhiteSpace(this.description.Text))
            //    throw new Exception("Description is empty    

            Db.Foreclosures.Foreclosure f = new Db.Foreclosures.Foreclosure();
            if (foreclosure_id != null)
                f.Id = (int)foreclosure_id;
            f.TYPE_OF_EN = TYPE_OF_EN.Text;
            f.COUNTY = COUNTY.Text;
            f.CASE_N = CASE_N.Text;
            f.FILING_DATE = FILING_DATE.Text;
            f.ENTRY_DATE = ENTRY_DATE.Text;
            f.LENDOR = LENDOR.Text;
            f.ORIGINAL_MTG = ORIGINAL_MTG.Text;
            f.DOCUMENT_N = DOCUMENT_N.Text;
            f.ORIGINAL_I = ORIGINAL_I.Text;
            f.LEGAL_D = LEGAL_D.Text;
            f.ADDRESS = ADDRESS.Text;
            f.CITY = CITY.Text;
            f.ZIP = ZIP.Text;
            f.PIN = PIN.Text;
            f.DATE_OF_CA = DATE_OF_CA.Text;
            f.LAST_PAY_DATE = LAST_PAY_DATE.Text;
            f.BALANCE_DU = BALANCE_DU.Text;
            f.PER_DIEM_I = PER_DIEM_I.Text;
            f.CURRENT_OW = CURRENT_OW.Text;
            f.IS_ORG = IS_ORG.IsChecked == true;
            f.DECEASED = DECEASED.IsChecked == true;
            f.OWNER_ROLE = OWNER_ROLE.Text;
            f.OTHER_LIENS = OTHER_LIENS.Text;
            f.ADDL_DEF = ADDL_DEF.Text;
            f.PUB_COMMENTS = PUB_COMMENTS.Text;
            f.INT_COMMENTS = INT_COMMENTS.Text;
            f.ATTY = ATTY.Text;
            f.ATTORNEY_S = ATTORNEY_S.Text;
            f.TYPE_OF_MO = TYPE_OF_MO.Text;
            f.INTEREST_R = INTEREST_R.Text;
            f.PROP_DESC = PROP_DESC.Text;
            f.MONTHLY_PAY = MONTHLY_PAY.Text;
            f.TERM_OF_MTG = TERM_OF_MTG.Text;
            f.DEF_ADDRESS = DEF_ADDRESS.Text;
            f.DEF_PHONE = DEF_PHONE.Text;
            foreclosure_id = foreclosures.Save(f);

            ListWindow.ItemSaved(f);
            Close();
        }

        private void County_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CITY.Items.Clear();
            foreach (string c in Db.GetValuesFromTable("cities", "city", new Dictionary<string, string>() { { "county", (string)COUNTY.SelectedItem } }))
                CITY.Items.Add(c);

            LENDOR.Items.Clear();
            foreach (string c in Db.GetValuesFromTable("plaintiffs", "plaintiff", new Dictionary<string, string>() { { "county", (string)COUNTY.SelectedItem } }))
                LENDOR.Items.Add(c);

            ATTY.Items.Clear();
            foreach (string c in Db.GetValuesFromTable("attorneys", "attorney", new Dictionary<string, string>() { { "county", (string)COUNTY.SelectedItem } }))
                ATTY.Items.Add(c);
        }

        private void City_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ZIP.Items.Clear();
            foreach (string c in Db.GetZipCodes((string)COUNTY.SelectedItem, (string)CITY.SelectedItem))
                ZIP.Items.Add(c);
        }

        private void ATTY_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ATTORNEY_S.Items.Clear();
            foreach (string c in Db.GetValuesFromTable("attorney_phones", "attorney_phone", new Dictionary<string, string>() { { "county", (string)COUNTY.SelectedItem }, { "attorney", (string)ATTY.SelectedItem } }))
                ATTORNEY_S.Items.Add(c);
            if (ATTORNEY_S.Items.Count == 1)
                ATTORNEY_S.SelectedIndex = 0;
        }

        void set_foreclosure(Db.Foreclosures.Foreclosure f)
        {
            if (f == null)
            {
                TYPE_OF_EN.Text = null;
                COUNTY.Text = null;
                CASE_N.Text = null;
                FILING_DATE.Text = DateTime.Now.ToString();
                ENTRY_DATE.Text = DateTime.Now.ToString();
                LENDOR.Text = null;
                ORIGINAL_MTG.Text = null;
                DOCUMENT_N.Text = null;
                ORIGINAL_I.Text = null;
                LEGAL_D.Text = null;
                ADDRESS.Text = null;
                CITY.Text = null;
                ZIP.Text = null;
                PIN.Text = null;
                DATE_OF_CA.Text = null;
                LAST_PAY_DATE.Text = null;
                BALANCE_DU.Text = null;
                PER_DIEM_I.Text = null;
                CURRENT_OW.Text = null;
                IS_ORG.IsChecked = false;
                DECEASED.IsChecked = false;
                OWNER_ROLE.Text = null;
                OTHER_LIENS.Text = null;
                ADDL_DEF.Text = null;
                PUB_COMMENTS.Text = null;
                INT_COMMENTS.Text = null;
                ATTY.Text = null;
                ATTORNEY_S.Text = null;
                TYPE_OF_MO.Text = null;
                INTEREST_R.Text = null;
                PROP_DESC.Text = null;
                MONTHLY_PAY.Text = null;
                TERM_OF_MTG.Text = null;
                DEF_ADDRESS.Text = null;
                DEF_PHONE.Text = null;

                f = foreclosures.GetLast();
                if (f != null)
                {
                    COUNTY.SelectedItem = f.COUNTY;
                    TYPE_OF_MO.SelectedItem = f.TYPE_OF_MO;
                }

                return;
            }
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