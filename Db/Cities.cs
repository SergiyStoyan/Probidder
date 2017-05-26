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

namespace Cliver.Foreclosures
{
    public partial class Db
    {
        public class Cities : Db.Json.Table<City>
        {
            new static public void RefreshFile()
            {
                refresh_json_file_by_request("https://i.probidder.com/api/fields/index.php?type=foreclosures&field=city");
            }

            public List<City> GetBy(string county)
            {
                lock (table)
                {
                    county = GetNormalized(county);
                    int min_count = Settings.Database.GetMinCountFor(GetType());
                    return table.Where(x => x.count > min_count && GetNormalized(x.county) == county).ToList();
                }
            }
        }

        public class City : Document
        {
            public string city { get; set; }
            public string county { get; set; }
            public int count { get; set; }
        }
    }
}