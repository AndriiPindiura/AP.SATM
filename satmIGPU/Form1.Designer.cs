namespace satmIGPU
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.axCamMonitor1 = new AxACTIVEXLib.AxCamMonitor();
            this.axCamMonitor2 = new AxACTIVEXLib.AxCamMonitor();
            this.axCamMonitor3 = new AxACTIVEXLib.AxCamMonitor();
            ((System.ComponentModel.ISupportInitialize)(this.axCamMonitor1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.axCamMonitor2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.axCamMonitor3)).BeginInit();
            this.SuspendLayout();
            // 
            // axCamMonitor1
            // 
            this.axCamMonitor1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.axCamMonitor1.Enabled = true;
            this.axCamMonitor1.Location = new System.Drawing.Point(0, 0);
            this.axCamMonitor1.Name = "axCamMonitor1";
            this.axCamMonitor1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axCamMonitor1.OcxState")));
            this.axCamMonitor1.Size = new System.Drawing.Size(784, 561);
            this.axCamMonitor1.TabIndex = 0;
            this.axCamMonitor1.OnConnectStateChanged += new AxACTIVEXLib._DCamMonitorEvents_OnConnectStateChangedEventHandler(this.axCamMonitor1_OnConnectStateChanged);
            // 
            // axCamMonitor2
            // 
            this.axCamMonitor2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.axCamMonitor2.Enabled = true;
            this.axCamMonitor2.Location = new System.Drawing.Point(0, 0);
            this.axCamMonitor2.Name = "axCamMonitor2";
            this.axCamMonitor2.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axCamMonitor2.OcxState")));
            this.axCamMonitor2.Size = new System.Drawing.Size(784, 561);
            this.axCamMonitor2.TabIndex = 1;
            this.axCamMonitor2.OnConnectStateChanged += new AxACTIVEXLib._DCamMonitorEvents_OnConnectStateChangedEventHandler(this.axCamMonitor1_OnConnectStateChanged);
            // 
            // axCamMonitor3
            // 
            this.axCamMonitor3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.axCamMonitor3.Enabled = true;
            this.axCamMonitor3.Location = new System.Drawing.Point(0, 0);
            this.axCamMonitor3.Name = "axCamMonitor3";
            this.axCamMonitor3.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axCamMonitor3.OcxState")));
            this.axCamMonitor3.Size = new System.Drawing.Size(784, 561);
            this.axCamMonitor3.TabIndex = 2;
            this.axCamMonitor3.OnConnectStateChanged += new AxACTIVEXLib._DCamMonitorEvents_OnConnectStateChangedEventHandler(this.axCamMonitor1_OnConnectStateChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.axCamMonitor3);
            this.Controls.Add(this.axCamMonitor2);
            this.Controls.Add(this.axCamMonitor1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Form1";
            this.Text = "satmIGPU";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.axCamMonitor1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.axCamMonitor2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.axCamMonitor3)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private AxACTIVEXLib.AxCamMonitor axCamMonitor1;
        private AxACTIVEXLib.AxCamMonitor axCamMonitor2;
        private AxACTIVEXLib.AxCamMonitor axCamMonitor3;
    }
}

