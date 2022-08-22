using Ash.System.Windows.Forms;
using Nhentai;
using System;
using System.Collections;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	public partial class TagsTreeView : UserControl
	{
		private readonly TreeViewEx treeView;
		private readonly ContextMenu treeViewContextMenu;

		private /*readonly*/ Font strikethroughFont;    // lazy loaded.

		public TreeView TreeView => treeView;

		public TagsFilter TagsFilter { get; }
		public TagsModel TagsModel { get; }
		public TagTextFormatter TagTextFormatter { get; }
		public Configuration.ConfigTagsList TagsListSettings { get; }
		public Configuration.ConfigNetwork NetworkSettings { get; }
		public MetadataKeywordLists MetadataKeywordLists { get; }
		public IPathFormatter PathFormatter { get; }
		public IMetadataCache MetadataCache { get; }
		public ISearchResultCache SearchResultCache { get; }
		public MetadataCacheSnapshot MetadataCacheSnapshot { get; }
		public Configuration.ConfigMetadataCache MetadataCacheSettings { get; }
		public SearchHandler SearchHandler { get; }
		public BookmarkPromptUtility BookmarkPromptUtility { get; }
		public HttpClient HttpClient { get; }

		public TagsTreeView()
		{
			InitializeComponent();
		}

		public TagsTreeView(TagsFilter tagsFilter, TagsModel tagsModel, TagTextFormatter tagTextFormatter
			, Configuration.ConfigTagsList tagsListSettings
			, Configuration.ConfigNetwork networkSettings
			, MetadataKeywordLists metadataKeywordLists
			, IPathFormatter pathFormatter
			, IMetadataCache metadataCache
			, ISearchResultCache searchResultCache
			, MetadataCacheSnapshot metadataCacheSnapshot
			, Configuration.ConfigMetadataCache metadataCacheSettings
			, SearchHandler searchHandler
			, BookmarkPromptUtility bookmarkPromptUtility
			, HttpClient httpClient)
		{
			InitializeComponent();

			TagsFilter = tagsFilter;
			TagsModel = tagsModel;
			TagTextFormatter = tagTextFormatter;
			TagsListSettings = tagsListSettings;
			NetworkSettings = networkSettings;
			MetadataKeywordLists = metadataKeywordLists;
			PathFormatter = pathFormatter;
			MetadataCache = metadataCache;
			SearchResultCache = searchResultCache;
			MetadataCacheSnapshot = metadataCacheSnapshot;
			MetadataCacheSettings = metadataCacheSettings;
			SearchHandler = searchHandler;
			BookmarkPromptUtility = bookmarkPromptUtility;
			HttpClient = httpClient;

			treeView = new TreeViewEx();
			treeViewContextMenu = new ContextMenu();

			SuspendLayout();

			//
			//
			//
			treeViewContextMenu.Popup += TreeViewContextMenu_Popup;
			treeViewContextMenu.Tag = treeView;

			//
			//
			//
			treeView.ContextMenu = treeViewContextMenu;
			treeView.HideSelection = false;
			treeView.HotTracking = true;
			treeView.Dock = DockStyle.Fill;
			treeView.Sorted = true;
			treeView.TreeViewNodeSorter = new TagsTreeViewNodeSorter(TagSortType.Name, SortOrder.Ascending);
			treeView.NodeActivated += TreeView_NodeActivated;
			treeView.NodeSelected += TreeView_NodeSelected;
			treeView.AfterExpand += TreeView_AfterExpand;
			treeView.AfterCollapse += TreeView_AfterCollapse;

			//
			// this
			//
			Controls.Add(treeView);

			MetadataKeywordLists.WhitelistChanged += Whitelist_Changed;
			MetadataKeywordLists.BlacklistChanged += Blacklist_Changed;
			MetadataKeywordLists.IgnorelistChanged += Ignorelist_Changed;
			MetadataKeywordLists.HidelistChanged += Hidelist_Changed;

			tagsFilter.TextChanged += TagsFilter_TextChanged;
			tagsFilter.SortTypeChanged += TagsFilter_SortTypeChanged;
			tagsFilter.SortOrderChanged += TagsFilter_SortOrderChanged;

			TagsModel.ItemAdded += TagsModel_ItemAdded;

			ResumeLayout(false);
		}

		private void TagsModel_ItemAdded(object sender, TagEventArgs e)
		{
			if (TagsListSettings.ForceRuntimeUpdate)
			{
				MetadataKeywordListChangedEventArgs mklEvent = new MetadataKeywordListChangedEventArgs(MetadataKeywordsListsEventType.Add, e.Tag.Type.ToString().ToLowerInvariant(), e.Tag.Name, e.Tag.Id);

				IAsyncResult asyncResult = BeginInvoke(new EventHandler<MetadataKeywordListChangedEventArgs>(TagsModel_ItemAddedMethod), new object[] { this, mklEvent });

				EndInvoke(asyncResult);
			}
		}

		private void TagsModel_ItemAddedMethod(object sender, MetadataKeywordListChangedEventArgs e)
		{
			MetadataKeywordLists_ListChanged(sender, e, (contentNode) => { }, (contentNode) => { });
		}

		private void TagsFilter_SortTypeChanged(object sender, EventArgs e)
		{
			TreeView.TreeViewNodeSorter = new TagsTreeViewNodeSorter(TagsFilter.SortType, TagsFilter.SortOrder);
		}

		private void TagsFilter_SortOrderChanged(object sender, EventArgs e)
		{
			TreeView.TreeViewNodeSorter = new TagsTreeViewNodeSorter(TagsFilter.SortType, TagsFilter.SortOrder);
		}

		private void TagsFilter_TextChanged(object sender, EventArgs e)
		{
			string text = TagsFilter.Text;

			// parse "category:name"
			if (text.Contains(':'))
			{
				string[] tokens = text.Split(new char[] { ':' }, 2);

				TagType tagType;

				if (Enum.TryParse(tokens[0], true, out tagType))
				{
					string searchFilter = tokens[1].ToLowerInvariant();

					Populate(x => TagsFilter.ShouldFilter(x, tagType, searchFilter));

					foreach (TreeNode categoryNode in TreeView.Nodes)
					{
						if ((TagType)categoryNode.Tag != tagType)
						{
							continue;
						}

						if (!categoryNode.IsExpanded)
						{
							categoryNode.Expand();
						}

						break;
					}
				}
			}
			// parse "name or id"
			else
			{
				Populate(x => TagsFilter.ShouldFilter(x));
			}
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
			|| !(node.Tag is TagType))
			{
				return;
			}

			TagType key = (TagType)node.Tag;

			if (TagsListSettings.Collapsed.ContainsKey(key))
			{
				TagsListSettings.Collapsed[key] = !node.IsExpanded;
			}
			else
			{
				TagsListSettings.Collapsed.Add(key, !node.IsExpanded);
			}
		}

		private void TreeView_NodeSelected(object sender, TreeViewEventArgs e)
		{
			TreeView_NodeActivated(sender, e);
		}

		private void TreeView_NodeActivated(object sender, TreeViewEventArgs e)
		{
			TagInfo tagInfo = e.Node.Tag as TagInfo;
			Enum tagType = e.Node.Tag as Enum;

			if (tagInfo != null)
			{
				if (TagsListSettings.BlockActions != TagsFilters.None)
				{
					bool isInWhitelist = MetadataKeywordLists.Whitelist[tagInfo.Type].Any(x => x.Equals(tagInfo.Name, StringComparison.OrdinalIgnoreCase));
					bool isInBlacklist = MetadataKeywordLists.Blacklist[tagInfo.Type].Any(x => x.Equals(tagInfo.Name, StringComparison.OrdinalIgnoreCase));
					bool isInIgnorelist = MetadataKeywordLists.Ignorelist[tagInfo.Type].Any(x => x.Equals(tagInfo.Name, StringComparison.OrdinalIgnoreCase));
					bool isInHidelist = MetadataKeywordLists.Hidelist[tagInfo.Type].Any(x => x.Equals(tagInfo.Name, StringComparison.OrdinalIgnoreCase));

					if (TagsListSettings.BlockActions.HasFlag(TagsFilters.Blacklist)
						&& isInBlacklist)
					{
						if (MessageBox.Show(string.Format(CultureInfo.CurrentUICulture, "The tag \"{0}\" is currently blacklisted. Are you sure you want to open this link?", tagInfo.Name), "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
						{
							return;
						}
					}
					else if (TagsListSettings.BlockActions.HasFlag(TagsFilters.Ignorelist)
						&& isInIgnorelist)
					{
						if (MessageBox.Show(string.Format(CultureInfo.CurrentUICulture, "The tag \"{0}\" is currently ignored. Are you sure you want to open this link?", tagInfo.Name), "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
						{
							return;
						}
					}
					else if (TagsListSettings.BlockActions.HasFlag(TagsFilters.Hidelist)
						&& isInHidelist)
					{
						if (MessageBox.Show(string.Format(CultureInfo.CurrentUICulture, "The tag \"{0}\" is currently hidden. Are you sure you want to open this link?", tagInfo.Name), "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
						{
							return;
						}
					}
					else if (TagsListSettings.BlockActions.HasFlag(TagsFilters.Other)
						&& !isInWhitelist
						&& !isInBlacklist
						&& !isInIgnorelist
						&& !isInHidelist)
					{
						if (MessageBox.Show(string.Format(CultureInfo.CurrentUICulture, "The tag \"{0}\" is currently unfiltered. Are you sure you want to open this link?", tagInfo.Name), "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
						{
							return;
						}
					}
				}

				SearchHandler.RunTaggedSearch(tagInfo.Id, 1);
			}
			else if (tagType.GetType() == typeof(TagType))
			{
				if (e.Action == TreeViewAction.ByKeyboard)
				{
					e.Node.ToggleCollapse();
				}
			}
		}

		public void Populate(Func<TagInfo, bool> predicate)
		{
			using (new CursorScope(Cursors.WaitCursor))
			{
				treeView.BeginUpdate();

				treeView.Nodes.Clear();

				Type tagTypeType = typeof(TagType);

				foreach (TagType tagType in Enum.GetValues(tagTypeType))
				{
					AddCategory(tagType, tagTypeType, predicate);
				}

				treeView.EndUpdate();
			}
		}

		private void AddCategory(TagType tagType, Type tagTypeType, Func<TagInfo, bool> predicate)
		{
			string tagTypeName = Enum.GetName(tagTypeType, tagType);
			TreeNode categoryNode = treeView.Nodes.Add(tagTypeName);

			foreach (TagInfo tag in TagsModel.AllTags
				.Where(x => x.Type == tagType)
				.Where(x => predicate.Invoke(x)))
			{
				AddItem(categoryNode, tag);
			}

			categoryNode.Tag = tagType;
			categoryNode.Text = TagTextFormatter.Format(new TagInfo() { Type = tagType, Count = categoryNode.Nodes.Count, Name = "", Url = "" }, TagsListSettings.LabelFormats.Header);

			if (TagsListSettings.Collapsed.ContainsKey(tagType))
			{
				if (TagsListSettings.Collapsed[tagType])
				{
					categoryNode.Collapse();
				}
				else
				{
					categoryNode.Expand();
				}
			}
		}

		private void AddItem(TreeNode categoryNode, TagInfo tag)
		{
			bool isInWhitelist = MetadataKeywordLists.Whitelist[tag.Type].Any(x => x.Equals(tag.Name, StringComparison.OrdinalIgnoreCase));
			bool isInBlacklist = MetadataKeywordLists.Blacklist[tag.Type].Any(x => x.Equals(tag.Name, StringComparison.OrdinalIgnoreCase));
			bool isInIgnorelist = MetadataKeywordLists.Ignorelist[tag.Type].Any(x => x.Equals(tag.Name, StringComparison.OrdinalIgnoreCase));
			bool isInHidelist = MetadataKeywordLists.Hidelist[tag.Type].Any(x => x.Equals(tag.Name, StringComparison.OrdinalIgnoreCase));

			string tagLabel = TagTextFormatter.Format(tag, TagsListSettings.LabelFormats.Tag);
			TreeNode contentNode = new TreeNode(tagLabel);

			if (isInIgnorelist)
			{
				if (strikethroughFont == null)
				{
					strikethroughFont = new Font(treeView.Font, FontStyle.Strikeout);
				}

				contentNode.NodeFont = strikethroughFont;
			}

			if (isInWhitelist)
			{
				contentNode.ForeColor = GetTreeNodeForeColor(TagsListSettings.Colors.Whitelist, isInHidelist);
			}
			else if (isInBlacklist)
			{
				contentNode.ForeColor = GetTreeNodeForeColor(TagsListSettings.Colors.Blacklist, isInHidelist);
			}
			else
			{
				contentNode.ForeColor = GetTreeNodeForeColor(TagsListSettings.Colors.Default, isInHidelist);
			}

			contentNode.Tag = tag;
			contentNode.ToolTipText = tag.Name;

			AddContextMenu(contentNode);

			categoryNode.Nodes.Add(contentNode);
		}

		// NOTE: ideally this would be automatically called in an OnNodeAdded event callback but since no such event exists...
		public void AddContextMenu(TreeNode contentNode)
		{
			ContextMenu contentNodeContextMenu = new ContextMenu();

			contentNodeContextMenu.Tag = contentNode;
			contentNodeContextMenu.Popup += ContentNodeContextMenu_Popup;

			contentNode.ContextMenu = contentNodeContextMenu;
		}

		private void ContentNodeContextMenu_Popup(object sender, EventArgs e)
		{
			ContextMenu contextMenu = sender as ContextMenu;
			TreeNode contentNode = contextMenu.Tag as TreeNode;
			TagInfo tag = contentNode.Tag as TagInfo;

			contextMenu.MenuItems.Clear();

			bool isInWhitelist = MetadataKeywordLists.Whitelist[tag.Type].Any(x => x.Equals(tag.Name, StringComparison.OrdinalIgnoreCase));
			bool isInBlacklist = MetadataKeywordLists.Blacklist[tag.Type].Any(x => x.Equals(tag.Name, StringComparison.OrdinalIgnoreCase));
			bool isInIgnorelist = MetadataKeywordLists.Ignorelist[tag.Type].Any(x => x.Equals(tag.Name, StringComparison.OrdinalIgnoreCase));
			bool isInHidelist = MetadataKeywordLists.Hidelist[tag.Type].Any(x => x.Equals(tag.Name, StringComparison.OrdinalIgnoreCase));

			if (isInWhitelist)
			{
				contentNode.ForeColor = GetTreeNodeForeColor(TagsListSettings.Colors.Whitelist, isInHidelist);
				contextMenu.MenuItems.Add(new MenuItem("&Remove from whitelist", (sender2, e2) =>
				{
					contentNode.ForeColor = GetTreeNodeForeColor(TagsListSettings.Colors.Default, isInHidelist);
					MetadataKeywordLists.Whitelist.Remove(tag);
				}) { Name = "whitelist_remove" });
			}
			else if (isInBlacklist)
			{
				contentNode.ForeColor = GetTreeNodeForeColor(TagsListSettings.Colors.Blacklist, isInHidelist);
				contextMenu.MenuItems.Add(new MenuItem("&Remove from blacklist", (sender2, e2) =>
				{
					contentNode.ForeColor = GetTreeNodeForeColor(TagsListSettings.Colors.Default, isInHidelist);
					MetadataKeywordLists.Blacklist.Remove(tag);
				}) { Name = "blacklist_remove" });
			}

			if (isInIgnorelist)
			{
				contentNode.NodeFont = strikethroughFont;
				contextMenu.MenuItems.Add(new MenuItem("&Remove from ignorelist", (sender2, e2) =>
				{
					contentNode.NodeFont = null;
					MetadataKeywordLists.Ignorelist.Remove(tag);
				}) { Name = "ignorelist_remove" });
			}

			if (isInHidelist)
			{
				contextMenu.MenuItems.Add(new MenuItem("&Remove from hidelist", (sender2, e2) =>
				{
					Color hidelistColor = GetHidelistColor(isInWhitelist, isInBlacklist);

					contentNode.ForeColor = GetTreeNodeForeColor(hidelistColor, false);
					MetadataKeywordLists.Hidelist.Remove(tag);
				}) { Name = "hidelist_remove" });
			}

			if (!isInWhitelist && !isInBlacklist)
			{
				contextMenu.MenuItems.Add(new MenuItem("&Add to whitelist", (sender2, e2) =>
				{
					contentNode.ForeColor = GetTreeNodeForeColor(TagsListSettings.Colors.Whitelist, isInHidelist);
					MetadataKeywordLists.Whitelist.Add(tag);
				}) { Name = "whitelist_add" });

				contextMenu.MenuItems.Add(new MenuItem("&Add to blacklist", (sender2, e2) =>
				{
					contentNode.ForeColor = GetTreeNodeForeColor(TagsListSettings.Colors.Blacklist, isInHidelist);
					MetadataKeywordLists.Blacklist.Add(tag);
				}) { Name = "blacklist_add" });
			}

			if (!isInIgnorelist)
			{
				contextMenu.MenuItems.Add(new MenuItem("&Add to ignorelist", (sender2, e2) =>
				{
					contentNode.NodeFont = strikethroughFont;
					MetadataKeywordLists.Ignorelist.Add(tag);
				}) { Name = "ignorelist_add" });
			}

			if (!isInHidelist)
			{
				contextMenu.MenuItems.Add(new MenuItem("&Add to hidelist", (sender2, e2) =>
				{
					Color hidelistColor = GetHidelistColor(isInWhitelist, isInBlacklist);

					contentNode.ForeColor = GetTreeNodeForeColor(hidelistColor, true);
					MetadataKeywordLists.Hidelist.Add(tag);
				}) { Name = "hidelist_add" });
			}

			if (contextMenu.MenuItems.Count > 0)
			{
				contextMenu.MenuItems.Add("-");
			}

			contextMenu.MenuItems.Add(new MenuItem("&Add bookmark", (sender2, e2) => { BookmarkPromptUtility.ShowAddTaggedBookmarkPrompt(tag.Id, 1); }) { Name = "search_definition" });

			if (tag.Type == TagType.Tag
				&& !NetworkSettings.Offline)
			{
				if (contextMenu.MenuItems.Count > 0)
				{
					contextMenu.MenuItems.Add("-");
				}

				contextMenu.MenuItems.Add(new MenuItem("&Search for definition", (sender2, e2) => { TagDefinition tagDefinition = new TagDefinition(tag.Name, HttpClient); tagDefinition.Search(); }) { Name = "search_definition" });
			}

			if (contextMenu.MenuItems.Count > 0)
			{
				contextMenu.MenuItems.Add("-");
			}

			//contextMenu.MenuItems.Add(new MenuItem("View in &library", (sender2, e2) => { SearchHandler.BrowseCache("tagged:" + tag.Id + ":1"); }) { Name = "view_in_library" });
			contextMenu.MenuItems.Add(new MenuItem("View in &cache", (sender2, e2) =>
			{
				/*
				on load: start worker 1 - discovery phase - searching for metadata...
				on load: start worker 2 - processing phase - adding metadata...
				on load: start worker 3 - processing phase - saving cache file...
				*/
				/*
				compression level:
				0 - none (save as plain json)
				1 - fastest (faster to save and load but larger file) (save as json.gz)
				2 - optimal (slower to save and load but smaller file) (save as json.gz)
				at program start up:
				synchronize full cache
				preload full cache file
				at program exit:
				delete full cache file
				*/
				if (!MetadataCacheSnapshot.IsReady)
				{
					if (MessageBox.Show(this, "To perform this operation, the cache needs to be built first. Do it now? (Go to File > Build)", "NHxD", MessageBoxButtons.YesNo) == DialogResult.Yes)
					{
						using (ManageMetadataCacheForm dialog = new ManageMetadataCacheForm(PathFormatter, MetadataCache, SearchResultCache, MetadataCacheSnapshot, MetadataCacheSettings))
						{
							var result = dialog.ShowDialog(this);
							/*
							if (!MetadataCacheSnapshot.IsReady)
							{
								MetadataCacheSnapshot.LoadFromFile();
							}

							SearchHandler.BrowseTaggedCache(tag.Id, 1);
							*/
						}
					}
				}

				if (MetadataCacheSnapshot.IsReady)
				{
					SearchHandler.BrowseTaggedCache(tag.Id, 1);
				}
				/*
				if (!MetadataCacheSnapshot.DoesExist)
				{
					if (MessageBox.Show(this, "The full metadata cache needs to be loaded first. This needs to be done only once, but might take a while... Proceed?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
					{
						MetadataCacheSnapshot.AggregateFromFiles();
						MetadataCacheSnapshot.SaveToFile();
					}
				}

				if (!MetadataCacheSnapshot.IsReady)
				{
					MetadataCacheSnapshot.LoadFromFile();
				}

				SearchHandler.BrowseTaggedCache(tag.Id, 1);
				*/
			}) { Name = "view_in_cache" });
		}

		private Color GetHidelistColor(bool isInWhitelist, bool isInBlacklist)
		{
			Color restoredColor;

			if (isInWhitelist)
			{
				restoredColor = TagsListSettings.Colors.Whitelist;
			}
			else if (isInBlacklist)
			{
				restoredColor = TagsListSettings.Colors.Blacklist;
			}
			else
			{
				restoredColor = TagsListSettings.Colors.Default;
			}

			return restoredColor;
		}

		public TreeNode GetTagTreeNode(string categoryName, string tagName, int tagId)
		{
			TagType tagType;

			if (!Enum.TryParse(categoryName, true, out tagType))
			{
				MessageBox.Show("Invalid tag type: " + categoryName, "Tag Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return null;
			}

			foreach (TreeNode categoryNode in treeView.Nodes)
			{
				if ((TagType)categoryNode.Tag != tagType)
				{
					continue;
				}

				foreach (TreeNode tagNode in categoryNode.Nodes)
				{
					TagInfo tagInfo = tagNode.Tag as TagInfo;

					if (tagInfo.Id != tagId)
					{
						continue;
					}

					return tagNode;
				}

				break;
			}
			
			return null;
		}

		public TreeNode GetCategoryTreeNode(string categoryName)
		{
			TagType tagType;

			if (!Enum.TryParse(categoryName, true, out tagType))
			{
				MessageBox.Show("Invalid tag type: " + categoryName, "Tag Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return null;
			}

			foreach (TreeNode categoryNode in treeView.Nodes)
			{
				if ((TagType)categoryNode.Tag != tagType)
				{
					continue;
				}

				return categoryNode;
			}

			return null;
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

		private void MetadataKeywordLists_ListChanged(object sender, MetadataKeywordListChangedEventArgs e, Action<TreeNode> onAdded, Action<TreeNode> onRemoved)
		{
			TreeNode contentNode = GetTagTreeNode(e.TagType, e.TagName, e.TagId);
			TreeNode categoryNode = GetCategoryTreeNode(e.TagType);

			if (contentNode != null)
			{
				TagInfo tagInfo = contentNode.Tag as TagInfo;

				if (e.EventType == MetadataKeywordsListsEventType.Add)
				{
					onAdded.Invoke(contentNode);
				}
				else if (e.EventType == MetadataKeywordsListsEventType.Remove)
				{
					onRemoved.Invoke(contentNode);
				}

				if (!TagsFilter.ShouldFilter(tagInfo))
				{
					contentNode.Remove();
				}
			}
			else
			{
				TagInfo tagInfo = new TagInfo() { Name = e.TagName, Id = e.TagId, Type = (TagType)Enum.Parse(typeof(TagType), e.TagType, true) };

				if (TagsFilter.ShouldFilter(tagInfo))
				{
					TagInfo tag = TagsModel.AllTags.FirstOrDefault(x => x.Id == e.TagId);

					contentNode = new TreeNode(TagTextFormatter.Format(tag, TagsListSettings.LabelFormats.Tag))
					{
						Tag = tag
					};

					if (e.EventType == MetadataKeywordsListsEventType.Add)
					{
						AddContextMenu(contentNode);

						onAdded.Invoke(contentNode);

						if (categoryNode != null)
						{
							categoryNode.Nodes.Add(contentNode);
						}
					}
					else if (e.EventType == MetadataKeywordsListsEventType.Remove)
					{
						onRemoved.Invoke(contentNode);

						contentNode.Remove();
					}
				}
			}

			// NOTE: there's no NodeAdded/NodeRemoved events.
			if (categoryNode != null)
			{
				TagType tagType = (TagType)categoryNode.Tag;

				categoryNode.Text = TagTextFormatter.Format(new TagInfo() { Type = tagType, Count = categoryNode.Nodes.Count, Name = "", Url = "" }, TagsListSettings.LabelFormats.Header);
			}
		}

		private void Whitelist_Changed(object sender, MetadataKeywordListChangedEventArgs e)
		{
			bool isInHidelist = MetadataKeywordLists.Hidelist[e.TagType].Any(x => x.Equals(e.TagName, StringComparison.OrdinalIgnoreCase));

			MetadataKeywordLists_ListChanged(sender, e,
				(contentNode) =>
				{
					contentNode.ForeColor = GetTreeNodeForeColor(TagsListSettings.Colors.Whitelist, isInHidelist);
				},
				(contentNode) =>
				{
					contentNode.ForeColor = GetTreeNodeForeColor(TagsListSettings.Colors.Default, isInHidelist);
				}
			);
		}

		private void Blacklist_Changed(object sender, MetadataKeywordListChangedEventArgs e)
		{
			bool isInHidelist = MetadataKeywordLists.Hidelist[e.TagType].Any(x => x.Equals(e.TagName, StringComparison.OrdinalIgnoreCase));

			MetadataKeywordLists_ListChanged(sender, e,
				(contentNode) =>
				{
					contentNode.ForeColor = GetTreeNodeForeColor(TagsListSettings.Colors.Blacklist, isInHidelist);
				},
				(contentNode) =>
				{
					contentNode.ForeColor = GetTreeNodeForeColor(TagsListSettings.Colors.Default, isInHidelist);
				}
			);
		}

		private void Ignorelist_Changed(object sender, MetadataKeywordListChangedEventArgs e)
		{
			MetadataKeywordLists_ListChanged(sender, e,
				(contentNode) =>
				{
					contentNode.NodeFont = strikethroughFont;
				},
				(contentNode) =>
				{
					contentNode.NodeFont = null;
				}
			);
		}

		private void Hidelist_Changed(object sender, MetadataKeywordListChangedEventArgs e)
		{
			bool isInWhitelist = MetadataKeywordLists.Whitelist[e.TagType].Any(x => x.Equals(e.TagName, StringComparison.OrdinalIgnoreCase));
			bool isInBlacklist = MetadataKeywordLists.Blacklist[e.TagType].Any(x => x.Equals(e.TagName, StringComparison.OrdinalIgnoreCase));

			MetadataKeywordLists_ListChanged(sender, e,
				(contentNode) =>
				{
					contentNode.ForeColor = GetTreeNodeForeColor(GetHidelistColor(isInWhitelist, isInBlacklist), true);
				},
				(contentNode) =>
				{
					contentNode.ForeColor = GetTreeNodeForeColor(GetHidelistColor(isInWhitelist, isInBlacklist), false);
				}
			);
		}

		private Color GetTreeNodeForeColor(Color baseColor, bool isInHidelist)
		{
			if (isInHidelist)
			{
				if (baseColor.GetBrightness() <= 0.1f)
				{
					return Color.Gray;
				}
				else
				{
					return ControlPaint.Light(baseColor, 25.0f);
				}
			}
			else
			{
				return baseColor;
			}
		}

		// TODO: handle the case of clicking inside the node (but outside the label) then releasing inside the label
		// TODO: handle the case of right clicking on a node (but outside the label) will select the item and show the node's context menu instead of the treeview's - handling BeforeSelect doesn't seem to work for right click.
	}

	public class TagsTreeViewNodeSorter : IComparer
	{
		public TagSortType SortType { get; set; }
		public SortOrder SortOrder { get; set; }

		public TagsTreeViewNodeSorter(TagSortType sortType, SortOrder sortOrder)
		{
			SortType = sortType;
			SortOrder = sortOrder;
		}

		public int Compare(object x, object y)
		{
			TreeNode lhs = (TreeNode)x;
			TreeNode rhs = (TreeNode)y;

			if (lhs.Level == 0) // tag types.
			{
				return lhs.Index - rhs.Index;
			}
			else
			{
				TagInfo tag1 = lhs.Tag as TagInfo;
				TagInfo tag2 = rhs.Tag as TagInfo;

				if (SortType == TagSortType.Name)
				{
					if (SortOrder == SortOrder.Descending)
					{
						return tag2.Name.CompareTo(tag1.Name);
					}
					else// if (SortOrder == SortOrder.Ascending)
					{
						return tag1.Name.CompareTo(tag2.Name);
					}
				}
				else if (SortType == TagSortType.Count)
				{
					if (SortOrder == SortOrder.Descending)
					{
						return tag2.Count.CompareTo(tag1.Count);
					}
					else// if (SortOrder == SortOrder.Ascending)
					{
						return tag1.Count.CompareTo(tag2.Count);
					}
				}
				else if (SortType == TagSortType.Id)
				{
					if (SortOrder == SortOrder.Descending)
					{
						return tag2.Id.CompareTo(tag1.Id);
					}
					else// if (SortOrder == SortOrder.Ascending)
					{
						return tag1.Id.CompareTo(tag2.Id);
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
