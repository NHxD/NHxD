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
	public class PageDownloaderJob : BackgroundWorkerJobBase
	{
		public event PageDownloadReportProgressEventHandler PageDownloadReportProgress = delegate { };
		public event PageDownloadStartedEventHandler PagesDownloadStarted = delegate { };
		public event PageDownloadCompletedEventHandler PagesDownloadCancelled = delegate { };
		public event PageDownloadCompletedEventHandler PagesDownloadCompleted = delegate { };

		public HttpClient HttpClient { get; }
		public IPathFormatter PathFormatter { get; }
		public ISearchResultCache SearchResultCache { get; }
		public ICacheFileSystem CacheFileSystem { get; }
		public int GalleryId { get; }
		public int[] PageIndices { get; }

		public PageDownloaderJob(HttpClient httpClient, IPathFormatter pathFormatter, ISearchResultCache searchResultCache, ICacheFileSystem cacheFileSystem, int galleryId, int[] pageIndices)
		{
			HttpClient = httpClient;
			PathFormatter = pathFormatter;
			SearchResultCache = searchResultCache;
			CacheFileSystem = cacheFileSystem;
			GalleryId = galleryId;
			PageIndices = pageIndices;

			BackgroundWorker.WorkerReportsProgress = true;
			BackgroundWorker.WorkerSupportsCancellation = true;

			BackgroundWorker.DoWork += DownloadPagesBackgroundWorker_DoWork;
			BackgroundWorker.ProgressChanged += DownloadPagesBackgroundWorker_ProgressChanged;
			BackgroundWorker.RunWorkerCompleted += DownloadPagesBackgroundWorker_RunWorkerCompleted;
		}

		protected virtual void OnPagesDownloadStarted(PagesDownloadStartedEventArgs e)
		{
			PagesDownloadStarted.Invoke(this, e);
		}

		protected virtual void OnPagesDownloadCancelled(PageDownloadCompletedEventArgs e)
		{
			PagesDownloadCancelled.Invoke(this, e);
		}

		protected virtual void OnPagesDownloadCompleted(PageDownloadCompletedEventArgs e)
		{
			PagesDownloadCompleted.Invoke(this, e);
		}

		protected virtual void OnPageDownloadReportProgress(PageDownloadReportProgressEventArgs e)
		{
			PageDownloadReportProgress.Invoke(this, e);
		}

		public override void RunAsync()
		{
			if (BackgroundWorker.IsBusy)
			{
				MessageBox.Show("Program is busy", "Download in progress", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			OnPagesDownloadStarted(new PagesDownloadStartedEventArgs(PageIndices, GalleryId));

			DownloadPagesRunArg runArg = new DownloadPagesRunArg(GalleryId, PageIndices, HttpClient, PathFormatter, SearchResultCache, CacheFileSystem);

			BackgroundWorker.RunWorkerAsync(runArg);
		}

		private static void DownloadPagesBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			BackgroundWorker backgroundWorker = sender as BackgroundWorker;
			DownloadPagesRunArg runArg = e.Argument as DownloadPagesRunArg;
			Metadata metadata = runArg.SearchResultCache.Find(runArg.Id);

			if (metadata == null)
			{
				string metadataFilePath;

				if (runArg.PathFormatter.IsEnabled)
				{
					metadataFilePath = runArg.PathFormatter.GetMetadata(runArg.Id);
				}
				else
				{
					// NOTE: must be an absolute path for the webbrowser to display the images correctly.
					metadataFilePath = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", runArg.PathFormatter.GetCacheDirectory(), runArg.Id, ".json");
				}

				metadata = JsonUtility.LoadFromFile<Metadata>(metadataFilePath);

				if (metadata == null)
				{
					// TODO? redownload it instead of aborting?

					e.Result = runArg.Id;
					//e.Cancel = true;
					return;
				}
			}

			int maxCount = (runArg.PageIndices != null && runArg.PageIndices.Length > 0) ?
				runArg.PageIndices.Count() : metadata.Images.Pages.Count;
			int loadCount = 0;
			int[] cachedPageIndices = runArg.CacheFileSystem.GetCachedPageIndices(metadata.Id);
			int cacheCount = cachedPageIndices.Length;

			for (int i = 0; i < metadata.Images.Pages.Count; ++i)
			{
				if (backgroundWorker.CancellationPending)
				{
					PageDownloadCompletedArg cancelledArg = new PageDownloadCompletedArg(loadCount, maxCount, cacheCount, metadata);

					e.Result = cancelledArg;
					//e.Cancel = true;
					return;
				}

				string error = "";
				string pageCachedFilePath;
				if (runArg.PathFormatter.IsEnabled)
				{
					pageCachedFilePath = runArg.PathFormatter.GetPage(metadata, i);
				}
				else
				{
					string paddedIndex = (i + 1).ToString(CultureInfo.InvariantCulture).PadLeft(global::NHxD.Frontend.Winforms.PathFormatter.GetBaseCount(metadata.Images.Pages.Count), '0');

					pageCachedFilePath = string.Format(CultureInfo.InvariantCulture, "{0}{1}/{2}{3}",
						runArg.PathFormatter.GetCacheDirectory(), metadata.Id, paddedIndex, metadata.Images.Pages[i].GetFileExtension()).Replace('\\', '/');
				}

				bool shouldFilter = (runArg.PageIndices != null && runArg.PageIndices.Length > 0) ?
					ShouldFilterPage(metadata, i, runArg.PageIndices)
					: false;

				if (shouldFilter)
				{
					pageCachedFilePath = "";
					error = "SKIP";
				}
				else
				{
					++loadCount;

					if (!File.Exists(pageCachedFilePath))
					{
						try
						{
							string uri = string.Format(CultureInfo.InvariantCulture, "https://i.nhentai.net/galleries/{0}/{1}{2}", metadata.MediaId, i + 1, metadata.Images.Pages[i].GetFileExtension());

							using (HttpResponseMessage response = Task.Run(() => runArg.HttpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead)).GetAwaiter().GetResult())
							{
								if (!response.IsSuccessStatusCode)
								{
									pageCachedFilePath = "";
									error = string.Format(CultureInfo.InvariantCulture, "{0} ({1})", response.ReasonPhrase, response.StatusCode);
								}
								else
								{
									try
									{
										byte[] imageData = Task.Run(() => response.Content.ReadAsByteArrayAsync()).GetAwaiter().GetResult();

										// TODO: the issue is that I have designed things to be immutable so metadatas can't change.
										// but even if I do change the metadata and replace it in the searchResult,
										// it won't affect other cached searchResults with the same metadata. (because searchresults embed metadatas directly instead of storing indices to them)
										// so I might be able to save the changes to the metadata (file), one searchResult, but not everything (without greatly impacting performances)
										/*
										//if (runArg.FixFileExtension)
										{
											ImageType pageImageType = metadata.Images.Pages[i].Type;
											ImageType? realPageImageType = null;
											
											if (imageData.Length >= 8
												&& imageData.Match(0, 8, PngFileSignature))
											{
												realPageImageType = ImageType.Png;
											}
											else if (imageData.Length >= 8
												&& imageData.Match(0, 2, JfifStartOfImageSegmentHeader)
												&& imageData.Match(2, 2, JfifStartOfImageSegmentHeader)
												&& imageData.Match(6, 5, JfifApp0SegmentSegmentIdentifier))
											{
												realPageImageType = ImageType.Jpeg;
											}
											else if (imageData.Length >= 8
												&& imageData.Match(0, 8, GifFileSignature))
											{
												realPageImageType = ImageType.Gif;
											}
											else
											{
												// unknown format.
											}

											if (pageImageType != realPageImageType)
											{

											}
										}
										*/
										Directory.CreateDirectory(Path.GetDirectoryName(pageCachedFilePath));
										File.WriteAllBytes(pageCachedFilePath, imageData);

										++cacheCount;
									}
									catch (Exception ex)
									{
										pageCachedFilePath = "";
										error = ex.Message;
										//continue;
									}
								}
							}
						}
						catch (Exception ex)
						{
							pageCachedFilePath = "";
							error = ex.Message;
						}
					}
				}

				PageDownloadProgressArg progressArg = new PageDownloadProgressArg(loadCount, maxCount, cacheCount, i, metadata, pageCachedFilePath, 0, 0, error);

				backgroundWorker.ReportProgress((int)(loadCount / (float)maxCount * 100), progressArg);
			}

			PageDownloadCompletedArg completedArg = new PageDownloadCompletedArg(loadCount, maxCount, cacheCount, metadata);

			e.Result = completedArg;
		}

		private void DownloadPagesBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			PageDownloadProgressArg arg = e.UserState as PageDownloadProgressArg;

			OnPageDownloadReportProgress(new PageDownloadReportProgressEventArgs(arg.LoadCount, arg.LoadTotal, arg.CacheCount, arg.PageIndex, arg.Metadata, arg.PageCachedFilePath, arg.DownloadedBytes, arg.DownloadSize, arg.Error));
		}

		private void DownloadPagesBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				MessageBox.Show(e.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			if (!e.Cancelled)
			{
				PageDownloadCompletedArg completedArg = e.Result as PageDownloadCompletedArg;

				if (completedArg != null)
				{
					OnPagesDownloadCompleted(new PageDownloadCompletedEventArgs(completedArg.LoadCount, completedArg.LoadTotal, completedArg.CacheCount, completedArg.Metadata));
				}
			}

			IsDone = true;
		}

		private static bool ShouldFilterPage(Metadata metadata, int pageIndex, int[] pageIndices)
		{
			return !pageIndices.Any(x => x == pageIndex);
		}

		private class DownloadPagesRunArg
		{
			public int Id { get; }
			public int[] PageIndices { get; }
			public HttpClient HttpClient { get; }
			public IPathFormatter PathFormatter { get; }
			public ISearchResultCache SearchResultCache { get; }
			public ICacheFileSystem CacheFileSystem { get; }

			public DownloadPagesRunArg(int galleryId, int[] pageIndices
				, HttpClient httpClient
				, IPathFormatter pathFormatter
				, ISearchResultCache searchResultCache
				, ICacheFileSystem cacheFileSystem)
			{
				Id = galleryId;
				PageIndices = pageIndices;
				HttpClient = httpClient;
				PathFormatter = pathFormatter;
				SearchResultCache = searchResultCache;
				CacheFileSystem = cacheFileSystem;
			}
		}

		private class PageDownloadProgressArg
		{
			public int LoadCount { get; }
			public int LoadTotal { get; }
			public int CacheCount { get; }
			public int PageIndex { get; }
			public Metadata Metadata { get; }
			public string PageCachedFilePath { get; }
			public int DownloadedBytes { get; }
			public int DownloadSize { get; }
			public string Error { get; }

			public PageDownloadProgressArg(int loadCount, int loadTotal, int cacheCount, int pageIndex, Metadata metadata, string pageCachedFilePath, int downloadedBytes, int downloadSize, string error)
			{
				LoadCount = loadCount;
				LoadTotal = loadTotal;
				CacheCount = cacheCount;
				PageIndex = pageIndex;
				Metadata = metadata;
				PageCachedFilePath = pageCachedFilePath;
				DownloadedBytes = downloadedBytes;
				DownloadSize = downloadSize;
				Error = error;
			}
		}

		private class PageDownloadCompletedArg
		{
			public int LoadCount { get; }
			public int LoadTotal { get; }
			public int CacheCount { get; }
			public Metadata Metadata { get; }

			public PageDownloadCompletedArg(int loadCount, int loadTotal, int cacheCount, Metadata metadata)
			{
				LoadCount = loadCount;
				LoadTotal = loadTotal;
				CacheCount = cacheCount;
				Metadata = metadata;
			}
		}
	}

	/*
	private static readonly byte[] GifFileSignature = System.Text.Encoding.ASCII.GetBytes("GIF87");
	private static readonly byte[] PngFileSignature = { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a };
	private static readonly byte[] JfifStartOfImageSegmentHeader = { 0xFF, 0xD8 };
	private static readonly byte[] JfifApp0SegmentSegmentHeader = { 0xFF, 0xE0 };
	private static readonly byte[] JfifApp0SegmentSegmentIdentifier = System.Text.Encoding.ASCII.GetBytes("JFIF");

	internal static class ArrayExtensionMethods
	{
		public static bool Match(this byte[] array, int startIndex, byte[] pattern)
		{
			return Match(array, startIndex, array.Length, pattern);
		}

		public static bool Match(this byte[] array, int startIndex, int endIndex, byte[] pattern)
		{
			if (pattern == null) { throw new ArgumentException("pattern is null.", "pattern"); }
			else if (startIndex < 0) { throw new ArgumentOutOfRangeException("startIndex", startIndex, "startIndex is negative."); }
			else if (endIndex < 0) { throw new ArgumentOutOfRangeException("endIndex", endIndex, "endIndex is negative."); }
			else if (startIndex > endIndex) { throw new ArgumentOutOfRangeException("endIndex", endIndex, "endIndex is less than startIndex."); }

			for (int i = 0; (i < pattern.Length) && (startIndex + i < endIndex); ++i)
			{
				if (array[startIndex + i] != pattern[i])
				{
					return false;
				}
			}

			return true;
		}
	}
	*/

	public delegate void PageDownloadStartedEventHandler(object sender, PagesDownloadStartedEventArgs e);
	public delegate void PageDownloadReportProgressEventHandler(object sender, PageDownloadReportProgressEventArgs e);
	public delegate void PageDownloadCompletedEventHandler(object sender, PageDownloadCompletedEventArgs e);

	public class PageDownloadReportProgressEventArgs : EventArgs
	{
		public int LoadCount { get; }
		public int LoadTotal { get; }
		public int CacheCount { get; }
		public int PageIndex { get; }
		public Metadata Metadata { get; }
		public string PageCachedFilePath { get; }
		public int DownloadedBytes { get; }
		public int DownloadSize { get; }
		public string Error { get; }

		public PageDownloadReportProgressEventArgs(int loadCount, int loadTotal, int cacheCount, int pageIndex, Metadata metadata, string pageCachedFilePath, int downloadedBytes, int downloadSize, string error)
		{
			LoadCount = loadCount;
			LoadTotal = loadTotal;
			CacheCount = cacheCount;
			PageIndex = pageIndex;
			Metadata = metadata;
			PageCachedFilePath = pageCachedFilePath;
			DownloadedBytes = downloadedBytes;
			DownloadSize = downloadSize;
			Error = error;
		}

		public object[] ToObjectArray()
		{
			return new object[]
			{
					LoadCount,
					LoadTotal,
					CacheCount,
					Metadata?.Images?.Pages?.Count ?? 0,
					PageIndex,
					Metadata?.Id,
					PageCachedFilePath,
					DownloadedBytes,
					DownloadSize,
					Error
			};
		}
	}


	public class PageDownloadCompletedEventArgs : EventArgs
	{
		public int LoadCount { get; }
		public int LoadTotal { get; }
		public int CacheCount { get; }
		public Metadata Metadata { get; }

		public PageDownloadCompletedEventArgs(int loadCount, int loadTotal, int cacheCount, Metadata metadata)
		{
			LoadCount = loadCount;
			LoadTotal = loadTotal;
			CacheCount = cacheCount;
			Metadata = metadata;
		}

		public object[] ToObjectArray()
		{
			return new object[]
			{
					LoadCount,
					LoadTotal,
					CacheCount,
					Metadata?.Images?.Pages?.Count ?? 0,
					Metadata?.Id
			};
		}
	}


	public class PagesDownloadStartedEventArgs : EventArgs
	{
		public int[] PageIndices { get; }
		public int GalleryId { get; }

		public PagesDownloadStartedEventArgs(int[] pageIndices, int galleryId)
		{
			PageIndices = pageIndices;
			GalleryId = galleryId;
		}

		public object[] ToObjectArray()
		{
			return new object[]
			{
					PageIndices != null ? string.Join(" ", PageIndices) : "",
					GalleryId
			};
		}
	}
}
