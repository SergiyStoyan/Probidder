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
        public static void OpenDialog(ForeclosureView fw)
        {
            ForeclosureWindow w = new ForeclosureWindow(fw);
            System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(w);
            w.ShowDialog();
        }

        ForeclosureWindow(ForeclosureView fw)
        {
            InitializeComponent();

            Icon = AssemblyRoutines.GetAppIconImageSource();

            if (fw == null)
                fw = new ForeclosureView();

            Loaded += delegate
            {
                //EventManager.RegisterClassHandler(typeof(Control), GotKeyboardFocusEvent, new RoutedEventHandler(got_focus));
            };
            //PreviewGotKeyboardFocus += ForeclosureWindow_PreviewGotKeyboardFocus;

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() =>
            {
                set_context(fw);
            }));

            Thread check_validity_t = ThreadRoutines.StartTry(() =>
              {
                  //while (true)
                  //{
                  //    Thread.Sleep(300);
                  //    Dispatcher.Invoke(() =>
                  //    {
                  //        check_validity((ForeclosureView)fields.DataContext);
                  //    });
                  //}
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
            };

            Closing += delegate (object sender, System.ComponentModel.CancelEventArgs e)
               {
                   if (!save_current_Foreclosure() && !Message.YesNo("Close without saving the current entry?"))
                       e.Cancel = true;
               };

            AddHandler(FocusManager.GotFocusEvent, (RoutedEventHandler)got_focus);
            AddHandler(Keyboard.KeyDownEvent, (KeyEventHandler)AutoComplete.Wpf.KeyDownHandler);
        }

        //private void ForeclosureWindow_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        //{
        //    got_focus(e.OriginalSource, e);
        //}

        void got_focus(object sender, RoutedEventArgs e)
        {
            sender = e.Source;
            if (sender is Window || sender is ScrollViewer)
                return;

            if (focused_control != null)
            {
                if (focused_control is DatePickerControl)
                    ((DatePickerControl)focused_control).Background = Brushes.White;
                else
                    focused_control.Background = Brushes.White;
            }
            if (sender is TextBox || sender is CheckBox || sender is ComboBox)
                ((Control)sender).Background = Settings.View.FocusedControlColor;
            else if (sender is DatePickerControl)
                ((DatePickerControl)sender).Background = Settings.View.FocusedControlColor;
            else
                return;
            focused_control = (Control)sender;            
        }
        Control focused_control = null;

        ForeclosureView set_context(ForeclosureView fw)
        {
            if (fw == null)
                fw = new ForeclosureView();

            this.MarkValid();

            FILING_DATE.Reset();
            ENTRY_DATE.Reset();
            ORIGINAL_MTG.Reset();
            DATE_OF_CA.Reset();
            LAST_PAY_DATE.Reset();
            
            fw.ErrorsChanged += delegate
              {
                  check_validity(fw);
              };
            fields.DataContext = fw;
            return fw;
        }

        void check_validity(ForeclosureView fw)
        {
            if (fw == null)
                return;
            if (fw.HasErrors)// !fields.IsValid())
            {
                Next.IsEnabled = false;
                Prev.IsEnabled = false;
                New.IsEnabled = false;
            }
            else
            {
                New.IsEnabled = true;
                if (fw.Id == 0)
                {
                    Prev.IsEnabled = ListWindow.This.ForeclosureViews.GetLast() != null;
                    Next.IsEnabled = false;
                }
                else
                {
                    Prev.IsEnabled = ListWindow.This.ForeclosureViews.GetPrevious(fw) != null;
                    Next.IsEnabled = ListWindow.This.ForeclosureViews.GetNext(fw) != null;
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
            if (fw.Id != 0)
            {
                ForeclosureView fw2 = ListWindow.This.ForeclosureViews.GetNext(fw);
                if (fw2 == null)
                    fw2 = ListWindow.This.ForeclosureViews.GetPrevious(fw);
                
                ListWindow.This.ForeclosureViews.Delete(fw);
                set_context(fw2);
            }
            else
                set_context(null);
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
            if (fw.Id == 0)
                fw = ListWindow.This.ForeclosureViews.GetLast();
            else
                fw = ListWindow.This.ForeclosureViews.GetPrevious(fw);
            if (fw == null)
            {
                Prev.IsEnabled = false;
                return;
            }
            set_context(fw);
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (!save_current_Foreclosure())
                return;
            ForeclosureView fw = (ForeclosureView)fields.DataContext;
            if (fw == null)
                return;
            if (fw.Id == 0)
            {
                Next.IsEnabled = false;
                return;
            }
            fw = ListWindow.This.ForeclosureViews.GetNext(fw);
            if (fw == null)
            {
                Next.IsEnabled = false;
                return;
            }
            set_context(fw);
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            if (!save_current_Foreclosure())
                return;
            set_context(null);
        }

        private bool save_current_Foreclosure()
        {
            try
            {
                ForeclosureView fw = (ForeclosureView)fields.DataContext;
                if (fw == null)
                    return false;
                if (/*fw.Id != 0 && */!fw.Edited)
                    return true;
                fw.ValidateAllProperties();
                if (/*!fields.IsValid() ||*/ fw.HasErrors)
                {
                    //throw new Exception("Some values are incorrect. Please correct fields surrounded with red borders before saving.");
                    return false;
                }
                ListWindow.This.ForeclosureViews.Update(fw);
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

            if (fw.Id == 0)
            {
                Prev.IsEnabled = ListWindow.This.ForeclosureViews.GetLast() != null;
                Next.IsEnabled = false;

                indicator.Content = "Record: [id=new] - / " + ListWindow.This.ForeclosureViews.Count();

                return;
            }

            Prev.IsEnabled = ListWindow.This.ForeclosureViews.GetPrevious(fw) != null;
            Next.IsEnabled = ListWindow.This.ForeclosureViews.GetNext(fw) != null;

            indicator.Content = "Record: [id=" + fw.Id + "] " + (ListWindow.This.ForeclosureViews.Get(x => x.Id < fw.Id).Count() + 1) + " / " + ListWindow.This.ForeclosureViews.Count();
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