using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	public class BrowsingFilter
	{
		private BrowsingFilters filters;
		private BrowsingDateFilters dateFilters;
		private DateTime beforeDate;
		private DateTime afterDate;
		private BrowsingSortType sortType;
		private SortOrder sortOrder;
		private string text;

		public BrowsingFilters Filters
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

		public BrowsingDateFilters DateFilters
		{
			get
			{
				return dateFilters;
			}
			set
			{
				dateFilters = value;
				OnDateFiltersChanged();
			}
		}

		public DateTime BeforeDate
		{
			get
			{
				return beforeDate;
			}
			set
			{
				beforeDate = value;
				OnBeforeDateChanged();
			}
		}

		public DateTime AfterDate
		{
			get
			{
				return afterDate;
			}
			set
			{
				afterDate = value;
				OnAfterDateChanged();
			}
		}

		public BrowsingSortType SortType
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
		public event EventHandler DateFiltersChanged = delegate { };
		public event EventHandler BeforeDateChanged = delegate { };
		public event EventHandler AfterDateChanged = delegate { };
		public event EventHandler SortTypeChanged = delegate { };
		public event EventHandler SortOrderChanged = delegate { };
		public event EventHandler TextChanged = delegate { };

		public Configuration.ConfigBrowsingList BrowsingListSettings { get; }

		public BrowsingFilter(Configuration.ConfigBrowsingList browsingListSettings)
		{
			BrowsingListSettings = browsingListSettings;

			text = "";
			filters = browsingListSettings.Filter.Filters;
			dateFilters = browsingListSettings.Filter.DateFilters;
			afterDate = browsingListSettings.Filter.AfterDate;
			beforeDate = browsingListSettings.Filter.BeforeDate;
			sortType = browsingListSettings.SortType;
			sortOrder = browsingListSettings.SortOrder;
		}

		protected virtual void OnFiltersChanged()
		{
			FiltersChanged.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnDateFiltersChanged()
		{
			DateFiltersChanged.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnBeforeDateChanged()
		{
			BeforeDateChanged.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnAfterDateChanged()
		{
			AfterDateChanged.Invoke(this, EventArgs.Empty);
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

		public bool ShouldFilter(KeyValuePair<string, BrowsingItem> value)
		{
			return ShouldFilterText(value)
				&& ShouldFilterType(value)
				&& ShouldFilterDate(value);
		}

		private bool ShouldFilterText(KeyValuePair<string, BrowsingItem> value)
		{
			string searchFilter = text;

			return value.Key.IndexOf(searchFilter, StringComparison.InvariantCultureIgnoreCase) >= 0;
		}

		private bool ShouldFilterDate(KeyValuePair<string, BrowsingItem> value)
		{
			bool after = dateFilters.HasFlag(BrowsingDateFilters.After);
			bool before = dateFilters.HasFlag(BrowsingDateFilters.Before);

			if (!after && !before)
			{
				return true;
			}
			else
			{
				DateTime date = DateTime.Now;

				if (sortType == BrowsingSortType.CreationTime)
				{
					date = value.Value.CreationTime;
				}
				else if (sortType == BrowsingSortType.LastAccessTime)
				{
					date = value.Value.LastAccessTime;
				}
				else if (sortType == BrowsingSortType.LastWriteTime)
				{
					date = value.Value.LastWriteTime;
				}

				if (after && before)
				{
					return (date > afterDate && date < beforeDate);
				}
				else if (!after && before)
				{
					return (date < beforeDate);
				}
				else if (after && !before)
				{
					return (date > afterDate);
				}

				return false;
			}
		}

		private bool ShouldFilterType(KeyValuePair<string, BrowsingItem> value)
		{
			bool none = filters == BrowsingFilters.None;
			bool all = filters == BrowsingFilters.All;

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
				bool recent = filters.HasFlag(BrowsingFilters.RecentSearch);
				bool query = filters.HasFlag(BrowsingFilters.QuerySearch);
				bool tagged = filters.HasFlag(BrowsingFilters.TaggedSearch);

				return ((recent && value.Key.StartsWith("recent:", StringComparison.OrdinalIgnoreCase))
					|| (query && value.Key.StartsWith("query:", StringComparison.OrdinalIgnoreCase))
					|| (tagged && value.Key.StartsWith("tagged:", StringComparison.OrdinalIgnoreCase)));
			}
		}
	}

	public enum BrowsingSortType
	{
		CreationTime,
		LastAccessTime,
		LastWriteTime,
	}

	[Flags]
	public enum BrowsingDateFilters
	{
		None = 0,
		After = 1,
		Before = 2,
		Between = After | Before,
		All = After | Before
	}

	[Flags]
	public enum BrowsingFilters
	{
		None = 0,
		RecentSearch = 1,
		QuerySearch = 2,
		TaggedSearch = 4,
		All = RecentSearch | QuerySearch | TaggedSearch
	}

	public static class BrowsingFiltersExtensionMethods
	{
		public static BrowsingFilters ToggleFlag(this BrowsingFilters flags, BrowsingFilters flag)
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

	public static class BrowsingDateFiltersExtensionMethods
	{
		public static BrowsingDateFilters ToggleFlag(this BrowsingDateFilters flags, BrowsingDateFilters flag)
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
