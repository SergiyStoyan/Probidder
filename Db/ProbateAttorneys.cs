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
        public class ProbateAttorneys : Db.Json.Table<ProbateAttorney>
        {
            static public void RefreshFile()
            {
                refresh_json_file_by_request("https://i.probidder.com/api/fields/index.php?type=probates&field=attorney");
            }

            public List<ProbateAttorney> GetBy(string county)
            {
                lock (table)
                {
                    county = GetStringNormalized(county);
                    int min_count = Settings.Database.GetMinCountFor(GetType());
                    return table.Where(x => x.count > min_count && x.county == county).ToList();
                }
            }
        }

        public class ProbateAttorney : Document
        {
            public string attorney { get; set; }
            public string county { get; set; }
            public int count { get; set; }
        }
    }
}