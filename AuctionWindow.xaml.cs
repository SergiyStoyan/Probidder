using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        public static AuctionWindow OpenNew(int? foreclosure_id = null)
        {
            AuctionWindow w = new AuctionWindow(foreclosure_id);
            System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(w);
            w.Show();
            return w;
        }

        AuctionWindow(int? foreclosure_id = null)
        {
            InitializeComponent();

            Icon = AssemblyRoutines.GetAppIconImageSource();

            //System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.CreateSpecificCulture(System.Globalization.CultureInfo.CurrentCulture.Name);
            //ci.DateTimeFormat.ShortDatePattern = "MMddyy";
            //ci.DateTimeFormat.LongDatePattern = "MMddyy";
            //Thread.CurrentThread.CurrentCulture = ci;
            
            if (foreclosure_id != null)
            {
                fields.IsEnabled = false;
                Save.Visibility = Visibility.Collapsed;
                Delete.Visibility = Visibility.Collapsed;
                Edit.Visibility = Visibility.Visible;
            }

            COUNTY.Text = Settings.Location.County;

            CASE_N.Items.Clear();
            foreach (string c in (new Db.CaseNumbers()).GetBy(Settings.Location.County).case_ns)
                CASE_N.Items.Add(c);

            CITY.Items.Clear();
            foreach (Db.City c in (new Db.Cities()).GetBy(Settings.Location.County))
                CITY.Items.Add(c.city);

            LENDOR.Items.Clear();
            foreach (Db.Plaintiff c in (new Db.Plaintiffs()).GetBy(Settings.Location.County))
                LENDOR.Items.Add(c.plaintiff);

            ATTY.Items.Clear();
            foreach (Db.Attorney c in (new Db.Attorneys()).GetBy(Settings.Location.County))
                ATTY.Items.Add(c.attorney);

            TYPE_OF_MO.Items.Clear();
            foreach (Db.MortgageType c in (new Db.MortgageTypes()).Get())
                TYPE_OF_MO.Items.Add(c.mortgage_type);

            PROP_DESC.Items.Clear();
            foreach (Db.PropertyCode c in (new Db.PropertyCodes()).GetAll())
                PROP_DESC.Items.Add(c.type);

            OWNER_ROLE.Items.Clear();
            foreach (Db.OwnerRole c in (new Db.OwnerRoles()).GetAll())
                OWNER_ROLE.Items.Add(c.role);

            Db.Foreclosure f;
            if (foreclosure_id != null)
                f = foreclosures.GetById((int)foreclosure_id);
            else
                f = new Db.Foreclosure();
            fields.DataContext = f;

            Closed += delegate
            {
                foreclosures.Dispose();
            };
        }
        Db.Foreclosures foreclosures = new Db.Foreclosures();

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (!Message.YesNo("The entry is about deletion. Are you sure to proceed?"))
                return;
            Db.Foreclosure f = (Db.Foreclosure)fields.DataContext;
            if (f.Id != 0)
                foreclosures.Delete(f.Id);
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
            //{
            //    Edit.Visibility = Visibility.Visible;
            //    Delete.Visibility = Visibility.Collapsed;
            //    Save.Visibility = Visibility.Collapsed;
            //    fields.IsEnabled = false;
            //}
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            Edit.Visibility = Visibility.Collapsed;
            Save.Visibility = Visibility.Visible;
            Delete.Visibility = Visibility.Visible;
            fields.IsEnabled = true;
        }

        private void Prev_Click(object sender, RoutedEventArgs e)
        {
        }
        private void Next_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            //if (string.IsNullOrWhiteSpace(subject.Text))
            //    throw new Exception("Subject is empty.");
            //if (string.IsNullOrWhiteSpace(this.description.Text))
            //    throw new Exception("Description is empty 
            Db.Foreclosure f = (Db.Foreclosure)fields.DataContext;
            foreclosures.Save(f);
            
            fields.DataContext = new Db.Foreclosure();
            Save.IsEnabled = false;
            Delete.IsEnabled = false;
            //Close();
        }

        private void City_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ZIP.Items.Clear();
            //foreach (string c in Db.GetValuesFromCsvTable("illinois_postal_codes", "postalcode", new Dictionary<string, string> { { "county", Settings.General.County }, { "placename", (string)CITY.SelectedItem } }))
            //ZIP.Items.Add(c);
            foreach (Db.Zip c in (new Db.Zips()).GetBy(Settings.Location.County, (string)CITY.SelectedItem))
                ZIP.Items.Add(c.zip);
        }

        private void ATTY_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ATTORNEY_S.Items.Clear();
            //foreach (string c in Db.GetValuesFromJsonTable("attorney_phones", "attorney_phone", new Dictionary<string, string>() { { "county", Settings.General.County }, { "attorney", (string)ATTY.SelectedItem } }))
            //ATTORNEY_S.Items.Add(c);
            foreach (Db.AttorneyPhone c in (new Db.AttorneyPhones()).GetBy((string)ATTY.SelectedItem, Settings.Location.County))
                ATTORNEY_S.Items.Add(c.attorney_phone);

            Db.Foreclosure f = (Db.Foreclosure)fields.DataContext;
            if (f.Id == 0)
                if (ATTORNEY_S.Items.Count == 1)
                    ATTORNEY_S.SelectedIndex = 0;
        }

        private void fields_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Db.Foreclosure f = (Db.Foreclosure)e.NewValue;
            if (f.Id == 0)
            {
                f.TYPE_OF_EN = "CHA";
                //f.CASE_N = 
                f.OWNER_ROLE = "OWNER";
                f.TYPE_OF_MO = "CNV";
                f.PROP_DESC = "SINGLE FAMILY";
                f.TERM_OF_MTG = 30;
                f.COUNTY = Settings.Location.County;
                f.FILING_DATE = DateTime.Now;
                f.ENTRY_DATE = DateTime.Now;
                f.IS_ORG = false;
                f.DECEASED = false;

                return;
            }
            //DATE_OF_CA.SelectedDate = f.DATE_OF_CA;
            //LAST_PAY_DATE.SelectedDate = f.LAST_PAY_DATE;
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (fields.IsEnabled)
            {
                Save.IsEnabled = true;
                Delete.IsEnabled = true;
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Window_PreviewMouseDown(null, null);
        }

        private DateTime? calendar_input(string text)
        {
            if (text.Length > 6 || Regex.IsMatch(text, @"[^\d]"))
                return null;
            Match m = Regex.Match(text, @"(\d{2})(\d{2})(\d{2})");
            if (!m.Success)
                return null;
            try
            {
                int y = int.Parse(m.Groups[3].Value);
                if (y < 30)
                    y += 2000;
                else
                    y += 1900;
                return new DateTime(y, int.Parse(m.Groups[1].Value), int.Parse(m.Groups[2].Value));
            }
            catch
            {
                return null;
            }
        }

        void set_DatePicker(DatePicker dp, string date)
        {
            var h = calendar_input(date);
            if (h != null)
                dp.SelectedDate = h;
        }

        private void ENTRY_DATE_TextChanged(object sender, TextChangedEventArgs e)
        {
            set_DatePicker(ENTRY_DATE, ((TextBox)sender).Text);
        }

        private void DATE_OF_CA_TextChanged(object sender, TextChangedEventArgs e)
        {
            set_DatePicker(DATE_OF_CA, ((TextBox)sender).Text);
        }

        private void FILING_DATE_TextChanged(object sender, TextChangedEventArgs e)
        {
            set_DatePicker(FILING_DATE, ((TextBox)sender).Text);
        }

        private void LAST_PAY_DATE_TextChanged(object sender, TextChangedEventArgs e)
        {
            set_DatePicker(LAST_PAY_DATE, ((TextBox)sender).Text);
        }

        private void ORIGINAL_MTG_TextChanged(object sender, TextChangedEventArgs e)
        {
            set_DatePicker(ORIGINAL_MTG, ((TextBox)sender).Text);
        }

        private void ZIP_TextInput(object sender, TextCompositionEventArgs e)
        {
            //if (Regex.IsMatch(e.Text, @"[^\d]") || ((ComboBox)sender).Text?.Length >= 5)
            //{
            //    Console.Beep(5000, 200);
            //    e.Handled = true;
            //}
        }
    }

    //public class MyCustomDateConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        if (value == null)
    //            return null;
    //        return ((DateTime)value).ToString("MMddyy", culture);
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        //try
    //        //{
    //            return DateTime.ParseExact((string)value, "MMddyy", culture);
    //        //}
    //        //catch
    //        //{
    //        //    return null;
    //        //}
    //    }
    //}
}