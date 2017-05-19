using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http;
using System.IO;
using System.Reflection;

namespace Cliver.Foreclosures
{
    public partial class Db
    {
        public class Json
        {
            public abstract class Table<D> where D : Document
            {
                //public static T Get<T>() where T: Table<D>, new()
                //{
                //    T y = new T();
                //    return null;
                //}

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
                        //table_types2list.Remove(GetType());
                        lock (tables)
                        {
                            tables.Remove(this);
                            if (tables.Count < 1 && !keep_open)
                                table_types2list.Clear();
                        }
                    }
                }

                public List<D> GetAll()
                {
                    return table;
                }

                //abstract public void Refresh();

                protected static void refresh_json_file_by_request(string url)
                {
                    Type t = MethodBase.GetCurrentMethod().DeclaringType;
                    Log.Main.Inform("Refreshing table: " + t.Name);
                    HttpResponseMessage rm = http_client.GetAsync(url).Result;
                    if (!rm.IsSuccessStatusCode)
                        throw new Exception("Could not refresh table: " + rm.ReasonPhrase);
                    if (rm.Content == null)
                        throw new Exception("Response content is null.");
                    string s = rm.Content.ReadAsStringAsync().Result;
                    System.IO.File.WriteAllText(db_dir + "\\" + t.Name + ".json", s);
                }

                protected static void refresh_json_file_by_file<D>(string file) where D : Document, new()
                {
                    Type t = MethodBase.GetCurrentMethod().DeclaringType;
                    Log.Main.Inform("Refreshing table: " + t.Name);
                    string[] ls = File.ReadAllLines(file);
                    string[] hs = ls[0].Split(',');
                    Dictionary<string, int> hs2i = new Dictionary<string, int>();
                    for (int i = 0; i < hs.Length; i++)
                        hs2i[hs[i]] = i;
                    PropertyInfo[] pis = typeof(D).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    List<D> ds = new List<D>();
                    for (int i = 1; i < ls.Length; i++)
                    {
                        string[] vs = ls[i].Split(',');
                        D d = new D();
                        foreach (PropertyInfo pi in pis)
                            pi.SetValue(d, vs[hs2i[pi.Name]]);
                        ds.Add(d);
                    }
                    string s = SerializationRoutines.Json.Serialize(ds);
                    File.WriteAllText(db_dir + "\\" + t.Name + ".json", s);
                }
            }
        }
    }
}