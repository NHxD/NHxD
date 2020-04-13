using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace Ash.System.ComponentModel
{
	public class BackgroundWorkerQueue<T> : IDisposable where T : IJob
	{
		private readonly Queue<T> jobs;
		private readonly BackgroundWorker backgroundWorker;

		public Queue<T> Jobs => jobs;

		public bool IsQueueEmpty => jobs.Count == 0;

		public int IdleWaitTime { get; set; } = 100;
		public int MaxConcurrentJobCount { get; set; } = 1;

		public BackgroundWorkerQueue()
		{
			jobs = new Queue<T>();
			backgroundWorker = new BackgroundWorker();

			backgroundWorker.WorkerReportsProgress = true;
			backgroundWorker.WorkerSupportsCancellation = true;

			backgroundWorker.DoWork += BackgroundWorker_DoWork;
			backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
			backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
		}

		public void Run()
		{
			BackgroundWorkerQueueRunArg<T> runArg = new BackgroundWorkerQueueRunArg<T>(jobs, IdleWaitTime, MaxConcurrentJobCount);

			backgroundWorker.RunWorkerAsync(runArg);
		}

		// HACK: not supposed to traverse queues.
		public IEnumerable<T> SelectJobs(Func<T, bool> selector)
		{
			List<T> subJobs = new List<T>();
			Func<T, bool> selectorHandler = selector;

			if (selectorHandler != null)
			{
				foreach (T job in jobs)
				{
					if (selectorHandler.Invoke(job))
					{
						subJobs.Add(job);
					}
				}
			}

			return subJobs;
		}

		// HACK: not supposed to traverse queues.
		public bool TryFindJob(Func<T, bool> selector, out T result)
		{
			Func<T, bool> selectorHandler = selector;

			if (selectorHandler != null)
			{
				foreach (T job in jobs)
				{
					if (selectorHandler.Invoke(job))
					{
						result = job;

						return true;
					}
				}
			}

			result = default(T);

			return false;
		}

		private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
		}

		private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			T task = (T)e.UserState;

			task.RunAsync();
		}

		private static void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			BackgroundWorker backgroundWorker = sender as BackgroundWorker;
			BackgroundWorkerQueueRunArg<T> runArg = e.Argument as BackgroundWorkerQueueRunArg<T>;
			T task = default(T);
			int concurrentJobCount = 0;

			while (!backgroundWorker.CancellationPending)
			{
				if (runArg.Jobs.Count == 0
					// TODO
					//	|| concurrentJobCount == runArg.MaxConcurrentJobCount
					|| runArg.MaxConcurrentJobCount > 1 // HACK
					|| (task != null && task.IsBusy())) // force sequential processing.
				{
					Thread.Sleep(runArg.IdleWaitTime);
					continue;
				}

				if (task != null)
				{
					--concurrentJobCount;
				}

				task = runArg.Jobs.Dequeue();

				++concurrentJobCount;

				backgroundWorker.ReportProgress(0, task);
			}
		}

		public void AddJob(T job)
		{
			jobs.Enqueue(job);
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
				}

				disposedValue = true;
			}
		}

		~BackgroundWorkerQueue()
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

	public class BackgroundWorkerQueueRunArg<T> where T : IJob
	{
		public Queue<T> Jobs { get; }
		public int IdleWaitTime { get; }
		public int MaxConcurrentJobCount { get; }

		public BackgroundWorkerQueueRunArg(Queue<T> jobs, int idleWaitTime, int maxConcurrentJobCount)
		{
			Jobs = jobs;
			IdleWaitTime = idleWaitTime;
			MaxConcurrentJobCount = maxConcurrentJobCount;
		}
	}
}
