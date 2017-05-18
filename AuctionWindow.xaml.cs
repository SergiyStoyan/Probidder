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

            System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.CreateSpecificCulture(System.Globalization.CultureInfo.CurrentCulture.Name);
            ci.DateTimeFormat.ShortDatePattern = "MMddyy";
            ci.DateTimeFormat.LongDatePattern = "MMddyy";
            Thread.CurrentThread.CurrentCulture = ci;
            
            if (foreclosure_id != null)
            {
                fields.IsEnabled = false;
                Save.Visibility = Visibility.Collapsed;
                Delete.Visibility = Visibility.Collapsed;
                Edit.Visibility = Visibility.Visible;
            }

            COUNTY.Text = Settings.General.County;

            CITY.Items.Clear();
            foreach (string c in Db.GetValuesFromJsonTable("cities", "city", new Dictionary<string, string>() { { "county", Settings.General.County } }))
                CITY.Items.Add(c);

            LENDOR.Items.Clear();
            foreach (string c in Db.GetValuesFromJsonTable("plaintiffs", "plaintiff", new Dictionary<string, string>() { { "county", Settings.General.County } }))
                LENDOR.Items.Add(c);

            ATTY.Items.Clear();
            foreach (string c in Db.GetValuesFromJsonTable("attorneys", "attorney", new Dictionary<string, string>() { { "county", Settings.General.County } }))
                ATTY.Items.Add(c);

            TYPE_OF_MO.Items.Clear();
            foreach (string c in Db.GetValuesFromJsonTable("mortgage_types", "mortgage_type", new Dictionary<string, string>() { }))
                TYPE_OF_MO.Items.Add(c);

            PROP_DESC.Items.Clear();
            foreach (string c in Db.GetValuesFromCsvTable("property_codes", "type", new Dictionary<string, string>() { }))
                PROP_DESC.Items.Add(c);

            OWNER_ROLE.Items.Clear();
            foreach (string c in Db.GetValuesFromCsvTable("owner_roles", "role", new Dictionary<string, string>() { }))
                OWNER_ROLE.Items.Add(c);

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
            {
                foreclosures.Delete(f.Id);
                ListWindow.ItemDeleted(f.Id);
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
            Db.Foreclosure f = (Db.Foreclosure)fields.DataContext;
            foreclosures.Save(f);

            ListWindow.ItemSaved(f);
            fields.DataContext = new Db.Foreclosure();
            Save.IsEnabled = false;
            Delete.IsEnabled = false;
            //Close();
        }

        private void City_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ZIP.Items.Clear();
            foreach (string c in Db.GetValuesFromCsvTable("illinois_postal_codes", "postalcode", new Dictionary<string, string> { { "county", Settings.General.County }, { "placename", (string)CITY.SelectedItem } }))
                ZIP.Items.Add(c);
        }

        private void ATTY_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ATTORNEY_S.Items.Clear();
            foreach (string c in Db.GetValuesFromJsonTable("attorney_phones", "attorney_phone", new Dictionary<string, string>() { { "county", Settings.General.County }, { "attorney", (string)ATTY.SelectedItem } }))
                ATTORNEY_S.Items.Add(c);

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
                COUNTY.Text = Settings.General.County;
                FILING_DATE.Text = DateTime.Now.ToString();
                ENTRY_DATE.Text = DateTime.Now.ToString();
                IS_ORG.IsChecked = false;
                DECEASED.IsChecked = false;

                return;
            }
            DATE_OF_CA.SelectedDate = f.DATE_OF_CA;
            LAST_PAY_DATE.SelectedDate = f.LAST_PAY_DATE;
        }

        class test
        {
            public string Value { get; set; }
            public string PIN { get; set; }
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

        private void PIN_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (PIN.Text.Length > 17)
            {
                Console.Beep(5000, 200);
                e.Handled = true;
                return;
            }

            if (e.Text == "-")
            {
                if (PIN.CaretIndex == 2
                    || PIN.CaretIndex == 5
                    || PIN.CaretIndex == 9
                    || PIN.CaretIndex == 13
                    )
                {
                    e.Handled = false;
                    return;
                }
                Console.Beep(5000, 200);
                e.Handled = true;
                return;
            }

            int cursor = PIN.CaretIndex;
            string t = PIN.Text.Insert(PIN.CaretIndex, e.Text);
            cursor++;
            for (int i = cursor - 1; i >= 0; i--)
                if (t[i] == '-')
                    cursor--;
            t = Regex.Replace(t, "-", "");
            ensure_separator(ref t, ref cursor, 2, "-");
            ensure_separator(ref t, ref cursor, 5, "-");
            ensure_separator(ref t, ref cursor, 9, "-");
            ensure_separator(ref t, ref cursor, 13, "-");
            PIN.Text = t;
            PIN.CaretIndex = cursor;
            e.Handled = true;
        }

        void ensure_separator(ref string t, ref int cursor, int position, string separator = "-")
        {
            if (t.Length < position)
                return;
            if (t.Length == position)
                t = t + separator;
            else
                t = t.Insert(position, separator);
            if (cursor >= position)
                cursor++;
        }

        private void DATE_OF_CA_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (Regex.IsMatch(e.Text, @"[^\d]"))
            {
                Console.Beep(5000, 200);
                e.Handled = true;
            }
        }

        private void DATE_OF_CA_TextInput(object sender, TextCompositionEventArgs e)
        {
            //DATE_OF_CA.SelectedDate = calendar_input(e.Text);
        }

        private DateTime? calendar_input(string text)
        {
            if(text.Length > 6 || Regex.IsMatch(text, @"[^\d]"))
            {
                //Console.Beep(5000, 200);
                Message.Error("Date is incorrect.");
                return null;
            }
            Match m = Regex.Match(text, @"(\d{2})(\d{2})(\d{2})");
            if (!m.Success)
            {
                //Console.Beep(5000, 200);
                Message.Error("Date is incorrect.");
                return null;
            }
            return new DateTime(2000 + int.Parse(m.Groups[3].Value), int.Parse(m.Groups[1].Value), int.Parse(m.Groups[2].Value));
        }

        private void DATE_OF_CA_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void DATE_OF_CA_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        private void DATE_OF_CA_DateValidationError(object sender, DatePickerDateValidationErrorEventArgs e)
        {

        }

        private void DATE_OF_CA_LostFocus(object sender, RoutedEventArgs e)
        {
            //DATE_OF_CA.SelectedDate = calendar_input(DATE_OF_CA.Text);
        }

        private void DATE_OF_CA_KeyDown(object sender, KeyEventArgs e)
        {
            //if(e.Key == Key.Enter)
            //    DATE_OF_CA.SelectedDate = calendar_input(DATE_OF_CA.Text);
        }

        private void ORIGINAL_MTG_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void PIN_Error(object sender, ValidationErrorEventArgs e)
        {

        }

        private void DATE_OF_CA_Error(object sender, ValidationErrorEventArgs e)
        {

        }
    }
}