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
        public static readonly DatabaseSettings Database;

        public class DatabaseSettings : Cliver.Settings
        {
            public int RefreshPeriodInSecs = 60 * 60 * 24;
            public int RefreshRetryPeriodInSecs = 60 * 60;
            public DateTime NextRefreshTime = DateTime.MinValue;
            public DateTime? LastRefreshTime = null;
            public Dictionary<string, int> TableNames2MinCount = new Dictionary<string, int> { { string.Empty, 11 }, { typeof(Db.Zips).Name, 4 } };
            
            public int GetMinCountFor(Type type)
            {                
                int min_count;
                if (!TableNames2MinCount.TryGetValue(type.Name, out min_count))
                    if (!TableNames2MinCount.TryGetValue(string.Empty, out min_count))
                        min_count = -1;
                return min_count;
            }            
        }
    }
}