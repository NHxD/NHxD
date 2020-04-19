using NHxD.Frontend.Winforms.Configuration;
using System;
using System.Drawing;
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

		public Settings Settings { get; }

		public event EventHandler Exit = delegate { };
		public event EventHandler ToggleListsPanel = delegate { };
		public event EventHandler ToggleDetailsPanel = delegate { };
		public event EventHandler ToggleFullScreen = delegate { };
		public event EventHandler ShowAbout = delegate { };
		public event EventHandler ShowPlugins = delegate { };

		public MainMenuStrip()
		{
		}

		public MainMenuStrip(Settings settings)
		{
			Settings = settings;

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
				aboutToolStripMenuItem
			});
			helpToolStripMenuItem.Name = "helpToolStripMenuItem";
			helpToolStripMenuItem.Text = "&Help";

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
			// this
			//
			Items.AddRange(new ToolStripItem[]
			{
				fileToolStripMenuItem,
				viewToolStripMenuItem,
				helpToolStripMenuItem
			});
			Name = "mainMenuStrip";

			ResumeLayout(false);
			PerformLayout();
		}

		private void ApplicationMenuStrip_MenuActivate(object sender, EventArgs e)
		{
			listToolStripMenuItem.Checked = !Settings.Panels.Lists.IsCollapsed;
			detailsToolStripMenuItem.Checked = !Settings.Panels.Details.IsCollapsed;
			fullScreenToolStripMenuItem.Checked = Settings.Window.FullScreen.IsActive;
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
	}
}
