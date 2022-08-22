using Ash.System.Diagnostics;
using Ash.System.Windows.Forms;
using Newtonsoft.Json;
using Nhentai;
using NHxD.Frontend.Winforms.Configuration;
using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	public class SearchHandler
	{
		// keep a copy of the last used search argument. Used when refreshing the page.
		private SearchArg currentSearchArg;

		public LibraryModel LibraryModel { get; }
		public TabControlEx MainViewTabControl { get; }
		public TabPage LibraryBrowserViewTabPage { get; }
		public TabPage GalleryBrowserViewTabPage { get; }
		public Configuration.ConfigCache CacheSettings { get; }
		public ISearchResultCache SearchResultCache { get; }
		public ILogger Logger { get; }
		public BrowsingModel BrowsingModel { get; }
		public IPathFormatter PathFormatter { get; }
		public HttpClient HttpClient { get; }
		public IQueryParser QueryParser { get; }
		public GalleryModel GalleryModel { get; }
		public DetailsModel DetailsModel { get; }
		public GalleryDownloader GalleryDownloader { get; }
		public ISessionManager SessionManager { get; }
		public ConfigNetwork NetworkSettings { get; }

		public SearchHandler(
			LibraryModel libraryModel
			, TabControlEx mainViewTabControl
			, TabPage libraryBrowserViewTabPage
			, TabPage galleryBrowserViewTabPage
			, Configuration.ConfigCache cacheSettings
			, ISearchResultCache searchResultCache
			, ILogger logger
			, BrowsingModel browsingModel
			, IPathFormatter pathFormatter
			, HttpClient httpClient
			, IQueryParser queryParser
			, GalleryModel galleryModel
			, DetailsModel detailsModel
			, GalleryDownloader galleryDownloader
			, ISessionManager sessionManager
			, ConfigNetwork networkSettings)
		{
			LibraryModel = libraryModel;
			MainViewTabControl = mainViewTabControl;
			LibraryBrowserViewTabPage = libraryBrowserViewTabPage;
			GalleryBrowserViewTabPage = galleryBrowserViewTabPage;
			CacheSettings = cacheSettings;
			SearchResultCache = searchResultCache;
			Logger = logger;
			BrowsingModel = browsingModel;
			PathFormatter = pathFormatter;
			HttpClient = httpClient;
			QueryParser = queryParser;
			GalleryModel = galleryModel;
			DetailsModel = detailsModel;
			GalleryDownloader = galleryDownloader;
			SessionManager = sessionManager;
			NetworkSettings = networkSettings;
		}


		public void BrowseLibrary()
		{
			BrowseLibrary(LibraryModel.PageIndex);
		}

		public void BrowseLibrary(int pageIndex)
		{
			BrowseLibrary(pageIndex, true);
		}

		public void BrowseLibrary(int pageIndex, bool activateTab)
		{
			if (activateTab)
			{
				MainViewTabControl.SelectTab(LibraryBrowserViewTabPage);
			}

			// HACK: sharing the library and cache views causes some issues...
			if (LibraryModel.TagId != -1 || !string.IsNullOrEmpty(LibraryModel.Query))
			{
				LibraryModel.TagId = -1;
				LibraryModel.Query = "";
				LibraryModel.PageIndex = 1;
				return;
			}

			LibraryModel.TagId = -1;
			LibraryModel.Query = "";
			LibraryModel.PageIndex = pageIndex;
		}

		public void BrowseTaggedCache(int tagId)
		{
			BrowseTaggedCache(tagId, LibraryModel.PageIndex, true);
		}

		public void BrowseTaggedCache(int tagId, int pageIndex)
		{
			BrowseTaggedCache(tagId, pageIndex, true);
		}

		public void BrowseTaggedCache(int tagId, int pageIndex, bool activateTab)
		{
			if (activateTab)
			{
				MainViewTabControl.SelectTab(LibraryBrowserViewTabPage);
			}

			LibraryModel.TagId = tagId;
			LibraryModel.Query = "";// "tagged:" + tagId + ":" + pageIndex;
			LibraryModel.PageIndex = pageIndex;
		}

		public void BrowseCache(string query)
		{
			BrowseCache(query, LibraryModel.PageIndex);
		}

		public void BrowseCache(string query, int pageIndex)
		{
			BrowseCache(query, pageIndex, true);
		}

		public void BrowseCache(string query, int pageIndex, bool activateTab)
		{
			if (activateTab)
			{
				MainViewTabControl.SelectTab(LibraryBrowserViewTabPage);
			}

			LibraryModel.TagId = -1;
			LibraryModel.Query = query;
			LibraryModel.PageIndex = pageIndex;
		}

		public SearchResult RecentSearch(int pageIndex)
		{
			if (pageIndex < 0)
			{
				return null;
			}

			return Search(CacheSettings.Session.CheckAtRuntime, CacheSettings.Session.RecentLifeSpan
				, string.Format(CultureInfo.InvariantCulture, "all?page={0}", pageIndex)
				, string.Format(CultureInfo.InvariantCulture, "recent:{0}", pageIndex));
		}

		public SearchResult Search(string query, int pageIndex)
		{
			if (string.IsNullOrEmpty(query) || pageIndex < 0)
			{
				return null;
			}

			return Search(CacheSettings.Session.CheckAtRuntime, CacheSettings.Session.SearchLifeSpan
				, string.Format(CultureInfo.InvariantCulture, "search?query={0}&page={1}", Uri.EscapeDataString(query), pageIndex)
				, string.Format(CultureInfo.InvariantCulture, "query:{0}:{1}", query, pageIndex));
		}

		public SearchResult TaggedSearch(int tagId, int pageIndex)
		{
			if (tagId < 0 || pageIndex < 0)
			{
				return null;
			}

			return Search(CacheSettings.Session.CheckAtRuntime, CacheSettings.Session.TaggedLifeSpan
				, string.Format(CultureInfo.InvariantCulture, "tagged?tag_id={0}&page={1}", tagId, pageIndex)
				, string.Format(CultureInfo.InvariantCulture, "tagged:{0}:{1}", tagId, pageIndex));
		}

		private SearchResult Search(bool checkSession, int lifetime, string uri, string searchQuery)
		{
			SearchResult searchResult;
			string url = "https://nhentai.net/api/galleries/" + uri;
			string sessionName = uri.Replace("?", "/");

			if (SearchResultCache.Items.TryGetValue(sessionName, out searchResult))
			{
				BrowsingModel.AddSearchHistory(searchQuery);

				Logger.InfoLineFormat("Loaded SearchResult from memory cache: {0}", searchQuery);

				return searchResult;
			}

			string cachedSearchResultsFilePath = PathFormatter.GetSession(sessionName);

			if (File.Exists(cachedSearchResultsFilePath))
			{
				if (checkSession
					&& lifetime > 0
					&& !NetworkSettings.Offline)
				{
					FileInfo cachedSessionFileInfo = new FileInfo(cachedSearchResultsFilePath);
					DateTime now = DateTime.Now;

					if ((now - cachedSessionFileInfo.CreationTime).TotalMilliseconds > lifetime)
					{
						//SessionManager.DeleteSession(cachedSearchResultsFilePath);
						File.Delete(cachedSearchResultsFilePath);
					}
				}

				searchResult = JsonUtility.LoadFromFile<SearchResult>(cachedSearchResultsFilePath);

				if (searchResult != null)
				{
					if (lifetime != 0)
					{
						SearchResultCache.Items.Add(sessionName, searchResult);
					}

					BrowsingModel.AddSearchHistory(searchQuery);

					Logger.InfoLineFormat("Loaded SearchResult from file cache: {0}", searchQuery);

					return searchResult;
				}
			}
			else
			{
				if (NetworkSettings.Offline)
				{
					throw new InvalidHttpResponseException("This page does not exist in the cache and is thus unavailable while in offline mode.");
				}
			}

			Logger.InfoLineFormat("Downloading SearchResult: {0}", searchQuery);

			try
			{
				using (HttpResponseMessage response = Task.Run(() => HttpClient?.GetAsync(url, HttpCompletionOption.ResponseHeadersRead)).GetAwaiter().GetResult())
				{
					if (!response.IsSuccessStatusCode)
					{
						Logger.ErrorLineFormat("{0} ({1})", response.ReasonPhrase, response.StatusCode);
						response.EnsureSuccessStatusCode();
						return null;
					}

					string jsonText = Task.Run(() => response.Content.ReadAsStringAsync()).GetAwaiter().GetResult();

					searchResult = JsonConvert.DeserializeObject<SearchResult>(jsonText);

					if (lifetime != 0)
					{
						if (searchResult != null)
						{
							if (searchResult.Error)
							{
								Logger.WarnLineFormat("The server returned an error while downloading search result.");
								throw new InvalidHttpResponseException("The server returned an error while downloading search result.");
							}

							Logger.LogLineFormat("File caching SearchResult: {0}", searchQuery);

							SearchResultCache.Items.Add(sessionName, searchResult);

							Directory.CreateDirectory(Path.GetDirectoryName(cachedSearchResultsFilePath));
							File.WriteAllText(cachedSearchResultsFilePath, jsonText);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.ErrorLine(ex.ToString());
				throw;
			}

			BrowsingModel.AddSearchHistory(searchQuery);

			return searchResult;
		}

		public void ParseAndExecuteSearchText(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return;
			}

			string[] parts = text.Split(new char[] { ':' });

			try
			{
				if (parts.Length == 0)
				{
					throw new ParserException("Please enter a valid search.");
				}

				if (parts[0].Equals("search", StringComparison.OrdinalIgnoreCase)
					|| parts[0].Equals("query", StringComparison.OrdinalIgnoreCase))
				{
					string query = parts[1];
					int pageIndex = 1;

					if (!QueryParser.ParseQuerySearch(parts, out query, out pageIndex))
					{
						throw new ParserException("Invalid query search syntax.");
					}

					RunSearch(query, pageIndex);
				}
				else if (parts[0].Equals("tag", StringComparison.OrdinalIgnoreCase)
					|| parts[0].Equals("tagged", StringComparison.OrdinalIgnoreCase))
				{
					int tagId;
					string tagType;
					string tagName;
					int pageIndex;

					if (!QueryParser.ParseTaggedSearch(parts, out tagId, out tagType, out tagName, out pageIndex))
					{
						throw new ParserException("Invalid tagged search syntax.");
					}

					RunTaggedSearch(tagId, pageIndex);
				}
				else if (parts[0].Equals("recent", StringComparison.OrdinalIgnoreCase)
					|| parts[0].Equals("all", StringComparison.OrdinalIgnoreCase))
				{
					int pageIndex;

					if (!QueryParser.ParseRecentSearch(parts, out pageIndex))
					{
						throw new ParserException("Invalid recent search syntax.");
					}

					RunRecentSearch(pageIndex);
				}
				else if (parts[0].Equals("library", StringComparison.OrdinalIgnoreCase))
				{
					int pageIndex;

					if (!QueryParser.ParseLibrarySearch(parts, out pageIndex))
					{
						throw new ParserException("Invalid library search syntax.");
					}

					BrowseLibrary(pageIndex);
				}
				else if (parts[0].Equals("details", StringComparison.OrdinalIgnoreCase))
				{
					int galleryId;

					if (!QueryParser.ParseDetailsSearch(parts, out galleryId))
					{
						throw new ParserException("Invalid details search syntax.");
					}

					ShowDetails(galleryId);
				}
				else if (parts[0].Equals("download", StringComparison.OrdinalIgnoreCase))
				{
					int galleryId;

					if (!QueryParser.ParseDownloadSearch(parts, out galleryId))
					{
						throw new ParserException("Invalid download search syntax.");
					}

					ShowDownload(galleryId);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Syntax Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}


		public void RunTaggedSearch(int tagId, int pageIndex)
		{
			GalleryModel.AddSearch(tagId, pageIndex);

			RunSearch(new SearchArg(tagId, pageIndex));
		}

		public void RunSearch(string query, int pageIndex)
		{
			GalleryModel.AddSearch(query, pageIndex);

			RunSearch(new SearchArg(query, pageIndex));
		}

		public void RunRecentSearch(int pageIndex)
		{
			GalleryModel.AddSearch(pageIndex);

			RunSearch(new SearchArg(pageIndex));
		}

		public void RunSearch(SearchArg searchArg)
		{
			RunSearch(searchArg, true);
		}

		public void RunSearch(SearchArg searchArg, bool activateGalleryTab)
		{
			if (activateGalleryTab)
			{
				MainViewTabControl.SelectTab(GalleryBrowserViewTabPage);
			}

			currentSearchArg = searchArg;

			GalleryModel.SearchArg = searchArg;
		}

		public void ReloadGalleryBrowser()
		{
			if (currentSearchArg == null)
			{
				return;
			}

			RunSearch(currentSearchArg);
		}


		public void ShowDetails(int galleryId)
		{
			ShowDetailsOrDownload(galleryId, (metadata) =>
			{
				DetailsModel.Target = DetailsTarget.Details;
				DetailsModel.Metadata = metadata;
			});
		}

		public void ShowDownload(int galleryId)
		{
			ShowDetailsOrDownload(galleryId, (metadata) =>
			{
				DetailsModel.Target = DetailsTarget.Download;
				DetailsModel.Metadata = metadata;
			});
		}

		// NOTE: at the moment both details and download share the same webbrowser.
		private bool ShowDetailsOrDownload(int galleryId, Action<Metadata> action)
		{
			DetailsModel.AddSearch(galleryId);

			string cachedMetadataFilePath;

			if (PathFormatter.IsEnabled)
			{
				cachedMetadataFilePath = PathFormatter.GetMetadata(galleryId);
			}
			else
			{
				cachedMetadataFilePath = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", PathFormatter.GetCacheDirectory(), galleryId, ".json");
			}

			Metadata metadata = SearchResultCache.Find(galleryId) ?? JsonUtility.LoadFromFile<Metadata>(cachedMetadataFilePath);

			if (metadata == null)
			{
				// let it download in the background, DetailsBrowserView will respond to its completion event.
				GalleryDownloader.Download(galleryId);

				return false;
			}

			if (metadata == null)
			{
				MessageBox.Show("can't find metadata");

				return false;
			}

			if (action != null)
			{
				action.Invoke(metadata);
			}

			return true;
		}
	}

	[Serializable]
	public class ParserException : Exception, ISerializable
	{
		public ParserException() { }
		public ParserException(string message) : base(message) { }
		public ParserException(string message, Exception innerException) : base(message, innerException) { }
		protected ParserException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class InvalidHttpResponseException : Exception, ISerializable
	{
		public InvalidHttpResponseException() { }
		public InvalidHttpResponseException(string message) : base(message) { }
		public InvalidHttpResponseException(string message, Exception innerException) : base(message, innerException) { }
		protected InvalidHttpResponseException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
