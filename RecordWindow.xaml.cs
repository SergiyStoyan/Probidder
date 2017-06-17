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

namespace Cliver.Probidder
{
    public partial class RecordWindow : Window
    {
        public static void OpenDialog(IView v, IViews vs)
        {
            RecordWindow w = new RecordWindow(v, vs);
            System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(w);
            w.ShowDialog();
        }

        RecordWindow(IView v, IViews vs)
        {
            InitializeComponent();

            Icon = AssemblyRoutines.GetAppIconImageSource();

            this.vs = vs;

            Loaded += delegate
            {
                //EventManager.RegisterClassHandler(typeof(Control), GotKeyboardFocusEvent, new RoutedEventHandler(got_focus));
            };
            //PreviewGotKeyboardFocus += ForeclosureWindow_PreviewGotKeyboardFocus;

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() =>
            {
                set_context(v);
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
                   if (!save_current_View() && !Message.YesNo("Close without saving the current entry?"))
                       e.Cancel = true;
               };

            AddHandler(FocusManager.GotFocusEvent, (RoutedEventHandler)got_focus);
            AddHandler(Keyboard.KeyDownEvent, (KeyEventHandler)AutoComplete.Wpf.KeyDownHandler);
        }
        readonly IViews vs;

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

        IView set_context(IView v)
        {
            if (v == null)
                switch (Settings.View.ActiveTable)
                {
                    case Settings.ViewSettings.Tables.Foreclosures:
                        v = new ForeclosureView(null);
                        break;
                    case Settings.ViewSettings.Tables.Probates:
                        v = new ProbateView(null);
                        break;
                    default:
                        throw new Exception("Unknown option: " + Settings.View.ActiveTable);
                }

            this.MarkValid();

            FILING_DATE.Reset();
            ENTRY_DATE.Reset();
            ORIGINAL_MTG.Reset();
            DATE_OF_CA.Reset();
            LAST_PAY_DATE.Reset();
            
            v.ErrorsChanged += delegate
              {
                  check_validity(v);
              };
            fields.DataContext = v;
            return v;
        }

        void check_validity(IView v)
        {
            if (v == null)
                return;
            if (v.HasErrors)// !fields.IsValid())
            {
                Next.IsEnabled = false;
                Prev.IsEnabled = false;
                New.IsEnabled = false;
            }
            else
            {
                New.IsEnabled = true;
                if (v.Id == 0)
                {
                    Prev.IsEnabled = vs.GetLast_() != null;
                    Next.IsEnabled = false;
                }
                else
                {
                    Prev.IsEnabled = vs.GetPrevious(v) != null;
                    Next.IsEnabled = vs.GetNext(v) != null;
                }
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (!Message.YesNo("The entry is about deletion. Are you sure to proceed?"))
                return;

            IView v = (IView)fields.DataContext;
            if (v == null)
                return;
            if (v.Id != 0)
            {
                IView fw2 = vs.GetNext(v);
                if (fw2 == null)
                    fw2 = vs.GetPrevious(v);

                vs.Delete(v);
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
            if (!save_current_View())
                return;
            IView v = (IView)fields.DataContext;
            if (v == null)
                return;
            if (v.Id == 0)
                v = vs.GetLast_();
            else
                v = vs.GetPrevious(v);
            if (v == null)
            {
                Prev.IsEnabled = false;
                return;
            }
            set_context(v);
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (!save_current_View())
                return;
            IView v = (IView)fields.DataContext;
            if (v == null)
                return;
            if (v.Id == 0)
            {
                Next.IsEnabled = false;
                return;
            }
            v = vs.GetNext(v);
            if (v == null)
            {
                Next.IsEnabled = false;
                return;
            }
            set_context(v);
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            if (!save_current_View())
                return;
            set_context(null);
        }

        private bool save_current_View()
        {
            try
            {
                IView v = (IView)fields.DataContext;
                if (v == null)
                    return false;
                if (/*v.Id != 0 && */!v.Edited)
                    return true;
                v.ValidateAllProperties();
                if (/*!fields.IsValid() ||*/ v.HasErrors)
                {
                    //throw new Exception("Some values are incorrect. Please correct fields surrounded with red borders before saving.");
                    return false;
                }
                vs.Update(v);
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
            IView v = (IView)e.NewValue;
            
            //Keyboard.Focus(TYPE_OF_EN);

            if (v == null)
                v = (IView)fields.DataContext;
            if (v == null)
                return;

            if (v.Id == 0)
            {
                Prev.IsEnabled = vs.GetLast_() != null;
                Next.IsEnabled = false;

                indicator.Content = "Record: [id=new] - / " + vs.Count();

                return;
            }

            Prev.IsEnabled = vs.GetPrevious(v) != null;
            Next.IsEnabled = vs.GetNext(v) != null;

            indicator.Content = "Record: [id=" + v.Id + "] " + (vs.Get(x => ((IView)x).Id < v.Id).Count() + 1) + " / " + vs.Count();
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