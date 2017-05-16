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
        public void Put2Memory()
        {
        }
        static Dictionary<string, string> table_names2table_content = new Dictionary<string, string>();

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

        public static List<string> GetOwnerRoles()
        {
            List<string> vs = new List<string>();
            string[] ss = File.ReadAllLines(db_dir + "\\owner_roles.csv");
            foreach (string s in ss)
            {
                string[] fs = s.Split(',');
                vs.Add(fs[0]);
            }
            return vs;
        }

        public static List<string> GetCounties()
        {
            List<string> vs = new List<string>();
            string[] ss = File.ReadAllLines(db_dir + "\\counties.csv");
            foreach (string s in ss)
            {
                string[] fs = s.Split(',');
                vs.Add(fs[0]);
            }
            return vs;
        }
    }
}