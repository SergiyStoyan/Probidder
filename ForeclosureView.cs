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
using System.Collections.ObjectModel;

namespace Cliver.Foreclosures
{
    public partial class ForeclosureView : INotifyPropertyChanged, IDataErrorInfo, INotifyDataErrorInfo
    {
        public ForeclosureView()
        {
            Model = new Db.Foreclosure();
            set_new_model();
        }

        public ForeclosureView(Db.Foreclosure f)
        {
            if (f != null)
                Model = f;
            else
            {
                Model = new Db.Foreclosure();
                set_new_model();
            }
        }

        void set_new_model()
        {
            Model.COUNTY = Settings.Location.County;
            Model.TYPE_OF_EN = "CHA";
            Model.CASE_N = CASE_Ns.FirstOrDefault();
            Model.ENTRY_DATE = DateTime.Now;
            Model.IS_ORG = false;
            Model.DECEASED = false;
            Model.OWNER_ROLE = "OWNER";
            Model.TYPE_OF_MO = "CNV";
            Model.PROP_DESC = "SINGLE FAMILY";
            Model.MONTHLY_PAY = 30;
        }
        readonly Db.Foreclosure Model;

        public int Id { get { return Model.Id; } }

        public IEnumerable<string> CASE_Ns
        {
            get
            {
                if (string.IsNullOrEmpty(COUNTY))
                    return null;
                return (new Db.CaseNumbers()).GetBy(COUNTY).case_ns.OrderBy(x => x);
            }
        }    
        public IEnumerable<string> LENDORs
        {
            get
            {
                if (string.IsNullOrEmpty(COUNTY))
                    return null;
                return (new Db.Plaintiffs()).GetBy(COUNTY).OrderBy(x => x.plaintiff).Select(x => x.plaintiff);
            }
        }
        public IEnumerable<string> CITYs
        {
            get
            {
                return (new Db.Cities()).GetBy(COUNTY).OrderBy(x => x.city).Select(x => x.city);
            }
        }
        public IEnumerable<string> ZIPs
        {
            get
            {
                if (string.IsNullOrEmpty(COUNTY))
                    return null;
                if (string.IsNullOrEmpty(CITY))
                    return null;
                return (new Db.Zips()).GetBy(COUNTY, CITY).OrderBy(x => x.zip).Select(x => x.zip);
            }
        }
        public IEnumerable<string> OWNER_ROLEs
        {
            get
            {
                return (new Db.OwnerRoles()).GetAll().OrderBy(x => x.role).Select(x => x.role);
            }
        }
        public IEnumerable<string> ATTYs
        {
            get
            {
                if (string.IsNullOrEmpty(COUNTY))
                    return null;
                return (new Db.Attorneys()).GetBy(COUNTY).OrderBy(x => x.attorney).Select(x => x.attorney);
            }
        }
        public IEnumerable<string> ATTORNEY_Ss
        {
            get
            {
                if (string.IsNullOrEmpty(COUNTY))
                    return null;
                if (string.IsNullOrEmpty(ATTY))
                    return null;
                return ComboBoxPhoneControl.GetItemsNormalized((new Db.AttorneyPhones()).GetBy(COUNTY, ATTY).OrderBy(x => x.attorney_phone).Select(x => x.attorney_phone));
            }
        }
        public IEnumerable<string> TYPE_OF_MOs
        {
            get
            {
                return (new Db.MortgageTypes()).Get().OrderBy(x => x.mortgage_type).Select(x => x.mortgage_type);
            }
        }
        public IEnumerable<string> PROP_DESCs
        {
            get
            {
                return  (new Db.PropertyCodes()).GetAll().OrderBy(x => x.type).Select(x => x.type);
            }
        }

        public string TYPE_OF_EN { get { return Model.TYPE_OF_EN; } set { InitialControlSetting = false; Model.TYPE_OF_EN = value; } }
        public string COUNTY {
            get
            {
                return Model.COUNTY;
            }
            set
            {
                InitialControlSetting = false;
                if (Model.COUNTY == value)
                    return;
                Model.COUNTY = value;
                OnPropertyChanged("CASE_Ns"); 
                OnPropertyChanged("LENDORs"); 
                OnPropertyChanged("CITYs"); 
                OnPropertyChanged("ZIPs"); 
                OnPropertyChanged("ATTYs"); 
                OnPropertyChanged("ATTORNEY_Ss"); 
            }
        }        
        public string CASE_N { get { return Model.CASE_N; } set { InitialControlSetting = false; Model.CASE_N = value; } }
        //public DateTime? FILING_DATE { get { return Model.FILING_DATE; } set { InitialControlSetting = false; Model.FILING_DATE = value; } }
        public string FILING_DATE
        {
            get
            {
                if (_FILING_DATE != null)
                    return _FILING_DATE;
                return DatePickerControl.GetMaskedString(Model.FILING_DATE);
            }
            set
            {
                InitialControlSetting = false;
                if (_FILING_DATE == value)
                    return;
                _FILING_DATE = value;
                DateTime? dt = DatePickerControl.ParseText(value);
                Model.FILING_DATE = dt;
            }
        }
        string _FILING_DATE = null;
        public DateTime? AUCTION_DATE { get { return Model.AUCTION_DATE; } set { InitialControlSetting = false; Model.AUCTION_DATE = value; } }  
        public DateTime? AUCTION_TIME { get { return Model.AUCTION_TIME; } set { InitialControlSetting = false; Model.AUCTION_TIME = value; } }  
        public string SALE_LOC { get { return Model.SALE_LOC; } set { InitialControlSetting = false; Model.SALE_LOC = value; } }
        //public DateTime? ENTRY_DATE { get { return Model.ENTRY_DATE; } set { InitialControlSetting = false; Model.ENTRY_DATE = value; } }
        public string ENTRY_DATE
        {
            get
            {
                if (_ENTRY_DATE != null)
                    return _ENTRY_DATE;
                return DatePickerControl.GetMaskedString(Model.ENTRY_DATE);
            }
            set
            {
                InitialControlSetting = false;
                if (_ENTRY_DATE == value)
                    return;
                _ENTRY_DATE = value;
                DateTime? dt = DatePickerControl.ParseText(value);
                Model.ENTRY_DATE = dt;
            }
        }
        string _ENTRY_DATE = null;
        public string LENDOR { get { return Model.LENDOR; } set { InitialControlSetting = false; Model.LENDOR = value; } }
        //public DateTime? ORIGINAL_MTG { get { return Model.ORIGINAL_MTG; } set { InitialControlSetting = false; Model.ORIGINAL_MTG = value; } }  
        public string ORIGINAL_MTG
        {
            get
            {
                if (_ORIGINAL_MTG != null)
                    return _ORIGINAL_MTG;
                return DatePickerControl.GetMaskedString(Model.ORIGINAL_MTG);
            }
            set
            {
                InitialControlSetting = false;
                if (_ORIGINAL_MTG == value)
                    return;
                _ORIGINAL_MTG = value;
                DateTime? dt = DatePickerControl.ParseText(value);
                Model.ORIGINAL_MTG = dt;
            }
        }
        string _ORIGINAL_MTG = null;
        public string DOCUMENT_N { get { return Model.DOCUMENT_N; } set { InitialControlSetting = false; Model.DOCUMENT_N = value; } }  
        public uint? ORIGINAL_I { get { return Model.ORIGINAL_I; } set { InitialControlSetting = false; Model.ORIGINAL_I = value; } }  
        public string LEGAL_D { get { return Model.LEGAL_D; } set { InitialControlSetting = false; Model.LEGAL_D = value; } }  
        public string ADDRESS { get { return Model.ADDRESS; } set { InitialControlSetting = false; Model.ADDRESS = value; } }  
        public string CITY { get { return Model.CITY; }
            set
            {
                InitialControlSetting = false;
                if (Model.CITY == value)
                    return;
                Model.CITY = value;
                OnPropertyChanged("ZIPs");
            }
        }  
        public string ZIP { get { return Model.ZIP; } set { InitialControlSetting = false; Model.ZIP = value; } }  
        public string PIN { get { return Model.PIN; } set { InitialControlSetting = false; Model.PIN = value; } }
        //public DateTime? DATE_OF_CA { get { return Model.DATE_OF_CA; } set { InitialControlSetting = false; Model.DATE_OF_CA = value; } }  
        public string DATE_OF_CA
        {
            get
            {
                if (_DATE_OF_CA != null)
                    return _DATE_OF_CA;
                return DatePickerControl.GetMaskedString(Model.DATE_OF_CA);
            }
            set
            {
                InitialControlSetting = false;
                if (_DATE_OF_CA == value)
                    return;
                _DATE_OF_CA = value;
                DateTime? dt = DatePickerControl.ParseText(value);
                Model.DATE_OF_CA = dt;
            }
        }
        string _DATE_OF_CA = null;
        //public DateTime? LAST_PAY_DATE { get { return Model.LAST_PAY_DATE; } set { InitialControlSetting = false; Model.LAST_PAY_DATE = value; } }  
        public string LAST_PAY_DATE
        {
            get
            {
                if (_LAST_PAY_DATE != null)
                    return _LAST_PAY_DATE;
                return DatePickerControl.GetMaskedString(Model.LAST_PAY_DATE);
            }
            set
            {
                InitialControlSetting = false;
                if (_LAST_PAY_DATE == value)
                    return;
                _LAST_PAY_DATE = value;
                DateTime? dt = DatePickerControl.ParseText(value);
                Model.LAST_PAY_DATE = dt;
            }
        }
        string _LAST_PAY_DATE = null;
        public uint? BALANCE_DU { get { return Model.BALANCE_DU; } set { InitialControlSetting = false; Model.BALANCE_DU = value; } }  
        public decimal? PER_DIEM_I { get { return Model.PER_DIEM_I; } set { InitialControlSetting = false; Model.PER_DIEM_I = value; } }  
        public string CURRENT_OW { get { return Model.CURRENT_OW; } set { Model.CURRENT_OW = value; } }  
        public bool IS_ORG { get { return Model.IS_ORG; } set { InitialControlSetting = false; Model.IS_ORG = value; } }
        public bool DECEASED { get { return Model.DECEASED; } set { InitialControlSetting = false; Model.DECEASED = value; } }
        public string OWNER_ROLE { get { return Model.OWNER_ROLE; } set { InitialControlSetting = false; Model.OWNER_ROLE = value; } }
        public string OTHER_LIENS { get { return Model.OTHER_LIENS; } set { InitialControlSetting = false; Model.OTHER_LIENS = value; } }  
        public string ADDL_DEF { get { return Model.ADDL_DEF; } set { InitialControlSetting = false; Model.ADDL_DEF = value; } }  
        public string PUB_COMMENTS { get { return Model.PUB_COMMENTS; } set { InitialControlSetting = false; Model.PUB_COMMENTS = value; } }  
        public string INT_COMMENTS { get { return Model.INT_COMMENTS; } set { InitialControlSetting = false; Model.INT_COMMENTS = value; } }  
        public string ATTY { get { return Model.ATTY; }
            set
            {
                InitialControlSetting = false;
                if (Model.ATTY == value)
                    return;
                Model.ATTY = value;
                OnPropertyChanged("ATTORNEY_Ss");
            } }  
        public string ATTORNEY_S { get { return Model.ATTORNEY_S; } set { InitialControlSetting = false; Model.ATTORNEY_S = value; } }  
        public string TYPE_OF_MO { get { return Model.TYPE_OF_MO; } set { InitialControlSetting = false; Model.TYPE_OF_MO = value; } }
        public string PROP_DESC { get { return Model.PROP_DESC; } set { InitialControlSetting = false; Model.PROP_DESC = value; } }
        public decimal? INTEREST_R { get { return Model.INTEREST_R; } set { InitialControlSetting = false; Model.INTEREST_R = value; } }  
        public decimal? MONTHLY_PAY { get { return Model.MONTHLY_PAY; } set { InitialControlSetting = false; Model.MONTHLY_PAY = value; } }
        public uint? TERM_OF_MTG { get { return Model.TERM_OF_MTG; } set { InitialControlSetting = false; Model.TERM_OF_MTG = value; } }  
        public string DEF_ADDRESS { get { return Model.DEF_ADDRESS; } set { InitialControlSetting = false; Model.DEF_ADDRESS = value; } }  
        public string DEF_PHONE { get { return Model.DEF_PHONE; } set { InitialControlSetting = false; Model.DEF_PHONE = value; } }
        
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public string Error
        {
            get { return "...."; }
            set { }
        }

        Dictionary<string, string> columnNames2error = new Dictionary<string, string>();

        public string this[string columnName]
        {
            get
            {
                if (InitialControlSetting)
                    return null;
                string e = validate(columnName);
                string e0 = null;
                columnNames2error.TryGetValue(columnName, out e0);
                columnNames2error[columnName] = e;
                if (e0 != e)
                    ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(columnName));
                //PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(columnName));
                return e;
            }
        }
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public bool HasErrors
        {
            get
            {
                return columnNames2error.Where(x => x.Value != null).Select(x => x.Key).FirstOrDefault() != null;
            }
        }

        public bool InitialControlSetting = true;

        public bool Edited
        {
            get
            {
                return columnNames2error.Select(x => x.Key).FirstOrDefault() != null;
            }
        }

        private string validate(string propertyName)
        {
            switch (propertyName)
            {
                case "TYPE_OF_EN":
                    if (string.IsNullOrEmpty(TYPE_OF_EN))
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
                    //if (FILING_DATE == null)
                    //    return "Error";
                    if (DatePickerControl.ParseText(FILING_DATE) == null)
                        return "Error";
                    return null;
                case "AUCTION_DATE":
                    return null;
                case "AUCTION_TIME":
                    return null;
                case "ENTRY_DATE":
                    //if (ENTRY_DATE == null)
                    //    return "Error";
                    if (DatePickerControl.ParseText(ENTRY_DATE) == null)
                        return "Error";
                    return null;
                case "LENDOR":
                    if (string.IsNullOrEmpty(LENDOR))
                        return "Error";
                    return null;
                case "ORIGINAL_MTG":
                    //if (ORIGINAL_MTG == null)
                    //    return "Error";
                    if (DatePickerControl.ParseText(ORIGINAL_MTG) == null)
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
                    //if (DATE_OF_CA == null)
                    //    return "Error";
                    if (DatePickerControl.ParseText(DATE_OF_CA) == null)
                        return "Error";
                    return null;
                case "LAST_PAY_DATE":
                    //if (LAST_PAY_DATE == null)
                    //    return "Error";
                    if (DatePickerControl.ParseText(LAST_PAY_DATE) == null)
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

        public IEnumerable GetErrors(string propertyName)
        {
            return columnNames2error.Where(x => x.Key == propertyName && x.Value != null).Select(x => x.Key);
        }
    }
}