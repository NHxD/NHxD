using Newtonsoft.Json;
using Nhentai;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

// TODO: don't append Uri to Path, instead use a FullPath property (which is just an alias for Path + Uri).
// TODO: allow for editing the path for a folder node (it changes the folder's and its children's path).
// TODO: allow for editing the path for a bookmark node.

namespace NHxD
{
	public class BookmarksModel
	{
		private readonly Dictionary<string, BookmarkFolder> bookmarkFolders;
		private readonly Dictionary<string, BookmarkNode> bookmarks;

		public Dictionary<string, BookmarkFolder> BookmarkFolders => bookmarkFolders;
		public Dictionary<string, BookmarkNode> Bookmarks => bookmarks;

		public event BookmarkItemChangeEventHandler ItemChanged = delegate { };
		public event BookmarkItemChangeEventHandler ItemAdded = delegate { };
		public event BookmarkItemChangeEventHandler ItemRemoved = delegate { };

		public TagsModel TagsModel { get; }
		public IBookmarkFormatter BookmarkFormatter { get; }

		public BookmarksModel(TagsModel tagsModel, IBookmarkFormatter bookmarkFormatter)
		{
			TagsModel = tagsModel;
			BookmarkFormatter = bookmarkFormatter;

			bookmarkFolders = new Dictionary<string, BookmarkFolder>();
			bookmarks = new Dictionary<string, BookmarkNode>();
		}

		protected virtual void OnItemChanged(BookmarkItemChangeEventArgs e)
		{
			ItemChanged.Invoke(this, e);
		}

		protected virtual void OnItemAdded(BookmarkItemChangeEventArgs e)
		{
			ItemAdded.Invoke(this, e);
		}

		protected virtual void OnItemRemoved(BookmarkItemChangeEventArgs e)
		{
			ItemRemoved.Invoke(this, e);
		}

		public void RemovePath(string path)
		{
			foreach (string key in Bookmarks
				.Where(x => x.Key.Equals(path))
				.Select(x => x.Key).ToList())
			{
				Bookmarks.Remove(key);
			}

			if (BookmarkFolders.ContainsKey(path))
			{
				BookmarkFolders.Remove(path);
			}

			foreach (string key in Bookmarks
				.Where(x => x.Key.StartsWith(path + '/', StringComparison.OrdinalIgnoreCase))
				.Select(x => x.Key).ToList())
			{
				Bookmarks.Remove(key);
			}

			foreach (string key in BookmarkFolders
				.Where(x => x.Key.StartsWith(path + '/', StringComparison.OrdinalIgnoreCase))
				.Select(x => x.Key).ToList())
			{
				BookmarkFolders.Remove(key);
			}
		}

		public BookmarkFolder RegisterPath(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return null;
			}

			BookmarkFolder folder = null;
			string[] parts = path.Split(new char[] { '/' });

			for (int i = 0; i < parts.Length; ++i)
			{
				string part = parts[i];
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

				if (!BookmarkFolders.TryGetValue(subPath, out folder))
				{
					folder = new BookmarkFolder() { Path = subPath, Text = part };

					BookmarkFolders.Add(subPath, folder);
				}
			}

			return folder;
		}


		public void RemoveBookmark(string fullPath)
		{
			BookmarkNode bookmark;

			if (!Bookmarks.TryGetValue(fullPath, out bookmark))
			{
				return;
			}

			Bookmarks.Remove(fullPath);

			OnItemRemoved(new BookmarkItemChangeEventArgs(fullPath, bookmark));
		}


		public void AddDetailsBookmark(Metadata metadata, string path)
		{
			if (metadata == null)
			{
				throw new ArgumentNullException("metadata");
			}

			RegisterPath(path);

			string value = string.Format(CultureInfo.InvariantCulture, "details:{0}", metadata.Id);
			string fullPath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", path, value);
			BookmarkNode bookmark = new BookmarkNode();

			bookmark.Path = fullPath;
			bookmark.Value = value;
			bookmark.Text = BookmarkFormatter.GetDetailsText(metadata);

			Bookmarks.Add(fullPath, bookmark);

			OnItemAdded(new BookmarkItemChangeEventArgs(fullPath, bookmark));
		}

		public void EditDetailsBookmark(Metadata metadata, string path)
		{
			if (metadata == null)
			{
				throw new ArgumentNullException("metadata");
			}

			string value = string.Format(CultureInfo.InvariantCulture, "details:{0}", metadata.Id);
			string fullPath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", path, value);
			BookmarkNode bookmark;

			if (!Bookmarks.TryGetValue(fullPath, out bookmark))
			{
				return;
			}

			bookmark.Value = value;
			bookmark.Text = BookmarkFormatter.GetDetailsText(metadata);

			OnItemChanged(new BookmarkItemChangeEventArgs(fullPath, bookmark));
		}

		public bool HasDetailsBookmark(Metadata metadata, string path)
		{
			if (metadata == null)
			{
				throw new ArgumentNullException("metadata");
			}

			string value = string.Format(CultureInfo.InvariantCulture, "details:{0}", metadata.Id);
			string fullPath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", path, value);
			BookmarkNode bookmark;

			return Bookmarks.TryGetValue(fullPath, out bookmark);
		}


		public void AddDownloadBookmark(Metadata metadata, string path)
		{
			if (metadata == null)
			{
				throw new ArgumentNullException("metadata");
			}

			RegisterPath(path);

			string value = string.Format(CultureInfo.InvariantCulture, "download:{0}", metadata.Id);
			string fullPath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", path, value);
			BookmarkNode bookmark = new BookmarkNode();

			bookmark.Path = fullPath;
			bookmark.Value = value;
			bookmark.Text = BookmarkFormatter.GetDownloadText(metadata);

			Bookmarks.Add(fullPath, bookmark);

			OnItemAdded(new BookmarkItemChangeEventArgs(fullPath, bookmark));
		}

		public void EditDownloadBookmark(Metadata metadata, string path)
		{
			if (metadata == null)
			{
				throw new ArgumentNullException("metadata");
			}

			string value = string.Format(CultureInfo.InvariantCulture, "download:{0}", metadata.Id);
			string fullPath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", path, value);
			BookmarkNode bookmark;

			if (!Bookmarks.TryGetValue(fullPath, out bookmark))
			{
				return;
			}

			bookmark.Value = value;
			bookmark.Text = BookmarkFormatter.GetDownloadText(metadata);

			OnItemChanged(new BookmarkItemChangeEventArgs(fullPath, bookmark));
		}

		public bool HasDownloadBookmark(Metadata metadata, string path)
		{
			if (metadata == null)
			{
				throw new ArgumentNullException("metadata");
			}

			string value = string.Format(CultureInfo.InvariantCulture, "download:{0}", metadata.Id);
			string fullPath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", path, value);
			BookmarkNode bookmark;

			return Bookmarks.TryGetValue(fullPath, out bookmark);
		}


		public void AddTaggedBookmark(int tagId, int pageIndex, string path)
		{
			RegisterPath(path);

			string value = string.Format(CultureInfo.InvariantCulture, "tag:{0}:{1}", tagId, pageIndex);
			string fullPath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", path, value);
			BookmarkNode bookmark = new BookmarkNode();

			bookmark.Path = fullPath;
			bookmark.Value = value;
			bookmark.Text = BookmarkFormatter.GetTaggedSearchText(tagId, pageIndex);

			Bookmarks.Add(fullPath, bookmark);

			OnItemAdded(new BookmarkItemChangeEventArgs(fullPath, bookmark));
		}

		public void EditTaggedBookmark(int tagId, int pageIndex, string path)
		{
			string value = string.Format(CultureInfo.InvariantCulture, "tag:{0}:{1}", tagId, pageIndex);
			string fullPath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", path, value);
			BookmarkNode bookmark;

			if (!Bookmarks.TryGetValue(fullPath, out bookmark))
			{
				return;
			}

			bookmark.Value = value;
			bookmark.Text = BookmarkFormatter.GetTaggedSearchText(tagId, pageIndex);

			OnItemChanged(new BookmarkItemChangeEventArgs(fullPath, bookmark));
		}

		public bool HasTaggedBookmark(int tagId, int pageIndex, string path)
		{
			string value = string.Format(CultureInfo.InvariantCulture, "tag:{0}:{1}", tagId, pageIndex);
			string fullPath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", path, value);
			BookmarkNode bookmark;

			return Bookmarks.TryGetValue(fullPath, out bookmark);
		}



		public void AddQueryBookmark(string query, int pageIndex, string path)
		{
			RegisterPath(path);

			string value = string.Format(CultureInfo.InvariantCulture, "search:{0}:{1}", query, pageIndex);
			string fullPath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", path, value);
			BookmarkNode bookmark = new BookmarkNode();

			bookmark.Path = fullPath;
			bookmark.Value = value;
			bookmark.Text = BookmarkFormatter.GetQuerySearchText(query, pageIndex);

			Bookmarks.Add(fullPath, bookmark);

			OnItemAdded(new BookmarkItemChangeEventArgs(fullPath, bookmark));
		}

		public void EditQueryBookmark(string query, int pageIndex, string path)
		{
			string value = string.Format(CultureInfo.InvariantCulture, "search:{0}:{1}", query, pageIndex);
			string fullPath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", path, value);
			BookmarkNode bookmark;

			if (!Bookmarks.TryGetValue(fullPath, out bookmark))
			{
				return;
			}

			bookmark.Value = value;
			bookmark.Text = BookmarkFormatter.GetQuerySearchText(query, pageIndex);

			OnItemChanged(new BookmarkItemChangeEventArgs(fullPath, bookmark));
		}

		public bool HasQueryBookmark(string query, int pageIndex, string path)
		{
			string value = string.Format(CultureInfo.InvariantCulture, "search:{0}:{1}", query, pageIndex);
			string fullPath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", path, value);
			BookmarkNode bookmark;

			return Bookmarks.TryGetValue(fullPath, out bookmark);
		}


		public void AddRecentBookmark(int pageIndex, string path)
		{
			RegisterPath(path);

			string value = string.Format(CultureInfo.InvariantCulture, "recent:{0}", pageIndex);
			string fullPath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", path, value);
			BookmarkNode bookmark = new BookmarkNode();

			bookmark.Path = fullPath;
			bookmark.Value = value;
			bookmark.Text = BookmarkFormatter.GetRecentSearchText(pageIndex);

			Bookmarks.Add(fullPath, bookmark);

			OnItemAdded(new BookmarkItemChangeEventArgs(fullPath, bookmark));
		}

		public void EditRecentBookmark(int pageIndex, string path)
		{
			string value = string.Format(CultureInfo.InvariantCulture, "recent:{0}", pageIndex);
			string fullPath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", path, value);
			BookmarkNode bookmark;

			if (!Bookmarks.TryGetValue(fullPath, out bookmark))
			{
				return;
			}

			bookmark.Value = value;
			bookmark.Text = BookmarkFormatter.GetRecentSearchText(pageIndex);

			OnItemChanged(new BookmarkItemChangeEventArgs(fullPath, bookmark));
		}

		public bool HasRecentBookmark(int pageIndex, string path)
		{
			string value = string.Format(CultureInfo.InvariantCulture, "recent:{0}", pageIndex);
			string fullPath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", path, value);
			BookmarkNode bookmark;

			return Bookmarks.TryGetValue(fullPath, out bookmark);
		}


		public void AddLibraryBookmark(int pageIndex, string path)
		{
			RegisterPath(path);

			string value = string.Format(CultureInfo.InvariantCulture, "library:{0}", pageIndex);
			string fullPath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", path, value);
			BookmarkNode bookmark = new BookmarkNode();

			bookmark.Path = fullPath;
			bookmark.Value = value;
			bookmark.Text = BookmarkFormatter.GetLibraryText(pageIndex);

			Bookmarks.Add(fullPath, bookmark);

			OnItemAdded(new BookmarkItemChangeEventArgs(fullPath, bookmark));
		}

		public void EditLibraryBookmark(int pageIndex, string path)
		{
			string value = string.Format(CultureInfo.InvariantCulture, "library:{0}", pageIndex);
			string fullPath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", path, value);
			BookmarkNode bookmark;

			if (!Bookmarks.TryGetValue(fullPath, out bookmark))
			{
				return;
			}

			bookmark.Value = value;
			bookmark.Text = BookmarkFormatter.GetLibraryText(pageIndex);

			OnItemChanged(new BookmarkItemChangeEventArgs(fullPath, bookmark));
		}

		public bool HasLibraryBookmark(int pageIndex, string path)
		{
			string value = string.Format(CultureInfo.InvariantCulture, "library:{0}", pageIndex);
			string fullPath = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", path, value);
			BookmarkNode bookmark;

			return Bookmarks.TryGetValue(fullPath, out bookmark);
		}
	}

	public delegate void BookmarkItemChangeEventHandler(object sender, BookmarkItemChangeEventArgs e);

	public class BookmarkItemChangeEventArgs : EventArgs
	{
		public string Path { get; }
		public BookmarkNode Value { get; }

		public BookmarkItemChangeEventArgs(string path, BookmarkNode value)
		{
			Path = path;
			Value = value;
		}
	}

	public class BookmarkFolder
	{
		[JsonProperty("path")]
		public string Path { get; set; }

		[JsonProperty("text")]
		public string Text { get; set; }
	}

	public class BookmarkNode
	{
		// TODO: store an index instead.
		[JsonProperty("path")]
		public string Path { get; set; }

		[JsonProperty("value")]
		public string Value { get; set; }

		[JsonProperty("text")]
		public string Text { get; set; }
	}

	[Flags]
	public enum BookmarkFilters
	{
		None = 0,
		RecentSearch = 1,
		QuerySearch = 2,
		TaggedSearch = 4,
		Library = 8,
		Details = 16,
		Download = 32,
		All = RecentSearch | QuerySearch | TaggedSearch | Library | Details | Download,
	}

	public class AddBookmarkItemTask
	{
		public string Path { get; }
		public BookmarkNode Item { get; }

		public AddBookmarkItemTask(string path, BookmarkNode item)
		{
			Path = path;
			Item = item;
		}
	}

	public interface IBookmarkFormatter
	{
		//Formatter GetBookmarkFormatter(SearchArg searchArg);
		//Formatter GetBookmarkFormatter(Metadata metadata);
		string GetRecentSearchText(int pageIndex);
		string GetQuerySearchText(string query, int pageIndex);
		string GetTaggedSearchText(int tagId, int pageIndex);
		string GetLibraryText(int pageIndex);
		string GetDetailsText(Metadata metadata);
		string GetDownloadText(Metadata metadata);
	}
}
