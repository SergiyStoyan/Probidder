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
    public partial class ForeclosureView : INotifyPropertyChanged, INotifyDataErrorInfo
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
            Model.TERM_OF_MTG = 30;
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
            get
            {
                string value = Model.TYPE_OF_EN;
                check("TYPE_OF_EN", string.IsNullOrEmpty(value) ? "Error" : null);
                return value;
            }
            set
            {
                edited = true;
                Model.TYPE_OF_EN = value;
            }
        }
        public string COUNTY
        {
            get
            {
                string value = Model.COUNTY;
                check("COUNTY", string.IsNullOrEmpty(value) ? "Error" : null);
                return value;
            }
            set
            {
                edited = true;
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
            get
            {
                string value = Model.CASE_N;
                check("CASE_N", value != null && (Regex.IsMatch(value, @"[^\w\-_]") || value.Length > 16) ? "Error" : null);
                return value;
            }
            set
            {
                edited = true;
                Model.CASE_N = value;
            }
        }
        //public DateTime? FILING_DATE { get { return Model.FILING_DATE; } set {  Model.FILING_DATE = value; } }
        public string FILING_DATE
        {
            get
            {
                string value;
                if (_FILING_DATE != null)
                    value = _FILING_DATE;
                else
                    value = DatePickerControl.GetMaskedString(Model.FILING_DATE);
                check("FILING_DATE", (DatePickerControl.ParseText(value) == null) ? "Error" : null);
                return value;
            }
            set
            {
                edited = true;
                _FILING_DATE = value;
                DateTime? dt = DatePickerControl.ParseText(value);
                Model.FILING_DATE = dt;
            }
        }
        string _FILING_DATE = null;
        public DateTime? AUCTION_DATE
        {
            get
            {
                check("AUCTION_DATE", null);
                return Model.AUCTION_DATE;
            }
            set
            {
                edited = true;
                Model.AUCTION_DATE = value;
            }
        }
        public DateTime? AUCTION_TIME
        {
            get
            {
                check("AUCTION_TIME", null);
                return Model.AUCTION_TIME;
            }
            set
            {
                edited = true;
                Model.AUCTION_TIME = value;
            }
        }
        public string SALE_LOC
        {
            get
            {
                check("SALE_LOC", null);
                return Model.SALE_LOC;
            }
            set
            {
                edited = true;
                Model.SALE_LOC = value;
            }
        }
        //public DateTime? ENTRY_DATE { get { return Model.ENTRY_DATE; } set {  Model.ENTRY_DATE = value; } }
        public string ENTRY_DATE
        {
            get
            {
                string value;
                if (_ENTRY_DATE != null)
                    value = _ENTRY_DATE;
                else
                    value = DatePickerControl.GetMaskedString(Model.ENTRY_DATE);
                check("ENTRY_DATE", (DatePickerControl.ParseText(value) == null) ? "Error" : null);
                return value;
            }
            set
            {
                edited = true;
                _ENTRY_DATE = value;
                DateTime? dt = DatePickerControl.ParseText(value);
                Model.ENTRY_DATE = dt;
            }
        }
        string _ENTRY_DATE = null;
        public string LENDOR
        {
            get
            {
                string value = Model.LENDOR;
                check("LENDOR", string.IsNullOrEmpty(value) ? "Error" : null);
                return value;
            }
            set
            {
                edited = true;
                Model.LENDOR = value;
            }
        }
        //public DateTime? ORIGINAL_MTG { get { return Model.ORIGINAL_MTG; } set {  Model.ORIGINAL_MTG = value; } }  
        public string ORIGINAL_MTG
        {
            get
            {
                string value;
                if (_ORIGINAL_MTG != null)
                    value = _ORIGINAL_MTG;
                else
                    value = DatePickerControl.GetMaskedString(Model.ORIGINAL_MTG);
                check("ORIGINAL_MTG", (DatePickerControl.ParseText(value) == null) ? "Error" : null);
                return value;
            }
            set
            {
                edited = true;
                _ORIGINAL_MTG = value;
                DateTime? dt = DatePickerControl.ParseText(value);
                Model.ORIGINAL_MTG = dt;
            }
        }
        string _ORIGINAL_MTG = null;
        public string DOCUMENT_N
        {
            get
            {
                check("DOCUMENT_N", null);
                return Model.DOCUMENT_N;
            }
            set
            {
                edited = true;
                Model.DOCUMENT_N = value;
            }
        }
        public uint? ORIGINAL_I
        {
            get
            {
                check("ORIGINAL_I", null);
                return Model.ORIGINAL_I;
            }
            set
            {
                edited = true;
                Model.ORIGINAL_I = value;
            }
        }
        public string LEGAL_D
        {
            get
            {
                check("LEGAL_D", null);
                return Model.LEGAL_D;
            }
            set
            {
                edited = true;
                Model.LEGAL_D = value;
            }
        }
        public string ADDRESS
        {
            get
            {
                string value = Model.ADDRESS;
                check("ADDRESS", string.IsNullOrEmpty(value) ? "Error" : null);
                return value;
            }
            set
            {
                edited = true;
                Model.ADDRESS = value;
            }
        }
        public string CITY
        {
            get
            {
                string value = Model.CITY;
                check("CITY", string.IsNullOrEmpty(value) ? "Error" : null);
                return value;
            }
            set
            {
                edited = true;
                Model.CITY = value;
                OnPropertyChanged("ZIPs");
            }
        }
        public string ZIP
        {
            get
            {
                string value = Model.ZIP;
                check("ZIP", !string.IsNullOrEmpty(value) && (Regex.IsMatch(value, @"[^\d]") || value.Length > 5 || value.Length < 4) ? "Error" : null);
                return value;
            }
            set
            {
                edited = true;
                Model.ZIP = value;
            }
        }
        public string PIN
        {
            get
            {
                string value = Model.PIN;
                check("PIN", !string.IsNullOrEmpty(value) && (!Regex.IsMatch(value, "[^_]{2}-[^_]{2}-[^_]{3}-[^_]{3}-[^_]{4}") && !Regex.IsMatch(value, "[^_]{2}-[^_]{2}-[^_]{3}-[^_]{3}-[_]{4}")) ? "Error" : null);
                return Model.PIN;
            }
            set
            {
                edited = true;
                Model.PIN = value;
            }
        }
        //public DateTime? DATE_OF_CA { get { return Model.DATE_OF_CA; } set {  Model.DATE_OF_CA = value; } }  
        public string DATE_OF_CA
        {
            get
            {
                string value;
                if (_DATE_OF_CA != null)
                    value = _DATE_OF_CA;
                else
                    value = DatePickerControl.GetMaskedString(Model.DATE_OF_CA);
                check("DATE_OF_CA", (DatePickerControl.ParseText(value) == null) ? "Error" : null);
                return value;
            }
            set
            {
                edited = true;
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
                string value;
                if (_LAST_PAY_DATE != null)
                    value = _LAST_PAY_DATE;
                else
                    value = DatePickerControl.GetMaskedString(Model.LAST_PAY_DATE);
                check("LAST_PAY_DATE", null);
                return value;
            }
            set
            {
                edited = true;
                _LAST_PAY_DATE = value;
                DateTime? dt = DatePickerControl.ParseText(value);
                Model.LAST_PAY_DATE = dt;
            }
        }
        string _LAST_PAY_DATE = null;
        public uint? BALANCE_DU
        {
            get
            {
                check("BALANCE_DU", null);
                return Model.BALANCE_DU;
            }
            set
            {
                edited = true;
                Model.BALANCE_DU = value;
            }
        }
        public decimal? PER_DIEM_I
        {
            get
            {
                check("PER_DIEM_I", null);
                return Model.PER_DIEM_I;
            }
            set
            {
                edited = true;
                Model.PER_DIEM_I = value;
            }
        }
        public string CURRENT_OW
        {
            get
            {
                string value = Model.CURRENT_OW;
                check("CURRENT_OW", string.IsNullOrEmpty(value) ? "Error" : null);
                return value;
            }
            set
            {
                edited = true;
                Model.CURRENT_OW = value;
            }
        }
        public bool IS_ORG
        {
            get
            {
                check("IS_ORG", null);
                return Model.IS_ORG;
            }
            set
            {
                edited = true;
                Model.IS_ORG = value;
            }
        }
        public bool DECEASED
        {
            get
            {
                check("DECEASED", null);
                return Model.DECEASED;
            }
            set
            {
                edited = true;
                Model.DECEASED = value;
            }
        }
        public string OWNER_ROLE
        {
            get
            {
                string value = Model.OWNER_ROLE;
                check("OWNER_ROLE", string.IsNullOrEmpty(value) ? "Error" : null);
                return value;
            }
            set
            {
                edited = true;
                Model.OWNER_ROLE = value;
            }
        }
        public string OTHER_LIENS
        {
            get
            {
                check("OTHER_LIENS", null);
                return Model.OTHER_LIENS;
            }
            set
            {
                edited = true;
                Model.OTHER_LIENS = value;
            }
        }
        public string ADDL_DEF
        {
            get
            {
                check("ADDL_DEF", null);
                return Model.ADDL_DEF;
            }
            set
            {
                edited = true;
                Model.ADDL_DEF = value;
            }
        }
        public string PUB_COMMENTS
        {
            get
            {
                check("PUB_COMMENTS", null);
                return Model.PUB_COMMENTS;
            }
            set
            {
                edited = true;
                Model.PUB_COMMENTS = value;
            }
        }
        public string INT_COMMENTS
        {
            get
            {
                check("INT_COMMENTS", null);
                return Model.INT_COMMENTS;
            }
            set
            {
                edited = true;
                Model.INT_COMMENTS = value;
            }
        }
        public string ATTY
        {
            get
            {
                check("ATTY", null);
                return Model.ATTY;
            }
            set
            {
                edited = true;
                Model.ATTY = value;
                OnPropertyChanged("ATTORNEY_Ss");
            }
        }
        public string ATTORNEY_S
        {
            get
            {
                string value = Model.ATTORNEY_S;
                check("ATTORNEY_S", !string.IsNullOrEmpty(value) && (Regex.IsMatch(value, @"_") && Regex.IsMatch(value, @"\d")) ? "Error" : null);
                return value;
            }
            set
            {
                edited = true;
                Model.ATTORNEY_S = value;
            }
        }
        public string TYPE_OF_MO
        {
            get
            {
                check("TYPE_OF_MO", null);
                return Model.TYPE_OF_MO;
            }
            set
            {
                edited = true;
                Model.TYPE_OF_MO = value;
            }
        }
        public string PROP_DESC
        {
            get
            {
                string value = Model.PROP_DESC;
                check("PROP_DESC", string.IsNullOrEmpty(value) ? "Error" : null);
                return value;
            }
            set
            {
                edited = true;
                Model.PROP_DESC = value;
            }
        }
        public decimal? INTEREST_R
        {
            get
            {
                check("INTEREST_R", null);
                return Model.INTEREST_R;
            }
            set
            {
                edited = true;
                Model.INTEREST_R = value;
            }
        }
        public decimal? MONTHLY_PAY
        {
            get
            {
                check("MONTHLY_PAY", null);
                return Model.MONTHLY_PAY;
            }
            set
            {
                edited = true;
                Model.MONTHLY_PAY = value;
            }
        }
        public uint? TERM_OF_MTG
        {
            get
            {
                check("TERM_OF_MTG", null);
                return Model.TERM_OF_MTG;
            }
            set
            {
                edited = true;
                Model.TERM_OF_MTG = value;
            }
        }
        public string DEF_ADDRESS
        {
            get
            {
                check("DEF_ADDRESS", null);
                return Model.DEF_ADDRESS;
            }
            set
            {
                edited = true;
                Model.DEF_ADDRESS = value;
            }
        }
        public string DEF_PHONE
        {
            get
            {
                string value = Model.DEF_PHONE;
                check("DEF_PHONE", !string.IsNullOrEmpty(value) && Regex.IsMatch(value, @"_") && Regex.IsMatch(value, @"\d") ? "Error" : null);
                return value;
            }
            set
            {
                edited = true;
                Model.DEF_PHONE = value;
            }
        }

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public void ValidateAllProperties()
        {
            forced_validation = true;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(null));
            forced_validation = false;
        }

        void check(string property, string error)
        {
            if (!edited && !forced_validation)
                return;
            string e0 = null;
            if (columnNames2error.TryGetValue(property, out e0))
                InitialControlSetting = false;
            columnNames2error[property] = error;
            if (e0 != error)
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(property));
        }
        readonly Dictionary<string, string> columnNames2error = new Dictionary<string, string>();
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public bool InitialControlSetting = true;

        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return null;
            return columnNames2error.Where(x => x.Key == propertyName && x.Value != null).Select(x => x.Key);
        }
        bool forced_validation = false;

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
                //return columnNames2error.Select(x => x.Key).FirstOrDefault() != null;
                return edited;
            }
        }
        bool edited
        {
            get;
            set;
        }
    }
}