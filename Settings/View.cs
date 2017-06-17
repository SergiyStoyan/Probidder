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

namespace Cliver.Probidder
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
            public System.Windows.Media.Brush FocusedControlColor = System.Windows.Media.Brushes.Gold;
            public Tables ActiveTable
            {
                get
                {
                    return _ActiveTable;
                }
                set
                {
                    _ActiveTable = value;
                    ListWindow.This?.ActiveTableChanged();
                }
            }
            Tables _ActiveTable = Tables.Foreclosures;
            public enum Tables
            {
                Foreclosures,
                Probates
            }
            public class Columns
            {
                public List<string> Showed = null;
                public List<string> Searched = null;
            }
            public Dictionary<Tables, Columns> Tables2Columns = null;

            public override void Loaded()
            {
                if (Tables2Columns == null)
                {
                    Tables2Columns = new Dictionary<Tables, Columns>();

                    List<string> fs = typeof(Db.Foreclosure).GetProperties(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance).Select(x => x.Name).ToList();                   
                    Tables2Columns[Tables.Foreclosures] = new Columns { Showed = fs, Searched = fs };

                    fs = typeof(Db.Probate).GetProperties(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance).Select(x => x.Name).ToList();
                    Tables2Columns[Tables.Probates] = new Columns { Showed = fs, Searched = fs };
                }
            }

            public override void Saving()
            {
                if (Tables2Columns != null)
                {
                    Tables2Columns[Tables.Foreclosures].Showed = Tables2Columns[Tables.Foreclosures].Showed.Distinct().ToList();
                    Tables2Columns[Tables.Foreclosures].Searched = Tables2Columns[Tables.Foreclosures].Searched.Distinct().ToList();
                    Tables2Columns[Tables.Probates].Showed = Tables2Columns[Tables.Probates].Showed.Distinct().ToList();
                    Tables2Columns[Tables.Probates].Searched = Tables2Columns[Tables.Probates].Searched.Distinct().ToList();                    
                }
            }            
        }
    }
}