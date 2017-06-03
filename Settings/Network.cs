/********************************************************************************************
        Author: Sergey Stoyan
        sergey.stoyan@gmail.com
        http://www.cliversoft.com
********************************************************************************************/
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
        public static readonly NetworkSettings Network;

        public class NetworkSettings : Cliver.Settings
        {
            public string UserName = "protest";
            public string EncryptedPassword = "";//"qpwoei";
            public string ExportUrl = "https://dev.probidder.com/api/upload-cases/index.php";

            public string Password()
            {
                try
                {
                    return c.Decrypt(EncryptedPassword);
                }
                catch (Exception e)
                {
                    Message.Error("Could not decrypt string: " + Log.GetExceptionMessage(e));
                }
                return null;
            }

            public string Decrypt(string s)
            {
                try
                {
                    return c.Decrypt(s);
                }
                catch (Exception e)
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