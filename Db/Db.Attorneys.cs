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
        public class Attorneys : Db.Table<Attorney>
        {
            public List<Attorney> GetBy(string county)
            {
                county = get_normalized(county);
                return table.Find(x => get_normalized(x.county) == county).ToList();
            }
        }

        public class Attorney : Document
        {
            public string attorney { get; set; }
            public string county { get; set; }
            public string count { get; set; }
        }
    }
}