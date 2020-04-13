using Ash.System.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	public partial class BookmarksTreeView : UserControl
	{
		private readonly TreeViewEx treeView;
		private readonly ContextMenu treeViewContextMenu;

		public TreeView TreeView => treeView;

		public BookmarksFilter BookmarksFilter { get; }
		public BookmarksModel BookmarksModel { get; }
		public Configuration.ConfigBookmarksList BookmarksListSettings { get; }
		public WebBrowserTreeNodeToolTip WebBrowserToolTip { get; }
		public IQueryParser QueryParser { get; }
		public ICacheFileSystem CacheFileSystem { get; }
		public SearchHandler SearchHandler { get; }

		public BookmarksTreeView()
		{
			InitializeComponent();
		}

		public BookmarksTreeView(BookmarksFilter bookmarksFilter, BookmarksModel bookmarksModel, Configuration.ConfigBookmarksList bookmarksListSettings, WebBrowserTreeNodeToolTip webBrowserToolTip, IQueryParser queryParser, ICacheFileSystem cacheFileSystem, SearchHandler searchHandler)
		{
			InitializeComponent();

			BookmarksFilter = bookmarksFilter;
			BookmarksModel = bookmarksModel;
			BookmarksListSettings = bookmarksListSettings;
			WebBrowserToolTip = webBrowserToolTip;
			QueryParser = queryParser;
			CacheFileSystem = cacheFileSystem;
			SearchHandler = searchHandler;

			treeView = new TreeViewEx();
			treeViewContextMenu = new ContextMenu();

			SuspendLayout();

			//
			//
			//
			treeViewContextMenu.Tag = treeView;
			treeViewContextMenu.Popup += TreeViewContextMenu_Popup;

			//
			//
			//
			treeView.HideSelection = false;
			treeView.HotTracking = true;
			treeView.Dock = DockStyle.Fill;
			//treeView.TreeViewNodeSorter = new TreeViewNodeSorter();
			treeView.Sorted = true;
			treeView.ContextMenu = treeViewContextMenu;
			treeView.NodeActivated += TreeView_NodeActivated;
			treeView.NodeSelected += TreeView_NodeSelected;
			treeView.AfterExpand += TreeView_AfterExpand;
			treeView.AfterCollapse += TreeView_AfterCollapse;

			//
			// this
			//
			Controls.Add(treeView);

			bookmarksFilter.TextChanged += BookmarksFilter_TextChanged;

			ResumeLayout(false);
		}

		private void BookmarksFilter_TextChanged(object sender, EventArgs e)
		{
			Populate(x => BookmarksFilter.ShouldFilter(x));
		}

		private void TreeView_AfterCollapse(object sender, TreeViewEventArgs e)
		{
			UpdateCollapseState(e.Node);
		}

		private void TreeView_AfterExpand(object sender, TreeViewEventArgs e)
		{
			UpdateCollapseState(e.Node);
		}

		private void UpdateCollapseState(TreeNode node)
		{
			if (node == null
			|| !(node.Tag is BookmarkFolder))
			{
				return;
			}

			string key = ((BookmarkFolder)node.Tag).Path;

			if (BookmarksListSettings.Collapsed.ContainsKey(key))
			{
				BookmarksListSettings.Collapsed[key] = !node.IsExpanded;
			}
			else
			{
				BookmarksListSettings.Collapsed.Add(key, !node.IsExpanded);
			}
		}

		public void Populate(Func<BookmarkNode, bool> predicate)
		{
			bool shouldInitializeTree = treeView.Nodes.Count == 0;

			using (new CursorScope(Cursors.WaitCursor))
			{
				treeView.BeginUpdate();

				treeView.Nodes.Clear();

				if (shouldInitializeTree)
				{
					foreach (KeyValuePair<string, BookmarkFolder> kvp in BookmarksModel.BookmarkFolders)
					{
						AddFolder(kvp.Key, false);
					}
				}

				foreach (KeyValuePair<string, BookmarkNode> kvp in BookmarksModel.Bookmarks.Where(x => predicate.Invoke(x.Value)))
				{
					AddItem(kvp.Key, kvp.Value);
				}

				ExpandOrCollapseFolderRecursive(treeView.Nodes);

				treeView.EndUpdate();
			}
		}

		private void ExpandOrCollapseFolderRecursive(TreeNodeCollection nodes)
		{
			foreach (TreeNode folderNode in nodes)
			{
				BookmarkFolder folder = folderNode.Tag as BookmarkFolder;

				if (folder == null)
				{
					continue;
				}

				if (BookmarksListSettings.Collapsed.ContainsKey(folder.Path))
				{
					if (BookmarksListSettings.Collapsed[folder.Path])
					{
						folderNode.Collapse();
					}
					else
					{
						folderNode.Expand();
					}
				}

				if (folderNode.Nodes != null && folderNode.Nodes.Count > 0)
				{
					ExpandOrCollapseFolderRecursive(folderNode.Nodes);
				}
			}
		}

		public TreeNode AddFolder(string path, bool skipLastPart)
		{
			TreeNode parentNode = null;
			string[] parts = path.Split(new char[] { '/' });
			int maxParts = skipLastPart ? parts.Length - 1 : parts.Length;

			for (int i = 0; i < maxParts; ++i)
			{
				StringBuilder sb = new StringBuilder();

				for (int j = 0; j <= i; ++j)
				{
					if (j != 0)
					{
						sb.Append('/');
					}
					sb.Append(parts[j]);
				}

				string subPath = sb.ToString();

				TreeNode node = FindFolder(treeView, parentNode, subPath);

				if (node == null)
				{
					node = new TreeNode();

					BookmarkFolder folder;

					if (BookmarksModel.BookmarkFolders.TryGetValue(subPath, out folder))
					{
						node.Tag = folder;
						node.Text = folder.Text;
					}
					// fallback...
					else
					{
						node.Text = parts[i];
					}

					node.ToolTipText = subPath;

					ContextMenu folderNodeContextMenu = new ContextMenu();

					folderNodeContextMenu.Popup += FolderNodeContextMenu_Popup;
					folderNodeContextMenu.Tag = node;

					node.ContextMenu = folderNodeContextMenu;

					if (parentNode != null)
					{
						parentNode.Nodes.Add(node);
					}
					else
					{
						treeView.Nodes.Add(node);
					}
					/*
					if (Form.settings.BookmarksBrowser.Collapsed.ContainsKey(folder.Path))
					{
						if (Form.settings.BookmarksBrowser.Collapsed[folder.Path])
						{
							node.Collapse();
						}
						else
						{
							node.Expand();
						}
					}
					*/
				}

				parentNode = node;
			}

			return parentNode;
		}

		public TreeNode AddItem(string path, BookmarkNode item)
		{
			TreeNode contentNode = new TreeNode();
			
			contentNode.Text = item.Text;
			contentNode.Tag = new AddBookmarkItemTask(path, item);

			TreeNodeCollection parentNodes = null;

			if (path.Contains('/') && !path.StartsWith("/", StringComparison.OrdinalIgnoreCase))
			{
				TreeNode parentNode = AddFolder(path, true);

				parentNodes = parentNode.Nodes;
			}
			else
			{
				parentNodes = treeView.Nodes;
			}

			ContextMenu contentNodeContextMenu = new ContextMenu();

			contentNodeContextMenu.Popup += ItemContentNodeContextMenu_Popup;
			contentNodeContextMenu.Tag = contentNode;
			contentNode.ContextMenu = contentNodeContextMenu;

			parentNodes.Add(contentNode);

			string[] tokens = item.Value.Split(new char[] { ':' });
			int galleryId;

			if (QueryParser.ParseDetailsSearch(tokens, out galleryId)
				|| QueryParser.ParseDownloadSearch(tokens, out galleryId))
			{
				WebBrowserToolTip.SetToolTip(contentNode, galleryId);
			}

			treeView.SelectedNode = contentNode;

			return contentNode;
		}

		private static TreeNode FindFolder(TreeView treeView, TreeNode parentNode, string folderName)
		{
			TreeNodeCollection nodes = parentNode != null ? parentNode.Nodes : treeView.Nodes;

			foreach (TreeNode node in nodes)
			{
				BookmarkFolder nodeFolderName = node.Tag as BookmarkFolder;

				if (nodeFolderName == null)
				{
					continue;
				}

				if (nodeFolderName.Path.Equals(folderName))
				{
					return node;
				}
			}

			return null;
		}

		// NOTE: bookmark path allows invalid filesystem characters so it's unsafe to call Path.GetFileName.
		private static string GetPathFileName(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return "";
			}

			string[] folders = path.Split('/');

			if (folders.Length == 0)
			{
				return "";
			}

			return folders[folders.Length - 1];
		}

		private void FolderNodeContextMenu_Popup(object sender, EventArgs e)
		{
			ContextMenu contextMenu = sender as ContextMenu;
			TreeNode treeNode = contextMenu.Tag as TreeNode;
			BookmarkFolder folder = treeNode.Tag as BookmarkFolder;

			contextMenu.MenuItems.Clear();

			if (folder != null)
			{
				contextMenu.MenuItems.Add(new MenuItem("&Edit Folder", (sender2, e2) =>
				{
					string dialogResult = PromptBox.Show("Change name to:", folder.Text, "Bookmark folder name", (new string[] { folder.Text, GetPathFileName(folder.Path) }).Distinct().Where(x => !string.IsNullOrEmpty(x)).ToArray());

					if (dialogResult != null)
					{
						folder.Text = dialogResult;
						treeNode.Text = dialogResult;
					}
				}) { Name = "folder_edit" });

				contextMenu.MenuItems.Add(new MenuItem("&Remove Folder", (sender2, e2) =>
				{
					treeNode.Remove();
					BookmarksModel.RemovePath(folder.Path);
				}) { Name = "remove_folder" });

				// TODO: "move to..." command (to change the folder (and its children) path)
			}
		}

		private void ItemContentNodeContextMenu_Popup(object sender, EventArgs e)
		{
			ContextMenu contextMenu = sender as ContextMenu;
			TreeNode treeNode = contextMenu.Tag as TreeNode;
			AddBookmarkItemTask item = treeNode.Tag as AddBookmarkItemTask;

			contextMenu.MenuItems.Clear();

			if (item != null)
			{
				string[] tokens = item.Item.Value.Split(new char[] { ':' });
				int galleryId;
				int pageIndex;
				string query;
				int tagId;
				string tagType;
				string tagName;

				if (QueryParser.ParseDetailsSearch(tokens, out galleryId))
				{
					contextMenu.MenuItems.Add(new MenuItem("&Show Details", (sender2, e2) =>
					{
						SearchHandler.ParseAndExecuteSearchText(item.Item.Value);
					}));

					contextMenu.MenuItems.Add("-");

					contextMenu.MenuItems.Add(new MenuItem("&Read", (sender2, e2) =>
					{
						CacheFileSystem.OpenFirstCachedPage(galleryId);
					}));

					contextMenu.MenuItems.Add(new MenuItem("&Browse", (sender2, e2) =>
					{
						CacheFileSystem.SelectFirstCachedPage(galleryId);
					}));

					contextMenu.MenuItems.Add(new MenuItem("&Show in Explorer", (sender2, e2) =>
					{
						CacheFileSystem.SelectPagesFolder(galleryId);
					}));
				}
				else if (QueryParser.ParseDownloadSearch(tokens, out galleryId))
				{
					contextMenu.MenuItems.Add(new MenuItem("&Show Download", (sender2, e2) =>
					{
						SearchHandler.ParseAndExecuteSearchText(item.Item.Value);
					}));

					contextMenu.MenuItems.Add("-");

					contextMenu.MenuItems.Add(new MenuItem("&Read", (sender2, e2) =>
					{
						CacheFileSystem.OpenFirstCachedPage(galleryId);
					}));

					contextMenu.MenuItems.Add(new MenuItem("&Browse", (sender2, e2) =>
					{
						CacheFileSystem.SelectFirstCachedPage(galleryId);
					}));

					contextMenu.MenuItems.Add(new MenuItem("&Show in Explorer", (sender2, e2) =>
					{
						CacheFileSystem.SelectPagesFolder(galleryId);
					}));
				}
				else if (QueryParser.ParseRecentSearch(tokens, out pageIndex))
				{
					contextMenu.MenuItems.Add(new MenuItem("&Recent Search", (sender2, e2) =>
					{
						SearchHandler.ParseAndExecuteSearchText(item.Item.Value);
					}));
				}
				else if (QueryParser.ParseQuerySearch(tokens, out query, out pageIndex))
				{
					contextMenu.MenuItems.Add(new MenuItem("&Query Search", (sender2, e2) =>
					{
						SearchHandler.ParseAndExecuteSearchText(item.Item.Value);
					}));
				}
				else if (QueryParser.ParseTaggedSearch(tokens, out tagId, out tagType,out tagName, out pageIndex))
				{
					contextMenu.MenuItems.Add(new MenuItem("&Tagged Search", (sender2, e2) =>
					{
						SearchHandler.ParseAndExecuteSearchText(item.Item.Value);
					}));
				}
				else if (QueryParser.ParseLibrarySearch(tokens, out pageIndex))
				{
					contextMenu.MenuItems.Add(new MenuItem("&Show in Library", (sender2, e2) =>
					{
						SearchHandler.ParseAndExecuteSearchText(item.Item.Value);
					}));
				}

				contextMenu.MenuItems.Add("-");

				contextMenu.MenuItems.Add(new MenuItem("&Remove bookmark", (sender2, e2) =>
				{
					WebBrowserToolTip.RemoveToolTip(treeNode);
					treeNode.Remove();
					BookmarksModel.RemoveBookmark(item.Path);
				}) { Name = "remove" });

				// TODO: "move to..." command (to change the bookmark path)
			}
		}

		private static void TreeViewContextMenu_Popup(object sender, EventArgs e)
		{
			ContextMenu contextMenu = sender as ContextMenu;
			TreeView treeView = contextMenu.Tag as TreeView;

			contextMenu.MenuItems.Clear();

			//if (treeView.IsCollapsed)
			{
				contextMenu.MenuItems.Add(new MenuItem("Collapse All", (sender2, e2) => treeView.CollapseAll()) { Name = "collapse_all" });
			}

			//if (!treeView.IsExpanded)
			{
				contextMenu.MenuItems.Add(new MenuItem("Expand All", (sender2, e2) => treeView.ExpandAll()) { Name = "expand_all" });
			}
		}

		private void TreeView_NodeSelected(object sender, TreeViewExEventArgs e)
		{
			TreeView_NodeActivated(sender, e);
		}

		private void TreeView_NodeActivated(object sender, TreeViewExEventArgs e)
		{
			AddBookmarkItemTask item = e.Node.Tag as AddBookmarkItemTask;
			BookmarkFolder folder = e.Node.Tag as BookmarkFolder;

			if (item != null)
			{
				SearchHandler.ParseAndExecuteSearchText(item.Item.Value);
			}
			else if (folder != null)
			{
				if (e.Action == TreeViewAction.ByKeyboard)
				{
					e.Node.ToggleCollapse();
				}
			}
		}
	}
}
