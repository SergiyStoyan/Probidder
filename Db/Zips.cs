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
        public class Zips : Db.Json.Table<Zip>
        {
            new static public void RefreshFile()
            {
                refresh_json_file_by_request("https://i.probidder.com/api/fields/index.php?type=foreclosures&field=zip");
            }

            public List<Zip> GetBy(string county, string city)
            {
                lock (table)
                {
                    county = GetStringNormalized(county);
                    city = GetStringNormalized(city);
                    return table.Where(x => x.city == city && x.county == county).OrderBy(x => x.zip).ToList();
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