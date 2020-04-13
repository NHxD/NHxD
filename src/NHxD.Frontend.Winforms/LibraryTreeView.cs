using Ash.System.Windows.Forms;
using NHxD.Plugin.ArchiveWriter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	public partial class LibraryTreeView : UserControl
	{
		private readonly TreeViewEx treeView;

		public TreeView TreeView => treeView;

		public LibraryFilter LibraryFilter { get; }
		public LibraryModel LibraryModel { get; }
		public List<IArchiveWriter> ArchiveWriters { get; }
		public WebBrowserTreeNodeToolTip WebBrowserToolTip { get; }
		public SearchHandler SearchHandler { get; }
		public CacheFileSystem CacheFileSystem { get; }

		public LibraryTreeView()
		{
			InitializeComponent();
		}

		public LibraryTreeView(LibraryFilter libraryFilter, LibraryModel libraryModel, List<IArchiveWriter> archiveWriters, WebBrowserTreeNodeToolTip webBrowserToolTip, SearchHandler searchHandler, ICacheFileSystem cacheFileSystem)
		{
			InitializeComponent();

			LibraryFilter = libraryFilter;
			LibraryModel = libraryModel;
			ArchiveWriters = archiveWriters;
			SearchHandler = searchHandler;
			CacheFileSystem = cacheFileSystem as CacheFileSystem;	// lazy hack.
			WebBrowserToolTip = webBrowserToolTip;

			treeView = new TreeViewEx();

			SuspendLayout();

			//
			//
			//
			treeView.HideSelection = false;
			treeView.HotTracking = true;
			treeView.Dock = DockStyle.Fill;
			treeView.Sorted = true;
			treeView.TreeViewNodeSorter = new LibraryTreeViewNodeSorter(LibrarySortType.CreationTime, SortOrder.Descending);
			treeView.AfterSelect += TreeView_AfterSelect;
			treeView.NodeActivated += TreeView_NodeActivated;
			treeView.NodeContextMenuRequested += TreeView_NodeContextMenuRequested;
			// TODO: add this once I have sorting options
			//treeViewContextMenu = new ContextMenu();
			//treeViewContextMenu.Popup += TreeViewContextMenu_Popup;
			//treeView.ContextMenu = treeViewContextMenu;

			//
			// this
			//
			Controls.Add(treeView);

			LibraryFilter.SortTypeChanged += LibraryFilter_SortTypeChanged;
			LibraryFilter.SortOrderChanged += LibraryFilter_SortOrderChanged;
			LibraryFilter.TextChanged += LibraryFilter_TextChanged;
			LibraryModel.Poll += LibraryModel_Poll;

			ResumeLayout(false);
		}

		private void LibraryFilter_TextChanged(object sender, EventArgs e)
		{
			Populate(x => LibraryFilter.ShouldFilter(x));
		}

		private void LibraryFilter_SortTypeChanged(object sender, EventArgs e)
		{
			TreeView.TreeViewNodeSorter = new LibraryTreeViewNodeSorter(LibraryFilter.SortType, LibraryFilter.SortOrder);
		}

		private void LibraryFilter_SortOrderChanged(object sender, EventArgs e)
		{
			TreeView.TreeViewNodeSorter = new LibraryTreeViewNodeSorter(LibraryFilter.SortType, LibraryFilter.SortOrder);
		}

		private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			try
			{
				string fullName = e.Node.Tag as string;
				string fileName = Path.GetFileNameWithoutExtension(fullName);
				// HACK: expect filenames to be "{Id]"...
				// TODO: at least use a regexp to extract a number anywhere.
				int galleryId;

				if (int.TryParse(fileName, out galleryId))
				{
					SearchHandler.ShowDetails(galleryId);
				}
			}
			catch
			{

			}
		}

		private void TreeView_NodeContextMenuRequested(object sender, TreeNodeMouseClickEventArgs e)
		{
			ContextMenu nodeContextMenu = new ContextMenu();
			nodeContextMenu.Tag = e.Node;
			nodeContextMenu.Popup += NodeContextMenu_Popup;
			//e.Node.ContextMenu = treeViewContextMenu;
			nodeContextMenu.Show(treeView, e.Location);
		}

		private void TreeView_NodeActivated(object sender, TreeViewEventArgs e)
		{
			ActivateNode(e.Node);
		}

		private void LibraryModel_Poll(object sender, LibraryEventArgs e)
		{
			LibraryEvent fileSystemEvent = e.LibraryEvent;

			if (fileSystemEvent.EventType == LibraryEventType.Rename)
			{
				FileSystemWatcher_Renamed(LibraryModel.FileSystemWatcher, fileSystemEvent.EventData as RenamedEventArgs);
			}
			else if (fileSystemEvent.EventType == LibraryEventType.Create)
			{
				FileSystemWatcher_Created(LibraryModel.FileSystemWatcher, fileSystemEvent.EventData as FileSystemEventArgs);
			}
			else if (fileSystemEvent.EventType == LibraryEventType.Delete)
			{
				FileSystemWatcher_Deleted(LibraryModel.FileSystemWatcher, fileSystemEvent.EventData as FileSystemEventArgs);
			}
		}

		/*
		private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
		{
			
		}
		*/
		private void FileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
		{
			//FileSystemWatcher_Deleted(sender, new FileSystemEventArgs(WatcherChangeTypes.Deleted, e.OldFullPath, e.Name));
			foreach (TreeNode node in TreeView.Nodes)
			{
				if ((node.Tag as string).Equals(e.OldFullPath, StringComparison.OrdinalIgnoreCase))
				{
					node.Text = e.Name;
					node.Tag = e.FullPath;
					break;
				}
			}
		}

		private void FileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
		{
			foreach (TreeNode node in TreeView.Nodes)
			{
				if ((node.Tag as string).Equals(e.FullPath, StringComparison.OrdinalIgnoreCase))
				{
					WebBrowserToolTip.RemoveToolTip(node);

					node.Remove();
					break;
				}
			}
		}

		private void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
		{
			// TODO
			/*if (!ToolStrip.ShouldFilter(e.FullPath))
			{
				return;
			}*/

			// TODO: sanitize the name...
			TreeNode treeNode = new TreeNode(e.Name) { Tag = e.FullPath, Name = FileNameToControlName(e.Name) };

			treeView.Nodes.Add(treeNode);

			OnNodeAdded(treeNode);
		}

		private void ActivateNode(TreeNode node)
		{
			string fullName = node.Tag as string;

			if (File.Exists(fullName))
			{
				CacheFileSystem.OpenFile(fullName);
			}
			else if (Directory.Exists(fullName))
			{
				CacheFileSystem.OpenFolder(fullName);
			}
		}

		private void NodeContextMenu_Popup(object sender, EventArgs e)
		{
			ContextMenu contextMenu = sender as ContextMenu;
			TreeNode node = contextMenu.Tag as TreeNode;
			string path = node.Tag as string;

			if (File.Exists(path))
			{
				contextMenu.MenuItems.Add(new MenuItem("Open Archive", (sender2, e2) => { CacheFileSystem.OpenFile(path); }));
			}
			else
			{
				contextMenu.MenuItems.Add(new MenuItem("Open Folder", (sender2, e2) => { CacheFileSystem.OpenFolder(path); }));
			}

			contextMenu.MenuItems.Add(new MenuItem("Show in Explorer", (sender2, e2) => { CacheFileSystem.SelectFile(path); }));
		}

		public void Populate(Func<string, bool> predicate)
		{
			using (new CursorScope(Cursors.WaitCursor))
			{
				treeView.BeginUpdate();

				treeView.Nodes.Clear();

				if (Directory.Exists(LibraryModel.FileSystemWatcher.Path))
				{
					DirectoryInfo dirInfo = new DirectoryInfo(LibraryModel.FileSystemWatcher.Path);

					IEnumerable<DirectoryInfo> dirInfos = dirInfo.EnumerateDirectories();

					foreach (DirectoryInfo subDirInfo in dirInfos
						.Where(x => predicate.Invoke(x.FullName)))
					{
						TreeNode treeNode = new TreeNode(subDirInfo.Name) { Tag = subDirInfo.FullName, Name = FileNameToControlName(subDirInfo.Name) };

						treeView.Nodes.Add(treeNode);

						OnNodeAdded(treeNode);
					}

					// TODO: create a list of ArchiveReaders and use that instead of ArchiveWriters.
					if (ArchiveWriters != null)
					{
						foreach (IArchiveWriter archiveWriter in ArchiveWriters)
						{
							IEnumerable<FileInfo> fileInfos = dirInfo.EnumerateFiles("*" + archiveWriter.FileExtension);

							foreach (FileInfo subFileInfo in fileInfos
								.Where(x => predicate.Invoke(x.FullName)))
							{
								TreeNode treeNode = new TreeNode(subFileInfo.Name) { Tag = subFileInfo.FullName, Name = FileNameToControlName(subFileInfo.Name) };

								treeView.Nodes.Add(treeNode);

								OnNodeAdded(treeNode);
							}
						}
					}
				}

				treeView.EndUpdate();
			}
		}

		private void OnNodeAdded(TreeNode treeNode)
		{
			string fullName = treeNode.Tag as string;

			if (string.IsNullOrEmpty(fullName))
			{
				return;
			}

			int galleryId;

			if (int.TryParse(Path.GetFileNameWithoutExtension(fullName), out galleryId))
			{
				WebBrowserToolTip.SetToolTip(treeNode, galleryId);
			}
		}

		private static string FileNameToControlName(string fileName)
		{
			StringBuilder sb = new StringBuilder(fileName.Length);

			foreach (char c in fileName)
			{
				if (char.IsLetterOrDigit(c))
				{
					sb.Append(c);
				}
				else
				{
					sb.Append('_');
				}
			}

			return sb.ToString();
		}
	}

	public class LibraryTreeViewNodeSorter : IComparer
	{
		public LibrarySortType SortType { get; set; }
		public SortOrder SortOrder { get; set; }

		public LibraryTreeViewNodeSorter(LibrarySortType sortType, SortOrder sortOrder)
		{
			SortType = sortType;
			SortOrder = sortOrder;
		}

		public int Compare(object x, object y)
		{
			TreeNode lhs = (TreeNode)x;
			TreeNode rhs = (TreeNode)y;

			if (SortType == LibrarySortType.Title)
			{
				string fileName1 = lhs.Tag as string;
				string fileName2 = rhs.Tag as string;

				if (SortOrder == SortOrder.Descending)
				{
					return fileName2.CompareTo(fileName1);
				}
				else
				{
					return fileName1.CompareTo(fileName2);
				}
			}
			else if (SortType == LibrarySortType.CreationTime)
			{
				DateTime date1 = File.GetCreationTime(lhs.Tag as string);
				DateTime date2 = File.GetCreationTime(rhs.Tag as string);

				if (SortOrder == SortOrder.Descending)
				{
					return (int)((date2 - date1).TotalSeconds);
				}
				else// if (SortOrder == SortOrder.Ascending)
				{
					return (int)((date1 - date2).TotalSeconds);
				}
			}
			else if (SortType == LibrarySortType.LastAccessTime)
			{
				DateTime date1 = File.GetLastAccessTime(lhs.Tag as string);
				DateTime date2 = File.GetLastAccessTime(rhs.Tag as string);

				if (SortOrder == SortOrder.Descending)
				{
					return (int)((date2 - date1).TotalSeconds);
				}
				else// if (SortOrder == SortOrder.Ascending)
				{
					return (int)((date1 - date2).TotalSeconds);
				}
			}
			else if (SortType == LibrarySortType.LastWriteTime)
			{
				DateTime date1 = File.GetLastWriteTime(lhs.Tag as string);
				DateTime date2 = File.GetLastWriteTime(rhs.Tag as string);

				if (SortOrder == SortOrder.Descending)
				{
					return (int)((date2 - date1).TotalSeconds);
				}
				else// if (SortOrder == SortOrder.Ascending)
				{
					return (int)((date1 - date2).TotalSeconds);
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
