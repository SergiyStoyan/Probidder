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
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Script.Serialization;

namespace Cliver.Foreclosures
{
    public partial class Settings
    {
        [Cliver.Settings.Obligatory]
        public static readonly ViewSettings View;

        public class ViewSettings : Cliver.Settings
        {
            public int InfoToastLifeTimeInSecs = 5;
            public string InfoSoundFile = "inform.wav";
            public int InfoToastBottom = 100;
            public int InfoToastRight = 0;
            public System.Windows.Media.Brush SearchHighlightColor = System.Windows.Media.Brushes.Yellow;
            public List<string> ShowedColumns = null;
            public List<string> SearchedColumns = null;
            public System.Windows.Media.Brush FocusedControlColor = System.Windows.Media.Brushes.Bisque;

            public override void Loaded()
            {
                if (ShowedColumns == null)
                    ShowedColumns = new List<string> { "FILING_DATE", "CITY", "ADDRESS" };
                ShowedColumns = ShowedColumns.Select(x => x).Distinct().ToList();
                if (SearchedColumns == null)
                    SearchedColumns = new List<string> { "CITY", "ADDRESS" };
                SearchedColumns = SearchedColumns.Select(x => x).Distinct().ToList();

                ListWindow.This?.OrderColumns();
            }

            public override void Saving()
            {
            }            
        }
    }
}