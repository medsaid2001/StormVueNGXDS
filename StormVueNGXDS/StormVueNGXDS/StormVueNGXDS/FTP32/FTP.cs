using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace FTP32
{
	public class FTP
	{
		public delegate void LastResponseEventHandler(string strResponse);

		public enum FileTransferType
		{
			ftpUnknown,
			ftpAscii,
			ftpBinary
		}

		public enum ConnectionModes
		{
			Active,
			Passive = 0x8000000
		}

		private struct FILETIME
		{
			private int dwLowDateTime;

			private int dwHighDateTime;
		}

		public struct WIN32_FIND_DATA
		{
			public int dwFileAttributes;

			private FILETIME ftCreationTime;

			private FILETIME ftLastAccessTime;

			private FILETIME ftLastWriteTime;

			private int nFileSizeHigh;

			private int nFileSizeLow;

			private int dwReserved0;

			private int dwReserved1;
		}

		private const int FTP_TRANSFER_TYPE_UNKNOWN = 0;

		private const int FTP_TRANSFER_TYPE_BINARY = 2;

		private const int FTP_TRANSFER_TYPE_ASCII = 1;

		private const int INTERNET_OPEN_TYPE_DIRECT = 1;

		private const int INTERNET_SERVICE_FTP = 1;

		private const int INTERNET_FLAG_PASSIVE = 134217728;

		private const int FILE_ATTRIBUTE_READONLY = 1;

		private const int FILE_ATTRIBUTE_HIDDEN = 2;

		private const int FILE_ATTRIBUTE_SYSTEM = 4;

		private const int FILE_ATTRIBUTE_DIRECTORY = 16;

		private const int FILE_ATTRIBUTE_ARCHIVE = 32;

		private const int FILE_ATTRIBUTE_NORMAL = 128;

		private const int FILE_ATTRIBUTE_TEMPORARY = 256;

		private const int FILE_ATTRIBUTE_COMPRESSED = 2048;

		private const int FILE_ATTRIBUTE_OFFLINE = 4096;

		private int lngInternetSession;

		private int lngFTPSession;

		[MarshalAs(UnmanagedType.LPTStr)]
		private string strErr;

		private FileTransferType mTransferType = FileTransferType.ftpBinary;

		private LastResponseEventHandler LastResponseEvent;

		private string AppDir = "";

		private const int MAX_PATH = 260;

		public FileTransferType TransferType
		{
			get
			{
				return this.mTransferType;
			}
			set
			{
				this.mTransferType = FileTransferType.ftpBinary;
			}
		}

		public bool IsConnected
		{
			get
			{
				return this.lngFTPSession != 0;
			}
		}

		public event LastResponseEventHandler LastResponse
		{
			add
			{
				this.LastResponseEvent = (LastResponseEventHandler)Delegate.Combine(this.LastResponseEvent, value);
			}
			remove
			{
				this.LastResponseEvent = (LastResponseEventHandler)Delegate.Remove(this.LastResponseEvent, value);
			}
		}

		[DllImport("wininet.dll", CharSet = CharSet.Ansi, EntryPoint = "InternetOpenA", ExactSpelling = true, SetLastError = true)]
		private static extern int InternetOpen(string sAgent, int lAccessType, string sProxyName, string sProxyBypass, int lFlags);

		[DllImport("wininet.dll", CharSet = CharSet.Ansi, EntryPoint = "InternetConnectA", ExactSpelling = true, SetLastError = true)]
		private static extern int InternetConnect(int hInternetSession, string sServerName, int nServerPort, string sUserName, string sPassword, int lService, int lFlags, int lContext);

		[DllImport("wininet.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
		private static extern int InternetCloseHandle(int hInet);

		[DllImport("wininet.dll", CharSet = CharSet.Ansi, EntryPoint = "InternetGetLastResponseInfoA", ExactSpelling = true, SetLastError = true)]
		private static extern bool InternetGetLastResponseInfo(out int lpdwError, [Out] [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpszBuffer, out int lpdwBufferLength);

		[DllImport("wininet.dll", CharSet = CharSet.Ansi, EntryPoint = "FtpSetCurrentDirectoryA", ExactSpelling = true, SetLastError = true)]
		private static extern bool FtpSetCurrentDirectory(int hFtpSession, string lpszDirectory);

		[DllImport("wininet.dll", CharSet = CharSet.Ansi, EntryPoint = "FtpGetCurrentDirectoryA", ExactSpelling = true, SetLastError = true)]
		private static extern int FtpGetCurrentDirectory(int hFtpSession, string lpszCurrentDirectory, int lpdwCurrentDirectory);

		[DllImport("wininet.dll", CharSet = CharSet.Ansi, EntryPoint = "FtpCreateDirectoryA", ExactSpelling = true, SetLastError = true)]
		private static extern bool FtpCreateDirectory(int hFtpSession, string lpszDirectory);

		[DllImport("wininet.dll", CharSet = CharSet.Ansi, EntryPoint = "FtpRemoveDirectoryA", ExactSpelling = true, SetLastError = true)]
		private static extern bool FtpRemoveDirectory(int hFtpSession, string lpszDirectory);

		[DllImport("wininet.dll", CharSet = CharSet.Ansi, EntryPoint = "FtpDeleteFileA", ExactSpelling = true, SetLastError = true)]
		private static extern bool FtpDeleteFile(int hFtpSession, string lpszFileName);

		[DllImport("wininet.dll", CharSet = CharSet.Ansi, EntryPoint = "FtpRenameFileA", ExactSpelling = true, SetLastError = true)]
		private static extern bool FtpRenameFile(int hFtpSession, string lpszExisting, string lpszNew);

		[DllImport("wininet.dll", CharSet = CharSet.Ansi, EntryPoint = "FtpPutFileA", ExactSpelling = true, SetLastError = true)]
		private static extern bool FtpPutFile(int hFtpSession, string lpszLocalFile, string lpszRemoteFile, int dwFlags, int dwContext);

		[DllImport("wininet.dll", CharSet = CharSet.Ansi, EntryPoint = "FtpGetFileA", ExactSpelling = true, SetLastError = true)]
		private static extern bool FtpGetFile(int hConnect, string lpszRemoteFile, string lpszNewFile, int fFailIfExists, int dwFlagsAndAttributes, int dwFlags, ref int dwContext);

		[DllImport("wininet.dll", CharSet = CharSet.Auto, EntryPoint = "FtpFindFirstFileA", ExactSpelling = true, SetLastError = true)]
		private static extern int FtpFindFirstFile(int hFtpSession, string lpszSearchFile, ref WIN32_FIND_DATA lpFindFileData, int dwFlags, int dwContent);

		[DllImport("wininet.dll", CharSet = CharSet.Auto, EntryPoint = "InternetFindNextFileA", ExactSpelling = true, SetLastError = true)]
		private static extern int InternetFindNextFile(int hFind, ref WIN32_FIND_DATA lpvFindData);

		public bool ConnectToServer(string Server, int Port, ConnectionModes ConnectionMode, string Username, string Password)
		{
			bool result = false;
			try
			{
				this.CloseConnection();
				if (Username == "")
				{
					Username = "anon@nowhere.com";
				}
				this.lngInternetSession = FTP.InternetOpen("StormVueNGX_DS", 1, null, null, 0);
				if (this.lngInternetSession != 0)
				{
					this.lngFTPSession = FTP.InternetConnect(this.lngInternetSession, Server, Port, Username, Password, 1, Convert.ToInt32(ConnectionMode), 0);
					if (this.lngFTPSession != 0)
					{
						result = true;
					}
				}
				return result;
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message, ex);
			}
			finally
			{
				this.GetLastInternetResponse();
			}
		}

		public void CloseConnection()
		{
			if (this.lngFTPSession != 0)
			{
				FTP.InternetCloseHandle(this.lngFTPSession);
				this.lngFTPSession = 0;
			}
			if (this.lngInternetSession != 0)
			{
				FTP.InternetCloseHandle(this.lngInternetSession);
			}
		}

		public bool PutFile(string LocalFilePath, string RemoteFilename)
		{
			try
			{
				return FTP.FtpPutFile(this.lngFTPSession, LocalFilePath, RemoteFilename, Convert.ToInt32(this.TransferType), 0);
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message, ex);
			}
			finally
			{
				this.GetLastInternetResponse();
			}
		}

		public bool DeleteFile(string strFile)
		{
			try
			{
				return FTP.FtpDeleteFile(this.lngFTPSession, strFile);
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message, ex);
			}
			finally
			{
				this.GetLastInternetResponse();
			}
		}

		public bool RenameFile(string oldFileName, string NewFileName)
		{
			try
			{
				return FTP.FtpRenameFile(this.lngFTPSession, oldFileName, NewFileName);
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message, ex);
			}
			finally
			{
				this.GetLastInternetResponse();
			}
		}

		public bool CreateDirectory(string DirectoryName)
		{
			try
			{
				return FTP.FtpCreateDirectory(this.lngFTPSession, DirectoryName + "\0");
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message, ex);
			}
			finally
			{
				this.GetLastInternetResponse();
			}
		}

		public bool RemoveDirectory(string DirectoryName)
		{
			try
			{
				return FTP.FtpRemoveDirectory(this.lngFTPSession, DirectoryName);
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message, ex);
			}
			finally
			{
				this.GetLastInternetResponse();
			}
		}

		public bool SetWorkingDir(string WorkingDir)
		{
			try
			{
				return FTP.FtpSetCurrentDirectory(this.lngFTPSession, WorkingDir);
			}
			catch (Exception ex)
			{
				throw new Exception(ex.Message, ex);
			}
			finally
			{
				this.GetLastInternetResponse();
			}
		}

		public int GetLastInternetResponse()
		{
			return 0;
		}

		public void LogMessage(string msg)
		{
			string path = Application.StartupPath + "\\dbglog.txt";
			StreamWriter streamWriter = File.Exists(path) ? File.AppendText(path) : File.CreateText(path);
			StreamWriter streamWriter2 = streamWriter;
			DateTime now = DateTime.Now;
			string arg = now.ToShortDateString();
			now = DateTime.Now;
			streamWriter2.WriteLine("{0} {1} - {2}", arg, now.ToLongTimeString(), msg);
			streamWriter.Flush();
			streamWriter.Close();
		}
	}
}
