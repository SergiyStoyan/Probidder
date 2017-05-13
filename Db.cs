using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http;
using System.IO;
using System.Data.SQLite;
//using MongoDB.Bson;
//using MongoDB.Driver;
using LiteDB;

namespace Cliver.Foreclosures
{
  public  class Db
    {
        public class Foreclosures
        {
            //static SQLiteConnection dbc;
            //static readonly string db_file = Db.db_dir + "\\db.sqlite";
            static readonly string db_file = Db.db_dir + "\\db.litedb";
            static LiteCollection<Foreclosure> foreclosures;

            static Foreclosures()
            {
                LiteDatabase db = new LiteDatabase(db_file);
                foreclosures = db.GetCollection<Foreclosure>("foreclosures");



                //                SQLiteConnection.CreateFile(db_file);
                //                dbc = new SQLiteConnection("Data Source=" + db_file + "; Version=3;");
                //                dbc.Open();
                //                SQLiteCommand c = new SQLiteCommand(@"CREATE TABLE foreclosures (
                //TYPE_OF_EN VARCHAR(20), 
                //COUNTY VARCHAR(20), 
                //CASE_N VARCHAR(20), 
                //FILING_DATE VARCHAR(20), 
                //AUCTION_DATE VARCHAR(20), 
                //AUCTION_TIME VARCHAR(20), 
                //SALE_LOC;
                //                public string ENTRY_DATE;
                //                public string LENDOR;
                //                public string ORIGINAL_MTG;
                //                public string DOCUMENT_N;
                //                public string ORIGINAL_I;
                //                public string LEGAL_D;
                //                public string ADDRESS;
                //                public string CITY;
                //                public string ZIP;
                //                public string PIN;
                //                public string DATE_OF_CA;
                //                public string LAST_PAY_DATE;
                //                public string BALANCE_DU;
                //                public string PER_DIEM_I;
                //                public string CURRENT_OW;
                //                public string IS_ORG;
                //                public string DECEASED;
                //                public string OWNER_ROLE;
                //                public string OTHER_LIENS;
                //                public string ADDL_DEF;
                //                public string PUB_COMMENTS;
                //                public string INT_COMMENTS;
                //                public string ATTY;
                //                public string ATTORNEY_S;
                //                public string TYPE_OF_MO;
                //                public string PROP_DESC;
                //                public string INTEREST_R;
                //                public string MONTHLY_PAY;
                //                public string TERM_OF_MTG;
                //                public string DEF_ADDRESS;
                //                public string DEF_PHONE;                    


                //                    VARCHAR(20), score INT)
                //", dbc);
            }

            static public Foreclosure Back(Foreclosure current)
            {
                if (current == null)
                    current = GetLast();
                return foreclosures.Find(x => x.Id < current.Id).OrderByDescending(x => x.Id).FirstOrDefault();
            }

            static public Foreclosure Forward(Foreclosure current)
            {
                if (current == null)
                    current = GetFirst();
                return foreclosures.Find(x => x.Id > current.Id).OrderBy(x => x.Id).FirstOrDefault();
            }

            static public Foreclosure GetFirst()
            {
                return foreclosures.FindAll().OrderBy(x => x.Id).FirstOrDefault();
            }

            static public Foreclosure GetLast()
            {
                return foreclosures.FindAll().OrderByDescending(x => x.Id).FirstOrDefault();
            }

            static public Foreclosure GetById(int id)
            {
                return foreclosures.FindById(id);
            }

            static public List<Foreclosure> GetAll()
            {
                return foreclosures.FindAll().OrderBy(x => x.FILING_DATE).ToList();
            }

            static public int Save(Foreclosure foreclosure)
            {
                if (foreclosure.Id == 0)
                {
                    var b = foreclosures.Insert(foreclosure);
                    return b.AsInt32;
                }
                foreclosures.Update(foreclosure);
                return foreclosure.Id;
            }

            static public void Delete(int id)
            {
                foreclosures.Delete(id);
            }

            static public int Count()
            {
                return foreclosures.Count();
            }

            public class Foreclosure
            {
                public int Id { get; set; }
                public string TYPE_OF_EN { get; set; }
                public string COUNTY { get; set; }
                public string CASE_N { get; set; }
                public string FILING_DATE { get; set; }
                public string AUCTION_DATE { get; set; }
                public string AUCTION_TIME { get; set; }
                public string SALE_LOC { get; set; }
                public string ENTRY_DATE { get; set; }
                public string LENDOR { get; set; }
                public string ORIGINAL_MTG { get; set; }
                public string DOCUMENT_N { get; set; }
                public string ORIGINAL_I { get; set; }
                public string LEGAL_D { get; set; }
                public string ADDRESS { get; set; }
                public string CITY { get; set; }
                public string ZIP { get; set; }
                public string PIN { get; set; }
                public string DATE_OF_CA { get; set; }
                public string LAST_PAY_DATE { get; set; }
                public string BALANCE_DU { get; set; }
                public string PER_DIEM_I { get; set; }
                public string CURRENT_OW { get; set; }
                public bool IS_ORG { get; set; }
                public bool DECEASED { get; set; }
                public string OWNER_ROLE { get; set; }
                public string OTHER_LIENS { get; set; }
                public string ADDL_DEF { get; set; }
                public string PUB_COMMENTS { get; set; }
                public string INT_COMMENTS { get; set; }
                public string ATTY { get; set; }
                public string ATTORNEY_S { get; set; }
                public string TYPE_OF_MO { get; set; }
                public string PROP_DESC { get; set; }
                public string INTEREST_R { get; set; }
                public string MONTHLY_PAY { get; set; }
                public string TERM_OF_MTG { get; set; }
                public string DEF_ADDRESS { get; set; }
                public string DEF_PHONE { get; set; }
            }
        }

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

                string s = File.ReadAllText(Log.AppDir + "\\illinois_postal_codes.csv");
                File.WriteAllText(db_dir + "\\illinois_postal_codes.csv", get_normalized(s));

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
                System.IO.File.WriteAllText(db_dir + "\\" + table + ".json", get_normalized(s));
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
            Dictionary<string, string> ks2v = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> k2v in keys2value)
                ks2v[k2v.Key] = get_normalized(k2v.Value);

                List<string> vs = new List<string>();
            string s = System.IO.File.ReadAllText(db_dir + "\\" + table + ".json");
            dynamic json = SerializationRoutines.Json.Deserialize<dynamic>(s);
            foreach (dynamic d in (dynamic)json)
            {
                bool found = true;
                foreach (KeyValuePair<string, string> k2v in ks2v)
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

        static string get_normalized(string s)
        {
            if (s == null)
                return null;
            return System.Text.RegularExpressions.Regex.Replace(s.ToLower(), @" +", " ");
        }

        public static List<string> GetZipCodes(string county, string city)
        {
            county = county.ToLower();
            city = city.ToLower();
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

        //public static List<string> GetMortgageTypes()
        //{
        //    return null;
        //}
    }
}