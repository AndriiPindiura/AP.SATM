using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System.Net;
using System.Drawing.Imaging;

namespace satmCCTV
{
    public class GPUcore
    {
        private Form cctvUI;
        private bool streamActivated;
        private bool jobExists;
        private DateTime archDate;
        private string camera;
        private string uuid;
        public delegate void OnConnectDelegate(bool status);
        public event OnConnectDelegate OnConnect;
        private void cctvConnectDelegate(bool status)
        {
            //MessageBox.Show("Delegate");
        }



        private void ocx_OnVideoFrame(object sender, AxACTIVEXLib._DCamMonitorEvents_OnVideoFrameEvent e)
        {
            //cctv.DoReactMonitor("MONITOR||STOP_VIDEO|cam<" + e.cam_id + ">");
            streamActivated = true;
            if (jobExists)
            {
                AxACTIVEXLib.AxCamMonitor cctv = (AxACTIVEXLib.AxCamMonitor)sender;
                cctv.DoReactMonitor("MONITOR||ACTIVATE_CAM|cam<" + camera + ">");
                cctv.DoReactMonitor("MONITOR||ARCH_FRAME_TIME|cam<" + camera + ">,date<" + archDate.ToString("dd-MM-yy") + ">,time<" + archDate.ToString("HH:mm:ss") + ">");
                cctv.DoReactMonitor("MONITOR||EXPORT_FRAME|cam<" + camera + ">,file<" + uuid + "-" + camera + ".jpg>");
                cctv.ShowCam(e.cam_id, 1, 1);
                jobExists = false;
            }
            //MessageBox.Show(e.cam_id.ToString() + ":-:" + e.date.ToString() + ":-:" + e.fraction.ToString());
            /*if (File.Exists(Path.GetTempPath() + uid + "-" + e.cam_id + ".jpg"))
                cctv.DoReactMonitor("MONITOR||STOP_VIDEO|cam<" + e.cam_id + ">");
            else
                cctv.DoReactMonitor("MONITOR||EXPORT_FRAME|cam<" + e.cam_id + ">,file<" + Path.GetTempPath() + uid + "-" + e.cam_id + ".jpg>");
        */
        }

        private void ocx_OnConnectStateChanged(object sender, AxACTIVEXLib._DCamMonitorEvents_OnConnectStateChangedEvent e)
        {
            AxACTIVEXLib.AxCamMonitor cctv = (AxACTIVEXLib.AxCamMonitor)sender;
            if (e.state == 1)
            {
                //MessageBox.Show("CORE connected");
                OnConnect(true);
                Thread cctvLive = new Thread(() => ocxStream(sender));
                cctvLive.Start();
            }
            else
            {
                OnConnect(false);
                //OnImageRecieved(false);
            }
        }

        private void ocxStream(object sender)
        {
            Thread.Sleep(1000);
            AxACTIVEXLib.AxCamMonitor cctv = (AxACTIVEXLib.AxCamMonitor)sender;
            cctv.ShowCam(1, 1, 1);
        }

        private void Close()
        {
            if (cctvUI.IsHandleCreated)
            {
                cctvUI.Invoke(new MethodInvoker(cctvUI.Close));
            }
        }

        public void CCTVInit(string ip)
        {
            cctvUI = new Form();
            streamActivated = false;
            AxACTIVEXLib.AxCamMonitor ocx = new AxACTIVEXLib.AxCamMonitor();
            cctvUI.Size = new Size(800, 600);
            cctvUI.FormBorderStyle = FormBorderStyle.None;
            ocx.Parent = cctvUI;
            ocx.Dock = DockStyle.Fill;
            ocx.OnVideoFrame += ocx_OnVideoFrame;
            ocx.OnConnectStateChanged += ocx_OnConnectStateChanged;
            OnConnect += new OnConnectDelegate(cctvConnectDelegate);
            //ocx.DblClick += ocx_DblClick;
            //OnImageRecieved += new OnImageDelegate(Dummy);
            cctvUI.StartPosition = FormStartPosition.CenterScreen;
            cctvUI.Show();
            ocx.Connect(ip, "", "", "", 0);
        }

        public void Export(string cam, DateTime date, string id)
        {
            camera = cam;
            archDate = date;
            uuid = id;
            jobExists = true;
        }

    }
}
