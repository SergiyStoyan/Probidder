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
using System.Threading;
using System.Net.Http;
using System.IO;
using LiteDB;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Cliver.Probidder
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
                    case Modes.KEEP_ALL_OPEN_TABLES_FOREVER:
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
        static Modes mode = Db.Modes.KEEP_ALL_OPEN_TABLES_WHILE_AT_LEAST_ONE_TABLE_IN_USE;

        public enum Modes
        {
            KEEP_ALL_OPEN_TABLES_FOREVER,//requires explicite call Close()
            KEEP_ALL_OPEN_TABLES_WHILE_AT_LEAST_ONE_TABLE_IN_USE,
            CLOSE_TABLE_ON_DISPOSE,
        }

        static public void Close()
        {
            lock (table_types2table_info)
            {
                table_types2table_info.Clear();
            }
        }

        public class Document
        {
            public class ObligatoryField : Attribute { }

            public int Id { get; set; }
        }

        public abstract class Table : IDisposable
        {
            public Table()
            {
                Name = GetType().Name;
                get_table_info().Count++;
            }
            public readonly string Name;

            protected TableInfo get_table_info()
            {
                lock (table_types2table_info)
                {
                    TableInfo ti;
                    if (!table_types2table_info.TryGetValue(GetType(), out ti))
                    {
                        ti = new TableInfo { Count = 0, Core = create_table_core() };
                        table_types2table_info[GetType()] = ti;
                    }
                    return ti;
                }
            }

            protected abstract object create_table_core();

            //protected object table_core
            //{
            //    get
            //    {
            //        lock (table_types2table_core)
            //        {
            //            object tc;
            //            if (table_types2table_core.TryGetValue(GetType(), out tc))
            //                return tc;
            //            else
            //                return null;
            //        }
            //    }
            //    set
            //    {
            //        lock (table_types2table_core)
            //        {
            //            table_types2table_core[GetType()] = value;
            //        }
            //    }
            //}

            ~Table()
            {
                Dispose();
            }

            virtual public void Dispose()
            {
                lock (table_types2table_info)
                {
                    if (disposed)
                        return;
                    disposed = true;

                    TableInfo ti;
                    if (!table_types2table_info.TryGetValue(GetType(), out ti))
                        return;
                    ti.Count--;
                    switch (mode)
                    {
                        case Modes.CLOSE_TABLE_ON_DISPOSE:
                            if (ti.Count < 1)
                                table_types2table_info.Remove(GetType());
                            break;
                        case Modes.KEEP_ALL_OPEN_TABLES_FOREVER:
                            break;
                        case Modes.KEEP_ALL_OPEN_TABLES_WHILE_AT_LEAST_ONE_TABLE_IN_USE:
                            if (table_types2table_info.Values.Where(x => x.Count > 0).FirstOrDefault() != null)
                                return;
                            table_types2table_info.Clear();
                            break;
                        default:
                            throw new Exception("No option: " + mode);
                    }
                }
            }
            protected bool disposed = false;
        }
        public class TableInfo
        {
            public int Count = 0;
            public object Core = null;

            public delegate void SavedHandler(Document document, bool inserted);
            public event SavedHandler Saved = null;
            public void InvokeSaved(Document document, bool inserted)
            {
                Saved?.Invoke(document, inserted);
            }

            public delegate void DeletedHandler(int document_id, bool sucess);
            public event DeletedHandler Deleted = null;
            public void InvokeDeleted(int document_id, bool sucess)
            {
                Deleted?.Invoke(document_id, sucess);
            }
        }
        static readonly Dictionary<Type, TableInfo> table_types2table_info = new Dictionary<Type, TableInfo>();

        public static string GetStringNormalized(string s)
        {
            if (s == null)
                return null;
            return Regex.Replace(s.ToUpper(), @"[^\S\r\n]+", " ").Trim();
        }

        public static string GetJsonNormalized(string s)
        {
            if (s == null)
                return null;

            s = Regex.Replace(s, @"(?'quotation'(?<![^\\]\\)\"").*?(?'-quotation'(?<![^\\]\\)\"")", (Match m) =>
            {
                string v = SerializationRoutines.Json.Deserialize<string>(m.Value);
                v = SerializationRoutines.Json.Serialize(GetStringNormalized(v));
                return v;
            });
            return s;
        }
    }
}