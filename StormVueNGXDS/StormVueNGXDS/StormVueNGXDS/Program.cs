using System;
using System.Windows.Forms;

namespace StormVue2RTCM
{
	internal static class Program
	{
		private static string appGuid = "ngxds-2-instance_mutex";

		[STAThread]
		private static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1());
		}
	}
}
