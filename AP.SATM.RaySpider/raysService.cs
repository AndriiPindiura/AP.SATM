using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;

namespace AP.SATM.RaySpider
{
    public partial class raysService : ServiceBase
    {
        public raysService()
        {
            InitializeComponent();
            spider = new Core();
            startSpider = new Thread(StartCaptureEvents);
            startSpider.IsBackground = true;
        }

        private Core spider;

        private Thread startSpider;

        private System.Timers.Timer timer;

        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //MyServiceApp.ServiceWork.Main(); // my separate static method for do work
            this.timer.Stop();
            spider.UpdateDb();
            this.timer.Start();
        }


        private void StartCaptureEvents()
        {
            spider.StartCaptureEvents();
        }

        protected override void OnStart(string[] args)
        {
            startSpider.Start();
            this.timer = new System.Timers.Timer(60 * 1000 * 1);  // 30000 milliseconds = 30 seconds
            this.timer.AutoReset = true;
            this.timer.Elapsed += new System.Timers.ElapsedEventHandler(this.timer_Elapsed);
            this.timer.Start();
        }

        protected override void OnStop()
        {
            spider.StopCaptureEvents();
        }
    }
}
