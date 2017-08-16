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
using System.Reflection;
using System.Collections;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;

namespace Cliver.Probidder
{
    public interface IViews : IDisposable
    {
        int Count();
        void Drop();
        void Delete(IView v);
        void Update(IView v);
        IView GetPrevious(IView v);
        IView GetNext(IView v);
        IView GetFirst_();
        IView GetLast_();
        List<object> Get(Func<object, bool> query = null);
    }

    public partial class View<D> 
    {
        public class Views<V, T> : ObservableCollection<V>, IViews where V : View<D> where T : Db.LiteDb.Table<D>, new()
        {
            static public Views<V, T> Create(Window owner)
            {
                T t = new T();
                List<V> vs = t.GetAll().Select(x => (V)Activator.CreateInstance(typeof(V), new[] { x })).ToList();
                return new Views<V, T>(owner, t, vs);
            }

            Views(Window owner, T table, List<V> vs) : base(vs)
            {
                this.table = table;
                this.owner = owner;
            }
            readonly T table = new T();
            readonly Window owner;

            ~Views()
            {
                Dispose();
            }

            public void Dispose()
            {
                if (table != null)
                {
                    table.Dispose();
                    //table = null;
                }
            }

            public int Count()
            {
                return table.Count();
            }

            public List<V> Get(Func<V, bool> query)
            {
                return this.Where(query).ToList();
            }
            public List<object> Get(Func<object, bool> query)
            {
                return this.Where(query).ToList();
            }

            public V GetPrevious(V fw)
            {
                return this.Where(x => x.Id < fw.Id).OrderByDescending(x => x.Id).FirstOrDefault();
            }
            public IView GetPrevious(IView v)
            {
                return GetPrevious((V)v);
            }

            public V GetNext(V fw)
            {
                if (fw == null)
                    fw = GetFirst();
                return this.Where(x => x.Id > fw.Id).OrderBy(x => x.Id).FirstOrDefault();
            }
            public IView GetNext(IView v)
            {
                return GetNext((V)v);
            }

            public V GetFirst()
            {
                return this.OrderBy(x => x.Id).FirstOrDefault();
            }
            public IView GetFirst_()
            {
                return GetFirst();
            }

            public V GetLast()
            {
                return this.OrderByDescending(x => x.Id).FirstOrDefault();
            }
            public IView GetLast_()
            {
                return GetLast();
            }

            public void Delete(V fw)
            {
                table.Delete(fw.Model.Id);
                owner.Dispatcher.Invoke(() =>
                {
                    this.Remove(fw);
                });
            }
            public void Delete(IView v)
            {
                Delete((V)v);
            }

            public void Update(V fw)
            {
                owner.Dispatcher.Invoke(() =>
                {
                    table.Save(fw.Model);
                    if (this.Where(x => x == fw).FirstOrDefault() == null)
                    {
                        this.Add(fw);
                    }
                    //else
                    //    fw.OnPropertyChanged(null);
                    fw.edited = false;
                });
            }
            public void Update(IView v)
            {
                Update((V)v);
            }

            public void Drop()
            {
                owner.Dispatcher.Invoke(() =>
                {
                    table.Drop();
                    this.Clear();
                });
            }
        }
    }
}