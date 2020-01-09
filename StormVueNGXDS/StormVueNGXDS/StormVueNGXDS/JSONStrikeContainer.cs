using System.Collections.Generic;

namespace StormVue2RTCM
{
	internal class JSONStrikeContainer
	{
		public long TimestampEpoch
		{
			get;
			set;
		}

		public double StationLat
		{
			get;
			set;
		}

		public double StationLon
		{
			get;
			set;
		}

		public string SoftwareVersion
		{
			get;
			set;
		}

		public string DetectorModel
		{
			get;
			set;
		}

		public double AntennaAlignment
		{
			get;
			set;
		}

		public int StrikeCount
		{
			get;
			set;
		}

		public int StormCount
		{
			get;
			set;
		}

		public List<StrikeData> StrikeData
		{
			get;
			set;
		}

		public List<StormData> StormData
		{
			get;
			set;
		}

		public JSONStrikeContainer()
		{
			this.TimestampEpoch = 0L;
			this.SoftwareVersion = "0.0.0.0";
			this.DetectorModel = "Unknown";
			this.StationLat = 0.0;
			this.StationLon = 0.0;
			this.AntennaAlignment = 0.0;
			this.StrikeCount = 0;
			this.StormCount = 0;
			this.StrikeData = new List<StrikeData>();
			this.StormData = new List<StormData>();
		}
	}
}
