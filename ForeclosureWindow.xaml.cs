/********************************************************************************************
        Author: Sergey Stoyan
        sergey.stoyan@gmail.com
        http://www.cliversoft.com
********************************************************************************************/
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

            COUNTY.Text = Settings.Location.County;

            Loaded += delegate
            {
                Db.Foreclosure f;
                if (foreclosure_id != null)
                    f = foreclosures.GetById((int)foreclosure_id);
                else
                    f = new Db.Foreclosure();
                set_context(f);
                f.InitialControlSetting = true;
            };

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() => {
                Db.Foreclosure f = (Db.Foreclosure)fields.DataContext;
                if (f != null)
                    f.InitialControlSetting = false;
            }));

          Thread check_validity_t = ThreadRoutines.StartTry(() =>
            {
                while(true)
                {
                    Thread.Sleep(300);
                    Dispatcher.Invoke(() => {
                        check_validity();
                    });
                }
            });

            PreviewKeyDown += delegate
              {
              };

            PreviewMouseDown += delegate
            {
            };

            Closed += delegate
            {
                if (check_validity_t != null && check_validity_t.IsAlive)
                    check_validity_t.Abort();
                foreclosures.Dispose();
            };

            Closing += delegate (object sender, System.ComponentModel.CancelEventArgs e)
               {
                   if (!save_current_Foreclosure() && !Message.YesNo("Close without saving the current entry?"))
                       e.Cancel = true;
               };

            //AddHandler(FocusManager.GotFocusEvent, (GotFocusHandler)GotFocusHandler);
            AddHandler(Keyboard.KeyDownEvent, (KeyEventHandler)AutoComplete.Wpf.KeyDownHandler);
        }
        Db.Foreclosures foreclosures = new Db.Foreclosures();

        void set_context(Db.Foreclosure f)
        {
            this.MarkValid();

            FILING_DATE.Text = FILING_DATE.Mask;
            ENTRY_DATE.Text = ENTRY_DATE.Mask;
            ORIGINAL_MTG.Text = ORIGINAL_MTG.Mask;
            DATE_OF_CA.Text = DATE_OF_CA.Mask;
            LAST_PAY_DATE.Text = LAST_PAY_DATE.Mask;
            LENDOR.Text = "";
            CITY.Text = "";
            ZIP.Text = "";
            PROP_DESC.Text = "";
            TYPE_OF_MO.Text = "";
            ATTORNEY_S.Text = ATTORNEY_S.Mask;
            ATTY.Text = "";
            OWNER_ROLE.Text = "";

            CITY.ItemsSource = (new Db.Cities()).GetBy(Settings.Location.County).OrderBy(x => x.city).Select(x => x.city);
            LENDOR.ItemsSource = (new Db.Plaintiffs()).GetBy(Settings.Location.County).OrderBy(x => x.plaintiff).Select(x => x.plaintiff);
            ATTY.ItemsSource = (new Db.Attorneys()).GetBy(Settings.Location.County).OrderBy(x => x.attorney).Select(x => x.attorney);
            TYPE_OF_MO.ItemsSource = (new Db.MortgageTypes()).Get().OrderBy(x => x.mortgage_type).Select(x => x.mortgage_type);
            PROP_DESC.ItemsSource = (new Db.PropertyCodes()).GetAll().OrderBy(x => x.type).Select(x => x.type);
            OWNER_ROLE.ItemsSource = (new Db.OwnerRoles()).GetAll().OrderBy(x => x.role).Select(x => x.role);
            ZIP.ItemsSource = null;
            ATTORNEY_S.ItemsSource = null;
            CASE_N.ItemsSource = null;

            f.PropertyChanged2 += delegate
              {
                  check_validity();
              };
            f.InitialControlSetting = true;
            fields.DataContext = f;
            f.InitialControlSetting = false;
        }

        void check_validity()
        {
            if (!fields.IsValid())
            {
                Next.IsEnabled = false;
                Prev.IsEnabled = false;
                New.IsEnabled = false;
            }
            else
            {
                New.IsEnabled = true;
                Db.Foreclosure f = get_current_Foreclosure();
                if (f.Id == 0)
                {
                    Prev.IsEnabled = foreclosures.GetLast() != null;
                    Next.IsEnabled = false;
                }
                else
                {
                    Prev.IsEnabled = foreclosures.GetPrevious(f) != null;
                    Next.IsEnabled = foreclosures.GetNext(f) != null;
                }
            }
        }

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
                set_context(f2);
            }
            else
                set_context(new Db.Foreclosure());
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Prev_Click(object sender, RoutedEventArgs e)
        {
            if (!save_current_Foreclosure())
                return;
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
            set_context(f);
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (!save_current_Foreclosure())
                return;
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
            set_context(f);
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            if (!save_current_Foreclosure())
                return;
            set_context(new Db.Foreclosure());
        }

        private bool save_current_Foreclosure()
        {
            try
            {
                Db.Foreclosure f = get_current_Foreclosure();

                if (!f.Edited)
                    return true;

                f.OnPropertyChanged(null);

                if (!fields.IsValid())
                {
                    //throw new Exception("Some values are incorrect. Please correct fields surrounded with red borders before saving.");
                    return false;
                }

                foreclosures.Save(f);

                //fields.IsEnabled = false;
                //ThreadRoutines.StartTry(() =>
                //{
                //    Thread.Sleep(200);
                //    fields.Dispatcher.Invoke(() => { fields.IsEnabled = true; });
                //});
                
                return true;
            }
            catch (Exception ex)
            {
                Message.Error2(ex);
            }
            return false;
        }

        private void City_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ZIP.ItemsSource = (new Db.Zips()).GetBy(Settings.Location.County, (string)CITY.SelectedItem).OrderBy(x => x.zip).Select(x=>x.zip);
        }

        private void ATTY_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ATTORNEY_S.ItemsSourceNomalized = (new Db.AttorneyPhones()).GetBy(Settings.Location.County, (string)ATTY.SelectedItem).OrderBy(x => x.attorney_phone).Select(x=>x.attorney_phone);

            //Db.Foreclosure f = (Db.Foreclosure)fields.DataContext;
            //if (f.Id == 0)
            //    if (ATTORNEY_S.Items.Count > 0)
            //        ATTORNEY_S.SelectedIndex = 0;
        }

        private void fields_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            CASE_N.ItemsSource = (new Db.CaseNumbers()).GetBy(Settings.Location.County).case_ns.OrderBy(x => x);
            
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

                indicator.Content = "Record: [id=new] - / " + foreclosures.Count();

                return;
            }

            Prev.IsEnabled = foreclosures.GetPrevious(f) != null;
            Next.IsEnabled = foreclosures.GetNext(f) != null;

            indicator.Content = "Record: [id=" + f.Id + "] " + (foreclosures.Get(x => x.Id < f.Id).Count() + 1) + " / " + foreclosures.Count();
        }

        Db.Foreclosure get_current_Foreclosure()
        {
            return (Db.Foreclosure)fields.DataContext;
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
    }
}