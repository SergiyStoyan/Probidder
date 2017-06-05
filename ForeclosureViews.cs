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

namespace Cliver.Foreclosures
{
    public partial class ForeclosureView
    {
        public class Collection : ObservableCollection<ForeclosureView>, IDisposable
        {
            public Collection(Window owner)
            {
                Items = new ObservableCollection<ForeclosureView>(fs.GetAll().Select(x => new ForeclosureView(x)).ToList());
                this.owner = owner;
            }
            public readonly ObservableCollection<ForeclosureView> Items;
            Db.Foreclosures fs = new Db.Foreclosures();
            readonly Window owner;

            ~Collection()
            {
                Dispose();
            }

            public void Dispose()
            {
                if (fs != null)
                {
                    fs.Dispose();
                    fs = null;
                }
            }

            public int Count()
            {
                return fs.Count();
            }

            public List<ForeclosureView> Get(Func<ForeclosureView, bool> query)
            {
                return Items.Where(query).ToList();
            }

            public ForeclosureView GetPrevious(ForeclosureView fw)
            {
                return Items.Where(x => x.Id < fw.Id).OrderByDescending(x => x.Id).FirstOrDefault();
            }

            public ForeclosureView GetNext(ForeclosureView fw)
            {
                if (fw == null)
                    fw = GetFirst();
                return Items.Where(x => x.Id > fw.Id).OrderBy(x => x.Id).FirstOrDefault();
            }

            public ForeclosureView GetFirst()
            {
                return Items.OrderBy(x => x.Id).FirstOrDefault();
            }

            public ForeclosureView GetLast()
            {
                return Items.OrderByDescending(x => x.Id).FirstOrDefault();
            }

            public void Delete(ForeclosureView fw)
            {
                fs.Delete(fw.Model.Id);
                owner.Dispatcher.Invoke(() =>
                {
                    Items.Remove(fw);
                    Deleted?.Invoke();
                });
            }
            public delegate void DeletedHandler();
            public event DeletedHandler Deleted = null;

            public void Update(ForeclosureView fw)
            {
                owner.Dispatcher.Invoke(() =>
                {
                    if (Items.Where(x => x == fw).FirstOrDefault() == null)
                    {
                        Items.Add(fw);
                        Added?.Invoke();
                    }
                    else
                        fw.OnPropertyChanged(null);
                });
            }
            public delegate void AddedHandler();
            public event AddedHandler Added = null;

        }
    }
}