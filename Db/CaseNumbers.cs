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
            static public void RefreshFile()
            {
                //Type t = MethodBase.GetCurrentMethod().DeclaringType;
                //Log.Main.Inform("Refreshing table: " + t.Name);
                //HttpResponseMessage rm = http_client.GetAsync(url).Result;
                //if (!rm.IsSuccessStatusCode)
                //    throw new Exception("Could not refresh table: " + rm.ReasonPhrase);
                //if (rm.Content == null)
                //    throw new Exception("Response content is null.");
                //string s = rm.Content.ReadAsStringAsync().Result;

                //System.IO.File.WriteAllText(db_dir + "\\" + t.Name + ".json", s);
            }

            public string[] GetBy(string county)
            {
                lock (table)
                {
                    county = GetNormalized(county);
                    CountyCaseNumbers ccns = table.Where(x => GetNormalized(x.county) == county).FirstOrDefault();
                    if (ccns == null)
                        return new string[0];
                    return ccns.case_ns;
                }
            }
        }

        public class CountyCaseNumbers : Document
        {
            public string county { get; set; }
            public string[] case_ns { get; set; }
        }
    }
}