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
            
            Loaded += delegate
            {
                Db.Foreclosure f;
                if (foreclosure_id != null)
                    f = foreclosures.GetById((int)foreclosure_id);
                else
                    f = new Db.Foreclosure();
                ForeclosureView fw = set_context(f);
                fw.InitialControlSetting = true;
            };

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() =>
            {
                ForeclosureView fw = (ForeclosureView)fields.DataContext;
                if (fw != null)
                    fw.InitialControlSetting = false;
            }));

            Thread check_validity_t = ThreadRoutines.StartTry(() =>
              {
                  while (true)
                  {
                      Thread.Sleep(300);
                      Dispatcher.Invoke(() =>
                      {
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

        ForeclosureView set_context(Db.Foreclosure f)
        {
            this.MarkValid();

            FILING_DATE.Text = FILING_DATE.Mask;
            ENTRY_DATE.Text = ENTRY_DATE.Mask;
            ORIGINAL_MTG.Text = ORIGINAL_MTG.Mask;
            DATE_OF_CA.Text = DATE_OF_CA.Mask;
            LAST_PAY_DATE.Text = LAST_PAY_DATE.Mask;
            ATTORNEY_S.Text = ATTORNEY_S.Mask;            

            ForeclosureView fw = new ForeclosureView(f);
            fw.PropertyChanged2 += delegate
              {
                  check_validity();
              };
            fw.InitialControlSetting = true;
            fields.DataContext = fw;
            fw.InitialControlSetting = false;
            return fw;
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
                ForeclosureView fw = (ForeclosureView)fields.DataContext;
                if (fw == null)
                    return;
                Db.Foreclosure f = fw.Model;
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

            ForeclosureView fw = (ForeclosureView)fields.DataContext;
            if (fw == null)
                return;
            Db.Foreclosure f = fw.Model;
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
            ForeclosureView fw = (ForeclosureView)fields.DataContext;
            if (fw == null)
                return;
            Db.Foreclosure f = fw.Model;
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
            ForeclosureView fw = (ForeclosureView)fields.DataContext;
            if (fw == null)
                return;
            Db.Foreclosure f = fw.Model;
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
                ForeclosureView fw = (ForeclosureView)fields.DataContext;
                if (fw == null)
                    return false;

                if (!fw.Edited)
                    return true;

                fw.OnPropertyChanged(null);

                if (!fields.IsValid())
                {
                    //throw new Exception("Some values are incorrect. Please correct fields surrounded with red borders before saving.");
                    return false;
                }

                foreclosures.Save(fw.Model);

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

        private void fields_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ForeclosureView fw = (ForeclosureView)e.NewValue;
            
            //Keyboard.Focus(TYPE_OF_EN);

            if (fw == null)
                fw = (ForeclosureView)fields.DataContext;
            if (fw == null)
                return;

            if (fw.Model.Id == 0)
            {
                Prev.IsEnabled = foreclosures.GetLast() != null;
                Next.IsEnabled = false;

                indicator.Content = "Record: [id=new] - / " + foreclosures.Count();

                return;
            }

            Prev.IsEnabled = foreclosures.GetPrevious(fw.Model) != null;
            Next.IsEnabled = foreclosures.GetNext(fw.Model) != null;

            indicator.Content = "Record: [id=" + fw.Model.Id + "] " + (foreclosures.Get(x => x.Id < fw.Model.Id).Count() + 1) + " / " + foreclosures.Count();
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