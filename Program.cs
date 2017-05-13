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
using System.Net.Mail;
using Cliver;
using System.Configuration;
using System.Windows.Forms;
using Microsoft.Win32;
/*
 - when saving, which fields are obligatory and the user should be notified if they are empty?
 - AUCTION_DATE, AUCTION_TIME are not used in the access form so they are empty now. Let me know what to do with them?
 - 
     
     */
namespace Cliver.Foreclosures
{
    public class Program
    {
        static Program()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            AppDomain.CurrentDomain.UnhandledException += delegate (object sender, UnhandledExceptionEventArgs args)
            {
                Exception e = (Exception)args.ExceptionObject;
                Message.Error(e);
                Application.Exit();
            };

            Message.TopMost = true;

            Log.Initialize(Log.Mode.ONLY_LOG);
            //Cliver.Config.Initialize(new string[] { "General" });
            Cliver.Config.Reload();
        }        

        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                ProcessRoutines.RunSingleProcessOnly();

                Service.Running = true;
                //Db.BeginRefresh();

                Application.Run(SysTray.This);
            }
            catch (Exception e)
            {
                Message.Error(e);
            }
            finally
            {
                Environment.Exit(0);
            }
        }
    }
}