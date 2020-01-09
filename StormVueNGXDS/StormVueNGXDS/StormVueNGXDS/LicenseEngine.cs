using System;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;

namespace StormVue2RTCM
{
	internal class LicenseEngine
	{
		private string licFile = Settings.appStorePath + "\\ngxds.key";

		private string ISOLICENSE_FILE = "ngxds.key";

		private string licStorePath;

		private const string dateFormat = "yyyy-MM-dd HH:mm:ss";

		public LicenseEngine()
		{
			this.licStorePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\DSAppData\\activator\\";
			if (!Directory.Exists(this.licStorePath))
			{
				Directory.CreateDirectory(this.licStorePath);
			}
			this.ISOLICENSE_FILE = this.licStorePath + "ngxds.key";
		}

		public void StartupCheck()
		{
			if (File.Exists(this.ISOLICENSE_FILE))
			{
				this.InspectFileISO();
			}
			else
			{
				this.CreateTrialKeyISO();
			}
		}

		private int InspectFileISO()
		{
			int num = 0;
			try
			{
				string text = new StreamReader(this.ISOLICENSE_FILE).ReadLine();
				Console.WriteLine("License data: {0}", text);
				if (text.Length > 1 && text.StartsWith("T"))
				{
					Settings.trialExpDate = this.GetExpirationDate(text);
					if (DateTime.Compare(Settings.trialExpDate, DateTime.Now) <= 0)
					{
						Settings.trialExpired = true;
					}
					return 1;
				}
				if (text.Length > 1 && text.StartsWith("L"))
				{
					Settings.licenseValid = this.IsLicenseValid(text);
					return 2;
				}
				return 0;
			}
			catch (Exception ex)
			{
				Console.WriteLine("InspectFileISO error : {0}", ex.Message);
				return 0;
			}
		}

		private DateTime GetExpirationDate(string s)
		{
			if (s.Length < 2)
			{
				return DateTime.Now;
			}
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			try
			{
				return DateTime.ParseExact(EncryptionEngine.Decrypt(s.Substring(1), true), "yyyy-MM-dd HH:mm:ss", invariantCulture);
			}
			catch (Exception ex)
			{
				Console.WriteLine("GetExpirationDate error : {0}", ex.Message);
				return DateTime.Now;
			}
		}

		private bool IsLicenseValid(string s)
		{
			if (s.Length < 2)
			{
				return false;
			}
			string a = EncryptionEngine.Decrypt(s.Substring(1), true);
			string value = new MachineIdent().get();
			value = EncryptionEngine.R13(value);
			return a == value;
		}

		private bool FileExistsISO()
		{
			string[] fileNames = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null).GetFileNames(this.ISOLICENSE_FILE);
			for (int i = 0; i < fileNames.Length; i++)
			{
				if (fileNames[i] == this.ISOLICENSE_FILE)
				{
					return true;
				}
			}
			return false;
		}

		private bool CreateTrialKeyISO()
		{
			try
			{
				using (StreamWriter streamWriter = new StreamWriter(this.ISOLICENSE_FILE, false))
				{
					DateTime trialExpDate = DateTime.Now.AddDays(21.0);
					string value = "T" + EncryptionEngine.Encrypt(trialExpDate.ToString("yyyy-MM-dd HH:mm:ss"), true);
					streamWriter.WriteLine(value);
					streamWriter.Close();
					Settings.trialExpDate = trialExpDate;
					Settings.trialExpired = false;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Trial create error : {0}", ex.Message);
				return false;
			}
			return true;
		}

		public bool WriteLicenseDataISO(string licData)
		{
			try
			{
				using (StreamWriter streamWriter = new StreamWriter(this.ISOLICENSE_FILE, false))
				{
					string value = "L" + EncryptionEngine.Encrypt(licData, true);
					streamWriter.WriteLine(value);
					streamWriter.Close();
					Settings.trialExpired = false;
					Settings.licenseValid = true;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("License write error : {0}", ex.Message);
				return false;
			}
			return true;
		}

		public bool DeletFileISO()
		{
			IsolatedStorageFile store = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
			try
			{
				store.DeleteFile(this.ISOLICENSE_FILE);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Deletion error : {0}", ex.Message);
				return false;
			}
			return true;
		}
	}
}
