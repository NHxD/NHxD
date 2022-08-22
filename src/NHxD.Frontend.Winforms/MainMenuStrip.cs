using NHxD.Frontend.Winforms.Configuration;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	class MainMenuStrip : MenuStrip
	{
		private readonly ToolStripMenuItem fileToolStripMenuItem;
		private readonly ToolStripMenuItem exitToolStripMenuItem;
		private readonly ToolStripMenuItem helpToolStripMenuItem;
		private readonly ToolStripMenuItem aboutToolStripMenuItem;
		private readonly ToolStripMenuItem pluginsToolStripMenuItem;
		private readonly ToolStripSeparator toolStripSeparator1;
		private readonly ToolStripMenuItem viewToolStripMenuItem;
		private readonly ToolStripMenuItem listToolStripMenuItem;
		private readonly ToolStripMenuItem detailsToolStripMenuItem;
		private readonly ToolStripSeparator toolStripSeparator2;
		private readonly ToolStripMenuItem fullScreenToolStripMenuItem;
		private readonly ToolStripSeparator toolStripSeparator3;
		private readonly ToolStripMenuItem linksToolStripMenuItem;
		private readonly ToolStripMenuItem linkDlsiteToolStripMenuItem;
		private readonly ToolStripMenuItem linkJlistToolStripMenuItem;
		private readonly ToolStripMenuItem linkFakkuToolStripMenuItem;

		private readonly ToolStripMenuItem toolsToolStripMenuItem;
		private readonly ToolStripMenuItem manageMetadataCacheToolStripMenuItem;
		private readonly ToolStripMenuItem clearCookiesToolStripMenuItem;
		public Settings Settings { get; }
		public ISearchResultCache SearchResultCache { get; }
		public IMetadataCache MetadataCache { get; }
		public MetadataCacheSnapshot MetadataCacheSnapshot { get; }
		public IPathFormatter PathFormatter { get; }

		public event EventHandler Exit = delegate { };
		public event EventHandler ToggleListsPanel = delegate { };
		public event EventHandler ToggleDetailsPanel = delegate { };
		public event EventHandler ToggleFullScreen = delegate { };
		public event EventHandler ShowAbout = delegate { };
		public event EventHandler ShowPlugins = delegate { };

		public MainMenuStrip()
		{
		}

		public MainMenuStrip(Settings settings, IPathFormatter pathFormatter, IMetadataCache metadataCache, ISearchResultCache searchResultCache, MetadataCacheSnapshot metadataCacheSnapshot)
		{
			Settings = settings;
			PathFormatter = pathFormatter;
			MetadataCache = metadataCache;
			SearchResultCache = searchResultCache;
			MetadataCacheSnapshot = metadataCacheSnapshot;

			fileToolStripMenuItem = new ToolStripMenuItem();
			exitToolStripMenuItem = new ToolStripMenuItem();
			viewToolStripMenuItem = new ToolStripMenuItem();
			listToolStripMenuItem = new ToolStripMenuItem();
			detailsToolStripMenuItem = new ToolStripMenuItem();
			toolStripSeparator2 = new ToolStripSeparator();
			fullScreenToolStripMenuItem = new ToolStripMenuItem();
			helpToolStripMenuItem = new ToolStripMenuItem();
			pluginsToolStripMenuItem = new ToolStripMenuItem();
			toolStripSeparator1 = new ToolStripSeparator();
			aboutToolStripMenuItem = new ToolStripMenuItem();
			toolStripSeparator3 = new ToolStripSeparator();
			linksToolStripMenuItem = new ToolStripMenuItem();
			linkDlsiteToolStripMenuItem = new ToolStripMenuItem();
			linkJlistToolStripMenuItem = new ToolStripMenuItem();
			linkFakkuToolStripMenuItem = new ToolStripMenuItem();
			toolsToolStripMenuItem = new ToolStripMenuItem();
			manageMetadataCacheToolStripMenuItem = new ToolStripMenuItem();
			clearCookiesToolStripMenuItem = new ToolStripMenuItem();

			SuspendLayout();

			// 
			// fileToolStripMenuItem
			// 
			fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
			{
				exitToolStripMenuItem
			});
			fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			fileToolStripMenuItem.Text = "&File";

			// 
			// exitToolStripMenuItem
			// 
			exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			exitToolStripMenuItem.ShortcutKeys = Keys.Alt | Keys.F4;
			exitToolStripMenuItem.Text = "&Exit";
			exitToolStripMenuItem.Click += new EventHandler(ExitToolStripMenuItem_Click);

			// 
			// viewToolStripMenuItem
			// 
			viewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
			{
				listToolStripMenuItem,
				detailsToolStripMenuItem,
				toolStripSeparator2,
				fullScreenToolStripMenuItem
			});
			viewToolStripMenuItem.Name = "viewToolStripMenuItem";
			viewToolStripMenuItem.Text = "&View";

			// 
			// listToolStripMenuItem
			// 
			listToolStripMenuItem.Name = "listToolStripMenuItem";
			listToolStripMenuItem.Text = "&List Panel";
			listToolStripMenuItem.Click += new EventHandler(ListToolStripMenuItem_Click);

			// 
			// detailsToolStripMenuItem
			// 
			detailsToolStripMenuItem.Name = "detailsToolStripMenuItem";
			detailsToolStripMenuItem.Text = "&Details Panel";
			detailsToolStripMenuItem.Click += new EventHandler(DetailsToolStripMenuItem_Click);

			// 
			// toolStripSeparator2
			// 
			toolStripSeparator2.Name = "toolStripSeparator2";

			// 
			// fullScreenToolStripMenuItem
			// 
			fullScreenToolStripMenuItem.Name = "fullScreenToolStripMenuItem";
			fullScreenToolStripMenuItem.ShortcutKeys = Keys.F11;
			fullScreenToolStripMenuItem.Text = "&Full Screen";
			fullScreenToolStripMenuItem.Click += new EventHandler(FullScreenToolStripMenuItem_Click);

			// 
			// helpToolStripMenuItem
			// 
			helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
			{
				pluginsToolStripMenuItem,
				toolStripSeparator1,
				linksToolStripMenuItem,
				toolStripSeparator3,
				aboutToolStripMenuItem
			});
			helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			helpToolStripMenuItem.Text = "&Help";

			// 
			// linksToolStripMenuItem
			// 
			linksToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
			{
				linkDlsiteToolStripMenuItem,
				linkJlistToolStripMenuItem,
				linkFakkuToolStripMenuItem
			});
			linksToolStripMenuItem.Name = "linksToolStripMenuItem";
			linksToolStripMenuItem.Text = "&Links";

			// 
			// linkDlsiteStripMenuItem
			// 
			linkDlsiteToolStripMenuItem.Name = "linkDlsiteStripMenuItem";
			linkDlsiteToolStripMenuItem.Text = "Visit &DLsite...";
			linkDlsiteToolStripMenuItem.Click += new EventHandler(VisitDlsiteToolStripMenuItem_Click);

			// 
			// linkJlistStripMenuItem
			// 
			linkJlistToolStripMenuItem.Name = "linkJlistStripMenuItem";
			linkJlistToolStripMenuItem.Text = "Visit &J-List...";
			linkJlistToolStripMenuItem.Click += new EventHandler(VisitJlistToolStripMenuItem_Click);

			// 
			// linkFakkuStripMenuItem
			// 
			linkFakkuToolStripMenuItem.Name = "linkFakkuStripMenuItem";
			linkFakkuToolStripMenuItem.Text = "Visit &FAKKU...";
			linkFakkuToolStripMenuItem.Click += new EventHandler(VisitFakkuToolStripMenuItem_Click);

			// 
			// toolStripSeparator3
			// 
			toolStripSeparator3.Name = "toolStripSeparator3";

			// 
			// pluginsToolStripMenuItem
			// 
			pluginsToolStripMenuItem.Name = "pluginsToolStripMenuItem";
			pluginsToolStripMenuItem.Text = "&Plugins";
			pluginsToolStripMenuItem.Click += new EventHandler(PluginsToolStripMenuItem_Click);

			// 
			// toolStripSeparator1
			// 
			toolStripSeparator1.Name = "toolStripSeparator1";

			// 
			// aboutToolStripMenuItem
			// 
			aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			aboutToolStripMenuItem.Text = "&About";
			aboutToolStripMenuItem.Click += new EventHandler(AboutToolStripMenuItem_Click);

			// 
			// toolsToolStripMenuItem
			// 
			toolsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[]
			{
				manageMetadataCacheToolStripMenuItem,
				clearCookiesToolStripMenuItem,
			});
			toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
			toolsToolStripMenuItem.Text = "&Tools";

			// 
			// manageMetadataCacheToolStripMenuItem
			// 
			manageMetadataCacheToolStripMenuItem.Name = "manageMetadataCacheToolStripMenuItem";
			manageMetadataCacheToolStripMenuItem.Text = "Manage &Metadata Cache";
			manageMetadataCacheToolStripMenuItem.Click += new EventHandler(ManageMetadataCacheToolStripMenuItem_Click);

			// 
			// clearCookiesToolStripMenuItem
			// 
			clearCookiesToolStripMenuItem.Name = "clearCookiesToolStripMenuItem";
			clearCookiesToolStripMenuItem.Text = "Clear &Cookies";
			clearCookiesToolStripMenuItem.Click += new EventHandler(ClearCookiesToolStripMenuItem_Click);

			//
			// this
			//
			Items.AddRange(new ToolStripItem[]
			{
				fileToolStripMenuItem,
				viewToolStripMenuItem,
				toolsToolStripMenuItem,
				helpToolStripMenuItem
			});
			Name = "mainMenuStrip";

			MenuActivate += MainMenuStrip_MenuActivate;

			ResumeLayout(false);
			PerformLayout();
		}

		private void MainMenuStrip_MenuActivate(object sender, EventArgs e)
		{
			listToolStripMenuItem.Checked = !Settings.Panels.Lists.IsCollapsed;
			detailsToolStripMenuItem.Checked = !Settings.Panels.Details.IsCollapsed;
			fullScreenToolStripMenuItem.Checked = Settings.Window.FullScreen.IsActive;
			clearCookiesToolStripMenuItem.Enabled = CanClearCookies();
		}

		private void ApplicationMenuStrip_MenuActivate(object sender, EventArgs e)
		{
			listToolStripMenuItem.Checked = !Settings.Panels.Lists.IsCollapsed;
			detailsToolStripMenuItem.Checked = !Settings.Panels.Details.IsCollapsed;
			fullScreenToolStripMenuItem.Checked = Settings.Window.FullScreen.IsActive;
		}

		
		private void ManageMetadataCacheToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (var dialog = new ManageMetadataCacheForm(PathFormatter, MetadataCache, SearchResultCache, MetadataCacheSnapshot, Settings.Cache.MetadataCache))
			{
				if (dialog.ShowDialog(this) == DialogResult.OK)
				{
					//MetadataCacheSnapshot.Delete();
					//MetadataCacheSnapshot.EnsureReady();
				}
			}
		}

		private void ClearCookiesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (!CanClearCookies())
			{
				return;
			}

			Settings.Network.Client.Cookies.Clear();
			Settings.Network.Client.UserAgent = "";

			MessageBox.Show(this, "Changes will be applied the next time the application is started.", "NHxD", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Exit.Invoke(this, e);
		}

		private void ListToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ToggleListsPanel.Invoke(this, e);
		}

		private void DetailsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ToggleDetailsPanel.Invoke(this, e);
		}

		private void FullScreenToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ToggleFullScreen.Invoke(this, e);
		}

		private void PluginsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ShowPlugins.Invoke(this, e);
		}

		private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ShowAbout.Invoke(this, e);
		}

		private void VisitDlsiteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Process.Start("https://www.dlsite.com/books/");
		}

		private void VisitJlistToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Process.Start("https://www.jlist.com/category/doujin-and-touhou/doujinshi-books");
		}

		private void VisitFakkuToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Process.Start("https://www.fakku.net/doujin");
		}

		private bool CanClearCookies()
			=> Settings.Network.Client.Cookies.Count > 0
				|| !string.IsNullOrEmpty(Settings.Network.Client.UserAgent);
	}
}
