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

        public static void SaveItem(Db.Foreclosures.Foreclosure f)
        {
        }

        public static void DeleteItem(Db.Foreclosures.Foreclosure f)
        {
        }

        static ListWindow lw = null; 
           
        ListWindow()
        {
            InitializeComponent();

            Icon = AssemblyRoutines.GetAppIconImageSource();
            
            foreach (Db.Foreclosures.Foreclosure f in Db.Foreclosures.GetAll())
                list.Items.Add(new Item { Text = f.FILING_DATE, Id = f.Id });


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
            public string Text;
            public int Id;
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
            aw.Show();
        }
    }
}