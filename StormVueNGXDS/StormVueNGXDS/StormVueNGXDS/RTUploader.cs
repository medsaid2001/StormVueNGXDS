using FTP32;
using System;
using System.IO;
using System.Threading;

namespace StormVue2RTCM
{
	internal class RTUploader
	{
		private FTP ftp;

		private FTPConnectInfo m_ftpcon;

		private ManualResetEvent m_EventStop;

		private ManualResetEvent m_EventStopped;

		private Form1 m_form;

		private bool _activateUpload;

		public bool Activate
		{
			get
			{
				return this._activateUpload;
			}
			set
			{
				this._activateUpload = value;
			}
		}

		public RTUploader(FTPConnectInfo ftpcon, ManualResetEvent eventStop, ManualResetEvent eventStopped, Form1 form)
		{
			this.m_EventStop = eventStop;
			this.m_EventStopped = eventStopped;
			this.m_form = form;
			this.m_ftpcon = ftpcon;
			this.ftp = new FTP();
			this._activateUpload = false;
		}

		public void SetFTPConnectInfo(FTPConnectInfo ftpcon)
		{
			this.m_ftpcon = ftpcon;
		}

		public void Run()
		{
			bool flag = true;
			try
			{
				do
				{
					if (this._activateUpload && !Settings.transferPaused && !this.UploadData() && !Settings.fileCopyMode)
					{
						Syslogger.AddMsg("RTUPloader()", "Connection error occured, resetting....");
						this.ftp.CloseConnection();
						this.m_form.Invoke(this.m_form.m_FTPThreadError);
					}
					if (this.m_EventStop.WaitOne(1, true))
					{
						this.m_EventStopped.Set();
						flag = false;
					}
				}
				while (flag);
			}
			catch (ThreadAbortException)
			{
			}
			catch (Exception)
			{
			}
			finally
			{
				Syslogger.AddMsg("RTUPloader()", "Shutting down");
				if (this.ftp.IsConnected)
				{
					this.ftp.CloseConnection();
					Syslogger.AddMsg("RTUPloader()", "Closing connection");
				}
				try
				{
					this.m_form.Invoke(this.m_form.m_DelegateFTPThreadFinished);
					this.m_EventStopped.Set();
				}
				catch (ObjectDisposedException)
				{
				}
				catch (Exception)
				{
				}
			}
		}

		public bool UploadData()
		{
			bool flag = true;
			if (Settings.fileCopyMode)
			{
				try
				{
					File.Copy(Settings.locSrcRealtime, Path.Combine(Settings.remoteStorePath, Settings.tgtRealtime), true);
					if (Settings.exportJSON)
					{
						File.Copy(Settings.locJSONFile, Path.Combine(Settings.remoteStorePath, Settings.tgtJSONRealtime), true);
					}
				}
				catch (IOException ex)
				{
					Syslogger.AddMsg("RTUploader()", "File.Copy exception: " + ex.Message);
					flag = false;
					Settings.UploadErrorCount++;
				}
				this._activateUpload = false;
				this.m_form.Invoke(this.m_form.m_FTPUploadComplete);
				return flag;
			}
			if (!this.ftp.IsConnected)
			{
				Syslogger.AddMsg("RTUPloader()", "Connecting to FTP server...");
				if (!this.ftp.ConnectToServer(this.m_ftpcon.server, this.m_ftpcon.port, this.m_ftpcon.conmode, this.m_ftpcon.username, this.m_ftpcon.password))
				{
					Syslogger.AddMsg("RTUPloader()", "ConnectToServer failed");
					flag = false;
				}
			}
			if (flag && !this.ftp.SetWorkingDir(Settings.remoteStorePath))
			{
				Syslogger.AddMsg("RTUPloader()", "SetWorkingDir failed (" + Settings.remoteStorePath + ")");
				flag = false;
			}
			if (flag && !this.ftp.PutFile(Settings.locSrcRealtime, Settings.tmpRealtime))
			{
				Syslogger.AddMsg("RTUPloader()", "PutFile failed (" + Settings.locSrcRealtime + "->" + Settings.tmpRealtime + ")");
				flag = false;
			}
			if (flag && !this.ftp.DeleteFile(Settings.tgtRealtime))
			{
				Syslogger.AddMsg("RTUPloader()", "DeleteFile failed (" + Settings.tgtRealtime + ")");
			}
			if (flag && !this.ftp.RenameFile(Settings.tmpRealtime, Settings.tgtRealtime))
			{
				Syslogger.AddMsg("RTUPloader()", "RenameFile failed (" + Settings.tmpRealtime + "->" + Settings.tgtRealtime + ")");
				flag = false;
			}
			if (flag && Settings.exportJSON)
			{
				if (flag && !this.ftp.PutFile(Settings.locJSONFile, Settings.tmpJSONRealtime))
				{
					Syslogger.AddMsg("RTUPloader()", "PutFile failed (" + Settings.locJSONFile + "->" + Settings.tmpJSONRealtime + ")");
					flag = false;
				}
				if (flag && !this.ftp.DeleteFile(Settings.tgtJSONRealtime))
				{
					Syslogger.AddMsg("RTUPloader()", "DeleteFile failed (" + Settings.tgtJSONRealtime + ")");
				}
				if (flag && !this.ftp.RenameFile(Settings.tmpJSONRealtime, Settings.tgtJSONRealtime))
				{
					Syslogger.AddMsg("RTUPloader()", "RenameFile failed (" + Settings.tmpJSONRealtime + "->" + Settings.tgtJSONRealtime + ")");
					flag = false;
				}
			}
			this._activateUpload = false;
			this.m_form.Invoke(this.m_form.m_FTPUploadComplete);
			return flag;
		}
	}
}
