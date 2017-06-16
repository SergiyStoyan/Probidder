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
        public class PropertyCodes : Db.Json.Table<PropertyCode>
        {
            new static public void RefreshFile()
            {
                refresh_json_file_by_file(Log.AppDir + "\\property_codes.csv");
            }
        }

        public class PropertyCode : Document
        {
            public string type { get; set; }
            public string description { get; set; }
        }
    }
}