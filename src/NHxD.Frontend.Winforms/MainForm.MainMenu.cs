using NHxD.Plugin;
using NHxD.Plugin.ArchiveWriter;
using NHxD.Plugin.MetadataConverter;
using NHxD.Plugin.MetadataProcessor;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	public partial class MainForm : Form
	{
		// NOTE: the hacks are required because SplitterPanel doesn't provide collapsing events.

		private void EnterFullScreen()
		{
			fullScreenRestoreState.WindowState = WindowState;
			fullScreenRestoreState.IsListPanelVisible = splitContainer3.Panel1Collapsed;
			fullScreenRestoreState.IsDetailsPanelVisible = splitContainer2.Panel2Collapsed;

			FormBorderStyle = FormBorderStyle.None;
			WindowState = FormWindowState.Maximized;

			if (Settings.Window.FullScreen.AutoHidePanels.Lists.IsCollapsed)
			{
				splitContainer3.Panel1Collapsed = true;

				// HACK
				Settings.Panels.Lists.IsCollapsed = splitContainer3.Panel1Collapsed;
			}

			if (Settings.Window.FullScreen.AutoHidePanels.Details.IsCollapsed)
			{
				splitContainer2.Panel2Collapsed = true;

				// HACK
				Settings.Panels.Details.IsCollapsed = splitContainer2.Panel2Collapsed;
			}

			Padding = Settings.Window.FullScreen.Padding;
		}

		private void LeaveFullScreen()
		{
			FormBorderStyle = FormBorderStyle.Sizable;
			WindowState = fullScreenRestoreState.WindowState;

			if (Settings.Window.FullScreen.AutoHidePanels.Lists.IsCollapsed
				&& fullScreenRestoreState.IsListPanelVisible != null)
			{
				splitContainer3.Panel1Collapsed = fullScreenRestoreState.IsListPanelVisible.Value;

				// HACK
				Settings.Panels.Lists.IsCollapsed = splitContainer3.Panel1Collapsed;

				fullScreenRestoreState.IsListPanelVisible = null;
			}

			if (Settings.Window.FullScreen.AutoHidePanels.Details.IsCollapsed
				&& fullScreenRestoreState.IsDetailsPanelVisible != null)
			{
				splitContainer2.Panel2Collapsed = fullScreenRestoreState.IsDetailsPanelVisible.Value;

				// HACK
				Settings.Panels.Details.IsCollapsed = splitContainer2.Panel2Collapsed;

				fullScreenRestoreState.IsDetailsPanelVisible = null;
			}

			Padding = Padding.Empty;
		}

		private void MainMenuStrip_Exit(object sender, EventArgs e)
		{
			Application.Exit();
		}

		private void MainMenuStrip_ToggleListsPanel(object sender, EventArgs e)
		{
			Settings.Panels.Lists.IsCollapsed = !Settings.Panels.Lists.IsCollapsed;

			SuspendLayout();
			splitContainer3.Panel1Collapsed = Settings.Panels.Lists.IsCollapsed;
			ResumeLayout(false);

			fullScreenRestoreState.IsListPanelVisible = null;
		}

		private void MainMenuStrip_ToggleDetailsPanel(object sender, EventArgs e)
		{
			Settings.Panels.Details.IsCollapsed = !Settings.Panels.Details.IsCollapsed;

			SuspendLayout();
			splitContainer2.Panel2Collapsed = Settings.Panels.Details.IsCollapsed;
			ResumeLayout(false);

			fullScreenRestoreState.IsDetailsPanelVisible = null;
		}

		private void MainMenuStrip_ToggleFullScreen(object sender, EventArgs e)
		{
			Settings.Window.FullScreen.IsActive = !Settings.Window.FullScreen.IsActive;

			SuspendLayout();

			if (!Settings.Window.FullScreen.IsActive)
			{
				LeaveFullScreen();
			}
			else
			{
				EnterFullScreen();
			}

			ResumeLayout(false);
		}

		private void MainMenuStrip_ShowPlugins(object sender, EventArgs e)
		{
			List<IPlugin> plugins = new List<IPlugin>();

			foreach (IArchiveWriter plugin in archiveWriters)
			{
				plugins.Add(plugin);
			}

			foreach (IMetadataConverter plugin in metadataConverters)
			{
				plugins.Add(plugin);
			}

			foreach (IMetadataProcessor plugin in metadataProcessors)
			{
				plugins.Add(plugin);
			}

			using (PluginsForm dialog = new PluginsForm()
			{
				ArchiveWriters = archiveWriters,
				MetadataConverters = metadataConverters,
				MetadataProcessors = metadataProcessors,
				PluginDirectory = pathFormatter.GetPluginDirectory()
			})
			{
				DialogResult dialogResult = dialog.ShowDialog(this);

				if (dialogResult == DialogResult.OK)
				{
					if (dialog.ArchiveWriters.Count != archiveWriters.Count)
					{
						// TODO: replace archive writers.
					}

					if (dialog.MetadataConverters.Count != metadataConverters.Count)
					{
						// TODO: replace metadata converters.
					}

					if (dialog.MetadataProcessors.Count != metadataProcessors.Count)
					{
						// TODO: replace metadata processors.
					}
				}
			}
		}

		private void MainMenuStrip_ShowAbout(object sender, EventArgs e)
		{
			using (AboutForm dialog = new AboutForm(aboutTextFormatter, documentTemplates.About, Settings.About.Size))
			{
				dialog.ShowDialog(this);
			}
		}
	}
}
