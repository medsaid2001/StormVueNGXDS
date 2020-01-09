using System.Globalization;

namespace StormVue2RTCM
{
	internal class Util
	{
		public enum CloseActivityType
		{
			None,
			Strike,
			Storm,
			StrikeAndStorm
		}

		public static CultureInfo ci = new CultureInfo("en-US");
	}
}
