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
                    Plaintiffs.RefreshFile();
                    //refresh_table<Db.Plaintiffs, Db.Plaintiff>("https://i.probidder.com/api/fields/index.php?type=foreclosures&field=plaintiff");
                });
                t.Start();
                tasks.Add(t);
                t = new Task(() =>
                {
                    AttorneyPhones.RefreshFile();
                    //refresh_table<Db.AttorneyPhones, Db.AttorneyPhone>("https://i.probidder.com/api/fields/index.php?type=foreclosures&field=attorney_phone");
                });
                t.Start();
                tasks.Add(t);
                t = new Task(() =>
                {
                    Attorneys.RefreshFile();
                    //refresh_table<Db.Attorneys, Db.Attorney>("https://i.probidder.com/api/fields/index.php?type=foreclosures&field=attorney");
                });
                t.Start();
                tasks.Add(t);
                t = new Task(() =>
                {
                    MortgageTypes.RefreshFile();
                    //refresh_table<Db.MortgageTypes, Db.MortgageType>("https://i.probidder.com/api/fields/index.php?type=foreclosures&field=mortgage_type");
                });
                t.Start();
                tasks.Add(t);
                t = new Task(() =>
                {
                    Cities.RefreshFile();
                   //refresh_table<Db.Cities, Db.City>("https://i.probidder.com/api/fields/index.php?type=foreclosures&field=city");
               });
                t.Start();
                tasks.Add(t);
                t = new Task(() =>
                {
                    Zips.RefreshFile();
                    //refresh_table<Db.Zips, Db.Zip>("https://i.probidder.com/api/fields/index.php?type=foreclosures&field=zip");
                });
                t.Start();
                tasks.Add(t);
                t = new Task(() =>
                {
                    CaseNumbers.RefreshFile();
                });                
                t.Start();
                tasks.Add(t);

                Counties.RefreshFile();
                OwnerRoles.RefreshFile();
                PropertyCodes.RefreshFile();

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