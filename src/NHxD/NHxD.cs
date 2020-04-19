using Nhentai;
using NHxD.Plugin.ArchiveWriter;
using NHxD.Plugin.MetadataConverter;
using System;
using System.Collections.Generic;

namespace NHxD
{
	public interface ISearchProgressArg
	{
		ISearchArg SearchArg { get; }
		SearchResult SearchResult { get; }
		int PageIndex { get; }
	}

	public interface ISearchArg
	{
		int TagId { get; }
		string Query { get; }
		int PageIndex { get; }
		SearchTarget Target { get; }
	}

	public enum SearchTarget
	{
		Recent,
		Query,
		Tagged,
		Library
	}

	public interface ISearchResultCacheItem
	{
		List<int> CachedMetadataIds { get; }
		int NumPages { get; }
		int PerPage { get; }
	}

	public interface IMetadataCache
	{
		Dictionary<int, Metadata> Items { get; }

		bool Contains(int galleryId);
		bool TryGet(int galleryId, out Metadata metadata);
	}

	public interface ISearchResultCache
	{
		IMetadataCache MetadataCache { get; }
		//Dictionary<string, SearchResultCacheItem> Items { get; }
		Dictionary<string, SearchResult> Items { get; }

		bool Contains(string uri);
		//void Add(string uri, SearchResultCacheItem cachedSearchResult);
		void Add(string uri, SearchResult cachedSearchResult);
		bool TryGet(string uri, out SearchResult searchResult);
		Metadata Find(int galleryId);
		void CacheRuntimeMetadata(Metadata metadata);
	}

	public interface IPathFormatter
	{
		string ApplicationPath { get; }
		string SourcePath { get; }
		Dictionary<string, string> CustomPaths { get; }
		bool IsEnabled { get; }

		// HACK: some parts of the program currently expects cache resources to be easy to find so I put everything in a cache folder but this is wrong.
		string GetCacheDirectory();
		string GetPluginDirectory();
		string GetMetadataDirectory();
		string GetConvertedMetadataDirectory();
		string GetCoverDirectory();
		string GetPagesDirectory();
		string GetArchiveDirectory();
		string GetSessionDirectory();
		string GetSessionDirectory(string sessionType);
		string GetResourceDirectory();
		string GetTemplateDirectory();

		string GetPlugin(string pluginName);
		string GetMetadata(int galleryId);
		string GetCover(Metadata metadata);
		string GetPages(Metadata metadata);
		string GetPage(Metadata metadata, int pageIndex);
		string GetArchive(Metadata metadata, IArchiveWriter archiveWriter);
		string GetConvertedMetadata(Metadata metadata, IMetadataConverter metadataConverter);

		string GetSession(string sessionQuery);
		string GetDefaultConfiguration(string name);
		string GetConfiguration(string name);
		string GetResource(string name, string fileExt);
		string GetTemplate(string name);
		string GetLog();
		string GetLog(DateTime dateTime);

		string GetPath(string format, Dictionary<string, string> contextMapping, Dictionary<string, string> customMapping);
		string GetPath(string format, Metadata metadata, Dictionary<string, string> contextMapping, Dictionary<string, string> customMapping);
	}

	public interface ISessionManager
	{
		IPathFormatter PathFormatter { get; }
		ISearchResultCache SearchResultCache { get; }
		int RecentSearchLifeSpan { get; set; }
		int QuerySearchLifeSpan { get; set; }
		int TaggedSearchLifeSpan { get; set; }

		string GetSessionQuery(int pageIndex);
		string GetSessionQuery(int tagId, int pageIndex);
		string GetSessionQuery(string query, int pageIndex);
		string GetSessionFileName(int pageIndex);
		string GetSessionFileName(int tagId, int pageIndex);
		string GetSessionFileName(string query, int pageIndex);
		void ForgetSession(int tagId, int pageIndex);
		void ForgetSession(string query, int pageIndex);
		void ForgetSession(int pageIndex);
		void DeleteSession(int tagId, int pageIndex);
		void DeleteSession(string query, int pageIndex);
		void DeleteSession(int pageIndex);
		void DeleteExpiredSessions();
		void DeleteExpiredSessions(int lifetime, string searchPath);
	}

	public interface IQueryParser
	{
		bool ParseQuerySearch(string[] tokens, out string query, out int pageIndex);
		bool ParseTaggedSearch(string[] tokens, out int tagId, out string tagType, out string tagName, out int pageIndex);
		bool ParseRecentSearch(string[] tokens, out int pageIndex);
		bool ParseLibrarySearch(string[] tokens, out int pageIndex);
		bool ParseDetailsSearch(string[] tokens, out int galleryId);
		bool ParseDownloadSearch(string[] tokens, out int galleryId);
	}

	public interface ICacheFileSystem
	{
		void OpenMetadata(int galleryId);
		void SelectMetadata(int galleryId);

		void OpenCover(string metadataJson);
		void SelectCover(string metadataJson);

		void OpenPage(string metadataJson, int pageIndex);
		void SelectPage(string metadataJson, int pageIndex);

		void OpenPagesFolder(int galleryId);
		void SelectPagesFolder(int galleryId);

		void OpenArchive(int galleryId);
		void SelectArchive(int galleryId);

		void OpenFirstCachedPage(int galleryId);
		void SelectFirstCachedPage(int galleryId);

		bool DoesMetadataExists(int galleryId);
		bool DoesCoverExists(int galleryId);
		bool AnyPageExists(int galleryId);
		bool DoesPageExists(int galleryId, int pageIndex);
		bool DoesPagesFolderExists(int galleryId);
		bool DoesComicBookArchiveExists(int galleryId);

		int[] GetCachedPageIndices(int galleryId);
	}
}
