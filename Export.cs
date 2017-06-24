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
using System.Threading.Tasks;
using System.Reflection;

namespace Cliver.Probidder
{
    public class Export
    {
        public static bool ToServerRuns
        {
            get
            {
                return (to_server_t != null && to_server_t.IsAlive);
            }
        }

        public delegate void OnToServerStateChanged();
        public static event OnToServerStateChanged ToServerStateChanged = null;

        public static Thread BeginToServer<D>(Db.LiteDb.Table<D> table, bool show_start_notification = true) where D : Db.Document, new()
        {
            lock (o)
            {
                if (to_server_t != null && to_server_t.IsAlive)
                    return to_server_t;

                to_server_t = ThreadRoutines.StartTry(() => { to_server<D>(table, show_start_notification); });
                return to_server_t;
            }
        }
        static Thread to_server_t = null;
        static readonly object o = new object();

        static void to_server<D>(Db.LiteDb.Table<D> table, bool show_start_notification = true) where D : Db.Document, new()
        {
            MessageForm mf = null;

            try
            {
                ToServerStateChanged?.BeginInvoke(null, null);

                if (show_start_notification)
                {
                    ThreadRoutines.StartTry(() =>
                    {
                        mf = new MessageForm(System.Windows.Forms.Application.ProductName, System.Drawing.SystemIcons.Information, "Uploading database to the server. It is preferable to wait until completion to avoid mixing data...", new string[1] { "OK" }, 0, null);
                        mf.ShowDialog();
                    });
                    if (SleepRoutines.WaitForObject(() => { return mf; }, 10000) == null)
                        Log.Main.Exit("SleepRoutines.WaitForObject got null");
                }

                string url;
                if (table is Db.Foreclosures)
                    url = Settings.Network.ExportUrl + "?mode=estate";
                else if (table is Db.Probates)
                    url = Settings.Network.ExportUrl + "?mode=probate";
                else
                    throw new Exception("Unknown option: " + table.GetType());
                Log.Main.Inform("Uploading " + table.GetType() + " to: " + url);

                List<object> records = new List<object>();
                foreach (D d in table.GetAll())
                {
                    Dictionary<string, object> fs2v = get_record(d);
                    fs2v["recorder"] = Settings.Network.UserName;
                    records.Add(fs2v);
                }

                HttpClient http_client = new HttpClient();
                //if (!loginByUsername(ref http_client, Settings.Network.UserName, Settings.Network.Password()))
                if (!loginByUsername(ref http_client, null, Settings.Network.UserName, Settings.Network.Password()))
                    throw new Exception("Could not login with Username: " + Settings.Network.UserName);

                string s = SerializationRoutines.Json.Serialize(records);
                StringContent post_data = new StringContent(s);
                post_data.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");
                HttpResponseMessage rm = http_client.PostAsync(url, post_data).Result;
                if (!rm.IsSuccessStatusCode)
                    throw new Exception(rm.StatusCode + "\r\n" + rm.ReasonPhrase);
                string r = rm.Content.ReadAsStringAsync().Result;
                JObject jo = SerializationRoutines.Json.Deserialize<JObject>(r);
                Log.Main.Inform("Inserted records: " + jo["records_inserted"].ToString());
                try
                {
                    mf?.Close();
                }
                catch { }
                if (jo["failed_records"].Count() > 0)
                {
                    Log.Main.Error2("Failed insert records: \r\n" + jo["failed_records"].ToString());
                    //InfoWindow.Create(ProgramRoutines.GetAppName(), "Some records could not be uploaded!\r\nSee log for more details.", null, "OK", null, System.Windows.Media.Brushes.WhiteSmoke, System.Windows.Media.Brushes.Red);
                    Message.Error("Some records could not be uploaded!\r\nSee log for more details.");
                }
                else
                {
                    Log.Main.Inform("Table " + table.GetType() + " has been uploaded successfully.");
                    //InfoWindow.Create(ProgramRoutines.GetAppName(), "Table " + table.GetType() + " has been uploaded successfully.", null, "OK", null, System.Windows.Media.Brushes.White, System.Windows.Media.Brushes.Green);
                    if (Message.YesNo("Table " + table.GetType() + " has been uploaded succesfully to the server.\r\n\r\nClean up the table?"))
                    {
                        Log.Inform("Dropping table " + table.GetType());
                        ListWindow.This.Views.Drop();
                    }
                }
            }
            catch (Exception e)
            {
                try
                {
                    mf?.Close();
                }
                catch { }
                Log.Main.Error("Could not upload data.", e);
                InfoWindow.Create(ProgramRoutines.GetAppName() + ": could not upload data! Check connection to the intenet.", Log.GetExceptionMessage(e), null, "OK", null, System.Windows.Media.Brushes.WhiteSmoke, System.Windows.Media.Brushes.Red);
            }
            finally
            {
                ThreadRoutines.StartTry(() =>
                {
                    if (to_server_t != null)
                        to_server_t.Join();
                    ToServerStateChanged?.BeginInvoke(null, null);
                });
            }
        }

        static public string getOAuthTAccessToken(ref HttpClient http_client)
        {
            Log.Main.Inform("Getting OAuthTAccessToken");

            byte[] privateKey = File.ReadAllBytes(Log.AppDir + "\\win_recording_app.key");
            var payload = new
            {
                iss = "win_recording_app",
                sub = "WinRecordingApp",
                aud = "https://dev-auth.probidder.com",
                exp = DateTime.Now.GetSecondsSinceUnixEpoch() + 1000,
                iat = DateTime.Now.GetSecondsSinceUnixEpoch(),
            };
            string jwt = JsonWebToken.Encode(payload, privateKey, JwtHashAlgorithm.RS512);
            //Log.Main.Warning(jwt);
            FormUrlEncodedContent fuec = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer" },
                { "assertion",  jwt }
            });
            if (http_client == null)
                http_client = new HttpClient();
            HttpResponseMessage rm = http_client.PostAsync("https://dev-auth.probidder.com/api/oauth/token", fuec).Result;
            if (!rm.IsSuccessStatusCode)
                throw new Exception("Could not get AuthTAccessToken: " + rm.StatusCode + "\r\n" + rm.ReasonPhrase);
            dynamic d = SerializationRoutines.Json.Deserialize<dynamic>(rm.Content.ReadAsStringAsync().Result);
            return (string)d["access_token"];
        }

        static public bool loginByUsername(ref HttpClient http_client, string access_token, string username, string password)
        {
            Log.Main.Inform("Loging in as " + username);

            FormUrlEncodedContent fuec = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "username", username },
                { "password",  password }
            });

            HttpRequestMessage hrm = new HttpRequestMessage(HttpMethod.Get, "https://dev-auth.probidder.com/api/authenticate/recorders/win/app?" + fuec.ReadAsStringAsync().Result);
            if (access_token != null)
                hrm.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", access_token);
            if (http_client == null)
                http_client = new HttpClient();
            HttpResponseMessage rm = http_client.SendAsync(hrm).Result;
            if (!rm.IsSuccessStatusCode)
                throw new Exception("Could not get login: " + rm.StatusCode + "\r\n" + rm.ReasonPhrase);
            dynamic d = SerializationRoutines.Json.Deserialize<dynamic>(rm.Content.ReadAsStringAsync().Result);
            return d["status"];
        }

        public static bool ToDisk<D>(Db.LiteDb.Table<D> table) where D : Db.Document, new()
        {
            string file;
            using (var d = new System.Windows.Forms.FolderBrowserDialog())
            {
                d.Description = "Choose the folder where to save the exported file.";
                if (d.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return false;
                file = d.SelectedPath + "\\" + table.GetType().Name + "_" + DateTime.Now.ToString("yy-MM-dd_HH-mm-ss") + ".csv";
            }

            try
            {
                TextWriter tw = new StreamWriter(file);
                bool header_printed = false;
                foreach (D d in table.GetAll())
                {
                    Dictionary<string, object> fs2v = get_record(d);
                    if(!header_printed)
                    {
                        header_printed = true;
                        tw.WriteLine(FieldPreparation.GetCsvHeaderLine(fs2v.Keys, FieldPreparation.FieldSeparator.COMMA));
                    }
                    tw.WriteLine(FieldPreparation.GetCsvLine(fs2v.Values, FieldPreparation.FieldSeparator.COMMA));
                }
                tw.Close();

                if (Message.YesNo("Table " + table.GetType() + " has been exported succesfully to " + file + "\r\n\r\nClean up the table?"))
                {
                    ListWindow.This.Views.Drop();
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogMessage.Error(ex);
            }
            return false;
        }

        static void normalize_date(Dictionary<string, object> d, string field)
        {
            if (d[field] != null)
                d[field] = ((DateTime)d[field]).ToString("yyyy-MM-dd");
        }

        static Dictionary<string, object> get_record<D>(D d) where D : Db.Document, new()
        {
            Dictionary<string, object> fs2v = typeof(D).GetProperties(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance).ToDictionary(x => x.Name, x => (x.GetValue(d)));

            if (typeof(D) == typeof(Db.Foreclosure))
            {
                normalize_date(fs2v, "FILING_DATE");
                normalize_date(fs2v, "ENTRY_DATE");
                normalize_date(fs2v, "DATE_OF_CA");
                normalize_date(fs2v, "ORIGINAL_MTG");
                normalize_date(fs2v, "LAST_PAY_DATE");
                //normalize_date(d, "AUCTION_DATE");
                //normalize_date(d, "AUCTION_TIME");

                if (fs2v["PIN"] != null)
                    fs2v["PIN"] = ((string)fs2v["PIN"]).Replace("____", "0000");
            }
            else if (typeof(D) == typeof(Db.Probate))
            {
                normalize_date(fs2v, "Filling_Date");
                normalize_date(fs2v, "Death_Date");
                normalize_date(fs2v, "Will_Date");

                if (fs2v["Re_Property"] != null)
                    fs2v["Re_Property"] = fs2v["Re_Property"].ToString();
                if (fs2v["Testate"] != null)
                    fs2v["Testate"] = fs2v["Testate"].ToString();
                if (fs2v["Heir_Or_Other_0"] != null)
                    fs2v["Heir_Or_Other_0"] = fs2v["Heir_Or_Other_0"].ToString();
                if (fs2v["Heir_Or_Other_1"] != null)
                    fs2v["Heir_Or_Other_1"] = fs2v["Heir_Or_Other_1"].ToString();
                if (fs2v["Heir_Or_Other_2"] != null)
                    fs2v["Heir_Or_Other_2"] = fs2v["Heir_Or_Other_2"].ToString();
                if (fs2v["Heir_Or_Other_3"] != null)
                    fs2v["Heir_Or_Other_3"] = fs2v["Heir_Or_Other_3"].ToString();
                if (fs2v["Heir_Or_Other_4"] != null)
                    fs2v["Heir_Or_Other_4"] = fs2v["Heir_Or_Other_4"].ToString();
                if (fs2v["Heir_Or_Other_5"] != null)
                    fs2v["Heir_Or_Other_5"] = fs2v["Heir_Or_Other_5"].ToString();
            }

            return fs2v;
        }
    }

    public enum JwtHashAlgorithm
    {
        RS256,
        RS512,
        HS384,
        HS512
    }

    public class JsonWebToken
    {
        private static Dictionary<JwtHashAlgorithm, Func<byte[], byte[], byte[]>> HashAlgorithms;

        static JsonWebToken()
        {
            HashAlgorithms = new Dictionary<JwtHashAlgorithm, Func<byte[], byte[], byte[]>>
            {
                { JwtHashAlgorithm.RS256, (key, value) => { using (var sha = new HMACSHA256(key)) { return sha.ComputeHash(value); } } },
                { JwtHashAlgorithm.RS512, (key, value) => {

                    //string k = Regex.Replace(Encoding.UTF8.GetString(  key), @"^\s*-----BEGIN PRIVATE KEY-----", "");
                    // k = Regex.Replace( k, @"-----END PRIVATE KEY-----\s*$", "").Trim();
                    //using (RSA rsa = RSA.Create())
                    //{
                    //    return rsa.SignHash(value, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
                    //}
                    //{
                    //RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                    //rsa.FromXmlString(key);
                    //SHA512Managed hash = new SHA512Managed();
                    //byte[] hashedData = hash.ComputeHash(value);
                    //byte[] signedData = RSA.SignHash( hashedData,                                    CryptoConfig.MapNameToOID("SHA512")                                  );
                    //}

                    //byte[] bs;
                    //using (SHA512Managed sha = new SHA512Managed())
                    //{sha.Initialize();
                    //    sha.
                    //   bs = sha.ComputeHash(value);
                    //}
                    
                    string key_file = ProgramRoutines.GetAppTempDirectory() + "\\key.bin";
                    string value_file = ProgramRoutines.GetAppTempDirectory() + "\\payload.bin";
                    string signed_file = ProgramRoutines.GetAppTempDirectory() + "\\payload.bin.signed";
                    File.WriteAllBytes(key_file, key);
                    File.WriteAllBytes(value_file, value);
                    string ssl_command = "dgst -sha512 -sign \"" + key_file + "\" -out \"" + signed_file + "\" \"" + value_file + "\"";
                    Log.Main.Inform("Launching openssl: " + ssl_command);
                    ProcessStartInfo psi = new ProcessStartInfo("openssl\\openssl.exe",ssl_command);
                    psi.RedirectStandardOutput = true;
                    psi.RedirectStandardError = true;
                    psi.UseShellExecute = false;
                    psi.CreateNoWindow = true;
                    Process p = new Process();
                    p.StartInfo = psi;
                    p.Start();
                    string output = "";
                    while (!p.StandardOutput.EndOfStream || !p.StandardError.EndOfStream)
                    {
                        output+= p.StandardOutput.ReadLine();
                        output+= p.StandardOutput.ReadLine();
                    }
                    Log.Main.Inform("openssl output: " + output);
                    p.WaitForExit();
                    byte[] bs = File.ReadAllBytes(signed_file);

                    //bool g = true;
                    //for(int i=0;i<bs.Length; i++)
                    //    if(bs[i]!=bs2[i])
                    //    {
                    //        g=false;
                    //        break;
                    //    }
                    return bs;
                } },
                { JwtHashAlgorithm.HS384, (key, value) => { using (var sha = new HMACSHA384(key)) { return sha.ComputeHash(value); } } },
                { JwtHashAlgorithm.HS512, (key, value) => { using (var sha = new HMACSHA512(key)) { return sha.ComputeHash(value); } } },
            };
        }

        public static string Encode(object payload, string key, JwtHashAlgorithm algorithm)
        {
            return Encode(payload, Encoding.UTF8.GetBytes(key), algorithm);
        }

        public static string Encode(object payload, byte[] keyBytes, JwtHashAlgorithm algorithm)
        {
            var segments = new List<string>();
            var header = new { alg = algorithm.ToString(), typ = "JWT" };

            byte[] headerBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(header, Formatting.None));
            byte[] payloadBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload, Formatting.None));

            segments.Add(Base64UrlEncode(headerBytes));
            segments.Add(Base64UrlEncode(payloadBytes));

            var stringToSign = string.Join(".", segments);
            var bytesToSign = Encoding.UTF8.GetBytes(stringToSign);

            byte[] signature = HashAlgorithms[algorithm](keyBytes, bytesToSign);
            segments.Add(Base64UrlEncode(signature));

            return string.Join(".", segments);
        }

        public static string Decode(string token, string key)
        {
            return Decode(token, key, true);
        }

        public static string Decode(string token, string key, bool verify)
        {
            var parts = token.Split('.');
            var header = parts[0];
            var payload = parts[1];
            byte[] crypto = Base64UrlDecode(parts[2]);

            var headerJson = Encoding.UTF8.GetString(Base64UrlDecode(header));
            var headerData = JObject.Parse(headerJson);
            var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(payload));
            var payloadData = JObject.Parse(payloadJson);

            if (verify)
            {
                var bytesToSign = Encoding.UTF8.GetBytes(string.Concat(header, ".", payload));
                var keyBytes = Encoding.UTF8.GetBytes(key);
                var algorithm = (string)headerData["alg"];

                var signature = HashAlgorithms[GetHashAlgorithm(algorithm)](keyBytes, bytesToSign);
                var decodedCrypto = Convert.ToBase64String(crypto);
                var decodedSignature = Convert.ToBase64String(signature);

                if (decodedCrypto != decodedSignature)
                {
                    throw new ApplicationException(string.Format("Invalid signature. Expected {0} got {1}", decodedCrypto, decodedSignature));
                }
            }

            return payloadData.ToString();
        }

        private static JwtHashAlgorithm GetHashAlgorithm(string algorithm)
        {
            switch (algorithm)
            {
                case "RS256": return JwtHashAlgorithm.RS256;
                case "RS512": return JwtHashAlgorithm.RS256;
                case "HS384": return JwtHashAlgorithm.HS384;
                case "HS512": return JwtHashAlgorithm.HS512;
                default: throw new InvalidOperationException("Algorithm not supported.");
            }
        }

        // from JWT spec
        private static string Base64UrlEncode(byte[] input)
        {
            var output = Convert.ToBase64String(input);
            output = output.Split('=')[0]; // Remove any trailing '='s
            output = output.Replace('+', '-'); // 62nd char of encoding
            output = output.Replace('/', '_'); // 63rd char of encoding
            return output;
        }

        // from JWT spec
        private static byte[] Base64UrlDecode(string input)
        {
            var output = input;
            output = output.Replace('-', '+'); // 62nd char of encoding
            output = output.Replace('_', '/'); // 63rd char of encoding
            switch (output.Length % 4) // Pad with trailing '='s
            {
                case 0: break; // No pad chars in this case
                case 2: output += "=="; break; // Two pad chars
                case 3: output += "="; break; // One pad char
                default: throw new System.Exception("Illegal base64url string!");
            }
            var converted = Convert.FromBase64String(output); // Standard base64 decoder
            return converted;
        }
    }
}