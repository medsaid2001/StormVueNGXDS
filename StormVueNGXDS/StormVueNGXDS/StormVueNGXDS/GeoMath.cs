using System;

namespace StormVue2RTCM
{
	public class GeoMath
	{
		public const double DEG = 57.295779513082323;

		public const double RAD = 0.017453292519943295;

		public const double Km2Nm = 0.539956803;

		public const double Nm2Km = 1.852;

		public const double r = 6371.0;

		public static double NLat;

		public static double SLat;

		public static double ELon;

		public static double WLon;

		public static double dLat;

		public static double dLon;

		public static double xScale;

		public static double yScale;

		public static void SetScaling(double NLat, double SLat, double ELon, double WLon, double w, double h)
		{
			GeoMath.NLat = NLat;
			GeoMath.SLat = SLat;
			GeoMath.ELon = ELon;
			GeoMath.WLon = WLon;
			GeoMath.dLat = Math.Abs(NLat - SLat);
			GeoMath.yScale = h / GeoMath.dLat;
			if (WLon > ELon)
			{
				WLon += 180.0;
			}
			GeoMath.dLon = Math.Abs(ELon - WLon);
			GeoMath.xScale = w / GeoMath.dLon;
		}

		public static void GetCoords(double lat, double lon, out double x, out double y)
		{
			if (lon <= GeoMath.WLon || lon >= GeoMath.ELon)
			{
				x = -1.0;
			}
			else
			{
				x = GeoMath.dLon - Math.Abs(GeoMath.ELon - lon);
				x *= GeoMath.xScale;
			}
			if (lat >= GeoMath.NLat || lat <= GeoMath.SLat)
			{
				y = -1.0;
			}
			else
			{
				y = Math.Abs(GeoMath.NLat - lat);
				y *= GeoMath.yScale;
			}
		}

		public static double Range(double lat1, double lon1, double lat2, double lon2)
		{
			double num = 57.295779513082323;
			double num2 = Math.Abs(lat1) / num;
			double num3 = Math.Abs(lon1) / num;
			double num4 = Math.Abs(lat2) / num;
			double num5 = Math.Abs(lon2) / num;
			double num6;
			try
			{
				double d = Math.Pow(Math.Sin((num4 - num2) / 2.0), 2.0) + Math.Cos(num2) * Math.Cos(num4) * Math.Pow(Math.Sin((num5 - num3) / 2.0), 2.0);
				num6 = 6370.0 * Math.Asin(Math.Min(1.0, Math.Sqrt(d)));
			}
			catch (Exception)
			{
				num6 = 0.0;
			}
			return num6 * 2.0;
		}

		public static void GCPoint(double lat, double lon, double tc, double d, out double nLat, out double nLon)
		{
			lat *= 0.017453292519943295;
			lon *= 0.017453292519943295;
			d = 0.00029088820866572158 * d;
			d *= 0.539956803;
			tc *= 0.017453292519943295;
			nLat = Math.Asin(Math.Sin(lat) * Math.Cos(d) + Math.Cos(lat) * Math.Sin(d) * Math.Cos(tc));
			if (Math.Cos(nLat) == 0.0)
			{
				nLon = lon;
			}
			else
			{
				double num = 6.2831853071795862;
				double num2 = lon + Math.Asin(Math.Sin(tc) * Math.Sin(d) / Math.Cos(nLat)) + 3.1415926535897931;
				nLon = num2 - num * Math.Floor(num2 / num) - 3.1415926535897931;
			}
			nLat /= 0.017453292519943295;
			nLon /= 0.017453292519943295;
		}

		public static void GCPointXY(double lat, double lon, double tc, double d, out double x, out double y)
		{
			double lat2 = default(double);
			double lon2 = default(double);
			GeoMath.GCPoint(lat, lon, tc, d, out lat2, out lon2);
			GeoMath.GetCoords(lat2, lon2, out x, out y);
		}

		public static void GeoPoints(double lat, double lon, double dist, out double xlatN, out double xlonE, out double xlatS, out double xlonW)
		{
			double num = dist * 1000.0;
			double num2 = default(double);
			double num3 = default(double);
			GeoMath.DegLatDistance(lat, lon, out num2, out num3);
			double num4 = num2 / 3600.0;
			double num5 = num3 / 3600.0;
			double num6 = Math.Abs(num / num4 / 3600.0);
			double num7 = Math.Abs(num / num5 / 3600.0);
			xlatN = lat + num6;
			xlatS = lat - num6;
			xlonE = lon + num7;
			xlonW = lon - num7;
		}

		public static void GeoPoints2(double lat, double lon, double dist, out double xlatN, out double xlonE, out double xlatS, out double xlonW)
		{
			double num = default(double);
			GeoMath.GCPoint(lat, lon, 360.0, dist, out xlatN, out num);
			GeoMath.GCPoint(lat, lon, 90.0, dist, out num, out xlonE);
			GeoMath.GCPoint(lat, lon, 180.0, dist, out xlatS, out num);
			GeoMath.GCPoint(lat, lon, 270.0, dist, out num, out xlonW);
		}

		public static void DegLatDistance(double lat, double lon, out double nmlat, out double nmlon)
		{
			lat = GeoMath.deg2rad(lat);
			lon = GeoMath.deg2rad(lon);
			double num = -559.82;
			double num2 = 1.175;
			double num3 = -0.0023;
			double num4 = 111412.84;
			double num5 = -93.5;
			double num6 = 0.118;
			double value = 111132.92 + num * Math.Cos(2.0 * lat) + num2 * Math.Cos(4.0 * lat) + num3 * Math.Cos(6.0 * lat);
			double value2 = num4 * Math.Cos(lat) + num5 * Math.Cos(3.0 * lat) + num6 * Math.Cos(5.0 * lat);
			nmlat = Math.Abs(value);
			nmlon = Math.Abs(value2);
		}

		public static double deg2rad(double deg)
		{
			return deg * 3.1415926535897931 / 180.0;
		}

		public static double rad2deg(double rad)
		{
			return rad / 3.1415926535897931 * 180.0;
		}

		public static int RoundInt(double v)
		{
			return GeoMath.RoundInt(Convert.ToSingle(v));
		}

		public static int RoundInt(float v)
		{
			int num = Convert.ToInt32(Math.Round((double)v));
			if (v > 0.75f && num % 2 != 0)
			{
				num = Convert.ToInt32(Math.Round((double)(v + 1f)));
			}
			else if (v <= 0.75f)
			{
				num = 0;
			}
			return num;
		}

		public static double NormalizeLongitude(double X)
		{
			if (X >= 180.0)
			{
				return X - 360.0;
			}
			if (X < -180.0)
			{
				return X + 360.0;
			}
			return X;
		}

		public static double PrecisionDistance(double lat1, double lon1, double lat2, double lon2)
		{
			double num = 6378137.0;
			double num2 = 6356752.3142;
			double num3 = 0.0033528106647474805;
			double num4 = GeoMath.deg2rad(lon2 - lon1);
			double num5 = Math.Atan((1.0 - num3) * Math.Tan(GeoMath.deg2rad(lat1)));
			double num6 = Math.Atan((1.0 - num3) * Math.Tan(GeoMath.deg2rad(lat2)));
			double num7 = Math.Sin(num5);
			double num8 = Math.Cos(num5);
			double num9 = Math.Sin(num6);
			double num10 = Math.Cos(num6);
			double num11 = num4;
			double num12 = 100.0;
			double num15;
			double num16;
			double num17;
			double num19;
			double num20;
			double num22;
			do
			{
				double num13 = Math.Sin(num11);
				double num14 = Math.Cos(num11);
				num15 = Math.Sqrt(num10 * num13 * (num10 * num13) + (num8 * num9 - num7 * num10 * num14) * (num8 * num9 - num7 * num10 * num14));
				if (num15 == 0.0)
				{
					return 0.0;
				}
				num16 = num7 * num9 + num8 * num10 * num14;
				num17 = Math.Atan2(num15, num16);
				double num18 = num8 * num10 * num13 / num15;
				num19 = 1.0 - num18 * num18;
				num20 = num16 - 2.0 * num7 * num9 / num19;
				if (double.IsNaN(num20))
				{
					num20 = 0.0;
				}
				double num21 = num3 / 16.0 * num19 * (4.0 + num3 * (4.0 - 3.0 * num19));
				num22 = num11;
				num11 = num4 + (1.0 - num21) * num3 * num18 * (num17 + num21 * num15 * (num20 + num21 * num16 * (-1.0 + 2.0 * num20 * num20)));
			}
			while (Math.Abs(num11 - num22) > 1E-12 && (num12 -= 1.0) > 0.0);
			if (num12 == 0.0)
			{
				return double.NaN;
			}
			double num23 = num19 * (num * num - num2 * num2) / (num2 * num2);
			double num24 = 1.0 + num23 / 16384.0 * (4096.0 + num23 * (-768.0 + num23 * (320.0 - 175.0 * num23)));
			double num25 = num23 / 1024.0 * (256.0 + num23 * (-128.0 + num23 * (74.0 - 47.0 * num23)));
			double num26 = num25 * num15 * (num20 + num25 / 4.0 * (num16 * (-1.0 + 2.0 * num20 * num20) - num25 / 6.0 * num20 * (-3.0 + 4.0 * num15 * num15) * (-3.0 + 4.0 * num20 * num20)));
			return num2 * num24 * (num17 - num26);
		}

		public static void PrecisionCoordinate(double lat1, double lon1, double brng, double dist, out double nLat, out double nLon)
		{
			double num = 6378137.0;
			double num2 = 6356752.3142;
			double num3 = 0.0033528106647474805;
			double num4 = GeoMath.deg2rad(brng);
			double num5 = Math.Sin(num4);
			double num6 = Math.Cos(num4);
			double num7 = (1.0 - num3) * Math.Tan(GeoMath.deg2rad(lat1));
			double num8 = 1.0 / Math.Sqrt(1.0 + num7 * num7);
			double num9 = num7 * num8;
			double num10 = Math.Atan2(num7, num6);
			double num11 = num8 * num5;
			double num12 = 1.0 - num11 * num11;
			double num13 = num12 * (num * num - num2 * num2) / (num2 * num2);
			double num14 = 1.0 + num13 / 16384.0 * (4096.0 + num13 * (-768.0 + num13 * (320.0 - 175.0 * num13)));
			double num15 = num13 / 1024.0 * (256.0 + num13 * (-128.0 + num13 * (74.0 - 47.0 * num13)));
			double num16 = dist / (num2 * num14);
			double num17 = 6.2831853071795862;
			double num18 = 0.0;
			double num19 = 0.0;
			double num20 = 0.0;
			double num21 = 0.0;
			while (Math.Abs(num16 - num17) > 1E-12)
			{
				num18 = Math.Cos(2.0 * num10 + num16);
				num19 = Math.Sin(num16);
				num20 = Math.Cos(num16);
				num21 = num15 * num19 * (num18 + num15 / 4.0 * (num20 * (-1.0 + 2.0 * num18 * num18) - num15 / 6.0 * num18 * (-3.0 + 4.0 * num19 * num19) * (-3.0 + 4.0 * num18 * num18)));
				num17 = num16;
				num16 = dist / (num2 * num14) + num21;
			}
			double num22 = num9 * num19 - num8 * num20 * num6;
			double rad = Math.Atan2(num9 * num20 + num8 * num19 * num6, (1.0 - num3) * Math.Sqrt(num11 * num11 + num22 * num22));
			double num23 = Math.Atan2(num19 * num5, num8 * num20 - num9 * num19 * num6);
			double num24 = num3 / 16.0 * num12 * (4.0 + num3 * (4.0 - 3.0 * num12));
			double rad2 = num23 - (1.0 - num24) * num3 * num11 * (num16 + num24 * num19 * (num18 + num24 * num20 * (-1.0 + 2.0 * num18 * num18)));
			Math.Atan2(num11, 0.0 - num22);
			nLat = GeoMath.rad2deg(rad);
			nLon = lon1 + GeoMath.rad2deg(rad2);
		}

		public static double ConvertToBaseUnits(double val, int distUnit)
		{
			if (distUnit != 0)
			{
				return (double)GeoMath.kmToMilesRounded(val);
			}
			return val;
		}

		public static double kmToMiles(double km)
		{
			return km * 0.621371192;
		}

		public static int kmToMilesRounded(double km)
		{
			return Convert.ToInt32(Math.Round(GeoMath.kmToMiles(km)));
		}

		public static double miToKms(double mi)
		{
			return mi * 1.609344;
		}

		public static int miToKmsRounded(double mi)
		{
			return Convert.ToInt32(Math.Round(GeoMath.miToKms(mi)));
		}
	}
}
