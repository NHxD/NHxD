using Ash.System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	public partial class MainForm : Form
	{
		private int loadPass;

		private void LoadTimer_Tick(object sender, EventArgs e)
		{
			try
			{
				string loadPassName = "";

				switch (loadPass)
				{
					case 0:
						loadPassName = "Initializing HTTP...";
						Logger.TraceLine(loadPassName);

						Cursor.Current = Cursors.WaitCursor;
						InitializeHttp();
						break;

					case 1:
						loadPassName = "Loading tag database...";
						Logger.TraceLine(loadPassName);

						ReadTags();
						break;

					case 2:
						loadPassName = "Loading user lists...";
						Logger.TraceLine(loadPassName);

						ReadWhitelist();
						ReadBlacklist();
						ReadIgnorelist();
						ReadHidelist();
						break;

					case 3:
						loadPassName = "Loading user bookmarks...";
						Logger.TraceLine(loadPassName);

						ReadBookmarks();
						break;

					case 4:
						loadPassName = "Loading user search history...";
						Logger.TraceLine(loadPassName);

						ReadSearchHistory();
						break;

					case 5:
						loadPassName = "Loading visited links...";
						Logger.TraceLine(loadPassName);

						ReadVisitedSearchHistory();
						ReadVisitedGalleryHistory();
						break;

					case 6:
						loadPassName = "Loading user filters...";
						Logger.TraceLine(loadPassName);

						ReadSearchFilters();
						ReadLibraryFilters();
						break;

					case 7:
						loadPassName = "Loading archive writer plugins...";
						Logger.TraceLine(loadPassName);

						pluginSystem.LoadPlugins(archiveWriters, Settings.Plugins.ArchiveWriters);
						break;

					case 8:
						loadPassName = "Loading metadata converter plugins...";
						Logger.TraceLine(loadPassName);

						pluginSystem.LoadPlugins(metadataConverters, Settings.Plugins.MetadataConverters);
						break;

					case 9:
						loadPassName = "Loading metadata processor plugins...";
						Logger.TraceLine(loadPassName);

						pluginSystem.LoadPlugins(metadataProcessors, Settings.Plugins.MetadataProcessors);
						break;

					case 10:
						loadPassName = "Applying settings...";
						Logger.TraceLine(loadPassName);

						ApplySettings();

						if (Settings.SplashScreen.IsVisible)
						{
							listsTabControl.Visible = true;
							mainViewTabControl.Visible = true;
							detailsTabControl.Visible = true;
						}
						break;

					case 11:
						loadPassName = "Deleting expired sessions...";
						Logger.TraceLine(loadPassName);

						if (Settings.Cache.Session.CheckAtStartup)
						{
							sessionManager.DeleteExpiredSessions();
						}
						break;

					case 12:
						loadPassName = "Starting working threads...";
						Logger.TraceLine(loadPassName);

						//Application.AddMessageFilter(this);
						Application.ApplicationExit += Application_ApplicationExit;

						backgroundTaskWorker.Run();
						pageDownloader.Run();
						coverDownloader.Run();
						coverLoader.Run();
						galleryDownloader.Run();
						break;

					case 14:
						loadPassName = "Loading static metadata cache...";
						Logger.TraceLine(loadPassName);

						if (Settings.Cache.MetadataCache.BuildAtStartup)
						{
							metadataCacheSnapshot.AggregateFromFiles();
							metadataCacheSnapshot.SaveToFile();
						}

						if (Settings.Cache.MetadataCache.LoadAtStartup)
						{
							if (!metadataCacheSnapshot.IsReady)
							{
								metadataCacheSnapshot.LoadFromFile();
							}
						}
						break;

					case 15:
						loadPassName = "Enabling form...";
						Logger.TraceLine(loadPassName);

						Enabled = true;
						break;

					case 16:
						loadPassName = "Removing splash screen...";
						Logger.TraceLine(loadPassName);

						if (Settings.SplashScreen.IsVisible)
						{
							startupWebBrowser.SendToBack();
							startupWebBrowser.WebBrowser.DocumentText = "";
							startupWebBrowser.Visible = false;
							Controls.Remove(startupWebBrowser);
						}
						break;

					case 17:
						loadPassName = "Starting up...";
						Logger.TraceLine(loadPassName);
						Logger.WriteSeparator();

						loadTimer.Stop();
						StartUp();
						Cursor.Current = Cursors.Default;
						break;
				}

				++loadPass;

				applicationLoader.SetProgress((int)(loadPass / 15.0f * 100.0f), loadPassName);
			}
			catch (Exception ex)
			{
				loadTimer.Stop();

				MessageBox.Show(ex.Message, "Internal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Logger.FatalLineFormat(ex.ToString());

				Application.Exit();
			}
		}

		private void StartSetup()
		{
			splitContainer1.SplitterDistance = Settings.SplitterDistances.Console;
			splitContainer2.SplitterDistance = Settings.SplitterDistances.Details;
			splitContainer3.SplitterDistance = Settings.SplitterDistances.Lists;
			splitContainer3.Panel1Collapsed = Settings.Panels.Lists.IsCollapsed;
			splitContainer2.Panel2Collapsed = Settings.Panels.Details.IsCollapsed;
			splitContainer1.Panel1Collapsed = Settings.Panels.Console.IsCollapsed;

			taskbar.WindowHandle = Handle;

			loadTimer.Start();
		}

		private void StartUp()
		{
			RestoreDefaultTabs();

			bool wasSpecialDate = startupSpecialHandler.Execute();

			if (!wasSpecialDate)
			{
				if (Settings.Gallery.StartupAction == Configuration.GalleryStartupAction.Custom)
				{
					searchHandler.ParseAndExecuteSearchText(Settings.Gallery.StartupUrl);
				}
				else if (Settings.Gallery.StartupAction == Configuration.GalleryStartupAction.Recent
					|| galleryModel.Searches.Count == 0)
				{
					searchHandler.ParseAndExecuteSearchText("recent");
				}
				else if (Settings.Gallery.StartupAction == Configuration.GalleryStartupAction.Continue)
				{
					if (galleryModel.Searches.Count > 0)
					{
						searchHandler.ParseAndExecuteSearchText(galleryModel.Searches[0]);
					}
				}
			}

			if (Settings.Details.StartupAction == Configuration.DetailsStartupAction.Continue)
			{
				if (detailsModel.Searches.Count > 0)
				{
					searchHandler.ShowDetails(detailsModel.Searches[0]);
				}
			}
		}

		private void RestoreDefaultTabs()
		{
			if (!string.IsNullOrEmpty(Settings.TabControls.Lists.SelectedTab))
			{
				listsTabControl.SelectTab(Settings.TabControls.Lists.SelectedTab, true);
			}
			else if (listsTabControl.TabPages.Count > 0)
			{
				listsTabControl.SelectTab(listsTabControl.TabPages[0].Name, true);
			}

			if (!string.IsNullOrEmpty(Settings.TabControls.Browser.SelectedTab))
			{
				mainViewTabControl.SelectTab(Settings.TabControls.Browser.SelectedTab, true);
			}
			else if (mainViewTabControl.TabPages.Count > 0)
			{
				mainViewTabControl.SelectTab(mainViewTabControl.TabPages[0].Name, true);
			}

			if (!string.IsNullOrEmpty(Settings.TabControls.Details.SelectedTab))
			{
				detailsTabControl.SelectTab(Settings.TabControls.Details.SelectedTab, true);
			}
			else if (detailsTabControl.TabPages.Count > 0)
			{
				detailsTabControl.SelectTab(detailsTabControl.TabPages[0].Name, true);
			}
		}

		private void WriteUserData()
		{
			Logger.WriteSeparator();
			Logger.TraceLine("Saving user data...");

			WriteSettings();
			WriteWhitelist();
			WriteBlacklist();
			WriteIgnorelist();
			WriteHidelist();
			WriteBookmarks();
			WriteSearchHistory();
			WriteTags();
			WriteVisitedSearchHistory();
			WriteVisitedGalleryHistory();
			WriteSearchFilters();
			WriteLibraryFilters();
			WriteTheme();

			Logger.TraceLine("Done.");
		}

		private void ReadTheme()
		{
			JsonUtility.PopulateFromFile(pathFormatter.GetConfiguration("theme"), theme);
		}

		private void ApplyTheme()
		{
			// TODO: redo the theme implementation.
		}

		private void ApplyVisualStyles()
		{
			Settings.Window.Apply(this);

			webBrowserToolTip.Size = Settings.Lists.Library.ToolTip.Size;

			tagsTreeView.TreeView.TreeViewNodeSorter = new TagsTreeViewNodeSorter(Settings.Lists.Tags.SortType, Settings.Lists.Tags.SortOrder);
			libraryTreeView.TreeView.TreeViewNodeSorter = new LibraryTreeViewNodeSorter(Settings.Lists.Library.SortType, Settings.Lists.Library.SortOrder);
			browsingTreeView.TreeView.TreeViewNodeSorter = new BrowsingTreeViewNodeSorter(Settings.Lists.Browsing.SortType, Settings.Lists.Browsing.SortOrder);

			Settings.Lists.Tags.Font.Apply(tagsTreeView.TreeView);
			Settings.Lists.Bookmarks.Font.Apply(bookmarksTreeView);
			Settings.Lists.Library.Font.Apply(libraryTreeView.TreeView);
			Settings.Lists.Browsing.Font.Apply(browsingTreeView);

			Settings.Gallery.Browser.Apply(galleryBrowserView.WebBrowser);
			Settings.Library.Browser.Apply(libraryBrowserView.WebBrowser);
			Settings.Details.Browser.Apply(detailsBrowserView.WebBrowser);

			Settings.TabControls.Lists.Tags.ToolStrip.Apply(galleryToolStrip);
			Settings.TabControls.Lists.Bookmarks.ToolStrip.Apply(bookmarksToolStrip);
			Settings.TabControls.Lists.Library.ToolStrip.Apply(libraryToolStrip);
			Settings.TabControls.Lists.Browsing.ToolStrip.Apply(browsingToolStrip);

			Settings.Gallery.ToolStrip.Apply(galleryToolStrip);
			Settings.Details.ToolStrip.Apply(detailsToolStrip);

			Settings.TabControls.Lists.Tags.Apply(tagsTabPage);
			Settings.TabControls.Lists.Bookmarks.Apply(bookmarksTabPage);
			Settings.TabControls.Lists.Library.Apply(libraryTabPage);
			Settings.TabControls.Lists.Browsing.Apply(browsingTabPage);

			Settings.TabControls.Browser.Gallery.Apply(galleryBrowserViewTabPage);
			Settings.TabControls.Browser.Library.Apply(libraryBrowserViewTabPage);
			Settings.TabControls.Browser.Downloads.Apply(downloadsListViewTabPage);

			Settings.TabControls.Details.Details.Apply(detailsTabPage);
			Settings.TabControls.Details.Download.Apply(downloadTabPage);

			if (Settings.Window.FullScreen.IsActive)
			{
				EnterFullScreen();
			}
		}

		private void ApplySettings()
		{
			galleryModel.Searches.AddRange(Settings.Gallery.ToolStrip.History);
			galleryToolStrip.RebindSearchComboBox();

			galleryModel.Filters.AddRange(Settings.Gallery.ToolStrip.Filters);
			galleryToolStrip.RebindFilterComboBox();

			galleryToolStrip.SearchComboBox.SelectedIndex = -1;
			galleryToolStrip.FilterComboBox.SelectedIndex = -1;

			detailsModel.Searches.AddRange(Settings.Details.ToolStrip.History);
			detailsToolStrip.Rebind();
			detailsToolStrip.SearchComboBox.SelectedIndex = -1;

			libraryModel.Filters.AddRange(Settings.Library.ToolStrip.Filters);
			libraryBrowserToolStrip.RebindFilterComboBox();
			libraryBrowserToolStrip.FilterComboBox.SelectedIndex = -1;
		}

		private void InitializeHttp()
		{
			if (Settings.Network.Offline)
			{
				return;
			}

			bool hasCustomMaxIdleTime = Settings.Network.MaxIdleTime > Timeout.Infinite;
			bool hasCustomConnectionLeaseTimeout = Settings.Network.ConnectionLeaseTimeout > Timeout.Infinite;

			if (!string.IsNullOrEmpty(Settings.Network.Client.UserAgent))
			{
				staticHttpClient.Client?.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Settings.Network.Client.UserAgent);
				staticHttpClient.GenericClient?.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", Settings.Network.Client.UserAgent);
			}

			if (staticHttpClient.Client != null)
			{
				staticHttpClient.Client.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue() { NoCache = true, NoStore = true };
			}

			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

			if (hasCustomMaxIdleTime || hasCustomConnectionLeaseTimeout)
			{
				Uri address = new Uri("https://nhentai.net/");

				ServicePoint servicePoint = ServicePointManager.FindServicePoint(address, staticHttpClient.WebProxy);

				if (hasCustomMaxIdleTime)
				{
					servicePoint.MaxIdleTime = Settings.Network.MaxIdleTime;
				}

				if (hasCustomConnectionLeaseTimeout)
				{
					servicePoint.ConnectionLeaseTimeout = Settings.Network.ConnectionLeaseTimeout;
				}
			}

			if (Settings.Network.ConnectionTimeout > 0)
			{
				if (staticHttpClient.Client != null)
				{
					staticHttpClient.Client.Timeout = TimeSpan.FromMilliseconds(Settings.Network.ConnectionTimeout);
				}
			}
		}

		private bool ReadTags()
		{
			string cachedTagsFilePath = pathFormatter.GetConfiguration("tags");

			JsonUtility.PopulateFromFile(cachedTagsFilePath, tagsModel.AllTags);

			if (tagsModel.AllTags != null)
			{
				foreach (TagInfo tag in tagsModel.AllTags)
				{
					tag.Name = tag.Name.Replace('-', ' ');
				}
			}

			return true;
		}

		private void ReadWhitelist()
		{
			JsonUtility.PopulateFromFile(pathFormatter.GetConfiguration("whitelist"), metadataKeywordLists.Whitelist);
		}

		private void ReadBlacklist()
		{
			JsonUtility.PopulateFromFile(pathFormatter.GetConfiguration("blacklist"), metadataKeywordLists.Blacklist);
		}

		private void ReadIgnorelist()
		{
			JsonUtility.PopulateFromFile(pathFormatter.GetConfiguration("ignorelist"), metadataKeywordLists.Ignorelist);
		}

		private void ReadHidelist()
		{
			JsonUtility.PopulateFromFile(pathFormatter.GetConfiguration("hidelist"), metadataKeywordLists.Hidelist);
		}

		private void ReadBookmarks()
		{
			if (!JsonUtility.PopulateFromFile(pathFormatter.GetConfiguration("bookmark-folders"), bookmarksModel.BookmarkFolders))
			{
				JsonUtility.PopulateFromFile(pathFormatter.GetDefaultConfiguration("bookmark-folders"), bookmarksModel.BookmarkFolders);
			}

			JsonUtility.PopulateFromFile(pathFormatter.GetConfiguration("bookmarks"), bookmarksModel.Bookmarks);
		}

		private void ReadSearchHistory()
		{
			JsonUtility.PopulateFromFile(pathFormatter.GetConfiguration("searches"), browsingModel.SearchHistory);
		}

		private void ReadVisitedSearchHistory()
		{
			Settings.Gallery.ToolStrip.History = JsonUtility.LoadFromFile<List<string>>(pathFormatter.GetConfiguration("search-searches")) ?? new List<string>();
		}

		private void ReadVisitedGalleryHistory()
		{
			Settings.Details.ToolStrip.History = JsonUtility.LoadFromFile<List<int>>(pathFormatter.GetConfiguration("details-searches")) ?? new List<int>();
		}

		private void ReadSearchFilters()
		{
			Settings.Gallery.ToolStrip.Filters = JsonUtility.LoadFromFile<List<string>>(pathFormatter.GetConfiguration("search-filters")) ?? new List<string>();
		}

		private void ReadLibraryFilters()
		{
			Settings.Library.ToolStrip.Filters = JsonUtility.LoadFromFile<List<string>>(pathFormatter.GetConfiguration("library-filters")) ?? new List<string>();
		}

		private void WriteSettings()
		{
			Settings.Window.State = WindowState;
			Settings.Window.Location = Location;
			Settings.Window.Size = Size;

			Settings.Lists.Tags.Font.Size = tagsTreeView.TreeView.Font.Size;
			Settings.Lists.Bookmarks.Font.Size = bookmarksTreeView.Font.Size;
			Settings.Lists.Library.Font.Size = libraryTreeView.TreeView.Font.Size;
			Settings.Lists.Browsing.Font.Size = browsingTreeView.TreeView.Font.Size;

			Settings.SplitterDistances.Console = splitContainer1.SplitterDistance;
			Settings.SplitterDistances.Details = splitContainer2.SplitterDistance;
			Settings.SplitterDistances.Lists = splitContainer3.SplitterDistance;

			Settings.Gallery.ToolStrip.History = galleryModel.Searches.ToList();
			Settings.Gallery.ToolStrip.Filters = galleryModel.Filters.ToList();
			Settings.Details.ToolStrip.History = detailsModel.Searches.ToList();
			Settings.Library.ToolStrip.Filters = libraryModel.Filters.ToList();

			Settings.TabControls.Lists.SelectedTab = listsTabControl.SelectedTab.Name;
			Settings.TabControls.Browser.SelectedTab = mainViewTabControl.SelectedTab.Name;
			Settings.TabControls.Details.SelectedTab = detailsTabControl.SelectedTab.Name;

			JsonUtility.SaveToFile(Settings, pathFormatter.GetConfiguration("settings"));
		}

		private void WriteWhitelist()
		{
			JsonUtility.SaveToFile(metadataKeywordLists.Whitelist, pathFormatter.GetConfiguration("whitelist"));
		}

		private void WriteBlacklist()
		{
			JsonUtility.SaveToFile(metadataKeywordLists.Blacklist, pathFormatter.GetConfiguration("blacklist"));
		}

		private void WriteIgnorelist()
		{
			JsonUtility.SaveToFile(metadataKeywordLists.Ignorelist, pathFormatter.GetConfiguration("ignorelist"));
		}

		private void WriteHidelist()
		{
			JsonUtility.SaveToFile(metadataKeywordLists.Hidelist, pathFormatter.GetConfiguration("hidelist"));
		}

		private void WriteBookmarks()
		{
			JsonUtility.SaveToFile(bookmarksModel.BookmarkFolders, pathFormatter.GetConfiguration("bookmark-folders"));
			JsonUtility.SaveToFile(bookmarksModel.Bookmarks, pathFormatter.GetConfiguration("bookmarks"));
		}

		private void WriteSearchHistory()
		{
			JsonUtility.SaveToFile(browsingModel.SearchHistory, pathFormatter.GetConfiguration("searches"));
		}

		private void WriteVisitedSearchHistory()
		{
			JsonUtility.SaveToFile(Settings.Gallery.ToolStrip.History, pathFormatter.GetConfiguration("search-searches"));
		}

		private void WriteVisitedGalleryHistory()
		{
			JsonUtility.SaveToFile(Settings.Details.ToolStrip.History, pathFormatter.GetConfiguration("details-searches"));
		}

		private void WriteSearchFilters()
		{
			JsonUtility.SaveToFile(Settings.Gallery.ToolStrip.Filters, pathFormatter.GetConfiguration("search-filters"));
		}

		private void WriteLibraryFilters()
		{
			JsonUtility.SaveToFile(Settings.Library.ToolStrip.Filters, pathFormatter.GetConfiguration("library-filters"));
		}

		private void WriteTags()
		{
			if (!tagsModel.IsDirty)
			{
				return;
			}

			JsonUtility.SaveToFile(tagsModel.AllTags, pathFormatter.GetConfiguration("tags"));
		}

		private void WriteTheme()
		{
			JsonUtility.SaveToFile(theme, pathFormatter.GetConfiguration("theme"));
		}
	}
}
