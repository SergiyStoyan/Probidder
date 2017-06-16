/********************************************************************************************
        Author: Sergey Stoyan
        sergey.stoyan@gmail.com
        http://www.cliversoft.com
********************************************************************************************/

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
//using System.Windows.Forms;
using Microsoft.Win32;
using System.Windows.Input;
//using GlobalHotKey;

namespace Cliver.Probidder
{
    public partial class AutoComplete
    {
        static AutoComplete()
        {
        }

        public static bool IsKeyTrigger(Key key)
        {
            return key == Settings.AutoComplete.TriggerKey && (Keyboard.Modifiers & Settings.AutoComplete.TriggerModifierKey) == Settings.AutoComplete.TriggerModifierKey;
        }

        public static string GetComplete(string text, ref int end_position)
        {
            if (key_filter == null || string.IsNullOrEmpty(text))
                return text;
            bool found = false;
            string t1, t2;
            if (end_position < 0)
            {
                t1 = text;
                t2 = "";
            }
            else
            {
                t1 = text.Substring(0, end_position);
                t2 = text.Substring(end_position, text.Length - end_position);
            }
            string t = key_filter.Replace(t1, (Match m) =>
            {
                found = true;
                return Settings.AutoComplete.Keys2Phrase[m.Value];
            }, 1, end_position) + t2;
            end_position = t.Length - t2.Length;
            if (found)
            {
                Console.Beep(4000, 80);
                Console.Beep(5000, 80);
                Console.Beep(6000, 50);
            }
            else
            {
                Console.Beep(5000, 200);
            }
            return t;
        }

        internal static void UpdateRegex(Dictionary<string, string> keys2Phrase)
        {
            key_filter = new Regex("(" + string.Join("|", keys2Phrase.Keys.Select(x => Regex.Escape(x))) + ")$", RegexOptions.RightToLeft);
        }
       static Regex key_filter = null;

    }
}