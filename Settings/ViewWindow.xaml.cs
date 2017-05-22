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

            int r = 0;
            foreach (PropertyInfo pi in typeof(Db.Foreclosure).GetProperties())
            {
                Columns.RowDefinitions.Add(new RowDefinition());
                r++;
                {
                    CheckBox c = new CheckBox();
                    c.SetValue(Grid.RowProperty, r);
                    c.SetValue(Grid.ColumnProperty, 0);
                    c.IsChecked = Settings.View.ShowedColumns.Contains(pi.Name);
                    c.Padding = new Thickness(0);
                    c.HorizontalAlignment = HorizontalAlignment.Center;
                    Columns.Children.Add(c);
                }
                {
                    CheckBox c = new CheckBox();
                    c.SetValue(Grid.RowProperty, r);
                    c.SetValue(Grid.ColumnProperty, 1);
                    c.IsChecked = Settings.View.SearchedColumns.Contains(pi.Name);
                    c.Padding = new Thickness(0);
                    c.HorizontalAlignment = HorizontalAlignment.Center;
                    Columns.Children.Add(c);
                }
                {
                    TextBlock l = new TextBlock();
                    l.SetValue(Grid.RowProperty, r);
                    l.SetValue(Grid.ColumnProperty, 2);
                    l.Text = pi.Name;
                    l.Padding = new Thickness(0);
                    l.HorizontalAlignment = HorizontalAlignment.Left;
                    Columns.Children.Add(l);
                }
            }
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Settings.View.ShowedColumns.Clear();
                Settings.View.SearchedColumns.Clear();
                List<UIElement> es = Columns.Children.Cast<UIElement>().ToList();
                for (int i = Columns.RowDefinitions.Count - 1; i > 0; i--)
                {
                    CheckBox c1 = (CheckBox)es.First(x => Grid.GetRow(x) == i && Grid.GetColumn(x) == 0);
                    CheckBox c2 = (CheckBox)es.First(x => Grid.GetRow(x) == i && Grid.GetColumn(x) == 1);
                    TextBlock l = (TextBlock)es.First(x => Grid.GetRow(x) == i && Grid.GetColumn(x) == 2);
                    if (c1.IsChecked == true)
                        Settings.View.ShowedColumns.Add(l.Text);
                    if (c2.IsChecked == true)
                        Settings.View.SearchedColumns.Add(l.Text);
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
    }
}