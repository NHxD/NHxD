using Ash.System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Nhentai;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms.Configuration
{
	public class StartupSettings
	{
		[JsonProperty("settingsPath")]
		public string SettingsPath { get; set; } = "";

		[JsonProperty("defaultSettingsPath")]
		public string DefaultSettingsPath { get; set; } = "";
	}

	public class Settings
	{
		[JsonProperty("version")]
		public Version Version { get; set; } = new Version(1, 0, 0, 0);

		[JsonProperty("eula")]
		public ConfigEula Eula { get; set; } = new ConfigEula();

		[JsonProperty("splashScreen")]
		public ConfigSplashScreen SplashScreen { get; set; } = new ConfigSplashScreen();

		[JsonProperty("pathFormatter")]
		public ConfigPathFormatter PathFormatter { get; set; } = new ConfigPathFormatter();
		
		[JsonProperty("plugins")]
		public ConfigPlugins Plugins { get; set; } = new ConfigPlugins();

		[JsonProperty("notifications")]
		public ConfigNotifications Notifications { get; set; } = new ConfigNotifications();
		
		[JsonProperty("window")]
		public ConfigWindow Window { get; set; } = new ConfigWindow();

		[JsonProperty("tabControls")]
		public ConfigTabControls TabControls { get; set; } = new ConfigTabControls();

		[JsonProperty("lists")]
		public ConfigLists Lists { get; set; } = new ConfigLists();

		[JsonProperty("panels")]
		public ConfigPanels Panels { get; set; } = new ConfigPanels();

		[JsonProperty("gallery")]
		public ConfigGallery Gallery { get; set; } = new ConfigGallery();

		[JsonProperty("library")]
		public ConfigLibrary Library { get; set; } = new ConfigLibrary();

		[JsonProperty("details")]
		public ConfigDetails Details { get; set; } = new ConfigDetails();

		[JsonProperty("splitterDistances")]
		public ConfigSplitterDistances SplitterDistances { get; set; } = new ConfigSplitterDistances();

		[JsonProperty("download")]
		public ConfigDownload Download { get; set; } = new ConfigDownload();

		[JsonProperty("network")]
		public ConfigNetwork Network { get; set; } = new ConfigNetwork();

		[JsonProperty("cache")]
		public ConfigCache Cache { get; set; } = new ConfigCache();

		[JsonProperty("polling")]
		public ConfigPolling Polling { get; set; } = new ConfigPolling();

		[JsonProperty("backgroundWorkers")]
		public ConfigBackgroundWorkers BackgroundWorkers { get; set; } = new ConfigBackgroundWorkers();

		[JsonProperty("about")]
		public ConfigAbout About { get; set; } = new ConfigAbout();

		[JsonProperty("log")]
		public ConfigLog Log { get; set; } = new ConfigLog();
	}

	public class ConfigCache
	{
		[JsonProperty("searchResults")]
		public bool SearchResults { get; set; } = true;

		[JsonProperty("metadata")]
		public bool Metadata { get; set; } = true;

		[JsonProperty("templates")]
		public bool Templates { get; set; } = true;

		[JsonProperty("session")]
		public ConfigSession Session { get; set; } = new ConfigSession();
	}

	public class ConfigTabControls
	{
		[JsonProperty("lists")]
		public ConfigListsViewTabControl Lists { get; set; } = new ConfigListsViewTabControl();

		[JsonProperty("browser")]
		public ConfigMainViewTabControl Browser { get; set; } = new ConfigMainViewTabControl();

		[JsonProperty("details")]
		public ConfigDetailsViewTabControl Details { get; set; } = new ConfigDetailsViewTabControl();
	}

	public class ConfigLists
	{
		[JsonProperty("tags")]
		public ConfigTagsList Tags { get; set; } = new ConfigTagsList();

		[JsonProperty("bookmarks")]
		public ConfigBookmarksList Bookmarks { get; set; } = new ConfigBookmarksList();

		[JsonProperty("library")]
		public ConfigLibraryList Library { get; set; } = new ConfigLibraryList();

		[JsonProperty("browsing")]
		public ConfigBrowsingList Browsing { get; set; } = new ConfigBrowsingList();
	}

	public class ConfigPanels
	{
		[JsonProperty("console")]
		public ConfigPanel Console { get; set; } = new ConfigPanel() { IsCollapsed = true };

		[JsonProperty("lists")]
		public ConfigPanel Lists { get; set; } = new ConfigPanel();

		[JsonProperty("details")]
		public ConfigPanel Details { get; set; } = new ConfigPanel();
	}

	public class ConfigPanel
	{
		[JsonProperty("collapse")]
		public bool IsCollapsed { get; set; } = false;
	}

	public class ConfigAbout
	{
		[JsonProperty("size")]
		public Size Size { get; set; } = new Size(600, 279);
	}

	public class ConfigLog
	{
		[JsonProperty("filters"), JsonConverter(typeof(StringEnumConverter))]
		public LogFilters Filters { get; set; } = LogFilters.None;

		[JsonProperty("overwrite")]
		public bool Overwrite { get; set; } = true;

		[JsonProperty("keepSeparateLogs")]
		public bool KeepSeparateLogs { get; set; } = false;
	}

	public class ConfigEula
	{
		[JsonProperty("checkLegalAge")]
		public bool CheckLegalAge { get; set; } = true;

		[JsonProperty("pleadArtistSupport")]
		public bool PleadArtistSupport { get; set; } = true;
	}

	public class ConfigPlugins
	{
		[JsonProperty("archive")]
		public ConfigArchive Archive { get; set; } = new ConfigArchive();

		[JsonProperty("convertMetadata")]
		public ConfigConvertMetadata ConvertMetadata { get; set; } = new ConfigConvertMetadata();

		[JsonProperty("archiveWriters")]
		public Dictionary<string, Dictionary<string, string>> ArchiveWriters { get; set; } = new Dictionary<string, Dictionary<string, string>>();

		[JsonProperty("metadataConverters")]
		public Dictionary<string, Dictionary<string, string>> MetadataConverters { get; set; } = new Dictionary<string, Dictionary<string, string>>();

		[JsonProperty("metadataProcessors")]
		public Dictionary<string, Dictionary<string, string>> MetadataProcessors { get; set; } = new Dictionary<string, Dictionary<string, string>>();
	}

	public class ConfigArchive
	{
		[JsonProperty("autoCreate")]
		public bool AutoCreate { get; set; } = false;

		[JsonProperty("autoDeletePagesFolder")]
		public bool AutoDeletePagesFolder { get; set; } = false;
	}

	public class ConfigConvertMetadata
	{
		[JsonProperty("autoCreate")]
		public bool AutoCreate { get; set; } = false;
	}

	public class ConfigSplashScreen
	{
		[JsonProperty("show")]
		public bool IsVisible { get; set; } = true;
	}

	public class ConfigPathFormatter
	{
		[JsonProperty("enabled")]
		public bool IsEnabled { get; set; } = false;	// disabled by default because it's slow.

		[JsonProperty("custom")]
		public Dictionary<string, string> Custom { get; set; } = new Dictionary<string, string>()
		{
			{ "CachePath", "{SpecialFolder.MyDocuments}/NHxD/cache/" },
			{ "SessionPath", "{SpecialFolder.MyDocuments}/NHxD/sessions/" },
			{ "LogPath", "{SpecialFolder.LocalApplicationData}/NHxD/logs/" },
			{ "PluginPath", "{Path.Application}/" },
			{ "ResourcePath", "{Path.Application}/assets/" },
			{ "TemplatePath", "{Path.Application}/assets/templates/" },
		};

		// HACK: may be removed later.
		[JsonProperty("cachePath")]
		public string CachePath { get; set; } = "{@CachePath}";

		// HACK: may be removed later.
		[JsonProperty("sessionPath")]
		public string SessionPath { get; set; } = "{@SessionPath}";

		[JsonProperty("sessionSubPath")]
		public string SessionSubPath { get; set; } = "{@SessionPath}/{SessionType}";

		// HACK: may be removed later.
		[JsonProperty("metadataPath")]
		public string MetadataPath { get; set; } = "{@CachePath}";

		// HACK: may be removed later.
		[JsonProperty("convertedMetadataPath")]
		public string ConvertedMetadataPath { get; set; } = "{@CachePath}";

		// HACK: may be removed later.
		[JsonProperty("coverPath")]
		public string CoverPath { get; set; } = "{@CachePath}";

		// HACK: may be removed later.
		[JsonProperty("pagesPath")]
		public string PagesPath { get; set; } = "{@CachePath}";

		// HACK: may be removed later.
		[JsonProperty("archivePath")]
		public string ArchivePath { get; set; } = "{@CachePath}";

		// HACK: may be removed later.
		[JsonProperty("pluginPath")]
		public string PluginPath { get; set; } = "{@PluginPath}";

		[JsonProperty("logsPath")]
		public string LogsPath { get; set; } = "{@LogPath}";

		// HACK: may be removed later.
		[JsonProperty("resourcePath")]
		public string ResourcePath { get; set; } = "{@ResourcePath}";

		// HACK: may be removed later.
		[JsonProperty("templatePath")]
		public string TemplatePath { get; set; } = "{@TemplatePath}";


		[JsonProperty("defaultConfiguration")]
		public string DefaultConfiguration { get; set; } = "{Path.Application}/assets/defaults/{FileTitle}{FileExt}";

		[JsonProperty("configuration")]
		public string Configuration { get; set; } = "{@SettingsPath}/{FileTitle}{FileExt}";

		[JsonProperty("session")]
		public string Session { get; set; } = "{@SessionPath}/{SessionQuery}{FileExt}";
		
		[JsonProperty("metadata")]
		public string Metadata { get; set; } = "{@CachePath}/{Metadata.Id}{FileExt}";

		[JsonProperty("convertedMetadata")]
		public string ConvertedMetadata { get; set; } = "{@CachePath}/{Metadata.Id}{FileExt}";

		[JsonProperty("cover")]
		public string Cover { get; set; } = "{@CachePath}/{Metadata.Id}{FileExt}";

		[JsonProperty("pages")]
		public string Pages { get; set; } = "{@CachePath}/{Metadata.Id}/";

		[JsonProperty("page")]
		public string Page { get; set; } = "{@CachePath}/{Metadata.Id}/{Metadata.PageIndex,0}{FileExt}";

		[JsonProperty("archive")]
		public string Archive { get; set; } = "{@CachePath}/{Metadata.Id}{FileExt}";

		[JsonProperty("plugin")]
		public string Plugin { get; set; } = "{@PluginPath}/{PluginName}{FileExt}";

		[JsonProperty("log")]
		public string Log { get; set; } = "{@LogPath}/{FileTitle}{FileExt}";

		[JsonProperty("resource")]
		public string Resource { get; set; } = "{@ResourcePath}/{FileTitle}{FileExt}";

		[JsonProperty("template")]
		public string Template { get; set; } = "{@TemplatePath}/{FileTitle}{FileExt}";
	}

	public class ConfigBackgroundWorkers
	{
		[JsonProperty("backgroundTaskWorker")]
		public ConfigBackgroundWorker BackgroundTaskWorker { get; set; } = new ConfigBackgroundWorker();

		[JsonProperty("pageDownloader")]
		public ConfigBackgroundWorker PageDownloader { get; set; } = new ConfigBackgroundWorker();

		[JsonProperty("coverDownloader")]
		public ConfigBackgroundWorker CoverDownloader { get; set; } = new ConfigBackgroundWorker();

		[JsonProperty("coverLoader")]
		public ConfigBackgroundWorker CoverLoader { get; set; } = new ConfigBackgroundWorker();

		[JsonProperty("galleryDownloader")]
		public ConfigBackgroundWorker GalleryDownloader { get; set; } = new ConfigBackgroundWorker();
	}
	public class ConfigPolling
	{
		[JsonProperty("filterDelay")]
		public int FilterDelay { get; set; } = 500;

		[JsonProperty("libraryRefreshInterval")]
		public int LibraryRefreshInterval { get; set; } = 1000;
	}

	public class ConfigBackgroundWorker
	{
		[JsonProperty("idleWaitTime")]
		public int IdleWaitTime { get; set; } = 100;

		[JsonProperty("maxConcurrentJobCount")]
		public int MaxConcurrentJobCount { get; set; } = 1;
	}

	public class ConfigNotifications
	{
		[JsonProperty("pagesDownloadCompleted"), JsonConverter(typeof(StringEnumConverter))]
		public PagesDownloadCompletedNotification PagesDownloadCompleted { get; set; } = PagesDownloadCompletedNotification.None;
	}

	public enum PagesDownloadCompletedNotification
	{
		None,
		Always,
		FocusedOnly
	}

	public class ConfigListsViewTabControl : ConfigTabControl
	{
		[JsonProperty("tags")]
		public ConfigTab Tags { get; set; } = new ConfigTab();

		[JsonProperty("bookmarks")]
		public ConfigTab Bookmarks { get; set; } = new ConfigTab();

		[JsonProperty("library")]
		public ConfigTab Library { get; set; } = new ConfigTab();

		[JsonProperty("browsing")]
		public ConfigTab Browsing { get; set; } = new ConfigTab();
	}

	public class ConfigMainViewTabControl : ConfigTabControl
	{
		[JsonProperty("gallery")]
		public ConfigTab Gallery { get; set; } = new ConfigTab() { OverrideMargin = true, OverridePadding = true };

		[JsonProperty("library")]
		public ConfigTab Library { get; set; } = new ConfigTab() { OverrideMargin = true, OverridePadding = true };

		[JsonProperty("downloads"), JsonIgnore]	// NOTE: ignored for now because not fully implemented yet.
		public ConfigTab Downloads { get; set; } = new ConfigTab() { OverrideMargin = true, OverridePadding = true, IsVisible = false };
	}

	public class ConfigDetailsViewTabControl : ConfigTabControl
	{
		[JsonProperty("details")]
		public ConfigTab Details { get; set; } = new ConfigTab() { OverrideMargin = true, OverridePadding = true };

		[JsonProperty("download"), JsonIgnore]  // NOTE: ignored for now because not fully implemented yet.
		public ConfigTab Download { get; set; } = new ConfigTab() { OverrideMargin = true, OverridePadding = true , IsVisible = false };
	}

	public class ConfigTabControl
	{
		[JsonProperty("selectedTab")]
		public string SelectedTab { get; set; } = "";
	}

	public class ConfigTab
	{
		[JsonProperty("show")]
		public bool IsVisible { get; set; } = true;

		[JsonProperty("overrideMargin")]
		public bool OverrideMargin { get; set; } = false;

		[JsonProperty("overridePadding")]
		public bool OverridePadding { get; set; } = false;

		[JsonProperty("margin")]
		public Padding Margin { get; set; } = Padding.Empty;

		[JsonProperty("padding")]
		public Padding Padding { get; set; } = Padding.Empty;

		[JsonProperty("toolStrip")]
		public ConfigToolStrip ToolStrip { get; set; } = new ConfigToolStrip();

		public void Apply(TabPage tabPage)
		{
			if (OverrideMargin)
			{
				tabPage.Margin = Margin;
			}

			if (OverridePadding)
			{
				tabPage.Padding = Padding;
			}

			if (!IsVisible)
			{
				tabPage?.Parent?.Controls.Remove(tabPage);
			}
		}
	}

	public class ConfigNetwork
	{
		[JsonProperty("connectionTimeout")]
		public int ConnectionTimeout { get; set; } = -1;

		[JsonProperty("connectionLeaseTimeout")]
		public int ConnectionLeaseTimeout { get; set; } = -1;

		[JsonProperty("maxIdleTime")]
		public int MaxIdleTime { get; set; } = -1;

		[JsonProperty("client")]
		public ConfigClient Client { get; set; } = new ConfigClient();
	}

	public class ConfigClient
	{
		[JsonProperty("userAgent")]
		public string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:44.0) Gecko/20100101 Firefox/44.0";

		[JsonProperty("credentials")]
		public ConfigNetworkCredentials Credentials { get; set; } = new ConfigNetworkCredentials();

		[JsonProperty("proxy")]
		public ConfigProxy Proxy { get; set; } = new ConfigProxy();

		[JsonIgnore]
		public bool HasCredentials => !string.IsNullOrEmpty(Credentials.UserName);
	}

	public class ConfigProxy
	{
		[JsonProperty("enable")]
		public bool IsEnabled { get; set; } = false;

		[JsonProperty("address")]
		public string Address { get; set; } = "";

		[JsonProperty("port")]
		public int Port { get; set; } = 80;

		[JsonProperty("bypassProxyOnLocal")]
		public bool BypassProxyOnLocal { get; set; } = true;
		
		[JsonProperty("bypassList")]
		public string[] BypassList { get; set; } = { };

		[JsonProperty("credentials")]
		public ConfigNetworkCredentials Credentials { get; set; } = new ConfigNetworkCredentials();

		[JsonIgnore]
		public bool HasCredentials => !string.IsNullOrEmpty(Credentials.UserName);
	}

	public class ConfigNetworkCredentials
	{
		[JsonProperty("userName")]
		public string UserName { get; set; } = "";

		[JsonProperty("password")]
		public string Password { get; set; } = "";
	}

	public class ConfigSession
	{
		[JsonProperty("checkAtStartup")]
		public bool CheckAtStartup { get; set; } = false;

		[JsonProperty("checkAtRuntime")]
		public bool CheckAtRuntime { get; set; } = true;

		[JsonProperty("recentLifeSpan")]
		public int RecentLifeSpan { get; set; } = (int)TimeSpan.FromMinutes(30).TotalMilliseconds;

		[JsonProperty("searchLifeSpan")]
		public int SearchLifeSpan { get; set; } = (int)TimeSpan.FromHours(1).TotalMilliseconds;

		[JsonProperty("taggedLifeSpan")]
		public int TaggedLifeSpan { get; set; } = (int)TimeSpan.FromHours(1).TotalMilliseconds;
	}

	public class ConfigGallery
	{
		[JsonProperty("browser")]
		public ConfigGalleryBrowserView Browser { get; set; } = new ConfigGalleryBrowserView();

		[JsonProperty("toolStrip")]
		public ConfigGalleryToolStrip ToolStrip { get; set; } = new ConfigGalleryToolStrip();

		[JsonProperty("startupAction"), JsonConverter(typeof(StringEnumConverter))]
		public GalleryStartupAction StartupAction { get; set; } = GalleryStartupAction.Continue;

		[JsonProperty("startupSpecialDateFilters"), JsonConverter(typeof(StringEnumConverter))]
		public StartupSpecialDateFilters StartupSpecialDateFilters { get; set; } = StartupSpecialDateFilters.All;

		[JsonProperty("latestActivatedStartupSpecialDate")]
		public DateTime LatestActivatedStartupSpecialDate { get; set; } = new DateTime();

		[JsonProperty("preventMultipleStartupSpecial")]
		public bool PreventMultipleStartupSpecial { get; set; } = true;

		[JsonProperty("startupUrl")]
		public string StartupUrl { get; set; } = "";
	}

	public enum GalleryStartupAction
	{
		None,
		Continue,
		Recent,
		Custom
	}

	public class ConfigGalleryBrowserView : ConfigBrowserView
	{
		[JsonProperty("sortType"), JsonConverter(typeof(StringEnumConverter))]
		public GallerySortType SortType { get; set; } = GallerySortType.None;

		[JsonProperty("sortOrder"), JsonConverter(typeof(StringEnumConverter))]
		public SortOrder SortOrder { get; set; } = SortOrder.Ascending;

		[JsonProperty("resultPerPage"), JsonIgnore]	// NOTE: currently not supported.
		public int NumResultsPerPage { get; set; } = 25;

		[JsonProperty("itemDownloadProgress")]
		public ConfigGalleryItemDownloadProgress ItemDownloadProgress { get; set; } = new ConfigGalleryItemDownloadProgress();
	}

	public class ConfigGalleryItemDownloadProgress : ConfigItemDownloadProgress
	{
	}

	public class ConfigGalleryToolStrip : ConfigToolStrip
	{
		// NOTE: serialize this separately because it can get very large.
		[JsonProperty("history"), JsonIgnore]
		public List<string> History { get; set; } = new List<string>();

		// NOTE: serialize this separately because it can get very large.
		[JsonProperty("filters"), JsonIgnore]
		public List<string> Filters { get; set; } = new List<string>();
	}

	public class ConfigLibrary
	{
		[JsonProperty("browser")]
		public ConfigLibraryBrowserView Browser { get; set; } = new ConfigLibraryBrowserView();

		[JsonProperty("toolStrip")]
		public ConfigLibraryToolStrip ToolStrip { get; set; } = new ConfigLibraryToolStrip();
	}

	public class ConfigLibraryBrowserView : ConfigBrowserView
	{
		[JsonProperty("sortType"), JsonConverter(typeof(StringEnumConverter))]
		public GallerySortType SortType { get; set; } = GallerySortType.None;

		[JsonProperty("sortOrder"), JsonConverter(typeof(StringEnumConverter))]
		public SortOrder SortOrder { get; set; } = SortOrder.Descending;

		[JsonProperty("globalSortType"), JsonConverter(typeof(StringEnumConverter))]
		public LibrarySortType GlobalSortType { get; set; } = LibrarySortType.CreationTime;

		[JsonProperty("globalSortOrder"), JsonConverter(typeof(StringEnumConverter))]
		public SortOrder GlobalSortOrder { get; set; } = SortOrder.Descending;

		[JsonProperty("resultPerPage")]
		public int NumResultsPerPage { get; set; } = 50;

		[JsonProperty("itemDownloadProgress")]
		public ConfigLibraryItemDownloadProgress ItemDownloadProgress { get; set; } = new ConfigLibraryItemDownloadProgress();
	}

	public class ConfigLibraryItemDownloadProgress : ConfigItemDownloadProgress
	{
	}

	public class ConfigItemDownloadProgress
	{
		[JsonProperty("show")]
		public bool IsVisible { get; set; } = true;
	}

	public class ConfigLibraryToolStrip : ConfigToolStrip
	{
		// NOTE: serialize this separately because it can get very large.
		[JsonProperty("filters"), JsonIgnore]
		public List<string> Filters { get; set; } = new List<string>();
	}

	public class ConfigDetails
	{
		[JsonProperty("browser")]
		public ConfigDetailsBrowserView Browser { get; set; } = new ConfigDetailsBrowserView();

		[JsonProperty("toolStrip")]
		public ConfigDetailsToolStrip ToolStrip { get; set; } = new ConfigDetailsToolStrip();

		[JsonProperty("startupAction"), JsonConverter(typeof(StringEnumConverter))]
		public DetailsStartupAction StartupAction { get; set; } = DetailsStartupAction.Continue;
	}

	public class ConfigDownload
	{
		[JsonProperty("browser")]
		public ConfigDownloadBrowserView Browser { get; set; } = new ConfigDownloadBrowserView();

		//[JsonProperty("toolStrip")]
		//public ConfigDownloadToolStrip ToolStrip { get; set; } = new ConfigDownloadToolStrip();

		//[JsonProperty("startupAction"), JsonConverter(typeof(StringEnumConverter))]
		//public DownloadStartupAction StartupAction { get; set; } = DownloadStartupAction.Continue;
	}

	public enum DetailsStartupAction
	{
		None,
		Continue,
	}

	public class ConfigDetailsBrowserView : ConfigBrowserView
	{
		[JsonProperty("reloadDocumentOnCoverClicked")]
		public bool ReloadDocumentOnCoverClicked { get; set; } = false;

		[JsonProperty("coverLoadBlockAction"), JsonConverter(typeof(StringEnumConverter))]
		public DetailsCoverLoadBlockAction CoverLoadBlockAction { get; set; } = DetailsCoverLoadBlockAction.Confirm;
	}

	public class ConfigDownloadBrowserView : ConfigBrowserView
	{
		[JsonProperty("reloadDocumentOnCoverClicked")]
		public bool ReloadDocumentOnCoverClicked { get; set; } = false;
	}

	public class ConfigDetailsToolStrip : ConfigToolStrip
	{
		// NOTE: serialize this separately because it can get very large.
		[JsonProperty("history"), JsonIgnore]
		public List<int> History { get; set; } = new List<int>();
	}

	public class ConfigSplitterDistances
	{
		[JsonProperty("console")]
		public int Console { get; set; } = 22;

		[JsonProperty("lists")]
		public int Lists { get; set; } = 240;

		[JsonProperty("details")]
		public int Details { get; set; } = 600;
	}

	public class ConfigTagsList : ConfigControl
	{
		[JsonProperty("filter")]
		public ConfigTagsListFilter Filter { get; set; } = new ConfigTagsListFilter();

		[JsonProperty("sortType"), JsonConverter(typeof(StringEnumConverter))]
		public TagSortType SortType { get; set; } = TagSortType.Name;

		[JsonProperty("sortOrder"), JsonConverter(typeof(StringEnumConverter))]
		public SortOrder SortOrder { get; set; } = SortOrder.Ascending;

		[JsonProperty("collapsed")]
		public Dictionary<TagType, bool> Collapsed { get; set; } = new Dictionary<TagType, bool>();

		[JsonProperty("labelFormats")]
		public ConfigTagsListLabelFormats LabelFormats { get; set; } = new ConfigTagsListLabelFormats();

		[JsonProperty("LanguageNames")]
		public string[] LanguageNames { get; set; } = { "english", "japanese", "korean", "chinese", "cebuano" };

		[JsonProperty("blockActions"), JsonConverter(typeof(StringEnumConverter))]
		public TagsFilters BlockActions { get; set; } = TagsFilters.Blacklist | TagsFilters.Ignorelist | TagsFilters.Hidelist;

		[JsonProperty("forceRuntimeUpdate")]
		public bool ForceRuntimeUpdate { get; set; } = true;

		[JsonProperty("colors")]
		public ConfigTagsListColors Colors { get; set; } = new ConfigTagsListColors();
	}

	public class ConfigTagsListColors
	{
		[JsonProperty("whitelist")]
		public Color Whitelist { get; set; } = Color.Blue;

		[JsonProperty("blacklist")]
		public Color Blacklist { get; set; } = Color.Red;

		[JsonProperty("default")]
		public Color Default { get; set; } = Color.FromKnownColor(KnownColor.ControlText);
	}

	public class ConfigTagsListLabelFormats
	{
		[JsonProperty("tag")]
		public string Tag { get; set; } = "{Tag.Name}";

		[JsonProperty("header")]
		public string Header { get; set; } = "{Tag.Type} ({Tag.Count})";
	}

	public class ConfigBookmarksList : ConfigControl
	{
		[JsonProperty("filter")]
		public ConfigBookmarksListFilter Filter { get; set; } = new ConfigBookmarksListFilter();

		[JsonProperty("collapsed")]
		public Dictionary<string, bool> Collapsed { get; set; } = new Dictionary<string, bool>();

		[JsonProperty("mostRecentPath")]
		public string MostRecentPath { get; set; } = "";

		[JsonProperty("restoreMostRecentPath")]
		public bool RestoreMostRecentPath { get; set; } = true;

		[JsonProperty("labelFormats")]
		public ConfigBookmarksListLabelFormats LabelFormats { get; set; } = new ConfigBookmarksListLabelFormats();
	}

	public class ConfigBookmarksListLabelFormats
	{
		[JsonProperty("recent")]
		public string RecentSearch { get; set; } = "Recent ({Search.PageIndex})";

		[JsonProperty("search")]
		public string QuerySearch { get; set; } = "Search \"{Search.Query:Html.Decode}\" ({Search.PageIndex})";

		[JsonProperty("tagged")]
		public string Tagged { get; set; } = "Tagged {Search.TagId} \"{Search.Query:Tag.Name}\" ({Search.PageIndex})";

		[JsonProperty("library")]
		public string Library { get; set; } = "Library ({Search.PageIndex})";

		[JsonProperty("details")]
		public string Details { get; set; } = "{Metadata.Id} - {Metadata.Title.Pretty:Html.Decode} by {Metadata.Artists:Html.Decode}";

		[JsonProperty("download")]
		public string Download { get; set; } = "{Metadata.Id} - {Metadata.Title.Pretty:Html.Decode} by {Metadata.Artists:Html.Decode}";
	}

	public class ConfigLibraryList : ConfigControl
	{
		[JsonProperty("filter")]
		public ConfigLibraryListFilter Filter { get; set; } = new ConfigLibraryListFilter();

		[JsonProperty("sortType"), JsonConverter(typeof(StringEnumConverter))]
		public LibrarySortType SortType { get; set; } = LibrarySortType.CreationTime;

		[JsonProperty("sortOrder"), JsonConverter(typeof(StringEnumConverter))]
		public SortOrder SortOrder { get; set; } = SortOrder.Descending;

		[JsonProperty("collapsed")]
		public Dictionary<string, bool> Collapsed { get; set; } = new Dictionary<string, bool>();

		[JsonProperty("tooltip")]
		public ConfigLibraryListToolTip ToolTip { get; set; } = new ConfigLibraryListToolTip();
	}

	public class ConfigLibraryListToolTip //: ConfigBrowserView
	{
		[JsonProperty("size")]
		public Size Size { get; set; } = new Size(640, 380);
	}

	public class ConfigBrowsingList : ConfigControl
	{
		[JsonProperty("filter")]
		public ConfigBrowsingListFilter Filter { get; set; } = new ConfigBrowsingListFilter();

		[JsonProperty("sortType"), JsonConverter(typeof(StringEnumConverter))]
		public BrowsingSortType SortType { get; set; } = BrowsingSortType.LastAccessTime;

		[JsonProperty("sortOrder"), JsonConverter(typeof(StringEnumConverter))]
		public SortOrder SortOrder { get; set; } = SortOrder.Descending;

		[JsonProperty("collapsed")]
		public Dictionary<DateTime, bool> Collapsed { get; set; } = new Dictionary<DateTime, bool>();
	}

	public class ConfigTagsListExpanded
	{
		[JsonProperty("tag")]
		public bool Tag { get; set; } = false;

		[JsonProperty("artist")]
		public bool Artist { get; set; } = false;

		[JsonProperty("group")]
		public bool Group { get; set; } = false;

		[JsonProperty("parody")]
		public bool Parody { get; set; } = false;

		[JsonProperty("character")]
		public bool Character { get; set; } = false;

		[JsonProperty("category")]
		public bool Category { get; set; } = false;

		[JsonProperty("language")]
		public bool Language { get; set; } = false;
	}

	public class ConfigTagsListFilter
	{
		[JsonProperty("filters"), JsonConverter(typeof(StringEnumConverter))]
		public TagsFilters Filters { get; set; } = TagsFilters.All;
	}

	public class ConfigBookmarksListFilter
	{
		[JsonProperty("filters"), JsonConverter(typeof(StringEnumConverter))]
		public BookmarkFilters Filters { get; set; } = BookmarkFilters.All;
	}

	public class ConfigLibraryListFilter
	{
		[JsonProperty("filters"), JsonConverter(typeof(StringEnumConverter))]
		public LibraryFilters Filters { get; set; } = LibraryFilters.All;

		[JsonProperty("dateFilters"), JsonConverter(typeof(StringEnumConverter))]
		public LibraryDateFilters DateFilters { get; set; } = LibraryDateFilters.None;

		[JsonProperty("afterDate")]
		public DateTime AfterDate { get; set; } = new DateTime();

		[JsonProperty("beforeDate")]
		public DateTime BeforeDate { get; set; } = new DateTime();
	}

	public class ConfigBrowsingListFilter
	{
		[JsonProperty("filters"), JsonConverter(typeof(StringEnumConverter))]
		public BrowsingFilters Filters { get; set; } = BrowsingFilters.All;

		[JsonProperty("dateFilters"), JsonConverter(typeof(StringEnumConverter))]
		public BrowsingDateFilters DateFilters { get; set; } = BrowsingDateFilters.None;

		[JsonProperty("afterDate")]
		public DateTime AfterDate { get; set; } = new DateTime();

		[JsonProperty("beforeDate")]
		public DateTime BeforeDate { get; set; } = new DateTime();
	}

	public class ConfigWindow
	{
		[JsonProperty("state"), JsonConverter(typeof(StringEnumConverter))]
		public FormWindowState State { get; set; } = FormWindowState.Maximized;

		[JsonProperty("location")]
		public Point Location { get; set; } = new Point();

		[JsonProperty("size")]
		public Size Size { get; set; } = new Size();

		[JsonProperty("fullScreen")]
		public ConfigFullScreen FullScreen { get; set; } = new ConfigFullScreen();

		[JsonProperty("text")]
		public string TextFormat { get; set; } = "{FileVersionInfo.ProductName}";

		[JsonProperty("useCompositeStyle")]
		public bool UseCompositeStyle { get; set; } = false;

		public void Apply(Form form)
		{
			if (State == FormWindowState.Minimized)
			{
				return;
			}

			form.WindowState = State;

			if (!Location.IsEmpty)
			{
				form.Location = Location;
			}

			if (!Size.IsEmpty)
			{
				form.Size = Size;
			}
		}
	}

	public class ConfigFullScreen
	{
		[JsonProperty("active")]
		public bool IsActive { get; set; } = false;

		[JsonProperty("padding")]
		public Padding Padding { get; set; } = new Padding(6, 3, 6, 3);

		[JsonProperty("autoHidePanels")]
		public ConfigAutoHidePanels AutoHidePanels { get; set; } = new ConfigAutoHidePanels();
	}

	public class ConfigAutoHidePanels
	{
		[JsonProperty("lists")]
		public ConfigAutoHidePanel Lists { get; set; } = new ConfigAutoHidePanel();

		[JsonProperty("details")]
		public ConfigAutoHidePanel Details { get; set; } = new ConfigAutoHidePanel();
	}

	public class ConfigAutoHidePanel
	{
		[JsonProperty("collapse")]
		public bool IsCollapsed { get; set; } = true;
	}

	public class ConfigBrowserView
	{
		[JsonProperty("zoomRatio")]
		public float ZoomRatio { get; set; } = 1.0f;

		[JsonProperty("allowContextMenu")]
		public bool AllowContextMenu { get; set; } = false;

		[JsonProperty("allowNavigation")]
		public bool AllowNavigation { get; set; } = true;

		[JsonProperty("allowShortcuts")]
		public bool AllowShortcuts { get; set; } = true;

		[JsonProperty("suppressScriptErrors")]
		public bool SuppressScriptErrors { get; set; } = true;

		public void Apply(WebBrowser webBrowser)
		{
			webBrowser.IsWebBrowserContextMenuEnabled = AllowContextMenu;
			webBrowser.AllowNavigation = AllowNavigation;
			webBrowser.WebBrowserShortcutsEnabled = AllowShortcuts;
			webBrowser.ScriptErrorsSuppressed = SuppressScriptErrors;
		}
	}

	public class ConfigToolStrip : ConfigControl
	{
		[JsonProperty("show")]
		public bool IsVisible { get; set; } = true;

		[JsonProperty("canOverflow")]
		public bool CanOverflow { get; set; } = false;

		[JsonProperty("layoutStyle"), JsonConverter(typeof(StringEnumConverter))]
		public ToolStripLayoutStyle LayoutStyle { get; set; } = ToolStripLayoutStyle.StackWithOverflow;

		public void Apply(ToolStrip toolStrip)
		{
			toolStrip.CanOverflow = CanOverflow;
			toolStrip.LayoutStyle = LayoutStyle;
			toolStrip.Visible = IsVisible;
		}
	}

	public class ConfigControl
	{
		[JsonProperty("font")]
		public ConfigFont Font { get; set; } = new ConfigFont();

		public void Apply(Control control)
		{
			Font.Apply(control);
		}
	}

	public class ConfigFont
	{
		[JsonProperty("family")]
		public string Family { get; set; } = "";

		[JsonProperty("size")]
		public float Size { get; set; } = 0;

		public void Apply(Control control)
		{
			if (!string.IsNullOrEmpty(Family)
				|| Size >= 0.1f)
			{
				string fontFamilyName = string.IsNullOrEmpty(Family) ? control.Font.FontFamily.Name : Family;
				float fontSize = Size < 0.1f ? control.Font.Size : Size;

				control.Font = new Font(fontFamilyName, fontSize);
			}
		}
	}
}
