namespace StormVue2RTCM
{
    partial class FrmSyslog
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
            this.btSave = new System.Windows.Forms.Button();
            this.listboxEvents = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // btSave
            // 
            this.btSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btSave.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btSave.Location = new System.Drawing.Point(404, 340);
            this.btSave.Name = "btSave";
            this.btSave.Size = new System.Drawing.Size(134, 26);
            this.btSave.TabIndex = 3;
            this.btSave.Text = "   Save log to textfile";
            this.btSave.UseVisualStyleBackColor = true;
            this.btSave.Click += new System.EventHandler(this.btSave_Click);
            // 
            // listboxEvents
            // 
            this.listboxEvents.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listboxEvents.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listboxEvents.FormattingEnabled = true;
            this.listboxEvents.HorizontalScrollbar = true;
            this.listboxEvents.Location = new System.Drawing.Point(2, 3);
            this.listboxEvents.Name = "listboxEvents";
            this.listboxEvents.Size = new System.Drawing.Size(536, 329);
            this.listboxEvents.TabIndex = 2;
            // 
            // FrmSyslog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(541, 369);
            this.Controls.Add(this.btSave);
            this.Controls.Add(this.listboxEvents);
            this.Name = "FrmSyslog";
            this.Text = "FrmSyslog";
           // this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmSyslog_FormClosing);
            this.VisibleChanged += new System.EventHandler(this.FrmSyslog_VisibleChanged);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btSave;
        private System.Windows.Forms.ListBox listboxEvents;
    }
}