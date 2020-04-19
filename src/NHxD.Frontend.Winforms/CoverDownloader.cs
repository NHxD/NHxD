using Ash.System.ComponentModel;
using Nhentai;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace NHxD.Frontend.Winforms
{
	public class CoverDownloader : BackgroundWorkerQueue<CoverDownloaderJob>
	{
		public event CoverDownloadReportProgressEventHandler CoverDownloadReportProgress = delegate { };
		public event CoverDownloadStartedEventHandler CoversDownloadStarted = delegate { };
		public event CoverDownloadCompletedEventHandler CoversDownloadCancelled = delegate { };
		public event CoverDownloadCompletedEventHandler CoversDownloadCompleted = delegate { };

		public HttpClient HttpClient { get; }
		public IPathFormatter PathFormatter { get; }
		public MetadataKeywordLists MetadataKeywordLists { get; }

		public CoverDownloader(HttpClient httpClient, IPathFormatter pathFormatter, MetadataKeywordLists metadataKeywordLists)
		{
			HttpClient = httpClient;
			PathFormatter = pathFormatter;
			MetadataKeywordLists = metadataKeywordLists;
		}

		public bool TryFindJob(int galleryId, out CoverDownloaderJob result)
		{
			return TryFindJob(job => (job.SearchResult != null
					&& job.SearchResult.Result != null
					&& job.SearchResult.Result.Any(x => x.Id == galleryId)), out result);
		}

		public bool TryFindJob(SearchResult searchResult, out CoverDownloaderJob result)
		{
			return TryFindJob(job => (job.SearchResult != null
					&& job.SearchResult.Result != null
					&& searchResult != null
					&& searchResult.Result != null
					&& job.SearchResult.Result.OrderBy(x => x.Id).SequenceEqual(searchResult.Result.OrderBy(x => x.Id))), out result);
		}

		public void Download(Metadata metadata, CoverDownloaderFilters filters)
		{
			if (metadata == null)
			{
				return;
			}

			CoverDownloaderJob job;

			if (TryFindJob(metadata.Id, out job))
			{
				return;
			}

			SearchResult searchResult = new SearchResult();

			searchResult.Result = new List<Metadata>();
			searchResult.Result.Add(metadata);
			searchResult.PerPage = 1;
			searchResult.NumPages = 1;

			job = new CoverDownloaderJob(HttpClient, PathFormatter, MetadataKeywordLists, searchResult, filters);

			job.CoverDownloadReportProgress += Job_CoverDownloadReportProgress;
			job.CoversDownloadStarted += Job_CoversDownloadStarted;
			job.CoversDownloadCancelled += Job_CoversDownloadCancelled;
			job.CoversDownloadCompleted += Job_CoversDownloadCompleted;

			AddJob(job);
		}

		public void Download(SearchResult searchResult, CoverDownloaderFilters filters)
		{
			CoverDownloaderJob job;

			if (MaxConcurrentJobCount > 1
				&& TryFindJob(searchResult, out job))
			{
				return;
			}

			job = new CoverDownloaderJob(HttpClient, PathFormatter, MetadataKeywordLists, searchResult, filters);

			job.CoverDownloadReportProgress += Job_CoverDownloadReportProgress;
			job.CoversDownloadStarted += Job_CoversDownloadStarted;
			job.CoversDownloadCancelled += Job_CoversDownloadCancelled;
			job.CoversDownloadCompleted += Job_CoversDownloadCompleted;

			AddJob(job);
		}

		public void CancelAll()
		{
			foreach (CoverDownloaderJob job in Jobs)
			{
				if (job != null)
				{
					job.CancelAsync();
				}
			}
		}

		public void Cancel(SearchResult searchResult)
		{
			CoverDownloaderJob job;

			if (TryFindJob(searchResult, out job))
			{
				job.CancelAsync();
			}
		}

		private void Job_CoversDownloadCompleted(object sender, CoverDownloadCompletedEventArgs e)
		{
			CoversDownloadCompleted.Invoke(sender, e);
		}

		private void Job_CoversDownloadCancelled(object sender, CoverDownloadCompletedEventArgs e)
		{
			CoversDownloadCancelled.Invoke(sender, e);
		}

		private void Job_CoversDownloadStarted(object sender, CoversDownloadStartedEventArgs e)
		{
			CoversDownloadStarted.Invoke(sender, e);
		}

		private void Job_CoverDownloadReportProgress(object sender, CoverDownloadReportProgressEventArgs e)
		{
			CoverDownloadReportProgress.Invoke(sender, e);
		}
	}
}
