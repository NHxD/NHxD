using System;
using System.IO;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	public class LibraryFilter
	{
		private LibraryFilters filters;
		private LibraryDateFilters dateFilters;
		private DateTime beforeDate;
		private DateTime afterDate;
		private LibrarySortType sortType;
		private SortOrder sortOrder;
		private string text;

		public LibraryFilters Filters
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

		public LibraryDateFilters DateFilters
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

		public LibrarySortType SortType
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

		public Configuration.ConfigLibraryList LibraryListSettings { get; }

		public LibraryFilter(Configuration.ConfigLibraryList libraryListSettings)
		{
			LibraryListSettings = libraryListSettings;

			text = "";
			filters = libraryListSettings.Filter.Filters;
			dateFilters = libraryListSettings.Filter.DateFilters;
			afterDate = libraryListSettings.Filter.AfterDate;
			beforeDate = libraryListSettings.Filter.BeforeDate;
			sortType = libraryListSettings.SortType;
			sortOrder = libraryListSettings.SortOrder;
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

		public bool ShouldFilter(string value)
		{
			return ShouldFilterText(value)
				&& ShouldFilterType(value)
				&& ShouldFilterDate(value);
		}

		public bool ShouldFilterText(string value)
		{
			string searchFilter = text;

			try
			{
				return Path.GetFileName(value).IndexOf(searchFilter, StringComparison.InvariantCultureIgnoreCase) >= 0;
			}
			catch
			{
				return value.IndexOf(searchFilter, StringComparison.InvariantCultureIgnoreCase) >= 0;
			}
		}

		public bool ShouldFilterDate(string value)
		{
			bool after = dateFilters.HasFlag(LibraryDateFilters.After);
			bool before = dateFilters.HasFlag(LibraryDateFilters.Before);

			if (!after && !before)
			{
				return true;
			}
			else
			{
				DateTime date = DateTime.Now;

				if (sortType == LibrarySortType.CreationTime)
				{
					date = File.GetCreationTime(value);
				}
				else if (sortType == LibrarySortType.LastAccessTime)
				{
					date = File.GetLastAccessTime(value);
				}
				else if (sortType == LibrarySortType.LastWriteTime)
				{
					date = File.GetLastWriteTime(value);
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

		public bool ShouldFilterType(string value)
		{
			bool none = filters == LibraryFilters.None;
			bool all = filters == LibraryFilters.All;

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
				bool folder = filters.HasFlag(LibraryFilters.Folder);
				bool archive = filters.HasFlag(LibraryFilters.Archive);

				return ((folder && Directory.Exists(value))
					|| (archive && File.Exists(value)));
			}
		}
	}

	[Flags]
	public enum LibraryDateFilters
	{
		None = 0,
		After = 1,
		Before = 2,
		Between = After | Before,
		All = After | Before
	}

	[Flags]
	public enum LibraryFilters
	{
		None = 0,
		Folder = 1,
		Archive = 2,
		All = Folder | Archive
	}

	public static class LibraryFiltersExtensionMethods
	{
		public static LibraryFilters ToggleFlag(this LibraryFilters flags, LibraryFilters flag)
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

	public static class LibraryDateFiltersExtensionMethods
	{
		public static LibraryDateFilters ToggleFlag(this LibraryDateFilters flags, LibraryDateFilters flag)
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
