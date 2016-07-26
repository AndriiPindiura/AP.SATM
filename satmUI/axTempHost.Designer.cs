namespace AP.SATM.Eyes
{
    partial class axTempHost
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(axTempHost));
            this.axCamMonitor1 = new AxACTIVEXLib.AxCamMonitor();
            ((System.ComponentModel.ISupportInitialize)(this.axCamMonitor1)).BeginInit();
            this.SuspendLayout();
            // 
            // axCamMonitor1
            // 
            this.axCamMonitor1.Enabled = true;
            this.axCamMonitor1.Location = new System.Drawing.Point(5, 5);
            this.axCamMonitor1.Name = "axCamMonitor1";
            this.axCamMonitor1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axCamMonitor1.OcxState")));
            this.axCamMonitor1.Size = new System.Drawing.Size(277, 253);
            this.axCamMonitor1.TabIndex = 0;
            // 
            // axTempHost
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.axCamMonitor1);
            this.Name = "axTempHost";
            this.Text = "axTempHost";
            ((System.ComponentModel.ISupportInitialize)(this.axCamMonitor1)).EndInit();
            this.ResumeLayout(false);

        }








        #endregion

        private AxACTIVEXLib.AxCamMonitor axCamMonitor1;
    }
}