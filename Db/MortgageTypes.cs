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
        public class MortgageTypes : Db.Json.Table<MortgageType>
        {
            public List<MortgageType> Get()
            {
                lock (table)
                {
                    int min_count = Settings.Database.GetMinCountFor(GetType());
                    return table.Where(x => x.count > min_count).ToList();
                }
            }
        }

        public class MortgageType : Document
        {
            public string mortgage_type { get; set; }
            public int count { get; set; }
        }
    }
}