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
        public class CaseNumbers : Db.Json.Table<CountyCaseNumbers>
        {
            new static public void RefreshFile()
            {
                Type t = MethodBase.GetCurrentMethod().DeclaringType;
                Log.Main.Inform("Refreshing table: " + t.Name);
                //string[] ls = File.ReadAllLines(Log.AppDir + "\\counties.csv");
                List<CountyCaseNumbers> ccns = new List<CountyCaseNumbers>();
                //for (int i = 1; i < ls.Length; i++)
                //    ccns.Add(get_CountyCaseNumbers(ls[i]));
                if(string.IsNullOrEmpty(Settings.Location.County))
                {
                    Message.Exclaim("Your location is not specified so Case Numbers cannot be refreshed now.");
                    return;
                }
                ccns.Add(get_CountyCaseNumbers(Settings.Location.County));
                string s = SerializationRoutines.Json.Serialize(ccns);
                System.IO.File.WriteAllText(db_dir + "\\" + t.Name + ".json", s);
            }

            static CountyCaseNumbers get_CountyCaseNumbers(string county)
            {
                county = GetNormalized(county);
                HttpResponseMessage rm = http_client.GetAsync("https://i.probidder.com/api/record-gaps/index.php?foreclosures&county=" + county).Result;
                if (!rm.IsSuccessStatusCode)
                    throw new Exception("Could not refresh table: " + rm.ReasonPhrase);
                if (rm.Content == null)
                    throw new Exception("Response content is null.");
                string s = rm.Content.ReadAsStringAsync().Result;
                List<string> case_ns = new List<string>();
                dynamic ccns = SerializationRoutines.Json.Deserialize<dynamic>(s);
                foreach (dynamic ccn in ccns)
                    case_ns.Add(((string)ccn.Name).Trim());
                return new CountyCaseNumbers { county = county, case_ns = case_ns };
            }

            public CountyCaseNumbers GetBy(string county)
            {
                lock (table)
                {
                    county = GetNormalized(county);
                    CountyCaseNumbers ccns = table.Where(x => GetNormalized(x.county) == county).FirstOrDefault();
                    if (ccns == null)
                        return new CountyCaseNumbers { case_ns = new List<string>()};
                    Db.Foreclosures fs = new Foreclosures();
                    //List<string> used_cns = fs.Get(x => GetNormalized(x.COUNTY) == county).ToList(); !!!does not work!!!
                    HashSet<string> used_cns = new HashSet<string>(fs.GetAll().Where(x => GetNormalized(x.COUNTY) == county).Select(x => x.CASE_N));
                    ccns.case_ns = ccns.case_ns.Where(x => !used_cns.Contains(x)).ToList();
                    return ccns;
                }
            }
        }

        public class CountyCaseNumbers : Document
        {
            public string county { get; set; }
            public List<string> case_ns { get; set; }
        }
    }
}