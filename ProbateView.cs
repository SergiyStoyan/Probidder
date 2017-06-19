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

namespace Cliver.Probidder
{
    public partial class ProbateView : View<Db.Probate>
    {
        public ProbateView() : base() { }
        public ProbateView(Db.Probate p) : base(p) { }

        override protected void set_new_model()
        {
            Model.FillingCounty = Settings.Location.County;
            Model.FillingState = "IL";
            Model.CaseNumber = CASE_Ns?.FirstOrDefault();
        }
        
        public IEnumerable<string> CASE_Ns
        {
            get
            {
                if (string.IsNullOrEmpty(FillingCounty))
                    return null;
                return (new Db.ProbateCaseNumbers()).GetBy(FillingCounty).case_ns.OrderBy(x => x);
            }
        }
        public IEnumerable<string> CITYs
        {
            get
            {
                return (new Db.Cities()).GetBy(FillingCounty).OrderBy(x => x.city).Select(x => x.city);
            }
        }
        public IEnumerable<string> DeceasedZips
        {
            get
            {
                if (string.IsNullOrEmpty(FillingCounty))
                    return null;
                if (string.IsNullOrEmpty(DeceasedCity))
                    return null;
                return (new Db.Zips()).GetBy(FillingCounty, DeceasedCity).OrderBy(x => x.zip).Select(x => x.zip);
            }
        }
        public IEnumerable<string> AdministratorZips
        {
            get
            {
                if (string.IsNullOrEmpty(FillingCounty))
                    return null;
                if (string.IsNullOrEmpty(AdministratorCity))
                    return null;
                return (new Db.Zips()).GetBy(FillingCounty, AdministratorCity).OrderBy(x => x.zip).Select(x => x.zip);
            }
        }
        public IEnumerable<string> Attorneys
        {
            get
            {
                if (string.IsNullOrEmpty(FillingCounty))
                    return null;
                return (new Db.Attorneys()).GetBy(FillingCounty).OrderBy(x => x.attorney).Select(x => x.attorney);
            }
        }
        public IEnumerable<string> AttorneyPhones
        {
            get
            {
                if (string.IsNullOrEmpty(FillingCounty))
                    return null;
                if (string.IsNullOrEmpty(Attorney))
                    return null;
                return ComboBoxPhoneControl.GetItemsNormalized((new Db.AttorneyPhones()).GetBy(FillingCounty, Attorney).OrderBy(x => x.attorney_phone).Select(x => x.attorney_phone));
            }
        }
        public IEnumerable<string> HeirOrOthers
        {
            get
            {
                return Enum.GetNames(typeof(Db.Probate.HeirOrOthers));
            }
        }        

        public string FillingCounty
        {
            get
            {
                string value = Model.FillingCounty;
                check("FillingCounty", string.IsNullOrEmpty(value) ? "Error" : null);
                return value;
            }
            set
            {
                edited = true;
                Model.FillingCounty = value;
                OnPropertyChanged("CASE_Ns");
                OnPropertyChanged("CITYs");
                OnPropertyChanged("DeceasedZips");
                OnPropertyChanged("AdministratorZips");
                OnPropertyChanged("Attorneys");
                OnPropertyChanged("AttorneyPhones");
            }
        }
        public string CaseNumber
        {
            get
            {
                string value = Model.CaseNumber;
                check("CaseNumber", value != null && (Regex.IsMatch(value, @"[^\w\-_]") || value.Length > 16) ? "Error" : null);
                return value;
            }
            set
            {
                edited = true;
                Model.CaseNumber = value;
            }
        }
        public string FillingDate
        {
            get
            {
                string value;
                if (_FillingDate != null)
                    value = _FillingDate;
                else
                    value = DatePickerControl.GetMaskedString(Model.FillingDate);
                check("FillingDate", (DatePickerControl.ParseText(value) == null) ? "Error" : null);
                return value;
            }
            set
            {
                edited = true;
                _FillingDate = value;
                DateTime? dt = DatePickerControl.ParseText(value);
                Model.FillingDate = dt;
            }
        }
        string _FillingDate = null;
        public string DeceasedFullName
        {
            get
            {
                check("DeceasedFullName", null);
                return Model.DeceasedFullName;
            }
            set
            {
                edited = true;
                Model.DeceasedFullName = value;
            }
        }
        public string AdministratorFullName
        {
            get
            {
                check("AdministratorFullName", null);
                return Model.AdministratorFullName;
            }
            set
            {
                edited = true;
                Model.AdministratorFullName = value;
            }
        }
        public string DeceasedStreetLogo
        {
            get
            {
                check("DeceasedStreetLogo", null);
                return Model.DeceasedStreetLogo;
            }
            set
            {
                edited = true;
                Model.DeceasedStreetLogo = value;
            }
        }
        public string AdministratorStreetLogo
        {
            get
            {
                check("AdministratorStreetLogo", null);
                return Model.AdministratorStreetLogo;
            }
            set
            {
                edited = true;
                Model.AdministratorStreetLogo = value;
            }
        }
        public string AdministratorAddress
        {
            get
            {
                check("AdministratorAddress", null);
                return Model.AdministratorAddress;
            }
            set
            {
                edited = true;
                Model.AdministratorAddress = value;
            }
        }
        public string Attorney
        {
            get
            {
                check("Attorney", null);
                return Model.Attorney;
            }
            set
            {
                edited = true;
                Model.Attorney = value;
                OnPropertyChanged("Attorneys");
            }
        }
        public string AttorneyPhone
        {
            get
            {
                string value = Model.AttorneyPhone;
                check("AttorneyPhone", !string.IsNullOrEmpty(value) && (Regex.IsMatch(value, @"_") && Regex.IsMatch(value, @"\d")) ? "Error" : null);
                return value;
            }
            set
            {
                edited = true;
                Model.AttorneyPhone = value;
            }
        }
        public string DeceasedAddress
        {
            get
            {
                check("DeceasedAddress", null);
                return Model.DeceasedAddress;
            }
            set
            {
                edited = true;
                Model.DeceasedAddress = value;
            }
        }
        public string Comments
        {
            get
            {
                check("Comments", null);
                return Model.Comments;
            }
            set
            {
                edited = true;
                Model.Comments = value;
            }
        }
        public string DeceasedCity
        {
            get
            {
                check("DeceasedCity", null);
                return Model.DeceasedCity;
            }
            set
            {
                edited = true;
                Model.DeceasedCity = value;
            }
        }
        public string AdministratorCity
        {
            get
            {
                check("AdministratorCity", null);
                return Model.AdministratorCity;
            }
            set
            {
                edited = true;
                Model.AdministratorCity = value;
            }
        }
        public string DeceasedCounty
        {
            get
            {
                check("DeceasedCounty", null);
                return Model.DeceasedCounty;
            }
            set
            {
                edited = true;
                Model.DeceasedCounty = value;
            }
        }
        public string AdministratorState
        {
            get
            {
                check("AdministratorState", null);
                return Model.AdministratorState;
            }
            set
            {
                edited = true;
                Model.AdministratorState = value;
            }
        }
        public string FillingState
        {
            get
            {
                string value = Model.FillingState;
                check("FillingState", string.IsNullOrEmpty(value) ? "Error" : null);
                return value;
            }
            set
            {
                edited = true;
                Model.FillingState = value;
            }
        }
        public string AdministratorZip
        {
            get
            {
                string value = Model.AdministratorZip;
                check("AdministratorZip", !string.IsNullOrEmpty(value) && (Regex.IsMatch(value, @"[^\d]") || value.Length > 5 || value.Length < 4) ? "Error" : null);
                return value;
            }
            set
            {
                edited = true;
                Model.AdministratorZip = value;
            }
        }
        public string DeceasedZip
        {
            get
            {
                string value = Model.DeceasedZip;
                check("DeceasedZip", !string.IsNullOrEmpty(value) && (Regex.IsMatch(value, @"[^\d]") || value.Length > 5 || value.Length < 4) ? "Error" : null);
                return value;
            }
            set
            {
                edited = true;
                Model.DeceasedZip = value;
            }
        }
        public string DeathDate
        {
            get
            {
                string value;
                    if (_DeathDate != null)
                    value = _DeathDate;
                else
                    value = DatePickerControl.GetMaskedString(Model.DeathDate);
                check("DeathDate", (DatePickerControl.ParseText(value) == null) ? "Error" : null);
                return value;
            }
            set
            {
                edited = true;
                _DeathDate = value;
                DateTime? dt = DatePickerControl.ParseText(value);
                Model.DeathDate = dt;
            }
        }
        string _DeathDate = null;
        public string WillDate
        {
            get
            {
                string value;
                if (_WillDate != null)
                    value = _WillDate;
                else
                    value = DatePickerControl.GetMaskedString(Model.WillDate);
                check("WillDate", null);
                return value;
            }
            set
            {
                edited = true;
                _WillDate = value;
                DateTime? dt = DatePickerControl.ParseText(value);
                Model.WillDate = dt;
            }
        }
        string _WillDate = null;
        public string ReProperty
        {
            get
            {
                string value = Model.ReProperty;
                check("ReProperty", string.IsNullOrEmpty(value) || !Regex.IsMatch(value, @"^\s*[yn]\s*$", RegexOptions.IgnoreCase) ? "Error" : null);
                return value;
            }
            set
            {
                edited = true;
                Model.ReProperty = value;
            }
        }
        public string ReValue
        {
            get
            {
                check("ReValue", null);
                return Model.ReValue;
            }
            set
            {
                edited = true;
                Model.ReValue = value;
            }
        }
        public string Testate
        {
            get
            {
                string value = Model.Testate;
                check("Testate", string.IsNullOrEmpty(value) || !Regex.IsMatch(value, @"^\s*[yn]\s*$", RegexOptions.IgnoreCase) ? "Error" : null);
                return value;
            }
            set
            {
                edited = true;
                Model.Testate = value;
            }
        }
        public string PersonalValue
        {
            get
            {
                check("PersonalValue", null);
                return Model.PersonalValue;
            }
            set
            {
                edited = true;
                Model.PersonalValue = value;
            }
        }

        public string H_L_Name_0
        {
            get
            {
                check("H_L_Name_0", null);
                return Model.H_L_Name_0;
            }
            set
            {
                edited = true;
                Model.H_L_Name_0 = value;
            }
        }
        public Db.Probate.HeirOrOthers HeirOrOther_0
        {
            get
            {
                check("HeirOrOther_0", null);
                return Model.HeirOrOther_0;
            }
            set
            {
                edited = true;
                Model.HeirOrOther_0 = value;
            }
        }
        public string H_L_Address_0
        {
            get
            {
                check("H_L_Address_0", null);
                return Model.H_L_Address_0;
            }
            set
            {
                edited = true;
                Model.H_L_Address_0 = value;
            }
        }

        public string H_L_Name_1
        {
            get
            {
                check("H_L_Name_1", null);
                return Model.H_L_Name_1;
            }
            set
            {
                edited = true;
                Model.H_L_Name_1 = value;
            }
        }
        public Db.Probate.HeirOrOthers HeirOrOther_1
        {
            get
            {
                check("HeirOrOther_1", null);
                return Model.HeirOrOther_1;
            }
            set
            {
                edited = true;
                Model.HeirOrOther_1 = value;
            }
        }
        public string H_L_Address_1
        {
            get
            {
                check("H_L_Address_1", null);
                return Model.H_L_Address_1;
            }
            set
            {
                edited = true;
                Model.H_L_Address_1 = value;
            }
        }

        public string H_L_Name_2
        {
            get
            {
                check("H_L_Name_2", null);
                return Model.H_L_Name_2;
            }
            set
            {
                edited = true;
                Model.H_L_Name_2 = value;
            }
        }
        public Db.Probate.HeirOrOthers HeirOrOther_2
        {
            get
            {
                check("HeirOrOther_2", null);
                return Model.HeirOrOther_2;
            }
            set
            {
                edited = true;
                Model.HeirOrOther_2 = value;
            }
        }
        public string H_L_Address_2
        {
            get
            {
                check("H_L_Address_2", null);
                return Model.H_L_Address_2;
            }
            set
            {
                edited = true;
                Model.H_L_Address_2 = value;
            }
        }

        public string H_L_Name_3
        {
            get
            {
                check("H_L_Name_3", null);
                return Model.H_L_Name_3;
            }
            set
            {
                edited = true;
                Model.H_L_Name_3 = value;
            }
        }
        public Db.Probate.HeirOrOthers HeirOrOther_3
        {
            get
            {
                check("HeirOrOther_3", null);
                return Model.HeirOrOther_3;
            }
            set
            {
                edited = true;
                Model.HeirOrOther_3 = value;
            }
        }
        public string H_L_Address_3
        {
            get
            {
                check("H_L_Address_3", null);
                return Model.H_L_Address_3;
            }
            set
            {
                edited = true;
                Model.H_L_Address_3 = value;
            }
        }

        public string H_L_Name_4
        {
            get
            {
                check("H_L_Name_4", null);
                return Model.H_L_Name_4;
            }
            set
            {
                edited = true;
                Model.H_L_Name_4 = value;
            }
        }
        public Db.Probate.HeirOrOthers HeirOrOther_4
        {
            get
            {
                check("HeirOrOther_4", null);
                return Model.HeirOrOther_4;
            }
            set
            {
                edited = true;
                Model.HeirOrOther_4 = value;
            }
        }
        public string H_L_Address_4
        {
            get
            {
                check("H_L_Address_4", null);
                return Model.H_L_Address_4;
            }
            set
            {
                edited = true;
                Model.H_L_Address_4 = value;
            }
        }

        public string H_L_Name_5
        {
            get
            {
                check("H_L_Name_5", null);
                return Model.H_L_Name_5;
            }
            set
            {
                edited = true;
                Model.H_L_Name_5 = value;
            }
        }
        public Db.Probate.HeirOrOthers HeirOrOther_5
        {
            get
            {
                check("HeirOrOther_5", null);
                return Model.HeirOrOther_5;
            }
            set
            {
                edited = true;
                Model.HeirOrOther_5 = value;
            }
        }
        public string H_L_Address_5
        {
            get
            {
                check("H_L_Address_5", null);
                return Model.H_L_Address_5;
            }
            set
            {
                edited = true;
                Model.H_L_Address_5 = value;
            }
        }
    }
}