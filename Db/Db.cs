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

namespace Cliver.Foreclosures
{
    public partial class Db
    {
        static readonly string db_dir = Log.GetAppCommonDataDir();
        static readonly string db_file = db_dir + "\\db.litedb";
        
        static Db()
        {
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
            public int Id { get; set; }
        }

        public abstract class Table : IDisposable
        {
            public Table()
            {
                Name = GetType().Name;
            }
            public readonly string Name;

            protected TableInfo get_table_info()
            { 
                lock (table_types2table_info)
                {
                    WeakReference wr;
                    if (!table_types2table_info.TryGetValue(GetType(), out wr)
                        || !wr.IsAlive
                        )
                    {
                        TableInfo ti = new TableInfo { Core = create_table_core() };
                        wr = new WeakReference(ti);
                        table_types2table_info[GetType()] = wr;
                    }
                    return (TableInfo)wr.Target;
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
            }
        }
        public class TableInfo
        {
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
        static readonly Dictionary<Type, WeakReference> table_types2table_info = new Dictionary<Type, WeakReference>();

        public static string GetNormalized(string s)
        {
            if (s == null)
                return null;
            return System.Text.RegularExpressions.Regex.Replace(s.ToLower(), @" +", " ").Trim();
        }
    }
}