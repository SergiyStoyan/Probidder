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
//using System.Windows.Shapes;
using System.Net.Http;
using System.Net;
using System.IO;
using System.Management;
using System.Threading;

namespace Cliver.Foreclosures
{
    public partial class AuctionWindow : Window
    {
        public AuctionWindow()
        {
            InitializeComponent();

            Icon = AssemblyRoutines.GetAppIconImageSource();

            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            //string temp_dir = Path.GetTempPath() + "\\" + ProgramRoutines.GetAppName();
            //DateTime delete_time = DateTime.Now.AddDays(-3);
            //foreach (FileInfo fi in (new DirectoryInfo(temp_dir)).GetFiles())
            //    if (fi.LastWriteTime < delete_time)
            //        try
            //        {
            //            fi.Delete();
            //        }
            //        catch { }

            HttpClientHandler handler = new HttpClientHandler();
            //handler.Credentials = new System.Net.NetworkCredential(Settings.General.ZendeskUser, Settings.General.ZendeskPassword);
            http_client = new HttpClient(handler);

            Closing += delegate (object sender, System.ComponentModel.CancelEventArgs e)
            {
                if (Message.YesNo("Posting the ticket is in progress. Do you want to cancel it?"))
                {
                    create_ticket_t = null;
                    http_client.CancelPendingRequests();
                    Log.Main.Inform("Cancelling...");
                }
                e.Cancel = true;
            };

            Closed += delegate
            {
                http_client.CancelPendingRequests();
            };
        }
        
        readonly HttpClient http_client;
        
        void submit(object sender, EventArgs e)
        {
            try
            {
                //if (string.IsNullOrWhiteSpace(subject.Text))
                //    throw new Exception("Subject is empty.");
                //if (string.IsNullOrWhiteSpace(this.description.Text))
                //    throw new Exception("Description is empty

                //if (!ok.IsEnabled)
                //    return;
                //ok.IsEnabled = false;
                Cursor = Cursors.Wait;
                //string description = this.description.Text;
                create_ticket_t = ThreadRoutines.StartTry(
                    () =>
                    {
                        //create_ticket(Environment.UserName, Settings.General.UserEmail, "Request from support app", description, files);
                    }
                );
            }
            catch (Exception ex)
            {
                Message.Exclaim(ex.Message);
            }
        }
        Thread create_ticket_t = null;

        static string userPrincipalEmail = null;

        async void create_ticket(string user, string user_email, string subject, string description, List<string> files)
        {
            try
            {
                Log.Main.Inform("Creating ticket.");

                if (create_ticket_t == null)
                    return;

                if (string.IsNullOrWhiteSpace(user_email))
                {
                    if (userPrincipalEmail == null)
                    {//consumes a long time 
                        userPrincipalEmail = System.DirectoryServices.AccountManagement.UserPrincipal.Current.EmailAddress;
                        if (userPrincipalEmail == null)
                            userPrincipalEmail = string.Empty;
                    }
                    if (userPrincipalEmail != string.Empty)
                        user_email = userPrincipalEmail;
                }

                if (create_ticket_t == null)
                    return;

                if (create_ticket_t == null)
                    return;

                List<string> ps = new List<string>();
                foreach (SystemInfo.ProcessorInfo p in SystemInfo.GetProcessorInfo())
                    ps.Add(p.procName);
                long hdd_total = 0;
                long hdd_free = 0;
                foreach (SystemInfo.DiskInfo h in SystemInfo.GetDiskInfo().Values)
                {
                    hdd_total += h.total;
                    hdd_free += h.free;

                    if (create_ticket_t == null)
                        return;
                }
                string hostname = Dns.GetHostName();
                List<string> sils = new List<string>();
                sils.Add("hostname: <a href='https://support.bomgar.com/api/client_script?type=rep&operation=generate&action=start_pinned_client_session&search_string=" + hostname + "'>" + hostname + "</a>");

                if (create_ticket_t == null)
                    return;

                sils.Add("currentuser: " + Environment.UserName); //System.DirectoryServices.AccountManagement.UserPrincipal.Current.Name

                if (create_ticket_t == null)
                    return;

                sils.Add("os: " + SystemInfo.GetWindowsVersion());

                if (create_ticket_t == null)
                    return;

                sils.Add("os uptime: " + SystemInfo.GetUpTime().ToString());

                if (create_ticket_t == null)
                    return;

                sils.Add("cpu: " + string.Join("\r\ncpu:", ps));

                if (create_ticket_t == null)
                    return;

                sils.Add("mem: " + SystemInfo.GetTotalPhysicalMemory());

                if (create_ticket_t == null)
                    return;

                sils.Add("hdd:");
                sils.Add("total: " + hdd_total);
                sils.Add("free: " + hdd_free);
                //sils.Add("total: " + h.total + "\r\nfree: " + h.free); string.Join("\r\nhdd:\r\n", hs) +);
                sils.Add("ip: " + SystemInfo.GetLocalIp().ToString());
                string system_info = string.Join("<br>", sils);
                var data = new
                {
                };

                if (create_ticket_t == null)
                    return;

                string json_string = SerializationRoutines.Json.Serialize(data);

                if (create_ticket_t == null)
                    return;

                Log.Main.Inform("Posting ticket: " + json_string);
                var post_data = new StringContent(json_string, Encoding.UTF8, "application/json");
                HttpResponseMessage rm = await http_client.PostAsync("https://", post_data);
                if (!rm.IsSuccessStatusCode)
                    throw new Exception("Could not create ticket: " + rm.ReasonPhrase);
                //if (rm.Content != null)
                //    var responseContent = await rm.Content.ReadAsStringAsync();

                this.Dispatcher.Invoke(() =>
                {
                    //ok.IsEnabled = true;
                    Close();
                });
                LogMessage.Inform("The ticket was succesfully created.");
            }
            catch (System.Threading.Tasks.TaskCanceledException e)
            {
                if (create_ticket_t == null)
                    return;
                Log.Main.Warning(e);
            }
            catch (Exception e)
            {
                LogMessage.Error(e);
            }
            finally
            {
                //ok.Dispatcher.Invoke(() => {
                //    Cursor = Cursors.Arrow;
                //    ok.IsEnabled = true;
                //});
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            //error.Visibility = Visibility.Collapsed;
            //if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            //    return;
            //foreach (string file in (string[])e.Data.GetData(DataFormats.FileDrop))
            //    add_attachment(file);
        }
    }
}
