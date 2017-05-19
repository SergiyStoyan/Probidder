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
        public class OwnerRoles : Db.Json.Table<OwnerRole>
        {
            static public void RefreshFile()
            {
                refresh_json_file_by_file<OwnerRole>(Log.AppDir + "\\owner_roles.csv");
            }
        }

        public class OwnerRole : Document
        {
            public string role { get; set; }
        }
    }
}