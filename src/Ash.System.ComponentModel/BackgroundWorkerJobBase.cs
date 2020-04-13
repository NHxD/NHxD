using System;
using System.ComponentModel;
using Timer = System.Windows.Forms.Timer;

namespace Ash.System.ComponentModel
{
	public abstract class BackgroundWorkerJobBase : IJob, IDisposable
	{
		private readonly BackgroundWorker backgroundWorker;
		private readonly Timer cancelTimer;

		protected BackgroundWorker BackgroundWorker => backgroundWorker;

		protected bool IsDone { get; set; }

		protected BackgroundWorkerJobBase()
		{
			backgroundWorker = new BackgroundWorker();
			cancelTimer = new Timer();

			cancelTimer.Interval = 100;
			cancelTimer.Tick += CancelTimer_Tick;
		}

		public abstract void RunAsync();

		public bool IsBusy()
		{
			return !IsDone;
		}

		private void CancelTimer_Tick(object sender, EventArgs e)
		{
			Timer timer = sender as Timer;

			if (!backgroundWorker.IsBusy
				|| !backgroundWorker.WorkerSupportsCancellation
				|| backgroundWorker.CancellationPending)
			{
				return;
			}

			timer.Stop();
			backgroundWorker.CancelAsync();
		}

		public void CancelAsync()
		{
			if (!backgroundWorker.IsBusy
				|| !backgroundWorker.WorkerSupportsCancellation)
			{
				if (backgroundWorker.CancellationPending)
				{
					return;
				}
				else if (!cancelTimer.Enabled)
				{
					cancelTimer.Start();

					return;
				}
			}

			backgroundWorker.CancelAsync();
		}

		#region IDisposable Support
		private bool disposedValue;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					if (backgroundWorker != null)
					{
						backgroundWorker.Dispose();
					}

					if (cancelTimer != null)
					{
						cancelTimer.Dispose();
					}
				}

				disposedValue = true;
			}
		}

		~BackgroundWorkerJobBase()
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
