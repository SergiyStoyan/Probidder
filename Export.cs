//********************************************************************************************
//Author: Sergey Stoyan, CliverSoft.com
//        http://cliversoft.com
//        stoyan@cliversoft.com
//        sergey.stoyan@gmail.com
//        27 February 2007
//Copyright: (C) 2007, Sergey Stoyan
//********************************************************************************************

using System;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Diagnostics;
using Cliver;
using System.Configuration;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Windows.Input;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using Jose;

namespace Cliver.Foreclosures
{
    public class Export
    {
        public static bool ToServer()
        {
            Log.Main.Inform("Exporting to: " + Settings.Login.ExportUrl);

            try
            {
                HttpClient http_client = new HttpClient();
                if (!loginByUsername(ref http_client, Settings.Login.UserName, Settings.Login.Password()))
                    return false;

                Db.Foreclosures fs = new Db.Foreclosures();
                string s = SerializationRoutines.Json.Serialize(fs.GetAll());
                StringContent post_data = new StringContent(s);
                post_data.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");
                HttpResponseMessage rm = http_client.PostAsync(Settings.Login.ExportUrl, post_data).Result;
                if (!rm.IsSuccessStatusCode)
                    throw new Exception(rm.ReasonPhrase);

                if (Message.YesNo("Data has been uploaded succesfully to " + Settings.Login.ExportUrl + "\r\n\r\nClean up the database?"))
                {
                    fs.Drop();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Message.Error("Could not upload: " + ex.Message);
                Log.Error("Could not upload: " + Log.GetExceptionMessage(ex));
            }
            return false;

        }
        
      static  public string getOAuthTAccessToken(ref HttpClient http_client)
        {
            Dictionary<string, string> payload = new Dictionary<string, string>
            {
                { "iss", "win_recording_app" },
                { "sub", "WinRecordingApp" },
                { "aud","https://dev-auth.probidder.com" },
                { "exp" ,(DateTime.Now.GetSecondsSinceUnixEpoch() + 1000 ).ToString()},
                { "iat" , DateTime.Now.GetSecondsSinceUnixEpoch().ToString() }
            };
            byte[] privateKey = File.ReadAllBytes(Log.AppDir + "\\win_recording_app.key");
            //string jwt = JsonWebToken.Encode(post_object, privateKey, JwtHashAlgorithm.RS256);
            string jwt = JWT.Encode(payload, privateKey, JwsAlgorithm.RS512);

            FormUrlEncodedContent fuec = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer" },
                { "assertion",  jwt }
            });
            if (http_client == null)
                http_client = new HttpClient();
            HttpResponseMessage rm = http_client.PostAsync("https://dev-auth.probidder.com/api/oauth/token", fuec).Result;
            string c = rm.Content.ReadAsStringAsync().Result;
            if (!rm.IsSuccessStatusCode)
                throw new Exception("Could not get AuthTAccessToken: " + rm.ReasonPhrase);
            JObject jo = new JObject(rm.Content);
            return (string)jo["access_token"];
        }

        static public bool loginByUsername(ref HttpClient http_client, string username, string password)
        {
            FormUrlEncodedContent fuec = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "username", username },
                { "password",  password }
            });
            if (http_client == null)
                http_client = new HttpClient();
            http_client.DefaultRequestHeaders.Add("Bearer", getOAuthTAccessToken(ref http_client));
            HttpResponseMessage rm = http_client.PostAsync("https://dev-auth.probidder.com/api/authenticate/recorders/win/app?", fuec).Result;
            if (!rm.IsSuccessStatusCode)
                throw new Exception("Could not get login: " + rm.ReasonPhrase);
            JObject jo = new JObject(rm.Content);
            return true; 
        }

        public static bool ToDisk()
        {
            string file;
            using (var d = new System.Windows.Forms.FolderBrowserDialog())
            {
                d.Description = "Choose the folder where to save the exported file.";
                if (d.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return false;
                file = d.SelectedPath + "\\foreclosure_" + DateTime.Now.ToString("yy-MM-dd_HH-mm-ss") + ".csv";
            }

            try
            {
                TextWriter tw = new StreamWriter(file);
                tw.WriteLine(FieldPreparation.GetCsvHeaderLine(typeof(Db.Foreclosure), FieldPreparation.FieldSeparator.COMMA));
                Db.Foreclosures fs = new Db.Foreclosures();
                foreach (Db.Foreclosure f in fs.GetAll())
                    tw.WriteLine(FieldPreparation.GetCsvLine(f, FieldPreparation.FieldSeparator.COMMA));
                tw.Close();

                if (Message.YesNo("Data has been exported succesfully to " + file + "\r\n\r\nClean up the database?"))
                {
                    fs.Drop();
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogMessage.Error(ex);
            }
            return false;
        }
    }
}