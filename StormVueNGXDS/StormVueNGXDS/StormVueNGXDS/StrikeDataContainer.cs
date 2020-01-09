using System.Collections.Generic;

namespace StormVue2RTCM
{
	public class StrikeDataContainer
	{
		public long Timestamp
		{
			get;
			set;
		}

		public int Count
		{
			get;
			set;
		}

		public List<StrikeData> StrikeData
		{
			get;
			set;
		}

		public StrikeDataContainer()
		{
			this.StrikeData = new List<StrikeData>();
		}
	}
}
