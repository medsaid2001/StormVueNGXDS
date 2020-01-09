using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StormVue2RTCM
{
    public partial class ConfigurationDlg : Form
    {
        private bool settingsChanged;

        private Form1 mainFrm;
        public ConfigurationDlg()
        {
            this.InitializeComponent();
        }

        public ConfigurationDlg(Form callingForm)
        {
            this.mainFrm = (callingForm as Form1);
            this.InitializeComponent();
        }

        private void SetConfigOptions()
        {
            string[] portNames = TxComHandler.GetPortNames();
            this.cbCOMPorts.Items.AddRange(portNames);
            this.cbCOMPorts.SelectedIndex = this.cbCOMPorts.FindStringExact(TxComHandler.hwPort.PortName);
            if (this.cbCOMPorts.SelectedIndex == -1 && this.cbCOMPorts.Items.Count > 0)
            {
                this.cbCOMPorts.SelectedIndex = 0;
            }
            ComboBox comboBox = this.cbBaudRate;
            ComboBox comboBox2 = this.cbBaudRate;
            int num = TxComHandler.hwPort.BaudRate;
            comboBox.SelectedIndex = comboBox2.FindStringExact(num.ToString());
            if (this.cbBaudRate.SelectedIndex == -1)
            {
                this.cbBaudRate.SelectedIndex = 1;
            }
            this.cbEnableComMessaging.Checked = Settings.comMsgEnabled;
            this.cbEnableStrikeMsgs.Checked = Settings.comStrikeMsgEnabled;
            this.txComStatus.Text = Settings.comPeriodicMsg;
            this.txComStrike.Text = Settings.comStrikeMsg;
            this.cbCOMPorts.Enabled = !Settings.comMsgEnabled;
            this.cbBaudRate.Enabled = !Settings.comMsgEnabled;
            this.lbP1StatusMsg.Text = "@P1 - Close Alarm Range (" + ((Settings.DistanceUnits == 0) ? "km)" : "mi)");
            this.lbP1StrikeMsg.Text = "@P1 - Strike distance (" + ((Settings.DistanceUnits == 0) ? "km)" : "mi)");
            this.txMailServer.Text = Settings.mServer;
            this.udPortNo.Value = Settings.mPort;
            this.cbConxSec.SelectedIndex = Settings.mSecIndex;
            this.cbAuth.SelectedIndex = Settings.mAuthIndex;
            this.txUsername.Text = Settings.mUsername;
            this.txPassword.Text = Settings.mPassword;
            this.txFrom.Text = Settings.mFrom;
            this.cbRecips.Items.AddRange(Settings.mToList.ToArray());
            if (this.cbRecips.Items.Count > 0)
            {
                this.cbRecips.SelectedIndex = 0;
            }
            this.txAlertSubj.Text = Settings.mAlarmSubj;
            this.txAlertMsg.Text = Settings.mAlarmBody;
            this.txClearSubj.Text = Settings.mClearSubj;
            this.txClearMsg.Text = Settings.mClearBody;
            TextBox textBox = this.txCloseStrikeRng;
            object text;
            if (Settings.DistanceUnits != 0)
            {
                num = GeoMath.kmToMilesRounded(Settings.CloseStrikeRangeKM);
                text = num.ToString();
            }
            else
            {
                text = Settings.CloseStrikeRangeKM.ToString(Util.ci);
            }
            textBox.Text = (string)text;
            this.txMinStrikerate.Text = Settings.MinCloseStrikeRate.ToString();
            this.txAllClearPeriod.Text = Settings.AllClearPeriod.ToString();
            this.cbDistanceUnits.SelectedIndex = Settings.DistanceUnits;
            this.cbSendStrikeAlert.Checked = Settings.mSendStrikeAlert;
            this.cbSendAllClear.Checked = Settings.mSendClear;
            this.cbTgmStrikeAlert.Checked = Settings.tSendStrikeAlert;
            this.cbTgmAllClear.Checked = Settings.tSendClear;
            this.txToken.Text = Settings.tBotToken;
            this.txDestinationID.Text = ((Settings.tDestId > 0) ? Settings.tDestId.ToString() : "");
            this.cbExportJSON.Checked = Settings.exportJSON;
            this.cbDatabase.Checked = Settings.storeDB;
            this.txDBPath.Text = Settings.dbPath;
        }

        private bool GetConfigOptions()
        {
            try
            {
                Settings.comMsgEnabled = this.cbEnableComMessaging.Checked;
                Settings.comStrikeMsgEnabled = this.cbEnableStrikeMsgs.Checked;
                Settings.comPeriodicMsg = this.txComStatus.Text;
                Settings.comStrikeMsg = this.txComStrike.Text;
                Settings.mServer = this.txMailServer.Text;
                Settings.mPort = Convert.ToInt32(this.udPortNo.Value);
                Settings.mSecIndex = this.cbConxSec.SelectedIndex;
                Settings.mAuthIndex = this.cbAuth.SelectedIndex;
                Settings.mUsername = this.txUsername.Text;
                Settings.mPassword = this.txPassword.Text;
                Settings.mFrom = this.txFrom.Text;
                Settings.mAlarmSubj = this.txAlertSubj.Text;
                Settings.mAlarmBody = this.txAlertMsg.Text;
                Settings.mClearSubj = this.txClearSubj.Text;
                Settings.mClearBody = this.txClearMsg.Text;
                Settings.mAlarmSubj = Settings.mAlarmSubj.Replace('|', ':');
                Settings.mAlarmBody = Settings.mAlarmBody.Replace('|', ':');
                Settings.mClearSubj = Settings.mClearSubj.Replace('|', ':');
                Settings.mClearBody = Settings.mClearBody.Replace('|', ':');
                Settings.tSendStrikeAlert = this.cbTgmStrikeAlert.Checked;
                Settings.tSendClear = this.cbTgmAllClear.Checked;
                Settings.tBotToken = this.txToken.Text;
                if (!string.IsNullOrEmpty(this.txDestinationID.Text) && !long.TryParse(this.txDestinationID.Text, out Settings.tDestId))
                {
                    throw new Exception("Destination ID invalid format (expected a number)");
                }
                if (!double.TryParse(this.txCloseStrikeRng.Text, NumberStyles.Float, (IFormatProvider)Util.ci, out Settings.CloseStrikeRangeKM))
                {
                    throw new Exception("Close strike range invalid format (expected a number)");
                }
                Settings.CloseStrikeRangeKM = ((Settings.DistanceUnits == 0) ? Settings.CloseStrikeRangeKM : ((double)GeoMath.miToKmsRounded(Settings.CloseStrikeRangeKM)));
                if (!int.TryParse(this.txMinStrikerate.Text, out Settings.MinCloseStrikeRate))
                {
                    throw new Exception("Minimum strike rate invalid format (expected a number)");
                }
                if (!int.TryParse(this.txAllClearPeriod.Text, out Settings.AllClearPeriod))
                {
                    throw new Exception("All clear period invalid format (expected a number)");
                }
                Settings.DistanceUnits = this.cbDistanceUnits.SelectedIndex;
                Settings.mSendStrikeAlert = this.cbSendStrikeAlert.Checked;
                Settings.mSendClear = this.cbSendAllClear.Checked;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }
            Settings.Save();
            return true;
        }

        private void cbDistanceUnits_SelectionChangeCommitted(object sender, EventArgs e)
        {
            Settings.DistanceUnits = this.cbDistanceUnits.SelectedIndex;
            this.txCloseStrikeRng.Text = ((Settings.DistanceUnits == 0) ? Settings.CloseStrikeRangeKM.ToString(Util.ci) : GeoMath.kmToMilesRounded(Settings.CloseStrikeRangeKM).ToString());
            this.lbP1StatusMsg.Text = "@P1 - Close Alarm Range (" + ((Settings.DistanceUnits == 0) ? "km)" : "mi)");
            this.lbP1StrikeMsg.Text = "@P1 - Strike distance (" + ((Settings.DistanceUnits == 0) ? "km)" : "mi)");
        }

        private void ConfigurationDlg_Load(object sender, EventArgs e)
        {
            this.SetConfigOptions();
            this.settingsChanged = false;
        }

        private void btApply_Click(object sender, EventArgs e)
        {
            if (this.GetConfigOptions())
            {
                this.settingsChanged = false;
            }
        }

        private void btTestSend_Click(object sender, EventArgs e)
        {
            this.mainFrm.TestSendAlert(true);
        }

        private void btAddRecip_Click(object sender, EventArgs e)
        {
            string text = this.cbRecips.Text;
            if (!text.Contains('@'))
            {
                MessageBox.Show(text + " is not a valid e-mail address", "Address format error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                this.cbRecips.Focus();
            }
            else
            {
                this.cbRecips.Items.Add(text);
                Settings.mToList.Add(this.cbRecips.Text);
                MessageBox.Show(text + " succesfully added to alert recipient list", "New e-mail address added", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                Settings.Save();
                if (this.cbRecips.Items.Count > 30)
                {
                    MessageBox.Show("You have " + this.cbRecips.Items.Count.ToString() + " alert recipients. To maintain optimal performance please consider using an external mailing list instead.", "Performance warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        private void btDelRecip_Click(object sender, EventArgs e)
        {
            if (this.cbRecips.Items.Count >= 1)
            {
                string text = this.cbRecips.Text;
                try
                {
                    Settings.mToList.Remove(text);
                    this.cbRecips.Items.Remove(this.cbRecips.SelectedItem);
                    Settings.Save();
                    MessageBox.Show(text + " removed from recipient list", "Address removed", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not remove " + text + ". Error details: " + ex.Message, "Address removal error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                finally
                {
                    this.cbRecips.Text = "";
                }
            }
        }

        private void cbEnableComMessaging_Click(object sender, EventArgs e)
        {
            Settings.comMsgEnabled = this.cbEnableComMessaging.Checked;
            if (Settings.comMsgEnabled)
            {
                this.cbCOMPorts.Enabled = false;
                this.cbBaudRate.Enabled = false;
                this.mainFrm.ConnectBoard();
            }
            else
            {
                this.mainFrm.DisconnectBoard();
                this.cbCOMPorts.Enabled = true;
                this.cbBaudRate.Enabled = true;
            }
        }

        private void cbEnableStrikeMsgs_CheckedChanged(object sender, EventArgs e)
        {
            Settings.comStrikeMsgEnabled = this.cbEnableStrikeMsgs.Checked;
        }

        private void cbCOMPorts_SelectionChangeCommitted(object sender, EventArgs e)
        {
            try
            {
                TxComHandler.hwPort.PortName = ((this.cbCOMPorts.SelectedIndex != -1) ? this.cbCOMPorts.SelectedItem.ToString() : "COM1");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Set COM port error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void cbBaudRate_SelectionChangeCommitted(object sender, EventArgs e)
        {
            try
            {
                TxComHandler.hwPort.BaudRate = Convert.ToInt32(this.cbBaudRate.SelectedItem);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Set baud rate error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void nudComMsgInterval_ValueChanged(object sender, EventArgs e)
        {
            Settings.comPeriodicIntervalSecs = Convert.ToInt32(this.nudComMsgInterval.Value);
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            this.settingsChanged = true;
        }

        private void checkBox_Click(object sender, EventArgs e)
        {
        }

        private void ConfigurationDlg_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.settingsChanged && MessageBox.Show("Some settings have changed, are you sure you want to exit? All changes will be lost. To save changes click Apply button before closing Properties dialog.", "Warning!", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        private void cbExportJSON_Click(object sender, EventArgs e)
        {
            Settings.exportJSON = !Settings.exportJSON;
            this.cbExportJSON.Checked = Settings.exportJSON;
        }

        private void cbDatabase_Click(object sender, EventArgs e)
        {
            Settings.storeDB = !Settings.storeDB;
            this.cbDatabase.Checked = Settings.storeDB;
            if (Settings.storeDB)
            {
                this.mainFrm.StartDBStorage();
            }
            else
            {
                this.mainFrm.StopDBStorage();
            }
        }

        private void btSelDBPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                Settings.dbPath = folderBrowserDialog.SelectedPath;
            }
            this.txDBPath.Text = Settings.dbPath;
        }

        private void btRefreshCOM_Click(object sender, EventArgs e)
        {
            string[] portNames = TxComHandler.GetPortNames();
            this.cbCOMPorts.Items.AddRange(portNames);
            this.cbCOMPorts.SelectedIndex = this.cbCOMPorts.FindStringExact(TxComHandler.hwPort.PortName);
            if (this.cbCOMPorts.SelectedIndex == -1 && this.cbCOMPorts.Items.Count > 0)
            {
                this.cbCOMPorts.SelectedIndex = 0;
            }
        }

        private async void btTestTelegram_Click(object sender, EventArgs e)
        {
            await this.mainFrm.TestSendTelegram(true);
        }

        
    }
}
