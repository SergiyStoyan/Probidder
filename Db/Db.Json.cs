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
        public class Json
        {
            public abstract class Table<D> where D : Document
            {
                public Table()
                {
                    lock (table_types2list)
                    {
                        Name = GetType().Name;
                        if (!table_types2list.TryGetValue(GetType(), out table))
                        {
                            string file = db_dir + "\\" + Name + ".json";
                            if (!File.Exists(file))
                            {
                                if (!Message.YesNo("The app needs data which should be downloaded over the internet. Make sure your computer is connected to the internet and then click Yes. Otherwise, the app will exit."))
                                    Environment.Exit(0);
                                Thread t = Db.BeginRefresh();
                                MessageForm mf = null;
                                Thread tm = ThreadRoutines.StartTry(() =>
                                {
                                    mf = new MessageForm(System.Windows.Forms.Application.ProductName, System.Drawing.SystemIcons.Exclamation, "Getting data from the net. Please wait...", new string[1] { "OK" }, 0, null);
                                    mf.ShowDialog();
                                });
                                t.Join();
                                if (SleepRoutines.WaitForObject(() =>
                                {
                                    return mf;
                                }, 10000) == null)
                                    Log.Main.Exit("SleepRoutines.WaitForObject got null");
                                mf.Invoke(() =>
                                {
                                    try
                                    {
                                        mf.Close();
                                    }
                                    catch { }//if closed already
                                });
                                if (!File.Exists(file))
                                {
                                    Message.Error("Unfrotunately the required data has not been downloaded. Please try later.");
                                    table = new List<D>();
                                }
                            }

                            string s = System.IO.File.ReadAllText(file);
                            table = SerializationRoutines.Json.Deserialize<List<D>>(s);
                            table_types2list[GetType()] = table;
                        }
                    }
                }
                protected readonly List<D> table = null;
                static readonly Dictionary<Type, List<D>> table_types2list = new Dictionary<Type, List<D>>();
                public readonly string Name;

                ~Table()
                {
                    Dispose();
                }

                public void Dispose()
                {
                    lock (table_types2list)
                    {
                        table_types2list.Remove(GetType());
                    }
                }

                public List<D> GetAll()
                {
                    return table;
                }
            }
        }
    }
}