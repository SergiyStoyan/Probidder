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
        public static readonly ViewSettings View;

        public class ViewSettings : Cliver.Settings
        {
            public int InfoToastLifeTimeInSecs = 5;
            public string InfoSoundFile = "inform.wav";
            public int InfoToastBottom = 100;
            public int InfoToastRight = 0;
            public System.Windows.Media.Brush SearchHighlightColor = System.Windows.Media.Brushes.Yellow;
            public HashSet<string> ShowedColumns = new HashSet<string> { "Id", "FILING_DATE", "CITY", "ZIP", "PIN", "ADDRESS" };

            public override void Loaded()
            {
            }

            public override void Saving()
            {
            }            
        }
    }
}