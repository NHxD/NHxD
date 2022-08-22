using Newtonsoft.Json;
using Nhentai;
using NHxD.Frontend.Winforms.Configuration;
using NHxD.Plugin;
using NHxD.Plugin.ArchiveWriter;
using NHxD.Plugin.MetadataConverter;
using NHxD.Plugin.MetadataProcessor;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;

// TODO: redo the public API to be mutable? (COM-visible classes need to be instantiated by their default constructor)

namespace NHxD.Frontend.Winforms
{
	[ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	public interface ISettingsApi
	{
		bool HasBlockActions();
		bool ShouldBlockBlacklistActions();
		bool ShouldBlockIgnorelistActions();
		bool ShouldBlockHidelistActions();
		bool ShouldBlockUnfilteredActions();
	}

	[ComDefaultInterface(typeof(ISettingsApi)), ComVisible(true), ClassInterface(ClassInterfaceType.None), PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public class SettingsApi : ISettingsApi
	{
		private Configuration.Settings Settings { get; }

		public SettingsApi(Configuration.Settings settings)
		{
			Settings = settings;
		}

		public bool HasBlockActions()
		{
			return Settings.Lists.Tags.BlockActions != TagsFilters.None;
		}
		public bool ShouldBlockBlacklistActions()
		{
			return Settings.Lists.Tags.BlockActions.HasFlag(TagsFilters.Blacklist);
		}
		public bool ShouldBlockIgnorelistActions()
		{
			return Settings.Lists.Tags.BlockActions.HasFlag(TagsFilters.Ignorelist);
		}
		public bool ShouldBlockHidelistActions()
		{
			return Settings.Lists.Tags.BlockActions.HasFlag(TagsFilters.Hidelist);
		}
		public bool ShouldBlockUnfilteredActions()
		{
			return Settings.Lists.Tags.BlockActions.HasFlag(TagsFilters.Other);
		}
	}


	[ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	public interface ILibraryBrowserApi
	{
		float GetZoomRatio();
		float SetZoomRatio(float zoomRatio);
		bool GetItemDownloadProgressVisible();
		bool SetItemDownloadProgressVisible(bool value);
		void ApplyFilter();
	}

	[ComDefaultInterface(typeof(ILibraryBrowserApi)), ComVisible(true), ClassInterface(ClassInterfaceType.None), PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public class LibraryBrowserApi : ILibraryBrowserApi
	{
		private LibraryBrowserToolStrip LibraryBrowserToolStrip { get; }
		private Configuration.ConfigLibrary LibrarySettings { get; }

		public LibraryBrowserApi(LibraryBrowserToolStrip libraryBrowserToolStrip, Configuration.ConfigLibrary librarySettings)
		{
			LibraryBrowserToolStrip = libraryBrowserToolStrip;
			LibrarySettings = librarySettings;
		}

		public float GetZoomRatio()
		{
			return LibrarySettings.Browser.ZoomRatio;
		}

		public float SetZoomRatio(float zoomRatio)
		{
			return LibrarySettings.Browser.ZoomRatio = zoomRatio;
		}

		public bool GetItemDownloadProgressVisible()
		{
			return LibrarySettings.Browser.ItemDownloadProgress.IsVisible;
		}

		public bool SetItemDownloadProgressVisible(bool value)
		{
			LibrarySettings.Browser.ItemDownloadProgress.IsVisible = value;

			return LibrarySettings.Browser.ItemDownloadProgress.IsVisible;
		}

		public void ApplyFilter()
		{
			LibraryBrowserToolStrip.ApplyFilter();
		}
	}

	[ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	public interface IGalleryBrowserApi
	{
		float GetZoomRatio();
		float SetZoomRatio(float zoomRatio);
		bool GetItemDownloadProgressVisible();
		bool SetItemDownloadProgressVisible(bool value);
		void ApplyFilter();
	}

	[ComDefaultInterface(typeof(IGalleryBrowserApi)), ComVisible(true), ClassInterface(ClassInterfaceType.None), PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public class GalleryBrowserApi : IGalleryBrowserApi
	{
		private GalleryBrowserToolStrip GalleryBrowserToolStrip { get; }
		private Configuration.ConfigGallery GallerySettings { get; }

		public GalleryBrowserApi(GalleryBrowserToolStrip galleryBrowserToolStrip, Configuration.ConfigGallery gallerySettings)
		{
			GalleryBrowserToolStrip = galleryBrowserToolStrip;
			GallerySettings = gallerySettings;
		}

		public float GetZoomRatio()
		{
			return GallerySettings.Browser.ZoomRatio;
		}

		public float SetZoomRatio(float value)
		{
			GallerySettings.Browser.ZoomRatio = value;

			return GallerySettings.Browser.ZoomRatio;
		}

		public bool GetItemDownloadProgressVisible()
		{
			return GallerySettings.Browser.ItemDownloadProgress.IsVisible;
		}

		public bool SetItemDownloadProgressVisible(bool value)
		{
			GallerySettings.Browser.ItemDownloadProgress.IsVisible = value;

			return GallerySettings.Browser.ItemDownloadProgress.IsVisible;
		}

		public void ApplyFilter()
		{
			GalleryBrowserToolStrip.ApplyFilter();
		}
	}


	[ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	public interface IDetailsBrowserApi
	{
		float GetZoomRatio();
		float SetZoomRatio(float zoomRatio);
		bool GetReloadPageOnCoverClicked();
		bool SetReloadPageOnCoverClicked(bool value);
	}

	[ComDefaultInterface(typeof(IDetailsBrowserApi)), ComVisible(true), ClassInterface(ClassInterfaceType.None), PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public class DetailsBrowserApi : IDetailsBrowserApi
	{
		private Configuration.ConfigDetails DetailsSettings { get; }

		public DetailsBrowserApi(Configuration.ConfigDetails detailsSettings)
		{
			DetailsSettings = detailsSettings;
		}

		public float GetZoomRatio()
		{
			return DetailsSettings.Browser.ZoomRatio;
		}

		public float SetZoomRatio(float zoomRatio)
		{
			return DetailsSettings.Browser.ZoomRatio = zoomRatio;
		}

		public bool GetReloadPageOnCoverClicked()
		{
			return DetailsSettings.Browser.ReloadDocumentOnCoverClicked;
		}

		public bool SetReloadPageOnCoverClicked(bool value)
		{
			return DetailsSettings.Browser.ReloadDocumentOnCoverClicked = value;
		}
	}


	[ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	public interface IBrowsersApi
	{
		IGalleryBrowserApi Gallery { get; }
		ILibraryBrowserApi Library { get; }
		IDetailsBrowserApi Details { get; }
		// aliases.
		IDetailsBrowserApi Download { get; }
		IGalleryBrowserApi GalleryPreload { get; }
		ILibraryBrowserApi LibraryPreload { get; }
		IDetailsBrowserApi DetailsPreload { get; }
	}

	[ComDefaultInterface(typeof(IBrowsersApi)), ComVisible(true), ClassInterface(ClassInterfaceType.None), PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public class BrowsersApi : IBrowsersApi
	{
		private readonly IGalleryBrowserApi gallery;
		private readonly ILibraryBrowserApi library;
		private readonly IDetailsBrowserApi details;

		public IGalleryBrowserApi Gallery => gallery;
		public ILibraryBrowserApi Library => library;
		public IDetailsBrowserApi Details => details;
		// aliases.
		public IDetailsBrowserApi Download => details;
		public IGalleryBrowserApi GalleryPreload => gallery;
		public ILibraryBrowserApi LibraryPreload => library;
		public IDetailsBrowserApi DetailsPreload => details;

		public BrowsersApi(
			Configuration.ConfigGallery gallerySettings
			, GalleryBrowserToolStrip galleryBrowserToolStrip
			, Configuration.ConfigLibrary librarySettings
			, LibraryBrowserToolStrip libraryBrowserToolStrip
			, Configuration.ConfigDetails detailsSettings
			)
		{
			gallery = new GalleryBrowserApi(galleryBrowserToolStrip, gallerySettings);
			library = new LibraryBrowserApi(libraryBrowserToolStrip, librarySettings);
			details = new DetailsBrowserApi(detailsSettings);
		}
	}


	[ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	public interface IFileApi
	{
		bool Exists(string path);
	}

	[ComDefaultInterface(typeof(IFileApi)), ComVisible(true), ClassInterface(ClassInterfaceType.None), PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public class FileApi : IFileApi
	{
		public FileApi()
		{
		}

		public bool Exists(string path)
		{
			return File.Exists(path);
		}
	}

	[ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	public interface IDirectoryApi
	{
		bool Exists(string path);
	}

	[ComDefaultInterface(typeof(IDirectoryApi)), ComVisible(true), ClassInterface(ClassInterfaceType.None), PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public class DirectoryApi : IDirectoryApi
	{
		public DirectoryApi()
		{
		}

		public bool Exists(string path)
		{
			return Directory.Exists(path);
		}
	}

	[ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	public interface IPathFormatterApi
	{
		bool IsEnabled();

		string GetCacheDirectory();

		string GetPlugin(string pluginName);
		string GetMetadata(int galleryId);
		string GetCover(string metadataJson);
		string GetPages(string metadataJson);
		string GetPage(string metadataJson, int pageIndex);
		string GetArchive(string metadataJson, int archiveWriterIndex);
		string GetConvertedMetadata(string metadataJson, int metadataConverterIndex);
	}

	[ComDefaultInterface(typeof(IPathFormatterApi)), ComVisible(true), ClassInterface(ClassInterfaceType.None), PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public class PathFormatterApi : IPathFormatterApi
	{
		private IPathFormatter PathFormatter { get; }
		private List<IArchiveWriter> ArchiveWriters { get; }
		private List<IMetadataConverter> MetadataConverters { get; }

		public PathFormatterApi(IPathFormatter pathFormatter, List<IArchiveWriter> archiveWriters, List<IMetadataConverter> metadataConverters)
		{
			PathFormatter = pathFormatter;
			ArchiveWriters = archiveWriters;
			MetadataConverters = metadataConverters;
		}

		public bool IsEnabled()
		{
			return PathFormatter.IsEnabled;
		}

		public string GetCacheDirectory()
		{
			return PathFormatter.GetCacheDirectory();
		}

		public string GetPlugin(string pluginName)
		{
			return PathFormatter.GetPlugin(pluginName);
		}

		public string GetMetadata(int galleryId)
		{
			return PathFormatter.GetMetadata(galleryId);
		}

		public string GetCover(string metadataJson)
		{
			try
			{
				Metadata metadata = JsonConvert.DeserializeObject<Metadata>(metadataJson);

				return PathFormatter.GetCover(metadata);
			}
			catch
			{
				return "";
			}
		}

		public string GetPages(string metadataJson)
		{
			try
			{
				Metadata metadata = JsonConvert.DeserializeObject<Metadata>(metadataJson);

				return PathFormatter.GetPages(metadata);
			}
			catch
			{
				return "";
			}
		}

		public string GetPage(string metadataJson, int pageIndex)
		{
			try
			{
				Metadata metadata = JsonConvert.DeserializeObject<Metadata>(metadataJson);

				return PathFormatter.GetPage(metadata, pageIndex);
			}
			catch
			{
				return "";
			}
		}

		public string GetArchive(string metadataJson, int archiveWriterIndex)
		{
			try
			{
				Metadata metadata = JsonConvert.DeserializeObject<Metadata>(metadataJson);

				return PathFormatter.GetArchive(metadata, ArchiveWriters[archiveWriterIndex]);
			}
			catch
			{
				return "";
			}
		}

		public string GetConvertedMetadata(string metadataJson, int metadataConverterIndex)
		{
			try
			{
				Metadata metadata = JsonConvert.DeserializeObject<Metadata>(metadataJson);

				return PathFormatter.GetConvertedMetadata(metadata, MetadataConverters[metadataConverterIndex]);
			}
			catch
			{
				return "";
			}
		}
	}

	[ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	public interface IPathApi
	{
		IPathFormatterApi Formatter { get; }

		string GetDirectoryName(string path);
		string GetExtension(string path);
		string GetFileName(string path);
		string GetFileNameWithoutExtension(string path);
		string GetFullPath(string path);
		string GetPathRoot(string path);
		bool DoesMetadataExists(int galleryId);
		bool DoesCoverExists(int galleryId);
		bool AnyPageExists(int galleryId);
		bool DoesPageExists(int galleryId, int pageIndex);
		string GetCachedPageIndices(int galleryId);
		bool DoesPagesFolderExists(int galleryId);
		bool DoesComicBookArchiveExists(int galleryId);
	}

	[ComDefaultInterface(typeof(IPathApi)), ComVisible(true), ClassInterface(ClassInterfaceType.None), PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public class PathApi : IPathApi
	{
		private readonly IPathFormatterApi formatter;

		public IPathFormatterApi Formatter => formatter;

		private ICacheFileSystem CacheFileSystem { get; }

		public PathApi(ICacheFileSystem cacheFileSystem, IPathFormatter pathFormatter, List<IArchiveWriter> archiveWriters, List<IMetadataConverter> metadataConverters)
		{
			CacheFileSystem = cacheFileSystem;

			formatter = new PathFormatterApi(pathFormatter, archiveWriters, metadataConverters);
		}

		public string GetDirectoryName(string path)
		{
			return Path.GetDirectoryName(path);
		}

		public string GetExtension(string path)
		{
			return Path.GetExtension(path);
		}

		public string GetFileName(string path)
		{
			return Path.GetFileName(path);
		}

		public string GetFileNameWithoutExtension(string path)
		{
			return Path.GetFileNameWithoutExtension(path);
		}

		public string GetFullPath(string path)
		{
			return Path.GetFullPath(path);
		}

		public string GetPathRoot(string path)
		{
			return Path.GetPathRoot(path);
		}

		public bool DoesMetadataExists(int galleryId)
		{
			return CacheFileSystem.DoesMetadataExists(galleryId);
		}

		public bool DoesCoverExists(int galleryId)
		{
			return CacheFileSystem.DoesCoverExists(galleryId);
		}

		public bool AnyPageExists(int galleryId)
		{
			return CacheFileSystem.AnyPageExists(galleryId);
		}

		public bool DoesPageExists(int galleryId, int pageIndex)
		{
			return CacheFileSystem.DoesPageExists(galleryId, pageIndex);
		}

		public string GetCachedPageIndices(int galleryId)
		{
			return string.Join(" ", CacheFileSystem.GetCachedPageIndices(galleryId));
		}

		public bool DoesPagesFolderExists(int galleryId)
		{
			return CacheFileSystem.DoesPagesFolderExists(galleryId);
		}

		public bool DoesComicBookArchiveExists(int galleryId)
		{
			return CacheFileSystem.DoesComicBookArchiveExists(galleryId);
		}
	}

	[ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	public interface ICacheApi
	{
		void OpenMetadata(int galleryId);
		void SelectMetadata(int galleryId);
		void OpenCover(string metadataJson);
		void SelectCover(string metadataJson);
		void OpenPage(string metadataJson, int pageIndex);
		void SelectPage(string metadataJson, int pageIndex);
		void Browse(int galleryId);
		void SelectPagesFolder(int galleryId);
		void Read(int galleryId);
		void Select(int galleryId);
		void OpenArchive(int galleryId);
		void SelectArchive(int galleryId);
	}

	[ComDefaultInterface(typeof(ICacheApi)), ComVisible(true), ClassInterface(ClassInterfaceType.None), PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public class CacheApi : ICacheApi
	{
		private ICacheFileSystem CacheFileSystem { get; }

		public CacheApi(ICacheFileSystem cacheFileSystem)
		{
			CacheFileSystem = cacheFileSystem;
		}

		public void OpenMetadata(int galleryId)
		{
			CacheFileSystem.OpenMetadata(galleryId);
		}

		public void SelectMetadata(int galleryId)
		{
			CacheFileSystem.SelectMetadata(galleryId);
		}

		public void OpenCover(string metadataJson)
		{
			CacheFileSystem.OpenCover(metadataJson);
		}

		public void SelectCover(string metadataJson)
		{
			CacheFileSystem.SelectCover(metadataJson);
		}

		public void OpenPage(string metadataJson, int pageIndex)
		{
			CacheFileSystem.OpenPage(metadataJson, pageIndex);
		}

		public void SelectPage(string metadataJson, int pageIndex)
		{
			CacheFileSystem.SelectPage(metadataJson, pageIndex);
		}

		public void Browse(int galleryId)
		{
			CacheFileSystem.OpenPagesFolder(galleryId);
		}

		public void SelectPagesFolder(int galleryId)
		{
			CacheFileSystem.SelectPagesFolder(galleryId);
		}

		public void Read(int galleryId)
		{
			CacheFileSystem.OpenFirstCachedPage(galleryId);
		}

		public void Select(int galleryId)
		{
			CacheFileSystem.SelectFirstCachedPage(galleryId);
		}

		public void OpenArchive(int galleryId)
		{
			CacheFileSystem.OpenArchive(galleryId);
		}

		public void SelectArchive(int galleryId)
		{
			CacheFileSystem.SelectArchive(galleryId);
		}
	}

	[ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	public interface IFileSystemApi
	{
		IFileApi File { get; }
		IDirectoryApi Directory { get; }
		IPathApi Path { get; }
		ICacheApi Cache { get; }
	}

	[ComDefaultInterface(typeof(IFileSystemApi)), ComVisible(true), ClassInterface(ClassInterfaceType.None), PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public class FileSystemApi : IFileSystemApi
	{
		private readonly IFileApi file;
		private readonly IDirectoryApi directory;
		private readonly IPathApi path;
		private readonly ICacheApi cache;

		public IFileApi File => file;
		public IDirectoryApi Directory => directory;
		public IPathApi Path => path;
		public ICacheApi Cache => cache;

		public FileSystemApi(ICacheFileSystem cacheFileSystem, IPathFormatter pathFormatter, List<IArchiveWriter> archiveWriters, List<IMetadataConverter> metadataConverters)
		{
			file = new FileApi();
			directory = new DirectoryApi();
			path = new PathApi(cacheFileSystem, pathFormatter, archiveWriters, metadataConverters);
			cache = new CacheApi(cacheFileSystem);
		}
	}

	[ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	public interface INotificationsApi
	{
		Configuration.PagesDownloadCompletedNotification PagesDownloadCompleted { get; set; }
	}

	[ComDefaultInterface(typeof(INotificationsApi)), ComVisible(true), ClassInterface(ClassInterfaceType.None), PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public class NotificationsApi : INotificationsApi
	{
		private Configuration.ConfigNotifications NotificationSettings { get; }

		public Configuration.PagesDownloadCompletedNotification PagesDownloadCompleted
		{
			get { return NotificationSettings.PagesDownloadCompleted; }
			set { NotificationSettings.PagesDownloadCompleted = value; }
		}

		public NotificationsApi(Configuration.ConfigNotifications notificationSettings)
		{
			NotificationSettings = notificationSettings;
		}
	}

	[ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	public interface IGalleryDownloaderApi
	{
		bool IsQueueEmpty();
		bool IsInQueue(int galleryId);
		void CancelAll();
		void Cancel(int galleryId);
	}

	[ComDefaultInterface(typeof(IGalleryDownloaderApi)), ComVisible(true), ClassInterface(ClassInterfaceType.None), PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public class GalleryDownloaderApi : IGalleryDownloaderApi
	{
		private GalleryDownloader GalleryDownloader { get; }

		public GalleryDownloaderApi(GalleryDownloader galleryDownloader)
		{
			GalleryDownloader = galleryDownloader;
		}

		public bool IsQueueEmpty()
		{
			return GalleryDownloader.IsQueueEmpty;
		}

		public bool IsInQueue(int galleryId)
		{
			GalleryDownloaderJob job;

			return GalleryDownloader.TryFindJob(galleryId, out job);
		}

		public void CancelAll()
		{
			GalleryDownloader.CancelAll();
		}

		public void Cancel(int galleryId)
		{
			GalleryDownloaderJob job;

			if (GalleryDownloader.TryFindJob(galleryId, out job))
			{
				job.CancelAsync();
			}
		}
	}

	[ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	public interface ICoverDownloaderApi
	{
		bool IsQueueEmpty();
		bool IsInQueue(int galleryId);
		void CancelAll();
		void Cancel(int galleryId);
		void CancelSearchResult(string searchResultJson);
	}

	[ComDefaultInterface(typeof(ICoverDownloaderApi)), ComVisible(true), ClassInterface(ClassInterfaceType.None), PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public class CoverDownloaderApi : ICoverDownloaderApi
	{
		private CoverDownloader CoverDownloader { get; }

		public CoverDownloaderApi(CoverDownloader coverDownloader)
		{
			CoverDownloader = coverDownloader;
		}

		public bool IsQueueEmpty()
		{
			return CoverDownloader.IsQueueEmpty;
		}

		public bool IsInQueue(int galleryId)
		{
			CoverDownloaderJob job;

			return CoverDownloader.TryFindJob(galleryId, out job);
		}

		public void CancelAll()
		{
			CoverDownloader.CancelAll();
		}

		public void Cancel(int galleryId)
		{
			CoverDownloaderJob job;

			if (CoverDownloader.TryFindJob(galleryId, out job))
			{
				job.CancelAsync();
			}
		}

		public void CancelSearchResult(string searchResultJson)
		{
			SearchResult searchResult = JsonConvert.DeserializeObject<SearchResult>(searchResultJson);

			if (searchResult == null)
			{
				return;
			}

			CoverDownloaderJob job;

			if (CoverDownloader.TryFindJob(searchResult, out job))
			{
				job.CancelAsync();
			}
		}
	}

	[ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	public interface IPageDownloaderApi
	{
		void Download(int galleryId);
		void DownloadCustom(int galleryId, string pageIndicesList);

		bool IsQueueEmpty();
		bool IsInQueue(int galleryId);
		bool IsInQueueCustom(int galleryId, string pageIndicesList);
		void CancelAll();
		void Cancel(int galleryId);
		void CancelCustom(int galleryId, string pageIndicesList);
	}

	[ComDefaultInterface(typeof(IPageDownloaderApi)), ComVisible(true), ClassInterface(ClassInterfaceType.None), PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public class PageDownloaderApi : IPageDownloaderApi
	{
		private PageDownloader PageDownloader { get; }

		public PageDownloaderApi(PageDownloader pageDownloader)
		{
			PageDownloader = pageDownloader;
		}

		public void Download(int galleryId)
		{
			PageDownloader.Download(galleryId);
		}

		public void DownloadCustom(int galleryId, string pageIndicesList)
		{
			int[] pageIndices = pageIndicesList.Split(new char[] { ' ' }).Select(int.Parse).ToArray();

			PageDownloader.Download(galleryId, pageIndices);
		}

		public bool IsQueueEmpty()
		{
			return PageDownloader.IsQueueEmpty;
		}

		public bool IsInQueue(int galleryId)
		{
			return PageDownloader.HasAnyJob(galleryId);
			//return PageDownloader.GetJobs(galleryId).Count() > 0;
		}

		public bool IsInQueueCustom(int galleryId, string pageIndicesList)
		{
			try
			{
				int[] pageIndices = pageIndicesList.Split(new char[] { ' ' }).Select(int.Parse).ToArray();
				PageDownloaderJob job;

				return PageDownloader.TryFindJob(galleryId, pageIndices, out job);
			}
			catch { }

			return false;
		}

		public void CancelAll()
		{
			PageDownloader.CancelAll();
		}

		public void Cancel(int galleryId)
		{
			foreach (PageDownloaderJob job in PageDownloader.GetJobs(galleryId))
			{
				job.CancelAsync();
			}
		}

		public void CancelCustom(int galleryId, string pageIndicesList)
		{
			try
			{
				int[] pageIndices = pageIndicesList.Split(new char[] { ' ' }).Select(int.Parse).ToArray();
				PageDownloaderJob job;

				if (PageDownloader.TryFindJob(galleryId, pageIndices, out job))
				{
					job.CancelAsync();
				}
			}
			catch { }
		}
	}

	[ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	public interface IBackgroundWorkersApi
	{
		IGalleryDownloaderApi GalleryDownloader { get; }
		ICoverDownloaderApi CoverDownloader { get; }
		IPageDownloaderApi PageDownloader { get; }
	}

	[ComDefaultInterface(typeof(IBackgroundWorkersApi)), ComVisible(true), ClassInterface(ClassInterfaceType.None), PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public class BackgroundWorkersApi : IBackgroundWorkersApi
	{
		private readonly IGalleryDownloaderApi galleryDownloader;
		private readonly ICoverDownloaderApi coverDownloader;
		private readonly IPageDownloaderApi pageDownloader;

		public IGalleryDownloaderApi GalleryDownloader => galleryDownloader;
		public ICoverDownloaderApi CoverDownloader => coverDownloader;
		public IPageDownloaderApi PageDownloader => pageDownloader;

		public BackgroundWorkersApi(GalleryDownloader galleryDownloader, CoverDownloader coverDownloader, PageDownloader pageDownloader)
		{
			this.galleryDownloader = new GalleryDownloaderApi(galleryDownloader);
			this.coverDownloader = new CoverDownloaderApi(coverDownloader);
			this.pageDownloader = new PageDownloaderApi(pageDownloader);
		}
	}

	// NOTE: COM interfaces can't be inherited in C#.
	/*
	[ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	public interface IPluginsApi
	{
		int Count();
		string GetPluginInfo(int pluginIndex);
	}
	*/

	[ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	public interface IArchiveWriterPluginsApi// : IPluginsApi
	{
		int Count();
		string GetPluginInfo(int pluginIndex);

		void Create(int pluginIndex, int galleryId);
		void CreateEntryFromFile(int pluginIndex, string entryName, string entryFilePath, string archiveFilePath);
		void CreateFromDirectory(int pluginIndex, string directoryPath, string archiveFilePath);
		void CreateEmpty(int pluginIndex, string archiveFilePath);
	}

	[ComDefaultInterface(typeof(IArchiveWriterPluginsApi)), ComVisible(true), ClassInterface(ClassInterfaceType.None), PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public class ArchiveWriterPluginsApi : IArchiveWriterPluginsApi
	{
		private ISearchResultCache SearchResultCache { get; }
		private IPathFormatter PathFormatter { get; }
		private PluginSystem PluginSystem { get; }

		public ArchiveWriterPluginsApi(IPathFormatter pathFormatter, ISearchResultCache searchResultCache, PluginSystem pluginSystem)
		{
			PathFormatter = pathFormatter;
			SearchResultCache = searchResultCache;
			PluginSystem = pluginSystem;
		}

		private bool IsValidPluginIndex(int pluginIndex)
		{
			return pluginIndex >= 0
				&& pluginIndex <= PluginSystem.ArchiveWriters.Count() - 1;
		}

		public void Create(int pluginIndex, int galleryId)
		{
			if (!IsValidPluginIndex(pluginIndex))
			{
				return;
			}

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
				return;
			}

			PluginSystem.CreateArchive(PluginSystem.ArchiveWriters[pluginIndex], metadata);
		}

		public void CreateEntryFromFile(int pluginIndex, string entryName, string entryFilePath, string archiveFilePath)
		{
			if (!IsValidPluginIndex(pluginIndex))
			{
				return;
			}

			PluginSystem.ArchiveWriters[pluginIndex].CreateEntryFromFile(entryName, entryFilePath, archiveFilePath);
		}

		public void CreateFromDirectory(int pluginIndex, string directoryPath, string archiveFilePath)
		{
			if (!IsValidPluginIndex(pluginIndex))
			{
				return;
			}

			PluginSystem.ArchiveWriters[pluginIndex].CreateFromDirectory(directoryPath, archiveFilePath);
		}

		public void CreateEmpty(int pluginIndex, string archiveFilePath)
		{
			if (!IsValidPluginIndex(pluginIndex))
			{
				return;
			}

			PluginSystem.ArchiveWriters[pluginIndex].CreateEmpty(archiveFilePath);
		}

		public int Count()
		{
			return PluginSystem.ArchiveWriters.Count();
		}

		public string GetPluginInfo(int pluginIndex)
		{
			if (!IsValidPluginIndex(pluginIndex))
			{
				return "";
			}

			try
			{
				PluginInfoForScripting pluginInfo = new PluginInfoForScripting(PluginSystem.ArchiveWriters[pluginIndex].Info);

				return JsonConvert.SerializeObject(pluginInfo);
			}
			catch
			{
				return "";
			}
		}
	}

	[ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	public interface IMetadataConverterPluginsApi// : IPluginsApi
	{
		int Count();
		string GetPluginInfo(int pluginIndex);

		void WriteToCache(int pluginIndex, string metadataJson);
		string Convert(int pluginIndex, string metadataJson);
	}

	[ComDefaultInterface(typeof(IMetadataConverterPluginsApi)), ComVisible(true), ClassInterface(ClassInterfaceType.None), PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public class MetadataConverterPluginsApi : IMetadataConverterPluginsApi
	{
		private PluginSystem PluginSystem { get; }

		public MetadataConverterPluginsApi(PluginSystem pluginSystem)
		{
			PluginSystem = pluginSystem;
		}

		private bool IsValidPluginIndex(int pluginIndex)
		{
			return pluginIndex >= 0
				&& pluginIndex <= PluginSystem.MetadataConverters.Count() - 1;
		}

		public void WriteToCache(int pluginIndex, string metadataJson)
		{
			if (!IsValidPluginIndex(pluginIndex))
			{
				return;
			}

			try
			{
				Metadata metadata = JsonConvert.DeserializeObject<Metadata>(metadataJson);

				if (metadata == null)
				{
					return;
				}

				PluginSystem.CreateMetadataEmbed(PluginSystem.MetadataConverters[pluginIndex], metadata);
			}
			catch
			{

			}
		}

		public string Convert(int pluginIndex, string metadataJson)
		{
			if (!IsValidPluginIndex(pluginIndex))
			{
				return "";
			}

			try
			{
				Metadata metadata = JsonConvert.DeserializeObject<Metadata>(metadataJson);
				string result;

				if (PluginSystem.MetadataConverters[pluginIndex].Write(metadata, out result))
				{
					return result;
				}
			}
			catch
			{
			}

			return "";
		}

		public int Count()
		{
			return PluginSystem.MetadataConverters.Count();
		}

		public string GetPluginInfo(int pluginIndex)
		{
			if (!IsValidPluginIndex(pluginIndex))
			{
				return "";
			}

			try
			{
				PluginInfoForScripting pluginInfo = new PluginInfoForScripting(PluginSystem.MetadataConverters[pluginIndex].Info);

				return JsonConvert.SerializeObject(pluginInfo);
			}
			catch
			{
				return "";
			}
		}
	}

	[ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	public interface IMetadataProcessorPluginsApi// : IPluginsApi
	{
		int Count();
		string GetPluginInfo(int pluginIndex);

		bool Run(int pluginIndex, string metadataJson);
	}

	[ComDefaultInterface(typeof(IMetadataProcessorPluginsApi)), ComVisible(true), ClassInterface(ClassInterfaceType.None), PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public class MetadataProcessorPluginsApi : IMetadataProcessorPluginsApi
	{
		private IPathFormatter PathFormatter { get; }
		private PluginSystem PluginSystem { get; }

		public MetadataProcessorPluginsApi(IPathFormatter pathFormatter, PluginSystem pluginSystem)
		{
			PathFormatter = pathFormatter;
			PluginSystem = pluginSystem;
		}

		private bool IsValidPluginIndex(int pluginIndex)
		{
			return pluginIndex >= 0
				&& pluginIndex <= PluginSystem.MetadataProcessors.Count() - 1;
		}

		public bool Run(int pluginIndex, string metadataJson)
		{
			if (!IsValidPluginIndex(pluginIndex))
			{
				return false;
			}

			try
			{
				Metadata metadata = JsonConvert.DeserializeObject<Metadata>(metadataJson);

				if (metadata == null)
				{
					return false;
				}

				List<string> metadatas = new List<string>();
				List<string> covers = new List<string>();
				List<string> pages = new List<string>();
				List<string> archives = new List<string>();

				if (PathFormatter.IsEnabled)
				{
					for (int i = 0; i < metadata.Images.Pages.Count; ++i)
					{
						pages.Add(PathFormatter.GetPage(metadata, i));
					}

					covers.Add(PathFormatter.GetCover(metadata));

					foreach (var archiveWriter in PluginSystem.ArchiveWriters)
					{
						archives.Add(PathFormatter.GetArchive(metadata, archiveWriter));
					}

					metadatas.Add(PathFormatter.GetMetadata(metadata.Id));
				}
				// TODO: replace this with DefaultPathFormatter.* (which returns static formats instead of dynamic)
				else
				{
					for (int i = 0; i < metadata.Images.Pages.Count; ++i)
					{
						pages.Add(string.Format(CultureInfo.InvariantCulture, "{0}{1}/{2}{3}",
							PathFormatter.GetCacheDirectory(),
							metadata.Id,
							(i + 1).ToString(CultureInfo.InvariantCulture).PadLeft(global::NHxD.Frontend.Winforms.PathFormatter.GetBaseCount(metadata.Images.Pages.Count), '0'),
							metadata.Images.Pages[i].GetFileExtension()
						));
					}

					covers.Add(string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}",
						PathFormatter.GetCacheDirectory(),
						metadata.Id,
						metadata.Images.Cover.GetFileExtension()
						));

					foreach (var archiveWriter in PluginSystem.ArchiveWriters)
					{
						archives.Add(string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}",
							PathFormatter.GetCacheDirectory(),
							metadata.Id,
							archiveWriter.FileExtension
						));
					}

					metadatas.Add(string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}",
							PathFormatter.GetCacheDirectory(),
							metadata.Id,
							".json"
						));
				}

				GalleryResourcePaths paths = new GalleryResourcePaths(metadatas, covers, pages, archives);

				return PluginSystem.MetadataProcessors[pluginIndex].Run(metadata, paths);
			}
			catch
			{
			}

			return false;
		}

		public int Count()
		{
			return PluginSystem.MetadataProcessors.Count();
		}

		public string GetPluginInfo(int pluginIndex)
		{
			if (!IsValidPluginIndex(pluginIndex))
			{
				return "";
			}

			try
			{
				PluginInfoForScripting pluginInfo = new PluginInfoForScripting(PluginSystem.MetadataProcessors[pluginIndex].Info);

				return JsonConvert.SerializeObject(pluginInfo);
			}
			catch
			{
				return "";
			}
		}
	}

	public class PluginInfoForScripting : IPluginInfo
	{
		[JsonProperty("name")]
		public string Name { get; }

		[JsonProperty("description")]
		public string Description { get; }

		[JsonProperty("author")]
		public string Author { get; }

		[JsonProperty("version")]
		public string Version { get; }

		public PluginInfoForScripting(IPluginInfo pluginInfo)
		{
			Name = pluginInfo.Name;
			Description = pluginInfo.Description;
			Author = pluginInfo.Author;
			Version = pluginInfo.Version;
		}
	}

	[ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	public interface IAllPluginsApi
	{
		IArchiveWriterPluginsApi ArchiveWriters { get; }
		IMetadataConverterPluginsApi MetadataConverters { get; }
		IMetadataProcessorPluginsApi MetadataProcessors { get; }
	}

	[ComDefaultInterface(typeof(IAllPluginsApi)), ComVisible(true), ClassInterface(ClassInterfaceType.None), PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public class AllPluginsApi : IAllPluginsApi
	{
		private readonly IArchiveWriterPluginsApi archiveWriters;
		private readonly IMetadataConverterPluginsApi metadataConverters;
		private readonly IMetadataProcessorPluginsApi metadataProcessors;

		public IArchiveWriterPluginsApi ArchiveWriters => archiveWriters;
		public IMetadataConverterPluginsApi MetadataConverters => metadataConverters;
		public IMetadataProcessorPluginsApi MetadataProcessors => metadataProcessors;

		public AllPluginsApi(ISearchResultCache searchResultCache, IPathFormatter pathFormatter, PluginSystem pluginSystem)
		{
			archiveWriters = new ArchiveWriterPluginsApi(pathFormatter, searchResultCache, pluginSystem);
			metadataConverters = new MetadataConverterPluginsApi(pluginSystem);
			metadataProcessors = new MetadataProcessorPluginsApi(pathFormatter, pluginSystem);
		}
	}

	[ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	public interface ISearchApi
	{
		void ShowDetails(int galleryId);
		void ShowDownload(int galleryId);
		void BrowseLibrary(int pageIndex);
		void RunTaggedSearch(int tagId, int pageIndex);
		void RunSearch(string query, int pageIndex);
		void RunRecentSearch(int pageIndex);
	}

	[ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	public interface IFileSystemCacheApi
	{
		void OpenMetadata(int galleryId);
		void SelectMetadata(int galleryId);
		void OpenCover(string metadataJson);
		void SelectCover(string metadataJson);
		void OpenPage(string metadataJson, int pageIndex);
		void SelectPage(string metadataJson, int pageIndex);
		void Browse(int galleryId);
		void SelectPagesFolder(int galleryId);
		void Read(int galleryId);
		void Select(int galleryId);
		void OpenArchive(int galleryId);
		void SelectArchive(int galleryId);
		void ShowDetails(int galleryId);
		void ShowDownload(int galleryId);
	}

	[ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	public interface IDictionaryApi
	{
		void ShowTagDefinition(string term);
	}

	[ComDefaultInterface(typeof(IDictionaryApi)), ComVisible(true), ClassInterface(ClassInterfaceType.None), PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public class DictionaryApi : IDictionaryApi
	{
		private HttpClient HttpClient { get; }

		public DictionaryApi(HttpClient httpClient)
		{
			HttpClient = httpClient;
		}

		public void ShowTagDefinition(string term)
		{
			TagDefinition tagDefinition = new TagDefinition(term, HttpClient);

			tagDefinition.Search();
		}
	}

	[ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	public interface IBookmarkApi
	{
		void AddBookmark(string metadataJson, string path);
		void ShowAddDetailsBookmarkPrompt(string metadataJson);
		void ShowAddDownloadBookmarkPrompt(string metadataJson);
		void ShowAddRecentBookmarkPrompt(int pageIndex);
		void ShowAddSearchBookmarkPrompt(string query, int pageIndex);
		void ShowAddTaggedBookmarkPrompt(int tagId, int pageIndex);
		void ShowAddLibraryBookmarkPrompt(int pageIndex);
	}

	[ComDefaultInterface(typeof(IBookmarkApi)), ComVisible(true), ClassInterface(ClassInterfaceType.None), PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public class BookmarkApi : IBookmarkApi
	{
		private BookmarkPromptUtility BookmarkPromptUtility { get; }

		public BookmarkApi(BookmarkPromptUtility bookmarkPromptUtility)
		{
			BookmarkPromptUtility = bookmarkPromptUtility;
		}

		public void AddBookmark(string metadataJson, string path)
		{
			BookmarkPromptUtility.AddBookmark(metadataJson, path);
		}

		public void ShowAddDetailsBookmarkPrompt(string metadataJson)
		{
			BookmarkPromptUtility.ShowAddBookmarkPrompt(metadataJson);
		}

		public void ShowAddDownloadBookmarkPrompt(string metadataJson)
		{
			BookmarkPromptUtility.ShowAddDownloadBookmarkPrompt(metadataJson);
		}

		public void ShowAddRecentBookmarkPrompt(int pageIndex)
		{
			BookmarkPromptUtility.ShowAddRecentBookmarkPrompt(pageIndex);
		}

		public void ShowAddSearchBookmarkPrompt(string query, int pageIndex)
		{
			BookmarkPromptUtility.ShowAddQueryBookmarkPrompt(query, pageIndex);
		}

		public void ShowAddTaggedBookmarkPrompt(int tagId, int pageIndex)
		{
			BookmarkPromptUtility.ShowAddTaggedBookmarkPrompt(tagId, pageIndex);
		}

		public void ShowAddLibraryBookmarkPrompt(int pageIndex)
		{
			BookmarkPromptUtility.ShowAddLibraryBookmarkPrompt(pageIndex);
		}
	}

	[ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	public interface IMetadataKeywordListApi
	{
		//void AddProperty(string propertyType, string value);
		//void RemoveProperty(string propertyType, string value);
		void AddTag(string tagType, string tagName, int tagId);
		void RemoveTag(string tagType, string tagName, int tagId);
	}

	[ComDefaultInterface(typeof(IMetadataKeywordListApi)), ComVisible(true), ClassInterface(ClassInterfaceType.None), PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public class MetadataKeywordListApi : IMetadataKeywordListApi
	{
		private MetadataKeywordList MetadataKeywordList { get; }

		public MetadataKeywordListApi(MetadataKeywordList metadataKeywordList)
		{
			MetadataKeywordList = metadataKeywordList;
		}

		public void AddTag(string tagType, string tagName, int tagId)
		{
			TagType tagTypeValue;

			if (Enum.TryParse(tagType, true, out tagTypeValue))
			{
				MetadataKeywordList.Add(new TagInfo() { Type = tagTypeValue, Name = tagName, Id = tagId });
			}
		}

		public void RemoveTag(string tagType, string tagName, int tagId)
		{
			TagType tagTypeValue;

			if (Enum.TryParse(tagType, true, out tagTypeValue))
			{
				MetadataKeywordList.Remove(new TagInfo() { Type = tagTypeValue, Name = tagName, Id = tagId });
			}
		}
	}

	[ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	public interface IMetadataKeywordListsApi
	{
		MetadataKeywordListApi Whitelist { get; }
		MetadataKeywordListApi Blacklist { get; }
		MetadataKeywordListApi Ignorelist { get; }
		MetadataKeywordListApi Hidelist { get; }
	}

	[ComDefaultInterface(typeof(IMetadataKeywordListsApi)), ComVisible(true), ClassInterface(ClassInterfaceType.None), PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public class MetadataKeywordListsApi : IMetadataKeywordListsApi
	{
		private readonly MetadataKeywordListApi whitelist;
		private readonly MetadataKeywordListApi blacklist;
		private readonly MetadataKeywordListApi ignorelist;
		private readonly MetadataKeywordListApi hidelist;

		public MetadataKeywordListApi Whitelist => whitelist;
		public MetadataKeywordListApi Blacklist => blacklist;
		public MetadataKeywordListApi Ignorelist => ignorelist;
		public MetadataKeywordListApi Hidelist => hidelist;

		public MetadataKeywordListsApi(MetadataKeywordLists metadataKeywordLists)
		{
			whitelist = new MetadataKeywordListApi(metadataKeywordLists.Whitelist);
			blacklist = new MetadataKeywordListApi(metadataKeywordLists.Blacklist);
			ignorelist = new MetadataKeywordListApi(metadataKeywordLists.Ignorelist);
			hidelist = new MetadataKeywordListApi(metadataKeywordLists.Hidelist);
		}
	}

	[ComDefaultInterface(typeof(ISearchApi)), ComVisible(true), ClassInterface(ClassInterfaceType.None), PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public class SearchApi : ISearchApi
	{
		private SearchHandler SearchHandler { get; }

		public SearchApi(SearchHandler searchHandler)
		{
			SearchHandler = searchHandler;
		}

		public void ShowDetails(int galleryId)
		{
			SearchHandler.ShowDetails(galleryId);
		}

		public void ShowDownload(int galleryId)
		{
			SearchHandler.ShowDownload(galleryId);
		}

		public void BrowseLibrary(int pageIndex)
		{
			SearchHandler.BrowseLibrary(pageIndex);
		}

		public void RunTaggedSearch(int tagId, int pageIndex)
		{
			SearchHandler.RunTaggedSearch(tagId, pageIndex);
		}

		public void RunSearch(string query, int pageIndex)
		{
			SearchHandler.RunSearch(query, pageIndex);
		}

		public void RunRecentSearch(int pageIndex)
		{
			SearchHandler.RunRecentSearch(pageIndex);
		}
	}

	[ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
	public interface IPublicApi
	{
		IFileSystemApi FileSystem { get; }
		IBrowsersApi Browsers { get; }
		IBackgroundWorkersApi BackgroundWorkers { get; }
		IAllPluginsApi Plugins { get; }
		INotificationsApi Notifications { get; }
		IMetadataKeywordListsApi MetadataKeywordLists { get; }
		IBookmarkApi Bookmark { get; }
		IDictionaryApi Dictionary { get; }
		ISearchApi Search { get; }
		ISettingsApi Settings { get; }
	}

	[ComDefaultInterface(typeof(IPublicApi)), ComVisible(true), ClassInterface(ClassInterfaceType.None), PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
	public class PublicApi : IPublicApi
	{
		private readonly IFileSystemApi fileSystem;
		private readonly IBrowsersApi browsers;
		private readonly IBackgroundWorkersApi backgroundWorkers;
		private readonly IAllPluginsApi plugins;
		private readonly INotificationsApi notifications;
		private readonly IMetadataKeywordListsApi metadataKeywordLists;
		private readonly IBookmarkApi bookmark;
		private readonly IDictionaryApi dictionary;
		private readonly ISearchApi search;
		private readonly ISettingsApi settings;

		public IFileSystemApi FileSystem => fileSystem;
		public IBrowsersApi Browsers => browsers;
		public IBackgroundWorkersApi BackgroundWorkers => backgroundWorkers;
		public IAllPluginsApi Plugins => plugins;
		public INotificationsApi Notifications => notifications;
		public IMetadataKeywordListsApi MetadataKeywordLists => metadataKeywordLists;
		public IBookmarkApi Bookmark => bookmark;
		public IDictionaryApi Dictionary => dictionary;
		public ISearchApi Search => search;
		public ISettingsApi Settings => settings;

		public PublicApi(HttpClient httpClient
			, HttpClient genericHttpClient
			, IPathFormatter pathFormatter
			, ICacheFileSystem cacheFileSystem
			, ISearchResultCache searchResultCache
			, SearchHandler searchHandler
			, BookmarkPromptUtility bookmarkPromptUtility
			, PluginSystem pluginSystem
			, LibraryBrowserToolStrip libraryBrowserToolStrip
			, Configuration.ConfigLibrary librarySettings
			, GalleryBrowserToolStrip galleryBrowserToolStrip
			, Configuration.ConfigGallery gallerySettings
			, Configuration.ConfigDetails detailsSettings
			, GalleryDownloader galleryDownloader
			, CoverDownloader coverDownloader
			, PageDownloader pageDownloader
			, Configuration.ConfigNotifications notificationsSettings
			, MetadataKeywordLists metadataKeywordLists
			, Configuration.Settings settings
			)
		{
			fileSystem = new FileSystemApi(cacheFileSystem, pathFormatter, pluginSystem.ArchiveWriters, pluginSystem.MetadataConverters);
			browsers = new BrowsersApi(gallerySettings, galleryBrowserToolStrip, librarySettings, libraryBrowserToolStrip, detailsSettings);
			backgroundWorkers = new BackgroundWorkersApi(galleryDownloader, coverDownloader, pageDownloader);
			plugins = new AllPluginsApi(searchResultCache, pathFormatter, pluginSystem);
			notifications = new NotificationsApi(notificationsSettings);
			this.metadataKeywordLists = new MetadataKeywordListsApi(metadataKeywordLists);
			bookmark = new BookmarkApi(bookmarkPromptUtility);
			dictionary = new DictionaryApi(genericHttpClient);
			search = new SearchApi(searchHandler);
			this.settings = new SettingsApi(settings);
		}
	}
}
