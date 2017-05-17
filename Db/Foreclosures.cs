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
        public class Foreclosures : Table<Foreclosure>
        {
        }

        public class Foreclosure : Document
        {
            public string TYPE_OF_EN { get; set; }
            public string COUNTY { get; set; }
            public string CASE_N { get; set; }
            public DateTime? FILING_DATE { get; set; }
            public DateTime? AUCTION_DATE { get; set; }
            public DateTime? AUCTION_TIME { get; set; }
            public string SALE_LOC { get; set; }
            public DateTime? ENTRY_DATE { get; set; }
            public string LENDOR { get; set; }
            public string ORIGINAL_MTG { get; set; }
            public string DOCUMENT_N { get; set; }
            public string ORIGINAL_I { get; set; }
            public string LEGAL_D { get; set; }
            public string ADDRESS { get; set; }
            public string CITY { get; set; }
            public string ZIP { get; set; }
            public string PIN { get; set; }
            public DateTime? DATE_OF_CA { get; set; }
            public DateTime? LAST_PAY_DATE { get; set; }
            public string BALANCE_DU { get; set; }
            public string PER_DIEM_I { get; set; }
            public string CURRENT_OW { get; set; }
            public bool IS_ORG { get; set; }
            public bool DECEASED { get; set; }
            public string OWNER_ROLE { get; set; }
            public string OTHER_LIENS { get; set; }
            public string ADDL_DEF { get; set; }
            public string PUB_COMMENTS { get; set; }
            public string INT_COMMENTS { get; set; }
            public string ATTY { get; set; }
            public string ATTORNEY_S { get; set; }
            public string TYPE_OF_MO { get; set; }
            public string PROP_DESC { get; set; }
            public string INTEREST_R { get; set; }
            public string MONTHLY_PAY { get; set; }
            public string TERM_OF_MTG { get; set; }
            public string DEF_ADDRESS { get; set; }
            public string DEF_PHONE { get; set; }
        }
    }
}