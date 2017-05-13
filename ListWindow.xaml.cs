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

namespace Cliver.Foreclosures
{
    public partial class ListWindow : Window
    {
        public static void Open()
        {
            if (lw == null || !lw.IsLoaded)
                lw = new ListWindow();
            lw.Show();
        }

        public static void ItemSaved(Db.Foreclosures.Foreclosure f)
        {
            if (lw == null || !lw.IsLoaded)
                return;

            for (int i = lw.list.Items.Count - 1; i >= 0; i--)
            {
                Item t = (Item)lw.list.Items[i];
                if (f.Id == t.Foreclosure.Id)
                {
                    t.Foreclosure = f;
                    lw.list.Items.Refresh();
                    return;
                }
            }
            lw.list.Items.Add(new Item { Foreclosure = f });
            lw.list.Items.Refresh();
        }

        public static void ItemDeleted(int foreclosure_id)
        {
            if (lw == null || !lw.IsLoaded)
                return;

            for (int i = lw.list.Items.Count - 1; i >= 0; i--)
            {
                Item t = (Item)lw.list.Items[i];
                if (foreclosure_id == t.Foreclosure.Id)
                {
                    lw.list.Items.Remove(t);
                    return;
                }
            }
        }

        static ListWindow lw = null;

        ListWindow()
        {
            InitializeComponent();

            Icon = AssemblyRoutines.GetAppIconImageSource();

            foreach (Db.Foreclosures.Foreclosure f in Db.Foreclosures.GetAll())
                list.Items.Add(new Item { Foreclosure = f });


            Closing += delegate (object sender, System.ComponentModel.CancelEventArgs e)
            {
            };

            Closed += delegate
            {
                lw = null;
            };
        }

        public class Item
        {
            public Db.Foreclosures.Foreclosure Foreclosure { get; set; }
            public AuctionWindow Aw = null;
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void export_Click(object sender, RoutedEventArgs e)
        {
        }

        private void new_Click(object sender, RoutedEventArgs e)
        {
            AuctionWindow aw = new AuctionWindow();
            System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(aw);
            aw.Show();
        }

        //private void show_Click(object sender, RoutedEventArgs e)
        //{
        //    Item i = (Item)((Button)e.Source).DataContext;
        //    if (i.Aw == null || !i.Aw.IsLoaded)
        //    {
        //        i.Aw = new AuctionWindow(i.Foreclosure.Id);
        //        System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(i.Aw);
        //        i.Aw.Show();
        //    }
        //    else
        //    {
        //        i.Aw.BringIntoView();
        //        i.Aw.Activate();
        //    }
        //}

        private void list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (Item i in e.AddedItems)
            {
                show_AuctionWindow(i);
            }
        }

        void show_AuctionWindow(Item i)
        {
            if (i.Aw == null || !i.Aw.IsLoaded)
            {
                i.Aw = new AuctionWindow(i.Foreclosure.Id);
                System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(i.Aw);
                i.Aw.Show();
            }
            else
            {
                i.Aw.BringIntoView();
                i.Aw.Activate();
            }
        }

        private void list_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListViewItem lvi = sender as ListViewItem;
            if (lvi == null)
                return;
            Item i = (Item)lvi.Content;
            if (i == null)
                return;
            show_AuctionWindow(i);
            e.Handled = true;
        }
    }
}