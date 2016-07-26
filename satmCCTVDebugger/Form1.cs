using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace satmCCTVDebugger
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            cctv.OnConnect += cctvConnect;
        }
        private bool cctvStatus;
        private satmCCTV.GPUcore cctv = new satmCCTV.GPUcore();
        private void timer()
        {
           for (int i = 1; i < 21; i++)
           {
               Thread.Sleep(1000);
               if (cctvStatus)
                   break;
           }
        }
        private void cctvConnect(bool status)
        {
            button2.Enabled = status;
            if (status)
                cctvStatus = true;
            else
                cctvStatus = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            cctv.CCTVInit("10.1.11.101");
            Thread wait = new Thread(timer);
            wait.Start();
            wait.Join();
            /*if (cctvStatus)
                MessageBox.Show("Connected");
            else
                MessageBox.Show("Timeout");*/
        }

        private void button2_Click(object sender, EventArgs e)
        {
            cctv.Export("1", DateTime.Parse("2014-07-01 08:00:00"), Guid.NewGuid().ToString());
        }
    }
}
