using System;
using System.ServiceProcess;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace AP.SATM.Heart
{
    public class Daemon:ServiceBase
    {

        public Daemon() 
        { 
            this.ServiceName = "SATM Core"; 
            this.CanStop = true; 
            this.CanPauseAndContinue = false; 
            this.AutoLog = true;
            //this.EventLog = new System.Diagnostics.EventLog();
            this.EventLog.Source = this.ServiceName;
            this.EventLog.Log = "Application";
        }

        private System.Timers.Timer timer;

        private ServiceHost satmHost = new ServiceHost(typeof(Core));

        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //MyServiceApp.ServiceWork.Main(); // my separate static method for do work
            this.timer.Stop();
            try
            {
                Process syncProtocols = new Process();
                syncProtocols.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "\\AP.SATM.Brain.exe";
                syncProtocols.StartInfo.Arguments = "/sync";
                syncProtocols.Start();
                syncProtocols.WaitForExit();
            }
            catch (Exception ex)
            {
                this.EventLog.WriteEntry(ex.Message, EventLogEntryType.Warning);
            }
            this.timer.Start();
        }


        public void logwrite(string message, System.Diagnostics.EventLogEntryType level)
        {
            try
            {
                this.EventLog.WriteEntry(message, level);
            }
            catch { }
        }

        protected override void OnStart(string[] args) 
        { 
            // TODO: add startup stuff 
            try
            {
                satmHost.Open();
#if (DEBUG)
                    foreach (var endpt in satmHost.Description.Endpoints)
                    {
                        this.EventLog.WriteEntry(endpt.Address.ToString(), System.Diagnostics.EventLogEntryType.Information);
                        this.EventLog.WriteEntry(endpt.Binding.ToString(), System.Diagnostics.EventLogEntryType.Information);
                        this.EventLog.WriteEntry(endpt.Contract.ContractType.Name, System.Diagnostics.EventLogEntryType.Information);
                        /*Console.WriteLine("Enpoint address:\t{0}", endpt.Address);
                        Console.WriteLine("Enpoint binding:\t{0}", endpt.Binding);
                        Console.WriteLine("Enpoint contract:\t{0}\n", endpt.Contract.ContractType.Name);*/
                    }
#else
#endif
                this.timer = new System.Timers.Timer(60*1000*10);  // 30000 milliseconds = 30 seconds
                this.timer.AutoReset = true;
                this.timer.Elapsed += new System.Timers.ElapsedEventHandler(this.timer_Elapsed);
                this.timer.Start();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    this.EventLog.WriteEntry("InnerException: " + ex.InnerException.Message, System.Diagnostics.EventLogEntryType.Error);
                else
                    this.EventLog.WriteEntry("Exception: " + ex.Message, System.Diagnostics.EventLogEntryType.Error);
                satmHost.Abort();
            }


            /*try
            {
                satmHost.Open();
            }
            catch (Exception e)
            {
                satmHost.Abort();
            }*/
        }


        protected override void OnStop() 
        { 
            // TODO: add shutdown stuff '
            try
            {
                satmHost.Close();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    this.EventLog.WriteEntry("InnerException: " + ex.InnerException.Message, System.Diagnostics.EventLogEntryType.Error);
                else
                    this.EventLog.WriteEntry("Exception: " + ex.Message, System.Diagnostics.EventLogEntryType.Error);
                satmHost.Abort();
                satmHost.Abort();
            }
        }

        private void InitializeComponent()
        {

        } 
    }
}
