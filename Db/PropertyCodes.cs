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
        public class PropertyCodes : Db.Json.Table<PropertyCode>
        {
        }

        public class PropertyCode : Document
        {
            public string type { get; set; }
            public string description { get; set; }
        }
    }
}