using Ash.System.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	public partial class LibraryBrowserToolStrip : UserControl
	{
		private readonly ToolStrip toolStrip;
		private readonly ToolStripSpringComboBox filterComboBox;
		private readonly ToolStripComboBox sortTypeComboBox;
		private readonly ToolStripComboBox sortOrderComboBox;
		private readonly ToolStripComboBox globalSortTypeComboBox;
		private readonly ToolStripComboBox globalSortOrderComboBox;

		public ToolStrip ToolStrip => toolStrip;
		public ToolStripSpringComboBox FilterComboBox => filterComboBox;
		public ToolStripComboBox SortTypeComboBox => sortTypeComboBox;
		public ToolStripComboBox SortOrderComboBox => sortOrderComboBox;
		public ToolStripComboBox GlobalSortTypeComboBox => globalSortTypeComboBox;
		public ToolStripComboBox GlobalSortOrderComboBox => globalSortOrderComboBox;

		public LibraryBrowserFilter LibraryBrowserFilter { get; }
		public LibraryModel LibraryModel { get; }
		public Configuration.ConfigLibraryBrowserView LibraryBrowserSettings { get; }
		public SearchHandler SearchHandler { get; }

		public LibraryBrowserToolStrip()
		{
			InitializeComponent();
		}

		public LibraryBrowserToolStrip(LibraryBrowserFilter libraryBrowserFilter, LibraryModel libraryModel, Configuration.ConfigLibraryBrowserView libraryBrowserSettings, SearchHandler searchHandler)
		{
			InitializeComponent();

			LibraryBrowserFilter = libraryBrowserFilter;
			LibraryModel = libraryModel;
			LibraryBrowserSettings = libraryBrowserSettings;
			SearchHandler = searchHandler;

			toolStrip = new ToolStrip();
			filterComboBox = new ToolStripSpringComboBox();
			sortTypeComboBox = new ToolStripComboBox();
			sortOrderComboBox = new ToolStripComboBox();
			globalSortTypeComboBox = new ToolStripComboBox();
			globalSortOrderComboBox = new ToolStripComboBox();

			SuspendLayout();

			filterComboBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
			filterComboBox.AutoCompleteSource = AutoCompleteSource.ListItems;
			filterComboBox.DropDownStyle = ComboBoxStyle.DropDown;
			filterComboBox.Dock = DockStyle.Fill;
			filterComboBox.Margin = new Padding(0, 0, 6, 0);
			filterComboBox.KeyDown += FilterComboBox_KeyDown;
			filterComboBox.ComboBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
			filterComboBox.ComboBox.SelectionChangeCommitted += FilterComboBox_SelectionChangeCommitted;
			// WORKAROUND: bug since win7 - selectedindex is wrong when a datasource is bound
			//searchComboBox.ComboBox.DataSource = galleryModel.Searches;
			//galleryModel.Searches.ListChanged += Searches_ListChanged;
			filterComboBox.ComboBox.OverrideMouseWheelBehaviour();
			filterComboBox.EnableMiddleClickToClear();
			filterComboBox.OverrideUpDownKeys();

			sortTypeComboBox.Text = "&Sort";
			sortTypeComboBox.FlatStyle = FlatStyle.Flat;
			sortTypeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			List<ComboBoxItem> sortTypeItems = new List<ComboBoxItem>
			{
				new ComboBoxItem("orderByNone", "None", SortTypeComboBox_None, GallerySortType.None),
				new ComboBoxItem("orderByTitle", "Title", SortTypeComboBox_Title, GallerySortType.Title),
				new ComboBoxItem("orderByLanguage", "Language", SortTypeComboBox_Language, GallerySortType.Language),
				new ComboBoxItem("orderByArtist", "Artist", SortTypeComboBox_Artist, GallerySortType.Artist),
				new ComboBoxItem("orderByGroup", "Group", SortTypeComboBox_Group, GallerySortType.Group),
				new ComboBoxItem("orderByTag", "Tag", SortTypeComboBox_Tag, GallerySortType.Tag),
				new ComboBoxItem("orderByParody", "Parody", SortTypeComboBox_Parody, Tag = GallerySortType.Parody),
				new ComboBoxItem("orderByCharacter", "Character", SortTypeComboBox_Character, GallerySortType.Character),
				new ComboBoxItem("orderByCategory", "Category", SortTypeComboBox_Category, GallerySortType.Category),
				new ComboBoxItem("orderByScanlator", "Scanlator", SortTypeComboBox_Scanlator, GallerySortType.Scanlator),
				new ComboBoxItem("orderByUploadDate", "Upload Date", SortTypeComboBox_UploadDate, GallerySortType.UploadDate),
				new ComboBoxItem("orderByNumPages", "Num Pages", SortTypeComboBox_NumPages, GallerySortType.NumPages),
				new ComboBoxItem("orderByNumFavorites", "Num Favorites", SortTypeComboBox_NumFavorites, GallerySortType.NumFavorites),
				new ComboBoxItem("orderById", "Id", SortTypeComboBox_Id, GallerySortType.Id),
				//new SortItem("orderByComiket", "Comiket", SortTypeComboBox_Comiket, GallerySortType.Comiket),
				//new SortItem("orderByVersion", "Version", SortTypeComboBox_Version, GallerySortType.Version),
				//new SortItem("orderByCensorship", "Censorship", SortTypeComboBox_Censorship, GallerySortType.Censorship),
			};
			sortTypeComboBox.Items.AddRange(sortTypeItems.Cast<object>().ToArray());
			sortTypeComboBox.ComboBox.SelectionChangeCommitted += SortTypeComboBox_SelectionChangeCommitted;
			sortTypeComboBox.SelectedItem = sortTypeItems.First(x => (GallerySortType)x.Tag == LibraryBrowserFilter.SortType);
			sortTypeComboBox.ComboBox.OverrideMouseWheelBehaviour();
			sortTypeComboBox.OverrideUpDownKeys();

			sortOrderComboBox.Text = "&Sort";
			sortOrderComboBox.FlatStyle = FlatStyle.Flat;
			sortOrderComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			List<ComboBoxItem> sortOrderItems = new List<ComboBoxItem>
			{
				new ComboBoxItem("orderByAscending", "Ascending", SortOrderComboBox_Ascending, SortOrder.Ascending),
				new ComboBoxItem("orderByDescending", "Descending", SortOrderComboBox_Descending, SortOrder.Descending)
			};
			sortOrderComboBox.Items.AddRange(sortOrderItems.Cast<object>().ToArray());
			sortOrderComboBox.ComboBox.SelectionChangeCommitted += SortOrderComboBox_SelectionChangeCommitted;
			sortOrderComboBox.SelectedItem = sortOrderItems.First(x => (SortOrder)x.Tag == LibraryBrowserFilter.SortOrder);
			sortOrderComboBox.ComboBox.OverrideMouseWheelBehaviour();
			sortOrderComboBox.Enabled = LibraryBrowserFilter.SortType != GallerySortType.None;
			sortOrderComboBox.OverrideUpDownKeys();


			globalSortTypeComboBox.Text = "&Sort";
			globalSortTypeComboBox.FlatStyle = FlatStyle.Flat;
			globalSortTypeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			List<ComboBoxItem> globalSortTypeItems = new List<ComboBoxItem>
			{
				new ComboBoxItem("orderByNone", "None", GlobalSortTypeComboBox_None, LibrarySortType.None),
				new ComboBoxItem("orderByCreationTime", "Creation time", GlobalSortTypeComboBox_CreationTime, LibrarySortType.CreationTime),
				new ComboBoxItem("orderByLastAccessTime", "Last access time", GlobalSortTypeComboBox_LastAccessTime, LibrarySortType.LastAccessTime),
				new ComboBoxItem("orderByLastWriteTime", "Last write time", GlobalSortTypeComboBox_LastWriteTime, LibrarySortType.LastWriteTime),
			};
			globalSortTypeComboBox.Items.AddRange(globalSortTypeItems.Cast<object>().ToArray());
			globalSortTypeComboBox.ComboBox.SelectionChangeCommitted += GlobalSortTypeComboBox_SelectionChangeCommitted;
			globalSortTypeComboBox.SelectedItem = globalSortTypeItems.First(x => (LibrarySortType)x.Tag == LibraryBrowserFilter.GlobalSortType);
			globalSortTypeComboBox.ComboBox.OverrideMouseWheelBehaviour();
			globalSortTypeComboBox.OverrideUpDownKeys();

			globalSortOrderComboBox.Text = "&Sort";
			globalSortOrderComboBox.FlatStyle = FlatStyle.Flat;
			globalSortOrderComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			List<ComboBoxItem> globalSortOrderItems = new List<ComboBoxItem>
			{
				new ComboBoxItem("orderByAscending", "Ascending", GlobalSortOrderComboBox_Ascending, SortOrder.Ascending),
				new ComboBoxItem("orderByDescending", "Descending", GlobalSortOrderComboBox_Descending, SortOrder.Descending)
			};
			globalSortOrderComboBox.Items.AddRange(globalSortOrderItems.Cast<object>().ToArray());
			globalSortOrderComboBox.ComboBox.SelectionChangeCommitted += SortOrderComboBox_SelectionChangeCommitted;
			globalSortOrderComboBox.SelectedItem = globalSortOrderItems.First(x => (SortOrder)x.Tag == LibraryBrowserFilter.GlobalSortOrder);
			globalSortOrderComboBox.ComboBox.OverrideMouseWheelBehaviour();
			globalSortOrderComboBox.Enabled = LibraryBrowserFilter.GlobalSortType != LibrarySortType.None;
			globalSortOrderComboBox.OverrideUpDownKeys();

			toolStrip.Dock = DockStyle.Fill;
			toolStrip.CanOverflow = false;
			toolStrip.Items.Add(filterComboBox);
			toolStrip.Items.Add(sortTypeComboBox);
			toolStrip.Items.Add(sortOrderComboBox);
			toolStrip.Items.Add(globalSortTypeComboBox);
			toolStrip.Items.Add(globalSortOrderComboBox);

			//
			// this
			//
			Controls.Add(toolStrip);

			//LibraryModel.SearchesChanged += LibraryModel_SearchesChanged;
			LibraryModel.FiltersChanged += LibraryModel_FiltersChanged;

			libraryBrowserFilter.SortTypeChanged += LibraryBrowserFilter_SortTypeChanged;
			libraryBrowserFilter.SortOrderChanged += LibraryBrowserFilter_SortOrderChanged;
			libraryBrowserFilter.GlobalSortTypeChanged += LibraryBrowserFilter_GlobalSortTypeChanged;
			libraryBrowserFilter.GlobalSortOrderChanged += LibraryBrowserFilter_GlobalSortOrderChanged;

			ResumeLayout(false);
		}

		private void Filter()
		{
			SearchHandler.BrowseLibrary();
		}

		private void LibraryModel_FiltersChanged(object sender, EventArgs e)
		{
			RebindFilterComboBox();
			filterComboBox.Text = filterComboBox.ComboBox.Items[0] as string;
		}

		private void LibraryBrowserFilter_SortTypeChanged(object sender, EventArgs e)
		{
			LibraryBrowserSettings.SortType = LibraryBrowserFilter.SortType;
			Filter();
		}

		private void LibraryBrowserFilter_SortOrderChanged(object sender, EventArgs e)
		{
			LibraryBrowserSettings.SortOrder = LibraryBrowserFilter.SortOrder;
			Filter();
		}

		private void LibraryBrowserFilter_GlobalSortTypeChanged(object sender, EventArgs e)
		{
			LibraryBrowserSettings.GlobalSortType = LibraryBrowserFilter.GlobalSortType;
			Filter();
		}

		private void LibraryBrowserFilter_GlobalSortOrderChanged(object sender, EventArgs e)
		{
			LibraryBrowserSettings.GlobalSortOrder = LibraryBrowserFilter.GlobalSortOrder;
			Filter();
		}


		private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			ComboBox comboBox = sender as ComboBox;

			if (comboBox.SelectedIndex == -1)
			{
				SubmitFilter();
			}
		}

		public void RebindFilterComboBox()
		{
			filterComboBox.ComboBox.Items.Clear();
			filterComboBox.ComboBox.Items.AddRange(LibraryModel.Filters.Cast<object>().ToArray());
		}

		private void SortTypeComboBox_SelectionChangeCommitted(object sender, EventArgs e)
		{
			ComboBox comboBox = sender as ComboBox;
			ComboBoxItem sortItem = comboBox.SelectedItem as ComboBoxItem;

			if (sortItem == null)
			{
				return;
			}

			sortItem.Action.Invoke();
			sortOrderComboBox.Enabled = LibraryBrowserFilter.SortType != GallerySortType.None;
		}

		private void GlobalSortTypeComboBox_SelectionChangeCommitted(object sender, EventArgs e)
		{
			ComboBox comboBox = sender as ComboBox;
			ComboBoxItem sortItem = comboBox.SelectedItem as ComboBoxItem;

			if (sortItem == null)
			{
				return;
			}

			sortItem.Action.Invoke();
			globalSortOrderComboBox.Enabled = LibraryBrowserFilter.GlobalSortType != LibrarySortType.None;
		}

		private void SortOrderComboBox_SelectionChangeCommitted(object sender, EventArgs e)
		{
			ComboBox comboBox = sender as ComboBox;
			ComboBoxItem sortItem = comboBox.SelectedItem as ComboBoxItem;

			if (sortItem == null)
			{
				return;
			}

			sortItem.Action.Invoke();
		}

		private void GlobalSortTypeComboBox_None()
		{
			LibraryBrowserFilter.GlobalSortType = LibrarySortType.None;
		}

		private void GlobalSortTypeComboBox_CreationTime()
		{
			LibraryBrowserFilter.GlobalSortType = LibrarySortType.CreationTime;
		}

		private void GlobalSortTypeComboBox_LastWriteTime()
		{
			LibraryBrowserFilter.GlobalSortType = LibrarySortType.LastWriteTime;
		}

		private void GlobalSortTypeComboBox_LastAccessTime()
		{
			LibraryBrowserFilter.GlobalSortType = LibrarySortType.LastAccessTime;
		}

		private void GlobalSortOrderComboBox_Ascending()
		{
			LibraryBrowserFilter.GlobalSortOrder = SortOrder.Ascending;
		}

		private void GlobalSortOrderComboBox_Descending()
		{
			LibraryBrowserFilter.GlobalSortOrder = SortOrder.Descending;
		}

		private void SortTypeComboBox_None()
		{
			LibraryBrowserFilter.SortType = GallerySortType.None;
		}

		private void SortTypeComboBox_Title()
		{
			LibraryBrowserFilter.SortType = GallerySortType.Title;
		}

		private void SortTypeComboBox_Language()
		{
			LibraryBrowserFilter.SortType = GallerySortType.Language;
		}

		private void SortTypeComboBox_Artist()
		{
			LibraryBrowserFilter.SortType = GallerySortType.Artist;
		}

		private void SortTypeComboBox_Group()
		{
			LibraryBrowserFilter.SortType = GallerySortType.Group;
		}

		private void SortTypeComboBox_Tag()
		{
			LibraryBrowserFilter.SortType = GallerySortType.Tag;
		}

		private void SortTypeComboBox_Parody()
		{
			LibraryBrowserFilter.SortType = GallerySortType.Parody;
		}

		private void SortTypeComboBox_Character()
		{
			LibraryBrowserFilter.SortType = GallerySortType.Character;
		}

		private void SortTypeComboBox_Category()
		{
			LibraryBrowserFilter.SortType = GallerySortType.Category;
		}

		private void SortTypeComboBox_Scanlator()
		{
			LibraryBrowserFilter.SortType = GallerySortType.Scanlator;
		}

		private void SortTypeComboBox_UploadDate()
		{
			LibraryBrowserFilter.SortType = GallerySortType.UploadDate;
		}

		private void SortTypeComboBox_NumPages()
		{
			LibraryBrowserFilter.SortType = GallerySortType.NumPages;
		}

		private void SortTypeComboBox_NumFavorites()
		{
			LibraryBrowserFilter.SortType = GallerySortType.NumFavorites;
		}

		private void SortTypeComboBox_Id()
		{
			LibraryBrowserFilter.SortType = GallerySortType.Id;
		}
		/*
		private void SortTypeComboBox_Comiket()
		{
			LibraryBrowserFilter.SortType = GallerySortType.Comiket;
		}

		private void SortTypeComboBox_Version()
		{
			LibraryBrowserFilter.SortType = GallerySortType.Version;
		}

		private void SortTypeComboBox_Censorship()
		{
			LibraryBrowserFilter.SortType = GallerySortType.Censorship;
		}
		*/
		private void SortOrderComboBox_Ascending()
		{
			LibraryBrowserFilter.SortOrder = SortOrder.Ascending;
		}

		private void SortOrderComboBox_Descending()
		{
			LibraryBrowserFilter.SortOrder = SortOrder.Descending;
		}

		private void FilterComboBox_SelectionChangeCommitted(object sender, EventArgs e)
		{
			SubmitFilter();
		}

		private void FilterComboBox_KeyDown(object sender, KeyEventArgs e)
		{
			ToolStripComboBox comboBox = sender as ToolStripComboBox;

			if (e.KeyCode == Keys.Return)
			{
				SubmitFilter();

				e.Handled = true;
				e.SuppressKeyPress = true;
			}
			else if (e.KeyCode == Keys.Delete)
			{
				if (comboBox.SelectedIndex != -1)
				{
					// FIXME: this crashes sometimes.

					LibraryModel.RemoveFilter(comboBox.SelectedItem as string);
					//comboBox.Items.RemoveAt(comboBox.SelectedIndex);

					e.Handled = true;
					e.SuppressKeyPress = true;
				}
			}
		}

		public void ApplyFilter()
		{
			LibraryBrowserFilter.Text = filterComboBox.Text;
		}

		private void SubmitFilter()
		{
			string text = filterComboBox.SelectedIndex != -1 ? filterComboBox.Items[filterComboBox.SelectedIndex] as string : filterComboBox.Text;

			LibraryBrowserFilter.Text = text;

			if (!string.IsNullOrEmpty(text))
			{
				LibraryModel.AddFilter(text);
			}
		}
	}
}
