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

namespace Cliver.Foreclosures
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

        public static Thread BeginToServer(bool show_start_notification)
        {
            if (to_server_t != null && to_server_t.IsAlive)
                return to_server_t;

            MessageForm mf = null;
            to_server_t = ThreadRoutines.StartTry(() =>
            {
                ToServerStateChanged?.BeginInvoke(null, null);
                Log.Main.Inform("Uploading database.");

                if (show_start_notification)
                {
                    ThreadRoutines.StartTry(() =>
                    {
                        mf = new MessageForm(System.Windows.Forms.Application.ProductName, System.Drawing.SystemIcons.Exclamation, "Uploading database to the server. Please wait...", new string[1] { "OK" }, 0, null);
                        mf.ShowDialog();
                    });
                    if (SleepRoutines.WaitForObject(() => { return mf; }, 10000) == null)
                        Log.Main.Exit("SleepRoutines.WaitForObject got null");
                }

                HttpClientHandler handler = new HttpClientHandler();
                HttpClient http_client = new HttpClient(handler);

                bool r = _ToServer();
                mf?.Close();
                InfoWindow.Create(ProgramRoutines.GetAppName(), "Database has been uploaded successfully.", null, "OK", null, System.Windows.Media.Brushes.White, System.Windows.Media.Brushes.Green);
                if (r)
                    if (Message.YesNo("Data has been uploaded succesfully to " + Settings.Login.ExportUrl + "\r\n\r\nClean up the database?"))
                    {
                        Log.Inform("Dropping the database.");
                        Db.Foreclosures fs = new Db.Foreclosures();
                        fs.Drop();
                    }
            },
            (Exception e) =>
            {
                Log.Main.Error(e);
                Log.Main.Error("Could not upload data.");

                mf?.Close();
                InfoWindow.Create(ProgramRoutines.GetAppName() + ": database could not be uploaded!", Log.GetExceptionMessage(e), null, "OK", null, System.Windows.Media.Brushes.WhiteSmoke, System.Windows.Media.Brushes.Red);
            },
            () =>
            {
                Settings.Database.Save();
                ToServerStateChanged?.BeginInvoke(null, null);
            }
            );
            return to_server_t;
        }
        static Thread to_server_t = null;

        public static bool _ToServer()
        {
            Log.Main.Inform("Exporting to: " + Settings.Login.ExportUrl);

            try
            {
                HttpClient http_client = new HttpClient();
                if (!loginByUsername(ref http_client, getOAuthTAccessToken(ref http_client), Settings.Login.UserName, Settings.Login.Password()))
                {
                    Message.Error("Could not login with Username: " + Settings.Login.UserName);
                    return false;
                }

                Db.Foreclosures fs = new Db.Foreclosures();
                string s = SerializationRoutines.Json.Serialize(fs.GetAll());
                StringContent post_data = new StringContent(s);
                post_data.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");
                HttpResponseMessage rm = http_client.PostAsync(Settings.Login.ExportUrl, post_data).Result;
                if (!rm.IsSuccessStatusCode)
                    throw new Exception(rm.ReasonPhrase);
                return true;
            }
            catch (Exception ex)
            {
                Message.Error("Could not upload: " + ex.Message);
                Log.Error("Could not upload: " + Log.GetExceptionMessage(ex));
            }
            return false;

        }

        static public string getOAuthTAccessToken(ref HttpClient http_client)
        {
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

            FormUrlEncodedContent fuec = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer" },
                { "assertion",  jwt }
            });
            if (http_client == null)
                http_client = new HttpClient();
            HttpResponseMessage rm = http_client.PostAsync("https://dev-auth.probidder.com/api/oauth/token", fuec).Result;
            if (!rm.IsSuccessStatusCode)
                throw new Exception("Could not get AuthTAccessToken: " + rm.ReasonPhrase);
            dynamic d = SerializationRoutines.Json.Deserialize<dynamic>(rm.Content.ReadAsStringAsync().Result);
            return (string)d["access_token"];
        }

        static public bool loginByUsername(ref HttpClient http_client, string access_token, string username, string password)
        {
            FormUrlEncodedContent fuec = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "username", username },
                { "password",  password }
            });

            HttpRequestMessage hrm = new HttpRequestMessage(HttpMethod.Get, "https://dev-auth.probidder.com/api/authenticate/recorders/win/app?" + fuec.ReadAsStringAsync().Result);
            hrm.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", access_token);
            if (http_client == null)
                http_client = new HttpClient();
            HttpResponseMessage rm = http_client.SendAsync(hrm).Result;
            if (!rm.IsSuccessStatusCode)
                throw new Exception("Could not get login: " + rm.ReasonPhrase);
            dynamic d = SerializationRoutines.Json.Deserialize<dynamic>(rm.Content.ReadAsStringAsync().Result);
            return d["status"]; 
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

                    string key_file = Path.GetTempPath() + "\\key.bin";
                    string value_file = Path.GetTempPath() + "\\payload.bin";
                    string signed_file = Path.GetTempPath() + "\\payload.bin.signed";
                    File.WriteAllBytes(key_file, key);
                    File.WriteAllBytes(value_file, value);
                    ProcessStartInfo psi = new ProcessStartInfo("openssl\\openssl.exe", "dgst -sha512 -sign \"" + key_file + "\" -out \"" + signed_file + "\" \"" + value_file + "\"");
                    psi.RedirectStandardOutput = true;
                    psi.RedirectStandardError = true;
                    psi.UseShellExecute = false;
                    psi.CreateNoWindow = true;
                    Process p = new Process();
                    p.StartInfo = psi;
                    p.Start();
                    p.WaitForExit();
                    return File.ReadAllBytes(signed_file);
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