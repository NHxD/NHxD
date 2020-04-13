using Ash.System.Linq;
using Newtonsoft.Json;
using Nhentai;
using System.Linq;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	public class BookmarkPromptUtility
	{
		public BookmarksModel BookmarksModel { get; }
		public Configuration.ConfigBookmarksList BookmarkListSettings { get; }

		public BookmarkPromptUtility(BookmarksModel bookmarksModel, Configuration.ConfigBookmarksList bookmarkListSettings)
		{
			BookmarksModel = bookmarksModel;
			BookmarkListSettings = bookmarkListSettings;
		}

		public string ShowAddBookmarkPrompt()
		{
			string defaultValue = BookmarkListSettings.RestoreMostRecentPath ? BookmarkListSettings.MostRecentPath : "";
			string[] defaultValues = BookmarksModel.BookmarkFolders.OrderBy(x => x.Key, SortOrder.Ascending).Select(x => x.Key).ToArray();
			string result = PromptBox.Show("Add bookmark:", defaultValue, "Add Bookmark", defaultValues);

			if (result != null)
			{
				BookmarkListSettings.MostRecentPath = result;
			}

			return result;
		}

		public void ShowAddBookmarkPrompt(string metadataJson)
		{
			Metadata metadata = JsonConvert.DeserializeObject<Metadata>(metadataJson);

			if (metadata == null)
			{
				return;
			}

			string dialogResult = ShowAddBookmarkPrompt();

			if (dialogResult != null)
			{
				AddBookmark(metadataJson, dialogResult);
			}
		}

		public void ShowAddDownloadBookmarkPrompt(string metadataJson)
		{
			Metadata metadata = JsonConvert.DeserializeObject<Metadata>(metadataJson);

			if (metadata == null)
			{
				return;
			}

			string dialogResult = ShowAddBookmarkPrompt();

			if (dialogResult != null)
			{
				AddDownloadBookmark(metadataJson, dialogResult);
			}
		}

		public void ShowAddRecentBookmarkPrompt(int pageIndex)
		{
			string dialogResult = ShowAddBookmarkPrompt();

			if (dialogResult != null)
			{
				AddRecentBookmark(pageIndex, dialogResult);
			}
		}

		public void ShowAddQueryBookmarkPrompt(string query, int pageIndex)
		{
			string dialogResult = ShowAddBookmarkPrompt();

			if (dialogResult != null)
			{
				AddQueryBookmark(query, pageIndex, dialogResult);
			}
		}

		public void ShowAddTaggedBookmarkPrompt(int tagId, int pageIndex)
		{
			string dialogResult = ShowAddBookmarkPrompt();

			if (dialogResult != null)
			{
				AddTaggedBookmark(tagId, pageIndex, dialogResult);
			}
		}

		public void ShowAddLibraryBookmarkPrompt(int pageIndex)
		{
			string dialogResult = ShowAddBookmarkPrompt();

			if (dialogResult != null)
			{
				AddLibraryBookmark(pageIndex, dialogResult);
			}
		}


		public void AddBookmark(string metadataJson, string path)
		{
			Metadata metadata = JsonConvert.DeserializeObject<Metadata>(metadataJson);

			if (metadata == null)
			{
				return;
			}

			if (BookmarksModel.HasDetailsBookmark(metadata, path))
			{
				BookmarksModel.EditDetailsBookmark(metadata, path);
			}
			else
			{
				BookmarksModel.AddDetailsBookmark(metadata, path);
			}
		}

		public void AddDownloadBookmark(string metadataJson, string path)
		{
			Metadata metadata = JsonConvert.DeserializeObject<Metadata>(metadataJson);

			if (metadata == null)
			{
				return;
			}

			if (BookmarksModel.HasDownloadBookmark(metadata, path))
			{
				BookmarksModel.EditDownloadBookmark(metadata, path);
			}
			else
			{
				BookmarksModel.AddDownloadBookmark(metadata, path);
			}
		}

		public void AddLibraryBookmark(int pageIndex, string path)
		{
			if (BookmarksModel.HasLibraryBookmark(pageIndex, path))
			{
				BookmarksModel.EditLibraryBookmark(pageIndex, path);
			}
			else
			{
				BookmarksModel.AddLibraryBookmark(pageIndex, path);
			}
		}

		public void AddRecentBookmark(int pageIndex, string path)
		{
			if (BookmarksModel.HasRecentBookmark(pageIndex, path))
			{
				BookmarksModel.EditRecentBookmark(pageIndex, path);
			}
			else
			{
				BookmarksModel.AddRecentBookmark(pageIndex, path);
			}
		}

		public void AddQueryBookmark(string query, int pageIndex, string path)
		{
			if (BookmarksModel.HasQueryBookmark(query, pageIndex, path))
			{
				BookmarksModel.EditQueryBookmark(query, pageIndex, path);
			}
			else
			{
				BookmarksModel.AddQueryBookmark(query, pageIndex, path);
			}
		}

		public void AddTaggedBookmark(int tagId, int pageIndex, string path)
		{
			if (BookmarksModel.HasTaggedBookmark(tagId, pageIndex, path))
			{
				BookmarksModel.EditTaggedBookmark(tagId, pageIndex, path);
			}
			else
			{
				BookmarksModel.AddTaggedBookmark(tagId, pageIndex, path);
			}
		}
	}
}
