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
            public string[] Subjects = new string[] { "test", "test2" };
            public System.Windows.Input.Key TicketKey = System.Windows.Input.Key.F8;
            public System.Windows.Input.ModifierKeys TicketModifierKey1 = System.Windows.Input.ModifierKeys.None;
            public System.Windows.Input.ModifierKeys TicketModifierKey2 = System.Windows.Input.ModifierKeys.None;
            public int InfoToastLifeTimeInSecs = 5;
            public string InfoSoundFile = "alert.wav";
            public int InfoToastBottom = 100;
            public int InfoToastRight = 0;

            //[Newtonsoft.Json.JsonIgnore]
            //public System.Text.Encoding Encoding = System.Text.Encoding.Unicode;

            public override void Loaded()
            {
            }

            //public override void Saving()
            //{
            //    UserEmail = string.IsNullOrWhiteSpace(UserEmail) ? null : UserEmail.Trim();
            //}
        }
    }
}