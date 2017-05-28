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

        void fill()
        {
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
            list.ItemsSource = new ObservableCollection<Db.Foreclosure>(fs);

            indicator_total.Content = "Total: " + fs.Count;
            filter();
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void export_Click(object sender, RoutedEventArgs e)
        {
            string file;
            using (var d = new System.Windows.Forms.FolderBrowserDialog())
            {
                d.Description = "Choose the folder where to save the exported file.";
                if (d.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;
                file = d.SelectedPath + "\\foreclosure_" + DateTime.Now.ToString("yy-MM-dd_HH-mm-ss") + ".csv";
            }

            try
            {
                TextWriter tw = new StreamWriter(file);
                tw.WriteLine(FieldPreparation.GetCsvHeaderLine(typeof(Db.Foreclosure), FieldPreparation.FieldSeparator.COMMA));
                foreach (Db.Foreclosure f in foreclosures.GetAll())
                    tw.WriteLine(FieldPreparation.GetCsvLine(f, FieldPreparation.FieldSeparator.COMMA));
                tw.Close();

                if (Message.YesNo("Data has been exported succesfully to " + file + "\r\n\r\nClean up the database?"))
                {
                    foreclosures.Drop();
                    fill();
                }
            }
            catch (Exception ex)
            {
                LogMessage.Error(ex);
            }
        }

        private void new_Click(object sender, RoutedEventArgs e)
        {
            ForeclosureWindow.OpenDialog();
        }

        private void list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Db.Foreclosure f = list.SelectedItem as Db.Foreclosure;

            if (edited_item != null && edited_item != f)
            {
                foreclosures.Save(edited_item);
                edited_item = null;
            }

            if (f == null)
            {
                indicator_selected.Content = null;
                return;
            }
            indicator_selected.Content = "Selected ID: " + f.Id;
            //show_AuctionWindow(f);
        }

        void show_AuctionWindow(Db.Foreclosure d)
        {
            //ForeclosureWindow aw;
            //if (!foreclosures2AuctionWindow.TryGetValue(d, out aw)
            //    || (aw == null || !aw.IsLoaded)
            //    )
            //{
            //    aw = ForeclosureWindow.OpenDialog(d.Id);
            //    foreclosures2AuctionWindow[d] = aw;
            //    System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(aw);
            //    aw.Closed += delegate
            //      {
            //          Db.Foreclosure f = aw.fields.DataContext as Db.Foreclosure;
            //          if (f == null)
            //              return;
            //          foreclosures2AuctionWindow.Remove(f);
            //      };
            //    aw.Show();
            //}
            //else
            //{
            //    aw.BringIntoView();
            //    aw.Activate();
            //}
            ForeclosureWindow.OpenDialog(d.Id);
        }
        //Dictionary<Db.Foreclosure, ForeclosureWindow> foreclosures2AuctionWindow = new Dictionary<Db.Foreclosure, ForeclosureWindow>();

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

        private void login_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow.OpenDialog();
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
            Db.Foreclosure f = list.SelectedItem as Db.Foreclosure;
            if (f == null)
            {
                ForeclosureWindow.OpenDialog();
                return;
            }
            show_AuctionWindow(f);
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

            PropertyInfo[] pis_ = typeof(Db.Foreclosure).GetProperties();
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
                Db.Foreclosure d = (Db.Foreclosure)o;
                foreach (PropertyInfo pi in pis)
                {
                    object v = pi.GetValue(d);
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
                if(o is Db.Foreclosure)
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
                if (Settings.View.SearchedColumns.Contains(list.Columns[i].Header))
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
            e.Column.IsReadOnly = false;
            e.Column.Width = new DataGridLength(100, DataGridLengthUnitType.SizeToHeader);
            e.Column.HeaderTemplate = Resources["Header"] as DataTemplate;//to keep '_' in names
            e.Column.CanUserSort = true;
            e.Column.CanUserResize = true;
            e.Column.CanUserReorder = true;
            if (e.PropertyType == typeof(DateTime?))
            {
                DataGridTextColumn tc = e.Column as DataGridTextColumn;
                if (tc != null)
                    tc.Binding.StringFormat = DATE_FORMAT;
                //e.Column.SortMemberPath = e.Column.Header.ToString();
            }
        }
        readonly string DATE_FORMAT = "MM/dd/yyyy";

        private void list_TargetUpdated(object sender, DataTransferEventArgs e)
        {

        }

        private void list_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            edited_item = list.SelectedItem as Db.Foreclosure;
        }
        Db.Foreclosure edited_item = null;

        private void delete_Click(object sender, RoutedEventArgs e)
        {
            Db.Foreclosure f = list.SelectedItem as Db.Foreclosure;
            if (f == null)
                return;
            if (!Message.YesNo("You are abount deleting record [Id=" + f.Id + "]. Proceed?"))
                return;
            foreclosures.Delete(f.Id);
        }
    }

    public static class DataGridExtensions
    {
        public static DataGridCell GetCell(this DataGrid grid, DataGridRow row, int columnIndex = 0)
        {
            if (row == null)
                return null;

            var presenters = row.FindVisualChildrenOfType< System.Windows.Controls.Primitives.DataGridCellsPresenter > ().ToList();
            if (presenters.Count  <1)
                return null;

            var cell = (DataGridCell)presenters[0].ItemContainerGenerator.ContainerFromIndex(columnIndex);
            if (cell != null)
                return cell;

            // now try to bring into view and retreive the cell
            grid.ScrollIntoView(row, grid.Columns[columnIndex]);
            cell = (DataGridCell)presenters[0].ItemContainerGenerator.ContainerFromIndex(columnIndex);

            return cell;
        }
    }
}