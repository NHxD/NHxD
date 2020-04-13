using Ash.System.ComponentModel;
using System.ComponentModel;

namespace NHxD.Frontend.Winforms
{
	public class BackgroundTaskWorker : BackgroundWorkerQueue<BackgroundTaskJob>
	{
		public event ProgressChangedEventHandler ProgressChanged = delegate { };

		public void OnProgressChanged(ProgressChangedEventArgs e)
		{
			ProgressChanged.Invoke(this, e);
		}

		public void AddBookmark(BookmarkItemChangeEventArgs e)
		{
			BackgroundTaskJob job = new BackgroundTaskJob(new AddBookmarkItemTask(e.Path, e.Value));

			job.ProgressChanged += Job_ProgressChanged;

			AddJob(job);
		}

		public void AddBrowsing(BrowsingItemChangeEventArgs e)
		{
			BackgroundTaskJob job = new BackgroundTaskJob(new AddBrowsingItemTask(e.Key, e.Value));

			job.ProgressChanged += Job_ProgressChanged;

			AddJob(job);
		}

		private void Job_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			OnProgressChanged(e);
		}
	}
}
