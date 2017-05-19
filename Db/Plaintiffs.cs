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
        public class Plaintiffs : Db.Json.Table<Plaintiff>
        {
            public List<Plaintiff> GetBy(string county)
            {
                lock (table)
                {
                    county = GetNormalized(county);
                    return table.Where(x => GetNormalized(x.county) == county).ToList();
                }
            }
        }

        public class Plaintiff : Document
        {
            public string plaintiff { get; set; }
            public string county { get; set; }
            public int count { get; set; }
        }
    }
}