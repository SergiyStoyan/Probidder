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
            
            if (foreclosure_id != null)
                fields.DataContext = foreclosures.GetById((int)foreclosure_id);
            else
                fields.DataContext = new Db.Foreclosure();

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

            Db.Foreclosure f = get_current_Foreclosure();
            if (f.Id != 0)
            {
                Db.Foreclosure f2 = foreclosures.GetNext(f);
                if (f2 == null)
                    f2 = foreclosures.GetPrevious(f);
                if (f2 == null)
                    f2 = new Db.Foreclosure();

                foreclosures.Delete(f.Id);
                fields.DataContext = f2;
            }
            else
                fields.DataContext = new Db.Foreclosure();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Prev_Click(object sender, RoutedEventArgs e)
        {
            Db.Foreclosure f = get_current_Foreclosure();
            if (f.Id == 0)
                f = foreclosures.GetLast();
            else
                f = foreclosures.GetPrevious(f);
            if (f == null)
            {
                Prev.IsEnabled = false;
                return;
            }
            fields.DataContext = f;
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            Db.Foreclosure f = get_current_Foreclosure();
            if (f.Id == 0)
            {
                Next.IsEnabled = false;
                return;
            }
            f = foreclosures.GetNext(f);
            if (f == null)
            {
                Next.IsEnabled = false;
                return;
            }
            fields.DataContext = f;
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            fields.DataContext = new Db.Foreclosure();
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

                Db.Foreclosure f = get_current_Foreclosure();
                bool new_record = f.Id == 0;
                foreclosures.Save(f);

                if (new_record)
                    fields.DataContext = new Db.Foreclosure();
                else
                {
                    //Edit.IsChecked = false;
                    fields.IsEnabled = false;
                }
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
            CASE_N.Items.Clear();
            foreach (string c in (new Db.CaseNumbers()).GetBy(Settings.Location.County).case_ns)
                CASE_N.Items.Add(c);

            Db.Foreclosure f = (Db.Foreclosure)e.NewValue;

            set_controls(f);

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

        void set_controls(Db.Foreclosure f = null)
        {
            //Keyboard.Focus(TYPE_OF_EN);

            if (f == null)
                f = get_current_Foreclosure();

            if (f.Id == 0)
            {
                Prev.IsEnabled = foreclosures.GetLast() != null;
                Next.IsEnabled = false;
                fields.IsEnabled = true;
                Save.Content = "Save and Continue";
                Edit.IsChecked = true;
                Edit.Visibility = Visibility.Collapsed;

                indicator.Content = "Record: [id=new] - / " + foreclosures.Count();

                return;
            }

            Prev.IsEnabled = foreclosures.GetPrevious(f) != null;
            Next.IsEnabled = foreclosures.GetNext(f) != null;
            Save.Content = "Save";
            Edit.Visibility = Visibility.Visible;

            if (Edit.IsChecked == true)
                Edit_Checked(null, null);
            else
                Edit_Unchecked(null, null);

            indicator.Content = "Record: [id=" + f.Id + "] " + (foreclosures.Get(x => x.Id < f.Id).Count() + 1) + " / " + foreclosures.Count();
        }

        private void Edit_Checked(object sender, RoutedEventArgs e)
        {
            fields.IsEnabled = true;
            //Edit.Visibility = Visibility.Collapsed;
            New.Visibility = Visibility.Collapsed;
            Save.Visibility = Visibility.Visible;
            if (get_current_Foreclosure().Id == 0)
                Delete.Visibility = Visibility.Collapsed;
            else
                Delete.Visibility = Visibility.Visible;
        }

        private void Edit_Unchecked(object sender, RoutedEventArgs e)
        {
            fields.IsEnabled = false;
            //Edit.Visibility = Visibility.Visible;
            New.Visibility = Visibility.Visible;
            Save.Visibility = Visibility.Collapsed;
            Delete.Visibility = Visibility.Collapsed;
        }

        Db.Foreclosure get_current_Foreclosure()
        {
            return (Db.Foreclosure)fields.DataContext;
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!fields.IsEnabled && Edit.IsEnabled)//after save
            {
                fields.IsEnabled = true;
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Window_PreviewMouseDown(null, null);
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

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//make text left alignment
            ComboBox cb = (ComboBox)sender;
            TextBox tb = cb.GetVisualChild<TextBox>();
            if (tb == null)//can be so due to asynchronous building
                return;
            if (e.AddedItems.Count < 1)
                return;
            ThreadRoutines.StartTry(() => {
                DateTime end = DateTime.Now.AddMilliseconds(200);
                while(end > DateTime.Now)
                {
                    Thread.Sleep(20);
                    tb.Dispatcher.Invoke(() =>
                    {
                        tb.Select(0, tb.Text.Length);
                        tb.ScrollToHome();
                    });
                }
            });
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