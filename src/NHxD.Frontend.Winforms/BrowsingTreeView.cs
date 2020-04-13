using Ash.System.Windows.Forms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	//
	// FIXME: what did I do to make the tree always expanding? (the tags tree works fine but both the browsing and bookmark trees auto expands)
	//

	public partial class BrowsingTreeView : UserControl
	{
		private readonly TreeViewEx treeView;
		private readonly ContextMenu treeViewContextMenu;

		public TreeView TreeView => treeView;

		public BrowsingFilter BrowsingFilter { get; }
		public BrowsingModel BrowsingModel { get; }
		public Configuration.ConfigBrowsingList BrowsingListSettings { get; }
		public SearchHandler SearchHandler { get; }
		public ISessionManager SessionManager { get; }
		public IQueryParser QueryParser { get; }

		public BrowsingTreeView()
		{
			InitializeComponent();
		}

		public BrowsingTreeView(BrowsingFilter browsingFilter, BrowsingModel browsingModel, Configuration.ConfigBrowsingList browsingListSettings, SearchHandler searchHandler, ISessionManager sessionManager, IQueryParser queryParser)
		{
			InitializeComponent();

			BrowsingFilter = browsingFilter;
			BrowsingModel = browsingModel;
			BrowsingListSettings = browsingListSettings;
			SearchHandler = searchHandler;
			SessionManager = sessionManager;
			QueryParser = queryParser;

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
			treeView.ContextMenu = treeViewContextMenu;
			treeView.HideSelection = false;
			treeView.HotTracking = true;
			treeView.Dock = DockStyle.Fill;
			treeView.TreeViewNodeSorter = new BrowsingTreeViewNodeSorter(BrowsingSortType.LastAccessTime, SortOrder.Descending);
			treeView.NodeActivated += TreeView_NodeActivated;
			treeView.NodeSelected += TreeView_NodeSelected;
			treeView.AfterExpand += TreeView_AfterExpand;
			treeView.AfterCollapse += TreeView_AfterCollapse;

			//
			//
			//
			Controls.Add(treeView);

			BrowsingFilter.SortTypeChanged += BrowsingFilter_SortTypeChanged;
			BrowsingFilter.SortOrderChanged += BrowsingFilter_SortOrderChanged;
			BrowsingFilter.TextChanged += BrowsingFilter_TextChanged;

			ResumeLayout(false);
		}

		private void BrowsingFilter_TextChanged(object sender, EventArgs e)
		{
			Populate(x => BrowsingFilter.ShouldFilter(x));
		}

		private void BrowsingFilter_SortTypeChanged(object sender, EventArgs e)
		{
			TreeView.TreeViewNodeSorter = new BrowsingTreeViewNodeSorter(BrowsingFilter.SortType, BrowsingFilter.SortOrder);
		}

		private void BrowsingFilter_SortOrderChanged(object sender, EventArgs e)
		{
			TreeView.TreeViewNodeSorter = new BrowsingTreeViewNodeSorter(BrowsingFilter.SortType, BrowsingFilter.SortOrder);
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
			|| node.Level != 0
			|| !(node.Tag is DateTime))
			{
				return;
			}

			DateTime key = ((DateTime)node.Tag).Date;

			if (BrowsingListSettings.Collapsed.ContainsKey(key))
			{
				BrowsingListSettings.Collapsed[key] = !node.IsExpanded;
			}
			else
			{
				BrowsingListSettings.Collapsed.Add(key, !node.IsExpanded);
			}
		}

		public void Populate(Func<KeyValuePair<string, BrowsingItem>, bool> predicate)
		{
			using (new CursorScope(Cursors.WaitCursor))
			{
				treeView.BeginUpdate();

				treeView.Nodes.Clear();

				foreach (KeyValuePair<string, BrowsingItem> searchRecord in BrowsingModel.SearchHistory
					.Where(x => predicate.Invoke(x)))
				{
					AddItem(searchRecord.Key, searchRecord.Value);
				}
					
				foreach (TreeNode dateNode in treeView.Nodes)
				{
					DateTime date = (DateTime)dateNode.Tag;

					if (BrowsingListSettings.Collapsed.ContainsKey(date.Date))
					{
						if (BrowsingListSettings.Collapsed[date.Date])
						{
							dateNode.Collapse();
						}
						else
						{
							dateNode.Expand();
						}
					}
				}

				treeView.EndUpdate();
			}
		}

		public void AddItem(string uri, BrowsingItem item)
		{
			TreeNode dateNode = null;
			DateTime date = DateTime.Now;

			if (BrowsingListSettings.SortType == BrowsingSortType.CreationTime)
			{
				date = item.CreationTime;
			}
			else if (BrowsingListSettings.SortType == BrowsingSortType.LastWriteTime)
			{
				date = item.LastWriteTime;
			}
			else if (BrowsingListSettings.SortType == BrowsingSortType.LastAccessTime)
			{
				date = item.LastAccessTime;
			}

			foreach (TreeNode node in treeView.Nodes)
			{
				DateTime nodeDate = (DateTime)node.Tag;

				//if (nodeDate.Equals(today))
				if (nodeDate.Year == date.Year
					&& nodeDate.Month == date.Month
					&& nodeDate.Day == date.Day)
				{
					dateNode = node;
					break;
				}
			}

			if (dateNode == null)
			{
				// add new category for this date
				dateNode = new TreeNode(date.ToLongDateString()) { Tag = date.Date };

				/*if (Form.settings.Lists.Browsing.Collapsed.ContainsKey(date.Date))
				{
					if (Form.settings.Lists.Browsing.Collapsed[date.Date])
					{
						dateNode.Collapse();
					}
					else
					{
						dateNode.Expand();
					}
				}*/

				treeView.Nodes.Insert(0, dateNode);
			}

			foreach (TreeNode node in treeView.Nodes)
			{
				TreeNode resultNode = FindSearchHistoryNode(node, uri);

				if (resultNode != null)
				{
					resultNode.Remove();
					
					break;  // should be unique
				}
			}

			TreeNode contentNode = new TreeNode(uri)
			{
				Tag = new AddBrowsingItemTask(uri, item)
			};

			ContextMenu contentNodeContextMenu = new ContextMenu();

			contentNodeContextMenu.Popup += BrowsingContentNodeContextMenu_Popup;
			contentNodeContextMenu.Tag = contentNode;
			contentNode.ContextMenu = contentNodeContextMenu;

			dateNode.Nodes.Insert(0, contentNode);

			treeView.SelectedNode = contentNode;
		}

		private static TreeNode FindSearchHistoryNode(TreeNode parentNode, string searchQuery)
		{
			foreach (TreeNode node in parentNode.Nodes)
			{
				AddBrowsingItemTask nodeContent = node.Tag as AddBrowsingItemTask;

				if (searchQuery.Equals(nodeContent.Uri))
				{
					return node;
				}
			}

			return null;
		}
		
		private void BrowsingContentNodeContextMenu_Popup(object sender, EventArgs e)
		{
			ContextMenu contextMenu = sender as ContextMenu;
			TreeNode contentNode = contextMenu.Tag as TreeNode;
			AddBrowsingItemTask browsingRecord = contentNode.Tag as AddBrowsingItemTask;

			contextMenu.MenuItems.Clear();

			contextMenu.MenuItems.Add(new MenuItem("&Remove from history", (sender2, e2) =>
			{
				contentNode.Remove();
				BrowsingModel.SearchHistory.Remove(browsingRecord.Uri);
			}) { Name = "remove" });

			string uri = browsingRecord.Uri;

			if (string.IsNullOrEmpty(uri))
			{
				return;
			}

			string[] parts = uri.Split(new char[] { ':' });

			if (parts.Length == 0)
			{
				//MessageBox.Show("Please enter a valid search.", "Syntax Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			try
			{
				if (parts[0].Equals("search", StringComparison.OrdinalIgnoreCase)
					|| parts[0].Equals("query", StringComparison.OrdinalIgnoreCase))
				{
					string query = parts[1];
					int pageIndex = 1;

					if (!QueryParser.ParseQuerySearch(parts, out query, out pageIndex))
					{
						return;
					}

					if (File.Exists(SessionManager.GetSessionFileName(query, pageIndex)))
					{
						contextMenu.MenuItems.Add(new MenuItem("&Remove from cache", (sender2, e2) =>
						{
							SessionManager.ForgetSession(query, pageIndex);
							SessionManager.DeleteSession(query, pageIndex);
						})
						{ Name = "removeFromCache" });
					}
				}
				else if (parts[0].Equals("tag", StringComparison.OrdinalIgnoreCase)
					|| parts[0].Equals("tagged", StringComparison.OrdinalIgnoreCase))
				{
					int tagId;
					string tagType;
					string tagName;
					int pageIndex;

					if (!QueryParser.ParseTaggedSearch(parts, out tagId, out tagType, out tagName, out pageIndex))
					{
						MessageBox.Show("Please enter a valid search.", "Syntax Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
						return;
					}

					// TODO? support tag name?
					if (File.Exists(SessionManager.GetSessionFileName(tagId, pageIndex)))
					{
						contextMenu.MenuItems.Add(new MenuItem("&Remove from cache", (sender2, e2) =>
						{
							SessionManager.ForgetSession(tagId, pageIndex);
							SessionManager.DeleteSession(tagId, pageIndex);
						})
						{ Name = "removeFromCache" });
					}
				}
				else if (parts[0].Equals("recent", StringComparison.OrdinalIgnoreCase)
					|| parts[0].Equals("all", StringComparison.OrdinalIgnoreCase))
				{
					int pageIndex;

					if (!QueryParser.ParseRecentSearch(parts, out pageIndex))
					{
						MessageBox.Show("Please enter a valid search.", "Syntax Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
						return;
					}

					if (File.Exists(SessionManager.GetSessionFileName(pageIndex)))
					{
						contextMenu.MenuItems.Add(new MenuItem("&Remove from cache", (sender2, e2) =>
						{
							SessionManager.ForgetSession(pageIndex);
							SessionManager.DeleteSession(pageIndex);
						})
						{ Name = "removeFromCache" });
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Syntax Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void TreeViewContextMenu_Popup(object sender, EventArgs e)
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
			AddBrowsingItemTask browsingRecord = e.Node.Tag as AddBrowsingItemTask;

			if (browsingRecord != null)
			{
				SearchHandler.ParseAndExecuteSearchText(browsingRecord.Uri);
			}
			else
			{
				if (e.Action == TreeViewAction.ByKeyboard)
				{
					e.Node.ToggleCollapse();
				}
			}
		}
	}

	public class BrowsingTreeViewNodeSorter : IComparer
	{
		public BrowsingSortType SortType { get; set; }
		public SortOrder SortOrder { get; set; }

		public BrowsingTreeViewNodeSorter(BrowsingSortType sortType, SortOrder sortOrder)
		{
			SortType = sortType;
			SortOrder = sortOrder;
		}

		public int Compare(object x, object y)
		{
			TreeNode lhs = (TreeNode)x;
			TreeNode rhs = (TreeNode)y;

			if (lhs.Level == 0) // date groupings.
			{
				DateTime date1 = (DateTime)lhs.Tag;
				DateTime date2 = (DateTime)rhs.Tag;

				return (int)((date2 - date1).TotalSeconds); // descending
			}
			else
			{
				AddBrowsingItemTask date1 = lhs.Tag as AddBrowsingItemTask;
				AddBrowsingItemTask date2 = rhs.Tag as AddBrowsingItemTask;

				if (SortType == BrowsingSortType.CreationTime)
				{
					if (SortOrder == SortOrder.Descending)
					{
						return (int)((date2.Item.CreationTime - date1.Item.CreationTime).TotalSeconds);
					}
					else// if (SortOrder == SortOrder.Ascending)
					{
						return (int)((date1.Item.CreationTime - date2.Item.CreationTime).TotalSeconds);
					}
				}
				else if (SortType == BrowsingSortType.LastAccessTime)
				{
					if (SortOrder == SortOrder.Descending)
					{
						return (int)((date2.Item.LastAccessTime - date1.Item.LastAccessTime).TotalSeconds);
					}
					else// if (SortOrder == SortOrder.Ascending)
					{
						return (int)((date1.Item.LastAccessTime - date2.Item.LastAccessTime).TotalSeconds);
					}
				}
				else if (SortType == BrowsingSortType.LastWriteTime)
				{
					if (SortOrder == SortOrder.Descending)
					{
						return (int)((date2.Item.LastWriteTime - date1.Item.LastWriteTime).TotalSeconds);
					}
					else// if (SortOrder == SortOrder.Ascending)
					{
						return (int)((date1.Item.LastWriteTime - date2.Item.LastWriteTime).TotalSeconds);
					}
				}
				else
				{
					if (SortOrder == SortOrder.Descending)
					{
						return rhs.Index - lhs.Index;
					}
					else// if (SortOrder == SortOrder.Ascending)
					{
						return lhs.Index - rhs.Index;
					}
				}
			}
		}
	}
}
