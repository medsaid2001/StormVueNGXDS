using System;

namespace StormVue2RTCM
{
	public class LightningData
	{
		public static int puid = -999;

		public int uid;

		public int year;

		public int month;

		public int day;

		public int timestamp_sec;

		public DateTime timestamp;

		public double bng;

		public double dist;

		public int compound_type;

		public int type;

		public int polarity;

		public bool correlated;

		public LightningData()
		{
			this.uid = -1;
			this.year = 2000;
			this.month = 1;
			this.day = 1;
			this.timestamp_sec = -1;
			this.timestamp = new DateTime(this.year, this.month, this.day);
			this.bng = -1.0;
			this.dist = -1.0;
			this.compound_type = -1;
			this.type = StrikeDataTypes.UNCATEGORIZED;
			this.polarity = -1;
			this.correlated = false;
		}

		public static LightningData ParseLightningData(string s)
		{
			LightningData lightningData = null;
			string[] array = s.Split(',');
			if (array != null && array.Length != 0)
			{
				int num = -99;
				try
				{
					num = int.Parse(array[5]);
				}
				catch (Exception)
				{
				}
				if (num > -1)
				{
					try
					{
						lightningData = new LightningData();
						lightningData.uid = int.Parse(array[0]);
						if (LightningData.puid == lightningData.uid)
						{
							return null;
						}
						LightningData.puid = lightningData.uid;
						lightningData.year = int.Parse(array[1]);
						lightningData.month = int.Parse(array[2]);
						lightningData.day = int.Parse(array[3]);
						lightningData.timestamp_sec = int.Parse(array[4]);
						lightningData.timestamp = new DateTime(lightningData.year, lightningData.month, lightningData.day);
						lightningData.timestamp = lightningData.timestamp.AddSeconds(Convert.ToDouble(lightningData.timestamp_sec));
						lightningData.bng = Convert.ToDouble(int.Parse(array[5])) * 0.1;
						lightningData.bng += HeaderData.antAlignment;
						if (lightningData.bng > 360.0)
						{
							lightningData.bng -= 360.0;
						}
						else if (lightningData.bng < 0.0)
						{
							lightningData.bng += 360.0;
						}
						lightningData.dist = Convert.ToDouble(int.Parse(array[6]));
						lightningData.correlated = ((byte)((array[11] == "1") ? 1 : 0) != 0);
						lightningData.type = int.Parse(array[13]);
						lightningData.polarity = int.Parse(array[14]);
						if (lightningData.type == StrikeDataTypes.CG)
						{
							lightningData.compound_type = ((lightningData.polarity == StrikeDataTypes.POS) ? StrikeDataTypes.PCG : StrikeDataTypes.NCG);
						}
						else if (lightningData.type == StrikeDataTypes.IC)
						{
							lightningData.compound_type = StrikeDataTypes.PNIC;
						}
						else
						{
							lightningData.compound_type = StrikeDataTypes.UNCATEGORIZED;
						}
					}
					catch (Exception)
					{
						lightningData = null;
					}
				}
				else
				{
					switch (num)
					{
					case -9:
						HeaderData.antAlignment = Convert.ToDouble(int.Parse(array[7]));
						break;
					case -1:
						HeaderData.noiseCount++;
						break;
					}
				}
				IPCStatus.IPC1Online = true;
				IPCStatus.IPC1ActTmr = DateTime.Now;
			}
			return lightningData;
		}
	}
}
