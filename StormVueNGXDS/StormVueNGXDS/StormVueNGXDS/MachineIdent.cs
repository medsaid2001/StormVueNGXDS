using System;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace StormVue2RTCM
{
	internal class MachineIdent
	{
		private string baseIDStr;

		public MachineIdent()
		{
			this.baseIDStr = MachineIdent.baseId();
			if (string.IsNullOrEmpty(this.baseIDStr))
			{
				this.baseIDStr = Environment.OSVersion.ToString() + Environment.ProcessorCount.ToString();
			}
			this.baseIDStr = MachineIdent.GetHash(this.baseIDStr);
		}

		public string get()
		{
			return Environment.MachineName + "." + this.baseIDStr;
		}

		private static string GetHash(string s)
		{
			MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
			byte[] bytes = new ASCIIEncoding().GetBytes(s);
			return MachineIdent.GetHexString(mD5CryptoServiceProvider.ComputeHash(bytes));
		}

		public static string GetHexString(byte[] bt)
		{
			string text = string.Empty;
			for (int i = 0; i < bt.Length; i++)
			{
				byte num = bt[i];
				int num2 = num & 0xF;
				int num3 = num >> 4 & 0xF;
				char c;
				if (num3 > 9)
				{
					string str = text;
					c = (char)(num3 - 10 + 65);
					text = str + c.ToString();
				}
				else
				{
					text += num3.ToString();
				}
				if (num2 > 9)
				{
					string str2 = text;
					c = (char)(num2 - 10 + 65);
					text = str2 + c.ToString();
				}
				else
				{
					text += num2.ToString();
				}
				if (i + 1 != bt.Length && (i + 1) % 2 == 0)
				{
					text += "-";
				}
			}
			return text;
		}

		private static string cpuId()
		{
			string text = MachineIdent.identifier("Win32_Processor", "ProcessorId");
			if (text == "")
			{
				text = MachineIdent.identifier("Win32_Processor", "Name");
				if (text == "")
				{
					text = MachineIdent.identifier("Win32_Processor", "Manufacturer");
				}
			}
			return text;
		}

		private string biosId()
		{
			return MachineIdent.identifier("Win32_BIOS", "SerialNumber");
		}

		private static string diskId()
		{
			return MachineIdent.identifier("Win32_DiskDrive", "Signature");
		}

		private static string baseId()
		{
			return MachineIdent.identifier("Win32_BaseBoard", "SerialNumber");
		}

		private static string identifier(string wmiClass, string wmiProperty)
		{
			string text = "";
			foreach (ManagementObject instance in new ManagementClass(wmiClass).GetInstances())
			{
				if (text == "")
				{
					try
					{
						text = ((ManagementBaseObject)instance)[wmiProperty].ToString();
						return text;
					}
					catch
					{
					}
				}
			}
			return text;
		}
	}
}
