using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http;
using System.IO;
using LiteDB;

namespace Cliver.Foreclosures
{
    public partial class Db
    {
        static readonly string db_dir = Log.GetAppCommonDataDir();
        static readonly string db_file = db_dir + "\\db.litedb";

        static readonly HashSet<object> tables = new HashSet<object>();//to monitor count of opened db tables 

        static Db()
        {
        }

        static public bool KeepOpen
        {
            set
            {
                keep_open = value;
            }
            get
            {
                return keep_open;
            }
        }
        static bool keep_open = false;

        static public void Close()
        {
            lock (tables)
            {
                bool kp = keep_open;
                keep_open = false;
                foreach (dynamic t in tables)
                    t.Dispose();
                keep_open = kp;
            }
        }

        public class Document
        {
            public int Id { get; set; }
        }

        public static string GetNormalized(string s)
        {
            if (s == null)
                return null;
            return System.Text.RegularExpressions.Regex.Replace(s.ToLower(), @" +", " ").Trim();
        }
    }
}