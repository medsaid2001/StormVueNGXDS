using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows.Forms;

namespace StormVue2RTCM
{
	internal class VersionCheck
	{
		public static string CheckVersion()
		{
			string text = "";
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("http://www.astrogenic.com/vc/.ngx/vercheck.txt");
			try
			{
				httpWebRequest.MaximumAutomaticRedirections = 4;
				httpWebRequest.Timeout = 8000;
				text = new StreamReader(((HttpWebResponse)httpWebRequest.GetResponse()).GetResponseStream()).ReadToEnd();
			}
			catch (WebException ex)
			{
				MessageBox.Show(ex.Message, "Server communication error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return "-1";
			}
			return VersionCheck.CompareVersion(Assembly.GetExecutingAssembly().GetName().Version.ToString(), text.Trim());
		}

		private static string CompareVersion(string currVer, string newVer)
		{
			Version v = new Version(currVer);
			Version version = new Version(newVer);
			string result = "";
			if (v < version)
			{
				result = version.ToString();
			}
			return result;
		}
	}
}
