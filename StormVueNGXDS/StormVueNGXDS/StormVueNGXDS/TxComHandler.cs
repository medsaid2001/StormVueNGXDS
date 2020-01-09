using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Windows.Forms;

namespace StormVue2RTCM
{
	internal class TxComHandler
	{
		public static SerialPort hwPort = new SerialPort();

		public static void InitCom()
		{
			TxComHandler.ResetCOM();
		}

		public static void ResetCOM()
		{
			TxComHandler.hwPort.PortName = "COM1";
			TxComHandler.hwPort.BaudRate = 9600;
			TxComHandler.hwPort.Parity = Parity.None;
			TxComHandler.hwPort.ReadTimeout = 1;
			TxComHandler.hwPort.WriteTimeout = 500;
			TxComHandler.hwPort.StopBits = StopBits.One;
		}

		public static string[] GetPortNames()
		{
			return SerialPort.GetPortNames();
		}

		public static string[] GetParities()
		{
			List<string> list = new List<string>();
			string[] names = Enum.GetNames(typeof(Parity));
			foreach (string item in names)
			{
				list.Add(item);
			}
			return list.ToArray();
		}

		public static string[] GetStopBits()
		{
			List<string> list = new List<string>();
			string[] names = Enum.GetNames(typeof(StopBits));
			foreach (string text in names)
			{
				if (!text.Equals("None"))
				{
					list.Add(text);
				}
			}
			return list.ToArray();
		}

		public static bool OpenCOM()
		{
			try
			{
				TxComHandler.hwPort.Open();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Unable to open COM port", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return false;
			}
			Console.WriteLine("Opened {0}", TxComHandler.hwPort.PortName);
			return true;
		}

		public static void CloseCOM()
		{
			try
			{
				TxComHandler.hwPort.DiscardInBuffer();
				TxComHandler.hwPort.Close();
			}
			catch (InvalidOperationException ex)
			{
				Console.WriteLine(ex.Message);
			}
			catch (Exception ex2)
			{
				Console.WriteLine(ex2.Message);
			}
			Console.WriteLine("Closed {0}", TxComHandler.hwPort.PortName);
		}
	}
}
