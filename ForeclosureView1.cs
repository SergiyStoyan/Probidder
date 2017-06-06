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
    public partial class ForeclosureView : INotifyPropertyChanged, IDataErrorInfo//, INotifyDataErrorInfo//
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
                return (new Db.PropertyCodes()).GetAll().OrderBy(x => x.type).Select(x => x.type);
            }
        }

        public string TYPE_OF_EN
        {
            get { return Model.TYPE_OF_EN; }
            set
            {
                check("TYPE_OF_EN", string.IsNullOrEmpty(value) ? "Error" : null, Model.TYPE_OF_EN == value);
                Model.TYPE_OF_EN = value;
            }
        }
        public string COUNTY
        {
            get
            {
                return Model.COUNTY;
            }
            set
            {
                check("COUNTY", string.IsNullOrEmpty(value) ? "Error" : null, Model.COUNTY == value);
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
        public string CASE_N
        {
            get { return Model.CASE_N; }
            set
            {
                check("CASE_N", (Regex.IsMatch(value, @"[^\w\-_]") || value.Length > 16) ? "Error" : null, Model.CASE_N == value);
                Model.CASE_N = value;
            }
        }
        //public DateTime? FILING_DATE { get { return Model.FILING_DATE; } set {  Model.FILING_DATE = value; } }
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
                check("FILING_DATE", (DatePickerControl.ParseText(value) == null) ? "Error" : null, _FILING_DATE == value);
                if (_FILING_DATE == value)
                    return;
                _FILING_DATE = value;
                DateTime? dt = DatePickerControl.ParseText(value);
                Model.FILING_DATE = dt;
            }
        }
        string _FILING_DATE = null;
        public DateTime? AUCTION_DATE
        {
            get { return Model.AUCTION_DATE; }
            set
            {
                check("AUCTION_DATE", null, Model.AUCTION_DATE == value);
                Model.AUCTION_DATE = value;
            }
        }
        public DateTime? AUCTION_TIME
        {
            get { return Model.AUCTION_TIME; }
            set
            {
                check("AUCTION_TIME", null, Model.AUCTION_TIME == value);
                Model.AUCTION_TIME = value;
            }
        }
        public string SALE_LOC
        {
            get { return Model.SALE_LOC; }
            set
            {
                check("SALE_LOC", null, Model.SALE_LOC== value);
                Model.SALE_LOC = value;
            }
        }
        //public DateTime? ENTRY_DATE { get { return Model.ENTRY_DATE; } set {  Model.ENTRY_DATE = value; } }
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
                check("ENTRY_DATE", (DatePickerControl.ParseText(value) == null) ? "Error" : null, _ENTRY_DATE == value);
                if (_ENTRY_DATE == value)
                    return;
                _ENTRY_DATE = value;
                DateTime? dt = DatePickerControl.ParseText(value);
                Model.ENTRY_DATE = dt;
            }
        }
        string _ENTRY_DATE = null;
        public string LENDOR
        {
            get { return Model.LENDOR; }
            set
            {
                check("LENDOR", string.IsNullOrEmpty(value) ? "Error" : null, Model.LENDOR == value);
                Model.LENDOR = value;
            }
        }
        //public DateTime? ORIGINAL_MTG { get { return Model.ORIGINAL_MTG; } set {  Model.ORIGINAL_MTG = value; } }  
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
                check("ORIGINAL_MTG", (DatePickerControl.ParseText(value) == null) ? "Error" : null, _ORIGINAL_MTG == value);
                if (_ORIGINAL_MTG == value)
                    return;
                _ORIGINAL_MTG = value;
                DateTime? dt = DatePickerControl.ParseText(value);
                Model.ORIGINAL_MTG = dt;
            }
        }
        string _ORIGINAL_MTG = null;
        public string DOCUMENT_N
        {
            get { return Model.DOCUMENT_N; }
            set
            {
                check("DOCUMENT_N", null, Model.DOCUMENT_N == value);
                Model.DOCUMENT_N = value;
            }
        }
        public uint? ORIGINAL_I
        {
            get { return Model.ORIGINAL_I; }
            set
            {
                check("ORIGINAL_I", null, Model.ORIGINAL_I == value);
                Model.ORIGINAL_I = value;
            }
        }
        public string LEGAL_D
        {
            get { return Model.LEGAL_D; }
            set
            {
                check("LEGAL_D", null, Model.LEGAL_D == value);
                Model.LEGAL_D = value;
            }
        }
        public string ADDRESS
        {
            get { return Model.ADDRESS; }
            set
            {
                check("ADDRESS", string.IsNullOrEmpty(value) ? "Error" : null, Model.ADDRESS == value);
                Model.ADDRESS = value;
            }
        }
        public string CITY
        {
            get { return Model.CITY; }
            set
            {
                check("CITY", string.IsNullOrEmpty(value) ? "Error" : null, Model.CITY == value);
                if (Model.CITY == value)
                    return;
                Model.CITY = value;
                OnPropertyChanged("ZIPs");
            }
        }
        public string ZIP
        {
            get { return Model.ZIP; }
            set
            {
                check("ZIP", !string.IsNullOrEmpty(value) && (Regex.IsMatch(value, @"[^\d]") || value.Length > 5 || value.Length < 4) ? "Error" : null, Model.ZIP == value);
                Model.ZIP = value;
            }
        }
        public string PIN
        {
            get { return Model.PIN; }
            set
            {
                check("PIN", !string.IsNullOrEmpty(value) && (!Regex.IsMatch(value, "[^_]{2}-[^_]{2}-[^_]{3}-[^_]{3}-[^_]{4}") && !Regex.IsMatch(value, "[^_]{2}-[^_]{2}-[^_]{3}-[^_]{3}-[_]{4}")) ? "Error" : null, Model.PIN == value);
                Model.PIN = value;
            }
        }
        //public DateTime? DATE_OF_CA { get { return Model.DATE_OF_CA; } set {  Model.DATE_OF_CA = value; } }  
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
                check("DATE_OF_CA", (DatePickerControl.ParseText(value) == null) ? "Error" : null, _DATE_OF_CA == value);
                if (_DATE_OF_CA == value)
                    return;
                _DATE_OF_CA = value;
                DateTime? dt = DatePickerControl.ParseText(value);
                Model.DATE_OF_CA = dt;
            }
        }
        string _DATE_OF_CA = null;
        //public DateTime? LAST_PAY_DATE { get { return Model.LAST_PAY_DATE; } set {  Model.LAST_PAY_DATE = value; } }  
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
                check("LAST_PAY_DATE", null, _LAST_PAY_DATE == value);
                if (_LAST_PAY_DATE == value)
                    return;
                _LAST_PAY_DATE = value;
                DateTime? dt = DatePickerControl.ParseText(value);
                Model.LAST_PAY_DATE = dt;
            }
        }
        string _LAST_PAY_DATE = null;
        public uint? BALANCE_DU
        {
            get { return Model.BALANCE_DU; }
            set
            {
                check("BALANCE_DU", null, Model.BALANCE_DU == value);
                Model.BALANCE_DU = value;
            }
        }
        public decimal? PER_DIEM_I
        {
            get { return Model.PER_DIEM_I; }
            set
            {
                check("PER_DIEM_I", null, Model.PER_DIEM_I == value);
                Model.PER_DIEM_I = value;
            }
        }
        public string CURRENT_OW
        {
            get { return Model.CURRENT_OW; }
            set
            {
                check("CURRENT_OW", string.IsNullOrEmpty(value) ? "Error" : null, Model.CURRENT_OW == value);
                Model.CURRENT_OW = value;
            }
        }
        public bool IS_ORG
        {
            get { return Model.IS_ORG; }
            set
            {
                check("IS_ORG", null, Model.IS_ORG == value);
                Model.IS_ORG = value;
            }
        }
        public bool DECEASED
        {
            get { return Model.DECEASED; }
            set
            {
                check("DECEASED", null, Model.DECEASED == value);
                Model.DECEASED = value;
            }
        }
        public string OWNER_ROLE
        {
            get { return Model.OWNER_ROLE; }
            set
            {
                check("OWNER_ROLE", string.IsNullOrEmpty(value) ? "Error" : null, Model.OWNER_ROLE == value);
                Model.OWNER_ROLE = value;
            }
        }
        public string OTHER_LIENS
        {
            get { return Model.OTHER_LIENS; }
            set
            {
                check("OTHER_LIENS", null, Model.OTHER_LIENS == value);
                Model.OTHER_LIENS = value;
            }
        }
        public string ADDL_DEF
        {
            get { return Model.ADDL_DEF; }
            set
            {
                check("ADDL_DEF", null, Model.ADDL_DEF == value);
                Model.ADDL_DEF = value;
            }
        }
        public string PUB_COMMENTS
        {
            get { return Model.PUB_COMMENTS; }
            set
            {
                check("PUB_COMMENTS", null, Model.PUB_COMMENTS == value);
                Model.PUB_COMMENTS = value;
            }
        }
        public string INT_COMMENTS
        {
            get { return Model.INT_COMMENTS; }
            set
            {
                check("INT_COMMENTS", null, Model.INT_COMMENTS == value);
                Model.INT_COMMENTS = value;
            }
        }
        public string ATTY
        {
            get { return Model.ATTY; }
            set
            {
                check("ATTY", null, Model.ATTY == value);
                if (Model.ATTY == value)
                    return;
                Model.ATTY = value;
                OnPropertyChanged("ATTORNEY_Ss");
            }
        }
        public string ATTORNEY_S
        {
            get { return Model.ATTORNEY_S; }
            set
            {
                check("ATTORNEY_S", !string.IsNullOrEmpty(value) && (Regex.IsMatch(value, @"_") && Regex.IsMatch(value, @"\d")) ? "Error" : null, Model.ATTORNEY_S == value);
                Model.ATTORNEY_S = value;
            }
        }
        public string TYPE_OF_MO
        {
            get { return Model.TYPE_OF_MO; }
            set
            {
                check("TYPE_OF_MO", null, Model.TYPE_OF_MO == value);
                Model.TYPE_OF_MO = value;
            }
        }
        public string PROP_DESC
        {
            get { return Model.PROP_DESC; }
            set
            {
                check("PROP_DESC", string.IsNullOrEmpty(value) ? "Error" : null, Model.PROP_DESC == value);
                Model.PROP_DESC = value;
            }
        }
        public decimal? INTEREST_R
        {
            get { return Model.INTEREST_R; }
            set
            {
                check("INTEREST_R", null, Model.INTEREST_R == value);
                Model.INTEREST_R = value;
            }
        }
        public decimal? MONTHLY_PAY
        {
            get { return Model.MONTHLY_PAY; }
            set
            {
                check("MONTHLY_PAY", null, Model.MONTHLY_PAY == value);
                Model.MONTHLY_PAY = value;
            }
        }
        public uint? TERM_OF_MTG
        {
            get { return Model.TERM_OF_MTG; }
            set
            {
                check("TERM_OF_MTG", null, Model.TERM_OF_MTG == value);
                Model.TERM_OF_MTG = value;
            }
        }
        public string DEF_ADDRESS
        {
            get { return Model.DEF_ADDRESS; }
            set
            {
                check("DEF_ADDRESS", null, Model.DEF_ADDRESS == value);
                Model.DEF_ADDRESS = value;
            }
        }
        public string DEF_PHONE
        {
            get { return Model.DEF_PHONE; }
            set
            {
                check("DEF_PHONE", !string.IsNullOrEmpty(value) && Regex.IsMatch(value, @"_") && Regex.IsMatch(value, @"\d") ? "Error" : null, Model.DEF_PHONE == value);
                Model.DEF_PHONE = value;
            }
        }

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        void check(string property, string error, bool unchanged)
        {
            string e0 = null;
            if (columnNames2error.TryGetValue(property, out e0))
                InitialControlSetting = false;
            columnNames2error[property] = error;
            if (e0 != error)
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(property));
            //PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(columnName));
            columnNames2changed[property] = !unchanged;
        }
        Dictionary<string, string> columnNames2error = new Dictionary<string, string>();
        Dictionary<string, bool> columnNames2changed = new Dictionary<string, bool>();

        public string Error
        {
            get { return "Error..."; }
            set { }
        }

        public string this[string columnName]
        {
            get
            {
                if (InitialControlSetting)
                    return null;
                string e = null;
                columnNames2error.TryGetValue(columnName, out e);
                return e;
            }
        }
        public bool InitialControlSetting = true;
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        //public IEnumerable GetErrors(string propertyName)
        //{
        //    return columnNames2error.Where(x => x.Key == propertyName && x.Value != null).Select(x => x.Key);
        //}

        public bool HasErrors
        {
            get
            {
                return columnNames2error.Where(x => x.Value != null).Select(x => x.Key).FirstOrDefault() != null;
            }
        }

        public bool Edited
        {
            get
            {
                return columnNames2changed.Where(x => x.Value).Select(x => x.Key).FirstOrDefault() != null;
            }
        }
    }
}