using System;
using System.Collections.Generic;
using System.Threading;

namespace StormVue2RTCM
{
	internal class Syslogger
	{
		private static Queue<string> log = new Queue<string>();

		public static void AddMsg(string caller, string msg)
		{
			string item = DateTime.Now.ToString() + " " + caller + "::" + msg;
			Monitor.Enter(Syslogger.log);
			Syslogger.log.Enqueue(item);
			Monitor.Exit(Syslogger.log);
		}

		public static string[] GetMessages()
		{
			Monitor.Enter(Syslogger.log);
			string[] result = Syslogger.log.ToArray();
			Monitor.Exit(Syslogger.log);
			return result;
		}

		public static void Trim()
		{
			Monitor.Enter(Syslogger.log);
			while (Syslogger.log.Count > 4096)
			{
				Syslogger.log.Dequeue();
			}
			Monitor.Exit(Syslogger.log);
		}
	}
}
