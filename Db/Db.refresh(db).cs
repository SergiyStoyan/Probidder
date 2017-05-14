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
        public static Thread BeginRefresh()
        {
            if (refresh_t != null && refresh_t.IsAlive)
                return refresh_t;

            refresh_t = ThreadRoutines.StartTry(() =>
            {
                Log.Inform("Refreshing db.");

                //InfoWindow iw = InfoWindow.Create("Foreclosures", "Refreshing database... Please wait for completion.", null, "OK", null);

                HttpClientHandler handler = new HttpClientHandler();
                http_client = new HttpClient(handler);

                List<Task> tasks = new List<Task>();
                var t = new Task(() =>
                {
                    refresh_table<Db.MortgageTypes, Db.MortgageType>("https://i.probidder.com/api/fields/index.php?type=foreclosures&field=mortgage_type");
                });
                t.Start();
                tasks.Add(t);
                t = new Task(() =>
                {
                    refresh_table<Db.AttorneyPhones, Db.AttorneyPhone>("https://i.probidder.com/api/fields/index.php?type=foreclosures&field=attorney_phone");
                });
                t.Start();
                tasks.Add(t);
                t = new Task(() =>
                {
                    refresh_table<Db.Attorneys, Db.Attorney>("https://i.probidder.com/api/fields/index.php?type=foreclosures&field=attorney");
                });
                t.Start();
                tasks.Add(t);
                t = new Task(() =>
               {
                   refresh_table<Db.Cities, Db.City>("https://i.probidder.com/api/fields/index.php?type=foreclosures&field=city");
               });
                t.Start();
                tasks.Add(t);
                t = new Task(() =>
               {
                   refresh_table<Db.Plaintiffs, Db.Plaintiff>("https://i.probidder.com/api/fields/index.php?type=foreclosures&field=plaintiff");
               });
                t.Start();
                tasks.Add(t);

                if (!File.Exists(db_dir + "\\illinois_postal_codes.csv"))
                {
                    string s = File.ReadAllText(Log.AppDir + "\\illinois_postal_codes.csv");
                    File.WriteAllText(db_dir + "\\illinois_postal_codes.csv", get_normalized(s));
                }

                if (!File.Exists(db_dir + "\\property_codes.csv"))
                    File.Copy(Log.AppDir + "\\property_codes.csv", db_dir + "\\property_codes.csv");

                if (!File.Exists(db_dir + "\\owner_role.csv"))
                    File.Copy(Log.AppDir + "\\owner_role.csv", db_dir + "\\owner_role.csv");

                Task.WaitAll(tasks.ToArray());
                //Log.Inform("Db has been refreshed.");
                //iw.Dispatcher.Invoke(iw.Close);
                Settings.General.NextDbRefreshTime = DateTime.Now.AddSeconds(Settings.General.DbRefreshPeriodInSecs);
                InfoWindow.Create(ProgramRoutines.GetAppName(), "Database has been refreshed successfully.", null, "OK", null, System.Windows.Media.Brushes.White, System.Windows.Media.Brushes.Green);
            },
            (Exception e) =>
            {
                Log.Error(e);
                Log.Error("Could not refresh db.");
                Settings.General.NextDbRefreshTime = DateTime.Now.AddSeconds(Settings.General.DbRefreshRetryPeriodInSecs);
                InfoWindow.Create(ProgramRoutines.GetAppName() + ": database could not refresh!", Log.GetExceptionMessage(e), null, "OK", null, System.Windows.Media.Brushes.Beige, System.Windows.Media.Brushes.Red);
            },
            () =>
            {
                Settings.General.Save();
                if (http_client != null)
                    http_client.Dispose();
            }
            );
            return refresh_t;
        }
        static Thread refresh_t = null;
        static HttpClient http_client;

        static void refresh_table<T,D>(string url) where T:Table<D> where D:Document
        {
            Log.Main.Inform("Refreshing table: " + typeof(T).Name);
            HttpResponseMessage rm = http_client.GetAsync(url).Result;
            if (!rm.IsSuccessStatusCode)
                throw new Exception("Could not refresh table: " + rm.ReasonPhrase);
            if (rm.Content == null)
                throw new Exception("Response content is null.");
            string s = rm.Content.ReadAsStringAsync().Result;
            T table = (T)Activator.CreateInstance(typeof(T));
            table.Drop();
            List<D> ds = SerializationRoutines.Json.Deserialize<List<D>>(s);
            foreach (D d in ds)
                table.Save(d);
        }

        //public static List<string> GetValuesFromTable(string table, string field, Dictionary<string, string> keys2value)
        //{
        //    Dictionary<string, string> ks2v = new Dictionary<string, string>();
        //    foreach (KeyValuePair<string, string> k2v in keys2value)
        //        ks2v[k2v.Key] = get_normalized(k2v.Value);

        //    List<string> vs = new List<string>();
        //    string s = System.IO.File.ReadAllText(db_dir + "\\" + table + ".json");
        //    dynamic json = SerializationRoutines.Json.Deserialize<dynamic>(s);
        //    foreach (dynamic d in (dynamic)json)
        //    {
        //        bool found = true;
        //        foreach (KeyValuePair<string, string> k2v in ks2v)
        //            if (k2v.Value != null && get_normalized((string)d[k2v.Key]) != k2v.Value)
        //            {
        //                found = false;
        //                break;
        //            }
        //        if (found)
        //            vs.Add((string)d[field]);
        //    }
        //    return vs;
        //}

        public static List<string> GetZipCodes(string county, string city)
        {
            county = get_normalized(county);
            city = get_normalized(city);
            List<string> vs = new List<string>();
            string[] ss = File.ReadAllLines(db_dir + "\\illinois_postal_codes.csv");
            foreach (string s in ss)
            {
                string[] fs = s.Split(',');
                if (fs[1] == city && fs[3] == county)
                    vs.Add(fs[0]);
            }
            return vs;
        }

        public static List<string> GetPropertyCodes()
        {
            List<string> vs = new List<string>();
            string[] ss = File.ReadAllLines(db_dir + "\\property_codes.csv");
            foreach (string s in ss)
            {
                string[] fs = s.Split(',');
                vs.Add(fs[0]);
            }
            return vs;
        }

        public static List<string> GetOwnerRole()
        {
            List<string> vs = new List<string>();
            string[] ss = File.ReadAllLines(db_dir + "\\owner_role.csv");
            foreach (string s in ss)
            {
                string[] fs = s.Split(',');
                vs.Add(fs[0]);
            }
            return vs;
        }
    }
}