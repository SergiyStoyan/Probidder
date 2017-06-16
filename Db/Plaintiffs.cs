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
        public class Plaintiffs : Db.Json.Table<Plaintiff>
        {
            static public void RefreshFile()
            {
                refresh_json_file_by_request("https://i.probidder.com/api/fields/index.php?type=foreclosures&field=plaintiff");
            }

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