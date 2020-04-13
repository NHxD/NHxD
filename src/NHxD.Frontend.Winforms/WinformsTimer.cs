using System;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	public class WinformsTimer : ILibraryPollingTimer, IDisposable
	{
		private readonly Timer timer;
		private readonly int interval;
		private readonly object userData;

		public int Interval => interval;
		public object UserData => userData;

		public event TickEventHandler Tick = delegate { };

		public WinformsTimer(int timeout, int interval)
			: this(timeout, interval, null)
		{

		}

		public WinformsTimer(int timeout, int interval, object userData)
		{
			this.interval = interval;
			this.userData = userData;

			timer = new Timer();

			timer.Interval = interval;
			timer.Tag = userData;
			timer.Tick += Timer_Tick;

			if (timeout == 0)
			{
				Timer_Tick(timer, EventArgs.Empty);
			}
			else if (timeout > 1)
			{
				// not supported. (would have to create a second temporary timer.)
			}
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			Tick.Invoke(timer, new TickEventArgs(userData));
		}

		public void Start()
		{
			timer.Start();
		}

		public void Stop()
		{
			timer.Stop();
		}

		#region IDisposable Support
		private bool disposedValue;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					if (timer != null)
					{
						timer.Dispose();
					}
				}

				disposedValue = true;
			}
		}

		~WinformsTimer()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
