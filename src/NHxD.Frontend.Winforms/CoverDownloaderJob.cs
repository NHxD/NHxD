using Ash.System.ComponentModel;
using Nhentai;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	public class CoverDownloaderJob : BackgroundWorkerJobBase
	{
		public event CoverDownloadReportProgressEventHandler CoverDownloadReportProgress = delegate { };
		public event CoverDownloadStartedEventHandler CoversDownloadStarted = delegate { };
		public event CoverDownloadCompletedEventHandler CoversDownloadCancelled = delegate { };
		public event CoverDownloadCompletedEventHandler CoversDownloadCompleted = delegate { };

		public HttpClient HttpClient { get; }
		public IPathFormatter PathFormatter { get; }
		public SearchResult SearchResult { get; }
		public MetadataKeywordLists MetadataKeywordLists { get; }
		public CoverDownloaderFilters Filters { get; }

		public CoverDownloaderJob(HttpClient httpClient, IPathFormatter pathFormatter, MetadataKeywordLists metadataKeywordLists, SearchResult searchResult, CoverDownloaderFilters filters)
		{
			HttpClient = httpClient;
			PathFormatter = pathFormatter;
			SearchResult = searchResult;
			MetadataKeywordLists = metadataKeywordLists;
			Filters = filters;

			BackgroundWorker.WorkerReportsProgress = true;
			BackgroundWorker.WorkerSupportsCancellation = true;

			BackgroundWorker.DoWork += DownloadCoverBackgroundWorker_DoWork;
			BackgroundWorker.ProgressChanged += DownloadCoverBackgroundWorker_ProgressChanged;
			BackgroundWorker.RunWorkerCompleted += DownloadCoverBackgroundWorker_RunWorkerCompleted;
		}

		protected virtual void OnCoversDownloadStarted(CoversDownloadStartedEventArgs e)
		{
			CoversDownloadStarted.Invoke(this, e);
		}

		protected virtual void OnCoversDownloadCancelled(CoverDownloadCompletedEventArgs e)
		{
			CoversDownloadCancelled.Invoke(this, e);
		}

		protected virtual void OnCoversDownloadCompleted(CoverDownloadCompletedEventArgs e)
		{
			CoversDownloadCompleted.Invoke(this, e);
		}

		protected virtual void OnCoverDownloadReportProgress(CoverDownloadReportProgressEventArgs e)
		{
			CoverDownloadReportProgress.Invoke(this, e);
		}

		public override void RunAsync()
		{
			if (BackgroundWorker.IsBusy)
			{
				MessageBox.Show("Program is busy", "Download in progress", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			OnCoversDownloadStarted(new CoversDownloadStartedEventArgs(SearchResult/*pageIndices, galleryId*/));

			DownloadCoversRunArg runArg = new DownloadCoversRunArg(HttpClient, PathFormatter, SearchResult, MetadataKeywordLists, Filters/*galleryId, pageIndices, SearchResultCache, GetCachedPageIndices*/);

			BackgroundWorker.RunWorkerAsync(runArg);
		}

		private static void DownloadCoverBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			BackgroundWorker backgroundWorker = sender as BackgroundWorker;
			DownloadCoversRunArg runArg = e.Argument as DownloadCoversRunArg;
			SearchResult searchResult = runArg.SearchResult;
			int maxCount = searchResult.Result.Count;
			int loadCount = 0;

			for (int i = 0; i < maxCount; ++i)
			{
				if (backgroundWorker.CancellationPending)
				{
					CoverDownloadCompletedArg cancelledArg = new CoverDownloadCompletedArg(/*loadCount, maxCount, cacheCount, metadata*/searchResult);

					e.Result = cancelledArg;
					//e.Cancel = true;
					return;
				}

				Metadata metadata = searchResult.Result[i];
				string error = "";
				string coverCachedFilePath;
				if (runArg.PathFormatter != null && runArg.PathFormatter.IsEnabled)
				{
					coverCachedFilePath = runArg.PathFormatter.GetCover(metadata);
				}
				else
				{
					coverCachedFilePath = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}",
						runArg.PathFormatter.GetCacheDirectory(), metadata.Id, metadata.Images.Cover.GetFileExtension());
				}

				bool shouldFilter = ShouldFilter(metadata, runArg.MetadataKeywordLists, runArg.Filters);

				if (shouldFilter)
				{
					error = "SKIP";
				}
				else
				{
					++loadCount;

					if (!File.Exists(coverCachedFilePath))
					{
						try
						{
							string uri = string.Format(CultureInfo.InvariantCulture, "https://t.nhentai.net/galleries/{0}/{1}{2}", metadata.MediaId, "cover", metadata.Images.Cover.GetFileExtension());

							using (HttpResponseMessage response = Task.Run(() => runArg.HttpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead)).GetAwaiter().GetResult())
							{
								if (!response.IsSuccessStatusCode)
								{
									coverCachedFilePath = "";
									response.EnsureSuccessStatusCode();
									//error = string.Format(CultureInfo.InvariantCulture, "{0} ({1})", response.ReasonPhrase, response.StatusCode);
									//continue;
								}
								else
								{
									try
									{
										byte[] imageData = Task.Run(() => response.Content.ReadAsByteArrayAsync()).GetAwaiter().GetResult();

										Directory.CreateDirectory(Path.GetDirectoryName(coverCachedFilePath));
										File.WriteAllBytes(coverCachedFilePath, imageData);
									}
									catch (Exception ex)
									{
										coverCachedFilePath = "";
										error = ex.Message;
										//continue;
									}
								}
							}
						}
						catch (Exception ex)
						{
							coverCachedFilePath = "";
							error = ex.Message;
							//continue;
						}
					}
				}

				CoverDownloadProgressArg progressArg = new CoverDownloadProgressArg(searchResult, loadCount, metadata, coverCachedFilePath, error);

				backgroundWorker.ReportProgress((int)(loadCount / (float)maxCount * 100), progressArg);
			}

			CoverDownloadCompletedArg completedArg = new CoverDownloadCompletedArg(/*loadCount, maxCount, cacheCount, */searchResult);

			e.Result = completedArg;
		}

		private void DownloadCoverBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			CoverDownloadProgressArg progressArg = e.UserState as CoverDownloadProgressArg;

			if (progressArg != null)
			{
				Metadata metadata = progressArg.Metadata;

				OnCoverDownloadReportProgress(new CoverDownloadReportProgressEventArgs(
					/*arg.LoadCount, arg.LoadTotal, arg.CacheCount, arg.PageIndex, arg.Metadata, arg.PageCachedFilePath, arg.DownloadedBytes, arg.DownloadSize, arg.Error*/
					progressArg.SearchResult, progressArg.ItemIndex, metadata, progressArg.CoverCachedFilePath, progressArg.Error
					));
			}
		}

		private void DownloadCoverBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				MessageBox.Show(e.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			if (!e.Cancelled)
			{
				CoverDownloadCompletedArg completedArg = e.Result as CoverDownloadCompletedArg;
				//CoverDownloadCancelledArg cancelledArg = e.Result as CoverDownloadCancelledArg;

				if (completedArg != null)
				{
					OnCoversDownloadCompleted(new CoverDownloadCompletedEventArgs(/*completedArg.LoadCount, completedArg.LoadTotal, completedArg.CacheCount, completedArg.Metadata*/completedArg.SearchResult));
				}
				/*else if (cancelledArg != null)
				{
					OnCoverDownloadCancelled(new GalleryDownloadCancelledEventArgs(cancelledArg.GalleryId, cancelledArg.Error));
				}*/
			}

			IsDone = true;
		}

		private static bool ShouldFilter(Metadata metadata, MetadataKeywordLists metadataKeywordLists, CoverDownloaderFilters filters)
		{
			if (filters.HasFlag(CoverDownloaderFilters.Hidelist)
				&& metadataKeywordLists.Hidelist.IsInMetadata(metadata))
			{
				return true;
			}

			return false;
		}

		private class DownloadCoversRunArg
		{
			public HttpClient HttpClient { get; }
			public IPathFormatter PathFormatter { get; }
			public SearchResult SearchResult { get; }
			public MetadataKeywordLists MetadataKeywordLists { get; }
			public CoverDownloaderFilters Filters { get; }

			public DownloadCoversRunArg(HttpClient httpClient, IPathFormatter pathFormatter, SearchResult searchResult, MetadataKeywordLists metadataKeywordLists, CoverDownloaderFilters filters)
			{
				HttpClient = httpClient;
				PathFormatter = pathFormatter;
				SearchResult = searchResult;
				MetadataKeywordLists = metadataKeywordLists;
				Filters = filters;
			}
		}

		private class CoverDownloadProgressArg
		{
			//public int LoadCount { get; }
			//public int LoadTotal { get; }
			//public int CacheCount { get; }
			public SearchResult SearchResult { get; }
			public int ItemIndex { get; }
			public Metadata Metadata { get; }
			public string CoverCachedFilePath { get; }
			//public int DownloadedBytes { get; }
			//public int DownloadSize { get; }
			public string Error { get; }

			public CoverDownloadProgressArg(SearchResult searchResult,/*int loadCount, int loadTotal, int cacheCount, */int itemIndex, Metadata metadata, string coverCachedFilePath, /*int downloadedBytes, int downloadSize,*/ string error)
			{
				SearchResult = searchResult;
				//LoadCount = loadCount;
				//LoadTotal = loadTotal;
				//CacheCount = cacheCount;
				ItemIndex = itemIndex;
				Metadata = metadata;
				CoverCachedFilePath = coverCachedFilePath;
				//DownloadedBytes = DownloadedBytes;
				//DownloadSize = downloadSize;
				Error = error;
			}
		}

		private class CoverDownloadCompletedArg
		{
			//public int LoadCount { get; }
			//public int LoadTotal { get; }
			//public int CacheCount { get; }
			//public Metadata Metadata { get; }
			public SearchResult SearchResult { get; }

			public CoverDownloadCompletedArg(/*int loadCount, int loadTotal, int cacheCount, Metadata metadata*/SearchResult searchResult)
			{
				//LoadCount = loadCount;
				//LoadTotal = loadTotal;
				//CacheCount = cacheCount;
				//Metadata = metadata;
				SearchResult = searchResult;
			}
		}
	}

	[Flags]
	public enum CoverDownloaderFilters
	{
		None,
		Hidelist = 1,
		All = Hidelist
	}

	public delegate void CoverDownloadStartedEventHandler(object sender, CoversDownloadStartedEventArgs e);
	public delegate void CoverDownloadReportProgressEventHandler(object sender, CoverDownloadReportProgressEventArgs e);
	public delegate void CoverDownloadCompletedEventHandler(object sender, CoverDownloadCompletedEventArgs e);

	public class CoverDownloadReportProgressEventArgs : EventArgs
	{
		public SearchResult SearchResult { get; }
		public int ItemIndex { get; }
		public Metadata Metadata { get; }
		public string Path { get; }
		public string Error { get; }

		public CoverDownloadReportProgressEventArgs(SearchResult searchResult, int itemIndex, Metadata metadata, string path, string error)
		{
			SearchResult = searchResult;
			ItemIndex = itemIndex;
			Metadata = metadata;
			Path = path;
			Error = error;
		}

		public object[] ToObjectArray()
		{
			return new object[]
			{
					string.Join(" ", SearchResult.Result.Select(x => x.Id).ToArray()),
					ItemIndex,
					Metadata?.Id,
					Path,
					Error
			};
		}
	}

	public class CoverDownloadCompletedEventArgs : EventArgs
	{
		//public int ProgressPercentage { get; }
		//public int LoadCount { get; }
		//public int LoadTotal { get; }
		//public int CacheCount { get; }
		//public Metadata Metadata { get; }
		public SearchResult SearchResult { get; }

		public CoverDownloadCompletedEventArgs(/*int loadCount, int loadTotal, int cacheCount, Metadata metadata*/SearchResult searchResult)
		{
			//LoadCount = loadCount;
			//LoadTotal = loadTotal;
			//CacheCount = cacheCount;
			//Metadata = metadata;
			SearchResult = searchResult;
		}

		public object[] ToObjectArray()
		{
			return new object[]
			{
					//LoadCount,
					//LoadTotal,
					//CacheCount,
					//Metadata.Images.Pages.Count,
					//Metadata.Id
					string.Join(" ", SearchResult.Result.Select(x => x.Id).ToArray())
			};
		}
	}


	public class CoversDownloadStartedEventArgs : EventArgs
	{
		public SearchResult SearchResult { get; }
		public Metadata Metadata { get; }

		public CoversDownloadStartedEventArgs(SearchResult searchResult)
		{
			SearchResult = searchResult;
		}

		public object[] ToObjectArray()
		{
			return new object[]
			{
					string.Join(" ", SearchResult.Result.Select(x => x.Id).ToArray())
			};
		}
	}
}
