using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace StormVue2RTCM
{
	internal class Counter
	{
		private int peak;

		private TimeSpan timeSpan;

		private System.Windows.Forms.Timer cntTimer;

		private List<long> counterItem;

		public Counter(int minutes)
		{
			this.peak = 0;
			this.timeSpan = new TimeSpan(0, minutes, 0);
			this.counterItem = new List<long>();
			this.cntTimer = new System.Windows.Forms.Timer();
			this.cntTimer.Tick += this.cntTimer_Tick;
			this.cntTimer.Interval = 250;
			this.cntTimer.Start();
		}

		private void cntTimer_Tick(object myObject, EventArgs myEventArgs)
		{
			this.DecCounter();
		}

		public void IncCounter()
		{
			Monitor.Enter(this.counterItem);
			this.counterItem.Add(DateTime.UtcNow.Ticks);
			Monitor.Exit(this.counterItem);
		}

		private void DecCounter()
		{
			if (this.counterItem.Count > 0)
			{
				int num = 0;
				long num2 = DateTime.UtcNow.Ticks - this.timeSpan.Ticks;
				Monitor.Enter(this.counterItem);
				foreach (long item in this.counterItem)
				{
					if (item >= num2)
					{
						break;
					}
					num++;
				}
				if (num > 0)
				{
					this.counterItem.RemoveRange(0, num);
				}
				Monitor.Exit(this.counterItem);
			}
		}

		public int CurrentCount()
		{
			int count = this.counterItem.Count;
			if (this.peak < count)
			{
				this.peak = count;
			}
			return count;
		}

		public int Peak()
		{
			return this.peak;
		}
	}
}
