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
            public string Case_Number { get; set; }
            [ObligatoryField]
            public DateTime? Filling_Date { get; set; }
            public string Deceased_Full_Name { get; set; }
            public string Administrator_Full_Name { get; set; }
            public string Deceased_Street_Logo { get; set; }
            public string Administrator_Street_Logo { get; set; }
            public string Administrator_Address { get; set; }
            public string Administrator_Phone { get; set; }
            public string Attorney { get; set; }
            public string Attorney_Phone { get; set; }
            public string Deceased_Address { get; set; }
            public string Comments { get; set; }
            public string Deceased_City { get; set; }
            public string Administrator_City { get; set; }
            public string Filling_County { get; set; }
            public string Deceased_County { get; set; }
            public string Administrator_State { get; set; }
            public string Filling_State { get; set; }
            public string Administrator_Zip { get; set; }
            public string Deceased_Zip { get; set; }
            [ObligatoryField]
            public DateTime? Death_Date { get; set; }

            public enum YNs
            {
                Y,
                N
            }
            [ObligatoryField]
            public YNs Re_Property { get; set; }
            [ObligatoryField]
            public YNs Testate { get; set; }
            public DateTime? Will_Date { get; set; }
            public string Re_Value { get; set; }
            public string Personal_Value { get; set; }

            public enum Heir_Or_Others
            {
                H,
                H_L
            }
            public string H_L_Name_0 { get; set; }
            public Heir_Or_Others Heir_Or_Other_0 { get; set; }
            public string H_L_Address_0 { get; set; }
            public string H_L_Name_1 { get; set; }
            public Heir_Or_Others Heir_Or_Other_1 { get; set; }
            public string H_L_Address_1 { get; set; }
            public string H_L_Name_2 { get; set; }
            public Heir_Or_Others Heir_Or_Other_2 { get; set; }
            public string H_L_Address_2 { get; set; }
            public string H_L_Name_3 { get; set; }
            public Heir_Or_Others Heir_Or_Other_3 { get; set; }
            public string H_L_Address_3 { get; set; }
            public string H_L_Name_4 { get; set; }
            public Heir_Or_Others Heir_Or_Other_4 { get; set; }
            public string H_L_Address_4 { get; set; }
            public string H_L_Name_5 { get; set; }
            public Heir_Or_Others Heir_Or_Other_5 { get; set; }
            public string H_L_Address_5 { get; set; }
        }
    }
}