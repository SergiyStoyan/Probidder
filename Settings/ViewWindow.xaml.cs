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
using System.Reflection;
using System.ComponentModel;

namespace Cliver.Foreclosures
{
    public partial class ViewWindow : Window
    {
        public static void OpenDialog()
        {
            var w = new ViewWindow();
            System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(w);
            w.ShowDialog();
        }

        ViewWindow()
        {
            InitializeComponent();

            Icon = AssemblyRoutines.GetAppIconImageSource();

            Closing += delegate (object sender, System.ComponentModel.CancelEventArgs e)
            {
            };

            Closed += delegate
            {
            };

            ContentRendered += delegate
            {
                WpfRoutines.TrimWindowSize(this);
            };

            list.ItemsSource = typeof(Db.Foreclosure).GetProperties(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance).Where(x => x.GetCustomAttribute<FieldPreparation.IgnoredField>() == null).Select(x => new Item
            {
                Show = Settings.View.ShowedColumns.Contains(x.Name),
                Search = Settings.View.SearchedColumns.Contains(x.Name),
                Column = x.Name
            }
            ).ToList();
        }

        class Item: INotifyPropertyChanged
        {
            public bool Show { set; get; }
            public bool Search { set; get; }
            public string Column { set; get; }

            public void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
            public event PropertyChangedEventHandler PropertyChanged;
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Settings.View.SearchedColumns.Clear();
                foreach (Item i in list.ItemsSource)
                {
                    if (i.Search)
                        Settings.View.SearchedColumns.Add(i.Column);
                }

                List<string> showed_columns0 = Settings.View.ShowedColumns.ToList();
                Settings.View.ShowedColumns.Clear();
                List<Item> column_items = ((List<Item>)list.ItemsSource);
                foreach (string c in showed_columns0)
                {
                    Item i = column_items.Where(x => x.Show && x.Column == c).FirstOrDefault();
                    if (i != null)
                    {
                        Settings.View.ShowedColumns.Add(c);
                        column_items.Remove(i);
                    }
                }
                foreach (Item i in column_items)
                {
                    if (i.Show)
                        Settings.View.ShowedColumns.Insert(0, i.Column);
                }

                Settings.View.Save();
                Config.Reload();

                Close();
            }
            catch (Exception ex)
            {
                Message.Exclaim(ex.Message);
            }
        }

        private void ShowCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var c in list.SelectedCells)
            {
                Item i = c.Item as Item;
                if (i == null)
                    continue;
                if (!i.Show)
                {
                    i.Search = false;
                    i.OnPropertyChanged(null);
                }
            }
        }

        private void SearchCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var c in list.SelectedCells)
            {
                Item i = c.Item as Item;
                if (i == null)
                    continue;
                if (i.Search)
                {
                    i.Show = true;
                    i.OnPropertyChanged(null);
                }
            }
        }
    }
}