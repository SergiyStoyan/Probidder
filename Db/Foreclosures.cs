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
                    if (pi.CustomAttributes.Count() > 0)//engine fields
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

        public class Foreclosure : Document, /*System.ComponentModel.INotifyPropertyChanged,*/ System.ComponentModel.IDataErrorInfo
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
            public int? ORIGINAL_I { get; set; }
            public string LEGAL_D { get; set; }
            public string ADDRESS { get; set; }
            public string CITY { get; set; }
            public string ZIP { get; set; }
            public string PIN { get; set; }
            public DateTime? DATE_OF_CA { get; set; }
            public DateTime? LAST_PAY_DATE { get; set; }
            public int BALANCE_DU { get; set; }
            public decimal PER_DIEM_I { get; set; }
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
            public int TERM_OF_MTG { get; set; }
            public string DEF_ADDRESS { get; set; }
            public string DEF_PHONE { get; set; }





            #region Validation

            //public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

            //protected void OnPropertyChanged(string propertyName)
            //{
            //    PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            //}

            [BsonIgnore]
            public string Error
            {
                get { return "...."; }
                set { }
            }

            [BsonIgnore]
            public string this[string columnName]
            {
                get
                {
                    return validate(columnName);
                }
            }

            [BsonIgnore]
            private string validate(string propertyName)
            {
                switch (propertyName)
                {
                    case "TYPE_OF_EN":
                        if (TYPE_OF_EN == null)
                            return null;
                        if (TYPE_OF_EN.Length < 1)
                            return "Error";
                        return null;
                    case "COUNTY":
                        if (COUNTY == null)
                            return null;
                        if (COUNTY.Length < 1)
                            return "Error";
                        return null;
                    case "CASE_N":
                        if (CASE_N == null)
                            return null;
                        if (CASE_N.Length < 1)
                            return "Error";
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
                        if (ENTRY_DATE == null)
                            return "Error";
                        return null;
                    case "LENDOR":
                        if (LENDOR == null)
                            return null;
                        if (LENDOR.Length < 1)
                            return "Error";
                        return null;
                    case "ORIGINAL_MTG":
                        return null;
                    case "DOCUMENT_N":
                        return null;
                    case "ORIGINAL_I":
                        return null;
                    case "LEGAL_D":
                        if (LEGAL_D == null)
                            return null;
                        if (LEGAL_D.Length < 1)
                            return "Error";
                        return null;
                    case "ADDRESS":
                        if (ADDRESS == null)
                            return null;
                        if (ADDRESS.Length < 1)
                            return "Error";
                        return null;
                    case "CITY":
                        if (CITY == null)
                            return null;
                        if (CITY.Length < 1)
                            return "Error";
                        return null;
                    case "ZIP":
                        if (ZIP == null)
                            return null;
                        if (Regex.IsMatch(ZIP, @"[^\d]") || ZIP.Length > 5 || ZIP.Length < 4)
                            return "Error";
                        return null;
                    case "PIN":
                        if (PIN == null)
                            return null;
                        if (Regex.IsMatch(PIN, "_"))
                            return "Error";
                        return null;
                    case "DATE_OF_CA":
                        return null;
                    case "LAST_PAY_DATE":
                        return null;
                    case "BALANCE_DU":
                        return null;
                    case "PER_DIEM_I":
                        return null;
                    case "CURRENT_OW":
                        if (CURRENT_OW == null)
                            return null;
                        if (CURRENT_OW.Length < 1)
                            return "Error";
                        return null;
                    case "IS_ORG":
                        return null;
                    case "DECEASED":
                        return null;
                    case "OWNER_ROLE":
                        if (OWNER_ROLE == null)
                            return null;
                        if (OWNER_ROLE.Length < 1)
                            return "Error";
                        return null;
                    case "OTHER_LIENS":
                        if (OTHER_LIENS == null)
                            return null;
                        if (OTHER_LIENS.Length < 1)
                            return "Error";
                        return null;
                    case "ADDL_DEF":
                        if (ADDL_DEF == null)
                            return null;
                        if (ADDL_DEF.Length < 1)
                            return "Error";
                        return null;
                    case "PUB_COMMENTS":
                        if (PUB_COMMENTS == null)
                            return null;
                        if (PUB_COMMENTS.Length < 1)
                            return "Error";
                        return null;
                    case "INT_COMMENTS":
                        if (INT_COMMENTS == null)
                            return null;
                        if (INT_COMMENTS.Length < 1)
                            return "Error";
                        return null;
                    case "ATTY":
                        if (ATTY == null)
                            return null;
                        if (ATTY.Length < 1)
                            return "Error";
                        return null;
                    case "ATTORNEY_S":
                        if (ATTORNEY_S == null)
                            return null;
                        if (Regex.IsMatch(ATTORNEY_S, "_"))
                            return "Error";
                        return null;
                    case "TYPE_OF_MO":
                        if (TYPE_OF_MO == null)
                            return null;
                        if (TYPE_OF_MO.Length < 1)
                            return "Error";
                        return null;
                    case "PROP_DESC":
                        if (PROP_DESC == null)
                            return null;
                        if (PROP_DESC.Length < 1)
                            return "Error";
                        return null;
                    case "INTEREST_R":
                        return null;
                    case "MONTHLY_PAY":
                        return null;
                    case "TERM_OF_MTG":
                        return null;
                    case "DEF_ADDRESS":
                        if (DEF_ADDRESS == null)
                            return null;
                        if (DEF_ADDRESS.Length < 1)
                            return "Error";
                        return null;
                    case "DEF_PHONE":
                        if (DEF_PHONE == null)
                            return null;
                        if (Regex.IsMatch(DEF_PHONE, "_"))
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