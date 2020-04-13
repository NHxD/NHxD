using System;

namespace NHxD.Frontend.Winforms
{
	public class BookmarksFilter
	{
		private BookmarkFilters filters;
		private string text;

		public BookmarkFilters Filters
		{
			get
			{
				return filters;
			}
			set
			{
				filters = value;
				OnFiltersChanged();
			}
		}

		public string Text
		{
			get
			{
				return text;
			}
			set
			{
				text = value;
				OnTextChanged();
			}
		}

		public event EventHandler FiltersChanged = delegate { };
		public event EventHandler TextChanged = delegate { };

		public Configuration.ConfigBookmarksList BookmarksListSettings { get; }

		public BookmarksFilter(Configuration.ConfigBookmarksList bookmarksListSettings)
		{
			BookmarksListSettings = bookmarksListSettings;

			text = "";
			filters = bookmarksListSettings.Filter.Filters;
		}

		protected virtual void OnFiltersChanged()
		{
			FiltersChanged.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnTextChanged()
		{
			TextChanged.Invoke(this, EventArgs.Empty);
		}

		public bool ShouldFilter(BookmarkNode value)
		{
			return ShouldFilterText(value)
				&& ShouldFilterType(value);
		}

		public bool ShouldFilterText(BookmarkNode value)
		{
			string searchFilter = text;

			return value.Text.IndexOf(searchFilter, StringComparison.InvariantCultureIgnoreCase) >= 0
				|| value.Path.IndexOf(searchFilter, StringComparison.InvariantCultureIgnoreCase) >= 0;
		}

		public bool ShouldFilterType(BookmarkNode value)
		{
			bool none = filters == BookmarkFilters.None;
			bool all = filters == BookmarkFilters.All;

			if (none)
			{
				return false;
			}
			else if (all)
			{
				return true;
			}
			else
			{
				bool recentSearch = filters.HasFlag(BookmarkFilters.RecentSearch);
				bool querySearch = filters.HasFlag(BookmarkFilters.QuerySearch);
				bool taggedSearch = filters.HasFlag(BookmarkFilters.TaggedSearch);
				bool library = filters.HasFlag(BookmarkFilters.Library);
				bool details = filters.HasFlag(BookmarkFilters.Details);
				bool download = filters.HasFlag(BookmarkFilters.Download);

				return ((recentSearch && value.Value.StartsWith("recent:", StringComparison.OrdinalIgnoreCase))
					|| (querySearch && value.Value.StartsWith("search:", StringComparison.OrdinalIgnoreCase))
					|| (taggedSearch && value.Value.StartsWith("tagged:", StringComparison.OrdinalIgnoreCase))
					|| (library && value.Value.StartsWith("library:", StringComparison.OrdinalIgnoreCase))
					|| (details && value.Value.StartsWith("details:", StringComparison.OrdinalIgnoreCase))
					|| (download && value.Value.StartsWith("download:", StringComparison.OrdinalIgnoreCase)));
			}
		}
	}

	public static class BookmarkFiltersExtensionMethods
	{
		public static BookmarkFilters ToggleFlag(this BookmarkFilters flags, BookmarkFilters flag)
		{
			if (flags.HasFlag(flag))
			{
				flags &= ~flag;
			}
			else
			{
				flags |= flag;
			}

			return flags;
		}
	}
}
