namespace satmAnalyzer
{
    partial class cctv
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(cctv));
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.axCamMonitor1 = new AxACTIVEXLib.AxCamMonitor();
            ((System.ComponentModel.ISupportInitialize)(this.axCamMonitor1)).BeginInit();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.richTextBox1.Location = new System.Drawing.Point(0, 224);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.richTextBox1.Size = new System.Drawing.Size(320, 96);
            this.richTextBox1.TabIndex = 1;
            this.richTextBox1.Text = "";
            this.richTextBox1.TextChanged += new System.EventHandler(this.richTextBox1_TextChanged);
            // 
            // axCamMonitor1
            // 
            this.axCamMonitor1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.axCamMonitor1.Enabled = true;
            this.axCamMonitor1.Location = new System.Drawing.Point(0, 0);
            this.axCamMonitor1.Name = "axCamMonitor1";
            this.axCamMonitor1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axCamMonitor1.OcxState")));
            this.axCamMonitor1.Size = new System.Drawing.Size(320, 224);
            this.axCamMonitor1.TabIndex = 4;
            this.axCamMonitor1.OnConnectStateChanged += new AxACTIVEXLib._DCamMonitorEvents_OnConnectStateChangedEventHandler(this.axCamMonitor1_OnConnectStateChanged);
            this.axCamMonitor1.DblClick += new AxACTIVEXLib._DCamMonitorEvents_DblClickEventHandler(this.axCamMonitor1_DblClick);
            this.axCamMonitor1.OnVideoFrame += new AxACTIVEXLib._DCamMonitorEvents_OnVideoFrameEventHandler(this.axCamMonitor1_OnVideoFrame);
            // 
            // cctv
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(320, 320);
            this.Controls.Add(this.axCamMonitor1);
            this.Controls.Add(this.richTextBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "cctv";
            this.Text = "cctv";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.cctv_FormClosing);
            this.Load += new System.EventHandler(this.cctv_Load);
            ((System.ComponentModel.ISupportInitialize)(this.axCamMonitor1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox1;
        private AxACTIVEXLib.AxCamMonitor axCamMonitor1;


    }
}