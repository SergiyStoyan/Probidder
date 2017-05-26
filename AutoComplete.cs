/********************************************************************************************
        Author: Sergey Stoyan
        sergey.stoyan@gmail.com
        http://www.cliversoft.com
********************************************************************************************/
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
//using GlobalHotKey;

namespace Cliver.Foreclosures
{
    public class AutoComplete
    {
        //static AutoComplete This
        //{
        //    get
        //    {
        //        if (_This == null)
        //            _This = new AutoComplete();
        //        return _This;
        //    }
        //}
        //static AutoComplete _This = null;

        static AutoComplete()
        {
        }
        
        public static bool IsKeyTrigger(Key key)
        {
            return key == Settings.AutoComplete.TriggerKey && (Keyboard.Modifiers & Settings.AutoComplete.TriggerModifierKey) == Settings.AutoComplete.TriggerModifierKey;
        }

        public static string GetComplete(string text, int end_position = -1)
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
            if(found)
            {
                Console.Beep(4000, 100);
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