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
using LiteDB;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Collections;
using System.ComponentModel;

namespace Cliver.Foreclosures
{
    public partial class Db
    {
        public class Foreclosures : Db.LiteDb.Table<Foreclosure>
        {
            public override void Save(Foreclosure document)
            {
                foreach (PropertyInfo pi in typeof(Foreclosure).GetProperties(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance))
                {
                    if (pi.GetCustomAttribute<FieldPreparation.IgnoredField>() != null)
                        continue;
                    if (pi.PropertyType == typeof(string))
                    {
                        string v = (string)pi.GetValue(document);
                        if (string.IsNullOrEmpty(v))
                            continue;
                        pi.SetValue(document, v.Trim());
                    }
                }
                base.Save(document);
            }
        }
        //public class Foreclosures
        //{
        //    ListDb.Table<Foreclosure> t = ListDb.GetTable<Foreclosure>();

        //    public void Save(Foreclosure document)
        //    {
        //        if (document.Id == 0)
        //            document.Id = DateTime.Now.Ticks;

        //        foreach (PropertyInfo pi in typeof(Foreclosure).GetProperties(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance))
        //        {
        //            if (pi.CustomAttributes.Count() > 0)//engine fields
        //                continue;
        //            if (pi.PropertyType == typeof(string))
        //            {
        //                string v = (string)pi.GetValue(document);
        //                if (string.IsNullOrEmpty(v))
        //                    continue;
        //                pi.SetValue(document, v.Trim());
        //            }
        //        }
        //    }

        //    public void Delete(long foreclosure_id)
        //    {
        //      bool r =  t.Remove(t.Where(x => x.Id == foreclosure_id).First());
        //    }

        //    public List<Foreclosure> GetAll()
        //    {
        //        return t;
        //    }

        //    public Foreclosure GetById(int foreclosure_id)
        //    {
        //        return t.Where(x => x.Id == foreclosure_id).FirstOrDefault();
        //    }

        //    public void Dispose()
        //    {
        //        t.Dispose();
        //    }

        //    public Foreclosure GetNext(Foreclosure document)
        //    {
        //        return t.GetNext(document);
        //    }

        //    public Foreclosure GetPrevious(Foreclosure document)
        //    {
        //        return t.GetPrevious(document);
        //    }

        //    public int Count()
        //    {
        //        return t.Count();
        //    }

        //    public IEnumerable<Foreclosure> Get(System.Linq.Expressions.Expression<Func<Foreclosure, bool>> query)
        //    {
        //        return t.AsQueryable().Where(query);
        //    }

        //    public delegate void SavedHandler(Document document, bool inserted);
        //    public event SavedHandler Saved = null;

        //    public delegate void DeletedHandler(int document_id, bool sucess);
        //    public event DeletedHandler Deleted = null;
        //}

        public class Foreclosure : Document, System.ComponentModel.INotifyPropertyChanged, System.ComponentModel.IDataErrorInfo
        //public class Foreclosure : ListDb.Document, /*System.ComponentModel.INotifyPropertyChanged,*/ System.ComponentModel.IDataErrorInfo
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
            public DateTime? ORIGINAL_MTG { get; set; }
            public string DOCUMENT_N { get; set; }
            public uint? ORIGINAL_I { get; set; }
            public string LEGAL_D { get; set; }
            public string ADDRESS { get; set; }
            public string CITY { get; set; }
            public string ZIP { get; set; }
            public string PIN { get; set; }
            public DateTime? DATE_OF_CA { get; set; }
            public DateTime? LAST_PAY_DATE { get; set; }
            public uint? BALANCE_DU { get; set; }
            public decimal? PER_DIEM_I { get; set; }
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
            public decimal? INTEREST_R { get; set; }
            public decimal? MONTHLY_PAY { get; set; }
            public uint? TERM_OF_MTG { get; set; }
            public string DEF_ADDRESS { get; set; }
            public string DEF_PHONE { get; set; }

            #region Validation

            [FieldPreparation.IgnoredField]
            [BsonIgnore]
            public void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
            public event PropertyChangedEventHandler PropertyChanged;

            [FieldPreparation.IgnoredField]
            [BsonIgnore]
            public string Error
            {
                get { return "...."; }
                set { }
            }

            [FieldPreparation.IgnoredField]
            [BsonIgnore]
            Dictionary<string, string> columnNames2error = new Dictionary<string, string>();

            [FieldPreparation.IgnoredField]
            [BsonIgnore]
            public string this[string columnName]
            {
                get
                {
                    if (InitialControlSetting)
                        return null;
                    string e = validate(columnName);
                    columnNames2error[columnName] = e;
                    PropertyChanged2?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(columnName));
                    return e;
                }
            }
            public event PropertyChangedEventHandler PropertyChanged2;

            //[FieldPreparation.IgnoredField]
            //[BsonIgnore]
            //public bool HasError
            //{
            //    get
            //    {
            //        return columnNames2error.Where(x => x.Value != null).Select(x => x.Key).FirstOrDefault() != null;
            //    }
            //}

            [FieldPreparation.IgnoredField]
            [BsonIgnore]
            public bool InitialControlSetting = true;

            [FieldPreparation.IgnoredField]
            [BsonIgnore]
            public bool Edited
            {
                get
                {
                    return columnNames2error.Select(x => x.Key).FirstOrDefault() != null;
                }
            }

            [FieldPreparation.IgnoredField]
            [BsonIgnore]
            private string validate(string propertyName)
            {
                switch (propertyName)
                {
                    case "TYPE_OF_EN":
                        if (string.IsNullOrEmpty( TYPE_OF_EN))
                            return "Error";
                        return null;
                    case "COUNTY":
                        if (string.IsNullOrEmpty(COUNTY))
                            return "Error";
                        return null;
                    case "CASE_N":
                        if (CASE_N != null)
                        {
                            if (Regex.IsMatch(CASE_N, @"[^\w\-_]"))
                                return "Error";
                            if (CASE_N.Length > 16)
                                return "Error";
                        }
                        return null;
                    case "FILING_DATE":
                        if (FILING_DATE == null)
                            return "Error";
                        return null;
                    case "AUCTION_DATE":
                        return null;
                    case "AUCTION_TIME":
                        return null;
                    case "ENTRY_DATE":
                        //if (ENTRY_DATE == null)
                        //    return "Error";
                        return null;
                    case "LENDOR":
                        if (string.IsNullOrEmpty(LENDOR))
                            return "Error";
                        return null;
                    case "ORIGINAL_MTG":
                        if (ORIGINAL_MTG == null)
                            return "Error";
                        return null;
                    case "DOCUMENT_N":
                        return null;
                    case "ORIGINAL_I":
                        return null;
                    case "LEGAL_D":
                        //if (LEGAL_D == null)
                        //    return null;
                        //if (LEGAL_D.Length < 1)
                        //    return "Error";
                        return null;
                    case "ADDRESS":
                        if (string.IsNullOrEmpty(ADDRESS))
                            return "Error";
                        return null;
                    case "CITY":
                        if (string.IsNullOrEmpty(CITY))
                            return "Error";
                        return null;
                    case "ZIP":
                        if (string.IsNullOrEmpty(ZIP))
                            return null;
                        if (Regex.IsMatch(ZIP, @"[^\d]") || ZIP.Length > 5 || ZIP.Length < 4)
                            return "Error";
                        return null;
                    case "PIN":
                        if (string.IsNullOrEmpty(PIN))
                            return null;
                        if (!Regex.IsMatch(PIN, "[^_]{2}-[^_]{2}-[^_]{3}-[^_]{3}-[^_]{4}") && !Regex.IsMatch(PIN, "[^_]{2}-[^_]{2}-[^_]{3}-[^_]{3}-[_]{4}"))
                            return "Error";
                        return null;
                    case "DATE_OF_CA":
                        if (DATE_OF_CA == null)
                            return "Error";
                        return null;
                    case "LAST_PAY_DATE":
                        if (LAST_PAY_DATE == null)
                            return "Error";
                        return null;
                    case "BALANCE_DU":
                        return null;
                    case "PER_DIEM_I":
                        return null;
                    case "CURRENT_OW":
                        if (string.IsNullOrEmpty(CURRENT_OW))
                            return "Error";
                        return null;
                    case "IS_ORG":
                        return null;
                    case "DECEASED":
                        return null;
                    case "OWNER_ROLE":
                        if (string.IsNullOrEmpty(OWNER_ROLE))
                            return "Error";
                        return null;
                    case "OTHER_LIENS":
                        return null;
                    case "ADDL_DEF":
                        return null;
                    case "PUB_COMMENTS":
                        return null;
                    case "INT_COMMENTS":
                        return null;
                    case "ATTY":
                        return null;
                    case "ATTORNEY_S":
                        if (ATTORNEY_S == null)
                            return null;
                        if (Regex.IsMatch(ATTORNEY_S, @"_") && Regex.IsMatch(ATTORNEY_S, @"\d"))
                            return "Error";
                        return null;
                    case "TYPE_OF_MO":
                        return null;
                    case "PROP_DESC":
                        if (string.IsNullOrEmpty(PROP_DESC))
                            return "Error";
                        return null;
                    case "INTEREST_R":
                        return null;
                    case "MONTHLY_PAY":
                        return null;
                    case "TERM_OF_MTG":
                        return null;
                    case "DEF_ADDRESS":
                        return null;
                    case "DEF_PHONE":
                        if (DEF_PHONE == null)
                            return null;
                        if (Regex.IsMatch(DEF_PHONE, @"_") && Regex.IsMatch(DEF_PHONE, @"\d"))
                            return "Error";
                        return null;
                    default:
                        throw new Exception("Field " + propertyName + " is absent in validation.");
                }
            }
            #endregion
        }
    }
}