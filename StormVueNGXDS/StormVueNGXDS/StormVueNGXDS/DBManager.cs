using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace StormVue2RTCM
{
	internal class DBManager
	{
		public static void CreateDBFile()
		{
			string path = Path.Combine(Settings.dbPath, DBManager.getDBFileName());
			if (!File.Exists(path))
			{
				File.Create(path);
			}
		}

		public static bool LightningToDB(List<StrikeData> sdl)
		{
			try
			{
				string text = Path.Combine(Settings.dbPath, DBManager.getDBFileName());
				if (!File.Exists(text))
				{
					new FileStream(text, FileMode.Create, FileAccess.Write, FileShare.ReadWrite).Close();
				}
				string commandText = "insert into NGXLIGHTNING (id, epoch_ms, datetime_utc, latitude, longitude, type) values (NULL, @P0,@P1,@P2,@P3,@P4);";
				using (SQLiteConnection sQLiteConnection = new SQLiteConnection("Data Source=" + text + ";Version=3; PRAGMA synchronous=OFF;"))
				{
					sQLiteConnection.Open();
					using (SQLiteCommand sQLiteCommand = new SQLiteCommand("create table if not exists NGXLIGHTNING (id INTEGER PRIMARY KEY, epoch_ms INT(8), datetime_utc TEXT, latitude REAL, longitude REAL, type INT);", sQLiteConnection))
					{
						sQLiteCommand.ExecuteNonQuery();
					}
					SQLiteTransaction sQLiteTransaction = sQLiteConnection.BeginTransaction();
					using (SQLiteCommand sQLiteCommand2 = new SQLiteCommand(sQLiteConnection))
					{
						sQLiteCommand2.Transaction = sQLiteTransaction;
						sQLiteCommand2.CommandText = commandText;
						foreach (StrikeData item in sdl)
						{
							sQLiteCommand2.Parameters.AddWithValue("@P0", item.millis);
							sQLiteCommand2.Parameters.AddWithValue("@P1", TimeEx.millisToDateTime(item.millis).ToString("yyyy-MM-dd HH:mm:ss.fff"));
							sQLiteCommand2.Parameters.AddWithValue("@P2", item.lat.ToString(Util.ci));
							sQLiteCommand2.Parameters.AddWithValue("@P3", item.lon.ToString(Util.ci));
							sQLiteCommand2.Parameters.AddWithValue("@P4", item.compoundType);
							sQLiteCommand2.ExecuteNonQuery();
						}
						sQLiteTransaction.Commit();
					}
				}
			}
			catch (Exception ex)
			{
				Syslogger.AddMsg("LightningToDB", "Exception: " + ex.Message);
				return false;
			}
			return true;
		}

		private static string getDBFileName()
		{
			return DateTime.Now.ToString("NGXDS_yyyyMMdd") + ".db3";
		}
	}
}
