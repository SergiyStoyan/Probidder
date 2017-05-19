using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http;
using System.IO;
using LiteDB;
using System.Reflection;

namespace Cliver.Foreclosures
{
    public partial class Db
    {
        public static bool RefreshRuns
        {
            get
            {
                return (refresh_t != null && refresh_t.IsAlive);
            }
        }

        public delegate void OnRefreshStateChanged();
        public static event OnRefreshStateChanged RefreshStateChanged = null;

        public static Thread BeginRefresh()
        {
            if (refresh_t != null && refresh_t.IsAlive)
                return refresh_t;

            DateTime refresh_started = DateTime.Now;
            refresh_t = ThreadRoutines.StartTry(() =>
            {
                RefreshStateChanged?.BeginInvoke(null, null);
                Log.Main.Inform("Refreshing db.");

                //InfoWindow iw = InfoWindow.Create("Foreclosures", "Refreshing database... Please wait for completion.", null, "OK", null);

                HttpClientHandler handler = new HttpClientHandler();
                http_client = new HttpClient(handler);

                List<Task> tasks = new List<Task>();
                var t = new Task(() =>
                {
                    refresh_json_file_by_request<Plaintiffs>("https://i.probidder.com/api/fields/index.php?type=foreclosures&field=plaintiff");
                    //refresh_table<Db.Plaintiffs, Db.Plaintiff>("https://i.probidder.com/api/fields/index.php?type=foreclosures&field=plaintiff");
                });
                t.Start();
                tasks.Add(t);
                t = new Task(() =>
                {
                    refresh_json_file_by_request<AttorneyPhones>("https://i.probidder.com/api/fields/index.php?type=foreclosures&field=attorney_phone");
                    //refresh_table<Db.AttorneyPhones, Db.AttorneyPhone>("https://i.probidder.com/api/fields/index.php?type=foreclosures&field=attorney_phone");
                });
                t.Start();
                tasks.Add(t);
                t = new Task(() =>
                {
                    refresh_json_file_by_request<Attorneys>("https://i.probidder.com/api/fields/index.php?type=foreclosures&field=attorney");
                    //refresh_table<Db.Attorneys, Db.Attorney>("https://i.probidder.com/api/fields/index.php?type=foreclosures&field=attorney");
                });
                t.Start();
                tasks.Add(t);
                t = new Task(() =>
                {
                    refresh_json_file_by_request<MortgageTypes>("https://i.probidder.com/api/fields/index.php?type=foreclosures&field=mortgage_type");
                    //refresh_table<Db.MortgageTypes, Db.MortgageType>("https://i.probidder.com/api/fields/index.php?type=foreclosures&field=mortgage_type");
                });
                t.Start();
                tasks.Add(t);
                t = new Task(() =>
               {
                   refresh_json_file_by_request<Cities>("https://i.probidder.com/api/fields/index.php?type=foreclosures&field=city");
                   //refresh_table<Db.Cities, Db.City>("https://i.probidder.com/api/fields/index.php?type=foreclosures&field=city");
               });
                t.Start();
                tasks.Add(t);
                t = new Task(() =>
                {
                    refresh_json_file_by_request<Zips>("https://i.probidder.com/api/fields/index.php?type=foreclosures&field=zip");
                    //refresh_table<Db.Zips, Db.Zip>("https://i.probidder.com/api/fields/index.php?type=foreclosures&field=zip");
                });
                t.Start();
                tasks.Add(t);

                refresh_json_file_by_file<Counties, County>(Log.AppDir + "\\counties.csv");
                refresh_json_file_by_file<OwnerRoles, OwnerRole>(Log.AppDir + "\\owner_roles.csv");
                refresh_json_file_by_file<PropertyCodes, PropertyCode>(Log.AppDir + "\\property_codes.csv");

                Task.WaitAll(tasks.ToArray());
                //Log.Inform("Db has been refreshed.");
                //iw.Dispatcher.Invoke(iw.Close);
                Settings.Database.LastRefreshTime = DateTime.Now;
                Settings.General.Save();
                if (Settings.Database.RefreshPeriodInSecs > 0)
                    Settings.Database.NextRefreshTime = refresh_started.AddSeconds(Settings.Database.RefreshPeriodInSecs);
                InfoWindow.Create(ProgramRoutines.GetAppName(), "Database has been refreshed successfully.", null, "OK", null, System.Windows.Media.Brushes.White, System.Windows.Media.Brushes.Green);
            },
            (Exception e) =>
            {
                Log.Main.Error(e);
                Log.Main.Error("Could not refresh db.");
                if (Settings.Database.RefreshRetryPeriodInSecs > 0)
                    Settings.Database.NextRefreshTime = refresh_started.AddSeconds(Settings.Database.RefreshRetryPeriodInSecs);
                InfoWindow.Create(ProgramRoutines.GetAppName() + ": database could not refresh!", Log.GetExceptionMessage(e), null, "OK", null, System.Windows.Media.Brushes.Beige, System.Windows.Media.Brushes.Red);
            },
            () =>
            {
                Settings.Database.Save();
                RefreshStateChanged?.BeginInvoke(null, null);
            }
            );
            return refresh_t;
        }
        static Thread refresh_t = null;
        static HttpClient http_client;

        static void refresh_json_file_by_request<T>(string url)
        {
            Log.Main.Inform("Refreshing table: " + typeof(T).Name);
            HttpResponseMessage rm = http_client.GetAsync(url).Result;
            if (!rm.IsSuccessStatusCode)
                throw new Exception("Could not refresh table: " + rm.ReasonPhrase);
            if (rm.Content == null)
                throw new Exception("Response content is null.");
            string s = rm.Content.ReadAsStringAsync().Result;
            System.IO.File.WriteAllText(db_dir + "\\" + typeof(T).Name + ".json", s);
        }

        static void refresh_json_file_by_file<T, D>(string file) where T : Json.Table<D> where D : Document
        {
            Log.Main.Inform("Refreshing table: " + typeof(T).Name);
            string[] ls = File.ReadAllLines(file);
            string[] hs = ls[0].Split(',');
            Dictionary<string, int> hs2i = new Dictionary<string, int>();
            for (int i = 0; i < hs.Length; i++)
                hs2i[hs[i]] = i;
            PropertyInfo[] pis = typeof(D).GetProperties(BindingFlags.Public | BindingFlags.Instance| BindingFlags.DeclaredOnly);
            List<D> ds = new List<D>();
            for (int i = 1; i < ls.Length; i++)
            {
                string[] vs = ls[i].Split(',');
                D d = Activator.CreateInstance<D>();
                foreach (PropertyInfo pi in pis)
                    pi.SetValue(d, vs[hs2i[pi.Name]]);
                ds.Add(d);
            }
            string s = SerializationRoutines.Json.Serialize(ds);
            File.WriteAllText(db_dir + "\\" + typeof(T).Name + ".json", s);
        }

        //static void refresh_table<T, D>(string url) where T : Table<D> where D : Document
        //{
        //    Log.Main.Inform("Refreshing table: " + typeof(T).Name);
        //    HttpResponseMessage rm = http_client.GetAsync(url).Result;
        //    if (!rm.IsSuccessStatusCode)
        //        throw new Exception("Could not refresh table: " + rm.ReasonPhrase);
        //    if (rm.Content == null)
        //        throw new Exception("Response content is null.");
        //    string s = rm.Content.ReadAsStringAsync().Result;
        //    T t = Activator.CreateInstance<T>();
        //    foreach (D d in SerializationRoutines.Json.Deserialize<dynamic>(s))
        //        t.Save(d);
        //}
    }
}