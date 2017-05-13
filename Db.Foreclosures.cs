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
        public class Foreclosures
        {
            //static SQLiteConnection dbc;
            //static readonly string db_file = Db.db_dir + "\\db.sqlite";
            static LiteCollection<Foreclosure> foreclosures;

            static Foreclosures()
            {
                LiteDatabase db = new LiteDatabase(Db.db_file);
                foreclosures = db.GetCollection<Foreclosure>("foreclosures");



                //                SQLiteConnection.CreateFile(db_file);
                //                dbc = new SQLiteConnection("Data Source=" + db_file + "; Version=3;");
                //                dbc.Open();
                //                SQLiteCommand c = new SQLiteCommand(@"CREATE TABLE foreclosures (
                //TYPE_OF_EN VARCHAR(20), 
                //COUNTY VARCHAR(20), 
                //CASE_N VARCHAR(20), 
                //FILING_DATE VARCHAR(20), 
                //AUCTION_DATE VARCHAR(20), 
                //AUCTION_TIME VARCHAR(20), 
                //SALE_LOC;
                //                public string ENTRY_DATE;
                //                public string LENDOR;
                //                public string ORIGINAL_MTG;
                //                public string DOCUMENT_N;
                //                public string ORIGINAL_I;
                //                public string LEGAL_D;
                //                public string ADDRESS;
                //                public string CITY;
                //                public string ZIP;
                //                public string PIN;
                //                public string DATE_OF_CA;
                //                public string LAST_PAY_DATE;
                //                public string BALANCE_DU;
                //                public string PER_DIEM_I;
                //                public string CURRENT_OW;
                //                public string IS_ORG;
                //                public string DECEASED;
                //                public string OWNER_ROLE;
                //                public string OTHER_LIENS;
                //                public string ADDL_DEF;
                //                public string PUB_COMMENTS;
                //                public string INT_COMMENTS;
                //                public string ATTY;
                //                public string ATTORNEY_S;
                //                public string TYPE_OF_MO;
                //                public string PROP_DESC;
                //                public string INTEREST_R;
                //                public string MONTHLY_PAY;
                //                public string TERM_OF_MTG;
                //                public string DEF_ADDRESS;
                //                public string DEF_PHONE;                    


                //                    VARCHAR(20), score INT)
                //", dbc);
            }

            static public Foreclosure Back(Foreclosure current)
            {
                if (current == null)
                    current = GetLast();
                return foreclosures.Find(x => x.Id < current.Id).OrderByDescending(x => x.Id).FirstOrDefault();
            }

            static public Foreclosure Forward(Foreclosure current)
            {
                if (current == null)
                    current = GetFirst();
                return foreclosures.Find(x => x.Id > current.Id).OrderBy(x => x.Id).FirstOrDefault();
            }

            static public Foreclosure GetFirst()
            {
                return foreclosures.FindAll().OrderBy(x => x.Id).FirstOrDefault();
            }

            static public Foreclosure GetLast()
            {
                return foreclosures.FindAll().OrderByDescending(x => x.Id).FirstOrDefault();
            }

            static public Foreclosure GetById(int id)
            {
                return foreclosures.FindById(id);
            }

            static public List<Foreclosure> GetAll()
            {
                return foreclosures.FindAll().OrderBy(x => x.Id).ToList();
            }

            static public int Save(Foreclosure foreclosure)
            {
                if (foreclosure.Id == 0)
                {
                    var b = foreclosures.Insert(foreclosure);
                    return b.AsInt32;
                }
                foreclosures.Update(foreclosure);
                return foreclosure.Id;
            }

            static public void Delete(int id)
            {
                foreclosures.Delete(id);
            }

            static public int Count()
            {
                return foreclosures.Count();
            }

            public class Foreclosure
            {
                public int Id { get; set; }
                public string TYPE_OF_EN { get; set; }
                public string COUNTY { get; set; }
                public string CASE_N { get; set; }
                public string FILING_DATE { get; set; }
                public string AUCTION_DATE { get; set; }
                public string AUCTION_TIME { get; set; }
                public string SALE_LOC { get; set; }
                public string ENTRY_DATE { get; set; }
                public string LENDOR { get; set; }
                public string ORIGINAL_MTG { get; set; }
                public string DOCUMENT_N { get; set; }
                public string ORIGINAL_I { get; set; }
                public string LEGAL_D { get; set; }
                public string ADDRESS { get; set; }
                public string CITY { get; set; }
                public string ZIP { get; set; }
                public string PIN { get; set; }
                public string DATE_OF_CA { get; set; }
                public string LAST_PAY_DATE { get; set; }
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
}