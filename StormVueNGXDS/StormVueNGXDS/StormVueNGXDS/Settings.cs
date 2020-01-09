using FTP32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace StormVue2RTCM
{
	internal class Settings
	{
		public enum REACTFLAG
		{
			OFF,
			DEACTIVATED,
			REGISTERED
		}

		public const double UNDEF_COORD = -999999.0;

		public static double Lat = -999999.0;

		public static double Lon = -999999.0;

		public static string appStorePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Astrogenic Systems\\SVueNGX";

		public static string remoteStorePath = ".";

		public static string tgtRealtime = "svngrtd.txt";

		public static string tmpRealtime = "svngrtd.tmp";

		public static string tgtArchive = "svngarc.txt";

		public static string tmpArchive = "svngarc.tmp";

		public static string tgtJSONRealtime = "ngxdata.json";

		public static string tmpJSONRealtime = "ngxdata.tmp";

		public static string tgtJSONArchive = "ngxarchive.json";

		public static string tmpJSONArchive = "ngxarchive.tmp";

		public static string locSrcRealtime = Settings.appStorePath + "\\svngrtd.txt";

		public static string locTmpRealtime = Settings.appStorePath + "\\svngrtd.tmp";

		public static string locSrcArchive = Settings.appStorePath + "\\svngarc.txt";

		public static string locTmpArchive = Settings.appStorePath + "\\svngarc.tmp";

		public static bool exportJSON = false;

		public static string locJSONFile = Settings.appStorePath + "\\ngxdata.json";

		public static string locJSONTmpArchive = Settings.appStorePath + "\\ngxarchive.tmp";

		public static string locJSONArchive = Settings.appStorePath + "\\ngxarchive.json";

		public static bool storeDB = false;

		public static string dbPath = Path.Combine(Settings.appStorePath, "db");

		public static bool fileCopyMode = false;

		public static bool transferPaused = false;

		public static bool transferStopped = false;

		public static int rtUploadInterval = 10;

		public static int arcUploadInterval = 5;

		public static int idleLimiter = 0;

		public static int rateLimiter = 250;

		public static int maxArcRecords = 10000;

		public static int detectorModel = 0;

		public static bool IPC1Enabled = false;

		public static bool IPC2Enabled = false;

		public static long IPC1LastComm = 0L;

		public static long IPC2LastComm = 0L;

		public static int UploadErrorCount = 0;

		public static int UploadedRecsCount = 0;

		public static bool startOnLogin = false;

		public static FTPConnectInfo ftpCfg;

		public static double CloseStrikeRangeKM = 25.0;

		public static int MinCloseStrikeRate = 1;

		public static int AllClearPeriod = 5;

		public static int DistanceUnits = 0;

		public static bool mSendStrikeAlert = false;

		public static bool mSendClear = false;

		public static bool tSendStrikeAlert = false;

		public static bool tSendClear = false;

		public static string tBotToken = "";

		public static long tDestId = -1L;

		public static string mServer = "";

		public static int mPort = 587;

		public static int mSecIndex = 0;

		public static int mAuthIndex = 0;

		public static string mUsername = "";

		public static string mPassword = "";

		public static string mFrom = "";

		public static List<string> mToList = new List<string>();

		public static string mAlarmSubj = "NGX CLOSE ALARM <date>";

		public static string mAlarmBody = "Lightning activity detected within <distance> <distunits> at <date>.";

		public static string mClearSubj = "NGX All Clear Notification <date>";

		public static string mClearBody = "No lightning detected for <watchtime> minutes within <distance> <distunits> range.";

		public static bool comMsgEnabled = false;

		public static string comPeriodicMsg = "$R:@P1,C:@P2";

		public static string comStrikeMsg = "$S:@P1";

		public static bool comStrikeMsgEnabled = false;

		public static int comPeriodicIntervalSecs = 1;

		public static string MachineID_r13 = "";

		public static DateTime trialExpDate;

		public static bool licenseValid = false;

		public static bool trialExpired = false;

		public static REACTFLAG reactivationFlag = REACTFLAG.OFF;

		public static string updateURL = "http://nexstorm.astrogenic.com/updates.html";

		public static string forumURL = "http://forum.astrogenic.com";

		public static void Load()
		{
			string text = "";
			try
			{
				if (!Directory.Exists(Settings.appStorePath))
				{
					Directory.CreateDirectory(Settings.appStorePath);
				}
				if (!Directory.Exists(Settings.dbPath))
				{
					Directory.CreateDirectory(Settings.dbPath);
				}
				using (StreamReader streamReader = new StreamReader(Settings.appStorePath + "\\rtcm.config", false))
				{
					string text2;
					do
					{
						text2 = streamReader.ReadLine();
						if (text2 != null && !text2.StartsWith("//"))
						{
							string[] array = text2.Split(new char[1]
							{
								'|'
							}, 2);
							array[0] = array[0].ToUpper();
							switch (array[0])
							{
							case "FTPSERVER":
								Settings.ftpCfg.server = (string)Settings.getSetting(Settings.ftpCfg.server, array[1], "");
								break;
							case "FTPPORT":
								Settings.ftpCfg.port = (int)Settings.getSetting(Settings.ftpCfg.port, array[1], 21);
								break;
							case "FTPUSER":
								Settings.ftpCfg.username = (string)Settings.getSetting(Settings.ftpCfg.username, array[1], "");
								break;
							case "FTPPASSWORD":
								Settings.ftpCfg.password = (string)Settings.getSetting(Settings.ftpCfg.password, array[1], "");
								break;
							case "FTPCONMODE":
							{
								int num = 0;
								Settings.ftpCfg.conmode = (FTP.ConnectionModes)(((int)Settings.getSetting(num, array[1], 0) != 0) ? 134217728 : 0);
								break;
							}
							case "REMOTEPATH":
								Settings.remoteStorePath = (string)Settings.getSetting(Settings.remoteStorePath, array[1], ".");
								break;
							case "FILECOPYENABLED":
								Settings.fileCopyMode = (bool)Settings.getSetting(Settings.fileCopyMode, array[1], false);
								break;
							case "SYSLAT":
								Settings.Lat = (double)Settings.getSetting(Settings.Lat, array[1], -999999.0);
								break;
							case "SYSLON":
								Settings.Lon = (double)Settings.getSetting(Settings.Lon, array[1], -999999.0);
								break;
							case "REALTIMEINTERVAL":
								Settings.rtUploadInterval = (int)Settings.getSetting(Settings.rtUploadInterval, array[1], 10);
								break;
							case "ARCHIVEINTERVAL":
								Settings.arcUploadInterval = (int)Settings.getSetting(Settings.arcUploadInterval, array[1], 5);
								break;
							case "IDLELIMITER":
								Settings.idleLimiter = (int)Settings.getSetting(Settings.idleLimiter, array[1], 5);
								break;
							case "RATELIMITER":
								Settings.rateLimiter = (int)Settings.getSetting(Settings.rateLimiter, array[1], 500);
								break;
							case "MAXARCRECS":
								Settings.maxArcRecords = (int)Settings.getSetting(Settings.maxArcRecords, array[1], 10000);
								break;
							case "DETECTORMODEL":
								Settings.detectorModel = (int)Settings.getSetting(Settings.detectorModel, array[1], 0);
								break;
							case "STARTONLOGIN":
								Settings.startOnLogin = (bool)Settings.getSetting(Settings.startOnLogin, array[1], false);
								break;
							case "CLSSTRIKEMRNG":
								Settings.CloseStrikeRangeKM = (double)Settings.getSetting(Settings.CloseStrikeRangeKM, array[1], 25.0);
								break;
							case "MINCLSRATE":
								Settings.MinCloseStrikeRate = (int)Settings.getSetting(Settings.MinCloseStrikeRate, array[1], 1);
								break;
							case "ALLCLRPERIOD":
								Settings.AllClearPeriod = (int)Settings.getSetting(Settings.AllClearPeriod, array[1], 5);
								break;
							case "DISTANCEUNITS":
								Settings.DistanceUnits = (int)Settings.getSetting(Settings.DistanceUnits, array[1], 0);
								break;
							case "SENDSTRIKEALERT":
								Settings.mSendStrikeAlert = (bool)Settings.getSetting(Settings.mSendStrikeAlert, array[1], false);
								break;
							case "SENDALLCLEAR":
								Settings.mSendClear = (bool)Settings.getSetting(Settings.mSendClear, array[1], false);
								break;
							case "SENDSTRIKEALERTTGM":
								Settings.tSendStrikeAlert = (bool)Settings.getSetting(Settings.tSendStrikeAlert, array[1], false);
								break;
							case "SENDALLCLEARTGM":
								Settings.tSendClear = (bool)Settings.getSetting(Settings.tSendClear, array[1], false);
								break;
							case "TBOTTOKEN":
								Settings.tBotToken = (string)Settings.getSetting(Settings.tBotToken, array[1], "");
								break;
							case "TDESTID":
								Settings.tDestId = (long)Settings.getSetting(Settings.tDestId, array[1], -1L);
								break;
							case "COMIOON":
								Settings.comMsgEnabled = (bool)Settings.getSetting(Settings.comMsgEnabled, array[1], false);
								break;
							case "COMSTRIKEIOON":
								Settings.comStrikeMsgEnabled = (bool)Settings.getSetting(Settings.comStrikeMsgEnabled, array[1], false);
								break;
							case "COMPORT":
								TxComHandler.hwPort.PortName = array[1];
								break;
							case "COMBAUD":
								TxComHandler.hwPort.BaudRate = (int)Settings.getSetting(TxComHandler.hwPort.BaudRate, array[1], 0);
								break;
							case "EXPJSON":
								Settings.exportJSON = (bool)Settings.getSetting(Settings.exportJSON, array[1], false);
								break;
							case "STOREDB":
								Settings.storeDB = (bool)Settings.getSetting(Settings.storeDB, array[1], false);
								break;
							case "DBPATH":
								text = Settings.dbPath;
								Settings.dbPath = (string)Settings.getSetting(Settings.dbPath, array[1], text);
								break;
							case "MAILSRV":
								Settings.mServer = array[1];
								break;
							case "MAILPRT":
								int.TryParse(array[1], out Settings.mPort);
								break;
							case "MAILSEC":
								int.TryParse(array[1], out Settings.mSecIndex);
								break;
							case "MAILAUT":
								int.TryParse(array[1], out Settings.mAuthIndex);
								break;
							case "MAILUSR":
								Settings.mUsername = array[1];
								break;
							case "MAILPWD":
								Settings.mPassword = array[1];
								break;
							case "MAILFRM":
								Settings.mFrom = array[1];
								break;
							case "MAILALERTSUBJ":
								text = Settings.mAlarmSubj;
								Settings.mAlarmSubj = (string)Settings.getSetting(Settings.mAlarmSubj, array[1], text);
								break;
							case "MAILALERTBODY":
								text = Settings.mAlarmBody;
								Settings.mAlarmBody = (string)Settings.getSetting(Settings.mAlarmBody, array[1], text);
								break;
							case "MAILCLEARSUBJ":
								text = Settings.mClearSubj;
								Settings.mClearSubj = (string)Settings.getSetting(Settings.mClearSubj, array[1], text);
								break;
							case "MAILCLEARBODY":
								text = Settings.mClearBody;
								Settings.mClearBody = (string)Settings.getSetting(Settings.mClearBody, array[1], text);
								break;
							case "MAILRCP":
								if (!string.IsNullOrEmpty(array[1]))
								{
									string[] source = array[1].Split(';');
									Settings.mToList.AddRange(source.ToList());
								}
								break;
							}
						}
					}
					while (text2 != null);
				}
			}
			catch (Exception)
			{
				Syslogger.AddMsg("Settings.Load()", "Configuration load error (first time run?)");
			}
		}

		public static void Save()
		{
			try
			{
				if (!Directory.Exists(Settings.appStorePath))
				{
					Directory.CreateDirectory(Settings.appStorePath);
				}
				using (StreamWriter streamWriter = new StreamWriter(Settings.appStorePath + "\\rtcm.config", false))
				{
					streamWriter.WriteLine("// StormVue NG Real-time Connect Module settings /////////////////////");
					streamWriter.WriteLine("FTPSERVER|" + Settings.ftpCfg.server);
					streamWriter.WriteLine("FTPPORT|" + Settings.ftpCfg.port.ToString());
					streamWriter.WriteLine("FTPUSER|" + Settings.ftpCfg.username);
					streamWriter.WriteLine("FTPPASSWORD|" + Settings.ftpCfg.password);
					streamWriter.WriteLine("FTPCONMODE|" + ((Settings.ftpCfg.conmode == FTP.ConnectionModes.Active) ? "0" : "1"));
					streamWriter.WriteLine("FILECOPYENABLED|" + Settings.fileCopyMode.ToString());
					streamWriter.WriteLine("REMOTEPATH|" + Settings.remoteStorePath);
					streamWriter.WriteLine("SYSLAT|" + Settings.Lat.ToString(Util.ci));
					streamWriter.WriteLine("SYSLON|" + Settings.Lon.ToString(Util.ci));
					streamWriter.WriteLine("REALTIMEINTERVAL|" + Settings.rtUploadInterval.ToString());
					streamWriter.WriteLine("ARCHIVEINTERVAL|" + Settings.arcUploadInterval.ToString());
					streamWriter.WriteLine("IDLELIMITER|" + Settings.idleLimiter.ToString());
					streamWriter.WriteLine("RATELIMITER|" + Settings.rateLimiter.ToString());
					streamWriter.WriteLine("MAXARCRECS|" + Settings.maxArcRecords.ToString());
					streamWriter.WriteLine("DETECTORMODEL|" + Settings.detectorModel.ToString());
					streamWriter.WriteLine("STARTONLOGIN|" + Settings.startOnLogin.ToString());
					streamWriter.WriteLine("CLSSTRIKEMRNG|" + Settings.CloseStrikeRangeKM.ToString(Util.ci));
					streamWriter.WriteLine("MINCLSRATE|" + Settings.MinCloseStrikeRate.ToString());
					streamWriter.WriteLine("ALLCLRPERIOD|" + Settings.AllClearPeriod.ToString());
					streamWriter.WriteLine("DISTANCEUNITS|" + Settings.DistanceUnits.ToString());
					streamWriter.WriteLine("SENDSTRIKEALERT|" + Settings.mSendStrikeAlert.ToString(Util.ci));
					streamWriter.WriteLine("SENDALLCLEAR|" + Settings.mSendClear.ToString(Util.ci));
					streamWriter.WriteLine("SENDSTRIKEALERTTGM|" + Settings.tSendStrikeAlert.ToString(Util.ci));
					streamWriter.WriteLine("SENDALLCLEARTGM|" + Settings.tSendClear.ToString(Util.ci));
					streamWriter.WriteLine("COMIOON|" + Settings.comMsgEnabled.ToString(Util.ci));
					streamWriter.WriteLine("COMSTRIKEIOON|" + Settings.comStrikeMsgEnabled.ToString(Util.ci));
					streamWriter.WriteLine("COMPORT|" + TxComHandler.hwPort.PortName);
					streamWriter.WriteLine("COMBAUD|" + TxComHandler.hwPort.BaudRate.ToString());
					streamWriter.WriteLine("EXPJSON|" + Settings.exportJSON.ToString(Util.ci));
					streamWriter.WriteLine("STOREDB|" + Settings.storeDB.ToString(Util.ci));
					streamWriter.WriteLine("DBPATH|" + Settings.dbPath);
					streamWriter.WriteLine("MAILSRV|" + Settings.mServer);
					streamWriter.WriteLine("MAILPRT|" + Settings.mPort.ToString());
					streamWriter.WriteLine("MAILSEC|" + Settings.mSecIndex.ToString());
					streamWriter.WriteLine("MAILAUT|" + Settings.mAuthIndex.ToString());
					streamWriter.WriteLine("MAILUSR|" + Settings.mUsername);
					streamWriter.WriteLine("MAILPWD|" + Settings.mPassword);
					streamWriter.WriteLine("MAILFRM|" + Settings.mFrom);
					string text = "MAILRCP|";
					int num = 0;
					foreach (string mTo in Settings.mToList)
					{
						text += ((num++ == 0) ? mTo : (";" + mTo));
					}
					streamWriter.WriteLine(text);
					streamWriter.WriteLine("MAILALERTSUBJ|" + Settings.mAlarmSubj);
					streamWriter.WriteLine("MAILALERTBODY|" + Settings.mAlarmBody);
					streamWriter.WriteLine("MAILCLEARSUBJ|" + Settings.mClearSubj);
					streamWriter.WriteLine("MAILCLEARBODY|" + Settings.mClearBody);
					streamWriter.WriteLine("TBOTTOKEN|" + Settings.tBotToken);
					streamWriter.WriteLine("TDESTID|" + Settings.tDestId.ToString());
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Save configuration settings failed:" + ex.InnerException, "File error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}

		private static object getSetting(object obj, string val, object defaultVal)
		{
			if (obj is int)
			{
				try
				{
					obj = int.Parse(val);
				}
				catch
				{
					obj = defaultVal;
				}
			}
			if (obj is long)
			{
				try
				{
					obj = long.Parse(val);
					return obj;
				}
				catch
				{
					obj = defaultVal;
					return obj;
				}
			}
			if (obj is string)
			{
				obj = ((val.Length > 0) ? val : defaultVal);
			}
			else
			{
				if (obj is double)
				{
					try
					{
						obj = double.Parse(val, Util.ci);
						return obj;
					}
					catch
					{
						obj = defaultVal;
						return obj;
					}
				}
				if (obj is bool)
				{
					try
					{
						obj = bool.Parse(val);
						return obj;
					}
					catch
					{
						obj = defaultVal;
						return obj;
					}
				}
			}
			return obj;
		}
	}
}
