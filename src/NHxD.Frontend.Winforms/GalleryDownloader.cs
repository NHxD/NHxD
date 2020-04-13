using Ash.System.ComponentModel;
using System.Net.Http;

namespace NHxD.Frontend.Winforms
{
	public class GalleryDownloader : BackgroundWorkerQueue<GalleryDownloaderJob>
	{
		public event GalleryDownloadReportProgressEventHandler GalleryDownloadReportProgress = delegate { };
		public event GalleryDownloadStartedEventHandler GalleryDownloadStarted = delegate { };
		public event GalleryDownloadCancelledEventHandler GalleryDownloadCancelled = delegate { };
		public event GalleryDownloadCompletedEventHandler GalleryDownloadCompleted = delegate { };

		public HttpClient HttpClient { get; }
		public IPathFormatter PathFormatter { get; }
		public ISearchResultCache SearchResultCache { get; }

		public GalleryDownloader(HttpClient httpClient, IPathFormatter pathFormatter, ISearchResultCache searchResultCache)
		{
			HttpClient = httpClient;
			PathFormatter = pathFormatter;
			SearchResultCache = searchResultCache;
		}

		public void Download(int galleryId)
		{
			GalleryDownloaderJob job;

			if (MaxConcurrentJobCount > 1
				&& TryFindJob(galleryId, out job))
			{
				return;
			}

			job = new GalleryDownloaderJob(HttpClient, PathFormatter, SearchResultCache, galleryId);

			job.GalleryDownloadReportProgress += Job_GalleryDownloadReportProgress;
			job.GalleryDownloadStarted += Job_GalleryDownloadStarted;
			job.GalleryDownloadCancelled += Job_GalleryDownloadCancelled;
			job.GalleryDownloadCompleted += Job_GalleryDownloadCompleted;

			AddJob(job);
		}

		public void CancelAll()
		{
			// HACK: not supposed to traverse queues...
			foreach (GalleryDownloaderJob job in Jobs)
			{
				if (job != null)
				{
					job.CancelAsync();
				}
			}
		}

		public void Cancel(int galleryId)
		{
			GalleryDownloaderJob job;

			if (TryFindJob(galleryId, out job))
			{
				job.CancelAsync();
			}
		}

		public bool TryFindJob(int galleryId, out GalleryDownloaderJob result)
		{
			return TryFindJob(job => (job.GalleryId == galleryId), out result);
		}

		private void Job_GalleryDownloadCompleted(object sender, GalleryDownloadCompletedEventArgs e)
		{
			GalleryDownloadCompleted.Invoke(sender, e);
		}

		private void Job_GalleryDownloadCancelled(object sender, GalleryDownloadCancelledEventArgs e)
		{
			GalleryDownloadCancelled.Invoke(sender, e);
		}

		private void Job_GalleryDownloadStarted(object sender, GalleryDownloadStartedEventArgs e)
		{
			GalleryDownloadStarted.Invoke(sender, e);
		}

		private void Job_GalleryDownloadReportProgress(object sender, GalleryDownloadReportProgressEventArgs e)
		{
			GalleryDownloadReportProgress.Invoke(sender, e);
		}
	}
}
