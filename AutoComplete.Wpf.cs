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
using System.Windows;
using System.Windows.Controls;

namespace Cliver.Foreclosures
{
    public partial class AutoComplete
    {
        public class Wpf : AutoComplete
        {
            static public void KeyDownHandler(object sender, KeyEventArgs e)
            {
                if (!AutoComplete.IsKeyTrigger(e.Key))
                    return;
                IInputElement ii = Keyboard.FocusedElement;
                if (ii == null)
                    return;
                e.Handled = true;
                if (ii is TextBox)
                {
                    process_TextBox((TextBox)ii);
                    return;
                }
                if (ii is ComboBox)
                {
                    process_TextBox(((ComboBox)ii).FindChildrenOfType<TextBox>().Where(x => x.Visibility == Visibility.Visible).First());
                    return;
                }
            }

            static void process_TextBox(TextBox tb)
            {
                int position = tb.CaretIndex;
                tb.Text = AutoComplete.GetComplete(tb.Text, ref position);
                tb.CaretIndex = position;
            }
        }
    }
}