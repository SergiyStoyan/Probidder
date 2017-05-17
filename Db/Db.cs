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
        static Dictionary<string, List<string[]>> table_names2csv_table = new Dictionary<string, List<string[]>>();
        static Dictionary<string, dynamic> table_names2json_table = new Dictionary<string, dynamic>();

        static string get_normalized(string s)
        {
            if (s == null)
                return null;
            return System.Text.RegularExpressions.Regex.Replace(s.ToLower(), @" +", " ").Trim();
        }

        public static List<string> GetValuesFromJsonTable(string table, string field, Dictionary<string, string> keys2value)
        {            
            dynamic json;
            if (!table_names2json_table.TryGetValue(table, out json))
            {
                string file = db_dir + "\\" + table + ".json";
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
                    if (SleepRoutines.WaitForObject(() => { return mf; }, 10000) == null)
                        Log.Main.Exit("SleepRoutines.WaitForObject got null");
                    mf.Invoke(() => {
                        try
                        {
                            mf.Close();
                        }
                        catch { }//if closed already
                    });
                    if (!File.Exists(file))
                    {
                        Message.Error("Unfrotunately the required data has not been downloaded. Please try later.");
                        return new List<string>();
                    }
                }

                string s = System.IO.File.ReadAllText(file);
                json = SerializationRoutines.Json.Deserialize<dynamic>(s);
                table_names2json_table[table] = json;
            }
            
            Dictionary<string, string> ks2v = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> k2v in keys2value)
                ks2v[k2v.Key] = get_normalized(k2v.Value);

            List<string> vs = new List<string>();
            foreach (dynamic d in (dynamic)json)
            {
                bool found = true;
                foreach (KeyValuePair<string, string> k2v in ks2v)
                    if (k2v.Value != null && get_normalized((string)d[k2v.Key]) != k2v.Value)
                    {
                        found = false;
                        break;
                    }
                if (found)
                    vs.Add((string)d[field]);
            }
            return vs;
        }

        public static List<string> GetValuesFromCsvTable(string table, string field, Dictionary<string, string> keys2value)
        {
            List<string[]> ls;
            if (!table_names2csv_table.TryGetValue(table, out ls))
            {
                ls = new List<string[]>();
                foreach (string s in File.ReadAllLines(Log.AppDir + "\\" + table + ".csv"))
                    ls.Add(s.Split(','));
                table_names2csv_table[table] = ls;
            }
            
            int field_i = -1;
            for (int i = 0; i < ls[0].Length; i++)
                if (ls[0][i] == field)
                    field_i = i;
            
            Dictionary<int, string> ks2v = new Dictionary<int, string>();
            foreach (KeyValuePair<string, string> k2v in keys2value)
                for (int i = 0; i < ls[0].Length; i++)
                    if (ls[0][i] == k2v.Key)
                        ks2v[i] = k2v.Value;

            List<string> vs = new List<string>();
            for (int i = 1; i < ls.Count; i++)
            {
                bool found = true;
                foreach (KeyValuePair<int, string> k2v in ks2v)
                    if (k2v.Value != null && get_normalized(ls[i][k2v.Key]) != k2v.Value)
                    {
                        found = false;
                        break;
                    }
                if (found)
                    vs.Add(ls[i][field_i]);
            }
            return vs;
        }

        //public static List<string> GetZipCodes(string county, string city)
        //{
        //    string table_name = "illinois_postal_codes";
        //    List<string> vs;
        //    if (!table_names2csv_table.TryGetValue(table_name, out vs))
        //    {
        //        vs = new List<string>();
        //        string[] ss = File.ReadAllLines(Log.AppDir + "\\" + table_name + ".csv");
        //        foreach (string s in ss)
        //        {
        //            string[] fs = get_normalized(s).Split(',');
        //            if (fs[1] == city && fs[3] == county)
        //                vs.Add(fs[0]);
        //        }
        //        table_names2csv_table[table_name] = vs;
        //    }
        //    return vs;
        //}

        //public static List<string> GetPropertyCodes()
        //{
        //    string table_name = "property_codes";
        //    List<string> vs;
        //    if (!table_names2csv_table.TryGetValue(table_name, out vs))
        //    {
        //        vs = new List<string>();
        //        string[] ss = File.ReadAllLines(Log.AppDir + "\\" + table_name + ".csv");
        //        foreach (string s in ss)
        //        {
        //            string[] fs = s.Split(',');
        //            vs.Add(fs[0]);
        //        }
        //        table_names2csv_table[table_name] = vs;
        //    }
        //    return vs;
        //}

        //public static List<string> GetOwnerRoles()
        //{
        //    string table_name = "owner_roles";
        //    List<string> vs;
        //    if (!table_names2csv_table.TryGetValue(table_name, out vs))
        //    {
        //        vs = new List<string>();
        //        string[] ss = File.ReadAllLines(Log.AppDir + "\\" + table_name + ".csv");
        //        foreach (string s in ss)
        //        {
        //            string[] fs = s.Split(',');
        //            vs.Add(fs[0]);
        //        }
        //        table_names2csv_table[table_name] = vs;
        //    }
        //    return vs;
        //}

        //public static List<string> GetCounties()
        //{
        //    string table_name = "counties";
        //    List<string> vs;
        //    if (!table_names2csv_table.TryGetValue(table_name, out vs))
        //    {
        //        vs = new List<string>();
        //        string[] ss = File.ReadAllLines(Log.AppDir + "\\" + table_name + ".csv");
        //        foreach (string s in ss)
        //        {
        //            string[] fs = s.Split(',');
        //            vs.Add(fs[0]);
        //        }
        //        table_names2csv_table[table_name] = vs;
        //    }
        //    return vs;
        //}
    }
}