namespace StormVue2RTCM
{
	internal class StormData
	{
		public static string NMEAMarker = "@";

		public static string Sep = ",";

		public static string DATAID_RT = "RTTRC";

		public static string DATAID_ARC = "ARTRC";

		public static string End = "*";

		public string stormID;

		public double NSTimestamp_s;

		public long detected_millis;

		public long lastactive_millis;

		public double dist;

		public double bng;

		public double lat;

		public double lon;

		public double CloseAlarmNSRangeKM = 25.0;

		public double SevereAlarmLimRate = 50.0;

		public double SevereCellLimRate = 50.0;

		public bool CloseAlarmNSTriggered;

		public bool SevereAlarmNSTriggered;

		public bool SevereCellNSTriggered;

		public bool deleted;

		public int NMEAChecksum;

		public StormData()
		{
			this.stormID = "";
			this.NSTimestamp_s = 0.0;
			this.detected_millis = 0L;
			this.lastactive_millis = 0L;
			this.dist = -1.0;
			this.bng = 0.0;
			this.lat = 0.0;
			this.lon = 0.0;
			this.CloseAlarmNSRangeKM = -1.0;
			this.SevereAlarmLimRate = -1.0;
			this.SevereCellLimRate = -1.0;
			this.CloseAlarmNSTriggered = false;
			this.SevereAlarmNSTriggered = false;
			this.SevereCellNSTriggered = false;
			this.deleted = false;
			this.NMEAChecksum = 255;
		}
	}
}
