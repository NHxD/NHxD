using Ash.System.Windows.Forms;
using Nhentai;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	public partial class WebBrowserTreeNodeToolTip : UserControl
	{
		private readonly WebBrowserEx webBrowser;
		private readonly List<TreeView> observedTreeViews;
		private readonly Dictionary<TreeNode, int> associatedControls;

		private bool showing;

		public IPathFormatter PathFormatter { get; }
		public DocumentTemplate<Metadata> GalleryTooltipTemplate { get; }

		public WebBrowserTreeNodeToolTip()
		{
			InitializeComponent();
		}

		public WebBrowserTreeNodeToolTip(IPathFormatter pathFormatter, DocumentTemplate<Metadata> galleryTooltipTemplate)
		{
			InitializeComponent();

			Visible = false;

			PathFormatter = pathFormatter;
			GalleryTooltipTemplate = galleryTooltipTemplate;
			associatedControls = new Dictionary<TreeNode, int>();
			observedTreeViews = new List<TreeView>();
			webBrowser = new WebBrowserEx();

			SuspendLayout();

			//
			//
			//
			webBrowser.AllowWebBrowserDrop = false;
			//webBrowser.AllowNavigation = false;
			webBrowser.ScriptErrorsSuppressed = true;
			webBrowser.WebBrowserShortcutsEnabled = false;
			webBrowser.Dock = DockStyle.Fill;
			webBrowser.Name = "tooltipWebBrowser";
			webBrowser.TabIndex = 0;
			webBrowser.DocumentCompleted += WebBrowser_DocumentCompleted;

			//
			// this
			//
			Controls.Add(webBrowser);

			ResumeLayout(false);
		}

		public void UnregisterTreeView(TreeView treeView)
		{
			if (treeView == null)
			{
				return;
			}
			/*
			if (!observedTreeViews.Contains(treeView)
				|| observedTreeViews.Any(x => x.TreeView == treeView))
			{
				return;
			}

			treeView.AfterSelect -= TreeView_AfterSelect;
			treeView.NodeMouseHover -= TreeView_NodeMouseHover;
			treeView.MouseLeave -= TreeView_MouseLeave;

			observedTreeViews.Remove(treeView);
			*/
		}

		public void RegisterTreeView(TreeView treeView)
		{
			if (treeView == null)
			{
				return;
			}

			if (observedTreeViews.Contains(treeView))
			{
				return;
			}

			treeView.AfterSelect += TreeView_AfterSelect;
			treeView.NodeMouseHover += TreeView_NodeMouseHover;
			treeView.MouseLeave += TreeView_MouseLeave;

			observedTreeViews.Add(treeView);
		}

		private void TreeView_MouseLeave(object sender, EventArgs e)
		{
			HideToolTip();
		}

		private void TreeView_NodeMouseHover(object sender, TreeNodeMouseHoverEventArgs e)
		{
			ShowToolTip(e.Node);
		}

		private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			//ShowToolTip(e.Node);
		}

		public void RemoveToolTip(TreeNode control)
		{
			UnregisterTreeView(control.TreeView);

			if (associatedControls.ContainsKey(control))
			{
				associatedControls.Remove(control);
			}
		}

		public void SetToolTip(TreeNode control, int galleryId)
		{
			if (associatedControls.ContainsKey(control))
			{
				associatedControls[control] = galleryId;
			}
			else
			{
				associatedControls.Add(control, galleryId);
			}

			RegisterTreeView(control.TreeView);
		}

		public void ShowToolTip(TreeNode control)
		{
			if (showing)
			{
				return;
			}

			if (!associatedControls.ContainsKey(control))
			{
				if (control != null)
				{
					//MessageBox.Show(control.Name + " has not been associated.");
				}

				HideToolTip();

				return;
			}

			showing = true;

			//Hide();

			Screen screen = Screen.FromControl(this);
			Point toolTipLocation = new Point(control.TreeView.Bounds.Left, control.Bounds.Location.Y);

			toolTipLocation.Offset(control.TreeView.Bounds.Width, control.Bounds.Height * 2);
			toolTipLocation.Offset(control.TreeView.Margin.Horizontal, 0);

			if (toolTipLocation.X + Size.Width > screen.WorkingArea.Width)
			{
				toolTipLocation.X -= Size.Width;
			}

			if (toolTipLocation.Y + Size.Height > screen.WorkingArea.Height)
			{
				toolTipLocation.Y -= Size.Height;
			}

			Location = toolTipLocation;

			int associatedGalleryId = associatedControls[control];
			string cachedMetadataFileName;

			if (PathFormatter.IsEnabled)
			{
				cachedMetadataFileName = PathFormatter.GetMetadata(associatedGalleryId);
			}
			else
			{
				cachedMetadataFileName = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", PathFormatter.GetCacheDirectory(), associatedGalleryId, ".json");
			}

			Metadata metadata = JsonUtility.LoadFromFile<Metadata>(cachedMetadataFileName);

			if (metadata == null)
			{
				//Logger.LogLineFormat("{0} has no associated metadata.", associatedGalleryId);
				showing = false;
				return;
			}

			string html = GalleryTooltipTemplate.GetFormattedText(metadata);

			if (string.IsNullOrEmpty(html))
			{
				//Logger.LogLine("Failed to get template html.");
				showing = false;
				return;
			}

			webBrowser.DocumentText = html;
		}

		private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			BringToFront();
			Show();
			showing = false;
		}

		public void HideToolTip()
		{
			Hide();
			showing = false;
		}
	}
}
