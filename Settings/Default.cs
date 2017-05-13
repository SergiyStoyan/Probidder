using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Script.Serialization;

namespace Cliver.Foreclosures
{
    public partial class Settings
    {
        [Cliver.Settings.Obligatory]
        public static readonly DefaultSettings Default;

        public class DefaultSettings : Cliver.Settings
        {
            public string[] Counties = new string[] { "Cook", "Dupage", "Kane", "Kendall", "Lake", "Will", "McHenry" };
            //public string County = null;
            //public string City = null;
            //public string ZipCode = null;
            //public string Plaintiff = null;
            //public string Attorney = null;
            //public string AttorneyPhone = null;
            //public string MortgageType = null;
        }
    }
}