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
        public class Counties : Db.Json.Table<County>
        {
            static public void RefreshFile()
            {
                refresh_json_file_by_file<County>(Log.AppDir + "\\counties.csv");
            }
        }

        public class County : Document
        {
            public string county { get; set; }
        }
    }
}