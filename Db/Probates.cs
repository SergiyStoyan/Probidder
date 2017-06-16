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

        public class Probate: Document
        {
            [ObligatoryField]
            public string CaseNumber { get; set; }
            [ObligatoryField]
            public DateTime? FillingDate { get; set; }
            public string DeceasedFirst { get; set; }
            public string DeceasedMiddle { get; set; }
            public string DeceasedLast { get; set; }
            public string AdministratorFirst { get; set; }
            public string AdministratorMiddle { get; set; }
            public string AdministratorLast { get; set; }
            public string DeceasedPlaceLogo { get; set; }
            public string AdministratorStreetLogo { get; set; }
            public string DeceasedStreetNumber { get; set; }
            public string AdministratorStreetNumber { get; set; }
            public string DeceasedStreetDirect { get; set; }
            public string Attorney { get; set; }
            public string AttorneyPhone { get; set; }
            public string AdministratorStreetDirect { get; set; }
            public string HeirsOrOthers { get; set; }
            public string DeceasedStreetName { get; set; }
            public string AdministratorStreetName { get; set; }
            public string DeceasedUnitNumber { get; set; }
            public string AdministratorUnitNumber { get; set; }
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
        }
    }
}