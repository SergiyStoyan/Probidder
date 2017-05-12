using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http;

namespace Cliver.Foreclosures
{
    class Db
    {
        //static Db()
        //{
        //    BeginRefresh();
        //}
        
        static readonly string db_dir = Log.GetAppCommonDataDir();

        //static void initialize()
        //{
        //}

        public static void BeginRefresh()
        {
            refresh_t = ThreadRoutines.StartTry(() =>
            {
                Log.Inform("Refreshing db.");

                InfoWindow iw = InfoWindow.Create("Foreclosures", "Refreshing database... Please wait for completion.", null, "OK", null);

                HttpClientHandler handler = new HttpClientHandler();
                http_client = new HttpClient(handler);
                
                List<Task> tasks = new List<Task>();
                var t = new Task(() => {
                    refresh_table("https://i.probidder.com/api/fields/index.php?type=foreclosures&field=mortgage_type", "mortgage_types");
                });
                t.Start();
                tasks.Add(t);
                t = new Task(() => {
                    refresh_table("https://i.probidder.com/api/fields/index.php?type=foreclosures&field=attorney_phone", "attorney_phones");
                });
                t.Start();
                tasks.Add(t);
                t = new Task(() => {
                    refresh_table("https://i.probidder.com/api/fields/index.php?type=foreclosures&field=attorney", "attornies");
                });
                t.Start();
                tasks.Add(t);
                t = new Task(() => {
                    refresh_table("https://i.probidder.com/api/fields/index.php?type=foreclosures&field=city", "cities");
                });
                t.Start();
                tasks.Add(t);
                t = new Task(() => {
                    refresh_table("https://i.probidder.com/api/fields/index.php?type=foreclosures&field=plaintiff", "plaintiffs");
                });
                t.Start();
                tasks.Add(t);

                Task.WaitAll(tasks.ToArray());
                //Log.Inform("Db has been refreshed.");
                iw.Dispatcher.Invoke(iw.Close);
                InfoWindow.Create("Foreclosures", "Database has been refreshed successfully.", null, "OK", null);
            },
            (Exception e) =>
            {
                Log.Error(e);
                Log.Error("Could not refresh db.");
            },
            () =>
            {
            }
            );
        }
        static Thread refresh_t = null;
        static HttpClient http_client;

        static async void refresh_table(string url, string table)
        {
            try
            {
                Log.Main.Inform("Refreshing table: " + table);
                HttpResponseMessage rm = await http_client.GetAsync(url);
                if (!rm.IsSuccessStatusCode)
                    throw new Exception("Could not refresh table: " + rm.ReasonPhrase);
                if (rm.Content == null)
                    throw new Exception("Response content is null.");
                string s = await rm.Content.ReadAsStringAsync();
                System.IO.File.WriteAllText(db_dir + "\\" + table + ".json", s);
            }
            catch(Exception e)
            {
                Log.Error(e);
            }
        }

        //public class MortgageType
        //{
        //    public string mortgage_type;
        //    public string count;
        //}

        //public class Plaintiff
        //{
        //    public string plaintiff;
        //    public string county;
        //    public string count;
        //}

        //public class Attorney
        //{
        //    public string attorney;
        //    public string county;
        //    public string count;
        //}

        //public class AttorneyPhone
        //{
        //    public string attorney_phone;
        //    public string attorney;
        //    public string county;
        //    public string count;
        //}

        //public class City
        //{
        //    public string city;
        //    public string county;
        //    public string count;
        //}

        //public static List<string> GetCities(string county, string zip_code = null)
        //{
        //    List<string> vs = new List<string>();
        //    string s = System.IO.File.ReadAllText(db_dir + "\\cities.json");
        //    dynamic json = SerializationRoutines.Json.Deserialize<dynamic>(s);
        //    foreach (dynamic d in (dynamic)json)
        //        if (d["county"] == county && (zip_code == null || d["zip_code"] == zip_code))
        //            vs.Add(d["city"]);
        //    return vs;
        //}

        //public static List<string> GetPlaintiffs(string county)
        //{
        //    List<string> vs = new List<string>();
        //    string s = System.IO.File.ReadAllText(db_dir + "\\plaintiffs.json");
        //    dynamic json = SerializationRoutines.Json.Deserialize<dynamic>(s);
        //    foreach (dynamic d in (dynamic)json)
        //        if (d["county"] == county)
        //            vs.Add(d["plaintiff"]);
        //    return vs;
        //}

        public static List<string> GetValuesFromTable(string table, string field, Dictionary<string, string> keys2value)
        {
            List<string> vs = new List<string>();
            string s = System.IO.File.ReadAllText(db_dir + "\\" + table + ".json");
            dynamic json = SerializationRoutines.Json.Deserialize<dynamic>(s);
            foreach (dynamic d in (dynamic)json)
            {
                bool found = true;
                foreach (KeyValuePair<string, string> k2v in keys2value)
                    if (k2v.Value != null && d[k2v.Key] != k2v.Value)
                    {
                        found = false;
                        break;
                    }
                if (found)
                    vs.Add((string)d[field]);
            }
            return vs;
        }

        //public static List<string> GetZipCodes(string county, string city = null)
        //{
        //    return null;
        //}

        //public static List<string> GetMortgageTypes()
        //{
        //    return null;
        //}
    }
}