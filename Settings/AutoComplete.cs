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
        public static readonly AutoCompleteSettings AutoComplete;

        public class AutoCompleteSettings : Cliver.Settings
        {
            public Dictionary<string, string> Keys2Phrase = null;
            public System.Windows.Input.Key TriggerKey = System.Windows.Input.Key.D8;
            public System.Windows.Input.ModifierKeys TriggerModifierKey = System.Windows.Input.ModifierKeys.Shift;

            public override void Loaded()
            {
                if (Keys2Phrase == null)
                    Keys2Phrase = new Dictionary<string, string> {
{"ASS","JGT: [ASSET ACCEPTANCE] #R" },
{"BAN","MTG: [BANK OF NEW YORK] #R" },
{"BEN","MTG: [BENEFICIAL] #R" },
{"BIG","BIG DIFFERENCE BETWEEN MTG AND AMOUNT DUE" },
{"BMO","MTG: [BMO] #R" },
{"C","CONDOMINIUM" },
{"CA","CONDOMINIUM ASSN" },
{"CAP","JGT: [CAPITAL ONE] #R" },
{"CIT","MTG: [CITIZENS BANK] #R" },
{"D","DUPAGE" },
{"DIS","JGT: [DISCOVER] #R" },
{"FIF","MTG: [FIFTH THIRD] #R" },
{"FIR","MTG: [FIRST AMERICAN] #R" },
{"FOX","[FOX METRO WATER] #R" },
{"HOU","JGT: [HOUSEHOLD FINANCE] #R" },
{"HSBC","MTG: [HSBC] #R" },
{"HUD","MTG: [HUD] #R" },
{"JP","MTG: [JPMORGAN CHASE] #R" },
{"LAS","LAST PAY DATE IS NOT LISTED ON COMPLAINT" },
{"MERS","MTG: [MERS] #R" },
{"MID","JGT: [MIDLAND] #R" },
{"PNC","MTG: [PNC] #R" },
{"PORT","JGT: [PORTFOLIO RECOVERY] #R" },
{"RSB","MTG: [RSB] #R" },
{"TCF","MTG: [TCF] #R" },
{"TOW","TOWNHOME" },
{"UNI","JGT: [UNIFUND CCR] #R" },
{"US","MTG: [US BANK] #R" },
{"WEL","MTG: [WELLS FARGO] #R" },
                    };

                ListWindow.This?.Set();
                Cliver.Foreclosures.AutoComplete.UpdateRegex(Keys2Phrase);
            }

            public override void Saving()
            {
            }            
        }
    }
}