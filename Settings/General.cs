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
            public System.Windows.Input.Key TicketKey = System.Windows.Input.Key.F8;
            public System.Windows.Input.ModifierKeys TicketModifierKey1 = System.Windows.Input.ModifierKeys.None;
            public System.Windows.Input.ModifierKeys TicketModifierKey2 = System.Windows.Input.ModifierKeys.None;
            public int InfoToastLifeTimeInSecs = 5;
            public string InfoSoundFile = "inform.wav";
            public int InfoToastBottom = 100;
            public int InfoToastRight = 0;
            public int DbRefreshPeriodInSecs = 60 * 60 * 24;
            public int DbRefreshRetryPeriodInSecs = 60 * 60;
            public DateTime NextDbRefreshTime = DateTime.MinValue;
            public string UserName = "";
            public string EncryptedPassword = "";
            public string County = null;

            //[Newtonsoft.Json.JsonIgnore]
            //public System.Text.Encoding Encoding = System.Text.Encoding.Unicode;

            public override void Loaded()
            {
            }

            public override void Saving()
            {
            }

            public string Decrypt(string s)
            {
                try
                {
                    return c.Decrypt(s);
                }
                catch(Exception e)
                {
                    Message.Error("Could not decrypt string: " + Log.GetExceptionMessage(e));
                }
                return null;
            }
            public string Encrypt(string s)
            {
                return c.Encrypt(s);
            }
            CryptoRijndael c = new CryptoRijndael("poiuytrewq");
        }
    }
}