namespace StormVue2RTCM
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Net;
    using System.Windows.Forms;

    public class frmLicenseCheck : Form
    {
        public static string orderNumber = "";
        private IContainer components;
        private Label label1;
        private TextBox txOrderNo;
        private Button btCheck;

        public frmLicenseCheck()
        {
            this.InitializeComponent();
            this.txOrderNo.Text = orderNumber;
        }

        private void btCheck_Click(object sender, EventArgs e)
        {
            orderNumber = this.txOrderNo.Text;
            if (string.IsNullOrEmpty(orderNumber))
            {
                MessageBox.Show("Please enter a valid order number", "Licence status check", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else
            {
                this.CheckActivationStatus(orderNumber);
            }
        }

        private void CheckActivationStatus(string orderno)
        {
            string str = "?order=" + orderno.Trim() + "&lcc=1";
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(frmActivator.baseURL + str);
            string str2 = "";
            try
            {
                request.MaximumAutomaticRedirections = 4;
                request.Timeout = 0x1f40;
                str2 = new StreamReader(((HttpWebResponse) request.GetResponse()).GetResponseStream()).ReadToEnd();
            }
            catch (WebException exception1)
            {
                MessageBox.Show(exception1.Message, "Licence status check general error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            if (string.IsNullOrEmpty(str2))
            {
                MessageBox.Show("No response from server, check network, firewall or proxy server settings!", "Licence status check", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else
            {
                try
                {
                    char[] separator = new char[] { ':' };
                    string[] strArray = str2.Trim().Split(separator);
                    if ((strArray.Length == 2) && strArray[1].ToUpper().StartsWith("ACTREMAIN "))
                    {
                        char[] chArray2 = new char[] { ' ' };
                        string[] strArray2 = strArray[1].Split(chArray2);
                        string str3 = "";
                        if ((strArray2.Length == 2) && (strArray2[1] == "0"))
                        {
                            str3 = "\n\nContact techsupport@astrogenic.com if you need additional activations";
                        }
                        MessageBox.Show("License valid, remaining activations: " + strArray2[1] + str3, "Licence status check", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                    else if ((strArray.Length == 2) && strArray[1].ToUpper().Equals("NOLICDATA"))
                    {
                        MessageBox.Show("No license data found for your system", "Licence status check", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                    else
                    {
                        MessageBox.Show("An error occured while checking license status", "Licence status check", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    }
                }
                catch (Exception exception2)
                {
                    MessageBox.Show(exception2.Message, "Licence status check general error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                base.Close();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.label1 = new Label();
            this.txOrderNo = new TextBox();
            this.btCheck = new Button();
            base.SuspendLayout();
            this.label1.AutoSize = true;
            this.label1.Location = new Point(12, 0x13);
            this.label1.Name = "label1";
            this.label1.Size = new Size(0x75, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Purchase order number";
            this.txOrderNo.Location = new Point(15, 0x23);
            this.txOrderNo.Name = "txOrderNo";
            this.txOrderNo.Size = new Size(0xf9, 20);
            this.txOrderNo.TabIndex = 2;
            this.btCheck.Location = new Point(0xbd, 0x47);
            this.btCheck.Name = "btCheck";
            this.btCheck.Size = new Size(0x4b, 0x17);
            this.btCheck.TabIndex = 5;
            this.btCheck.Text = "Check";
            this.btCheck.UseVisualStyleBackColor = true;
            this.btCheck.Click += new EventHandler(this.btCheck_Click);
            base.AutoScaleDimensions = new SizeF(96f, 96f);
            base.AutoScaleMode = AutoScaleMode.Dpi;
            base.ClientSize = new Size(0x114, 0x68);
            base.Controls.Add(this.btCheck);
            base.Controls.Add(this.label1);
            base.Controls.Add(this.txOrderNo);
            base.FormBorderStyle = FormBorderStyle.FixedSingle;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "frmLicenseCheck";
            base.ShowIcon = false;
            this.Text = "License status check";
            base.TopMost = true;
            base.ResumeLayout(false);
            base.PerformLayout();
        }
    }
}

