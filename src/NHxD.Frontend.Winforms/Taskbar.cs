using Ash.System.Windows.ShellProvider;
using Nhentai;
using System;
using System.Linq;

namespace NHxD.Frontend.Winforms
{
	public class ProgressBar
	{
		private int value;
		private TaskbarProgressBarState state;

		public int Value
		{
			get { return value; }
			set { this.value = value; OnValueChanged(); }
		}

		public TaskbarProgressBarState State
		{
			get { return state; }
			set { state = value; OnStateChanged(); }
		}

		public event EventHandler ValueChanged = delegate { };
		public event EventHandler StateChanged = delegate { };

		protected virtual void OnValueChanged()
		{
			ValueChanged.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnStateChanged()
		{
			StateChanged.Invoke(this, EventArgs.Empty);
		}
	}

	public class Taskbar
	{
		private readonly TaskbarManager taskbarManager;
		private readonly ProgressBar progressBar;

		private int loadCount;
		private int loadTotal;

		public int LoadCount => loadCount;
		public int LoadTotal => loadTotal;
		public ProgressBar ProgressBar => progressBar;

		public CoverDownloader CoverDownloader { get; }
		public GalleryDownloader GalleryDownloader { get; }
		public PageDownloader PageDownloader { get; }
		public ISearchResultCache SearchResultCache { get; }
		public ICacheFileSystem CacheFileSystem { get; }

		public IntPtr WindowHandle { get; set; }

		public Taskbar(CoverDownloader coverDownloader, GalleryDownloader galleryDownloader, PageDownloader pageDownloader, ISearchResultCache searchResultCache, ICacheFileSystem cacheFileSystem)
		{
			CoverDownloader = coverDownloader;
			GalleryDownloader = galleryDownloader;
			PageDownloader = pageDownloader;
			SearchResultCache = searchResultCache;
			CacheFileSystem = cacheFileSystem;

			taskbarManager = new TaskbarManager();
			progressBar = new ProgressBar();

			progressBar.ValueChanged += ProgressBar_ValueChanged;
			progressBar.StateChanged += ProgressBar_StateChanged;

			CoverDownloader.CoversDownloadStarted += CoverDownloader_CoversDownloadStarted;
			CoverDownloader.CoverDownloadReportProgress += CoverDownloader_CoverDownloadReportProgress;

			GalleryDownloader.GalleryDownloadStarted += GalleryDownloader_GalleryDownloadStarted;
			GalleryDownloader.GalleryDownloadReportProgress += GalleryDownloader_GalleryDownloadReportProgress;

			PageDownloader.PagesDownloadEnqueued += PageDownloader_PagesDownloadEnqueued;
			PageDownloader.PageDownloadReportProgress += PageDownloader_PageDownloadReportProgress;
		}

		private void ProgressBar_StateChanged(object sender, EventArgs e)
		{
			ProgressBar progressBar = sender as ProgressBar;

			taskbarManager.SetState(WindowHandle, progressBar.State);
		}

		private void ProgressBar_ValueChanged(object sender, EventArgs e)
		{
			ProgressBar progressBar = sender as ProgressBar;

			taskbarManager.SetValue(WindowHandle, progressBar.Value, 100);
		}

		private void CoverDownloader_CoverDownloadReportProgress(object sender, CoverDownloadReportProgressEventArgs e)
		{
			if (loadCount >= loadTotal)
			{
				return;
			}

			++loadCount;

			UpdateProgressBarValue();
		}

		private void CoverDownloader_CoversDownloadStarted(object sender, CoversDownloadStartedEventArgs e)
		{
			if (e.SearchResult != null && e.SearchResult.Result != null)
			{
				AddTasks(e.SearchResult.Result.Count());
			}
		}

		private void GalleryDownloader_GalleryDownloadReportProgress(object sender, GalleryDownloadReportProgressEventArgs e)
		{
			if (loadCount >= loadTotal)
			{
				return;
			}

			++loadCount;

			UpdateProgressBarValue();
		}

		private void GalleryDownloader_GalleryDownloadStarted(object sender, GalleryDownloadStartedEventArgs e)
		{
			AddTasks(1);
		}

		private void PageDownloader_PageDownloadReportProgress(object sender, PageDownloadReportProgressEventArgs e)
		{
			if (loadCount >= loadTotal)
			{
				return;
			}

			if (!e.Error.Equals("SKIP", StringComparison.OrdinalIgnoreCase))
			{
				++loadCount;
			}

			UpdateProgressBarValue();
		}

		private void PageDownloader_PagesDownloadEnqueued(object sender, PagesDownloadStartedEventArgs e)
		{
			if (e.PageIndices == null)
			{
				Metadata metadata = SearchResultCache.Find(e.GalleryId);

				if (metadata != null)
				{
					AddTasks(metadata.Images.Pages.Count());
				}
			}
			else
			{
				AddTasks(e.PageIndices.Length);
			}
		}

		public void AddTasks(int amount)
		{
			if (amount == 0)
			{
				return;
			}

			if (loadTotal == 0)
			{
				progressBar.State = TaskbarProgressBarState.Indeterminate;
			}

			loadTotal += amount;

			int progressPercentage = (int)((loadCount / (float)loadTotal) * 100.0f);

			progressBar.Value = progressPercentage;
		}

		private void UpdateProgressBarValue()
		{
			if (loadTotal == 0)
			{
				return;
			}

			int progressPercentage = (int)((loadCount / (float)loadTotal) * 100.0f);

			progressBar.Value = progressPercentage;

			if (progressPercentage == 100)
			{
				progressBar.State = TaskbarProgressBarState.None;
				loadCount = 0;
				loadTotal = 0;
			}
		}
	}

	public delegate void TaskbarProgressBarStateChangedEventHandler(object sender, TaskbarProgressBarStateChangedEventArgs e);

	public class TaskbarProgressBarStateChangedEventArgs : EventArgs
	{
		public TaskbarProgressBarState TaskbarProgressBarState { get; }

		public TaskbarProgressBarStateChangedEventArgs(TaskbarProgressBarState taskbarProgressBarState)
		{
			TaskbarProgressBarState = taskbarProgressBarState;
		}
	}
}
