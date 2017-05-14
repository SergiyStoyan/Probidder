//********************************************************************************************
//Author: Sergey Stoyan, CliverSoft.com
//        http://cliversoft.com
//        stoyan@cliversoft.com
//        sergey.stoyan@gmail.com
//        27 February 2007
//Copyright: (C) 2007, Sergey Stoyan
//********************************************************************************************

using System;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Diagnostics;
using Cliver;
using System.Configuration;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Windows.Input;
using GlobalHotKey;

namespace Cliver.Foreclosures
{
    public class Service
    {
        static Service()
        {
        }

        public delegate void OnStateChanged();
        public static event OnStateChanged StateChanged = null;

        public static bool Running
        {
            set
            {
                if(value)
                    stop.Reset();
                else
                    stop.Set();
                set_hot_keys(value);
                //set_db_refresher(value);
                StateChanged?.Invoke();
                Log.Inform("Service: " + value);
            }
            get
            {
                return !stop.WaitOne(0);
            }
        }

        static ManualResetEvent stop = new ManualResetEvent(false);

        static void set_db_refresher(bool on)
        {
            if (!on)
            {
                if (db_refresher_t != null && db_refresher_t.IsAlive)
                {
                    if (!db_refresher_t.Join(1000))
                        db_refresher_t.Abort();
                }
                return;
            }
            if (db_refresher_t != null && db_refresher_t.IsAlive)
                return;
            db_refresher_t = ThreadRoutines.StartTry(() =>
            {
                while (true)
                {
                    if (Settings.General.NextDbRefreshTime <= DateTime.Now)
                    {
                        DateTime start_db_refresh_time = DateTime.Now;
                        Thread t = Db.BeginRefresh();
                        t.Join();
                    }
                    if (stop.WaitOne(10000))
                        return;
                }
            });
        }
        static Thread db_refresher_t = null;

        static void set_hot_keys(bool listen)
        {
            if (key_manager != null)
            {
                key_manager.Dispose();
                key_manager = null;
            }
            if (!listen)
                return;
            if (Settings.General.TicketKey != System.Windows.Input.Key.None)
            {
                key_manager = new HotKeyManager();
                System.Windows.Input.ModifierKeys mks;
                if (Settings.General.TicketModifierKey1 != ModifierKeys.None)
                {
                    mks = Settings.General.TicketModifierKey1;
                    if (Settings.General.TicketModifierKey2 != ModifierKeys.None)
                        mks |= Settings.General.TicketModifierKey2;
                }
                else
                    mks = ModifierKeys.None;
                var hotKey = key_manager.Register(Settings.General.TicketKey, mks);
                key_manager.KeyPressed += delegate (object sender, KeyPressedEventArgs e)
                {
                   // CreateTicket();
                };
            }
        }
        static HotKeyManager key_manager = null;
    }
}