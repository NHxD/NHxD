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
	using Formatting = Newtonsoft.Json.Formatting;

	public partial class GalleryBrowserView : UserControl
	{
		private readonly WebBrowserEx webBrowser;
		private readonly BackgroundWorker listBackgroundWorker;

		public WebBrowser WebBrowser => webBrowser;

		public GalleryBrowserFilter GalleryBrowserFilter { get; }
		public GalleryModel GalleryModel { get; }
		public IPathFormatter PathFormatter { get; }
		public DocumentTemplate<ISearchProgressArg> SearchCovergridTemplate { get; }
		public DocumentTemplate<ISearchArg> SearchPreloadTemplate { get; }
		public PageDownloader PageDownloader { get; }
		public CoverDownloader CoverDownloader { get; }
		public MetadataKeywordLists MetadataKeywordLists { get; }
		public TagsModel TagsModel { get; }
		public SearchHandler SearchHandler { get; }
		public Configuration.ConfigGalleryBrowserView GalleryBrowserSettings { get; }

		public GalleryBrowserView()
		{
			InitializeComponent();
		}

		public GalleryBrowserView(GalleryBrowserFilter galleryBrowserFilter, GalleryModel galleryModel, DocumentTemplate<ISearchProgressArg> searchCovergridTemplate, DocumentTemplate<ISearchArg> searchPreloadTemplate, IPathFormatter pathFormatter
			, Configuration.ConfigGalleryBrowserView galleryBrowserSettings
			, PageDownloader pageDownloader
			, CoverDownloader coverDownloader
			, MetadataKeywordLists metadataKeywordLists
			, TagsModel tagsModel
			, SearchHandler searchHandler)
		{
			InitializeComponent();

			GalleryBrowserFilter = galleryBrowserFilter;
			GalleryModel = galleryModel;
			PathFormatter = pathFormatter;
			SearchCovergridTemplate = searchCovergridTemplate;
			SearchPreloadTemplate = searchPreloadTemplate;
			GalleryBrowserSettings = galleryBrowserSettings;
			PageDownloader = pageDownloader;
			CoverDownloader = coverDownloader;
			MetadataKeywordLists = metadataKeywordLists;
			TagsModel = tagsModel;
			SearchHandler = searchHandler;

			webBrowser = new WebBrowserEx();
			listBackgroundWorker = new BackgroundWorker();

			SuspendLayout();

			webBrowser.AllowWebBrowserDrop = false;
			webBrowser.Dock = DockStyle.Fill;
			webBrowser.Name = "galleryWebBrowser";
			webBrowser.BeforeDocumentCompleted += WebBrowser_BeforeDocumentCompleted;
			webBrowser.DocumentCompleted += WebBrowser_DocumentCompleted;

			listBackgroundWorker.WorkerReportsProgress = true;
			listBackgroundWorker.WorkerSupportsCancellation = true;

			listBackgroundWorker.DoWork += ListBackgroundWorker_DoWork;
			listBackgroundWorker.ProgressChanged += ListBackgroundWorker_ProgressChanged;
			listBackgroundWorker.RunWorkerCompleted += ListBackgroundWorker_RunWorkerCompleted;

			PageDownloader.PageDownloadReportProgress += PageDownloader_PageDownloadReportProgress;
			PageDownloader.PagesDownloadStarted += PageDownloader_PagesDownloadStarted;
			PageDownloader.PagesDownloadCancelled += PageDownloader_PagesDownloadCancelled;
			PageDownloader.PagesDownloadCompleted += PageDownloader_PagesDownloadCompleted;

			CoverDownloader.CoverDownloadReportProgress += CoverDownloader_CoverDownloadReportProgress;
			CoverDownloader.CoversDownloadStarted += CoverDownloader_CoversDownloadStarted;
			CoverDownloader.CoversDownloadCancelled += CoverDownloader_CoversDownloadCancelled;
			CoverDownloader.CoversDownloadCompleted += CoverDownloader_CoversDownloadCompleted;

			MetadataKeywordLists.WhitelistChanged += Form_WhiteListChanged;
			MetadataKeywordLists.BlacklistChanged += Form_BlackListChanged;
			MetadataKeywordLists.IgnorelistChanged += Form_IgnoreListChanged;
			MetadataKeywordLists.HidelistChanged += Form_HideListChanged;

			GalleryModel.SearchArgChanged += GalleryModel_SearchArgChanged;
			GalleryModel.SearchProgressArgChanged += GalleryModel_SearchProgressArgChanged;

			galleryBrowserFilter.TextChanged += GalleryBrowserFilter_TextChanged;

			Controls.Add(webBrowser);

			ResumeLayout(false);
		}

		private void GalleryBrowserFilter_TextChanged(object sender, EventArgs e)
		{
			ApplyFilter(GalleryBrowserFilter.Text);
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

		private void GalleryModel_SearchProgressArgChanged(object sender, EventArgs e)
		{
			WebBrowser.Tag = GalleryModel.SearchProgressArg;
			WebBrowser.DocumentText = SearchCovergridTemplate.GetFormattedText(GalleryModel.SearchProgressArg);
		}

		private void GalleryModel_SearchArgChanged(object sender, EventArgs e)
		{
		//	WebBrowser.DocumentCompleted += GalleryWebBrowser_PreloadDocumentCompleted;
			WebBrowser.Tag = GalleryModel.SearchArg;
			WebBrowser.DocumentText = SearchPreloadTemplate.GetFormattedText(GalleryModel.SearchArg);
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
		
		private void CoverDownloader_CoversDownloadStarted(object sender, CoversDownloadStartedEventArgs e)
		{
			if (string.IsNullOrEmpty(WebBrowser.DocumentText))
			{
				return;
			}

			WebBrowser.Document?.InvokeScript("__onCoversDownloadStarted", e.ToObjectArray());
		}

		private void CoverDownloader_CoversDownloadCompleted(object sender, CoverDownloadCompletedEventArgs e)
		{
			if (string.IsNullOrEmpty(WebBrowser.DocumentText))
			{
				return;
			}

			WebBrowser.Document?.InvokeScript("__onCoversDownloadCompleted", e.ToObjectArray());
		}

		private void CoverDownloader_CoversDownloadCancelled(object sender, CoverDownloadCompletedEventArgs e)
		{
			if (string.IsNullOrEmpty(WebBrowser.DocumentText))
			{
				return;
			}

			WebBrowser.Document?.InvokeScript("__onCoversDownloadCancelled", e.ToObjectArray());
		}

		private void CoverDownloader_CoverDownloadReportProgress(object sender, CoverDownloadReportProgressEventArgs e)
		{
			if (string.IsNullOrEmpty(WebBrowser.DocumentText))
			{
				return;
			}

			WebBrowser.Document?.InvokeScript("__onCoverDownloadReportProgress", e.ToObjectArray());
		}

		private void WebBrowser_BeforeDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			webBrowser.ZoomFactor = GalleryBrowserSettings.ZoomRatio;
		}
		

		


		public void ApplyFilter(string filter)
		{
			if (webBrowser.Document != null)
			{
				webBrowser.Document.InvokeScript("__applyFilter", new object[] { filter ?? "" });
			}
		}

		public void Reload()
		{
			SearchHandler.ReloadGalleryBrowser();
		}

		private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			SearchArg searchArg = webBrowser.Tag as SearchArg;

			if (searchArg != null)
			{
				GalleryWebBrowser_PreloadDocumentCompleted(sender, e);
			}
			else
			{
				GalleryWebBrowser_SearchDocumentCompleted(sender, e);
			}
		}

		private void GalleryWebBrowser_SearchDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			SearchProgressArg searchProgressArg = webBrowser.Tag as SearchProgressArg;

			CoverDownloader.Download(searchProgressArg.SearchResult, CoverDownloaderFilters.All);
		}

		private void GalleryWebBrowser_PreloadDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			SearchArg searchArg = webBrowser.Tag as SearchArg;

			if (searchArg == null)
			{
				return;
			}

			if (listBackgroundWorker.IsBusy)
			{
				return;
			}

			SearchDoArg searchDoArg = new SearchDoArg(searchArg, TagsModel, SearchHandler, GalleryBrowserSettings.SortType, GalleryBrowserSettings.SortOrder, MetadataKeywordLists);

			listBackgroundWorker.RunWorkerAsync(searchDoArg);
		}

		private static void ListBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			BackgroundWorker backgroundWorker = sender as BackgroundWorker;
			SearchDoArg searchDoArg = e.Argument as SearchDoArg;

			if (searchDoArg == null)
			{
				return;
			}

			SearchArg searchArg = searchDoArg.SearchArg;
			SearchProgressArg searchProgressArg = null;
			int currentPage = searchArg.PageIndex;
			SearchResult searchResult = null;
			/*
			bool loadAll = false;   // TODO: read this from settings.Download.LoadAll

			do
			{
			*/
			try
			{
				if (searchArg.Target == SearchTarget.Query)
				{
					searchResult = searchDoArg.SearchHandler.Search(searchArg.Query, currentPage);
				}
				else if (searchArg.Target == SearchTarget.Tagged)
				{
					searchResult = searchDoArg.SearchHandler.TaggedSearch(searchArg.TagId, currentPage);
				}
				else if (searchArg.Target == SearchTarget.Recent)
				{
					searchResult = searchDoArg.SearchHandler.RecentSearch(currentPage);
				}

				if (searchResult == null)
				{
					e.Cancel = true;
					return;
				}

				searchDoArg.TagsModel.CollectTags(searchResult);

				searchProgressArg = new SearchProgressArg(searchArg, searchResult, currentPage, searchDoArg.MetadataKeywordLists, searchDoArg.SortType, searchDoArg.SortOrder);

				backgroundWorker.ReportProgress(searchResult.NumPages > 0 ? (int)(currentPage / (float)searchResult.NumPages * 100) : 100, searchProgressArg);
			}
			catch (Exception ex)
			{
				SearchErrorArg errorArg = new SearchErrorArg(searchArg, currentPage, (ex.InnerException != null) ? string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", ex.Message, Environment.NewLine, ex.InnerException.Message) : ex.Message);

				backgroundWorker.ReportProgress(0, errorArg);
			}
			/*
			} while (loadAll
				&& searchResult != null
				&& ++currentPage < searchResult.NumPages);
			*/

			if (searchResult != null
				&& searchResult.Result != null)
			{
				/*if (searchDoArg.SortType != GallerySortType.None)
				{
					searchResult = OrderByMetadata(searchResult, searchDoArg.SortType, searchDoArg.SortOrder);
				}*/

				e.Result = new SearchResultArg(searchArg, searchResult);
			}
		}

		private void ListBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			SearchErrorArg errorArg = e.UserState as SearchErrorArg;

			if (errorArg != null)
			{
				if (!string.IsNullOrEmpty(WebBrowser.DocumentText))
				{
					WebBrowser.Document?.InvokeScript("__onSearchError", errorArg.ToObjectArray());
				}

				return;
			}

			SearchProgressArg searchProgressArg = e.UserState as SearchProgressArg;

			if (searchProgressArg != null)
			{
				SearchResult searchResult = searchProgressArg.SearchResult;

				if (searchResult == null)
				{
					MessageBox.Show("Couldn't get search results for page " + (searchProgressArg.PageIndex + 1) + "!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}

				// TODO: should this be done in DoWork instead?

				for (int i = 0; i < searchResult.Result.Count; ++i)
				{
					Metadata metadata = searchResult.Result[i];
					string cachedMetadataPath;

					if (PathFormatter.IsEnabled)
					{
						cachedMetadataPath = PathFormatter.GetMetadata(metadata.Id);
					}
					else
					{
						cachedMetadataPath = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", PathFormatter.GetCacheDirectory(), metadata.Id.ToString(CultureInfo.InvariantCulture), ".json");
					}

					if (!File.Exists(cachedMetadataPath))
					{
						try
						{
							Directory.CreateDirectory(Path.GetDirectoryName(cachedMetadataPath));
							File.WriteAllText(cachedMetadataPath, JsonConvert.SerializeObject(metadata, Formatting.Indented));
						}
						catch (Exception ex)
						{
							MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
						}
					}

					// TODO: the progress should be multiple searchresult, not individual metadata in the first searchresult.

					if (!string.IsNullOrEmpty(WebBrowser.DocumentText))
					{
						// TODO: invoke an event instead of directly invoking a script function.

						WebBrowser.Document?.InvokeScript("__onSearchResultLoaded", new object[]
						{
							searchProgressArg.SearchArg.PageIndex,
							searchProgressArg.SearchArg.TagId,
							searchProgressArg.SearchArg.Query ?? "",
							searchProgressArg.SearchArg.Target.ToString().ToLowerInvariant(),
							e.ProgressPercentage,
							i + 1,
							searchResult.Result.Count,
							JsonConvert.SerializeObject(metadata)
						});
					}
				}

				if (searchProgressArg.PageIndex == searchProgressArg.SearchArg.PageIndex)
				{
					if (searchResult.Result != null
						&& searchProgressArg.SortType != GallerySortType.None)
					{
						SearchResult customSearchResult = new SearchResult();
						
						customSearchResult.NumPages = searchProgressArg.SearchResult.NumPages;
						customSearchResult.PerPage = searchProgressArg.SearchResult.PerPage;

						/*
						IEnumerable<Metadata> customResult;
						
						customResult = searchProgressArg.SearchResult.Result.GetFilteredSearchResult(searchProgressArg.MetadataKeywordLists);
						customResult = customResult.GetSortedSearchResult(searchProgressArg.SortType, searchProgressArg.SortOrder);
						*/
						IEnumerable<Metadata> customResult = searchProgressArg.SearchResult.Result.GetSortedSearchResult(searchProgressArg.SortType, searchProgressArg.SortOrder);

						customSearchResult.Result = customResult.ToList();

						SearchProgressArg customSearchProgressArg = new SearchProgressArg(searchProgressArg.SearchArg, customSearchResult, searchProgressArg.PageIndex, searchProgressArg.MetadataKeywordLists, searchProgressArg.SortType, searchProgressArg.SortOrder);

						GalleryModel.SearchProgressArg = customSearchProgressArg;
					}
					else
					{
						GalleryModel.SearchProgressArg = searchProgressArg;
					}
				}

				//
				// TODO notify javascript to add one page of items
				//
				//galleryBrowserView.WebBrowser.Document.InvokeScript("__onProgressChanged", new object[] { searchProgressArg.PageIndex, JsonConvert.SerializeObject(searchProgressArg.SearchResult) });
			}
		}

		private void ListBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				MessageBox.Show(e.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			if (!e.Cancelled)
			{

			}
		}

		private class SearchErrorArg
		{
			public SearchArg SearchArg { get; }
			public int PageIndex { get; }
			public string Error { get; }

			public SearchErrorArg(SearchArg searchArg, int pageIndex, string error)
			{
				SearchArg = searchArg;
				PageIndex = pageIndex;
				Error = error;
			}

			public object[] ToObjectArray()
			{
				return new object[]
				{
					PageIndex,	//SearchArg?.PageIndex,
					SearchArg?.TagId,
					SearchArg?.Query ?? "",
					SearchArg?.Target.ToString().ToLowerInvariant(),
					Error,
				};
			}
		}
	}

	public enum GallerySortType
	{
		None,
		Title,
		Language,
		Artist,
		Group,
		Tag,
		Parody,
		Character,
		Category,
		Scanlator,
		UploadDate,
		NumPages,
		NumFavorites,
		Id
		//Version,
		//Comicket,
		//Censhorship
	}

	public class SearchArg : ISearchArg
	{
		public int TagId { get; } = -1;
		public string Query { get; }
		public int PageIndex { get; }
		public SearchTarget Target { get; }

		public SearchArg(int tagId, int pageIndex)
		{
			TagId = tagId;
			PageIndex = pageIndex;
			Target = SearchTarget.Tagged;
		}

		public SearchArg(string query, int pageIndex)
		{
			Query = query;
			PageIndex = pageIndex;
			Target = SearchTarget.Query;
		}

		public SearchArg(int pageIndex)
		{
			PageIndex = pageIndex;
			Target = SearchTarget.Recent;
		}

		public SearchArg(int pageIndex, bool isLibrary)
		{
			PageIndex = pageIndex;
			Target = isLibrary ? SearchTarget.Library : SearchTarget.Recent;
		}
	}

	public class SearchDoArg
	{
		public SearchArg SearchArg { get; }
		public TagsModel TagsModel { get; }
		public SearchHandler SearchHandler { get; }
		public GallerySortType SortType { get; }
		public SortOrder SortOrder { get; }
		public MetadataKeywordLists MetadataKeywordLists { get; }

		public SearchDoArg(SearchArg searchArg
		, TagsModel tagsModel
		, SearchHandler searchHandler
		, GallerySortType sortType
		, SortOrder sortOrder
		, MetadataKeywordLists metadataKeywordLists)
		{
			SearchArg = searchArg;
			TagsModel = tagsModel;
			SearchHandler = searchHandler;
			SortType = sortType;
			SortOrder = sortOrder;
			MetadataKeywordLists = metadataKeywordLists;
		}
	}

	public class SearchProgressArg : ISearchProgressArg
	{
		public ISearchArg SearchArg { get; }
		public SearchResult SearchResult { get; }
		public int PageIndex { get; }
		public MetadataKeywordLists MetadataKeywordLists { get; }
		public GallerySortType SortType { get; }
		public SortOrder SortOrder { get; }

		public SearchProgressArg(ISearchArg searchArg, SearchResult searchResult, int pageIndex, MetadataKeywordLists metadataKeywordLists, GallerySortType sortType, SortOrder sortOrder)
		{
			SearchArg = searchArg;
			SearchResult = searchResult;
			PageIndex = pageIndex;
			MetadataKeywordLists = metadataKeywordLists;
			SortType = sortType;
			SortOrder = sortOrder;
		}
	}

	public class SearchResultArg
	{
		public SearchArg SearchArg { get; }
		public SearchResult SearchResult { get; }

		public SearchResultArg(SearchArg searchArg, SearchResult searchResult)
		{
			SearchArg = searchArg;
			SearchResult = searchResult;
		}
	}

	public static class LibraryMetadataCollectionExtensionMethods
	{
		public static IEnumerable<Metadata> GetFilteredSearchResult(this IEnumerable<Metadata> result, MetadataKeywordLists metadataKeywordLists)
		{
			ICollection<Metadata> filteredResult = new List<Metadata>(result.Count());

			foreach (Metadata metadata in result)
			
			{
				if (!metadataKeywordLists.Hidelist.IsInMetadata(metadata))
				{
					filteredResult.Add(metadata);
				}
			}
			/*
			List<int> indices = new List<int>();

			for (int i = 0, len = result.Count(); i < len; ++i)
			{
				if (metadataKeywordLists.Hidelist.IsInMetadata(searchResult.Result[i]))
				{
					indices.Add(i);
				}
			}

			for (int i = indices.Count; i > 0; --i)
			{
				searchResult.Result.RemoveAt(indices[i - 1]);
			}
			*/
			return filteredResult;
		}

		public static IEnumerable<Metadata> GetSortedSearchResult(this IEnumerable<Metadata> Result, GallerySortType sortType, SortOrder sortOrder)
		{
			IEnumerable<Metadata> orderedResult = new List<Metadata>(Result.Count());

			if (sortType == GallerySortType.Title)
			{
				orderedResult = OrderByTitle(Result, sortOrder);
			}
			else if (sortType == GallerySortType.Language)
			{
				orderedResult = OrderByLanguage(Result, sortOrder);
			}
			else if (sortType == GallerySortType.Artist)
			{
				orderedResult = OrderByArtist(Result, sortOrder);
			}
			else if (sortType == GallerySortType.Group)
			{
				orderedResult = OrderByGroup(Result, sortOrder);
			}
			else if (sortType == GallerySortType.Tag)
			{
				orderedResult = OrderByTag(Result, sortOrder);
			}
			else if (sortType == GallerySortType.Parody)
			{
				orderedResult = OrderByParody(Result, sortOrder);
			}
			else if (sortType == GallerySortType.Character)
			{
				orderedResult = OrderByCharacter(Result, sortOrder);
			}
			else if (sortType == GallerySortType.Category)
			{
				orderedResult = OrderByCategory(Result, sortOrder);
			}
			else if (sortType == GallerySortType.Scanlator)
			{
				orderedResult = OrderByScanlator(Result, sortOrder);
			}
			else if (sortType == GallerySortType.UploadDate)
			{
				orderedResult = OrderByUploadDate(Result, sortOrder);
			}
			else if (sortType == GallerySortType.NumPages)
			{
				orderedResult = OrderByPageCount(Result, sortOrder);
			}
			else if (sortType == GallerySortType.NumFavorites)
			{
				orderedResult = OrderByFavoriteCount(Result, sortOrder);
			}
			else if (sortType == GallerySortType.Id)
			{
				orderedResult = OrderById(Result, sortOrder);
			}

			return orderedResult;
		}

		private static IEnumerable<Metadata> OrderByTitle(IEnumerable<Metadata> Result, SortOrder sortOrder)
		{
			return Result.OrderBy(x => x.Title.Pretty, sortOrder);
		}

		private static IEnumerable<Metadata> OrderByLanguage(IEnumerable<Metadata> Result, SortOrder sortOrder)
		{
			return Result.OrderBy(x => string.Join(",", x.Tags.Where(y => y.Type == TagType.Language).Select(z => z.Name)), sortOrder);
		}

		private static IEnumerable<Metadata> OrderByArtist(IEnumerable<Metadata> Result, SortOrder sortOrder)
		{
			return Result.OrderBy(x => string.Join(",", x.Tags.Where(y => y.Type == TagType.Artist).Select(z => z.Name)), sortOrder);
		}

		private static IEnumerable<Metadata> OrderByGroup(IEnumerable<Metadata> Result, SortOrder sortOrder)
		{
			return Result.OrderBy(x => string.Join(",", x.Tags.Where(y => y.Type == TagType.Group).Select(z => z.Name)), sortOrder);
		}

		private static IEnumerable<Metadata> OrderByTag(IEnumerable<Metadata> Result, SortOrder sortOrder)
		{
			return Result.OrderBy(x => string.Join(",", x.Tags.Where(y => y.Type == TagType.Tag).Select(z => z.Name)), sortOrder);
		}

		private static IEnumerable<Metadata> OrderByParody(IEnumerable<Metadata> Result, SortOrder sortOrder)
		{
			return Result.OrderBy(x => string.Join(",", x.Tags.Where(y => y.Type == TagType.Parody).Select(z => z.Name)), sortOrder);
		}

		private static IEnumerable<Metadata> OrderByCharacter(IEnumerable<Metadata> Result, SortOrder sortOrder)
		{
			return Result.OrderBy(x => string.Join(",", x.Tags.Where(y => y.Type == TagType.Character).Select(z => z.Name)), sortOrder);
		}

		private static IEnumerable<Metadata> OrderByCategory(IEnumerable<Metadata> Result, SortOrder sortOrder)
		{
			return Result.OrderBy(x => string.Join(",", x.Tags.Where(y => y.Type == TagType.Category).Select(z => z.Name)), sortOrder);
		}

		private static IEnumerable<Metadata> OrderByScanlator(IEnumerable<Metadata> Result, SortOrder sortOrder)
		{
			return Result.OrderBy(x => x.Scanlator, sortOrder);
		}

		private static IEnumerable<Metadata> OrderByUploadDate(IEnumerable<Metadata> Result, SortOrder sortOrder)
		{
			return Result.OrderBy(x => x.UploadDate, sortOrder);
		}

		private static IEnumerable<Metadata> OrderByPageCount(IEnumerable<Metadata> Result, SortOrder sortOrder)
		{
			return Result.OrderBy(x => x.NumPages, sortOrder);
		}

		private static IEnumerable<Metadata> OrderByFavoriteCount(IEnumerable<Metadata> Result, SortOrder sortOrder)
		{
			return Result.OrderBy(x => x.NumFavorites, sortOrder);
		}

		private static IEnumerable<Metadata> OrderById(IEnumerable<Metadata> Result, SortOrder sortOrder)
		{
			return Result.OrderBy(x => x.Id, sortOrder);
		}
	}
}
