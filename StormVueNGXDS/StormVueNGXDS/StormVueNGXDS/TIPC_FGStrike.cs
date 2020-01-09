using StormVue2RTCM;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace StormVue2RTCM
{
	internal class TIPC_FGStrike
	{
		private static string WriterSemaphoreName = "Writer Semaphore";

		private static string ReaderSemaphoreName = "Reader Semaphore";

		private const string IPCName = "NXFGIPC_SHMEM_0822931589443_238731_GATE0";

		private bool SHUTDOWN;

		private string szData;

		private ManualResetEvent m_EventStop;

		private ManualResetEvent m_EventStopped;

		private Form1 m_form;

		private IntPtr hFileMapping;

		private IntPtr pViewOfFile;

		private IntPtr hWriterSemaphore;

		private IntPtr hReaderSemaphore;

		public TIPC_FGStrike(ManualResetEvent eventStop, ManualResetEvent eventStopped, Form1 form)
		{
			this.m_EventStop = eventStop;
			this.m_EventStopped = eventStopped;
			this.m_form = form;
			this.SHUTDOWN = false;
		}

		public void Run()
		{
			bool flag = true;
			this.IPCRoutineInit();
			Syslogger.AddMsg("TIPC_FGStrike", "IPC-1 has started");
			try
			{
				while (true)
				{
					if (flag && !this.SHUTDOWN)
					{
						this.szData = "";
						flag = this.IPCRead();
						if (!this.m_EventStop.WaitOne(0, true))
						{
							if (this.szData.Length > 0)
							{
								this.m_form.Invoke(this.m_form.m_DelegateFG1DataRec, this.szData);
							}
							Thread.Sleep(10);
							continue;
						}
						break;
					}
					return;
				}
				this.m_EventStopped.Set();
				flag = false;
			}
			catch (ThreadAbortException)
			{
			}
			finally
			{
				this.IPCRoutineShutdown();
				try
				{
					this.m_form.Invoke(this.m_form.m_DelegateFG1ThreadFinished, null);
				}
				catch (Exception)
				{
				}
			}
		}

		public bool IPCRoutineInit()
		{
			this.hFileMapping = Win32API.CreateFileMappingW(-1, IntPtr.Zero, 4, 0, 256, "NXFGIPC_SHMEM_0822931589443_238731_GATE0");
			int lastWin32Error = Marshal.GetLastWin32Error();
			if (this.hFileMapping == IntPtr.Zero)
			{
				return false;
			}
			this.pViewOfFile = Win32API.MapViewOfFile(this.hFileMapping, 4, 0, 0, 256);
			lastWin32Error = Marshal.GetLastWin32Error();
			if (this.pViewOfFile == IntPtr.Zero)
			{
				return false;
			}
			return true;
		}

		public void IPCRoutineShutdown()
		{
			if (this.pViewOfFile != IntPtr.Zero)
			{
				Win32API.UnmapViewOfFile(this.pViewOfFile);
			}
			if (this.hFileMapping != IntPtr.Zero)
			{
				Win32API.CloseHandle(this.hFileMapping);
			}
		}

		public bool IPCRead()
		{
			this.hWriterSemaphore = Win32API.CreateSemaphoreA(null, 1, 1, TIPC_FGStrike.WriterSemaphoreName);
			if (this.hWriterSemaphore == IntPtr.Zero)
			{
				return false;
			}
			this.hReaderSemaphore = Win32API.CreateSemaphoreA(null, 0, 1, TIPC_FGStrike.ReaderSemaphoreName);
			if (this.hReaderSemaphore == IntPtr.Zero)
			{
				Win32API.CloseHandle(this.hWriterSemaphore);
				return false;
			}
			if (Win32API.WaitForSingleObject(this.hReaderSemaphore, 100u) == 0)
			{
				byte[] array = new byte[1024];
				for (int i = 0; i < array.Length; i++)
				{
					byte b = Marshal.ReadByte(this.pViewOfFile, i);
					if (b == 0)
					{
						break;
					}
					array[i] = b;
				}
				this.szData = Encoding.ASCII.GetString(array);
				int num = this.szData.IndexOf("\0");
				if (num != -1)
				{
					this.szData = this.szData.Substring(0, num);
				}
				this.szData = this.szData.Trim();
			}
			int num2 = default(int);
			Win32API.ReleaseSemaphore(this.hReaderSemaphore, 1, out num2);
			Win32API.ReleaseSemaphore(this.hWriterSemaphore, 1, out num2);
			Win32API.CloseHandle(this.hWriterSemaphore);
			Win32API.CloseHandle(this.hReaderSemaphore);
			return true;
		}
	}
}
