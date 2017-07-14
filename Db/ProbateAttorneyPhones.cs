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
        public class ProbateAttorneyPhones : Db.Json.Table<Attorney_Phone>
        {
            new static public void RefreshFile()
            {
                refresh_json_file_by_request("https://i.probidder.com/api/fields/index.php?type=probates&field=attorney_phone");
            }

            //public List<Attorney_Phone> GetBy(string county, string attorney)
            //{
            //    lock (table)
            //    {
            //        county = GetStringNormalized(county);
            //        attorney = GetStringNormalized(attorney);
            //        return table.Where(x => x.attorney == attorney && x.county == county).ToList();
            //    }
            //}

            public List<Attorney_Phone> GetBy(string attorney)
            {
                lock (table)
                {
                    attorney = GetStringNormalized(attorney);
                    return table.Where(x => x.attorney == attorney).ToList();
                }
            }
        }

        public class Attorney_Phone : Document
        {
            public string attorney_phone { get; set; }
            public string attorney { get; set; }
            public string county { get; set; }
            public string count { get; set; }
        }
    }
}