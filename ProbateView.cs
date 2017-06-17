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
        public string DeceasedFirst
        {
            get
            {
                check("DeceasedFirst", null);
                return Model.DeceasedFirst;
            }
            set
            {
                edited = true;
                Model.DeceasedFirst = value;
            }
        }
        public string DeceasedMiddle
        {
            get
            {
                check("DeceasedMiddle", null);
                return Model.DeceasedMiddle;
            }
            set
            {
                edited = true;
                Model.DeceasedMiddle = value;
            }
        }
        public string DeceasedLast
        {
            get
            {
                check("DeceasedLast", null);
                return Model.DeceasedLast;
            }
            set
            {
                edited = true;
                Model.DeceasedLast = value;
            }
        }
        public string AdministratorFirst
        {
            get
            {
                check("AdministratorFirst", null);
                return Model.AdministratorFirst;
            }
            set
            {
                edited = true;
                Model.AdministratorFirst = value;
            }
        }
        public string AdministratorMiddle
        {
            get
            {
                check("AdministratorMiddle", null);
                return Model.AdministratorMiddle;
            }
            set
            {
                edited = true;
                Model.AdministratorMiddle = value;
            }
        }
        public string AdministratorLast
        {
            get
            {
                check("AdministratorLast", null);
                return Model.AdministratorLast;
            }
            set
            {
                edited = true;
                Model.AdministratorLast = value;
            }
        }
        public string DeceasedPlaceLogo
        {
            get
            {
                check("DeceasedPlaceLogo", null);
                return Model.DeceasedPlaceLogo;
            }
            set
            {
                edited = true;
                Model.DeceasedPlaceLogo = value;
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
        public string DeceasedStreetNumber
        {
            get
            {
                check("DeceasedStreetNumber", null);
                return Model.DeceasedStreetNumber;
            }
            set
            {
                edited = true;
                Model.DeceasedStreetNumber = value;
            }
        }
        public string AdministratorStreetNumber
        {
            get
            {
                check("AdministratorStreetNumber", null);
                return Model.AdministratorStreetNumber;
            }
            set
            {
                edited = true;
                Model.AdministratorStreetNumber = value;
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
        public string AdministratorStreetDirect
        {
            get
            {
                check("AdministratorStreetDirect", null);
                return Model.AdministratorStreetDirect;
            }
            set
            {
                edited = true;
                Model.AdministratorStreetDirect = value;
            }
        }
        public string HeirsOrOthers
        {
            get
            {
                check("HeirsOrOthers", null);
                return Model.HeirsOrOthers;
            }
            set
            {
                edited = true;
                Model.HeirsOrOthers = value;
            }
        }
        public string DeceasedStreetName
        {
            get
            {
                check("DeceasedStreetName", null);
                return Model.DeceasedStreetName;
            }
            set
            {
                edited = true;
                Model.DeceasedStreetName = value;
            }
        }
        public string AdministratorStreetName
        {
            get
            {
                check("AdministratorStreetName", null);
                return Model.AdministratorStreetName;
            }
            set
            {
                edited = true;
                Model.AdministratorStreetName = value;
            }
        }
        public string DeceasedUnitNumber
        {
            get
            {
                check("DeceasedUnitNumber", null);
                return Model.DeceasedUnitNumber;
            }
            set
            {
                edited = true;
                Model.DeceasedUnitNumber = value;
            }
        }
        public string AdministratorUnitNumber
        {
            get
            {
                check("AdministratorUnitNumber", null);
                return Model.AdministratorUnitNumber;
            }
            set
            {
                edited = true;
                Model.AdministratorUnitNumber = value;
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
                check("FillingState", null);
                return Model.FillingState;
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
                check("WillDate", (DatePickerControl.ParseText(value) == null) ? "Error" : null);
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
                check("ReProperty", null);
                return Model.ReProperty;
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
                check("Testate", null);
                return Model.Testate;
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
    }
}