using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http;
using System.IO;
//using MongoDB.Bson;
//using MongoDB.Driver;
using LiteDB;

namespace Cliver.Foreclosures
{
    public partial class Db : IDisposable
    {
        static readonly string db_dir = Log.GetAppCommonDataDir();
        static readonly string db_file = Db.db_dir + "\\db.litedb";

        public Db()
        {
            lock (dbs)
            {
                dbs.Add(this);
                if (db == null)
                    db = new LiteDatabase(Db.db_file);
            }
        }
        static readonly List<Db> dbs = new List<Db>();
        static LiteDatabase db = null;

        ~Db()
        {
            Dispose();
        }

        public void Dispose()
        {
            lock (dbs)
            {
                dbs.Remove(this);
                if (dbs.Count < 1)
                    if (db != null)
                    {
                        db.Dispose();
                        db = null;
                    }
            }
        }

        static string get_normalized(string s)
        {
            if (s == null)
                return null;
            return System.Text.RegularExpressions.Regex.Replace(s.ToLower(), @" +", " ").Trim();
        }

        public class Document
        {
            public int Id { get; set; }
        }

        public abstract class Table<D> : Db where D:Document
        {
            public Table()
            {                
                table = db.GetCollection<D>(this.GetType().Name);
            }
            protected readonly LiteCollection<D> table = null;

            public virtual int Save(D document)
            {
                lock (db)
                {
                    if (document.Id == 0)
                    {
                        var b = table.Insert(document);
                        return b.AsInt32;
                    }
                    table.Update(document);
                    return document.Id;
                }
            }

            public List<D> GetAll()
            {
                lock (db)
                {
                    return table.FindAll().ToList();
                }
            }

            public void Drop()
            {
                lock (db)
                {
                    db.DropCollection(table.Name);
                }
            }

            public int Count()
            {
                lock (db)
                {
                    return table.Count();
                }
            }
        }
    }
}