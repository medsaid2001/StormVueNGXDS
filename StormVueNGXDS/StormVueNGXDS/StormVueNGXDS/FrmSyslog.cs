using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StormVue2RTCM
{
    public partial class FrmSyslog : Form
    {
        private Timer updTmr = new Timer();
        public FrmSyslog()
        {
            InitializeComponent();
        }

        private void updTmr_Tick(object myObject, EventArgs myEventArgs)
        {
            this.UpdateSysLog();
        }

        private void UpdateSysLog()
        {
            try
            {
                this.listboxEvents.BeginUpdate();
                this.listboxEvents.Items.Clear();
                this.listboxEvents.Items.AddRange(Syslogger.GetMessages());
                if (this.listboxEvents.Items.Count > 1)
                {
                    this.listboxEvents.SetSelected(this.listboxEvents.Items.Count - 1, true);
                    this.listboxEvents.SetSelected(this.listboxEvents.Items.Count - 1, false);
                }
                this.listboxEvents.EndUpdate();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private void FrmSyslog_Load(object sender, EventArgs e)
        {
            this.UpdateSysLog();
            this.updTmr.Tick += this.updTmr_Tick;
            this.updTmr.Interval = 500;
            this.updTmr.Start();
        }

        private void SaveLogToFile()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text File|*.txt";
            saveFileDialog.Title = "Save Event log to textfile";
            saveFileDialog.ShowDialog();
            if (saveFileDialog.FileName != "")
            {
                string[] messages = Syslogger.GetMessages();
                try
                {
                    using (StreamWriter streamWriter = new StreamWriter(saveFileDialog.FileName, false))
                    {
                        streamWriter.WriteLine("StormVue NGX Data Server  - Event log created at " + DateTime.Now.ToString());
                        string[] array = messages;
                        foreach (string value in array)
                        {
                            streamWriter.WriteLine(value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Save file failed:" + ex.Message, "File error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }
        }

        private void FrmSyslog_VisibleChanged(object sender, EventArgs e)
        {
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            this.updTmr.Stop();
        }

        private void btSave_Click(object sender, EventArgs e)
        {
            this.SaveLogToFile();
        }

     
    }
}
