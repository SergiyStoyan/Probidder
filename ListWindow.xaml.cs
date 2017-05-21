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

            fill();

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

            foreclosures.SetOnSaved(Foreclosures_Saved);
            foreclosures.SetOnDeleted(Foreclosures_Deleted);
        }

        private void Foreclosures_Deleted(int document_id, bool sucess)
        {
            if (lw == null || !lw.IsLoaded)
                return;

            //for (int i = lw.list.Items.Count - 1; i >= 0; i--)
            //{
            //    Db.Foreclosure d = (Db.Foreclosure)lw.list.Items[i];
            //    if (document_id == d.Id)
            //    {
            //        lw.list.Items.Remove(d);
            //        return;
            //    }
            //}
            fill();
        }

        private void Foreclosures_Saved(Db.Document document, bool inserted)
        {
            if (lw == null || !lw.IsLoaded)
                return;

            //int i = lw.list.Items.IndexOf(document);
            //if (i >= 0)
            //{
            //    lw.list.Items.Refresh();
            //    return;
            //}
            //lw.list.Items.Add(document);
            //lw.list.Items.Refresh();
            fill();
        }

        Db.Foreclosures foreclosures = new Db.Foreclosures();

        void fill()
        {
            //list.ItemsSource = foreclosures.GetAll().Select(x => new Item { Foreclosure = x });
            list.ItemsSource = foreclosures.GetAll();
            filter();
            sort();

            //string k = keyword.Text;
            //foreach (GridViewColumn c in ((GridView)list.View).Columns)
            //{
            //    string field;
            //    Binding b = c.DisplayMemberBinding as Binding;
            //    if (b != null)
            //        field = b.Path.Path;
            //}
            //list.ItemsSource = foreclosures.Get2(x=> filter(x, k));
        }
        //static bool filter(Db.Foreclosure foreclosure, string keyword)
        //{
        //    foreach (PropertyInfo pi in pis)
        //        if (!Regex.IsMatch(pi.GetValue(foreclosure).ToString(), Regex.Escape(keyword), RegexOptions.IgnoreCase))
        //            return false;
        //    return true;
        //}
        //static PropertyInfo[] pis = typeof(Db.Foreclosure).GetProperties(BindingFlags.Public);

        //public class Item
        //{
        //    public Db.Foreclosure Foreclosure { get; set; }
        //    public AuctionWindow Aw = null;
        //}

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
                    c.Column.HeaderTemplate = null;
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
                    header = (string)c.Column.Header;
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
                cv.Filter = null;
                return;
            }

            //foreach (GridViewColumn c in ((GridView)list.View).Columns)
            //{
            //    string field;
            //    Binding b = c.DisplayMemberBinding as Binding;
            //    if (b != null)
            //        field = b.Path.Path;
            //}
            PropertyInfo[] pis = typeof(Db.Foreclosure).GetProperties();
            cv.Filter = o =>
            {
                Db.Foreclosure d = (Db.Foreclosure)o;
                foreach (PropertyInfo pi in pis)
                {
                    object v = pi.GetValue(d);
                    if (v == null)
                        continue;
                    string s = v.ToString();
                    if (!string.IsNullOrEmpty(s) && Regex.IsMatch(s, Regex.Escape(k), RegexOptions.IgnoreCase))
                        return true;
                }
                return false;
            };
        }
    }
}