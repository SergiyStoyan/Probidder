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
        public class LiteDb
        {
            public abstract class Table<D> where D : Document
            {
                public Table()
                {
                    lock (table_types2collection)
                    {
                        if (db == null)
                            db = new LiteDatabase(db_file);
                        if (!table_types2collection.TryGetValue(GetType(), out table))
                        {
                            table = db.GetCollection<D>(GetType().Name);
                            table_types2collection[GetType()] = table;
                        }
                    }
                }
                protected readonly LiteCollection<D> table = null;
                protected static LiteDatabase db = null;
                static readonly Dictionary<Type, LiteCollection<D>> table_types2collection = new Dictionary<Type, LiteCollection<D>>();

                ~Table()
                {
                    Dispose();
                }

                public void Dispose()
                {
                    lock (table_types2collection)
                    {
                        table_types2collection.Remove(GetType());
                        if (table_types2collection.Count < 1 && db != null)
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

                public virtual void Save(D document)
                {
                    lock (db)
                    {
                        if (document.Id == 0)
                        {
                            var b = table.Insert(document);
                            document.Id = b.AsInt32;
                        }
                        table.Update(document);
                    }
                }

                public List<D> GetAll()
                {
                    lock (db)
                    {
                        return table.FindAll().ToList();
                    }
                }

                public List<D> Get(Query query)
                {
                    lock (db)
                    {
                        return table.Find(query).ToList();
                    }
                }

                public List<D> Get(System.Linq.Expressions.Expression<Func<D, bool>> query)
                {
                    lock (db)
                    {
                        return table.Find(query).ToList();
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
}