using System;

namespace StormVue2RTCM
{
	internal class TimeEx
	{
		private static readonly DateTime StartOfEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public static long timeToMillisUTC(DateTime dt)
		{
			return Convert.ToInt64((dt.ToUniversalTime() - TimeEx.StartOfEpoch).TotalMilliseconds);
		}

		public static long timestampEpoch()
		{
			return TimeEx.timeToMillisUTC(DateTime.Now);
		}

		public static DateTime ConvertUnixEpochTime(long milliseconds)
		{
			DateTime dateTime = TimeEx.StartOfEpoch;
			dateTime = dateTime.ToUniversalTime();
			return dateTime.AddMilliseconds((double)milliseconds);
		}

		public static long secondsToMillis(double secs)
		{
			return Convert.ToInt64(secs * 1000.0);
		}

		public static int minutesToMillis(int min)
		{
			return min * 60000;
		}

		public static int millisToMinutes(long millis)
		{
			return Convert.ToInt32(millis / 60000);
		}

		public static int minutesToSeconds(int min)
		{
			return min * 60;
		}

		public static long ticksToMillis(long nanos)
		{
			return nanos / 10000;
		}

		public static long millisToTicks(long millis)
		{
			DateTime dateTime = TimeEx.StartOfEpoch;
			dateTime = dateTime.AddMilliseconds((double)millis);
			return dateTime.Ticks;
		}

		public static DateTime millisToDateTime(long millis)
		{
			return TimeEx.StartOfEpoch.AddMilliseconds((double)millis);
		}
	}
}
