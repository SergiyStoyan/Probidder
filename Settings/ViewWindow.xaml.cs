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

namespace Cliver.Probidder
{
    public partial class ViewWindow : Window
    {
        public static void OpenDialog(Settings.ViewSettings.Tables table)
        {
            var w = new ViewWindow(table);
            System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(w);
            w.ShowDialog();
        }

        ViewWindow(Settings.ViewSettings.Tables table)
        {
            InitializeComponent();

            Icon = Win.AssemblyRoutines.GetAppIconImageSource();

            this.table = table;

            Closing += delegate (object sender, System.ComponentModel.CancelEventArgs e)
            {
            };

            Closed += delegate
            {
            };

            ContentRendered += delegate
            {
                Wpf.Routines.TrimWindowSize(this);
            };

            switch (table)
            {
                case Settings.ViewSettings.Tables.Foreclosures:
                    document_type = typeof(Db.Foreclosure);
                    break;
                case Settings.ViewSettings.Tables.Probates:
                    document_type = typeof(Db.Probate);
                    break;
                default:
                    throw new Exception("Unknown option: " + table);
            }

            set(Settings.View);
        }
        readonly Type document_type;
        Settings.ViewSettings.Tables table;

        class Item : INotifyPropertyChanged
        {
            public bool Show { set; get; }
            public bool Editable { set; get; }
            public bool Search { set; get; }
            public string Column { set; get; }

            public void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
            public event PropertyChangedEventHandler PropertyChanged;
        }

        void set(Settings.ViewSettings s)
        {
            Dictionary<string, Item> ns2i = document_type.GetProperties(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance).Select(x => new Item
            {
                Show = s.Tables2Columns[table].Showed.Contains(x.Name) || x.GetCustomAttribute<Db.Document.ObligatoryField>() != null,
                Editable = x.GetCustomAttribute<Db.Document.ObligatoryField>() == null,
                Search = s.Tables2Columns[table].Searched.Contains(x.Name),
                Column = x.Name
            }).ToDictionary(x => x.Column, x => x);
            List<Item> ii = new List<Item>();
            foreach (string n in Settings.View.Tables2Columns[table].Showed)
            {
                Item i;
                if(!ns2i.TryGetValue(n, out i))
                {
                    string m = "Column " + n + " does not exist.";
                    Log.Main.Error(m);
                    if (ListWindow.This.IgnoreColumnDoesNotExist)
                        continue;
                    if (Message.YesNo(m + @"
If the application was recently updated then this error can be ignored because it will be fixed automatically.
However, if it appeared again after re-launch, please contact the vendor as it means a fatal problem.
Ignore this error now?", null, Message.Icons.Error
                    ))
                        ListWindow.This.IgnoreColumnDoesNotExist = true;
                    continue;
                }
                ii.Add(ns2i[n]);
                ns2i.Remove(n);
            }
            foreach (Item i in ns2i.Values)
                ii.Add(i);
            list.ItemsSource = ii;
        }

        private void reset_Click(object sender, RoutedEventArgs e)
        {
            set(Settings.View.GetResetInstance<Settings.ViewSettings>());
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (Item i in list.ItemsSource)
                {
                    if (!i.Show)
                    {
                        PropertyInfo pi = document_type.GetProperties(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance).Where(x => x.Name == i.Column).FirstOrDefault();
                        if (pi.GetCustomAttribute<Db.Document.ObligatoryField>() != null)
                            throw new Exception("Column " + pi.Name + " must be showed!");
                    }
                }

                Settings.View.Tables2Columns[table].Searched.Clear();
                foreach (Item i in list.ItemsSource)
                {
                    if (i.Search)
                        Settings.View.Tables2Columns[table].Searched.Add(i.Column);
                }

                List<string> showed_columns0 = Settings.View.Tables2Columns[table].Showed.ToList();
                Settings.View.Tables2Columns[table].Showed.Clear();
                List<Item> column_items = ((List<Item>)list.ItemsSource).ToList();
                foreach (string c in showed_columns0)
                {
                    Item i = column_items.Where(x => x.Column == c && x.Show).FirstOrDefault();
                    if (i != null)
                    {
                        Settings.View.Tables2Columns[table].Showed.Add(c);
                        column_items.Remove(i);
                    }
                }
                foreach (Item i in column_items)
                {
                    if (i.Show)
                        Settings.View.Tables2Columns[table].Showed.Insert(0, i.Column);
                }

                Settings.View.Save();
                //Config.Reload();
                ListWindow.This?.OrderColumns(table);

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