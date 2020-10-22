using Newtonsoft.Json;
using Nhentai;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;

// TODO: don't append Uri to Path, instead use a FullPath property (which is just an alias for Path + Uri).

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
		public event BookmarkFolderChangeEventHandler FolderChanged = delegate { };
		public event BookmarkFolderCancelEventHandler FolderAdding = delegate { };
		public event BookmarkFolderChangeEventHandler FolderAdded = delegate { };
		public event BookmarkFolderChangeEventHandler FolderRemoved = delegate { };

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

		protected virtual void OnFolderChanged(BookmarkFolderChangeEventArgs e)
		{
			FolderChanged.Invoke(this, e);
		}

		protected virtual void OnFolderAdded(BookmarkFolderChangeEventArgs e)
		{
			FolderAdded.Invoke(this, e);
		}

		protected virtual void OnFolderAdding(BookmarkFolderCancelEventArgs e)
		{
			FolderAdding.Invoke(this, e);
		}

		protected virtual void OnFolderRemoved(BookmarkFolderChangeEventArgs e)
		{
			FolderRemoved.Invoke(this, e);
		}

		public void CopyBookmarks(int level, string sourcePath, string targetPath)
		{
			if (sourcePath.Equals(targetPath, StringComparison.InvariantCulture))
			{
				return;
			}

			if (targetPath.StartsWith(sourcePath, StringComparison.InvariantCulture))
			{
				return;
			}

			foreach (KeyValuePair<string, BookmarkFolder> kvp in BookmarkFolders
				.Where(x => x.Key.StartsWith(sourcePath)).OrderBy(x => x.Key.Length).ToList())
			{
				string[] sourcePathParts = kvp.Key.Split(new char[] { '/' }).Skip(level).ToArray();
				string relativeSourcePath = string.Join("/", sourcePathParts);
				string combinedTargetPath = string.IsNullOrEmpty(targetPath) ? relativeSourcePath : (targetPath + "/" + relativeSourcePath);
				
				RegisterPath(combinedTargetPath, kvp.Value.Text);
			}

			foreach (string key in Bookmarks
				.Where(x => x.Key.StartsWith(sourcePath))
				.Select(x => x.Key).ToList())
			{
				string combinedTargetPath = CombineBookmarkPath(level, key, targetPath);

				CopyBookmark(key, combinedTargetPath);
			}
		}

		public string CombineBookmarkPath(int level, string key, string targetPath)
		{
			int lastPathSeparatorIndex = key.LastIndexOf('/');
			string keyPath = lastPathSeparatorIndex == -1 ? key : key.Substring(0, lastPathSeparatorIndex);
			string[] sourcePathParts = keyPath.Split(new char[] { '/' }).Skip(level).ToArray();
			string relativeSourcePath = string.Join("/", sourcePathParts);
			string combinedTargetPath = string.IsNullOrEmpty(targetPath) ? relativeSourcePath : (targetPath + "/" + relativeSourcePath);

			return combinedTargetPath;
		}

		public void MoveBookmarks(int level, string sourcePath, string targetPath)
		{
			if (sourcePath.Equals(targetPath, StringComparison.InvariantCulture))
			{
				return;
			}

			if (targetPath.StartsWith(sourcePath, StringComparison.InvariantCulture))
			{
				return;
			}

			CopyBookmarks(level, sourcePath, targetPath);
			RemoveBookmarks(sourcePath);
		}

		public void RemoveBookmarks(string sourcePath)
		{
			foreach (string key in Bookmarks
				.Where(x => x.Key.StartsWith(sourcePath))
				.Select(x => x.Key).ToList())
			{
				RemoveBookmark(key);
			}
		}

		public void CopyBookmark(string sourcePath, string targetPath)
		{
			if (sourcePath.Equals(targetPath, StringComparison.InvariantCulture))
			{
				return;
			}

			BookmarkNode sourceBookmark;

			if (!Bookmarks.TryGetValue(sourcePath, out sourceBookmark))
			{
				return;
			}

			RegisterPath(targetPath);

			string value = sourceBookmark.Value;
			string fullPath = string.IsNullOrEmpty(targetPath) ? value : string.Format(CultureInfo.InvariantCulture, "{0}/{1}", targetPath, value);
			BookmarkNode bookmark = new BookmarkNode();

			bookmark.Path = fullPath;
			bookmark.Value = sourceBookmark.Value;
			bookmark.Text = sourceBookmark.Text;

			Bookmarks.Add(fullPath, bookmark);

			OnItemAdded(new BookmarkItemChangeEventArgs(fullPath, bookmark));
		}

		public void MoveBookmark(string sourcePath, string targetPath)
		{
			if (!Bookmarks.ContainsKey(sourcePath))
			{
				return;
			}

			CopyBookmark(sourcePath, targetPath);
			RemoveBookmark(sourcePath);
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

				OnFolderRemoved(new BookmarkFolderChangeEventArgs(path, null));
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

				OnFolderRemoved(new BookmarkFolderChangeEventArgs(key, null));
			}
		}

		public BookmarkFolder RegisterPath(string path)
		{
			return RegisterPath(path, null);
		}

		public BookmarkFolder RegisterPath(string path, string customText)
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
					folder = new BookmarkFolder() { Path = subPath, Text = customText ?? part };

					BookmarkFolderCancelEventArgs e = new BookmarkFolderCancelEventArgs(subPath, folder);

					OnFolderAdding(e);

					if (e.Cancel)
					{
						return null;
					}

					BookmarkFolders.Add(subPath, folder);

					OnFolderAdded(new BookmarkFolderChangeEventArgs(subPath, folder));
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

	public delegate void BookmarkFolderChangeEventHandler(object sender, BookmarkFolderChangeEventArgs e);
	public delegate void BookmarkFolderCancelEventHandler(object sender, BookmarkFolderCancelEventArgs e);

	public class BookmarkFolderChangeEventArgs : EventArgs
	{
		public string Path { get; }
		public BookmarkFolder Value { get; }

		public BookmarkFolderChangeEventArgs(string path, BookmarkFolder value)
		{
			Path = path;
			Value = value;
		}
	}

	public class BookmarkFolderCancelEventArgs : CancelEventArgs
	{
		public string Path { get; }
		public BookmarkFolder Value { get; }

		public BookmarkFolderCancelEventArgs(string path, BookmarkFolder value)
		{
			Path = path;
			Value = value;
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
