using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cliver.Foreclosures
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
            this.Icon = AssemblyRoutines.GetAppIcon();

            FormClosed += delegate
              {
                  sf = null;
              };

            foreach (System.Windows.Input.Key k in Enum.GetValues(typeof(System.Windows.Input.Key)))
                TicketKey.Items.Add(k);
            TicketKey.SelectedItem = Settings.General.TicketKey;

            foreach (System.Windows.Input.ModifierKeys k in Enum.GetValues(typeof(System.Windows.Input.ModifierKeys)))
                TicketModifierKey1.Items.Add(k);
            TicketModifierKey1.SelectedItem = Settings.General.TicketModifierKey1;

            foreach (System.Windows.Input.ModifierKeys k in Enum.GetValues(typeof(System.Windows.Input.ModifierKeys)))
                TicketModifierKey2.Items.Add(k);
            TicketModifierKey2.SelectedItem = Settings.General.TicketModifierKey2;

            DbRefreshPeriodInSecs.Text = Settings.General.DbRefreshPeriodInSecs.ToString();
        }

        //public class EncodingItem
        //{
        //    public string Text { get; set; }
        //    public int CodePage { get; set; }
        //}

        static public void Open()
        {
            if (sf == null)
                sf = new SettingsForm();
            sf.Show();
            sf.Activate();
        }
        static SettingsForm sf = null;

        private void bCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void bOk_Click(object sender, EventArgs e)
        {
            try
            {
                Settings.General.TicketKey = (System.Windows.Input.Key)TicketKey.SelectedItem;
                Settings.General.TicketModifierKey1 = (System.Windows.Input.ModifierKeys)TicketModifierKey1.SelectedItem;
                Settings.General.TicketModifierKey2 = (System.Windows.Input.ModifierKeys)TicketModifierKey2.SelectedItem;
                int secs = int.Parse(DbRefreshPeriodInSecs.Text);
                if (secs <= 0)
                    throw new Exception("Db Refresh Period must be positive.");
                if (secs > Int32.MaxValue)
                    throw new Exception("Db Refresh Period is too big.");

                Settings.General.Save();
                Config.Reload();

                Close();

                //bool running = Service.Running;
                //Service.Running = false;
                //Service.Running = running;
            }
            catch (Exception ex)
            {
                Message.Exclaim(ex.Message);
            }
        }
    }
}