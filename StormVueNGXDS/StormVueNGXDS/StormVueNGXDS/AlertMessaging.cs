using System;
using System.ComponentModel;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace StormVue2RTCM
{
	internal class AlertMessaging
	{
		private static bool mailSent = true;

		public bool telegramSent = true;

		private Form1 mainFrm;

		public bool IsTestMessage;

		public bool IsTestTelegram;

		public AlertMessaging(Form callingForm)
		{
			this.mainFrm = (callingForm as Form1);
			this.IsTestMessage = false;
		}

		public async Task TelegramAlerts(int type, bool isTest = false)
		{
			if (this.telegramSent)
			{
				this.telegramSent = false;
				this.IsTestTelegram = isTest;
				try
				{
					string text = "";
					switch (type)
					{
					case 0:
						text = Settings.mAlarmBody;
						break;
					case 1:
						text = Settings.mClearBody;
						break;
					}
					int num = (Settings.DistanceUnits == 0) ? ((int)Settings.CloseStrikeRangeKM) : GeoMath.kmToMilesRounded(Settings.CloseStrikeRangeKM);
					string text2 = text;
					DateTime now = DateTime.Now;
					text = text2.Replace("<date>", now.ToString());
					string text3 = text;
					now = DateTime.Now;
					text = text3.Replace("<time>", now.ToShortTimeString());
					text = text.Replace("<distance>", num.ToString());
					text = text.Replace("<distunits>", (Settings.DistanceUnits == 0) ? "km" : "mi");
					text = text.Replace("<watchtime>", Settings.AllClearPeriod.ToString());
					text = ((type == 0) ? "<b>ALERT</b>: " : "<b>ALL CLEAR</b>: ") + text;
					if (this.IsTestTelegram)
					{
						text = "[TEST] " + text;
					}
					await new Api(Settings.tBotToken).SendTextMessage(Settings.tDestId, text, false, false, 0, null, ParseMode.Html);
				}
				catch (Exception ex)
				{
					if (this.IsTestTelegram)
					{
						MessageBox.Show("Exception: " + ex.Message, "A Telegram send error occured", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
					}
					this.IsTestTelegram = false;
					this.telegramSent = true;
					return;
				}
				this.telegramSent = true;
			}
		}

		public void EmailAlerts(int type, bool isTest = false)
		{
			if (AlertMessaging.mailSent)
			{
				AlertMessaging.mailSent = false;
				this.IsTestMessage = isTest;
				try
				{
					SmtpClient smtpClient = new SmtpClient(Settings.mServer, Settings.mPort);
					if (Settings.mAuthIndex > 0)
					{
						smtpClient.Credentials = new NetworkCredential(Settings.mUsername, Settings.mPassword);
					}
					else
					{
						smtpClient.UseDefaultCredentials = false;
					}
					MailMessage mailMessage = new MailMessage();
					try
					{
						mailMessage.From = new MailAddress(Settings.mFrom);
					}
					catch (FormatException ex)
					{
						throw new Exception("Format error (From)", ex.InnerException);
					}
					foreach (string mTo in Settings.mToList)
					{
						try
						{
							mailMessage.To.Add(mTo);
						}
						catch (FormatException ex2)
						{
							throw new Exception("Format error (To):" + mTo, ex2.InnerException);
						}
					}
					mailMessage.BodyEncoding = Encoding.UTF8;
					mailMessage.SubjectEncoding = Encoding.UTF8;
					string text = "";
					string text2 = "";
					switch (type)
					{
					case 0:
						text = Settings.mAlarmSubj;
						text2 = Settings.mAlarmBody;
						break;
					case 1:
						text = Settings.mClearSubj;
						text2 = Settings.mClearBody;
						break;
					}
					int num = (Settings.DistanceUnits == 0) ? ((int)Settings.CloseStrikeRangeKM) : GeoMath.kmToMilesRounded(Settings.CloseStrikeRangeKM);
					string text3 = text;
					DateTime now = DateTime.Now;
					text = text3.Replace("<date>", now.ToString());
					string text4 = text;
					now = DateTime.Now;
					text = text4.Replace("<time>", now.ToShortTimeString());
					text = text.Replace("<distance>", num.ToString());
					text = text.Replace("<distunits>", (Settings.DistanceUnits == 0) ? "km" : "mi");
					text = text.Replace("<watchtime>", Settings.AllClearPeriod.ToString());
					if (this.IsTestMessage)
					{
						text = "[TEST] " + text;
					}
					mailMessage.Subject = text;
					string text5 = text2;
					now = DateTime.Now;
					text2 = text5.Replace("<date>", now.ToString());
					string text6 = text2;
					now = DateTime.Now;
					text2 = text6.Replace("<time>", now.ToShortTimeString());
					text2 = text2.Replace("<distance>", num.ToString());
					text2 = text2.Replace("<distunits>", (Settings.DistanceUnits == 0) ? "km" : "mi");
					text2 = (mailMessage.Body = text2.Replace("<watchtime>", Settings.AllClearPeriod.ToString()));
					smtpClient.SendCompleted += this.SendCompletedCallback;
					smtpClient.EnableSsl = (Settings.mSecIndex > 0);
					string userToken = (type == 0) ? "Alert message" : "All Clear message";
					smtpClient.SendAsync(mailMessage, userToken);
				}
				catch (Exception ex3)
				{
					this.mainFrm.SendResult(ex3.Message);
					this.IsTestMessage = false;
					AlertMessaging.mailSent = true;
				}
			}
		}

		private void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
		{
			string arg = (string)e.UserState;
			if (e.Error != null)
			{
				Console.WriteLine("[{0}] {1}", arg, e.Error.ToString());
				this.mainFrm.SendResult(e.Error.GetBaseException().ToString());
			}
			else
			{
				this.mainFrm.SendResult(null);
				Console.WriteLine("Message sent.");
			}
			AlertMessaging.mailSent = true;
			this.IsTestMessage = false;
		}
	}
}
