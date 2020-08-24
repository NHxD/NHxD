using Ash.System.Diagnostics;
using Ash.System.Windows.Forms;
using Ash.System.Windows.ShellProvider;
using Nhentai;
using NHxD.Formatting;
using NHxD.Formatting.TokenModifiers;
using NHxD.Plugin.ArchiveWriter;
using NHxD.Plugin.MetadataConverter;
using NHxD.Plugin.MetadataProcessor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
//using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	public partial class MainForm : Form//, IMessageFilter
	{
		public Logger Logger => Program.Logger;
		public Configuration.Settings Settings => Program.Settings;

		private readonly PublicApi publicApi;
		private readonly StaticHttpClient staticHttpClient;
		private readonly MetadataKeywordLists metadataKeywordLists;
		private readonly List<IMetadataProcessor> metadataProcessors;
		private readonly List<IMetadataConverter> metadataConverters;
		private readonly List<IArchiveWriter> archiveWriters;
		private readonly PluginSystem pluginSystem;
		private readonly IQueryParser queryParser;
		private readonly IPathFormatter pathFormatter;
		private readonly CoreTextFormatter coreTextFormatter;
		private readonly AboutTextFormatter aboutTextFormatter;
		private readonly TagTextFormatter tagTextFormatter;
		private readonly MetadataTextFormatter metadataTextFormatter;
		private readonly SearchArgTextFormatter searchArgTextFormatter;
		private readonly SearchProgressArgTextFormatter searchProgressArgTextFormatter;
		private readonly BackgroundTaskWorker backgroundTaskWorker;
		private readonly PageDownloader pageDownloader;
		private readonly CoverDownloader coverDownloader;
		private readonly CoverDownloader coverLoader;
		private readonly GalleryDownloader galleryDownloader;
		private readonly IMetadataCache metadataCache;
		private readonly ISearchResultCache searchResultCache;
		private readonly ISessionManager sessionManager;
		private readonly ICacheFileSystem cacheFileSystem;
		private readonly Timer loadTimer;
		private readonly TagsModel tagsModel;
		private readonly BookmarksModel bookmarksModel;
		private readonly BrowsingModel browsingModel;
		private readonly LibraryModel libraryModel;
		private readonly GalleryModel galleryModel;
		private readonly DetailsModel detailsModel;
		private readonly TabControlEx listsTabControl;
		private readonly TabPage tagsTabPage;
		private readonly TabPage bookmarksTabPage;
		private readonly TabPage libraryTabPage;
		private readonly TabPage browsingTabPage;
		private readonly TagsToolStrip tagsToolStrip;
		private readonly TagsTreeView tagsTreeView;
		private readonly TagsFilter tagsFilter;
		private readonly BookmarksFilter bookmarksFilter;
		private readonly LibraryFilter libraryFilter;
		private readonly BrowsingFilter browsingFilter;
		private readonly LibraryBrowserFilter libraryBrowserFilter;
		private readonly GalleryBrowserFilter galleryBrowserFilter;
		private readonly DetailsBrowserFilter detailsBrowserFilter;
		private readonly BookmarksToolStrip bookmarksToolStrip;
		private readonly BookmarksTreeView bookmarksTreeView;
		private readonly LibraryToolStrip libraryToolStrip;
		private readonly LibraryTreeView libraryTreeView;
		private readonly LibraryBrowserView libraryBrowserView;
		private readonly LibraryBrowserToolStrip libraryBrowserToolStrip;
		private readonly GalleryBrowserToolStrip galleryToolStrip;
		private readonly GalleryBrowserView galleryBrowserView;
		private readonly DetailsBrowserToolStrip detailsToolStrip;
		private readonly DetailsBrowserView detailsBrowserView;
		private readonly BrowsingToolStrip browsingToolStrip;
		private readonly BrowsingTreeView browsingTreeView;
		private readonly TabControlEx mainViewTabControl;
		private readonly TabPage galleryBrowserViewTabPage;
		private readonly TabPage libraryBrowserViewTabPage;
		private readonly TabPage downloadsListViewTabPage;
		private readonly TabControlEx detailsTabControl;
		private readonly TabPage detailsTabPage;
		private readonly TabPage downloadTabPage;
		private readonly SplitContainerEx splitContainer1;
		private readonly SplitContainerEx splitContainer2;
		private readonly SplitContainerEx splitContainer3;
		private readonly WebBrowserTreeNodeToolTip webBrowserToolTip;
		private readonly StartupWebBrowserView startupWebBrowser;
		private readonly Theme theme;
		private readonly DocumentTemplates documentTemplates;
		private readonly ITokenModifier[] tokenModifiers;
		private readonly ApplicationLoader applicationLoader;
		private readonly SearchHandler searchHandler;
		private readonly BookmarkFormatter bookmarkFormatter;
		private readonly BookmarkPromptUtility bookmarkPromptUtility;
		private readonly MainMenuStrip mainMenuStrip;
		private readonly FullScreenRestoreState fullScreenRestoreState;
		private readonly StartupSpecialHandler startupSpecialHandler;
		private readonly Taskbar taskbar;

		public MainForm()
		{
			InitializeComponent();
			ProcessCommandLine();

			staticHttpClient = new StaticHttpClient(Settings.Network);

			pathFormatter = new PathFormatter(Program.ApplicationPath, Program.SourcePath, Settings.PathFormatter.Custom, Settings.PathFormatter, Settings.Lists.Tags.LanguageNames, Settings.PathFormatter.IsEnabled);

			tagsModel = new TagsModel();
			tokenModifiers = TokenModifiers.All.Concat(new ITokenModifier[] { new TagTokenModifier(tagsModel) }).ToArray();
			bookmarkFormatter = new BookmarkFormatter(Settings.Lists.Bookmarks, Settings.Lists.Tags, tagsModel, pathFormatter, tokenModifiers);
			bookmarksModel = new BookmarksModel(tagsModel, bookmarkFormatter);
			libraryModel = new LibraryModel(pathFormatter.GetCacheDirectory(), new WinformsTimer(0, Settings.Polling.LibraryRefreshInterval));
			browsingModel = new BrowsingModel();
			galleryModel = new GalleryModel(tagsModel);
			detailsModel = new DetailsModel();

			metadataKeywordLists = new MetadataKeywordLists();
			metadataProcessors = new List<IMetadataProcessor>();
			metadataConverters = new List<IMetadataConverter>();
			archiveWriters = new List<IArchiveWriter>();
			queryParser = new QueryParser(tagsModel);

			coreTextFormatter = new CoreTextFormatter(pathFormatter, tokenModifiers);
			aboutTextFormatter = new AboutTextFormatter(pathFormatter);
			tagTextFormatter = new TagTextFormatter();
			metadataTextFormatter = new MetadataTextFormatter(pathFormatter, Settings.Lists.Tags.LanguageNames, metadataKeywordLists, tokenModifiers);

			documentTemplates = new DocumentTemplates();
			documentTemplates.About = new DocumentTemplate<object>("about", Settings.Cache.Templates, pathFormatter, (x, context) => aboutTextFormatter.Format(x));
			documentTemplates.Details = new DocumentTemplate<Metadata>("details", Settings.Cache.Templates, pathFormatter, (text, context) => metadataTextFormatter.Format(text, context));
			documentTemplates.DetailsPreload = new DocumentTemplate<Metadata>("details-preload", Settings.Cache.Templates, pathFormatter, (text, context) => metadataTextFormatter.Format(text, context));
			documentTemplates.Download = new DocumentTemplate<Metadata>("download", Settings.Cache.Templates, pathFormatter, (text, context) => metadataTextFormatter.Format(text, context));
			documentTemplates.GalleryTooltip = new DocumentTemplate<Metadata>("gallery-tooltip", Settings.Cache.Templates, pathFormatter, (text, context) => metadataTextFormatter.Format(text, context));
			documentTemplates.LibraryCovergrid = new DocumentTemplate<ISearchProgressArg>("library-covergrid", Settings.Cache.Templates, pathFormatter, (text, context) => searchProgressArgTextFormatter.Format(text, context, "library"));
			documentTemplates.LibraryCovergridItem = new DocumentTemplate<Metadata>("library-covergrid-item", Settings.Cache.Templates, pathFormatter, (text, context) => metadataTextFormatter.Format(text, context));
			documentTemplates.SearchCovergrid = new DocumentTemplate<ISearchProgressArg>("search-covergrid", Settings.Cache.Templates, pathFormatter, (text, context) => searchProgressArgTextFormatter.Format(text, context, "search"));
			documentTemplates.SearchCovergridItem = new DocumentTemplate<Metadata>("search-covergrid-item", Settings.Cache.Templates, pathFormatter, (text, context) => metadataTextFormatter.Format(text, context));
			documentTemplates.SearchPreload = new DocumentTemplate<ISearchArg>("search-preload", Settings.Cache.Templates, pathFormatter, (text, context) => searchArgTextFormatter.Format(text, context));
			documentTemplates.Startup = new DocumentTemplate<object>("startup", Settings.Cache.Templates, pathFormatter, (text, context) => coreTextFormatter.Format(text, context));

			searchArgTextFormatter = new SearchArgTextFormatter(pathFormatter, metadataKeywordLists, tagsModel, tokenModifiers);
			searchProgressArgTextFormatter = new SearchProgressArgTextFormatter(pathFormatter, metadataKeywordLists, tagsModel, metadataTextFormatter, tokenModifiers, documentTemplates.SearchCovergridItem, documentTemplates.LibraryCovergridItem);

			metadataCache = new MetadataCache();
			searchResultCache = new SearchResultCache(metadataCache);
			sessionManager = new SessionManager(pathFormatter, searchResultCache, Settings.Cache.Session.RecentLifeSpan, Settings.Cache.Session.SearchLifeSpan, Settings.Cache.Session.TaggedLifeSpan, Settings.Network);
			cacheFileSystem = new CacheFileSystem(pathFormatter, searchResultCache, archiveWriters);
			pluginSystem = new PluginSystem(Logger, pathFormatter, cacheFileSystem, archiveWriters, metadataConverters, metadataProcessors);

			backgroundTaskWorker = new BackgroundTaskWorker() { IdleWaitTime = Settings.BackgroundWorkers.BackgroundTaskWorker.IdleWaitTime, MaxConcurrentJobCount = Settings.BackgroundWorkers.BackgroundTaskWorker.MaxConcurrentJobCount };
			pageDownloader = new PageDownloader(staticHttpClient.Client, pathFormatter, searchResultCache, cacheFileSystem) { IdleWaitTime = Settings.BackgroundWorkers.PageDownloader.IdleWaitTime, MaxConcurrentJobCount = Settings.BackgroundWorkers.PageDownloader.MaxConcurrentJobCount };
			coverDownloader = new CoverDownloader(staticHttpClient.Client, pathFormatter, metadataKeywordLists) { IdleWaitTime = Settings.BackgroundWorkers.CoverDownloader.IdleWaitTime, MaxConcurrentJobCount = Settings.BackgroundWorkers.CoverDownloader.MaxConcurrentJobCount };
			coverLoader = new CoverDownloader(staticHttpClient.Client, pathFormatter, metadataKeywordLists) { IdleWaitTime = Settings.BackgroundWorkers.CoverLoader.IdleWaitTime, MaxConcurrentJobCount = Settings.BackgroundWorkers.CoverLoader.MaxConcurrentJobCount };
			galleryDownloader = new GalleryDownloader(staticHttpClient.Client, pathFormatter, searchResultCache) { IdleWaitTime = Settings.BackgroundWorkers.GalleryDownloader.IdleWaitTime, MaxConcurrentJobCount = Settings.BackgroundWorkers.GalleryDownloader.MaxConcurrentJobCount };

			detailsTabControl = new TabControlEx();
			detailsTabPage = new TabPage();
			downloadTabPage = new TabPage();
			listsTabControl = new TabControlEx();
			tagsTabPage = new TabPage();
			bookmarksTabPage = new TabPage();
			libraryTabPage = new TabPage();
			browsingTabPage = new TabPage();
			mainViewTabControl = new TabControlEx();
			galleryBrowserViewTabPage = new TabPage();
			libraryBrowserViewTabPage = new TabPage();
			downloadsListViewTabPage = new TabPage();
			webBrowserToolTip = new WebBrowserTreeNodeToolTip(pathFormatter, documentTemplates.GalleryTooltip);
			loadTimer = new Timer();

			searchHandler = new SearchHandler(
				libraryModel
				, mainViewTabControl
				, libraryBrowserViewTabPage
				, galleryBrowserViewTabPage
				, Settings.Cache
				, searchResultCache
				, Logger
				, browsingModel
				, pathFormatter
				, staticHttpClient.Client
				, queryParser
				, galleryModel
				, detailsModel
				, galleryDownloader
				, sessionManager
				, Settings.Network);

			bookmarkPromptUtility = new BookmarkPromptUtility(bookmarksModel, Settings.Lists.Bookmarks);

			tagsFilter = new TagsFilter(Settings.Lists.Tags, metadataKeywordLists);
			bookmarksFilter = new BookmarksFilter(Settings.Lists.Bookmarks);
			libraryFilter = new LibraryFilter(Settings.Lists.Library);
			browsingFilter = new BrowsingFilter(Settings.Lists.Browsing);
			libraryBrowserFilter = new LibraryBrowserFilter(Settings.Library.Browser);
			galleryBrowserFilter = new GalleryBrowserFilter(Settings.Gallery.Browser);
			detailsBrowserFilter = new DetailsBrowserFilter(Settings.Details.Browser);

			tagsTreeView = new TagsTreeView(tagsFilter, tagsModel, tagTextFormatter, Settings.Lists.Tags, metadataKeywordLists, searchHandler, bookmarkPromptUtility, staticHttpClient.Client);
			tagsToolStrip = new TagsToolStrip(tagsFilter, tagsModel, Settings.Lists.Tags, metadataKeywordLists, Settings.Polling.FilterDelay);
			bookmarksTreeView = new BookmarksTreeView(bookmarksFilter, bookmarksModel, Settings.Lists.Bookmarks, webBrowserToolTip, queryParser, cacheFileSystem, searchHandler);
			bookmarksToolStrip = new BookmarksToolStrip(bookmarksFilter, bookmarksModel, Settings.Lists.Bookmarks, Settings.Polling.FilterDelay);
			libraryTreeView = new LibraryTreeView(libraryFilter, libraryModel, archiveWriters, webBrowserToolTip, searchHandler, cacheFileSystem);
			libraryToolStrip = new LibraryToolStrip(libraryFilter, libraryModel, Settings.Lists.Library, Settings.Polling.FilterDelay);
			browsingTreeView = new BrowsingTreeView(browsingFilter, browsingModel, Settings.Lists.Browsing, searchHandler, sessionManager, queryParser);
			browsingToolStrip = new BrowsingToolStrip(browsingFilter, browsingModel, Settings.Lists.Browsing, Settings.Polling.FilterDelay);

			galleryBrowserView = new GalleryBrowserView(galleryBrowserFilter, galleryModel, documentTemplates.SearchCovergrid, documentTemplates.SearchPreload, pathFormatter, Settings.Gallery.Browser, pageDownloader, coverDownloader, metadataKeywordLists, tagsModel, searchHandler);
			galleryToolStrip = new GalleryBrowserToolStrip(galleryBrowserFilter, galleryModel, Settings.Gallery.Browser, searchHandler);
			libraryBrowserView = new LibraryBrowserView(libraryBrowserFilter, libraryModel, documentTemplates.LibraryCovergrid, documentTemplates.SearchPreload, documentTemplates.LibraryCovergridItem, pathFormatter, pageDownloader, coverLoader, metadataKeywordLists, Settings.Library.Browser, searchResultCache);
			libraryBrowserToolStrip = new LibraryBrowserToolStrip(libraryBrowserFilter, libraryModel, Settings.Library.Browser, searchHandler);
			detailsToolStrip = new DetailsBrowserToolStrip(detailsBrowserFilter, detailsModel, searchHandler);
			detailsBrowserView = new DetailsBrowserView(detailsBrowserFilter, detailsModel, documentTemplates.Details, documentTemplates.Download, documentTemplates.DetailsPreload, galleryDownloader, pageDownloader, coverDownloader, metadataKeywordLists, Settings.Details.Browser, searchResultCache, cacheFileSystem);
			theme = new Theme();
			applicationLoader = new ApplicationLoader();
			fullScreenRestoreState = new FullScreenRestoreState();
			mainMenuStrip = new MainMenuStrip(Settings);
			startupSpecialHandler = new StartupSpecialHandler(Settings.Gallery, tagsModel, metadataKeywordLists, searchHandler);
			taskbar = new Taskbar(coverDownloader, galleryDownloader, pageDownloader, searchResultCache, cacheFileSystem);

			publicApi = new PublicApi(staticHttpClient.Client
				, pathFormatter
				, cacheFileSystem
				, searchResultCache
				, searchHandler
				, bookmarkPromptUtility
				, pluginSystem
				, libraryBrowserToolStrip
				, Settings.Library
				, galleryToolStrip
				, Settings.Gallery
				, Settings.Details
				, galleryDownloader
				, coverDownloader
				, pageDownloader
				, Settings.Notifications
				, metadataKeywordLists
				, Settings
				);

			splitContainer1 = new SplitContainerEx();
			splitContainer2 = new SplitContainerEx();
			splitContainer3 = new SplitContainerEx();

			splitContainer1.BeginInit();
			splitContainer1.Panel2.SuspendLayout();
			splitContainer1.SuspendLayout();

			splitContainer2.BeginInit();
			splitContainer2.Panel1.SuspendLayout();
			splitContainer2.SuspendLayout();

			splitContainer3.BeginInit();
			splitContainer3.SuspendLayout();

			SuspendLayout();

			//
			//
			//
			loadTimer.Interval = 100;
			loadTimer.Tick += LoadTimer_Tick;

			//
			//
			//
			tagsTabPage.Name = "tagsTabPage";
			tagsTabPage.TabIndex = 0;
			tagsTabPage.Tag = "tags";
			tagsTabPage.Text = "Tags";
			tagsTabPage.UseVisualStyleBackColor = true;

			bookmarksTabPage.Name = "bookmarksTabPage";
			bookmarksTabPage.TabIndex = 1;
			bookmarksTabPage.Tag = "bookmarks";
			bookmarksTabPage.Text = "Bookmarks";
			bookmarksTabPage.UseVisualStyleBackColor = true;

			libraryTabPage.Name = "libraryTabPage";
			libraryTabPage.TabIndex = 2;
			libraryTabPage.Tag = "library";
			libraryTabPage.Text = "Library";
			libraryTabPage.UseVisualStyleBackColor = true;

			browsingTabPage.Name = "browsingTabPage";
			browsingTabPage.TabIndex = 4;
			browsingTabPage.Tag = "browsing";
			browsingTabPage.Text = "Browsing";
			browsingTabPage.UseVisualStyleBackColor = true;

			//
			//
			//
			listsTabControl.Dock = DockStyle.Fill;
			listsTabControl.Name = "treeViewTabControl";
			listsTabControl.SelectedIndex = -1;
			listsTabControl.TabIndex = 2;
			listsTabControl.TabPages.Add(tagsTabPage);
			listsTabControl.TabPages.Add(bookmarksTabPage);
			listsTabControl.TabPages.Add(libraryTabPage);
			listsTabControl.TabPages.Add(browsingTabPage);
			listsTabControl.Selected += TreeViewTabControl_Selected;

			splitContainer3.Panel1.Controls.Add(listsTabControl);

			//
			//
			//
			tagsTreeView.Dock = DockStyle.Fill;

			//
			//
			//
			tagsToolStrip.Dock = DockStyle.Top;
			tagsToolStrip.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			tagsToolStrip.AutoSize = true;

			//
			//
			//
			tagsTabPage.Controls.Add(tagsTreeView);
			tagsTabPage.Controls.Add(tagsToolStrip);

			//
			//
			//
			bookmarksModel.ItemAdded += BookmarksModel_ItemAdded;
			bookmarksModel.ItemChanged += BookmarksModel_ItemChanged;

			//
			//
			//
			bookmarksTreeView.Dock = DockStyle.Fill;

			//
			//
			//
			bookmarksToolStrip.Dock = DockStyle.Top;
			bookmarksToolStrip.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			bookmarksToolStrip.AutoSize = true;

			//
			//
			//
			bookmarksTabPage.Controls.Add(bookmarksTreeView);
			bookmarksTabPage.Controls.Add(bookmarksToolStrip);

			//
			//
			//
			libraryTreeView.Dock = DockStyle.Fill;

			//
			//
			//
			libraryToolStrip.Dock = DockStyle.Top;
			libraryToolStrip.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			libraryToolStrip.AutoSize = true;

			//
			//
			//
			libraryTabPage.Controls.Add(libraryTreeView);
			libraryTabPage.Controls.Add(libraryToolStrip);

			//
			//
			//
			browsingModel.ItemAdded += BrowsingModel_ItemAdded;
			browsingModel.ItemChanged += BrowsingModel_ItemChanged;

			//
			//
			//
			browsingToolStrip.Dock = DockStyle.Top;
			browsingToolStrip.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			browsingToolStrip.AutoSize = true;

			//
			//
			//
			browsingTreeView.Dock = DockStyle.Fill;

			//
			//
			//
			browsingTabPage.Controls.Add(browsingTreeView);
			browsingTabPage.Controls.Add(browsingToolStrip);

			//
			//
			//
			galleryBrowserViewTabPage.Name = "galleryBrowserViewTabPage";
			galleryBrowserViewTabPage.Padding = Padding.Empty;
			galleryBrowserViewTabPage.TabIndex = 0;
			galleryBrowserViewTabPage.Text = "Search";
			galleryBrowserViewTabPage.UseVisualStyleBackColor = true;

			//
			//
			//
			libraryBrowserViewTabPage.Name = "libraryBrowserViewTabPage";
			libraryBrowserViewTabPage.TabIndex = 1;
			libraryBrowserViewTabPage.Text = "Library";
			libraryBrowserViewTabPage.UseVisualStyleBackColor = true;

			//
			//
			//
			downloadsListViewTabPage.Name = "downloadsListViewTabPage";
			downloadsListViewTabPage.TabIndex = 2;
			downloadsListViewTabPage.Text = "Downloads";
			downloadsListViewTabPage.UseVisualStyleBackColor = true;

			//
			//
			//
			mainViewTabControl.Controls.Add(galleryBrowserViewTabPage);
			mainViewTabControl.Controls.Add(libraryBrowserViewTabPage);
			mainViewTabControl.Controls.Add(downloadsListViewTabPage);
			mainViewTabControl.Dock = DockStyle.Fill;
			mainViewTabControl.Name = "mainViewTabControl";
			mainViewTabControl.SelectedIndex = 0;
			mainViewTabControl.TabIndex = 2;
			mainViewTabControl.Selected += MainViewTabControl_Selected;

			splitContainer3.Panel2.Controls.Add(mainViewTabControl);
			splitContainer2.Panel2.Controls.Add(detailsTabControl);

			// 
			// detailsTabControl
			// 
			detailsTabControl.Controls.Add(detailsTabPage);
			detailsTabControl.Controls.Add(downloadTabPage);
			detailsTabControl.Dock = DockStyle.Fill;
			detailsTabControl.Name = "detailsTabControl";
			detailsTabControl.SelectedIndex = 0;
			detailsTabControl.TabIndex = 2;

			// 
			// detailsTabPage
			// 
			detailsTabPage.Name = "detailsTabPage";
			detailsTabPage.TabIndex = 0;
			detailsTabPage.Text = "Details";
			detailsTabPage.UseVisualStyleBackColor = true;

			// 
			// downloadTabPage
			// 
			downloadTabPage.Name = "downloadTabPage";
			downloadTabPage.TabIndex = 1;
			downloadTabPage.Text = "Download";
			downloadTabPage.UseVisualStyleBackColor = true;

			//
			//
			//
			galleryBrowserView.Dock = DockStyle.Fill;
			galleryBrowserView.WebBrowser.ObjectForScripting = publicApi;

			//
			//
			//
			galleryToolStrip.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			galleryToolStrip.AutoSize = true;
			galleryToolStrip.Dock = DockStyle.Top;

			//
			//
			//
			galleryBrowserViewTabPage.Controls.Add(galleryBrowserView);
			galleryBrowserViewTabPage.Controls.Add(galleryToolStrip);

			//
			//
			//
			libraryBrowserView.Dock = DockStyle.Fill;
			libraryBrowserView.WebBrowser.ObjectForScripting = publicApi;

			//
			//
			//
			libraryBrowserToolStrip.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			libraryBrowserToolStrip.AutoSize = true;
			libraryBrowserToolStrip.Dock = DockStyle.Top;

			//
			//
			//
			libraryBrowserViewTabPage.Controls.Add(libraryBrowserView);
			libraryBrowserViewTabPage.Controls.Add(libraryBrowserToolStrip);

			//
			//
			//
			detailsToolStrip.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			detailsToolStrip.AutoSize = true;
			detailsToolStrip.Dock = DockStyle.Top;

			//
			//
			//
			detailsBrowserView.Dock = DockStyle.Fill;
			detailsBrowserView.WebBrowser.ObjectForScripting = publicApi;

			//
			//
			//
			detailsTabPage.Controls.Add(detailsBrowserView);
			detailsTabPage.Controls.Add(detailsToolStrip);

			// 
			// splitContainer1
			// 
			splitContainer1.Dock = DockStyle.Fill;
			splitContainer1.FixedPanel = FixedPanel.Panel1;
			splitContainer1.Margin = new Padding(0);
			splitContainer1.Name = "splitContainer1";
			splitContainer1.Orientation = Orientation.Horizontal;
			splitContainer1.Panel1MinSize = 22;

			// 
			// splitContainer1.Panel2
			// 
			splitContainer1.Panel2.Controls.Add(splitContainer2);
			splitContainer1.Size = new Size(1364, 637);
			splitContainer1.SplitterDistance = 25;
			splitContainer1.SplitterIncrement = 22;
			splitContainer1.SplitterWidth = 7;
			splitContainer1.TabIndex = 2;

			// 
			// splitContainer2
			// 
			splitContainer2.Dock = DockStyle.Fill;
			splitContainer2.FixedPanel = FixedPanel.Panel2;
			splitContainer2.Margin = new Padding(0);
			splitContainer2.Name = "splitContainer2";

			// 
			// splitContainer2.Panel1
			// 
			splitContainer2.Panel1.Controls.Add(splitContainer3);
			splitContainer2.Size = new Size(1364, 605);
			splitContainer2.SplitterDistance = 1214;
			splitContainer2.SplitterWidth = 7;
			splitContainer2.TabIndex = 1;

			// 
			// splitContainer3
			// 
			splitContainer3.Dock = DockStyle.Fill;
			splitContainer3.FixedPanel = FixedPanel.Panel1;
			splitContainer3.Margin = new Padding(0);
			splitContainer3.Name = "splitContainer3";
			splitContainer3.Size = new Size(1214, 605);
			splitContainer3.SplitterDistance = 200;
			splitContainer3.SplitterWidth = 7;
			splitContainer3.TabIndex = 0;

			//
			// applicationMenuStrip
			//
			mainMenuStrip.Exit += MainMenuStrip_Exit;
			mainMenuStrip.ToggleListsPanel += MainMenuStrip_ToggleListsPanel;
			mainMenuStrip.ToggleDetailsPanel += MainMenuStrip_ToggleDetailsPanel;
			mainMenuStrip.ToggleFullScreen += MainMenuStrip_ToggleFullScreen;
			mainMenuStrip.ShowAbout += MainMenuStrip_ShowAbout;
			mainMenuStrip.ShowPlugins += MainMenuStrip_ShowPlugins;

			//
			// this
			//
			Controls.Add(splitContainer1);
			Controls.Add(mainMenuStrip);
			Controls.Add(webBrowserToolTip);
			webBrowserToolTip.BringToFront();
			MainMenuStrip = mainMenuStrip;
			Padding = new Padding(0);
			Text = aboutTextFormatter.Format(Settings.Window.TextFormat);
			if (Settings.Network.Offline)
			{
				Text += " [OFFLINE]";
			}
			Enabled = false;
			
			//
			// splash screen
			//
			if (Settings.SplashScreen.IsVisible)
			{
				startupWebBrowser = new StartupWebBrowserView(coreTextFormatter, documentTemplates.Startup, applicationLoader);
				startupWebBrowser.Name = "startupWebBrowserView";
				startupWebBrowser.Dock = DockStyle.Fill;
				startupWebBrowser.WebBrowser.DocumentCompleted += WebBrowser_DocumentCompleted;

				Controls.Add(startupWebBrowser);
				startupWebBrowser.BringToFront();

				// avoid flickering
				listsTabControl.Visible = false;
				mainViewTabControl.Visible = false;
				detailsTabControl.Visible = false;
			}

			backgroundTaskWorker.ProgressChanged += TaskBackgroundWorker_ProgressChanged;

			pageDownloader.PagesDownloadCompleted += PageDownloader_PagesDownloadCompleted;

			ReadTheme();
			ApplyTheme();
			ApplyVisualStyles();

			splitContainer1.Panel2.ResumeLayout(false);
			splitContainer1.EndInit();
			splitContainer1.ResumeLayout(false);

			splitContainer2.Panel1.ResumeLayout(false);
			splitContainer2.EndInit();
			splitContainer2.ResumeLayout(false);

			splitContainer3.EndInit();
			splitContainer3.ResumeLayout(false);
			
			ResumeLayout(false);
			PerformLayout();
		}

		private void ProcessCommandLine()
		{
			string[] args = Environment.GetCommandLineArgs();

			Settings.Network.Offline = false;

			for (int i = 1; i < args.Length; ++i)
			{
				if (args[i].Equals("--offline", StringComparison.OrdinalIgnoreCase))
				{
					Settings.Network.Offline = true;
					break;
				}
			}
		}

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams createParams = base.CreateParams;

				if (Settings.Window.UseCompositeStyle)
				{
					createParams.ExStyle |= 0x02000000;
				}

				return createParams;
			}
		}
		
		// doesn't work.
		/*
		private void ListsPanel_VisibleChanged(object sender, EventArgs e)
		{
			settings.Panels.Lists.IsCollapsed = !splitContainer3.Panel1.Visible;	//splitContainer3.Panel1Collapsed;
		}

		private void DetailsPanel_VisibleChanged(object sender, EventArgs e)
		{
			settings.Panels.Details.IsCollapsed = !splitContainer2.Panel2.Visible;	//splitContainer2.Panel2Collapsed;
		}
		*/

		private void PageDownloader_PagesDownloadCompleted(object sender, PageDownloadCompletedEventArgs e)
		{
			if (Settings.Plugins.Archive.AutoCreate
				&& archiveWriters.Count > 0)
			{
				pluginSystem.CreateArchive(e.Metadata, Settings.Plugins.Archive.AutoDeletePagesFolder);
			}

			if (Settings.Plugins.ConvertMetadata.AutoCreate
				&& metadataConverters.Count > 0)
			{
				pluginSystem.ConvertMetadata(e.Metadata);
			}
		}

		private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			StartSetup();
		}

		private void TreeViewTabControl_Selected(object sender, TabControlEventArgs e)
		{
			if (e.TabPage == tagsTabPage)
			{
				tagsToolStrip.Filter();
			}
			else
			{
				tagsTreeView.TreeView.Nodes.Clear();
			}

			if (e.TabPage == bookmarksTabPage)
			{
				bookmarksToolStrip.Filter();
			}
			else
			{
				bookmarksTreeView.TreeView.Nodes.Clear();
			}

			if (e.TabPage == libraryTabPage)
			{
				libraryModel.Start();
				libraryToolStrip.Filter();
				libraryModel.EnablePolling();
			}
			else
			{
				libraryModel.Stop();
				libraryModel.DisablePolling();
				libraryTreeView.TreeView.Nodes.Clear();
			}

			if (e.TabPage == browsingTabPage)
			{
				browsingToolStrip.Filter();
			}
			else
			{
				browsingTreeView.TreeView.Nodes.Clear();
			}
		}

		private void MainViewTabControl_Selected(object sender, TabControlEventArgs e)
		{
			if (e.TabPage == libraryBrowserViewTabPage)
			{
				libraryBrowserView.Search();
			}
			else
			{
				// free up memory.
				libraryBrowserView.WebBrowser.DocumentText = "";
			}
		}

		private void MainForm_Shown(object sender, EventArgs e)
		{
			if (!Settings.SplashScreen.IsVisible)
			{
				StartSetup();
			}
		}

		private void Application_ApplicationExit(object sender, EventArgs e)
		{
			WriteUserData();
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (galleryBrowserView.WebBrowser != null
				&& galleryBrowserView.WebBrowser.Focused)
			{
				switch (keyData)
				{
					case Keys.Return:
					case Keys.F5:
						return true;
				}
			}
			else if (detailsBrowserView.WebBrowser != null
				&& detailsBrowserView.WebBrowser.Focused)
			{
				switch (keyData)
				{
					case Keys.Return:
					case Keys.F5:
						return true;
				}
			}
			else if (libraryBrowserView.WebBrowser != null
				&& libraryBrowserView.WebBrowser.Focused)
			{
				switch (keyData)
				{
					case Keys.Return:
					case Keys.F5:
						return true;
				}
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}

		// FIXME: not working properly yet.
		/*
		public bool PreFilterMessage(ref Message m)
		{
			if (FilterMessage(galleryBrowserView.WebBrowser, ref m)
				|| FilterMessage(detailsBrowserView.WebBrowser, ref m)
				|| FilterMessage(libraryBrowserView.WebBrowser, ref m))
			{
				return true;
			}

			return false;
		}

		private bool FilterMessage(WebBrowser target, ref Message m)
		{
			// browsers should respond to mousewheel events while the mouse is over the controls (regardless of whether they currently have input focus or not)

			if (m.Msg == User32.NativeMethods.WM_MOUSEWHEEL)
			{
				if (target != null)
				{
					//Point point = new Point((int)(lParam) & 0xFFFF, ((int)(lParam) >> 16) & 0xFFFF);

					if (target.Bounds.Contains(PointToClient(Cursor.Position))
						&& !target.Focused)
					{
						var handle = target.Handle;
						handle = User32.NativeMethods.FindWindowEx(handle, IntPtr.Zero, "Shell Embedding", null);
						handle = User32.NativeMethods.FindWindowEx(handle, IntPtr.Zero, "Shell DocObject View", null);
						handle = User32.NativeMethods.FindWindowEx(handle, IntPtr.Zero, "Internet Explorer_Server", null);
						User32.NativeMethods.SendMessage(handle, m.Msg, m.WParam, m.LParam);

						return true;
					}
				}
			}
			
			return false;
		}
		*/
	}
	/*
	internal partial class User32
	{
		private User32() { }

		internal partial class NativeMethods
		{
			private NativeMethods() { }

			internal const Int32 WM_MOUSEWHEEL = 0x20A;

			[DllImport("user32.dll"), CharSet=CharSet.Auto]
			internal static extern IntPtr SendMessage(IntPtr hWnd, Int32 Msg, IntPtr wParam, IntPtr lParam);

			[DllImport("user32.dll"), SetLastError=true]
			internal static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, String className, String windowName);
		}
	}
	*/
	public class FullScreenRestoreState
	{
		public FormWindowState WindowState { get; set; }
		public bool? IsListPanelVisible { get; set; }
		public bool? IsDetailsPanelVisible { get; set; }
	}
}
