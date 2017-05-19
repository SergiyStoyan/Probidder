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
        public class AttorneyPhones : Db.Json.Table<AttorneyPhone>
        {
            public List<AttorneyPhone> GetBy(string county, string attorney)
            {
                lock (table)
                {
                    county = GetNormalized(county);
                    attorney = GetNormalized(attorney);
                    return table.Where(x => GetNormalized(x.attorney) == attorney && GetNormalized(x.county) == county).ToList();
                }
            }
        }

        public class AttorneyPhone : Document
        {
            public string attorney_phone { get; set; }
            public string attorney { get; set; }
            public string county { get; set; }
            public string count { get; set; }
        }
    }
}