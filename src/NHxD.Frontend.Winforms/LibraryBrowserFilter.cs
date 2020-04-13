using System;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	public class LibraryBrowserFilter
	{
		private GallerySortType sortType;
		private SortOrder sortOrder;
		private LibrarySortType globalSortType;
		private SortOrder globalSortOrder;
		private string text;

		public GallerySortType SortType
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

		public LibrarySortType GlobalSortType
		{
			get
			{
				return globalSortType;
			}
			set
			{
				globalSortType = value;
				OnGlobalSortTypeChanged();
			}
		}

		public SortOrder GlobalSortOrder
		{
			get
			{
				return globalSortOrder;
			}
			set
			{
				globalSortOrder = value;
				OnGlobalSortOrderChanged();
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

		public event EventHandler SortTypeChanged = delegate { };
		public event EventHandler SortOrderChanged = delegate { };
		public event EventHandler GlobalSortTypeChanged = delegate { };
		public event EventHandler GlobalSortOrderChanged = delegate { };
		public event EventHandler TextChanged = delegate { };

		public Configuration.ConfigLibraryBrowserView LibraryBrowserSettings { get; }

		public LibraryBrowserFilter(Configuration.ConfigLibraryBrowserView libraryBrowserSettings)
		{
			LibraryBrowserSettings = libraryBrowserSettings;

			text = "";
			sortType = libraryBrowserSettings.SortType;
			sortOrder = libraryBrowserSettings.SortOrder;
			globalSortType = libraryBrowserSettings.GlobalSortType;
			globalSortOrder = libraryBrowserSettings.GlobalSortOrder;
		}

		protected virtual void OnSortTypeChanged()
		{
			SortTypeChanged.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnSortOrderChanged()
		{
			SortOrderChanged.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnGlobalSortTypeChanged()
		{
			GlobalSortTypeChanged.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnGlobalSortOrderChanged()
		{
			GlobalSortOrderChanged.Invoke(this, EventArgs.Empty);
		}

		protected virtual void OnTextChanged()
		{
			TextChanged.Invoke(this, EventArgs.Empty);
		}
	}

	public enum LibrarySortType
	{
		None,
		CreationTime,
		LastWriteTime,
		LastAccessTime,
		Title,
		Scanlator,
		UploadDate,
		NumPages,
		NumFavorites,
		Language,
		Tag,
		Artist,
		Group,
		Category,
		Parody,
		Character,
	}
}
