using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LiteDB;

namespace Cliver.Foreclosures
{
    public partial class Db
    {
        public class LiteDb
        {
            public abstract class Table<D> : Db.Table where D : Document, new()
            {
                public Table()
                {
                   
                }

                protected LiteCollection<D> table
                {
                    get
                    {
                        return (LiteCollection<D>)get_table_info().Core;
                    }
                }

                protected override object create_table_core()
                {
                    if (db == null)
                        db = new LiteDatabase(db_file);
                    return db.GetCollection<D>(GetType().Name);
                }
                protected static LiteDatabase db = null;

                override public void Dispose()
                {
                    lock (table_types2table_info)
                    {
                        base.Dispose();
                        if (table_types2table_info.Count < 1 && db != null)
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
                            Saved?.Invoke(document, true);
                            return;
                        }
                        table.Update(document);
                        Saved?.Invoke(document, false);
                    }
                }
                public delegate void OnSaved(D document, bool inserted);
                public event OnSaved Saved = null;

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

                public virtual void Delete(int document_id)
                {
                    lock (db)
                    {
                        bool success = table.Delete(document_id);
                        Deleted?.Invoke(document_id, success);
                    }
                }
                public delegate void OnDeleted(int document_id, bool sucess);
                public event OnDeleted Deleted = null;

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