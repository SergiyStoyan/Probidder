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
    public partial class Db
    {
        static readonly string db_dir = Log.GetAppCommonDataDir();
        static readonly string db_file = db_dir + "\\db.litedb";

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

        public abstract class Table<D> where D : Document
        {
            public Table()
            {
                lock (ts)
                {
                    if (db == null)
                        db = new LiteDatabase(db_file);
                    ts.Add(this);
                    table = db.GetCollection<D>(this.GetType().Name);
                }
            }
            protected readonly LiteCollection<D> table = null;
            static LiteDatabase db = null;
            static List<Table<D>> ts = new List<Table<D>>();

            ~Table()
            {
                Dispose();
            }

            public void Dispose()
            {
                lock (ts)
                {
                    ts.Remove(this);
                    if (ts.Count < 1 && db != null)
                    {
                        db.Dispose();
                        db = null;
                    }
                }
            }

            public D GetById(int id)
            {
                lock (db)
                {
                    return table.FindById(id);
                }
            }

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

            public void Delete(int id)
            {
                lock (db)
                {
                    table.Delete(id);
                }
            }

            public int Count()
            {
                lock (db)
                {
                    return table.Count();
                }
            }

            public D Prev(D d)
            {
                lock (db)
                {
                    if (d == null)
                        d = GetLast();
                    return table.Find(x => x.Id < d.Id).OrderByDescending(x => x.Id).FirstOrDefault();
                }
            }

            public D Forward(D d)
            {
                lock (db)
                {
                    if (d == null)
                        d = GetFirst();
                    return table.Find(x => x.Id > d.Id).OrderBy(x => x.Id).FirstOrDefault();
                }
            }

            public D GetFirst()
            {
                lock (db)
                {
                    return table.FindAll().OrderBy(x => x.Id).FirstOrDefault();
                }
            }

            public D GetLast()
            {
                lock (db)
                {
                    return table.FindAll().OrderByDescending(x => x.Id).FirstOrDefault();
                }
            }
        }
    }
}