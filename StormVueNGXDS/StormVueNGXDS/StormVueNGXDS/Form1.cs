using FTP32;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace StormVue2RTCM
{
    public partial class Form1 : Form
    {
        private FrmSyslog fSyslog = new FrmSyslog();

        private frmAbout fAbout = new frmAbout();

        private frmActivator fActivator = new frmActivator();

        private frmLicenseCheck fLicCheck = new frmLicenseCheck();

        private ConfigurationDlg cfgDialog;

        private List<StrikeData> strikeBuffer;

        private List<StrikeData> strikeRealtime;

        private List<StrikeData> strikeRealtimeDB;

        private List<StormData> tracBuffer;

        private List<StormData> tracRealtime;

        private long uid;

        private RTUploader rtUploader;

        private System.Windows.Forms.Timer rtTimer = new System.Windows.Forms.Timer();

        private System.Windows.Forms.Timer dbInsertTimer = new System.Windows.Forms.Timer();

        private System.Windows.Forms.Timer closeAlarmTmr = new System.Windows.Forms.Timer();

        private System.Windows.Forms.Timer comMsgTmr = new System.Windows.Forms.Timer();

        private int arcUploadTmr;

        private DateTime virtualDT;

        private TimeSpan deltaTime;

        private TimeSpan dataAge;

        private AlertMessaging alertMsg;

        public static DateTime expires = new DateTime(2012, 10, 1, 1, 0, 0);

        private bool isExpired;

        private System.Windows.Forms.Timer uiUpdateTmr;

        private System.Windows.Forms.Timer chkExpTmr;

        private System.Windows.Forms.Timer reactTmr;

        private int lastIPCCheck;

        private long nowMS;

        private Random rndStrikeEvent;

        private Random rUnique;

        private Thread m_FG1Thread;

        private Thread m_FG2Thread;

        private Thread m_FTPThread;

        private ManualResetEvent m_FG1EventStopThread;

        private ManualResetEvent m_FG1EventThreadStopped;

        private ManualResetEvent m_FG2EventStopThread;

        private ManualResetEvent m_FG2EventThreadStopped;

        private ManualResetEvent m_FTPEventStopThread;

        private ManualResetEvent m_FTPEventThreadStopped;

        public DelegateFG1DataRec m_DelegateFG1DataRec;

        public DelegateFG1ThreadFinished m_DelegateFG1ThreadFinished;

        public DelegateFG2DataRec m_DelegateFG2DataRec;

        public DelegateFG2ThreadFinished m_DelegateFG2ThreadFinished;

        public DelegateFTPThreadFinished m_DelegateFTPThreadFinished;

        public FTPUploadComplete m_FTPUploadComplete;

        public FTPThreadError m_FTPThreadError;

        private bool IsFTPBlocked;

        private Counter errorCounter;

        private Counter strikeRateCounter;

        private Counter closeStrikeRateCounter;

        private long lastCloseLightningTimestamp;

        private string FTPStopReason;

        private Thread TArcUploader;

        private Stopwatch sw;

        private int dbErrors;
        public bool closeAlarmActive
        {
            get;
            set;
        }
        public Form1()
        {
            DateTime now = DateTime.Now;
            this.virtualDT = now.AddDays(-0.7);
            this.deltaTime = new TimeSpan(0, 0, 10);
            this.dataAge = new TimeSpan(0, 4, 0);
            this.uiUpdateTmr = new System.Windows.Forms.Timer();
            this.chkExpTmr = new System.Windows.Forms.Timer();
            this.reactTmr = new System.Windows.Forms.Timer();
            this.rndStrikeEvent = new Random();
            now = DateTime.Now;
            this.rUnique = new Random(Convert.ToInt32(now.Ticks / 2147483647));
            this.FTPStopReason = "";
            this.sw = new Stopwatch();
            InitializeComponent();
            TimeZone.CurrentTimeZone.GetUtcOffset(this.virtualDT);
        }
        private void InitApp()
        {
            this.IsFTPBlocked = false;
            Settings.ftpCfg.server = "127.0.0.1";
            Settings.ftpCfg.port = 21;
            Settings.ftpCfg.username = "user";
            Settings.ftpCfg.password = "password";
            Settings.ftpCfg.conmode = FTP.ConnectionModes.Active;
            this.errorCounter = new Counter(10);
            this.strikeRateCounter = new Counter(1);
            this.closeStrikeRateCounter = new Counter(1);
            new LicenseEngine().StartupCheck();
            this.comboDetector.SelectedIndex = 0;
            Settings.Load();
            this.UpdateConfiguration();
            this.alertMsg = new AlertMessaging(this);
            this.m_DelegateFG1DataRec = this.FG1DataRec;
            this.m_DelegateFG1ThreadFinished = this.FG1ThreadFinished;
            this.m_DelegateFG2DataRec = this.FG2DataRec;
            this.m_DelegateFG2ThreadFinished = this.FG2ThreadFinished;
            this.m_FG1EventStopThread = new ManualResetEvent(false);
            this.m_FG1EventThreadStopped = new ManualResetEvent(false);
            this.m_FG2EventStopThread = new ManualResetEvent(false);
            this.m_FG2EventThreadStopped = new ManualResetEvent(false);
            this.m_FTPEventStopThread = new ManualResetEvent(false);
            this.m_FTPEventThreadStopped = new ManualResetEvent(false);
            this.m_DelegateFTPThreadFinished = this.FTPThreadFinished;
            this.m_FTPUploadComplete = this.FTPUploadCompleteEvent;
            this.m_FTPThreadError = this.FTPThreadErrorEvent;
            this.strikeRealtime = new List<StrikeData>();
            this.strikeRealtimeDB = new List<StrikeData>();
            this.strikeBuffer = new List<StrikeData>();
            this.tracRealtime = new List<StormData>();
            this.tracBuffer = new List<StormData>();
            Settings.transferPaused = false;
            Settings.transferStopped = false;
            this.SetUploadStatus();
            this.SetIPCStatus(false, false);
            this.rtTimer.Tick += this.rtTimer_Tick;
            this.rtTimer.Interval = Convert.ToInt32(TimeEx.secondsToMillis((double)Settings.rtUploadInterval));
            this.dbInsertTimer.Tick += this.dbInsertTimer_Tick;
            this.dbInsertTimer.Interval = 5000;
            if (Settings.storeDB)
            {
                this.StartDBStorage();
            }
            this.comMsgTmr.Tick += this.comMsgTmr_Tick;
            if (Settings.comMsgEnabled)
            {
                this.ConnectBoard();
            }
            this.uiUpdateTmr.Tick += this.uiUpdateTmr_Elapsed;
            this.uiUpdateTmr.Interval = 500;
            this.uiUpdateTmr.Start();
            this.lastIPCCheck = 0;
            this.chkExpTmr.Tick += this.chkExpTmr_Elapsed;
            this.chkExpTmr.Interval = 120000;
            this.chkExpTmr.Start();
            this.CheckExpires();
            this.reactTmr.Tick += this.reactTmr_Elapsed;
            this.reactTmr.Interval = 1000;
            this.arcUploadTmr = 25;
            this.StartIPCThreads();
        }

        private void CheckExpires()
        {
             
                Settings.reactivationFlag = Settings.REACTFLAG.REGISTERED;
                
        }

        private void Reactivate()
        {
            this.reactTmr.Stop();
            Settings.reactivationFlag = Settings.REACTFLAG.OFF;
            this.StartIPCThreads();
            this.StartFTPUploadThread();
            this.uiUpdateTmr.Start();
        }

        private void ActivateFTPLogic()
        {
            int num = this.strikeRateCounter.CurrentCount();
            if (this.m_FTPThread != null)
            {
                if (num < Settings.idleLimiter)
                {
                    Syslogger.AddMsg("ActivateFTPLogic()", "Upload stopped, strike rate below minimum");
                    this.StopFTPUploadThread();
                    this.FTPStopReason = "(System)";
                    this.btStartStop.Text = "Start";
                }
            }
            else if (num <= Settings.rateLimiter && num >= Settings.idleLimiter && this.FTPStopReason != "(User)")
            {
                Syslogger.AddMsg("ActivateFTPLogic()", "Upload enabled, strike rate within min/max range");
                if (this.m_FTPThread == null)
                {
                    this.StartFTPUploadThread();
                }
                this.FTPStopReason = "";
                this.btStartStop.Text = "Stop";
            }
        }

        private void UpdateConfiguration()
        {
            this.txFTPServer.Text = Settings.ftpCfg.server;
            this.txFTPPort.Text = Settings.ftpCfg.port.ToString();
            this.txUsername.Text = Settings.ftpCfg.username;
            this.txPassword.Text = Settings.ftpCfg.password;
            this.txTargetDir.Text = Settings.remoteStorePath;
            this.txLon.Text = Settings.Lon.ToString("0.000", Util.ci);
            this.txLat.Text = Settings.Lat.ToString("0.000", Util.ci);
            this.cbConMode.SelectedIndex = ((Settings.ftpCfg.conmode != 0) ? 1 : 0);
            this.nudRTInterval.Value = Settings.rtUploadInterval;
            this.nudArcInterval.Value = Settings.arcUploadInterval;
            this.nudMinStrikeRate.Value = Settings.idleLimiter;
            this.nudMaxStrikeRate.Value = Settings.rateLimiter;
            this.nudMaxArcRecs.Value = Settings.maxArcRecords;
            this.comboDetector.SelectedIndex = Settings.detectorModel;
            this.cbFileCopyMode.Checked = Settings.fileCopyMode;
            this.mnuStartOnLogin.Checked = Settings.startOnLogin;
            this.SetControlStatus();
            this.LoadRegNXVersion();
        }

        private void LoadRegLatitude()
        {
            bool flag = false;
            try
            {
                using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Classes\\VirtualStore\\MACHINE\\SOFTWARE\\Astrogenic\\NexStorm\\Config\\"))
                {
                    Settings.Lat = BitConverter.ToDouble((byte[])registryKey.GetValue("Latitude"), 0);
                    flag = true;
                }
            }
            catch
            {
                Syslogger.AddMsg("LoadRegLatitude()", "Unable to load latitude (HKEY_CURRENT_USER)");
            }
            if (!flag)
            {
                try
                {
                    using (RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey("Software\\Astrogenic\\NexStorm\\Config\\"))
                    {
                        Settings.Lat = BitConverter.ToDouble((byte[])registryKey2.GetValue("Latitude"), 0);
                        flag = true;
                    }
                }
                catch
                {
                    Syslogger.AddMsg("LoadRegLatitude()", "Unable to load latitude (HKEY_LOCAL_MACHINE)");
                }
            }
            this.txLat.Text = Settings.Lat.ToString(Util.ci);
        }

        private void LoadRegLongitude()
        {
            if (Settings.Lon == -999999.0)
            {
                bool flag = false;
                try
                {
                    using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Classes\\VirtualStore\\MACHINE\\SOFTWARE\\Astrogenic\\NexStorm\\Config\\"))
                    {
                        Settings.Lon = BitConverter.ToDouble((byte[])registryKey.GetValue("Longitude"), 0);
                        flag = true;
                    }
                }
                catch
                {
                    Syslogger.AddMsg("LoadRegLongitude()", "Unable to load latitude (HKEY_CURRENT_USER)");
                }
                if (!flag)
                {
                    try
                    {
                        using (RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey("Software\\Astrogenic\\NexStorm\\Config\\"))
                        {
                            Settings.Lon = BitConverter.ToDouble((byte[])registryKey2.GetValue("Longitude"), 0);
                        }
                    }
                    catch (Exception)
                    {
                        Syslogger.AddMsg("LoadRegLongitude()", "Unable to load longitude (HKEY_LOCAL_MACHINE)");
                    }
                }
            }
            this.txLon.Text = Settings.Lon.ToString(Util.ci);
        }

        private void LoadRegNXVersion()
        {
            try
            {
                using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Classes\\VirtualStore\\MACHINE\\SOFTWARE\\Astrogenic\\NexStorm\\Config\\"))
                {
                    HeaderData.swVersion = (string)registryKey.GetValue("AppVersion", "0.0.0.0");
                }
            }
            catch
            {
                Syslogger.AddMsg("LoadRegNXVersion()", "Unable to load software version (HKEY_CURRENT_USER)");
            }
            if (HeaderData.swVersion == "0.0.0.0")
            {
                try
                {
                    using (RegistryKey registryKey2 = Registry.LocalMachine.OpenSubKey("Software\\Astrogenic\\NexStorm\\Config\\"))
                    {
                        HeaderData.swVersion = (string)registryKey2.GetValue("AppVersion", "0.0.0.0");
                    }
                }
                catch
                {
                    Syslogger.AddMsg("LoadRegNXVersion()", "Unable to load software version (HKEY_LOCAL_MACHINE)");
                }
            }
            if (HeaderData.swVersion == "0.0.0.0")
            {
                try
                {
                    using (RegistryKey registryKey3 = Registry.LocalMachine.OpenSubKey("Software\\WOW6432Node\\Astrogenic\\NexStorm\\Config\\"))
                    {
                        HeaderData.swVersion = (string)registryKey3.GetValue("AppVersion", "0.0.0.0");
                    }
                }
                catch
                {
                    Syslogger.AddMsg("LoadRegNXVersion()", "Unable to load software version (HKEY_LOCAL_MACHINE)");
                }
            }
            this.lbNXVer.Text = HeaderData.swVersion;
        }

        private void rtTimer_Tick(object myObject, EventArgs myEventArgs)
        {
            this.rtTimer.Stop();
            this.WriteRealtimeData();
            if (this.m_FTPThread != null)
            {
                if (this.m_FTPThread.IsAlive)
                {
                    this.rtUploader.SetFTPConnectInfo(Settings.ftpCfg);
                    this.rtUploader.Activate = true;
                }
                if (!Settings.transferPaused && this.m_FTPThread.IsAlive && ++this.arcUploadTmr > TimeEx.minutesToMillis(Settings.arcUploadInterval) / this.rtTimer.Interval)
                {
                    this.arcUploadTmr = 0;
                    this.UploadArchive();
                    Syslogger.Trim();
                }
            }
        }

        private void WriteRealtimeData()
        {
            int num = 0;
            int num2 = 0;
            if (this.strikeRealtime != null)
            {
                Monitor.Enter(this.strikeRealtime);
                Monitor.Enter(this.tracRealtime);
                Monitor.Enter(this.strikeBuffer);
                try
                {
                    using (StreamWriter streamWriter = new StreamWriter(Settings.locSrcRealtime))
                    {
                        string text = "STTRC";
                        if (Settings.detectorModel == 1)
                        {
                            text = "LD250";
                        }
                        else if (Settings.detectorModel == 2)
                        {
                            text = "LD350";
                        }
                        streamWriter.WriteLine("@RTHDR," + this.rUnique.Next(0, 2147483647).ToString() + "," + HeaderData.swVersion + ",1.0," + text + "," + HeaderData.antAlignment.ToString("0.0", Util.ci) + ",4," + TimeEx.timeToMillisUTC(DateTime.Now));
                        if (this.strikeRealtime.Count != 0)
                        {
                            foreach (StrikeData item in this.strikeRealtime)
                            {
                                streamWriter.WriteLine(this.BuildLgtDataString(item, true));
                                this.strikeBuffer.Add(item);
                                num++;
                            }
                        }
                        if (this.tracRealtime.Count != 0)
                        {
                            foreach (StormData item2 in this.tracRealtime)
                            {
                                streamWriter.WriteLine(this.BuildStormDataString(item2, true));
                                num2++;
                            }
                        }
                    }
                    try
                    {
                        if (Settings.exportJSON)
                        {
                            JSONStrikeContainer jSONStrikeContainer = new JSONStrikeContainer();
                            jSONStrikeContainer.TimestampEpoch = TimeEx.timestampEpoch();
                            jSONStrikeContainer.StrikeCount = this.strikeRealtime.Count;
                            jSONStrikeContainer.StormCount = this.tracRealtime.Count;
                            jSONStrikeContainer.SoftwareVersion = HeaderData.swVersion;
                            jSONStrikeContainer.DetectorModel = "StormTracker";
                            if (Settings.detectorModel == 1)
                            {
                                jSONStrikeContainer.DetectorModel = "LD-250";
                            }
                            else if (Settings.detectorModel == 2)
                            {
                                jSONStrikeContainer.DetectorModel = "LD-350";
                            }
                            jSONStrikeContainer.StationLat = Settings.Lat;
                            jSONStrikeContainer.StationLon = Settings.Lon;
                            jSONStrikeContainer.AntennaAlignment = HeaderData.antAlignment;
                            jSONStrikeContainer.StrikeData = this.strikeRealtime;
                            jSONStrikeContainer.StormData = this.tracRealtime;
                            string contents = new JavaScriptSerializer().Serialize(jSONStrikeContainer);
                            File.WriteAllText(Settings.locJSONFile, contents);
                        }
                    }
                    catch (Exception ex)
                    {
                        Syslogger.AddMsg("WriteRealtimeData() JSON", "ERROR > " + ex.Message);
                    }
                }
                catch (Exception ex2)
                {
                    Syslogger.AddMsg("WriteRealtimeData()", "ERROR > " + ex2.Message);
                }
                this.strikeRealtime.Clear();
                this.tracRealtime.Clear();
                Monitor.Exit(this.tracRealtime);
                Monitor.Exit(this.strikeBuffer);
                Monitor.Exit(this.strikeRealtime);
                if (num <= 0 && num2 <= 0)
                {
                    return;
                }
                this.UpdateRecsCounter(num);
                Syslogger.AddMsg("WriteRealtimeData()", "Dumped " + num.ToString() + " strike + " + num2.ToString() + " storm records");
            }
        }

        private bool WriteArchiveData()
        {
            int num = 0;
            Monitor.Enter(this.strikeBuffer);
            if (this.strikeBuffer != null && this.strikeBuffer.Count >= 1)
            {
                long num2 = 0L;
                try
                {
                    using (StreamWriter streamWriter = new StreamWriter(Settings.locSrcArchive))
                    {
                        foreach (StrikeData item in this.strikeBuffer)
                        {
                            if (item.millis >= num2)
                            {
                                streamWriter.WriteLine(this.BuildLgtDataString(item, false));
                                num++;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Syslogger.AddMsg("WriteArchiveData()", "ERROR > " + ex.InnerException);
                    Monitor.Exit(this.strikeBuffer);
                    return false;
                }
                Syslogger.AddMsg("WriteArchiveData()", "Wrote " + num.ToString() + " records");
                try
                {
                    File.Copy(Settings.locSrcArchive, Settings.locTmpArchive, true);
                }
                catch (Exception ex2)
                {
                    Syslogger.AddMsg("WriteArchiveData()", "File copy " + Settings.locSrcArchive + " to " + Settings.locTmpArchive + " failed");
                    Syslogger.AddMsg("WriteArchiveData()", "ERROR > " + ex2.InnerException);
                    Monitor.Exit(this.strikeBuffer);
                    return false;
                }
                if (Settings.exportJSON)
                {
                    try
                    {
                        JSONStrikeContainer jSONStrikeContainer = new JSONStrikeContainer();
                        jSONStrikeContainer.TimestampEpoch = TimeEx.timestampEpoch();
                        jSONStrikeContainer.StrikeCount = this.strikeBuffer.Count;
                        jSONStrikeContainer.StormCount = 0;
                        jSONStrikeContainer.SoftwareVersion = HeaderData.swVersion;
                        jSONStrikeContainer.DetectorModel = "StormTracker";
                        if (Settings.detectorModel == 1)
                        {
                            jSONStrikeContainer.DetectorModel = "LD-250";
                        }
                        else if (Settings.detectorModel == 2)
                        {
                            jSONStrikeContainer.DetectorModel = "LD-350";
                        }
                        jSONStrikeContainer.StationLat = Settings.Lat;
                        jSONStrikeContainer.StationLon = Settings.Lon;
                        jSONStrikeContainer.AntennaAlignment = HeaderData.antAlignment;
                        jSONStrikeContainer.StrikeData = this.strikeBuffer;
                        jSONStrikeContainer.StormData = new List<StormData>();
                        string contents = new JavaScriptSerializer().Serialize(jSONStrikeContainer);
                        File.WriteAllText(Settings.locJSONArchive, contents);
                        File.Copy(Settings.locJSONArchive, Settings.locJSONTmpArchive, true);
                        Syslogger.AddMsg("WriteArchiveData()", "Created JSON format strike data archive");
                    }
                    catch (Exception ex3)
                    {
                        Syslogger.AddMsg("WriteArchiveData()", "JSON File copy " + Settings.locJSONArchive + " to " + Settings.locJSONTmpArchive + " failed");
                        Syslogger.AddMsg("WriteArchiveData() JSON", "ERROR > " + ex3.Message);
                    }
                }
                this.TrimArchiveData();
                Monitor.Exit(this.strikeBuffer);
                return true;
            }
            Monitor.Exit(this.strikeBuffer);
            return false;
        }

        private int TrimArchiveData()
        {
            int num = 0;
            try
            {
                long num2 = TimeEx.timeToMillisUTC(DateTime.Now) - 3600000;
                foreach (StrikeData item in this.strikeBuffer)
                {
                    if (item.millis < num2)
                    {
                        num++;
                    }
                }
                this.strikeBuffer.RemoveRange(0, num);
            }
            catch (Exception)
            {
                Syslogger.AddMsg("TrimArchiveData()", "Exception while trimming");
                num = 0;
            }
            if (this.strikeBuffer.Count > Settings.maxArcRecords)
            {
                try
                {
                    int num3 = this.strikeBuffer.Count - Settings.maxArcRecords;
                    this.strikeBuffer.RemoveRange(0, num3);
                    num += num3;
                }
                catch (Exception)
                {
                    Syslogger.AddMsg("TrimArchiveData()", "Exception while clamping");
                }
            }
            if (num > 0)
            {
                Syslogger.AddMsg("TrimArchiveData()", "Deleted " + num.ToString() + " old/owerflow records");
            }
            return num;
        }

        private string BuildLgtDataString(StrikeData s, bool isRealTime)
        {
            string str = StrikeData.NMEAMarker + (isRealTime ? StrikeData.DATAID_RT : StrikeData.DATAID_ARC) + StrikeData.Sep;
            long num = this.uid;
            this.uid = num + 1;
            return str + num.ToString() + StrikeData.Sep + s.millis.ToString() + StrikeData.Sep + s.lat.ToString("0.000", Util.ci) + StrikeData.Sep + s.lon.ToString("0.000", Util.ci) + StrikeData.Sep + s.bng.ToString(Util.ci) + StrikeData.Sep + s.dist.ToString(Util.ci) + StrikeData.Sep + ((Settings.detectorModel == 1) ? StrikeDataTypes.UNDEF.ToString() : s.compoundType.ToString()) + StrikeData.Sep + (s.correlated ? "1" : "0");
        }

        private string BuildStormDataString(StormData s, bool isRealTime)
        {
            string str = StormData.NMEAMarker + (isRealTime ? StormData.DATAID_RT : StormData.DATAID_ARC) + StormData.Sep;
            long num = this.uid;
            this.uid = num + 1;
            return str + num.ToString() + StormData.Sep + s.stormID + StormData.Sep + s.detected_millis.ToString() + StormData.Sep + s.lastactive_millis.ToString() + StormData.Sep + s.lat.ToString("0.000", Util.ci) + StormData.Sep + s.lon.ToString("0.000", Util.ci) + StormData.Sep + s.bng.ToString(Util.ci) + StormData.Sep + s.dist.ToString(Util.ci) + StormData.Sep + (s.deleted ? "1" : "0");
        }

        private void btConverrt_Click(object sender, EventArgs e)
        {
            string[] obj = new string[4]
            {
                "ftp://localhost/svngarc.txt",
                "svngarc.txt",
                "relko",
                "flotilla"
            };
            this.UploadData(new Uri("ftp://localhost/svngarc.txt"), "relko", "flotilla", "svngarc.txt");
        }

        private void UploadArchive()
        {
            if (this.strikeBuffer == null || this.strikeBuffer.Count < 1)
            {
                Syslogger.AddMsg("UploadArchive()", "Archive buffer is null or empty");
            }
            else
            {
                Syslogger.AddMsg("UploadArchive()", "Writing archived data");
                if (this.WriteArchiveData())
                {
                    if (this.TArcUploader != null && this.TArcUploader.IsAlive)
                    {
                        Syslogger.AddMsg("UploadArchive()", "Archive uploader thread alive, aborting");
                        this.TArcUploader.Abort();
                    }
                    this.TArcUploader = new Thread(this.TFTPUpload);
                    this.TArcUploader.Priority = ThreadPriority.Normal;
                    this.TArcUploader.Start();
                }
            }
        }

        private void TFTPUpload()
        {
            if (Settings.fileCopyMode)
            {
                try
                {
                    File.Copy(Settings.locTmpArchive, Path.Combine(Settings.remoteStorePath, Settings.tgtArchive), true);
                    if (Settings.exportJSON)
                    {
                        File.Copy(Settings.locJSONArchive, Path.Combine(Settings.remoteStorePath, Settings.tgtJSONArchive), true);
                    }
                }
                catch (IOException ex)
                {
                    Syslogger.AddMsg("TFTPUpload()", "File.Copy exception: " + ex.Message);
                    Settings.UploadErrorCount++;
                }
            }
            else
            {
                FTP fTP = new FTP();
                if (!fTP.ConnectToServer(Settings.ftpCfg.server, Settings.ftpCfg.port, Settings.ftpCfg.conmode, Settings.ftpCfg.username, Settings.ftpCfg.password))
                {
                    Syslogger.AddMsg("TFTPUpload()", "ConnectToServer() failed, aborting");
                }
                else if (!fTP.SetWorkingDir(Settings.remoteStorePath))
                {
                    Syslogger.AddMsg("TFTPUpload()", "SetWorkingDir failed, aborting");
                    fTP.CloseConnection();
                }
                else if (!fTP.PutFile(Settings.locTmpArchive, Settings.tmpArchive))
                {
                    Syslogger.AddMsg("TFTPUpload()", "PutFile failed, aborting");
                    fTP.CloseConnection();
                    Settings.UploadErrorCount++;
                }
                else
                {
                    if (!fTP.DeleteFile(Settings.tgtArchive))
                    {
                        Syslogger.AddMsg("TFTPUpload()", "DeleteFile failed, proceeding");
                        Settings.UploadErrorCount++;
                    }
                    if (!fTP.RenameFile(Settings.tmpArchive, Settings.tgtArchive))
                    {
                        Syslogger.AddMsg("TFTPUpload()", "RenameFile failed, aborting");
                        fTP.CloseConnection();
                        Settings.UploadErrorCount++;
                    }
                    else
                    {
                        if (Settings.exportJSON)
                        {
                            if (!fTP.PutFile(Settings.locJSONTmpArchive, Settings.tmpJSONArchive))
                            {
                                Syslogger.AddMsg("TFTPUpload()", "JSON PutFile failed, aborting");
                                fTP.CloseConnection();
                                Settings.UploadErrorCount++;
                                return;
                            }
                            if (!fTP.DeleteFile(Settings.tgtJSONArchive))
                            {
                                Syslogger.AddMsg("TFTPUpload()", "JSON DeleteFile failed, proceeding");
                                Settings.UploadErrorCount++;
                            }
                            if (!fTP.RenameFile(Settings.tmpJSONArchive, Settings.tgtJSONArchive))
                            {
                                Syslogger.AddMsg("TFTPUpload()", "JSON RenameFile failed, aborting");
                                fTP.CloseConnection();
                                Settings.UploadErrorCount++;
                                return;
                            }
                        }
                        fTP.CloseConnection();
                    }
                }
            }
        }

        private void StartFTPUploadThread()
        {
            if (this.isExpired)
            {
                Syslogger.AddMsg("StartFTPUploadThread", "Cannot proceed, version expired");
            }
            else
            {
                this.m_FTPEventStopThread.Reset();
                this.m_FTPEventThreadStopped.Reset();
                this.m_FTPThread = new Thread(this.ThreadStartFTP);
                this.m_FTPThread.Name = "FTP Continuous background thread";
                this.m_FTPThread.Priority = ThreadPriority.Normal;
                this.m_FTPThread.Start();
                this.rtTimer.Start();
                this.SetUploadStatus();
            }
        }

        private void ThreadStartFTP()
        {
            if (!this.IsFTPBlocked)
            {
                this.rtUploader = new RTUploader(Settings.ftpCfg, this.m_FTPEventStopThread, this.m_FTPEventThreadStopped, this);
                this.rtUploader.Run();
            }
        }

        private void StopFTPUploadThread()
        {
            try
            {
                if (this.m_FTPThread != null && this.m_FTPThread.IsAlive)
                {
                    this.m_FTPEventStopThread.Set();
                    while (this.m_FTPThread.IsAlive && !WaitHandle.WaitAll(new ManualResetEvent[1]
                    {
                        this.m_FTPEventThreadStopped
                    }, 100, true))
                    {
                        Application.DoEvents();
                    }
                    this.SetUploadStatus();
                }
            }
            catch (Exception ex)
            {
                Syslogger.AddMsg("StopFTPUploadThread()", "Exception:" + ex.Message);
            }
        }

        private void FTPUploadCompleteEvent()
        {
            this.rtTimer.Start();
        }

        private void FTPThreadErrorEvent()
        {
            if (Settings.UploadErrorCount < 2147483647)
            {
                Settings.UploadErrorCount++;
            }
            this.errorCounter.IncCounter();
        }

        private void FTPThreadFinished()
        {
            Syslogger.AddMsg("FTPThreadFinished()", "thread exit");
            this.m_FTPThread = null;
            this.SetUploadStatus();
            this.btStartStop.Text = "Start";
        }

        private void StartIPCThreads()
        {
            if (this.isExpired)
            {
                Syslogger.AddMsg("StartIPCThreads", "Cannot proceed, version expired");
            }
            else
            {
                this.m_FG1EventStopThread.Reset();
                this.m_FG1EventThreadStopped.Reset();
                this.m_FG1Thread = new Thread(this.TreadStartFG1);
                this.m_FG1Thread.Name = "FlashGate 1 - Strike thread";
                this.m_FG1Thread.Priority = ThreadPriority.Normal;
                this.m_FG1Thread.Start();
                this.m_FG2EventStopThread.Reset();
                this.m_FG2EventThreadStopped.Reset();
                this.m_FG2Thread = new Thread(this.TreadStartFG2);
                this.m_FG2Thread.Name = "FlashGate 2 - Storm thread";
                this.m_FG2Thread.Priority = ThreadPriority.Normal;
                this.m_FG2Thread.Start();
            }
        }

        private void StopIPCThreads()
        {
            this.ThreadStopFG1();
            this.ThreadStopFG2();
        }

        private void TreadStartFG1()
        {
            new TIPC_FGStrike(this.m_FG1EventStopThread, this.m_FG1EventThreadStopped, this).Run();
        }

        private void ThreadStopFG1()
        {
            if (this.m_FG1Thread != null && this.m_FG1Thread.IsAlive)
            {
                this.m_FG1EventStopThread.Set();
                while (this.m_FG1Thread.IsAlive && !WaitHandle.WaitAll(new ManualResetEvent[1]
                {
                    this.m_FG1EventThreadStopped
                }, 100, true))
                {
                    Application.DoEvents();
                }
            }
            this.SetIPCStatus(false, Settings.IPC2Enabled);
        }

        private void FG1ThreadFinished()
        {
        }

        private void FG1DataRec(string s)
        {
            if (this.strikeRateCounter.CurrentCount() < Settings.rateLimiter)
            {
                this.SetLightningData(s);
            }
            this.SetIPCStatus(true, Settings.IPC2Enabled);
            Settings.IPC1LastComm = this.nowMS;
        }

        private void SetLightningData(string s)
        {
            LightningData lightningData = LightningData.ParseLightningData(s);
            if (lightningData != null)
            {
                this.AppendListLD(lightningData);
                this.strikeRateCounter.IncCounter();
                if (lightningData.dist <= Settings.CloseStrikeRangeKM)
                {
                    this.closeStrikeRateCounter.IncCounter();
                }
                if (Settings.comStrikeMsgEnabled)
                {
                    string newValue = (Settings.DistanceUnits == 0) ? lightningData.dist.ToString() : GeoMath.kmToMilesRounded(lightningData.dist).ToString();
                    string text = Settings.comStrikeMsg.Replace("@P1", newValue);
                    if (TxComHandler.hwPort.IsOpen)
                    {
                        try
                        {
                            TxComHandler.hwPort.WriteLine(text);
                            Console.WriteLine(text);
                        }
                        catch (Exception ex)
                        {
                            this.ComErrorHandler(ex.Message);
                        }
                    }
                }
            }
        }

        private void TreadStartFG2()
        {
            new TIPC_FGStorm(this.m_FG2EventStopThread, this.m_FG2EventThreadStopped, this).Run();
        }

        private void ThreadStopFG2()
        {
            if (this.m_FG2Thread != null && this.m_FG2Thread.IsAlive)
            {
                this.m_FG2EventStopThread.Set();
                while (this.m_FG2Thread.IsAlive && !WaitHandle.WaitAll(new ManualResetEvent[1]
                {
                    this.m_FG2EventThreadStopped
                }, 100, true))
                {
                    Application.DoEvents();
                }
            }
            this.SetIPCStatus(Settings.IPC1Enabled, false);
        }

        private void FG2ThreadFinished()
        {
        }

        private void FG2DataRec(string s)
        {
            this.SetStormData(s);
            this.SetIPCStatus(Settings.IPC1Enabled, true);
            Settings.IPC2LastComm = this.nowMS;
        }

        private void SetStormData(string s)
        {
            TRACData tRACData = TRACData.ParseTRACData(s);
            this.lbAntAlign.Text = ((HeaderData.antAlignment > 0.0) ? "+" : "") + HeaderData.antAlignment.ToString("0°");
            if (tRACData != null)
            {
                this.AppendListTD(tRACData);
            }
        }

        private void AppendListTD(TRACData td)
        {
            if (!td.isDeletionMsg)
            {
                StormData stormData = new StormData();
                stormData.NSTimestamp_s = Convert.ToDouble(td.timestamp_sec);
                stormData.detected_millis = TimeEx.timeToMillisUTC(td.detect_timestamp);
                stormData.lastactive_millis = TimeEx.timeToMillisUTC(td.lastactive_timestamp);
                stormData.stormID = td.TRACId;
                stormData.bng = td.bng;
                stormData.dist = td.dist;
                stormData.deleted = td.isDeletionMsg;
                stormData.CloseAlarmNSRangeKM = td.CA_dist;
                stormData.CloseAlarmNSTriggered = td.CA_trig;
                stormData.SevereAlarmLimRate = (double)td.SA_rate;
                stormData.SevereAlarmNSTriggered = td.SA_trig;
                stormData.SevereCellLimRate = (double)td.SSC_rate;
                stormData.SevereCellNSTriggered = td.SSC_trig;
                GeoMath.PrecisionCoordinate(Settings.Lat, Settings.Lon, stormData.bng, stormData.dist * 1000.0, out stormData.lat, out stormData.lon);
                Monitor.Enter(this.tracRealtime);
                this.tracRealtime.Add(stormData);
                Monitor.Exit(this.tracRealtime);
            }
        }

        private void AppendListLD(LightningData ld)
        {
            StrikeData strikeData = new StrikeData();
            strikeData.time = Convert.ToDouble(ld.timestamp_sec);
            strikeData.millis = TimeEx.timeToMillisUTC(ld.timestamp);
            strikeData.bng = ld.bng;
            strikeData.dist = ld.dist;
            strikeData.type = ld.type;
            strikeData.polarity = ld.polarity;
            strikeData.compoundType = ld.compound_type;
            strikeData.correlated = ld.correlated;
            GeoMath.PrecisionCoordinate(Settings.Lat, Settings.Lon, strikeData.bng, strikeData.dist * 1000.0, out strikeData.lat, out strikeData.lon);
            Monitor.Enter(this.strikeRealtime);
            this.strikeRealtime.Add(strikeData);
            Monitor.Exit(this.strikeRealtime);
            Monitor.Enter(this.strikeRealtimeDB);
            this.strikeRealtimeDB.Add(strikeData);
            Monitor.Exit(this.strikeRealtimeDB);
        }

        private Util.CloseActivityType CloseLightningCheck()
        {
            Util.CloseActivityType closeActivityType = Util.CloseActivityType.None;
            if (this.closeStrikeRateCounter.CurrentCount() >= Settings.MinCloseStrikeRate)
            {
                closeActivityType = Util.CloseActivityType.Strike;
            }
            if (closeActivityType != 0)
            {
                this.lastCloseLightningTimestamp = TimeEx.timestampEpoch();
            }
            return closeActivityType;
        }

        private bool AllClearCheck()
        {
            return this.AllClearDeltaTime() > Settings.AllClearPeriod;
        }

        private int AllClearDeltaTime()
        {
            if (this.lastCloseLightningTimestamp == 0L)
            {
                return 0;
            }
            return TimeEx.millisToMinutes(TimeEx.timestampEpoch() - this.lastCloseLightningTimestamp);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.DisconnectBoard();
            this.IsFTPBlocked = true;
            Settings.Save();
            this.StopFTPUploadThread();
            this.StopIPCThreads();
            int num = 10;
            while (num-- > 0)
            {
                Thread.Sleep(10);
            }
        }

        private void reactTmr_Elapsed(object source, EventArgs e)
        {
            if (Settings.reactivationFlag == Settings.REACTFLAG.REGISTERED)
            {
                this.Reactivate();
            }
        }

        private void uiUpdateTmr_Elapsed(object source, EventArgs e)
        {
            int num = this.strikeRateCounter.CurrentCount();
            this.lbDataRate.ForeColor = ((num >= Settings.rateLimiter) ? Color.Red : Color.Black);
            this.lbDataRate.Text = num.ToString();
            Label label = this.lbErrsPerMin;
            int num2 = this.errorCounter.CurrentCount();
            label.Text = num2.ToString();
            this.lbTotErrs.Text = Settings.UploadErrorCount.ToString();
            this.ActivateFTPLogic();
            this.nowMS = TimeEx.ticksToMillis(DateTime.Now.Ticks);
            this.lastIPCCheck += this.uiUpdateTmr.Interval;
            if (this.lastIPCCheck > 180000)
            {
                this.CheckIPCStatus();
            }
            if (this.CloseLightningCheck() != 0 && !this.closeAlarmActive)
            {
                this.closeAlarmActive = true;
                if (Settings.mSendStrikeAlert)
                {
                    this.alertMsg.EmailAlerts(0, false);
                }
                if (Settings.tSendStrikeAlert)
                {
                    this.alertMsg.TelegramAlerts(0, false);
                }
            }
            if (this.closeAlarmActive && this.AllClearCheck())
            {
                this.closeAlarmActive = false;
                if (Settings.mSendStrikeAlert && Settings.mSendClear)
                {
                    this.alertMsg.EmailAlerts(1, false);
                }
                if (Settings.tSendStrikeAlert && Settings.tSendClear)
                {
                    this.alertMsg.TelegramAlerts(1, false);
                }
            }
            if (this.mnuRegister.Enabled && Settings.licenseValid)
            {
                this.mnuRegister.Enabled = false;
            }
            int num3 = Settings.AllClearPeriod - this.AllClearDeltaTime();
            if (num3 < 0 || !this.closeAlarmActive)
            {
                num3 = 0;
            }
            Label label2 = this.lbCloseMin;
            num2 = this.closeStrikeRateCounter.CurrentCount();
            label2.Text = num2.ToString();
            string arg = "";
            if (num3 == 0 && this.closeAlarmActive)
            {
                num3 = 1;
                arg = "<";
            }
            this.lbClearMin.Text = string.Format("{0}{1} min", arg, num3);
            this.lbClearMin.ForeColor = (this.closeAlarmActive ? Color.Blue : Color.Gray);
            string text = "Disabled";
            Color foreColor = Color.Gray;
            if ((Settings.mSendStrikeAlert || Settings.tSendStrikeAlert) && !this.closeAlarmActive)
            {
                text = "Monitoring";
                foreColor = Color.Black;
                goto IL_024c;
            }
            if ((Settings.mSendStrikeAlert || Settings.tSendStrikeAlert) && this.closeAlarmActive)
            {
                text = "Active";
                foreColor = Color.Blue;
            }
            goto IL_024c;
        IL_024c:
            this.lbCAStatus.Text = text;
            this.lbCAStatus.ForeColor = foreColor;
            int num4 = (Settings.DistanceUnits == 0) ? ((int)Settings.CloseStrikeRangeKM) : GeoMath.kmToMilesRounded(Settings.CloseStrikeRangeKM);
            this.lbCARange.Text = string.Format("{0} {1}", num4, (Settings.DistanceUnits == 0) ? "km" : "mi");
            this.lbExtInt.Text = (Settings.comMsgEnabled ? TxComHandler.hwPort.PortName : "Off");
            this.lbExtInt.ForeColor = (Settings.comMsgEnabled ? Color.Black : Color.Gray);
            this.lbJSONStatus.Text = (Settings.exportJSON ? "Enabled" : "Disabled");
            this.lbJSONStatus.ForeColor = (Settings.exportJSON ? Color.Black : Color.Gray);
            this.lbDBStatus.Text = (Settings.storeDB ? "On" : "Off");
            this.lbDBStatus.ForeColor = (Settings.storeDB ? Color.Black : Color.Gray);
        }

        private void chkExpTmr_Elapsed(object source, EventArgs e)
        {
            this.CheckExpires();
        }

        private void UploadData(Uri uri, string username, string password, string localfile)
        {
            using (WebClient webClient = new WebClient())
            {
                webClient.Proxy = null;
                webClient.Credentials = new NetworkCredential(username, password);
                webClient.UploadFileCompleted += this.wc_UploadFileCompleted;
                webClient.UploadFileAsync(uri, localfile);
            }
        }

        private void wc_UploadFileCompleted(object sender, UploadFileCompletedEventArgs e)
        {
            this.rtTimer.Start();
        }

        private StrikeData RandomData()
        {
            StrikeData strikeData = new StrikeData();
            Random random = new Random();
            strikeData.lat = Settings.Lat + (random.NextDouble() * 10.0 - 5.0);
            strikeData.lon = Settings.Lon + (random.NextDouble() * 10.0 - 5.0);
            strikeData.millis = TimeEx.timeToMillisUTC(DateTime.Now);
            strikeData.compoundType = random.Next(StrikeDataTypes.PCG, StrikeDataTypes.PNIC);
            strikeData.correlated = true;
            return strikeData;
        }

        public void LoadExtData()
        {
            this.strikeBuffer = new List<StrikeData>();
            using (StreamReader streamReader = new StreamReader(Application.StartupPath + "\\data.txt"))
            {
                string text;
                do
                {
                    text = streamReader.ReadLine();
                    if (text != null)
                    {
                        string[] array = text.Split(',');
                        StrikeData strikeData = new StrikeData();
                        strikeData.time = Convert.ToDouble(int.Parse(array[0]));
                        strikeData.millis = TimeEx.timeToMillisUTC(this.virtualDT.AddSeconds(strikeData.time));
                        if (strikeData.millis < TimeEx.timeToMillisUTC(this.virtualDT.AddDays(0.7)))
                        {
                            strikeData.bng = double.Parse(array[1], Util.ci);
                            strikeData.dist = Convert.ToDouble(int.Parse(array[2]));
                            strikeData.type = int.Parse(array[3]);
                            strikeData.polarity = int.Parse(array[4]);
                            if (strikeData.type == StrikeDataTypes.CG)
                            {
                                strikeData.compoundType = ((strikeData.polarity == StrikeDataTypes.POS) ? StrikeDataTypes.PCG : StrikeDataTypes.NCG);
                            }
                            else if (strikeData.type == StrikeDataTypes.IC)
                            {
                                strikeData.compoundType = StrikeDataTypes.PNIC;
                            }
                            else
                            {
                                strikeData.compoundType = StrikeDataTypes.UNCATEGORIZED;
                            }
                            strikeData.correlated = false;
                            GeoMath.GCPoint(Settings.Lat, Settings.Lon, strikeData.bng, strikeData.dist, out strikeData.lat, out strikeData.lon);
                            this.strikeBuffer.Add(strikeData);
                            continue;
                        }
                        break;
                    }
                }
                while (text != null);
            }
        }

        private void txTargetDir_TextChanged(object sender, EventArgs e)
        {
        }

        private void txFTPServer_Leave(object sender, EventArgs e)
        {
            Settings.ftpCfg.server = this.txFTPServer.Text;
        }

        private void txFTPPort_TextChanged(object sender, EventArgs e)
        {
        }

        private void txFTPPort_Leave(object sender, EventArgs e)
        {
            try
            {
                Settings.ftpCfg.port = int.Parse(this.txFTPPort.Text);
            }
            catch
            {
                MessageBox.Show("Value must be integer", "Input error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                this.txFTPPort.Focus();
            }
        }

        private void txUsername_Leave(object sender, EventArgs e)
        {
            Settings.ftpCfg.username = this.txUsername.Text;
        }

        private void txPassword_Leave(object sender, EventArgs e)
        {
            Settings.ftpCfg.password = this.txPassword.Text;
        }

        private void txTargetDir_Leave(object sender, EventArgs e)
        {
            Settings.remoteStorePath = this.txTargetDir.Text;
        }

        private void cbFileCopyMode_CheckedChanged(object sender, EventArgs e)
        {
            Settings.fileCopyMode = this.cbFileCopyMode.Checked;
            this.SetControlStatus();
        }

        private void SetControlStatus()
        {
            this.txFTPServer.Enabled = !Settings.fileCopyMode;
            this.txFTPPort.Enabled = !Settings.fileCopyMode;
            this.txUsername.Enabled = !Settings.fileCopyMode;
            this.txPassword.Enabled = !Settings.fileCopyMode;
            this.cbConMode.Enabled = !Settings.fileCopyMode;
        }

        private void txLat_Leave(object sender, EventArgs e)
        {
            try
            {
                Settings.Lat = double.Parse(this.txLat.Text, Util.ci);
            }
            catch
            {
                MessageBox.Show("Latitude must be decimal degrees", "Input error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                this.txLat.Focus();
            }
        }

        private void txLon_Leave(object sender, EventArgs e)
        {
            try
            {
                Settings.Lon = double.Parse(this.txLon.Text, Util.ci);
            }
            catch
            {
                MessageBox.Show("Longitude must be decimal degrees", "Input error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                this.txLon.Focus();
            }
        }

        private void cbConMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            Settings.ftpCfg.conmode = (FTP.ConnectionModes)((this.cbConMode.SelectedIndex != 0) ? 134217728 : 0);
        }

        private void btPause_Click(object sender, EventArgs e)
        {
            Settings.transferPaused = !Settings.transferPaused;
            Syslogger.AddMsg("User", "UPLOADS " + (Settings.transferPaused ? "PAUSED" : "RESUMING"));
            this.SetUploadStatus();
        }

        private void SetUploadStatus()
        {
            try
            {
                this.btPause.Text = (Settings.transferPaused ? "Resume" : "Hold");
                if (this.m_FTPThread == null || !this.m_FTPThread.IsAlive)
                {
                    this.lbUploadStatus.ForeColor = Color.Red;
                    this.lbUploadStatus.Text = "Stopped " + this.FTPStopReason;
                }
                else
                {
                    this.lbUploadStatus.ForeColor = (Settings.transferPaused ? Color.Red : Color.Blue);
                    this.lbUploadStatus.Text = (Settings.transferPaused ? "Hold" : "Active");
                }
            }
            catch (Exception)
            {
            }
        }

        private void CheckIPCStatus()
        {
            this.lastIPCCheck = 0;
            long num = TimeEx.ticksToMillis(DateTime.Now.Ticks) - 180000;
            if (Settings.IPC1Enabled && Settings.IPC1LastComm < num)
            {
                Settings.IPC1Enabled = false;
                Syslogger.AddMsg("CheckIPCStatus()", "IPC 1/Strike - inactivity detected");
            }
            if (Settings.IPC2Enabled && Settings.IPC2LastComm < num)
            {
                Settings.IPC2Enabled = false;
                Syslogger.AddMsg("CheckIPCStatus()", "IPC 2/Storm - inactivity detected");
            }
            this.SetIPCStatus(Settings.IPC1Enabled, Settings.IPC2Enabled);
        }

        private void SetIPCStatus(bool ipc1, bool ipc2)
        {
            try
            {
                Settings.IPC1Enabled = ipc1;
                Settings.IPC2Enabled = ipc2;
                if (Settings.IPC1Enabled)
                {
                    this.lbIPC1.ForeColor = Color.Blue;
                    this.lbIPC1.Text = "Active";
                }
                else
                {
                    this.lbIPC1.ForeColor = Color.Red;
                    this.lbIPC1.Text = "Inactive";
                }
                if (Settings.IPC2Enabled)
                {
                    this.lbIPC2.ForeColor = Color.Blue;
                    this.lbIPC2.Text = "Active";
                }
                else
                {
                    this.lbIPC2.ForeColor = Color.Red;
                    this.lbIPC2.Text = "Inactive";
                }
            }
            catch (Exception)
            {
            }
        }

        private void UpdateRecsCounter(int cnt)
        {
            Settings.UploadedRecsCount += cnt;
            this.lbRecCount.Text = Settings.UploadedRecsCount.ToString();
        }

        private void btSyslog_Click(object sender, EventArgs e)
        {
            if (this.fSyslog.IsDisposed)
            {
                this.fSyslog = new FrmSyslog();
            }
            if (!this.fSyslog.Visible)
            {
                this.fSyslog.Visible = true;
            }
            if (!this.fSyslog.Focused)
            {
                this.fSyslog.Focus();
            }
        }

        private void btResetErrors_Click(object sender, EventArgs e)
        {
            Settings.UploadErrorCount = 0;
        }

        private void label14_Click(object sender, EventArgs e)
        {
        }

        private void txLon_TextChanged(object sender, EventArgs e)
        {
        }

        private void nudRTInterval_ValueChanged(object sender, EventArgs e)
        {
            Settings.rtUploadInterval = Convert.ToInt32(this.nudRTInterval.Value);
            this.rtTimer.Interval = Convert.ToInt32(TimeEx.secondsToMillis((double)Settings.rtUploadInterval));
        }

        private void nudArcInterval_ValueChanged(object sender, EventArgs e)
        {
            Settings.arcUploadInterval = Convert.ToInt32(this.nudArcInterval.Value);
        }

        private void nudMinStrikeRate_ValueChanged(object sender, EventArgs e)
        {
            Settings.idleLimiter = Convert.ToInt32(this.nudMinStrikeRate.Value);
        }

        private void nudMaxStrikeRate_ValueChanged(object sender, EventArgs e)
        {
            Settings.rateLimiter = Convert.ToInt32(this.nudMaxStrikeRate.Value);
        }

        private void registerOnlineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new LicenseEngine().DeletFileISO();
        }

        private void saveSettingsMnu_Click(object sender, EventArgs e)
        {
            Settings.Save();
            MessageBox.Show("Settings saved", "StormVue NGX Data Server", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void btStartStop_Click(object sender, EventArgs e)
        {
            if ((this.m_FTPThread == null || !this.m_FTPThread.IsAlive) && this.FTPStopReason == "(User)")
            {
                this.StartFTPUploadThread();
                this.btStartStop.Text = "Stop";
                return;
            }
            if (this.m_FTPThread != null && this.m_FTPThread.IsAlive)
            {
                this.StopFTPUploadThread();
                this.FTPStopReason = "(User)";
                this.btStartStop.Text = "Start";
            }
        }

        private void nudMaxArcRecs_ValueChanged(object sender, EventArgs e)
        {
            Settings.maxArcRecords = Convert.ToInt32(this.nudMaxArcRecs.Value);
        }

        private void mnuAbout_Click(object sender, EventArgs e)
        {
            if (this.fAbout.IsDisposed)
            {
                this.fAbout = new frmAbout();
            }
            if (!this.fAbout.Visible)
            {
                this.fAbout.Visible = true;
            }
            if (!this.fAbout.Focused)
            {
                this.fAbout.Focus();
            }
        }

        private void mnuExit_Click(object sender, EventArgs e)
        {
            base.Close();
        }

        private void mnuHelp_Click(object sender, EventArgs e)
        {
            Process.Start("http://www.astrogenic.com/w3help/StormVue_NGX_suite");
        }

        private void comboDetector_SelectedIndexChanged(object sender, EventArgs e)
        {
            Settings.detectorModel = this.comboDetector.SelectedIndex;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (base.WindowState == FormWindowState.Minimized)
            {
                base.Hide();
                if (!this.fSyslog.IsDisposed)
                {
                    this.fSyslog.Hide();
                }
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            base.Show();
            base.WindowState = FormWindowState.Normal;
            if (!this.fSyslog.IsDisposed)
            {
                this.fSyslog.Show();
            }
        }

        private void mnuStartOnLogin_Click(object sender, EventArgs e)
        {
            Settings.startOnLogin = this.mnuStartOnLogin.Checked;
            this.SetAutorun(Settings.startOnLogin);
        }

        public void SetAutorun(bool enable)
        {
            try
            {
                RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
                if (registryKey != null)
                {
                    if (enable)
                    {
                        registryKey.SetValue("StormVueNGXDS", Application.ExecutablePath);
                    }
                    else
                    {
                        registryKey.DeleteValue("StormVueNGXDS", false);
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("StormVue NGX Data Server must run with Administrator privileges to perform this action", "UAC error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                this.mnuStartOnLogin.Checked = false;
                Settings.startOnLogin = false;
            }
        }

        private void mnuRegister_Click(object sender, EventArgs e)
        {
            if (this.fActivator.IsDisposed)
            {
                this.fActivator = new frmActivator();
            }
            if (!this.fActivator.Visible)
            {
                this.fActivator.Visible = true;
            }
            if (!this.fActivator.Focused)
            {
                this.fActivator.Focus();
            }
            this.fActivator.Left = base.Left + base.Width / 2 - this.fActivator.Width / 2;
            this.fActivator.Top = base.Top + base.Height / 2 - this.fActivator.Height / 2;
        }

        private void mnVerCheck_Click(object sender, EventArgs e)
        {
            string text = VersionCheck.CheckVersion();
            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show("Your version is up to date", "Version check completed", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            else if (!(text == "-2") && MessageBox.Show("There is a newer version available: Version " + text + "\r\n\r\n Do you want to update now?", "Version check completed", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.Yes)
            {
                try
                {
                    Process.Start(Settings.updateURL);
                }
                catch (Exception)
                {
                    MessageBox.Show("There was a problem sending commands to your web browser", "Browser error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }
        }

        private void mnuGoForum_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(Settings.forumURL);
            }
            catch (Exception)
            {
                MessageBox.Show("There was a problem sending commands to your web browser", "Browser error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private void mnuConfig_Click(object sender, EventArgs e)
        {
            if (this.cfgDialog == null || this.cfgDialog.IsDisposed)
            {
                this.cfgDialog = new ConfigurationDlg(this);
            }
            this.cfgDialog.Show();
            this.cfgDialog.BringToFront();
        }

        public async Task TestSendTelegram(bool isTestMsg = false)
        {
            if (!string.IsNullOrEmpty(Settings.tBotToken) && Settings.tDestId != -1)
            {
                await this.alertMsg.TelegramAlerts(0, true);
                if (this.alertMsg.telegramSent)
                {
                    MessageBox.Show("Telegram test message sent, check your device", "Status", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            }
            else
            {
                MessageBox.Show("Bot token or Destination ID is not set, cannot send message", "Telegram messenger error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        public void TestSendAlert(bool isTestMsg = false)
        {
            this.alertMsg.EmailAlerts(0, true);
        }

        public void SendResult(string res)
        {
            if (!string.IsNullOrEmpty(res))
            {
                Console.WriteLine("E-mail send error: " + res);
                Syslogger.AddMsg("SendResult()", "E-mail send error: " + res);
                if (this.alertMsg.IsTestMessage)
                {
                    MessageBox.Show(res, "E-mail test status", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }
            else
            {
                Console.WriteLine("Message sent succesfully!");
                Syslogger.AddMsg("SendResult()", "Message e-mailed succesfully");
                if (this.alertMsg.IsTestMessage)
                {
                    MessageBox.Show("Success!", "E-mail test status", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            }
        }

        public void ConnectBoard()
        {
            if (Settings.comMsgEnabled && !TxComHandler.hwPort.IsOpen)
            {
                if (TxComHandler.OpenCOM())
                {
                    Syslogger.AddMsg("COM I/O", "Opened " + TxComHandler.hwPort.PortName + " at " + TxComHandler.hwPort.BaudRate.ToString() + " baud");
                    this.comMsgTmr.Interval = Settings.comPeriodicIntervalSecs * 1000;
                    this.comMsgTmr.Start();
                }
                else
                {
                    Syslogger.AddMsg("COM I/O", "Error opening " + TxComHandler.hwPort.PortName + ", disabling external comms");
                    Settings.comMsgEnabled = false;
                }
            }
        }

        public void DisconnectBoard()
        {
            if (TxComHandler.hwPort.IsOpen)
            {
                this.comMsgTmr.Stop();
                TxComHandler.CloseCOM();
                Syslogger.AddMsg("COM I/O", "Closed " + TxComHandler.hwPort.PortName);
            }
        }

        private void comMsgTmr_Tick(object sender, EventArgs e)
        {
            try
            {
                string newValue = (Settings.DistanceUnits == 0) ? Settings.CloseStrikeRangeKM.ToString() : GeoMath.kmToMilesRounded(Settings.CloseStrikeRangeKM).ToString();
                string text = Settings.comPeriodicMsg.Replace("@P1", newValue);
                text = text.Replace("@P2", this.closeAlarmActive ? "1" : "0");
                if (TxComHandler.hwPort.IsOpen)
                {
                    try
                    {
                        TxComHandler.hwPort.WriteLine(text);
                        Console.WriteLine(text);
                    }
                    catch (Exception ex)
                    {
                        this.ComErrorHandler(ex.Message);
                    }
                }
            }
            catch (Exception ex2)
            {
                Console.WriteLine(ex2.Message);
            }
        }

        public void ComErrorHandler(string ExceptionMessage)
        {
            this.comMsgTmr.Stop();
            TxComHandler.CloseCOM();
            Settings.comMsgEnabled = false;
            Console.WriteLine(ExceptionMessage);
            Syslogger.AddMsg("ComErrorHandler", "An exception occured: " + ExceptionMessage);
        }

        public void StartDBStorage()
        {
            Settings.storeDB = true;
            this.dbInsertTimer.Start();
            Syslogger.AddMsg("StartDBStorage", "Database storage enabled");
        }

        public void StopDBStorage()
        {
            this.dbInsertTimer.Stop();
            Settings.storeDB = false;
            Syslogger.AddMsg("StopDBStorage", "Database storage disabled");
        }

        private void dbInsertTimer_Tick(object sender, EventArgs e)
        {
            if (Settings.storeDB)
            {
                this.sw.Restart();
                Monitor.Enter(this.strikeRealtimeDB);
                try
                {
                    if (this.strikeRealtimeDB.Count > 0)
                    {
                        DBManager.LightningToDB(this.strikeRealtimeDB);
                    }
                }
                catch (Exception ex)
                {
                    Syslogger.AddMsg("dbInsertTimer", "DB ERROR > " + ex.Message);
                    this.dbErrors++;
                }
                if (this.dbErrors == 0 || this.dbErrors > 4)
                {
                    this.dbErrors = 0;
                    this.strikeRealtimeDB.Clear();
                }
                Monitor.Exit(this.strikeRealtimeDB);
                this.sw.Stop();
                Console.WriteLine("DB operation took {0} ms", this.sw.ElapsedMilliseconds);
            }
            this.lbDBInsTime.Text = string.Format("{0} ms", Settings.storeDB ? this.sw.ElapsedMilliseconds.ToString() : "--");
        }

        private void mnuChkLicense_Click(object sender, EventArgs e)
        {
            if (this.fLicCheck.IsDisposed)
            {
                this.fLicCheck = new frmLicenseCheck();
            }
            if (!this.fLicCheck.Visible)
            {
                this.fLicCheck.Visible = true;
            }
            if (!this.fLicCheck.Focused)
            {
                this.fLicCheck.Focus();
            }
            this.fLicCheck.Left = base.Left + base.Width / 2 - this.fLicCheck.Width / 2;
            this.fLicCheck.Top = base.Top + base.Height / 2 - this.fLicCheck.Height / 2;
        }
    }
}
