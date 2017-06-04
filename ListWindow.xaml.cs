/********************************************************************************************
        Author: Sergey Stoyan
        sergey.stoyan@gmail.com
        http://www.cliversoft.com
********************************************************************************************/
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
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Collections.ObjectModel;

namespace Cliver.Foreclosures
{
    public partial class ListWindow : Window
    {
        public static void OpenDialog()
        {
            if (_This == null || !_This.IsLoaded)
            {
                _This = new ListWindow();
                System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(_This);
            }
            _This.ShowDialog();
        }

        static ListWindow _This = null;

        public static ListWindow This
        {
            get
            {
                return _This;
            }
        }

        ListWindow()
        {
            InitializeComponent();

            Icon = AssemblyRoutines.GetAppIconImageSource();

            Closing += delegate (object sender, System.ComponentModel.CancelEventArgs e)
            {
                Settings.View.ShowedColumns = new List<string>();
                foreach (DataGridColumn dgc in list.Columns.OrderBy(x=>x.DisplayIndex))
                {
                    string cn = get_column_name(dgc);
                    if (cn == null)//buttons
                        continue;
                    Settings.View.ShowedColumns.Add(cn);
                }
                Settings.View.Save();
            };

            Closed += delegate
            {
                _This = null;
                foreclosures.Dispose();
            };

            Db.RefreshStateChanged += delegate
            {
                refresh_db.Dispatcher.Invoke(() =>
                {
                    if (Db.RefreshRuns)
                    {
                        refresh_db.Header = "Refreshing...";
                        refresh_db.IsEnabled = false;
                    }
                    else
                    {
                        refresh_db.Header = "Refresh Database";
                        refresh_db.IsEnabled = true;
                    }
                });
            };

            Export.ToServerStateChanged += delegate
            {
                refresh_db.Dispatcher.Invoke(() =>
                {
                    if (Export.ToServerRuns)
                    {
                        upload.Header = "Uploading...";
                        upload.IsEnabled = false;
                    }
                    else
                    {
                        upload.Header = "Upload";
                        upload.IsEnabled = true;
                    }
                });
            };

            foreclosures.Saved += Foreclosures_Saved;
            foreclosures.Deleted += Foreclosures_Deleted;

            list.ItemContainerGenerator.StatusChanged += delegate (object sender, EventArgs e)
              {//needed for highlighting search keyword
                  highlight(list);
              };

            Set();

            ContentRendered += delegate
             {
                 WpfRoutines.TrimWindowSize(this);
             };
        }

        public void Set()
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                fill();
            }));
        }

        private void Foreclosures_Deleted(int document_id, bool sucess)
        {
            if (_This == null || !_This.IsLoaded)
                return;

            fill();
        }

        private void Foreclosures_Saved(Db.Document document, bool inserted)
        {
            if (_This == null || !_This.IsLoaded)
                return;

            fill();
        }

        Db.Foreclosures foreclosures = new Db.Foreclosures();

        public static string GetName(DependencyObject obj)
        {
            return (string)obj.GetValue(NameProperty);
        }

        string get_column_name(DataGridColumn dgc)
        {
            TextBlock tb = dgc.Header as TextBlock;
            if (tb == null)
                return null;
            return tb.Text;
        }

        void fill()
        {
            List<DataGridColumn> dgcs = new List<DataGridColumn>();
            for (int i = list.Columns.Count - 1; i >= 0; i--)
            {
                DataGridColumn dgc = list.Columns[i];
                string h = get_column_name(dgc);
                if (h != null && !Settings.View.ShowedColumns.Contains(h))
                {
                    list.Columns.RemoveAt(i);
                    continue;
                }
                dgc.CanUserSort = true;
                dgc.CanUserResize = true;
                dgc.CanUserReorder = true;
                if (h != null)//data
                {
                    dgc.Width = new DataGridLength(100, DataGridLengthUnitType.SizeToHeader);
                    list.Columns.RemoveAt(i);
                    dgcs.Add(dgc);
                }
            }
            foreach (string cn in Settings.View.ShowedColumns)
            {
                DataGridColumn dgc = dgcs.Where(x => get_column_name(x) == cn).First();
                list.Columns.Add(dgc);
            }

            ListCollectionView cv = (ListCollectionView)CollectionViewSource.GetDefaultView(list.ItemsSource);
            if (cv != null)
            {
                if (cv.IsEditingItem)
                    cv.CommitEdit();
                if (cv.IsAddingNew)
                    cv.CommitNew();
                
                //if (edited_item != null)
                //{
                //    edited_item = null;
                //    foreclosures.Save(edited_item);
                //}
            }
            var fs = foreclosures.GetAll();
            list.ItemsSource = new ObservableCollection<ForeclosureView>(foreclosures.GetAll().Select(x => new ForeclosureView(x)).ToList());

            indicator_total.Content = "Total: " + fs.Count;
            filter();
        }

        //ListCollectionView Items { get { return items; } set { items = value; } }
        //ListCollectionView items = new ListCollectionView((new Db.Foreclosures()).GetAll().Select(x => new ForeclosureView(x)).ToList());
        
        private void close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void export_Click(object sender, RoutedEventArgs e)
        {
            Export.ToDisk();
        }

        private void upload_Click(object sender, RoutedEventArgs e)
        {            
            Export.BeginToServer(new Db.Foreclosures());
        }

        private void new_Click(object sender, RoutedEventArgs e)
        {
            ForeclosureWindow.OpenDialog();
        }

        private void list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (keep_selected_fw != null)
            {
                list.SelectedItem = keep_selected_fw;
                keep_selected_fw = null;
                return;
            }

            ForeclosureView fw = list.SelectedItem as ForeclosureView;

            if (fw == null||fw.Model==null)
            {
                indicator_selected.Content = null;
                return;
            }
            indicator_selected.Content = "Selected ID: " + fw.Model.Id;
            //show_AuctionWindow(f);
        }

        void show_ForeclosureWindow(ForeclosureView fw)
        {
            ForeclosureWindow.OpenDialog(fw.Model.Id);
        }

        private void refresh_db_Click(object sender, RoutedEventArgs e)
        {
            Db.BeginRefresh(true);
        }

        private void work_dir_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Log.WorkDir);
        }

        private void about_Click(object sender, RoutedEventArgs e)
        {
            AboutForm.Open();
        }

        private void exit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void refresh_db_last_time_GotFocus(object sender, RoutedEventArgs e)
        {
            if (Settings.Database.LastRefreshTime != null)
            {
                refresh_db_last_time.Text = "Last refreshed at: " + Settings.Database.LastRefreshTime.ToString();
                refresh_db_last_time0.IsOpen = true;
            }
        }

        private void refresh_db_LostFocus(object sender, RoutedEventArgs e)
        {
            refresh_db_last_time0.IsOpen = false;
        }

        private void network_Click(object sender, RoutedEventArgs e)
        {
            NetworkWindow.OpenDialog();
        }

        private void database_Click(object sender, RoutedEventArgs e)
        {
            DatabaseWindow.OpenDialog();
        }

        private void auto_complete_Click(object sender, RoutedEventArgs e)
        {
            AutoCompleteWindow.OpenDialog();
        }

        private void location_Click(object sender, RoutedEventArgs e)
        {
            LocationWindow.OpenDialog();
        }

        private void reset_Click(object sender, RoutedEventArgs e)
        {
            if (Message.YesNo("This will reset all the settings to their initial values. Proceed?"))
                Config.Reset();
        }

        private void view_Click(object sender, RoutedEventArgs e)
        {
            ViewWindow.OpenDialog();
        }
        
        private void open_Click(object sender, RoutedEventArgs e)
        {
            ForeclosureView fw = list.SelectedItem as ForeclosureView;
            if (fw == null)
            {
                ForeclosureWindow.OpenDialog();
                return;
            }
            show_ForeclosureWindow(fw);
        }

        private void show_search_Click(object sender, RoutedEventArgs e)
        {
            show_search.IsChecked = !show_search.IsChecked;
            search.Visibility = ((MenuItem)e.Source).IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            filter();
        }

        private void keyword_TextChanged(object sender, TextChangedEventArgs e)
        {
            filter();
        }

        void filter()
        {
            string k = keyword.Text;
            ListCollectionView cv = (ListCollectionView)CollectionViewSource.GetDefaultView(list.ItemsSource);
            if (cv.IsEditingItem)
                cv.CommitEdit();
            if (cv.IsAddingNew)
                cv.CommitNew();

            if (string.IsNullOrEmpty(k) || search.Visibility != Visibility.Visible)
            {
                filter_regex = null;
                cv.Filter = null;
                indicator_filtered.Content = null;
                return;
            }

            PropertyInfo[] pis_ = typeof(ForeclosureView).GetProperties();
            List<PropertyInfo> pis = new List<PropertyInfo>();
            foreach (string c in Settings.View.SearchedColumns)
            {
                PropertyInfo pi = pis_.FirstOrDefault(x => x.Name == c);
                if (pi == null)
                    continue;
                pis.Add(pi);
            }

            filter_regex = new Regex("(" + Regex.Escape(k) + ")", RegexOptions.IgnoreCase);
            cv.Filter = o =>
            {
                ForeclosureView fw = (ForeclosureView)o;
                foreach (PropertyInfo pi in pis)
                {
                    object v = pi.GetValue(fw);
                    if (v == null)
                        continue;
                    string s;
                    if (pi.PropertyType == typeof(DateTime?))
                        s = ((DateTime)v).ToString(DATE_FORMAT);
                    else
                        s = v.ToString();
                    if (!string.IsNullOrEmpty(s) && filter_regex.IsMatch(s))
                        return true;
                }
                return false;
            };

            int count = 0;
            foreach (object o in cv)
                if(o is ForeclosureView)
                    count++;        
            indicator_filtered.Content = "Filtered: " + count;
        }
        Regex filter_regex = null;
        private void highlight(DataGrid grid)
        {
            if (filter_regex == null)
                return;

            HashSet<int> searched_columns = new HashSet<int>();
            for (int i = 0; i < list.Columns.Count; i++)
                if (Settings.View.SearchedColumns.Contains(get_column_name(list.Columns[i])))
                    searched_columns.Add(i);

            foreach (DataGridRow r in grid.FindChildrenOfType<DataGridRow>())
            {
                for (int j = 0; j < grid.Columns.Count; j++)
                {
                    if (!searched_columns.Contains(j))
                        continue;
                    foreach (TextBlock tb in grid.GetCell(r, j).FindChildrenOfType<TextBlock>())
                        highlight_TextBlock(tb);
                }
            }
        }
        private void highlight_TextBlock(TextBlock tb)
        {
            if (tb == null || filter_regex == null)
                return;
            string text = tb.Text;
            string[] ts = filter_regex.Split(text);
            if (ts.Length < 2)
                return;
            tb.Inlines.Clear();
            bool match = false;
            foreach (string t in ts)
            {
                if (match)
                    tb.Inlines.Add(new Run() { Text = t, Background = Settings.View.SearchHighlightColor });
                else
                    tb.Inlines.Add(new Run() { Text = t });
                match = !match;
            }
        }

        private void list_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (!Settings.View.ShowedColumns.Contains(e.PropertyName))
            {
                e.Cancel = true;
                return;
            }

            //e.Column.IsReadOnly = false;
            //e.Column.Width = new DataGridLength(100, DataGridLengthUnitType.SizeToHeader);
            //e.Column.HeaderTemplate = Resources["Header"] as DataTemplate;//to keep '_' in names
            //e.Column.CanUserSort = true;
            //e.Column.CanUserResize = true;
            //e.Column.CanUserReorder = true;

            //var tc = new DataGridTemplateColumn();
            //var dt = new DataTemplate();
            //var dtc = new FrameworkElementFactory(typeof(TextBlock));
            //Binding b = new Binding(e.PropertyName);
            //dtc.SetBinding(TextBlock.TextProperty, b);
            //dt.VisualTree = dtc;
            //tc.CellTemplate = dt;

            //{
            //    tc = new DataGridTemplateColumn();
            //    dt = new DataTemplate();
            //    dtc = new FrameworkElementFactory(typeof(TextBlock));
            //    b = new Binding(e.PropertyName);
            //    b.Mode = BindingMode.OneWay;
            //    b.NotifyOnSourceUpdated = true;
            //    b.NotifyOnTargetUpdated = true;
            //    b.NotifyOnValidationError = true;

            //    if (e.PropertyType == typeof(DateTime?))
            //    {
            //        b.StringFormat = DATE_FORMAT;
            //        dtc.SetBinding(DatePickerControl.TextProperty, b);
            //        dt.VisualTree = dtc;
            //        tc.CellEditingTemplate = dt;
            //    }
            //    else if (e.PropertyType == typeof(DateTime?))
            //    {
            //        b.StringFormat = DATE_FORMAT;
            //        dtc.SetBinding(DatePickerControl.TextProperty, b);
            //        dt.VisualTree = dtc;
            //        tc.CellEditingTemplate = dt;
            //    }
            //}

            //e.Column = tc;
        }
        readonly string DATE_FORMAT = "MM/dd/yyyy";

        private void list_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {            
        }

        private void list_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            var fw = e.Row.DataContext as ForeclosureView;
            if (fw == null)
                return;
            fw.InitialControlSetting = true;
        }

        private void list_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            var fw = e.Row.DataContext as ForeclosureView;
            if (fw == null)
                return;
            if (!e.Row.IsValid())
            {
                e.Cancel = true;
                //keep_selected_fw = fw;
                //var presenters = e.Row.FindVisualChildrenOfType<System.Windows.Controls.Primitives.DataGridCellsPresenter>().ToList();
                //foreach (var p in presenters)
                //    if (!p.IsValid())
                        return;
                //e.Row.MarkValid();
            }
            e.Cancel = false;
            keep_selected_fw = null;
            foreclosures.Save(fw.Model);
        }
        ForeclosureView keep_selected_fw = null;

        private void delete_Click(object sender, RoutedEventArgs e)
        {
            ForeclosureView fw = list.SelectedItem as ForeclosureView;
            if (fw == null||fw.Model==null)
                return;
            if (!Message.YesNo("You are about deleting record [Id=" + fw.Model.Id + "]. Proceed?"))
                return;
            foreclosures.Delete(fw.Model.Id);
        }
    }

    //public class ForeclosuresView
    //{
    //  static  ObservableCollection<ForeclosureView> list = new ObservableCollection<ForeclosureView>((new Db.Foreclosures()).GetAll().Select(x => new ForeclosureView(x)));

    //   static public ObservableCollection<ForeclosureView> List { get { return list; } set { list = value; } }
    //}
}