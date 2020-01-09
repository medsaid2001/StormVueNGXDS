namespace StormVue2RTCM
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Net;
    using System.Windows.Forms;

    public class frmActivator : Form
    {
        public static string baseURL = "http://astrogenic.com/ngxregproc2.php";
        private string cmd = "?mid=xxxxxx&serial=yyyyyy&order=zzzzzz";
        private IContainer components;
        private TextBox txEmail;
        private Label label1;
        private Label label2;
        private TextBox txKey;
        private Button btActivate;
        private Button btRegSrvStatus;

        public frmActivator()
        {
            this.InitializeComponent();
            this.btActivate.Enabled = false;
            new ToolTip { ShowAlways = true }.SetToolTip(this.txEmail, "Enter your purchase order number");
            new ToolTip { ShowAlways = true }.SetToolTip(this.txKey, "Enter the activation key that you received by e-mail after completing your order");
        }

        private void btActivate_Click(object sender, EventArgs e)
        {
            this.btActivate.Enabled = false;
            this.CollectActivate();
        }

        private void btRegSrvStatus_Click(object sender, EventArgs e)
        {
            this.CheckServerComms();
        }

        private void CheckServerComms()
        {
            this.btRegSrvStatus.Enabled = false;
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(baseURL + "?comms=1");
            string str = "";
            try
            {
                request.MaximumAutomaticRedirections = 4;
                request.Timeout = 0x1f40;
                str = new StreamReader(((HttpWebResponse) request.GetResponse()).GetResponseStream()).ReadToEnd();
            }
            catch (WebException exception1)
            {
                MessageBox.Show(exception1.Message, "Activation server general error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            if (string.IsNullOrEmpty(str))
            {
                MessageBox.Show("Server communication failed, check network, firewall or proxy server settings!", "Activation server communications check", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                this.btRegSrvStatus.Enabled = true;
            }
            else
            {
                char[] separator = new char[] { ':' };
                if (str.Trim().Split(separator)[1].ToUpper() == "OK")
                {
                    this.btActivate.Enabled = true;
                    this.btRegSrvStatus.Enabled = false;
                    MessageBox.Show("Server communication working, activation now available", "Activation server communications check", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            }
        }

        private void CollectActivate()
        {
            string text = this.txEmail.Text;
            string str2 = this.txKey.Text;
            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show("Please enter order number", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                this.txEmail.Focus();
            }
            else if (string.IsNullOrEmpty(str2))
            {
                MessageBox.Show("Please enter activation key", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                this.txKey.Focus();
            }
            else
            {
                this.ConnectToServer(str2, text);
            }
        }

        private void ConnectToServer(string key, string email)
        {
            MachineIdent ident = new MachineIdent();
            string str = this.cmd.Replace("xxxxxx", ident.get()).Replace("yyyyyy", key).Replace("zzzzzz", email);
            Console.WriteLine("Query:" + str);
            string dataStr = "";
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(baseURL + str);
            try
            {
                request.MaximumAutomaticRedirections = 4;
                request.Timeout = 0x1f40;
                dataStr = new StreamReader(((HttpWebResponse) request.GetResponse()).GetResponseStream()).ReadToEnd();
            }
            catch (WebException exception1)
            {
                MessageBox.Show(exception1.Message, "Server communication error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            this.ParseResponse(dataStr);
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
            this.txEmail = new TextBox();
            this.label1 = new Label();
            this.label2 = new Label();
            this.txKey = new TextBox();
            this.btActivate = new Button();
            this.btRegSrvStatus = new Button();
            base.SuspendLayout();
            this.txEmail.Location = new Point(12, 0x20);
            this.txEmail.Name = "txEmail";
            this.txEmail.Size = new Size(0xf9, 20);
            this.txEmail.TabIndex = 0;
            this.label1.AutoSize = true;
            this.label1.Location = new Point(9, 0x10);
            this.label1.Name = "label1";
            this.label1.Size = new Size(0x75, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Purchase order number";
            this.label2.AutoSize = true;
            this.label2.Location = new Point(9, 0x3e);
            this.label2.Name = "label2";
            this.label2.Size = new Size(0x4a, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Activation key";
            this.txKey.Location = new Point(12, 0x4e);
            this.txKey.Name = "txKey";
            this.txKey.Size = new Size(0xf9, 20);
            this.txKey.TabIndex = 2;
            this.btActivate.Location = new Point(0xba, 0x74);
            this.btActivate.Name = "btActivate";
            this.btActivate.Size = new Size(0x4b, 0x17);
            this.btActivate.TabIndex = 4;
            this.btActivate.Text = "Activate";
            this.btActivate.UseVisualStyleBackColor = true;
            this.btActivate.Click += new EventHandler(this.btActivate_Click);
            this.btRegSrvStatus.Location = new Point(12, 0x74);
            this.btRegSrvStatus.Name = "btRegSrvStatus";
            this.btRegSrvStatus.Size = new Size(0xa8, 0x17);
            this.btRegSrvStatus.TabIndex = 5;
            this.btRegSrvStatus.Text = "Registration server link status";
            this.btRegSrvStatus.UseVisualStyleBackColor = true;
            this.btRegSrvStatus.Click += new EventHandler(this.btRegSrvStatus_Click);
            base.AutoScaleDimensions = new SizeF(96f, 96f);
            base.AutoScaleMode = AutoScaleMode.Dpi;
            base.ClientSize = new Size(0x116, 150);
            base.Controls.Add(this.btRegSrvStatus);
            base.Controls.Add(this.btActivate);
            base.Controls.Add(this.label2);
            base.Controls.Add(this.txKey);
            base.Controls.Add(this.label1);
            base.Controls.Add(this.txEmail);
            base.FormBorderStyle = FormBorderStyle.FixedSingle;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.Name = "frmActivator";
            base.ShowIcon = false;
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Activate full version";
            base.TopMost = true;
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private bool ParseResponse(string dataStr)
        {
            if (string.IsNullOrEmpty(dataStr))
            {
                return false;
            }
            dataStr = dataStr.Trim();
            char[] separator = new char[] { ':' };
            string[] strArray = dataStr.Split(separator);
            if (strArray[1].ToUpper() == "ALREADY ACTIVATED")
            {
                MessageBox.Show("Exceeded max number of activations for this key.\n\nPlease contact support at techsupport@astrogenic.com", "Invalid activation key", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (strArray[1].ToUpper() == "DB UPDATE FAIL")
            {
                MessageBox.Show("An error occured while updating the database. Please try again later", "Database error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (strArray[1].ToUpper() == "NO MATCH FOUND")
            {
                MessageBox.Show("Invalid activation key or order number. Please verify both fields are correct", "Invalid data", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (strArray[1].ToUpper() == "NO PARAMS")
            {
                MessageBox.Show("Unexpected communication problem, try again later", "Parameters not found", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (strArray[1].ToUpper().StartsWith("EXCEPTION"))
            {
                MessageBox.Show("Server error, please contact support and report the following message: " + strArray[1], "Critical error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            if (strArray[1].ToUpper().StartsWith("OK "))
            {
                char[] chArray2 = new char[] { ' ' };
                string[] strArray2 = strArray[1].Split(chArray2);
                Console.WriteLine("R13 unlock:" + strArray2[1]);
                Console.WriteLine("No enc unlock:" + EncryptionEngine.R13(strArray2[1]));
                new LicenseEngine().WriteLicenseDataISO(strArray2[1]);
                if (Settings.licenseValid)
                {
                    Settings.reactivationFlag = Settings.REACTFLAG.REGISTERED;
                    MessageBox.Show("Activation complete, expiration date has been removed", "Thanks for supporting Astrogenic Systems :-)", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    base.Close();
                }
            }
            Console.WriteLine(dataStr);
            this.btActivate.Enabled = true;
            return true;
        }
    }
}

