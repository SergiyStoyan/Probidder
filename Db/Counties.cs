/********************************************************************************************
        Author: Sergey Stoyan
        sergey.stoyan@gmail.com
        http://www.cliversoft.com
********************************************************************************************/
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

namespace Cliver.Probidder
{
    public partial class Db
    {
        public class Counties : Db.Json.Table<County>
        {
            new static public void RefreshFile()
            {
                refresh_json_file_by_file(Log.AppDir + "\\counties.csv");
            }
        }

        public class County : Document
        {
            public string county { get; set; }
        }
    }
}