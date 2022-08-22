using Ash.System.Linq;
using Ash.System.Windows.Forms;
using Newtonsoft.Json;
using Nhentai;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	public partial class LibraryBrowserView : UserControl
	{
		private readonly WebBrowserEx webBrowser;
		private readonly BackgroundWorker backgroundWorker;

		// TODO: match different format patterns.
		//private Regex metadataJsonFileNameRegex = new Regex(@"^((\d+).json)$", RegexOptions.Compiled);

		//private SearchResult searchResult;

		public WebBrowser WebBrowser => webBrowser;

		public LibraryBrowserFilter LibraryBrowserFilter { get; }
		public LibraryModel LibraryModel { get; }
		public IPathFormatter PathFormatter { get; }
		public IQueryParser QueryParser { get; }
		public DocumentTemplate<ISearchProgressArg> LibraryCovergridTemplate { get; }
		public DocumentTemplate<ISearchArg> LibraryPreloadTemplate { get; }
		public DocumentTemplate<Metadata> LibraryCovergridItemTemplate { get; }
		public PageDownloader PageDownloader { get; }
		public CoverDownloader CoverLoader { get; }
		public MetadataKeywordLists MetadataKeywordLists { get; }
		public IMetadataCache MetadataCache { get; }
		public ISearchResultCache SearchResultCache { get; }
		public MetadataCacheSnapshot MetadataCacheSnapshot { get; }
		public Configuration.ConfigLibraryBrowserView LibraryBrowserSettings { get; }

		public LibraryBrowserView()
		{
			InitializeComponent();
		}
	
		public LibraryBrowserView(LibraryBrowserFilter libraryBrowserFilter, LibraryModel libraryModel, DocumentTemplate<ISearchProgressArg> libraryCovergridTemplate, DocumentTemplate<ISearchArg> libraryPreloadTemplate, DocumentTemplate<Metadata> libraryCovergridItemTemplate
			, IPathFormatter pathFormatter
			, IQueryParser queryParser
			, PageDownloader pageDownloader
			, CoverDownloader coverLoader
			, MetadataKeywordLists metadataKeywordLists
			, Configuration.ConfigLibraryBrowserView libraryBrowserSettings
			, IMetadataCache metadataCache
			, ISearchResultCache searchResultCache
			, MetadataCacheSnapshot metadataCacheSnapshot)
		{
			InitializeComponent();

			LibraryBrowserFilter = libraryBrowserFilter;
			LibraryModel = libraryModel;
			PathFormatter = pathFormatter;
			QueryParser = queryParser;
			MetadataCache = metadataCache;
			SearchResultCache = searchResultCache;
			MetadataCacheSnapshot = metadataCacheSnapshot;
			LibraryCovergridTemplate = libraryCovergridTemplate;
			LibraryPreloadTemplate = libraryPreloadTemplate;
			LibraryCovergridItemTemplate = libraryCovergridItemTemplate;
			PageDownloader = pageDownloader;
			CoverLoader = coverLoader;
			MetadataKeywordLists = metadataKeywordLists;
			LibraryBrowserSettings = libraryBrowserSettings;

			webBrowser = new WebBrowserEx();
			backgroundWorker = new BackgroundWorker();

			SuspendLayout();

			webBrowser.AllowWebBrowserDrop = false;
			webBrowser.Dock = DockStyle.Fill;
			webBrowser.Name = "libraryWebBrowser";
			webBrowser.BeforeDocumentCompleted += WebBrowser_BeforeDocumentCompleted;
			webBrowser.DocumentCompleted += WebBrowser_DocumentCompleted;

			backgroundWorker.WorkerReportsProgress = true;
			backgroundWorker.WorkerSupportsCancellation = true;

			backgroundWorker.DoWork += BackgroundWorker_DoWork;
			backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
			backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;

			PageDownloader.PageDownloadReportProgress += PageDownloader_PageDownloadReportProgress;
			PageDownloader.PagesDownloadStarted += PageDownloader_PagesDownloadStarted;
			PageDownloader.PagesDownloadCancelled += PageDownloader_PagesDownloadCancelled;
			PageDownloader.PagesDownloadCompleted += PageDownloader_PagesDownloadCompleted;

			CoverLoader.CoverDownloadReportProgress += CoverLoader_CoverDownloadReportProgress;
			CoverLoader.CoversDownloadStarted += CoverLoader_CoversDownloadStarted;
			CoverLoader.CoversDownloadCancelled += CoverLoader_CoversDownloadCancelled;
			CoverLoader.CoversDownloadCompleted += CoverLoader_CoversDownloadCompleted;

			MetadataKeywordLists.WhitelistChanged += Form_WhiteListChanged;
			MetadataKeywordLists.BlacklistChanged += Form_BlackListChanged;
			MetadataKeywordLists.IgnorelistChanged += Form_IgnoreListChanged;
			MetadataKeywordLists.HidelistChanged += Form_HideListChanged;

			//libraryModel.Poll += LibraryModel_Poll;

			libraryBrowserFilter.TextChanged += LibraryBrowserFilter_TextChanged;

			LibraryModel.PageIndexChanged += LibraryModel_PageIndexChanged;
			LibraryModel.SearchProgressArgChanged += LibraryModel_SearchProgressArgChanged;

			Controls.Add(webBrowser);

			ResumeLayout(false);
		}

		private void LibraryModel_SearchProgressArgChanged(object sender, EventArgs e)
		{
			CoverLoader.Download(LibraryModel.SearchProgressArg.SearchResult, CoverDownloaderFilters.None);
		}

		private void LibraryModel_PageIndexChanged(object sender, EventArgs e)
		{
			Search();
		}

		private void LibraryBrowserFilter_TextChanged(object sender, EventArgs e)
		{
			ApplyFilter(LibraryBrowserFilter.Text);
		}

		private void Form_HideListChanged(object sender, MetadataKeywordListChangedEventArgs e)
		{
			if (string.IsNullOrEmpty(WebBrowser.DocumentText))
			{
				return;
			}

			WebBrowser.Document?.InvokeScript("__onHidelistChanged", e.ToObjectArray());
		}

		private void Form_IgnoreListChanged(object sender, MetadataKeywordListChangedEventArgs e)
		{
			if (string.IsNullOrEmpty(WebBrowser.DocumentText))
			{
				return;
			}

			WebBrowser.Document?.InvokeScript("__onIgnorelistChanged", e.ToObjectArray());
		}

		private void Form_BlackListChanged(object sender, MetadataKeywordListChangedEventArgs e)
		{
			if (string.IsNullOrEmpty(WebBrowser.DocumentText))
			{
				return;
			}

			WebBrowser.Document?.InvokeScript("__onBlacklistChanged", e.ToObjectArray());
		}

		private void Form_WhiteListChanged(object sender, MetadataKeywordListChangedEventArgs e)
		{
			if (string.IsNullOrEmpty(WebBrowser.DocumentText))
			{
				return;
			}

			WebBrowser.Document?.InvokeScript("__onWhitelistChanged", e.ToObjectArray());
		}

		private void PageDownloader_PagesDownloadStarted(object sender, PagesDownloadStartedEventArgs e)
		{
			if (string.IsNullOrEmpty(WebBrowser.DocumentText))
			{
				return;
			}

			WebBrowser.Document?.InvokeScript("__onPagesDownloadStarted", e.ToObjectArray());
		}

		private void PageDownloader_PagesDownloadCompleted(object sender, PageDownloadCompletedEventArgs e)
		{
			if (string.IsNullOrEmpty(WebBrowser.DocumentText))
			{
				return;
			}

			WebBrowser.Document?.InvokeScript("__onPagesDownloadCompleted", e.ToObjectArray());
		}

		private void PageDownloader_PagesDownloadCancelled(object sender, PageDownloadCompletedEventArgs e)
		{
			if (string.IsNullOrEmpty(WebBrowser.DocumentText))
			{
				return;
			}

			WebBrowser.Document?.InvokeScript("__onPagesDownloadCancelled", e.ToObjectArray());
		}

		private void PageDownloader_PageDownloadReportProgress(object sender, PageDownloadReportProgressEventArgs e)
		{
			if (string.IsNullOrEmpty(WebBrowser.DocumentText))
			{
				return;
			}

			WebBrowser.Document?.InvokeScript("__onPageDownloadReportProgress", e.ToObjectArray());
		}

		private void CoverLoader_CoversDownloadStarted(object sender, CoversDownloadStartedEventArgs e)
		{
			if (string.IsNullOrEmpty(WebBrowser.DocumentText))
			{
				return;
			}

			WebBrowser.Document?.InvokeScript("__onCoversDownloadStarted", e.ToObjectArray());
		}

		private void CoverLoader_CoversDownloadCompleted(object sender, CoverDownloadCompletedEventArgs e)
		{
			if (string.IsNullOrEmpty(WebBrowser.DocumentText))
			{
				return;
			}

			WebBrowser.Document?.InvokeScript("__onCoversDownloadCompleted", e.ToObjectArray());
		}

		private void CoverLoader_CoversDownloadCancelled(object sender, CoverDownloadCompletedEventArgs e)
		{
			if (string.IsNullOrEmpty(WebBrowser.DocumentText))
			{
				return;
			}

			WebBrowser.Document?.InvokeScript("__onCoversDownloadCancelled", e.ToObjectArray());
		}

		private void CoverLoader_CoverDownloadReportProgress(object sender, CoverDownloadReportProgressEventArgs e)
		{
			if (string.IsNullOrEmpty(WebBrowser.DocumentText))
			{
				return;
			}

			WebBrowser.Document?.InvokeScript("__onCoverDownloadReportProgress", e.ToObjectArray());
		}

		private void WebBrowser_BeforeDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			webBrowser.ZoomFactor = LibraryBrowserSettings.ZoomRatio;
		}

		public void ApplyFilter(string filter)
		{
			if (string.IsNullOrEmpty(WebBrowser.DocumentText))
			{
				return;
			}

			webBrowser.Document?.InvokeScript("__applyFilter", new object[] { filter ?? "" });
		}

		// FIXME
		/*
		private void LibraryModel_Poll(object sender, LibraryModel.LibraryEventArgs e)
		{
			LibraryModel.LibraryEvent fileSystemEvent = e.LibraryEvent;

			if (fileSystemEvent.EventType == LibraryModel.LibraryEventType.Rename)
			{
				FileSystemWatcher_Renamed(libraryModel.FileSystemWatcher, fileSystemEvent.EventData as RenamedEventArgs);
			}
			else if (fileSystemEvent.EventType == LibraryModel.LibraryEventType.Create)
			{
				FileSystemWatcher_Created(libraryModel.FileSystemWatcher, fileSystemEvent.EventData as FileSystemEventArgs);
			}
			else if (fileSystemEvent.EventType == LibraryModel.LibraryEventType.Delete)
			{
				FileSystemWatcher_Deleted(libraryModel.FileSystemWatcher, fileSystemEvent.EventData as FileSystemEventArgs);
			}
		}

		private void FileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
		{
			// TODO
		}

		// FIXME: I think this doesn't work anymore.
		private void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
		{
			int galleryId = ExtractGalleryIdFromFileName(e.Name);

			if (galleryId == -1)
			{
				return;
			}

			Metadata metadata = GetGalleryMetadata(galleryId, PathFormatter, SearchResultCache);

			if (metadata == null)
			{
				return;
			}

			HtmlDocument document = webBrowser.Document;

			if (document == null)
			{
				return;
			}

			HtmlElementCollection bodies = document.GetElementsByTagName("BODY");

			if (bodies == null || bodies.Count == 0)
			{
				return;
			}

			HtmlElement script = document.CreateElement("SCRIPT");
			script.SetAttribute("text", string.Format(CultureInfo.InvariantCulture, "searchResult.result.push({0}); ++searchResult.per_page", JsonConvert.SerializeObject(metadata)));

			bodies[0].AppendChild(script);

			webBrowser.Document.InvokeScript("__onGalleryAdded", new object[] { JsonConvert.SerializeObject(metadata) });

			// TODO: need to inject this html as well...
			HtmlElement searchResultContainer = document.GetElementById("search-result-container");
			
			string result = LibraryCovergridItemTemplate.GetText(metadata);
			
			searchResultContainer.InnerHtml = searchResultContainer.InnerHtml + result;
		}

		private void FileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
		{
			int galleryId = ExtractGalleryIdFromFileName(e.Name);

			if (galleryId == -1)
			{
				return;
			}

			//Metadata metadata = GetGalleryMetadata(galleryId, PathFormatter);

			webBrowser.Document.InvokeScript("__onGalleryRemoved", new object[] { galleryId });
		}
		*/

		public void Search()
		{
			SearchArg searchArg =
				LibraryModel.TagId != -1
				? new SearchArg(LibraryModel.TagId, LibraryModel.PageIndex, true)
				: new SearchArg(LibraryModel.Query, LibraryModel.PageIndex, true);

			// HACK: normally, filtered searches should go to its own tab and webbrowser document but that'd be too much work so just make it a temporary thing...
			LibraryModel.TagId = -1;
			LibraryModel.Query = "";

			webBrowser.Tag = searchArg;
			webBrowser.DocumentText = LibraryPreloadTemplate.GetFormattedText(searchArg);
		}

		private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			SearchArg searchArg = webBrowser.Tag as SearchArg;
			SearchProgressArg searchProgressArg = webBrowser.Tag as SearchProgressArg;

			if (searchArg != null)
			{
				WebBrowser_PreloadDocumentCompleted(sender, e);
			}
			else if (searchProgressArg != null)
			{
				WebBrowser_LibraryDocumentCompleted(sender, e);
			}
		}

		private void WebBrowser_PreloadDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			SearchArg searchArg = webBrowser.Tag as SearchArg;

			if (searchArg == null)
			{
				return;
			}
			
			if (backgroundWorker.IsBusy)
			{
				return;
			}

			LibraryLoadRunArg runArg = new LibraryLoadRunArg(PathFormatter, QueryParser, MetadataCache, SearchResultCache, MetadataCacheSnapshot, searchArg, LibraryBrowserSettings.NumResultsPerPage, LibraryModel.FileSystemWatcher.Path, LibraryBrowserSettings.SortType, LibraryBrowserSettings.GlobalSortType, LibraryBrowserSettings.SortOrder, LibraryBrowserSettings.GlobalSortOrder);

			backgroundWorker.RunWorkerAsync(runArg);
		}

		private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				MessageBox.Show(e.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			if (!e.Cancelled)
			{
				LibraryLoadCompletedArg completedArg = e.Result as LibraryLoadCompletedArg;
				SearchProgressArg searchProgressArg = new SearchProgressArg(completedArg.RunArg.SearchArg, completedArg.SearchResult, completedArg.RunArg.SearchArg.PageIndex, MetadataKeywordLists, completedArg.RunArg.SortType, completedArg.RunArg.SortOrder);

				//searchResult = completedArg.SearchResult;

				webBrowser.Tag = searchProgressArg;
				webBrowser.DocumentText = LibraryCovergridTemplate.GetFormattedText(searchProgressArg);
			}
		}

		private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			LibraryLoadProgressArg progressArg = e.UserState as LibraryLoadProgressArg;

			webBrowser.Document.InvokeScript("__onSearchResultLoaded", new object[]
			{
				progressArg?.RunArg?.SearchArg?.PageIndex ?? 0,
				progressArg?.RunArg?.SearchArg?.TagId ?? -1,
				progressArg?.RunArg?.SearchArg?.Query ?? "",
				progressArg?.RunArg?.SearchArg?.Target.ToString().ToLowerInvariant() ?? "",
				e.ProgressPercentage,
				progressArg?.CurrentLoadCount ?? 0,
				progressArg?.TotalLoadCount ?? 0,
				JsonConvert.SerializeObject(progressArg?.Metadata)
			});
		}

		private static void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			BackgroundWorker backgroundWorker = sender as BackgroundWorker;
			LibraryLoadRunArg runArg = e.Argument as LibraryLoadRunArg;
			List<int> galleryIds = new List<int>();
			SearchResult searchResult = new SearchResult();

			searchResult.Result = new List<Metadata>();
			searchResult.PerPage = runArg.NumItemsPerPage;

			if (runArg.SearchArg.TagId != -1)
			{
				runArg.MetadataCacheSnapshot.EnsureReady();

				IEnumerable<Metadata> results = runArg.MetadataCache.Items.Values;

				results = results.Where(x => x.Tags.Any(y =>
				{
					if (y.Id != runArg.SearchArg.TagId)
					{
						return false;
					}

					return true;
				}));

				int skipCount = (runArg.SearchArg.PageIndex - 1) * searchResult.PerPage;
				int totalItemCount = results.Count();
				int loadCount = 0;

				results = results
					.Skip(skipCount)
					.Take(searchResult.PerPage);

				galleryIds = results.Select(x => x.Id).ToList();

				foreach (Metadata metadata in results)
				{
					++loadCount;

					LibraryLoadProgressArg progressArg = new LibraryLoadProgressArg(runArg, loadCount, galleryIds.Count, metadata);

					backgroundWorker.ReportProgress((int)((loadCount / (float)galleryIds.Count) * 100), progressArg);

					if (metadata == null)
					{
						continue;
					}

					searchResult.Result.Add(metadata);
				}

				//searchResult.Result.AddRange(results);
				searchResult.NumPages = (int)Math.Ceiling(totalItemCount / (double)searchResult.PerPage);
			}
			else if (!string.IsNullOrEmpty(runArg.SearchArg.Query))
			{
				runArg.MetadataCacheSnapshot.EnsureReady();

				IEnumerable<Metadata> results = runArg.MetadataCache.Items.Values;

				string[] tokens = runArg.SearchArg.Query.Split(new char[] { ':' });
				string query;
				int tagId;
				string tagType;
				string tagName;
				int pageIndex;

				if (runArg.QueryParser.ParseTaggedSearch(tokens, out tagId, out tagType, out tagName, out pageIndex))
				{
					results = results.Where(x => x.Tags.Any(y =>
					{
						if (tagId != -1
						&& y.Id != tagId)
						{
							return false;
						}

						if (!string.IsNullOrEmpty(tagType)
						&& !y.Type.ToString().Equals(tagType, StringComparison.OrdinalIgnoreCase))
						{
							return false;
						}

						return true;
					}));
				}
				else if (runArg.QueryParser.ParseQuerySearch(tokens, out query, out pageIndex))
				{
					results = results.Where(x =>
					{
						// TODO: currently, the query parsing and evaluation is implemented on the JS side...
						/*
						if (x.Title.English.Equals(query, StringComparison.CurrentCulture))
						{
							return false;
						}
						*/
						return true;
					});
				}

				int skipCount = (runArg.SearchArg.PageIndex - 1) * searchResult.PerPage;
				int totalItemCount = results.Count();
				int loadCount = 0;

				results = results
					.Skip(skipCount)
					.Take(searchResult.PerPage);

				galleryIds = results.Select(x => x.Id).ToList();

				foreach (Metadata metadata in results)
				{
					++loadCount;

					LibraryLoadProgressArg progressArg = new LibraryLoadProgressArg(runArg, loadCount, galleryIds.Count, metadata);

					backgroundWorker.ReportProgress((int)((loadCount / (float)galleryIds.Count) * 100), progressArg);

					if (metadata == null)
					{
						continue;
					}

					searchResult.Result.Add(metadata);
				}

				//searchResult.Result.AddRange(results);
				searchResult.NumPages = (int)Math.Ceiling(totalItemCount / (double)searchResult.PerPage);
			}
			else
			if (Directory.Exists(runArg.LibraryPath))
			{
				DirectoryInfo dirInfo = new DirectoryInfo(runArg.LibraryPath);
				IEnumerable<DirectoryInfo> dirInfos =
					runArg.PathFormatter.IsEnabled ? dirInfo.EnumerateDirectories("*", SearchOption.AllDirectories)
					: dirInfo.EnumerateDirectories();

				dirInfos = dirInfos.OrderAllByTime(runArg.GlobalSortType, runArg.GlobalSortOrder);

				int skipCount = (runArg.SearchArg.PageIndex - 1) * searchResult.PerPage;
				int itemCount = 0;
				int totalItemCount = 0;

				foreach (DirectoryInfo subDirInfo in dirInfos)
				{
					int galleryId = ExtractGalleryIdFromFileName(subDirInfo.Name);

					if (galleryId == -1)
					{
						continue;
					}

					++totalItemCount;

					if (skipCount > 0)
					{
						--skipCount;
						continue;
					}

					if (itemCount == searchResult.PerPage)
					{
						continue;
					}

					++itemCount;

					galleryIds.Add(galleryId);
				}

				int loadCount = 0;

				foreach (int galleryId in galleryIds)
				{
					Metadata metadata = GetGalleryMetadata(galleryId, runArg.PathFormatter, runArg.SearchResultCache);

					++loadCount;

					LibraryLoadProgressArg progressArg = new LibraryLoadProgressArg(runArg, loadCount, galleryIds.Count, metadata);

					backgroundWorker.ReportProgress((int)((loadCount / (float)galleryIds.Count) * 100), progressArg);

					if (metadata == null)
					{
						continue;
					}

					searchResult.Result.Add(metadata);
				}

				searchResult.NumPages = (int)Math.Ceiling(totalItemCount / (double)searchResult.PerPage);
			}

			SearchResult finalSearchResult;

			if (searchResult.Result != null
				&& runArg.SortType != GallerySortType.None)
			{
				SearchResult customSearchResult = new SearchResult();

				customSearchResult.NumPages = searchResult.NumPages;
				customSearchResult.PerPage = searchResult.PerPage;

				// NOTE: don't hide items in the library.
				//IEnumerable<Metadata> customResult = searchResult.Result.GetFilteredSearchResult(runArg.MetadataKeywordLists);
				IEnumerable<Metadata> customResult = searchResult.Result.GetSortedSearchResult(runArg.SortType, runArg.SortOrder);

				customSearchResult.Result = customResult.ToList();

				finalSearchResult = customSearchResult;
			}
			else
			{
				finalSearchResult = searchResult;
			}

			LibraryLoadCompletedArg completedArg = new LibraryLoadCompletedArg(runArg, finalSearchResult);

			e.Result = completedArg;
		}

		private void WebBrowser_LibraryDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			SearchProgressArg searchProgressArg = webBrowser.Tag as SearchProgressArg;

			LibraryModel.SearchProgressArg = searchProgressArg;
		}

		private static int ExtractGalleryIdFromFileName(string fileName)
		{
			int galleryId;

			if (int.TryParse(fileName, out galleryId))
			//if (int.TryParse(Path.GetFileNameWithoutExtension(fileName), out galleryId))
			{
				return galleryId;
			}

			return -1;
		}
		
		private static Metadata GetGalleryMetadata(int galleryId, IPathFormatter pathFormatter, ISearchResultCache searchResultCache)
		{
			string cachedMetadataFilePath;

			if (pathFormatter.IsEnabled)
			{
				cachedMetadataFilePath = pathFormatter.GetMetadata(galleryId);
			}
			else
			{
				cachedMetadataFilePath = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", pathFormatter.GetCacheDirectory(), galleryId, ".json");
			}

			Metadata metadata = searchResultCache.Find(galleryId) ?? JsonUtility.LoadFromFile<Metadata>(cachedMetadataFilePath);

			return metadata;
		}

		private class LibraryLoadRunArg
		{
			public IPathFormatter PathFormatter { get; }
			public IQueryParser QueryParser { get; }
			public IMetadataCache MetadataCache { get; }
			public ISearchResultCache SearchResultCache { get; }
			public MetadataCacheSnapshot MetadataCacheSnapshot { get; }
			public SearchArg SearchArg { get; }
			public int NumItemsPerPage { get; }
			public string LibraryPath { get; }
			public GallerySortType SortType { get; }
			public LibrarySortType GlobalSortType { get; }
			public SortOrder SortOrder { get; }
			public SortOrder GlobalSortOrder { get; }

			public LibraryLoadRunArg(IPathFormatter pathFormatter, IQueryParser queryParser, IMetadataCache metadataCache, ISearchResultCache searchResultCache, MetadataCacheSnapshot metadataCacheSnapshot, SearchArg searchArg, int numItemsPerPage, string libraryPath
				, GallerySortType sortType, LibrarySortType globalSortType
				, SortOrder sortOrder, SortOrder globalSortOrder)
			{
				PathFormatter = pathFormatter;
				QueryParser = queryParser;
				MetadataCache = metadataCache;
				SearchResultCache = searchResultCache;
				MetadataCacheSnapshot = metadataCacheSnapshot;
				SearchArg = searchArg;
				NumItemsPerPage = numItemsPerPage;
				LibraryPath = libraryPath;
				SortType = sortType;
				GlobalSortType = globalSortType;
				SortOrder = sortOrder;
				GlobalSortOrder = globalSortOrder;
			}
		}

		private class LibraryLoadProgressArg
		{
			public LibraryLoadRunArg RunArg { get; }
			public int CurrentLoadCount { get; }
			public int TotalLoadCount { get; }
			public Metadata Metadata { get; }

			public LibraryLoadProgressArg(LibraryLoadRunArg runArg, int currentLoadCount, int totalLoadCount, Metadata metadata)
			{
				RunArg = runArg;
				CurrentLoadCount = currentLoadCount;
				TotalLoadCount = totalLoadCount;
				Metadata = metadata;
			}
		}

		private class LibraryLoadCompletedArg
		{
			public LibraryLoadRunArg RunArg { get; }
			public SearchResult SearchResult { get; }

			public LibraryLoadCompletedArg(LibraryLoadRunArg runArg, SearchResult searchResult)
			{
				RunArg = runArg;
				SearchResult = searchResult;
			}
		}
	}

	public static class LibraryItemCollectionExtensionMethods
	{
		public static IEnumerable<DirectoryInfo> OrderAllByTime(this IEnumerable<DirectoryInfo> dirInfos, LibrarySortType globalSortType, SortOrder globalSortOrder)
		{
			if (globalSortType == LibrarySortType.CreationTime)
			{
				dirInfos = OrderByCreationTime(dirInfos, globalSortOrder);
			}
			else if (globalSortType == LibrarySortType.LastWriteTime)
			{
				dirInfos = OrderByLastWriteTime(dirInfos, globalSortOrder);
			}
			else if (globalSortType == LibrarySortType.LastAccessTime)
			{
				dirInfos = OrderByLastAccessTime(dirInfos, globalSortOrder);
			}

			return dirInfos;
		}

		private static IOrderedEnumerable<DirectoryInfo> OrderByLastAccessTime(IEnumerable<DirectoryInfo> dirInfos, SortOrder globalSortOrder)
		{
			return dirInfos.OrderBy(x => x.LastAccessTime, globalSortOrder);
		}

		private static IOrderedEnumerable<DirectoryInfo> OrderByLastWriteTime(IEnumerable<DirectoryInfo> dirInfos, SortOrder globalSortOrder)
		{
			return dirInfos.OrderBy(x => x.LastWriteTime, globalSortOrder);
		}

		private static IOrderedEnumerable<DirectoryInfo> OrderByCreationTime(IEnumerable<DirectoryInfo> dirInfos, SortOrder globalSortOrder)
		{
			return dirInfos.OrderBy(x => x.CreationTime, globalSortOrder);
		}
	}
}
