using Ash.System.ComponentModel;
using Newtonsoft.Json;
using Nhentai;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	using Formatting = Newtonsoft.Json.Formatting;

	public class GalleryDownloaderJob : BackgroundWorkerJobBase
	{
		public event GalleryDownloadReportProgressEventHandler GalleryDownloadReportProgress = delegate { };
		public event GalleryDownloadStartedEventHandler GalleryDownloadStarted = delegate { };
		public event GalleryDownloadCancelledEventHandler GalleryDownloadCancelled = delegate { };
		public event GalleryDownloadCompletedEventHandler GalleryDownloadCompleted = delegate { };

		public int GalleryId { get; }
		public HttpClient HttpClient { get; }
		public IPathFormatter PathFormatter { get; }
		public ISearchResultCache SearchResultCache { get; }

		public GalleryDownloaderJob(HttpClient httpClient, IPathFormatter pathFormatter, ISearchResultCache searchResultCache, int galleryId)
		{
			GalleryId = galleryId;
			HttpClient = httpClient;
			PathFormatter = pathFormatter;
			SearchResultCache = searchResultCache;

			BackgroundWorker.WorkerReportsProgress = true;
			BackgroundWorker.WorkerSupportsCancellation = true;

			BackgroundWorker.DoWork += DownloadGalleryBackgroundWorker_DoWork;
			BackgroundWorker.ProgressChanged += DownloadGalleryBackgroundWorker_ProgressChanged;
			BackgroundWorker.RunWorkerCompleted += DownloadGalleryBackgroundWorker_RunWorkerCompleted;
		}

		protected virtual void OnGalleryDownloadStarted(GalleryDownloadStartedEventArgs e)
		{
			GalleryDownloadStarted.Invoke(this, e);
		}

		protected virtual void OnGalleryDownloadCancelled(GalleryDownloadCancelledEventArgs e)
		{
			GalleryDownloadCancelled.Invoke(this, e);
		}

		protected virtual void OnGalleryDownloadCompleted(GalleryDownloadCompletedEventArgs e)
		{
			GalleryDownloadCompleted.Invoke(this, e);
		}

		protected virtual void OnGalleryDownloadReportProgress(GalleryDownloadReportProgressEventArgs e)
		{
			GalleryDownloadReportProgress.Invoke(this, e);
		}

		public override void RunAsync()
		{
			if (BackgroundWorker.IsBusy)
			{
				MessageBox.Show("Program is busy", "Download in progress", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			OnGalleryDownloadStarted(new GalleryDownloadStartedEventArgs(GalleryId));

			DownloadGalleryRunArg runArg = new DownloadGalleryRunArg(GalleryId, HttpClient, PathFormatter, SearchResultCache);

			BackgroundWorker.RunWorkerAsync(runArg);
		}

		private static void DownloadGalleryBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			BackgroundWorker backgroundWorker = sender as BackgroundWorker;
			DownloadGalleryRunArg runArg = e.Argument as DownloadGalleryRunArg;
			string cachedMetadataPath;
			if (runArg.PathFormatter.IsEnabled)
			{
				cachedMetadataPath = runArg.PathFormatter.GetMetadata(runArg.Id);
			}
			else
			{
				cachedMetadataPath = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", runArg.PathFormatter.GetCacheDirectory(), runArg.Id, ".json");
			}
			Metadata metadata = runArg.SearchResultCache.Find(runArg.Id);

			if (metadata == null)
			{
				metadata = JsonUtility.LoadFromFile<Metadata>(cachedMetadataPath);
			}

			if (metadata == null)
			{
				string error = "";

				try
				{
					string uri = string.Format(CultureInfo.InvariantCulture, "https://nhentai.net/api/gallery/{0}", runArg.Id);

					using (HttpResponseMessage response = Task.Run(() => runArg.HttpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead)).GetAwaiter().GetResult())
					{
						if (!response.IsSuccessStatusCode)
						{
							response.EnsureSuccessStatusCode();
							//error = string.Format(CultureInfo.InvariantCulture, "{0} ({1})", response.ReasonPhrase, response.StatusCode);
						}
						else
						{
							try
							{
								string jsonText = Task.Run(() => response.Content.ReadAsStringAsync()).GetAwaiter().GetResult();

								// TODO: show actual download progress...
								{
									int currentDownloadSize = 1;
									int totalDownloadSize = 1;

									GalleryDownloadProgressArg progressArg = new GalleryDownloadProgressArg(0, 0, error);

									backgroundWorker.ReportProgress((int)(currentDownloadSize / (float)totalDownloadSize * 100), progressArg);
								}

								metadata = JsonConvert.DeserializeObject<Metadata>(jsonText);

								// TODO: handle unsafe concurrent workers downloading at the same time.

								//if (!File.Exists(cachedMetadataPath))
								{
									try
									{
										Directory.CreateDirectory(Path.GetDirectoryName(cachedMetadataPath));
										File.WriteAllText(cachedMetadataPath, JsonConvert.SerializeObject(metadata, Formatting.Indented));
									}
									catch (Exception ex)
									{
										GalleryDownloadCancelledArg cancelledArg = new GalleryDownloadCancelledArg(runArg.Id, ex.Message);

										e.Result = cancelledArg;

										return;
									}
								}
							}
							catch (Exception ex)
							{
								GalleryDownloadCancelledArg cancelledArg = new GalleryDownloadCancelledArg(runArg.Id, ex.Message);

								e.Result = cancelledArg;

								return;
							}
						}
					}
				}
				catch (Exception ex)
				{
					GalleryDownloadCancelledArg cancelledArg = new GalleryDownloadCancelledArg(runArg.Id, ex.Message);

					e.Result = cancelledArg;

					return;
				}
			}

			GalleryDownloadCompletedArg completedArg = new GalleryDownloadCompletedArg(runArg.Id, metadata);

			e.Result = completedArg;
		}

		private void DownloadGalleryBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			GalleryDownloadProgressArg arg = e.UserState as GalleryDownloadProgressArg;

			OnGalleryDownloadReportProgress(new GalleryDownloadReportProgressEventArgs(arg.DownloadedBytes, arg.DownloadSize, arg.Error));
		}

		private void DownloadGalleryBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				MessageBox.Show(e.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			if (!e.Cancelled)
			{
				GalleryDownloadCompletedArg completedArg = e.Result as GalleryDownloadCompletedArg;
				GalleryDownloadCancelledArg cancelledArg = e.Result as GalleryDownloadCancelledArg;

				if (completedArg != null)
				{
					OnGalleryDownloadCompleted(new GalleryDownloadCompletedEventArgs(completedArg.GalleryId, completedArg.Metadata));
				}
				else if (cancelledArg != null)
				{
					OnGalleryDownloadCancelled(new GalleryDownloadCancelledEventArgs(cancelledArg.GalleryId, cancelledArg.Error));
				}
			}

			IsDone = true;
		}

		private class DownloadGalleryRunArg
		{
			public int Id { get; }
			public HttpClient HttpClient { get; }
			public IPathFormatter PathFormatter { get; }
			public ISearchResultCache SearchResultCache { get; }

			public DownloadGalleryRunArg(int galleryId, HttpClient httpClient, IPathFormatter pathFormatter, ISearchResultCache searchResultCache)
			{
				Id = galleryId;
				HttpClient = httpClient;
				PathFormatter = pathFormatter;
				SearchResultCache = searchResultCache;
			}
		}

		private class GalleryDownloadProgressArg
		{
			public int DownloadedBytes { get; }
			public int DownloadSize { get; }
			public string Error { get; }

			public GalleryDownloadProgressArg(int downloadedBytes, int downloadSize, string error)
			{
				DownloadedBytes = downloadedBytes;
				DownloadSize = downloadSize;
				Error = error;
			}
		}

		private class GalleryDownloadCancelledArg
		{
			public int GalleryId { get; }
			public string Error { get; }

			public GalleryDownloadCancelledArg(int galleryId, string error)
			{
				GalleryId = galleryId;
				Error = error;
			}
		}

		private class GalleryDownloadCompletedArg
		{
			public int GalleryId { get; }
			public Metadata Metadata { get; }

			public GalleryDownloadCompletedArg(int galleryId, Metadata metadata)
			{
				GalleryId = galleryId;
				Metadata = metadata;
			}
		}
	}

	public delegate void GalleryDownloadStartedEventHandler(object sender, GalleryDownloadStartedEventArgs e);
	public delegate void GalleryDownloadReportProgressEventHandler(object sender, GalleryDownloadReportProgressEventArgs e);
	public delegate void GalleryDownloadCancelledEventHandler(object sender, GalleryDownloadCancelledEventArgs e);
	public delegate void GalleryDownloadCompletedEventHandler(object sender, GalleryDownloadCompletedEventArgs e);

	public class GalleryDownloadReportProgressEventArgs : EventArgs
	{
		public int DownloadedBytes { get; }
		public int DownloadSize { get; }
		public string Error { get; }

		public GalleryDownloadReportProgressEventArgs(int downloadedBytes, int downloadSize, string error)
		{
			DownloadedBytes = downloadedBytes;
			DownloadSize = downloadSize;
			Error = error;
		}

		public object[] ToObjectArray()
		{
			return new object[]
			{
					DownloadedBytes,
					DownloadSize,
					Error
			};
		}
	}

	public class GalleryDownloadCancelledEventArgs : EventArgs
	{
		public int GalleryId { get; }
		public string Error { get; }

		public GalleryDownloadCancelledEventArgs(int galleryId, string error)
		{
			GalleryId = galleryId;
			Error = error;
		}

		public object[] ToObjectArray()
		{
			return new object[]
			{
					GalleryId,
					Error
			};
		}
	}

	public class GalleryDownloadCompletedEventArgs : EventArgs
	{
		public int GalleryId { get; }
		public Metadata Metadata { get; }

		public GalleryDownloadCompletedEventArgs(int galleryId, Metadata metadata)
		{
			GalleryId = galleryId;
			Metadata = metadata;
		}

		public object[] ToObjectArray()
		{
			return new object[]
			{
					GalleryId,
					Metadata?.Id
			};
		}
	}

	public class GalleryDownloadStartedEventArgs : EventArgs
	{
		public int GalleryId { get; }

		public GalleryDownloadStartedEventArgs(int galleryId)
		{
			GalleryId = galleryId;
		}

		public object[] ToObjectArray()
		{
			return new object[]
			{
					GalleryId
			};
		}
	}
}
