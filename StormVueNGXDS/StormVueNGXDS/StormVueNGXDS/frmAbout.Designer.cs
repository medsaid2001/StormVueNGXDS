namespace StormVue2RTCM
{
    partial class frmAbout
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
            this.lbAlertExpires = new System.Windows.Forms.Label();
            this.lbRegistered = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lbCopyright = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lbSWVer = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lbAlertExpires
            // 
            this.lbAlertExpires.AutoSize = true;
            this.lbAlertExpires.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbAlertExpires.ForeColor = System.Drawing.Color.DarkRed;
            this.lbAlertExpires.Location = new System.Drawing.Point(105, 105);
            this.lbAlertExpires.Name = "lbAlertExpires";
            this.lbAlertExpires.Size = new System.Drawing.Size(210, 13);
            this.lbAlertExpires.TabIndex = 15;
            this.lbAlertExpires.Text = "NOTE: This ALPHA release expires ";
            // 
            // lbRegistered
            // 
            this.lbRegistered.AutoSize = true;
            this.lbRegistered.Location = new System.Drawing.Point(212, 43);
            this.lbRegistered.Name = "lbRegistered";
            this.lbRegistered.Size = new System.Drawing.Size(21, 13);
            this.lbRegistered.TabIndex = 14;
            this.lbRegistered.Text = "No";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(105, 43);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(58, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "Registered";
            // 
            // lbCopyright
            // 
            this.lbCopyright.AutoSize = true;
            this.lbCopyright.Location = new System.Drawing.Point(212, 67);
            this.lbCopyright.Name = "lbCopyright";
            this.lbCopyright.Size = new System.Drawing.Size(99, 13);
            this.lbCopyright.TabIndex = 12;
            this.lbCopyright.Text = "Astrogenic Systems";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(105, 67);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Copyright";
            // 
            // lbSWVer
            // 
            this.lbSWVer.AutoSize = true;
            this.lbSWVer.Location = new System.Drawing.Point(212, 19);
            this.lbSWVer.Name = "lbSWVer";
            this.lbSWVer.Size = new System.Drawing.Size(31, 13);
            this.lbSWVer.TabIndex = 10;
            this.lbSWVer.Text = "0.0.0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(105, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Software version";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(237, 137);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 8;
            this.button1.Text = "Close";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // frmAbout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(420, 178);
            this.Controls.Add(this.lbAlertExpires);
            this.Controls.Add(this.lbRegistered);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lbCopyright);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lbSWVer);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.Name = "frmAbout";
            this.Text = "frmAbout";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbAlertExpires;
        private System.Windows.Forms.Label lbRegistered;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lbCopyright;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lbSWVer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
    }
}