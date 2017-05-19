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
        public class Zips : Db.Json.Table<Zip>
        {
            public List<Zip> GetBy(string county, string city)
            {
                lock (table)
                {
                    county = GetNormalized(county);
                    city = GetNormalized(city);
                    return table.Where(x => GetNormalized(x.city) == city && GetNormalized(x.county) == county).ToList();
                }
            }
        }

        public class Zip : Document
        {
            public string zip { get; set; }
            public string city { get; set; }
            public string county { get; set; }
            public int count { get; set; }
        }
    }
}