using Ash.System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace NHxD.Frontend.Winforms
{
	public class PageDownloader : BackgroundWorkerQueue<PageDownloaderJob>
	{
		public event PageDownloadReportProgressEventHandler PageDownloadReportProgress = delegate { };
		public event PageDownloadStartedEventHandler PagesDownloadStarted = delegate { };
		public event PageDownloadCompletedEventHandler PagesDownloadCancelled = delegate { };
		public event PageDownloadCompletedEventHandler PagesDownloadCompleted = delegate { };

		public HttpClient HttpClient { get; }
		public IPathFormatter PathFormatter { get; }
		public ISearchResultCache SearchResultCache { get; }
		public ICacheFileSystem CacheFileSystem { get; }

		public PageDownloader(HttpClient httpClient, IPathFormatter pathFormatter, ISearchResultCache searchResultCache, ICacheFileSystem cacheFileSystem)
		{
			HttpClient = httpClient;
			SearchResultCache = searchResultCache;
			PathFormatter = pathFormatter;
			CacheFileSystem = cacheFileSystem;
		}

		public void Download(int galleryId)
		{
			Download(galleryId, null);
		}

		public void Download(int galleryId, int[] pageIndices)
		{
			PageDownloaderJob job;

			if (MaxConcurrentJobCount > 1
				&& TryFindJob(galleryId, pageIndices, out job))
			{
				return;
			}

			job = new PageDownloaderJob(HttpClient, PathFormatter, SearchResultCache, CacheFileSystem, galleryId, pageIndices);

			job.PageDownloadReportProgress += Job_PageDownloadReportProgress;
			job.PagesDownloadStarted += Job_PagesDownloadStarted;
			job.PagesDownloadCancelled += Job_PagesDownloadCancelled;
			job.PagesDownloadCompleted += Job_PagesDownloadCompleted;

			AddJob(job);
		}

		public void CancelAll()
		{
			// HACK: not supposed to traverse queues...
			foreach (PageDownloaderJob job in Jobs)
			{
				if (job != null)
				{
					job.CancelAsync();
				}
			}
		}

		public void Cancel(int galleryId)
		{
			foreach (PageDownloaderJob job in GetJobs(galleryId))
			{
				job.CancelAsync();
			}
		}

		public void Cancel(int galleryId, int[] pageIndices)
		{
			PageDownloaderJob job;

			if (TryFindJob(galleryId, pageIndices, out job))
			{
				job.CancelAsync();
			}
		}

		public bool HasAnyJob(int galleryId)
		{
			PageDownloaderJob result;

			return TryFindJob(job => (job.GalleryId == galleryId), out result);
		}

		public IEnumerable<PageDownloaderJob> GetJobs(int galleryId)
		{
			return SelectJobs(job => job.GalleryId == galleryId);
		}

		public bool TryFindJob(int galleryId, int[] pageIndices, out PageDownloaderJob result)
		{
			return TryFindJob(job => (job.GalleryId == galleryId
					&& job.PageIndices != null
					&& pageIndices != null
					&& job.PageIndices.OrderBy(x => x).SequenceEqual(pageIndices.OrderBy(x => x))), out result);
		}

		private void Job_PagesDownloadCompleted(object sender, PageDownloadCompletedEventArgs e)
		{
			PagesDownloadCompleted.Invoke(sender, e);
		}

		private void Job_PagesDownloadCancelled(object sender, PageDownloadCompletedEventArgs e)
		{
			PagesDownloadCancelled.Invoke(sender, e);
		}

		private void Job_PagesDownloadStarted(object sender, PagesDownloadStartedEventArgs e)
		{
			PagesDownloadStarted.Invoke(sender, e);
		}

		private void Job_PageDownloadReportProgress(object sender, PageDownloadReportProgressEventArgs e)
		{
			PageDownloadReportProgress.Invoke(sender, e);
		}
	}
}
