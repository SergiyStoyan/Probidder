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
            Model.Filling_County = Settings.Location.County;
            Model.Deceased_County = Settings.Location.County;
            Model.Filling_State = "IL";
            Model.Case_Number = CASE_Ns?.FirstOrDefault();
        }
        
        public IEnumerable<string> CASE_Ns
        {
            get
            {
                if (string.IsNullOrEmpty(Filling_County))
                    return null;
                return (new Db.ProbateCaseNumbers()).GetBy(Filling_County).case_ns.OrderBy(x => x);
            }
        }
        //public IEnumerable<string> DeceasedCitys
        //{
        //    get
        //    {
        //        return (new Db.Cities()).GetBy(Deceased_County).OrderBy(x => x.city).Select(x => x.city);
        //    }
        //}
        public IEnumerable<string> CITYs
        {
            get
            {
                return (new Db.Cities()).GetBy(Filling_County).OrderBy(x => x.city).Select(x => x.city);
            }
        }
        public IEnumerable<string> DeceasedZips
        {
            get
            {
                if (string.IsNullOrEmpty(Deceased_County))
                    return null;
                if (string.IsNullOrEmpty(Deceased_City))
                    return null;
                return (new Db.Zips()).GetBy(Deceased_County, Deceased_City).OrderBy(x => x.zip).Select(x => x.zip);
            }
        }
        public IEnumerable<string> AdministratorZips
        {
            get
            {
                if (string.IsNullOrEmpty(Filling_County))
                    return null;
                if (string.IsNullOrEmpty(Administrator_City))
                    return null;
                return (new Db.Zips()).GetBy(Filling_County, Administrator_City).OrderBy(x => x.zip).Select(x => x.zip);
            }
        }
        public IEnumerable<string> Attorneys
        {
            get
            {
                if (string.IsNullOrEmpty(Filling_County))
                    return null;
                return (new Db.Attorneys()).GetBy(Filling_County).OrderBy(x => x.attorney).Select(x => x.attorney);
            }
        }
        public IEnumerable<string> AttorneyPhones
        {
            get
            {
                if (string.IsNullOrEmpty(Filling_County))
                    return null;
                if (string.IsNullOrEmpty(Attorney))
                    return null;
                return ComboBoxPhoneControl.GetItemsNormalized((new Db.AttorneyPhones()).GetBy(Filling_County, Attorney).OrderBy(x => x.attorney_phone).Select(x => x.attorney_phone));
            }
        }
        public IEnumerable<string> YNs
        {
            get
            {
                return Enum.GetNames(typeof(Db.Probate.YNs));
            }
        }

        public string Filling_County
        {
            get
            {
                string value = Model.Filling_County;
                check("Filling_County", string.IsNullOrEmpty(value) ? "Error" : null);
                return value;
            }
            set
            {
                edited = true;
                Model.Filling_County = value;
                OnPropertyChanged("CASE_Ns");
                OnPropertyChanged("CITYs");
                OnPropertyChanged("DeceasedZips");
                OnPropertyChanged("AdministratorZips");
                OnPropertyChanged("Attorneys");
                OnPropertyChanged("AttorneyPhones");
            }
        }
        public string Case_Number
        {
            get
            {
                string value = Model.Case_Number;
                check("Case_Number", value != null && (Regex.IsMatch(value, @"[^\w\-_]") || value.Length > 16) ? "Error" : null);
                return value;
            }
            set
            {
                edited = true;
                Model.Case_Number = value;
            }
        }
        public string Filling_Date
        {
            get
            {
                string value;
                //if (_FillingDate != null)
                //    value = _FillingDate;
                //else
                    value = DatePickerControl.GetMaskedString(Model.Filling_Date);
                check("Filling_Date", (DatePickerControl.ParseText(value) == null) ? "Error" : null);
                return value;
            }
            set
            {
                edited = true;
                //_FillingDate = value;
                DateTime? dt = DatePickerControl.ParseText(value);
                Model.Filling_Date = dt;
            }
        }
        //string _FillingDate = null;
        public string Deceased_Full_Name
        {
            get
            {
                check("Deceased_Full_Name", null);
                return Model.Deceased_Full_Name;
            }
            set
            {
                edited = true;
                Model.Deceased_Full_Name = value;
            }
        }
        public string Administrator_Full_Name
        {
            get
            {
                check("Administrator_Full_Name", null);
                return Model.Administrator_Full_Name;
            }
            set
            {
                edited = true;
                Model.Administrator_Full_Name = value;
            }
        }
        public string Administrator_Phone
        {
            get
            {
                string value = Model.Administrator_Phone;
                check("Administrator_Phone", !string.IsNullOrEmpty(value) && (Regex.IsMatch(value, @"_") && Regex.IsMatch(value, @"\d")) ? "Error" : null);
                return value;
            }
            set
            {
                edited = true;
                Model.Administrator_Phone = value;
            }
        }
        public string Deceased_Street_Logo
        {
            get
            {
                check("Deceased_Street_Logo", null);
                return Model.Deceased_Street_Logo;
            }
            set
            {
                edited = true;
                Model.Deceased_Street_Logo = value;
            }
        }
        public string Administrator_Street_Logo
        {
            get
            {
                check("Administrator_Street_Logo", null);
                return Model.Administrator_Street_Logo;
            }
            set
            {
                edited = true;
                Model.Administrator_Street_Logo = value;
            }
        }
        public string Administrator_Address
        {
            get
            {
                check("Administrator_Address", null);
                return Model.Administrator_Address;
            }
            set
            {
                edited = true;
                Model.Administrator_Address = value;
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
                OnPropertyChanged("AttorneyPhones");
            }
        }
        public string Attorney_Phone
        {
            get
            {
                string value = Model.Attorney_Phone;
                check("Attorney_Phone", !string.IsNullOrEmpty(value) && (Regex.IsMatch(value, @"_") && Regex.IsMatch(value, @"\d")) ? "Error" : null);
                return value;
            }
            set
            {
                edited = true;
                Model.Attorney_Phone = value;
            }
        }
        public string Deceased_Address
        {
            get
            {
                check("Deceased_Address", null);
                return Model.Deceased_Address;
            }
            set
            {
                edited = true;
                Model.Deceased_Address = value;
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
        public string Deceased_City
        {
            get
            {
                check("Deceased_City", null);
                return Model.Deceased_City;
            }
            set
            {
                edited = true;
                Model.Deceased_City = value;
                OnPropertyChanged("DeceasedZips");
            }
        }
        public string Administrator_City
        {
            get
            {
                check("Administrator_City", null);
                return Model.Administrator_City;
            }
            set
            {
                edited = true;
                Model.Administrator_City = value;
                OnPropertyChanged("AdministratorZips");
            }
        }
        public string Deceased_County
        {
            get
            {
                check("Deceased_County", null);
                return Model.Deceased_County;
            }
            set
            {
                edited = true;
                Model.Deceased_County = value;
            }
        }
        public string Administrator_State
        {
            get
            {
                check("Administrator_State", null);
                return Model.Administrator_State;
            }
            set
            {
                edited = true;
                Model.Administrator_State = value;
            }
        }
        public string Filling_State
        {
            get
            {
                string value = Model.Filling_State;
                check("Filling_State", string.IsNullOrEmpty(value) ? "Error" : null);
                return value;
            }
            set
            {
                edited = true;
                Model.Filling_State = value;
            }
        }
        public string Administrator_Zip
        {
            get
            {
                string value = Model.Administrator_Zip;
                check("Administrator_Zip", !string.IsNullOrEmpty(value) && (Regex.IsMatch(value, @"[^\d]") || value.Length > 5 || value.Length < 4) ? "Error" : null);
                return value;
            }
            set
            {
                edited = true;
                Model.Administrator_Zip = value;
            }
        }
        public string Deceased_Zip
        {
            get
            {
                string value = Model.Deceased_Zip;
                check("Deceased_Zip", !string.IsNullOrEmpty(value) && (Regex.IsMatch(value, @"[^\d]") || value.Length > 5 || value.Length < 4) ? "Error" : null);
                return value;
            }
            set
            {
                edited = true;
                Model.Deceased_Zip = value;
            }
        }
        public string Death_Date
        {
            get
            {
                string value;
                //if (_DeathDate != null)
                //    value = _DeathDate;
                //else
                    value = DatePickerControl.GetMaskedString(Model.Death_Date);
                check("Death_Date", (DatePickerControl.ParseText(value) == null) ? "Error" : null);
                return value;
            }
            set
            {
                edited = true;
                //_DeathDate = value;
                DateTime? dt = DatePickerControl.ParseText(value);
                Model.Death_Date = dt;
            }
        }
        //string _DeathDate = null;
        public string Will_Date
        {
            get
            {
                string value;
                //if (_WillDate != null)
                //    value = _WillDate;
                //else
                    value = DatePickerControl.GetMaskedString(Model.Will_Date);
                check("Will_Date", null);
                return value;
            }
            set
            {
                edited = true;
                //_WillDate = value;
                DateTime? dt = DatePickerControl.ParseText(value);
                Model.Will_Date = dt;
            }
        }
        //string _WillDate = null;
        public Db.Probate.YNs Re_Property
        {
            get
            {
                Db.Probate.YNs value = Model.Re_Property;
                //check("Re_Property", string.IsNullOrEmpty(value) || !Regex.IsMatch(value, @"^\s*[yn]\s*$", RegexOptions.IgnoreCase) ? "Error" : null);
                check("Re_Property", null);
                return value;
            }
            set
            {
                edited = true;
                Model.Re_Property = value;
            }
        }
        public string Re_Value
        {
            get
            {
                check("Re_Value", null);
                return Model.Re_Value;
            }
            set
            {
                edited = true;
                Model.Re_Value = value;
            }
        }
        public Db.Probate.YNs Testate
        {
            get
            {
                Db.Probate.YNs value = Model.Testate;
                //check("Testate", string.IsNullOrEmpty(value) || !Regex.IsMatch(value, @"^\s*[yn]\s*$", RegexOptions.IgnoreCase) ? "Error" : null);
                check("Testate", null);
                return value;
            }
            set
            {
                edited = true;
                Model.Testate = value;
            }
        }
        public string Personal_Value
        {
            get
            {
                check("Personal_Value", null);
                return Model.Personal_Value;
            }
            set
            {
                edited = true;
                Model.Personal_Value = value;
            }
        }
        public string Heirs_Or_Legatees
        {
            get
            {
                check("Heirs_Or_Legatees", null);
                return Model.Heirs_Or_Legatees;
            }
            set
            {
                edited = true;
                Model.Heirs_Or_Legatees = value;
            }
        }

        public string Heir_Name_0
        {
            get
            {
                check("Heir_Name_0", null);
                return Model.Heir_Name_0;
            }
            set
            {
                edited = true;
                Model.Heir_Name_0 = value;
            }
        }
        public string Heir_Address_0
        {
            get
            {
                check("Heir_Address_0", null);
                return Model.Heir_Address_0;
            }
            set
            {
                edited = true;
                Model.Heir_Address_0 = value;
            }
        }

        public string Heir_Name_1
        {
            get
            {
                check("Heir_Name_1", null);
                return Model.Heir_Name_1;
            }
            set
            {
                edited = true;
                Model.Heir_Name_1 = value;
            }
        }
        public string Heir_Address_1
        {
            get
            {
                check("Heir_Address_1", null);
                return Model.Heir_Address_1;
            }
            set
            {
                edited = true;
                Model.Heir_Address_1 = value;
            }
        }

        public string Heir_Name_2
        {
            get
            {
                check("Heir_Name_2", null);
                return Model.Heir_Name_2;
            }
            set
            {
                edited = true;
                Model.Heir_Name_2 = value;
            }
        }
        public string Heir_Address_2
        {
            get
            {
                check("Heir_Address_2", null);
                return Model.Heir_Address_2;
            }
            set
            {
                edited = true;
                Model.Heir_Address_2 = value;
            }
        }

        public string Heir_Name_3
        {
            get
            {
                check("Heir_Name_3", null);
                return Model.Heir_Name_3;
            }
            set
            {
                edited = true;
                Model.Heir_Name_3 = value;
            }
        }
        public string Heir_Address_3
        {
            get
            {
                check("Heir_Address_3", null);
                return Model.Heir_Address_3;
            }
            set
            {
                edited = true;
                Model.Heir_Address_3 = value;
            }
        }
    }
}