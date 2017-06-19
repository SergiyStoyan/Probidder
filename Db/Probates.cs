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
    public partial class Db
    {
        public class Probates : Db.LiteDb.Table<Probate>
        {
            public override void Save(Probate document)
            {
                foreach (PropertyInfo pi in typeof(Probate).GetProperties(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance))
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

        public class Probate : Document
        {
            [ObligatoryField]
            public string CaseNumber { get; set; }
            [ObligatoryField]
            public DateTime? FillingDate { get; set; }
            public string DeceasedFullName { get; set; }
            public string AdministratorFullName { get; set; }
            public string DeceasedStreetLogo { get; set; }
            public string AdministratorStreetLogo { get; set; }
            public string AdministratorAddress { get; set; }
            public string Attorney { get; set; }
            public string AttorneyPhone { get; set; }
            public string DeceasedAddress { get; set; }
            public string Comments { get; set; }
            public string DeceasedCity { get; set; }
            public string AdministratorCity { get; set; }
            public string FillingCounty { get; set; }
            public string DeceasedCounty { get; set; }
            public string AdministratorState { get; set; }
            public string FillingState { get; set; }
            public string AdministratorZip { get; set; }
            public string DeceasedZip { get; set; }
            [ObligatoryField]
            public DateTime? DeathDate { get; set; }
            public string ReProperty { get; set; }
            public DateTime? WillDate { get; set; }
            public string ReValue { get; set; }
            [ObligatoryField]
            public string Testate { get; set; }
            public string PersonalValue { get; set; }

            public enum HeirOrOthers
            {
                H,
                H_L
            }
            public string H_L_Name_0 { get; set; }
            public HeirOrOthers HeirOrOther_0 { get; set; }
            public string H_L_Address_0 { get; set; }
            public string H_L_Name_1 { get; set; }
            public HeirOrOthers HeirOrOther_1 { get; set; }
            public string H_L_Address_1 { get; set; }
            public string H_L_Name_2 { get; set; }
            public HeirOrOthers HeirOrOther_2 { get; set; }
            public string H_L_Address_2 { get; set; }
            public string H_L_Name_3 { get; set; }
            public HeirOrOthers HeirOrOther_3 { get; set; }
            public string H_L_Address_3 { get; set; }
            public string H_L_Name_4 { get; set; }
            public HeirOrOthers HeirOrOther_4 { get; set; }
            public string H_L_Address_4 { get; set; }
            public string H_L_Name_5 { get; set; }
            public HeirOrOthers HeirOrOther_5 { get; set; }
            public string H_L_Address_5 { get; set; }
        }
    }
}