using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	public partial class MainForm : Form
	{
		private void BookmarksModel_ItemChanged(object sender, BookmarkItemChangeEventArgs e)
		{
			backgroundTaskWorker.AddBookmark(e);
		}

		private void BookmarksModel_ItemAdded(object sender, BookmarkItemChangeEventArgs e)
		{
			backgroundTaskWorker.AddBookmark(e);
		}

		private void BrowsingModel_ItemChanged(object sender, BrowsingItemChangeEventArgs e)
		{
			backgroundTaskWorker.AddBrowsing(e);
		}

		private void BrowsingModel_ItemAdded(object sender, BrowsingItemChangeEventArgs e)
		{
			backgroundTaskWorker.AddBrowsing(e);
		}

		private void TaskBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			if (e.UserState is AddBrowsingItemTask)
			{
				AddBrowsingItemTask task = e.UserState as AddBrowsingItemTask;

				if (browsingTreeView.Visible)
				{
					if (browsingFilter.ShouldFilter(new KeyValuePair<string, BrowsingItem>(task.Uri, task.Item)))
					{
						browsingTreeView.AddItem(task.Uri, task.Item);
					}
				}
			}
			else if (e.UserState is AddBookmarkItemTask)
			{
				AddBookmarkItemTask task = e.UserState as AddBookmarkItemTask;

				if (bookmarksTreeView.Visible)
				{
					if (bookmarksFilter.ShouldFilter(task.Item))
					{
						bookmarksTreeView.AddItem(task.Path, task.Item);
					}
				}
			}
		}
	}
}
