using System;

namespace StormVue2RTCM
{
	public class TRACData
	{
		public int uid;

		public int year;

		public int month;

		public int day;

		public int timestamp_sec;

		public int detect_timestamp_sec;

		public int lastactive_timestamp_sec;

		public bool isDeletionMsg;

		public DateTime timestamp;

		public DateTime detect_timestamp;

		public DateTime lastactive_timestamp;

		public string TRACId;

		public double bng;

		public double dist;

		public int strikerate;

		public int max_strikerate;

		public int category;

		public int trend;

		public int tot_strikes;

		public int pos_cg_strikes;

		public int neg_cg_strikes;

		public int pos_ic_strikes;

		public int neg_ic_strikes;

		public double CA_dist;

		public int SA_rate;

		public int SSC_rate;

		public bool CA_trig;

		public bool SA_trig;

		public bool SSC_trig;

		public TRACData()
		{
			this.uid = -1;
			this.year = 2000;
			this.month = 1;
			this.day = 1;
			this.timestamp_sec = (this.lastactive_timestamp_sec = (this.detect_timestamp_sec = -1));
			this.isDeletionMsg = false;
			this.timestamp = new DateTime(this.year, this.month, this.day);
			this.detect_timestamp = new DateTime(this.year, this.month, this.day);
			this.lastactive_timestamp = new DateTime(this.year, this.month, this.day);
			this.TRACId = "?-0000";
			this.bng = -1.0;
			this.dist = -1.0;
			this.strikerate = (this.max_strikerate = 0);
			this.category = (this.trend = 0);
			this.tot_strikes = (this.pos_cg_strikes = (this.neg_cg_strikes = (this.pos_ic_strikes = (this.neg_ic_strikes = 0))));
			this.CA_dist = 99999999.0;
			this.SA_rate = 99999999;
			this.SSC_rate = 99999999;
			this.CA_trig = false;
			this.SA_trig = false;
			this.SSC_trig = false;
		}

		public static TRACData ParseTRACData(string s)
		{
			TRACData tRACData = null;
			string[] array = s.Split(',');
			if (array != null && array.Length != 0)
			{
				int num = -99;
				try
				{
					num = int.Parse(array[7]);
				}
				catch (Exception)
				{
				}
				switch (num)
				{
				default:
					try
					{
						tRACData = new TRACData();
						tRACData.uid = int.Parse(array[0]);
						tRACData.timestamp_sec = int.Parse(array[1]);
						tRACData.year = int.Parse(array[2]);
						tRACData.month = int.Parse(array[3]);
						tRACData.day = int.Parse(array[4]);
						tRACData.detect_timestamp_sec = int.Parse(array[5]);
						if (tRACData.year == 0 && tRACData.month == 0 && tRACData.day == 0)
						{
							tRACData.isDeletionMsg = true;
						}
						tRACData.TRACId = array[6].Trim();
						TRACData tRACData2 = tRACData;
						tRACData2.TRACId = tRACData2.TRACId + "-" + array[7].Trim();
						if (!tRACData.isDeletionMsg)
						{
							tRACData.detect_timestamp = new DateTime(tRACData.year, tRACData.month, tRACData.day);
							tRACData.detect_timestamp = tRACData.detect_timestamp.AddSeconds(Convert.ToDouble(tRACData.detect_timestamp_sec));
							tRACData.lastactive_timestamp_sec = int.Parse(array[8]);
							tRACData.lastactive_timestamp = new DateTime(tRACData.year, tRACData.month, tRACData.day);
							if (tRACData.lastactive_timestamp_sec < tRACData.detect_timestamp_sec)
							{
								tRACData.lastactive_timestamp = tRACData.lastactive_timestamp.AddDays(1.0);
							}
							tRACData.lastactive_timestamp = tRACData.lastactive_timestamp.AddSeconds(Convert.ToDouble(tRACData.lastactive_timestamp_sec));
							tRACData.bng = Convert.ToDouble(int.Parse(array[9])) * 0.1;
							tRACData.bng += HeaderData.antAlignment;
							if (tRACData.bng > 360.0)
							{
								tRACData.bng -= 360.0;
							}
							else if (tRACData.bng < 0.0)
							{
								tRACData.bng += 360.0;
							}
							tRACData.dist = Convert.ToDouble(int.Parse(array[10]));
							tRACData.strikerate = int.Parse(array[13]);
							tRACData.max_strikerate = int.Parse(array[14]);
							tRACData.category = int.Parse(array[15]);
							tRACData.trend = int.Parse(array[16]);
							tRACData.tot_strikes = int.Parse(array[17]);
							tRACData.pos_cg_strikes = int.Parse(array[18]);
							tRACData.neg_cg_strikes = int.Parse(array[19]);
							tRACData.pos_ic_strikes = int.Parse(array[20]);
							tRACData.neg_ic_strikes = int.Parse(array[21]);
							tRACData.CA_dist = Convert.ToDouble(int.Parse(array[22]));
							tRACData.SA_rate = int.Parse(array[23]);
							tRACData.SSC_rate = int.Parse(array[24]);
							tRACData.CA_trig = ((byte)(array[25].Trim().Equals("1") ? 1 : 0) != 0);
							tRACData.SA_trig = ((byte)(array[26].Trim().Equals("1") ? 1 : 0) != 0);
							tRACData.SSC_trig = ((byte)(array[27].Trim().Equals("1") ? 1 : 0) != 0);
						}
					}
					catch (Exception)
					{
						tRACData = null;
					}
					break;
				case -9:
					HeaderData.antAlignment = Convert.ToDouble(int.Parse(array[9]));
					if (HeaderData.antAlignment >= 180.0)
					{
						HeaderData.antAlignment -= 360.0;
					}
					break;
				}
				IPCStatus.IPC2ActTmr = DateTime.Now;
				IPCStatus.IPC2Online = true;
			}
			return tRACData;
		}
	}
}
