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
        //public class AttorneyPhones : Db.Table<AttorneyPhone>
        //{
        //    public List<AttorneyPhone> GetBy(string county, string attorney)
        //    {
        //        county = get_normalized(county);
        //        attorney = get_normalized(attorney);
        //        return table.Find(x => get_normalized(x.county) == county && get_normalized(x.attorney) == attorney).ToList();
        //    }
        //}

        //public class AttorneyPhone : Document
        //{
        //    public string attorney_phone { get; set; }
        //    public string attorney { get; set; }
        //    public string county { get; set; }
        //    public string count { get; set; }
        //}
    }
}