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
                    refresh_table("https://i.probidder.com/api/fields/index.php?type=foreclosures&field=mortgage_type", "mortgage_types");
                });
                t.Start();
                tasks.Add(t);
                t = new Task(() =>
                {
                    refresh_table("https://i.probidder.com/api/fields/index.php?type=foreclosures&field=attorney_phone", "attorney_phones");
                });
                t.Start();
                tasks.Add(t);
                t = new Task(() =>
                {
                    refresh_table("https://i.probidder.com/api/fields/index.php?type=foreclosures&field=attorney", "attorneys");
                });
                t.Start();
                tasks.Add(t);
                t = new Task(() =>
               {
                   refresh_table("https://i.probidder.com/api/fields/index.php?type=foreclosures&field=city", "cities");
               });
                t.Start();
                tasks.Add(t);
                t = new Task(() =>
               {
                   refresh_table("https://i.probidder.com/api/fields/index.php?type=foreclosures&field=plaintiff", "plaintiffs");
               });
                t.Start();
                tasks.Add(t);

                //if (!File.Exists(db_dir + "\\illinois_postal_codes.csv"))
                //{
                //    string s = File.ReadAllText(Log.AppDir + "\\illinois_postal_codes.csv");
                //    File.WriteAllText(db_dir + "\\illinois_postal_codes.csv", get_normalized(s));
                //}

                //if (!File.Exists(db_dir + "\\property_codes.csv"))
                //    File.Copy(Log.AppDir + "\\property_codes.csv", db_dir + "\\property_codes.csv");

                //if (!File.Exists(db_dir + "\\owner_roles.csv"))
                //    File.Copy(Log.AppDir + "\\owner_roles.csv", db_dir + "\\owner_roles.csv");

                //if (!File.Exists(db_dir + "\\counties.csv"))
                //    File.Copy(Log.AppDir + "\\counties.csv", db_dir + "\\counties.csv");

                Task.WaitAll(tasks.ToArray());
                //Log.Inform("Db has been refreshed.");
                //iw.Dispatcher.Invoke(iw.Close);
                Settings.General.LastDbRefreshTime = DateTime.Now;
                Settings.General.Save();
                if (Settings.General.DbRefreshPeriodInSecs > 0)
                    Settings.General.NextDbRefreshTime = refresh_started.AddSeconds(Settings.General.DbRefreshPeriodInSecs);
                InfoWindow.Create(ProgramRoutines.GetAppName(), "Database has been refreshed successfully.", null, "OK", null, System.Windows.Media.Brushes.White, System.Windows.Media.Brushes.Green);
            },
            (Exception e) =>
            {
                Log.Main.Error(e);
                Log.Main.Error("Could not refresh db.");
                if (Settings.General.DbRefreshRetryPeriodInSecs > 0)
                    Settings.General.NextDbRefreshTime = refresh_started.AddSeconds(Settings.General.DbRefreshRetryPeriodInSecs);
                InfoWindow.Create(ProgramRoutines.GetAppName() + ": database could not refresh!", Log.GetExceptionMessage(e), null, "OK", null, System.Windows.Media.Brushes.Beige, System.Windows.Media.Brushes.Red);
            },
            () =>
            {
                Settings.General.Save();
                RefreshStateChanged?.BeginInvoke(null, null);
            }
            );
            return refresh_t;
        }
        static Thread refresh_t = null;
        static HttpClient http_client;

        static void refresh_table(string url, string table)
        {
            Log.Main.Inform("Refreshing table: " + table);
            HttpResponseMessage rm = http_client.GetAsync(url).Result;
            if (!rm.IsSuccessStatusCode)
                throw new Exception("Could not refresh table: " + rm.ReasonPhrase);
            if (rm.Content == null)
                throw new Exception("Response content is null.");
            string s = rm.Content.ReadAsStringAsync().Result;
            System.IO.File.WriteAllText(db_dir + "\\" + table + ".json", s);
        }
    }
}