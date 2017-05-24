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
using System.Collections.ObjectModel;

namespace Cliver.Foreclosures
{
    public partial class AutoCompleteWindow : Window
    {
        public static void OpenDialog()
        {
            var w = new AutoCompleteWindow();
            System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(w);
            w.ShowDialog();
        }

        AutoCompleteWindow()
        {
            InitializeComponent();

            Icon = AssemblyRoutines.GetAppIconImageSource();

            Closing += delegate (object sender, System.ComponentModel.CancelEventArgs e)
            {
            };

            Closed += delegate
            {
            };

            items = new ObservableCollection<Item>(Settings.AutoComplete.Keys2Phrase.Select(x => new Item { Key = x.Key, Phrase = x.Value }));
            //items = new List<Item>(Settings.AutoComplete.Keys2Phrase.Select(x => new Item { Key = x.Key, Phrase = x.Value }));
            list.ItemsSource = items;

            AddHandler(Keyboard.KeyDownEvent, (KeyEventHandler)KeyDownHandler);
            show_TriggerKey();
        }

        private ObservableCollection<Item> items;
        //private List<Item> items;

        class Item : System.ComponentModel.IEditableObject
        {
            public string Key { set; get; }
            public string Phrase { set; get; }


            public void BeginEdit()
            {
                original = this.MemberwiseClone() as Item;
            }
            private Item original;

            public void CancelEdit()
            {
                if (original == null)
                    return;
                this.Key = original.Key;
                this.Phrase = original.Phrase;
            }

            public void EndEdit()
            {
                original = null;
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
                Settings.AutoComplete.TriggerKey = triggerKey;
                Settings.AutoComplete.TriggerModifierKey = triggerModifierKey;
                Settings.AutoComplete.Keys2Phrase = items.ToDictionary(x => x.Key, x => x.Phrase);
                Settings.AutoComplete.Save();

                Config.Reload();

                Close();
            }
            catch (Exception ex)
            {
                Message.Exclaim(ex.Message);
            }
        }

        private void delete_Click(object sender, RoutedEventArgs e)
        {
            object dc = ((Button)e.Source).DataContext;
            Item i = dc as Item;
            if (i == null)
                return;
            items.Remove(i);
        }

        private void list_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit)
                return;
            Item i = e.Row.Item as Item;
            if (i == null)
                return;
            if (string.IsNullOrEmpty(i.Key) || string.IsNullOrEmpty(i.Phrase))
            {
                Message.Exclaim("The fields cannot be empty!");
                i.CancelEdit();
                //list.CancelEdit();
                list.ItemsSource = null;
                list.ItemsSource = items;
                e.Cancel = true;
            }
            if (items.Where(x => x.Key == i.Key).Count() > 1)
            {
                Message.Exclaim("This key exists already!");
                i.CancelEdit();
                //list.CancelEdit();
                list.ItemsSource = null;
                list.ItemsSource = items;
                e.Cancel = true;
            }
            i.EndEdit();
        }

        private void SetTriggerKey_Click(object sender, RoutedEventArgs e)
        {
            if (SetTriggerKey.IsChecked == true)
            {
                TriggerKey.Content = "Press keys...\r\n";
            }
        }

        public void KeyDownHandler(object sender, KeyEventArgs e)
        {
            if (SetTriggerKey.IsChecked != true)
                return;
            ThreadRoutines.StartTry(() =>
            {
                Thread.Sleep(500);
                SetTriggerKey.Dispatcher.Invoke(() =>
                {
                    pressed = null;
                    SetTriggerKey.IsChecked = false;
                    show_TriggerKey();
                });
            });
            if (pressed == null)
                pressed = DateTime.Now;
            triggerKey = e.Key;
            triggerModifierKey = Keyboard.Modifiers;
        }
        DateTime? pressed = null;

        void show_TriggerKey()
        {
            TriggerKey.Content = "Trigger key: " + triggerKey + "\r\nTrigger modifier key: " + triggerModifierKey;
        }
        Key triggerKey = Settings.AutoComplete.TriggerKey;
        ModifierKeys triggerModifierKey = Settings.AutoComplete.TriggerModifierKey;
    }
}