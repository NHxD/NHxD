using Nhentai;
using System;
using System.Globalization;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	public class TagsFilter
	{
		private TagsFilters filters;
		private TagSortType sortType;
		private SortOrder sortOrder;
		private string text;

		public TagsFilters Filters
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

		public TagSortType SortType
		{
			get
			{
				return sortType;
			}
			set
			{
				sortType = value;
				OnSortTypeChanged();
			}
		}

		public SortOrder SortOrder
		{
			get
			{
				return sortOrder;
			}
			set
			{
				sortOrder = value;
				OnSortOrderChanged();
			}
		}

		public event EventHandler FiltersChanged = delegate { };
		public event EventHandler SortTypeChanged = delegate { };
		public event EventHandler SortOrderChanged = delegate { };
		public event EventHandler TextChanged = delegate { };

		public Configuration.ConfigTagsList TagsListSettings { get; }
		public MetadataKeywordLists MetadataKeywordLists { get; }

		public TagsFilter(Configuration.ConfigTagsList tagsListSettings, MetadataKeywordLists metadataKeywordLists)
		{
			TagsListSettings = tagsListSettings;
			MetadataKeywordLists = metadataKeywordLists;

			text = "";
			filters = tagsListSettings.Filter.Filters;
			sortType = tagsListSettings.SortType;
			sortOrder = tagsListSettings.SortOrder;
		}

		protected virtual void OnFiltersChanged()
		{
			FiltersChanged.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnSortTypeChanged()
		{
			SortTypeChanged.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnSortOrderChanged()
		{
			SortOrderChanged.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnTextChanged()
		{
			TextChanged.Invoke(this, EventArgs.Empty);
		}

		public bool ShouldFilter(TagInfo tag, TagType tagType, string searchFilter)
		{
			return tag.Type == tagType
				&& ShouldFilterText(tag, tagType, searchFilter)
				&& ShouldFilterUserList(tag);
		}

		public bool ShouldFilterText(TagInfo tag, TagType tagType, string searchFilter)
		{
			return tag.Name.IndexOf(searchFilter, StringComparison.InvariantCultureIgnoreCase) >= 0
				|| tag.Id.ToString(CultureInfo.InvariantCulture).Equals(searchFilter);
		}

		public bool ShouldFilter(TagInfo tag)
		{
			return ShouldFilterText(tag)
				&& ShouldFilterUserList(tag);
		}

		public bool ShouldFilterText(TagInfo tag)
		{
			return tag.Name.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) >= 0
				|| tag.Id.ToString(CultureInfo.InvariantCulture).Equals(text);
		}

		public bool ShouldFilterUserList(TagInfo tag)
		{
			bool none = filters == TagsFilters.None;
			bool all = filters == TagsFilters.All;

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
				bool whitelist = filters.HasFlag(TagsFilters.Whitelist);
				bool blacklist = filters.HasFlag(TagsFilters.Blacklist);
				bool ignorelist = filters.HasFlag(TagsFilters.Ignorelist);
				bool hidelist = filters.HasFlag(TagsFilters.Hidelist);
				bool other = filters.HasFlag(TagsFilters.Other);

				return ((whitelist && MetadataKeywordLists.Whitelist.Any(tag))
					|| (blacklist && MetadataKeywordLists.Blacklist.Any(tag))
					|| (ignorelist && MetadataKeywordLists.Ignorelist.Any(tag))
					|| (hidelist && MetadataKeywordLists.Hidelist.Any(tag))
					|| (other && !MetadataKeywordLists.Whitelist.Any(tag) && !MetadataKeywordLists.Blacklist.Any(tag) && !MetadataKeywordLists.Ignorelist.Any(tag) && !MetadataKeywordLists.Hidelist.Any(tag)));
			}
		}
	}

	public enum TagSortType
	{
		Name,
		Count,
		DateAdded,
		Id,
	}

	public static class TagsFiltersExtensionMethods
	{
		public static TagsFilters ToggleFlag(this TagsFilters flags, TagsFilters flag)
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
