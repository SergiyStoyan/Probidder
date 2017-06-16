using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Reflection;

namespace Cliver.Probidder
{
    public class ComboBoxControl2 : ComboBox
    {
        public IEnumerable<string> ItemsSourceNomalized
        {
            set
            {
                List<string> vs = new List<string>();
                foreach (string s in value)
                {
                    if (string.IsNullOrWhiteSpace(s))
                        continue;
                    string v = apply_mask(s);
                    if (!vs.Contains(v))
                        vs.Add(v);
                }
                ItemsSource = vs;
                tb.Text = mask;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            tb = this.FindVisualChildrenOfType<TextBox>().Where(x => x.Name == "PART_EditableTextBox").First();
            tb.TextChanged += tb_TextChanged;
        }
        TextBox tb;

        private void tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            tb.Text = apply_mask(tb.Text);
            select(tb.SelectionStart, tb.SelectionLength);
        }

        public ComboBoxControl2()
        {
            List<string> ss = mask.ToCharArray().Distinct().Select(x => Regex.Escape(x.ToString())).ToList();
            mask_r = new Regex("[" + string.Join("", ss) + "]");
            ss.Remove(Regex.Escape("_"));
            mask_separators_r = new Regex("[" + string.Join("", ss) + "]");
        }
        readonly string mask = "(___) ___-____";
        readonly Regex mask_separators_r = null;
        readonly Regex mask_r = null;

        public string Mask
        {
            get
            {
                return mask;
            }
        }

        string apply_mask(string t)
        {
            if (t == null)
                return null;
            string s = strip_separators(t);
            t = mask;
            int j = 0;
            for (int i = 0; i < t.Length; i++)
            {
                if (j >= s.Length)
                    break;
                if (t[i] == '_')
                {
                    t = t.Remove(i, 1);
                    t = t.Insert(i, s[j++].ToString());
                }
            }
            return t;
        }

        string strip_separators(string t)
        {
            if (t == null)
                return null;
            return mask_separators_r.Replace(t, "");
        }

        string strip_mask(string t)
        {
            if (t == null)
                return null;
            return mask_r.Replace(t, "");
        }

        void select(int index, int length)
        {
            tb.BeginChange();
            tb.SelectionStart = index;
            tb.SelectionLength = length;
            tb.ScrollToHome();
            tb.EndChange();
        }        
    }
}
