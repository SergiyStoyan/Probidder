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

namespace Cliver.Foreclosures
{
    public partial class ListWindow : Window
    {
        public static void OpenDialog()
        {
            if (lw == null || !lw.IsLoaded)
            {
                lw = new ListWindow();
                System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(lw);
            }
            lw.ShowDialog();
        }

        static ListWindow lw = null;

        public static ListWindow This
        {
            get
            {
                return lw;
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
                lw = null;
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

            foreclosures.Saved+=Foreclosures_Saved;
            foreclosures.Deleted+=Foreclosures_Deleted;

            list.ItemContainerGenerator.StatusChanged += delegate
              {//needed for highlighting search keyword
                  highlight(list);
              };

            Set();
        }

        public void Set()
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                GridViewColumnCollection cs = ((GridView)list.View).Columns;
                cs.Clear();
                foreach (string f in Settings.View.ShowedColumns)
                {
                    GridViewColumn c = new GridViewColumn();
                    c.Header = f;
                    c.HeaderTemplate = Resources["ArrowLess"] as DataTemplate;
                    c.DisplayMemberBinding = new Binding(f);
                    cs.Add(c);
                }
                fill();
            }));
        }

        private void Foreclosures_Deleted(int document_id, bool sucess)
        {
            if (lw == null || !lw.IsLoaded)
                return;

            fill();
        }

        private void Foreclosures_Saved(Db.Document document, bool inserted)
        {
            if (lw == null || !lw.IsLoaded)
                return;

            fill();
        }

        Db.Foreclosures foreclosures = new Db.Foreclosures();

        void fill()
        {
            //list.ItemsSource = foreclosures.GetAll().Select(x => new Item { Foreclosure = x });
            list.ItemsSource = foreclosures.GetAll();
            filter();
            sort();
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
                d.Description = "Choose a folder where to save the exported file.";
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
            AuctionWindow.OpenNew();
        }

        private void list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (Db.Foreclosure d in e.AddedItems)
                show_AuctionWindow(d);
        }

        void show_AuctionWindow(Db.Foreclosure d)
        {
            AuctionWindow aw;
            if (!foreclosures2AuctionWindow.TryGetValue(d, out aw)
                || (aw == null || !aw.IsLoaded)
                )
            {
                aw = AuctionWindow.OpenNew(d.Id);
                foreclosures2AuctionWindow[d] = aw;
                System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(aw);
                aw.Closed += delegate
                  {
                      Db.Foreclosure f = aw.fields.DataContext as Db.Foreclosure;
                      if (f == null)
                          return;
                      foreclosures2AuctionWindow.Remove(f);
                  };
                aw.Show();
            }
            else
            {
                aw.BringIntoView();
                aw.Activate();
            }
        }
        Dictionary<Db.Foreclosure, AuctionWindow> foreclosures2AuctionWindow = new Dictionary<Db.Foreclosure, AuctionWindow>();

        private void list_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListViewItem lvi = sender as ListViewItem;
            if (lvi == null)
                return;
            Db.Foreclosure d = (Db.Foreclosure)lvi.Content;
            if (d == null)
                return;
            show_AuctionWindow(d);
            e.Handled = true;
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

        private void login_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow.OpenDialog();
        }

        private void database_Click(object sender, RoutedEventArgs e)
        {
            DatabaseWindow.OpenDialog();
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

        private void list_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = e.OriginalSource as GridViewColumnHeader;
            if (column == null)
                return;

            {//should be removed if multi-column sort
                List<System.Collections.DictionaryEntry> cs = sorted_columns2direction.Cast<System.Collections.DictionaryEntry>().ToList();
                for (int i = cs.Count - 1; i >= 0; i--)
                {
                    GridViewColumnHeader c = (GridViewColumnHeader)cs[i].Key;
                    if (c == column)
                        continue;
                    sorted_columns2direction.Remove(c);
                    c.Column.HeaderTemplate = Resources["ArrowLess"] as DataTemplate;
                    c.Column.Width = c.ActualWidth - 20;
                }
            }

            ListSortDirection direction;
            if (sorted_columns2direction.Contains(column))
            {
                direction = (ListSortDirection)sorted_columns2direction[column];
                direction = direction == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                sorted_columns2direction[column] = direction;
            }
            else
            {
                direction = ListSortDirection.Ascending;
                sorted_columns2direction.Add(column, direction);
                column.Column.Width = column.ActualWidth + 20;
            }

            if (direction == ListSortDirection.Ascending)
                column.Column.HeaderTemplate = Resources["ArrowUp"] as DataTemplate;
            else
                column.Column.HeaderTemplate = Resources["ArrowDown"] as DataTemplate;

            sort();
        }

        void sort()
        {
            ICollectionView cv = CollectionViewSource.GetDefaultView(list.ItemsSource);
            cv.SortDescriptions.Clear();
            foreach (GridViewColumnHeader c in sorted_columns2direction.Keys)
            {
                string header;
                Binding b = c.Column.DisplayMemberBinding as Binding;
                if (b != null)
                    header = b.Path.Path;
                else
                    header = c.Column.Header.ToString();
                cv.SortDescriptions.Add(new SortDescription(header, (ListSortDirection)sorted_columns2direction[c]));
            }
        }
        System.Collections.Specialized.OrderedDictionary sorted_columns2direction = new System.Collections.Specialized.OrderedDictionary { };

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
            ICollectionView cv = CollectionViewSource.GetDefaultView(list.ItemsSource);
            if (String.IsNullOrEmpty(k) || search.Visibility != Visibility.Visible)
            {
                filter_regex = null;
                cv.Filter = null;
                return;
            }

            PropertyInfo[] pis_ = typeof(Db.Foreclosure).GetProperties();
            List<PropertyInfo> pis = new List<PropertyInfo>();
            //foreach (GridViewColumn c in ((GridView)list.View).Columns)
            //{
            //    Binding b = c.DisplayMemberBinding as Binding;
            //    if (b == null)
            //        continue;
            //    string field = b.Path.Path;
            //    PropertyInfo pi = pis_.FirstOrDefault(x => x.Name == field);
            //    if (pi == null)
            //        continue;
            //    pis.Add(pi);
            //}
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
                    string s = v.ToString();
                    if (!string.IsNullOrEmpty(s) && filter_regex.IsMatch(s))
                        return true;
                }
                return false;
            };
        }
        Regex filter_regex = null;
        List<int> searched_columns = new List<int>();
        private void highlight(ListView lv)
        {
            if (filter_regex == null)
                return;

            searched_columns.Clear();
            for (int i = 0; i < Settings.View.ShowedColumns.Count; i++)
                if (Settings.View.SearchedColumns.Contains(Settings.View.ShowedColumns[i]))
                    searched_columns.Add(i);

            foreach (ListViewItem lvi in lv.FindChildrenOfType<ListViewItem>())
            {
                put(lvi);
                foreach (TextBlock tb in lvi.FindChildrenOfType<TextBlock>())
                    highlight_TextBlock(tb);
            }
        }
        void put(DependencyObject o)
        {
            foreach (DependencyObject f in o.GetChildren())
            {
                UIElement e = f as UIElement;
                if (e != null)
                    g += "," + Grid.GetColumn(e);
                put(f);
            }
        }
            string g = "";
        private void highlight_TextBlock(TextBlock tb)
        {
            if (tb == null)
                return;
            string g = "";
            for (FrameworkElement f = tb.Parent as FrameworkElement; f != null; f = f.Parent as FrameworkElement)
            {
                g += "," + Grid.GetColumn(f);
            }

            if (!searched_columns.Contains(Grid.GetColumn(tb)))
                return;
            string text = tb.Text;
            tb.Inlines.Clear();
            if (filter_regex == null)
            {
                tb.Inlines.Add(text);
                return;
            }
            bool match = false;
            foreach (string item in filter_regex.Split(text))
            {
                if (match)
                {
                    Run r = new Run(item);
                    r.Background = Settings.View.SearchHighlightColor;
                    tb.Inlines.Add(r);
                }
                else
                    tb.Inlines.Add(item);
                match = !match;
            }
        }
    }
}