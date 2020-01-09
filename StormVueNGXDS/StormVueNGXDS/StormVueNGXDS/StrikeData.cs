namespace StormVue2RTCM
{
	public class StrikeData
	{
		public static string NMEAMarker = "@";

		public static string Sep = ",";

		public static string DATAID_RT = "RTDAT";

		public static string DATAID_ARC = "ARDAT";

		public static string End = "*";

		public double time;

		public long millis;

		public double dist;

		public double bng;

		public double lat;

		public double lon;

		public int type;

		public int polarity;

		public int compoundType;

		public bool correlated;

		public int NMEAChecksum;

		public StrikeData()
		{
			this.time = 0.0;
			this.dist = -1.0;
			this.bng = 0.0;
			this.lat = 0.0;
			this.lon = 0.0;
			this.type = StrikeDataTypes.UNDEF;
			this.polarity = StrikeDataTypes.UNDEF;
			this.correlated = false;
			this.NMEAChecksum = 255;
		}
	}
}
