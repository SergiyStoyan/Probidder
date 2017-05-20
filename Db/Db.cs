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
        
        static Db()
        {
        }

        static public Modes Mode
        {
            set
            {
                mode = value;
                switch (mode)
                {
                    case Modes.CLOSE_TABLE_ON_DISPOSE:
                        break;
                    case Modes.KEEP_ALL_OPEN_TABLES_EVER:
                        break;
                    case Modes.KEEP_ALL_OPEN_TABLES_WHILE_AT_LEAST_ONE_TABLE_IN_USE:
                        break;
                    default:
                        throw new Exception("No option: " + mode);
                }
            }
            get
            {
                return mode;
            }
        }
        static Modes mode = Modes.CLOSE_TABLE_ON_DISPOSE;

        public enum Modes
        {
            KEEP_ALL_OPEN_TABLES_EVER,
            KEEP_ALL_OPEN_TABLES_WHILE_AT_LEAST_ONE_TABLE_IN_USE,
            CLOSE_TABLE_ON_DISPOSE,
        }

        static public void Close()
        {
            lock (tables)
            {
                foreach (dynamic t in tables)
                    t.Dispose();
                table_types2table_core.Clear();
            }
        }
        
        public class Document
        {
            public int Id { get; set; }
        }

        public abstract class Table:IDisposable
        {
            public Table()
            {
                Name = GetType().Name;
                lock (tables)
                {
                    tables.Add(this);
                }
            }
            public readonly string Name;

            ~Table()
            {
                Dispose();
            }

            virtual public void Dispose()
            {
                lock (table_types2table_core)
                {
                    lock (tables)
                    {
                        tables.Remove(this);
                    }
                    switch (mode)
                    {
                        case Modes.CLOSE_TABLE_ON_DISPOSE:
                            table_types2table_core.Remove(GetType());
                            break;
                        case Modes.KEEP_ALL_OPEN_TABLES_EVER:
                            break;
                        case Modes.KEEP_ALL_OPEN_TABLES_WHILE_AT_LEAST_ONE_TABLE_IN_USE:
                            if (tables.Count < 1)
                                table_types2table_core.Clear();
                            break;
                        default:
                            throw new Exception("No option: " + mode);
                    }
                }
            }

            static public void RefreshFile()
            {
                throw new Exception("Not implemented");
            }
        }
        static readonly HashSet<object> tables = new HashSet<object>();//to monitor count of opened db tables 
        static readonly Dictionary<Type, object> table_types2table_core = new Dictionary<Type, object>();

        public static string GetNormalized(string s)
        {
            if (s == null)
                return null;
            return System.Text.RegularExpressions.Regex.Replace(s.ToLower(), @" +", " ").Trim();
        }
    }
}