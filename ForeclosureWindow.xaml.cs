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
    public partial class ForeclosureWindow : Window
    {
        public static ForeclosureWindow OpenNew(int? foreclosure_id = null)
        {
            ForeclosureWindow w = new ForeclosureWindow(foreclosure_id);
            System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(w);
            w.Show();
            return w;
        }

        public static void OpenDialog(int? foreclosure_id = null)
        {
            ForeclosureWindow w = new ForeclosureWindow(foreclosure_id);
            System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(w);
            w.ShowDialog();
        }

        ForeclosureWindow(int? foreclosure_id = null)
        {
            InitializeComponent();

            Icon = AssemblyRoutines.GetAppIconImageSource();

            //System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.CreateSpecificCulture(System.Globalization.CultureInfo.CurrentCulture.Name);
            //ci.DateTimeFormat.ShortDatePattern = "MM/dd/yyyy";
            //ci.DateTimeFormat.LongDatePattern = "MM/dd/yyyy";
            //Thread.CurrentThread.CurrentCulture = ci;

            if (foreclosure_id != null)
            {
                fields.IsEnabled = false;
                Save.Visibility = Visibility.Collapsed;
                Delete.Visibility = Visibility.Collapsed;
                Edit.Visibility = Visibility.Visible;
            }

            COUNTY.Text = Settings.Location.County;

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

            //AddHandler(FocusManager.GotFocusEvent, (GotFocusHandler)GotFocusHandler);
            AddHandler(Keyboard.KeyDownEvent, (KeyEventHandler)KeyDownHandler);
        }
        Db.Foreclosures foreclosures = new Db.Foreclosures();

        void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //TextBox tb = sender as TextBox;
            //if (e.Text != AutoComplete.TriggerKey)
            //    return;
            //tb.Text = AutoComplete.GetComplete(tb.Text);
        }

        //public void GotFocusHandler(DependencyObject element, RoutedEventHandler handler)
        //{
        //}

        public void KeyDownHandler(object sender, KeyEventArgs e)
        {
            if (!AutoComplete.IsKeyTrigger(e.Key))
                return;
            IInputElement ii = Keyboard.FocusedElement;
            if (ii == null)
                return;
            e.Handled = true;
            TextBox tb = ii as TextBox;
            if (tb != null)
            {
                tb.Text = AutoComplete.GetComplete(tb.Text, tb.CaretIndex);
                return;
            }
            ComboBox cb = ii as ComboBox;
            if (cb != null)
            {
                cb.Text = AutoComplete.GetComplete(cb.Text, cb.FindChildrenOfType<TextBox>().First().CaretIndex);
                return;
            }
        }
        //IInputElement last_InputElement = null;
        //[DllImport("user32.dll")]
        //static extern short VkKeyScan(char ch);
        //static public Key ResolveKey(char charToResolve)
        //{
        //    return KeyInterop.KeyFromVirtualKey(VkKeyScan(charToResolve));
        //}

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
            try
            {
                //if (string.IsNullOrWhiteSpace(subject.Text))
                //    throw new Exception("Subject is empty.");
                //if (string.IsNullOrWhiteSpace(this.description.Text))
                //    throw new Exception("Description is empty 

                if (!this.IsValid())
                    throw new Exception("Some values are incorrect. Please correct fields surrounded with red borders before saving.");

                Db.Foreclosure f = (Db.Foreclosure)fields.DataContext;
                foreclosures.Save(f);

                fields.DataContext = new Db.Foreclosure();
                Save.IsEnabled = false;
                Delete.IsEnabled = false;
                //Close();
            }
            catch (Exception ex)
            {
                Message.Error2(ex);
            }
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
            foreach (Db.AttorneyPhone c in (new Db.AttorneyPhones()).GetBy(Settings.Location.County, (string)ATTY.SelectedItem))
                ATTORNEY_S.Items.Add(c.attorney_phone);

            Db.Foreclosure f = (Db.Foreclosure)fields.DataContext;
            if (f.Id == 0)
                if (ATTORNEY_S.Items.Count == 1)
                    ATTORNEY_S.SelectedIndex = 0;
        }

        private void fields_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //Keyboard.Focus(TYPE_OF_EN);
            
            CASE_N.Items.Clear();
            foreach (string c in (new Db.CaseNumbers()).GetBy(Settings.Location.County).case_ns)
                CASE_N.Items.Add(c);

            Db.Foreclosure f = (Db.Foreclosure)e.NewValue;
            if (f.Id == 0)
            {
                f.TYPE_OF_EN = "CHA";
                if (CASE_N.Items.Count > 0)
                    f.CASE_N = (string)CASE_N.Items[0];
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
            try
            {
                return DateTime.ParseExact(text, "MMddyy", null);
            }
            catch
            {
            }
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

        private void DatePicker_TextChanged(object sender, TextChangedEventArgs e)
        {
            DateTime? td = calendar_input(((TextBox)sender).Text);
            if (td == null)
                return;
            DatePicker dp = ((DependencyObject)sender).FindParentOfType<DatePicker>();
            DateTime? vd = dp.SelectedDate;
            if (vd != null && ((DateTime)td).Date == ((DateTime)vd).Date
                && ((TextBox)sender).Text.Length < 10
                && dp.IsValid()
                    )
                return;
            dp.SelectedDate = ((DateTime)td).Date;
            ((TextBox)sender).Text = Regex.Replace(dp.SelectedDate.ToString(), " .*", "");
        }

        private void Integer_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (Regex.IsMatch(e.Text, @"[^\d\,]"))
            {
                Console.Beep(5000, 200);
                e.Handled = true;
            }
        }

        private void Decimal_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (Regex.IsMatch(e.Text, @"[^\d\.\,]"))
            {
                Console.Beep(5000, 200);
                e.Handled = true;
            }
        }

        void DatePicker_LostFocus(object sender, EventArgs e)
        {
        }

        private void DatePicker_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //DatePicker dp = ((DatePicker)e.OldFocus);
            //if (!dp.IsValid())
            //{
            //    Console.Beep(5000, 200);
            //    e.Handled = true;
            //    Keyboard.Focus(dp);
            //}
        }

        private void ATTORNEY_S_TextInput(object sender, TextCompositionEventArgs e)
        {

        }

        private void ATTORNEY_S_TextChanged(object sender, RoutedEventArgs e)
        {

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//make text left alignment
            ComboBox cb = (ComboBox)sender;
            TextBox tb = cb.GetVisualChild<TextBox>();
            if (tb == null)//can be so due to asynchronous building
                return;
            tb.Select(0, tb.Text.Length);
            tb.ScrollToHome();
            if (e != null)
                e.Handled = true;
        }

        private void ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            ComboBox_SelectionChanged(sender, null);
        }
    }

    //public class MyCustomDateConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        if (keep_last_text)
    //            return last_text;
    //        last_value = (DateTime)value;
    //        if (last_value == null)
    //            return null;
    //        return Regex.Replace(((DateTime)last_value).ToString(culture), " .*", "");
    //    }
    //    DateTime? last_value = null;

    //    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        last_text = (string)value;
    //        keep_last_text = false;
    //        try
    //        {
    //            return DateTime.Parse((string)value, culture);
    //        }
    //        catch
    //        {
    //            try
    //            {
    //                return DateTime.ParseExact((string)value, "MMddyy", culture);
    //            }
    //            catch
    //            {
    //                keep_last_text = true;
    //                return last_value;
    //            }
    //        }
    //    }
    //    string last_text = null;
    //    bool keep_last_text = false;
    //}
}