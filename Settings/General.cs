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
        public static readonly GeneralSettings General;

        public class GeneralSettings : Cliver.Settings
        {
            public int InfoToastLifeTimeInSecs = 5;
            public string InfoSoundFile = "inform.wav";
            public int InfoToastBottom = 100;
            public int InfoToastRight = 0;
            public System.Windows.Media.Brush SearchHighlightColor = System.Windows.Media.Brushes.DarkOrange;

            public override void Loaded()
            {
            }

            public override void Saving()
            {
            }            
        }
    }
}