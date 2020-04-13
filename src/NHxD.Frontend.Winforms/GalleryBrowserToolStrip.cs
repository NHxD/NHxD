using Ash.System.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	public partial class GalleryBrowserToolStrip : UserControl
	{
		private readonly ToolStrip toolStrip;
		private readonly ToolStripSpringComboBox searchComboBox;
		private readonly ToolStripSpringComboBox filterComboBox;
		private readonly ToolStripComboBox sortTypeComboBox;
		private readonly ToolStripComboBox sortOrderComboBox;

		public ToolStrip ToolStrip => toolStrip;
		public ToolStripSpringComboBox SearchComboBox => searchComboBox;
		public ToolStripSpringComboBox FilterComboBox => filterComboBox;
		public ToolStripComboBox SortTypeComboBox => sortTypeComboBox;
		public ToolStripComboBox SortOrderComboBox => sortOrderComboBox;

		public GalleryBrowserFilter GalleryBrowserFilter { get; }
		public GalleryModel GalleryModel { get; }
		public Configuration.ConfigGalleryBrowserView GalleryBrowserSettings { get; }
		public SearchHandler SearchHandler { get; }

		public GalleryBrowserToolStrip()
		{
			InitializeComponent();
		}

		public GalleryBrowserToolStrip(GalleryBrowserFilter galleryBrowserFilter, GalleryModel galleryModel, Configuration.ConfigGalleryBrowserView galleryBrowserSettings, SearchHandler searchHandler)
		{
			InitializeComponent();

			GalleryBrowserFilter = galleryBrowserFilter;
			GalleryModel = galleryModel;
			GalleryBrowserSettings = galleryBrowserSettings;
			SearchHandler = searchHandler;

			toolStrip = new ToolStrip();
			searchComboBox = new ToolStripSpringComboBox();
			filterComboBox = new ToolStripSpringComboBox();
			sortTypeComboBox = new ToolStripComboBox();
			sortOrderComboBox = new ToolStripComboBox();

			SuspendLayout();

			//
			// searchComboBox
			//
			searchComboBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
			searchComboBox.AutoCompleteSource = AutoCompleteSource.ListItems;
			searchComboBox.DropDownStyle = ComboBoxStyle.DropDown;
			searchComboBox.Dock = DockStyle.Fill;
			searchComboBox.Margin = new Padding(0, 0, 6, 0);
			searchComboBox.KeyDown += SearchComboBox_KeyDown;
			searchComboBox.ComboBox.SelectionChangeCommitted += SearchComboBox_SelectionChangeCommitted;
			// WORKAROUND: bug since win7 - selectedindex is wrong when a datasource is bound
			//searchComboBox.ComboBox.DataSource = galleryModel.Searches;
			searchComboBox.ComboBox.OverrideMouseWheelBehaviour();
			searchComboBox.OverrideUpDownKeys();

			//
			// filterComboBox
			//
			filterComboBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
			filterComboBox.AutoCompleteSource = AutoCompleteSource.ListItems;
			filterComboBox.DropDownStyle = ComboBoxStyle.DropDown;
			filterComboBox.Dock = DockStyle.Fill;
			filterComboBox.Margin = new Padding(0, 0, 6, 0);
			filterComboBox.KeyDown += FilterComboBox_KeyDown;
			filterComboBox.ComboBox.SelectedIndexChanged += ComboBox_SelectedIndexChanged;
			filterComboBox.ComboBox.SelectionChangeCommitted += FilterComboBox_SelectionChangeCommitted;
			filterComboBox.ComboBox.OverrideMouseWheelBehaviour();
			filterComboBox.EnableMiddleClickToClear();
			filterComboBox.OverrideUpDownKeys();

			//
			// sortTypeComboBox
			//
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
				new ComboBoxItem("orderByParody", "Parody", SortTypeComboBox_Parody, GallerySortType.Parody),
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
			sortTypeComboBox.SelectedItem = sortTypeItems.First(x => (GallerySortType)x.Tag == GalleryBrowserFilter.SortType);
			sortTypeComboBox.ComboBox.OverrideMouseWheelBehaviour();
			sortTypeComboBox.OverrideUpDownKeys();

			//
			// sortOrderComboBox
			//
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
			sortOrderComboBox.SelectedItem = sortOrderItems.First(x => (SortOrder)x.Tag == GalleryBrowserFilter.SortOrder);
			sortOrderComboBox.ComboBox.OverrideMouseWheelBehaviour();
			sortOrderComboBox.Enabled = GalleryBrowserFilter.SortType != GallerySortType.None;
			sortOrderComboBox.OverrideUpDownKeys();

			//
			// toolStrip
			//
			toolStrip.Dock = DockStyle.Fill;
			toolStrip.CanOverflow = false;
			toolStrip.Items.Add(searchComboBox);
			toolStrip.Items.Add(filterComboBox);
			toolStrip.Items.Add(sortTypeComboBox);
			toolStrip.Items.Add(sortOrderComboBox);

			//
			// this
			//
			Controls.Add(toolStrip);

			//GalleryModel.Searches.ListChanged += Searches_ListChanged;
			GalleryModel.SearchesChanged += GalleryModel_SearchesChanged;
			GalleryModel.FiltersChanged += GalleryModel_FiltersChanged;

			galleryBrowserFilter.SortTypeChanged += GalleryBrowserFilter_SortTypeChanged;
			galleryBrowserFilter.SortOrderChanged += GalleryBrowserFilter_SortOrderChanged;

			ResumeLayout(false);
		}

		private void Filter()
		{
			SearchHandler.ReloadGalleryBrowser();
		}

		private void GalleryModel_SearchesChanged(object sender, EventArgs e)
		{
			RebindSearchComboBox();
			searchComboBox.Text = searchComboBox.ComboBox.Items[0] as string;
		}

		private void GalleryModel_FiltersChanged(object sender, EventArgs e)
		{
			RebindFilterComboBox();
			filterComboBox.Text = filterComboBox.ComboBox.Items[0] as string;
		}

		private void GalleryBrowserFilter_SortTypeChanged(object sender, EventArgs e)
		{
			GalleryBrowserSettings.SortType = GalleryBrowserFilter.SortType;
			Filter();
		}

		private void GalleryBrowserFilter_SortOrderChanged(object sender, EventArgs e)
		{
			GalleryBrowserSettings.SortOrder = GalleryBrowserFilter.SortOrder;
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

		public void RebindSearchComboBox()
		{
			//DetailsModel.Searches.ResetBindings();
			searchComboBox.ComboBox.Items.Clear();
			searchComboBox.ComboBox.Items.AddRange(GalleryModel.Searches.Cast<object>().ToArray());
		}

		public void RebindFilterComboBox()
		{
			filterComboBox.ComboBox.Items.Clear();
			filterComboBox.ComboBox.Items.AddRange(GalleryModel.Filters.Cast<object>().ToArray());
		}

		/*private void Searches_ListChanged(object sender, ListChangedEventArgs e)
		{
			BindingList<string> bindingList = sender as BindingList<string>;

			if (e.ListChangedType == ListChangedType.ItemAdded)
			{
				searchComboBox.Text = bindingList[0];
			}
		}*/

		private void SearchComboBox_SelectionChangeCommitted(object sender, EventArgs e)
		{
			if (searchComboBox.SelectedIndex == -1)
			{
				return;
			}

			SubmitSearch();
		}

		private void SearchComboBox_KeyDown(object sender, KeyEventArgs e)
		{
			ToolStripComboBox comboBox = sender as ToolStripComboBox;

			if (e.KeyCode == Keys.Return)
			{
				SubmitSearch();

				e.Handled = true;
				e.SuppressKeyPress = true;
			}
			else if (e.KeyCode == Keys.Delete)
			{
				if (comboBox.SelectedIndex != -1)
				{
					GalleryModel.RemoveSearch(comboBox.SelectedItem as string);
					//comboBox.Items.RemoveAt(comboBox.SelectedIndex);

					e.Handled = true;
					e.SuppressKeyPress = true;
				}
			}
		}

		private void SubmitSearch()
		{
			string text = searchComboBox.SelectedIndex != -1 ? searchComboBox.Items[searchComboBox.SelectedIndex] as string : searchComboBox.Text;

			SearchHandler.ParseAndExecuteSearchText(text);
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
			sortOrderComboBox.Enabled = GalleryBrowserFilter.SortType != GallerySortType.None;
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

		private void SortTypeComboBox_None()
		{
			GalleryBrowserFilter.SortType = GallerySortType.None;
		}

		private void SortTypeComboBox_Title()
		{
			GalleryBrowserFilter.SortType = GallerySortType.Title;
		}

		private void SortTypeComboBox_Language()
		{
			GalleryBrowserFilter.SortType = GallerySortType.Language;
		}

		private void SortTypeComboBox_Artist()
		{
			GalleryBrowserFilter.SortType = GallerySortType.Artist;
		}

		private void SortTypeComboBox_Group()
		{
			GalleryBrowserFilter.SortType = GallerySortType.Group;
		}

		private void SortTypeComboBox_Tag()
		{
			GalleryBrowserFilter.SortType = GallerySortType.Tag;
		}

		private void SortTypeComboBox_Parody()
		{
			GalleryBrowserFilter.SortType = GallerySortType.Parody;
		}

		private void SortTypeComboBox_Character()
		{
			GalleryBrowserFilter.SortType = GallerySortType.Character;
		}

		private void SortTypeComboBox_Category()
		{
			GalleryBrowserFilter.SortType = GallerySortType.Category;
		}

		private void SortTypeComboBox_Scanlator()
		{
			GalleryBrowserFilter.SortType = GallerySortType.Scanlator;
		}

		private void SortTypeComboBox_UploadDate()
		{
			GalleryBrowserFilter.SortType = GallerySortType.UploadDate;
		}

		private void SortTypeComboBox_NumPages()
		{
			GalleryBrowserFilter.SortType = GallerySortType.NumPages;
		}

		private void SortTypeComboBox_NumFavorites()
		{
			GalleryBrowserFilter.SortType = GallerySortType.NumFavorites;
		}

		private void SortTypeComboBox_Id()
		{
			GalleryBrowserFilter.SortType = GallerySortType.Id;
		}
		/*
		private void SortTypeComboBox_Comiket()
		{
			GalleryBrowserFilter.SortType = GallerySortType.Comiket;
		}

		private void SortTypeComboBox_Version()
		{
			GalleryBrowserFilter.SortType = GallerySortType.Version;
		}

		private void SortTypeComboBox_Censorship()
		{
			GalleryBrowserFilter.SortType = GallerySortType.Censorship;
		}
		*/
		private void SortOrderComboBox_Ascending()
		{
			GalleryBrowserFilter.SortOrder = SortOrder.Ascending;
		}

		private void SortOrderComboBox_Descending()
		{
			GalleryBrowserFilter.SortOrder = SortOrder.Descending;
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

					GalleryModel.RemoveFilter(comboBox.SelectedItem as string);
					//comboBox.Items.RemoveAt(comboBox.SelectedIndex);

					e.Handled = true;
					e.SuppressKeyPress = true;
				}
			}
		}

		public void ApplyFilter()
		{
			GalleryBrowserFilter.Text = filterComboBox.Text;
		}

		private void SubmitFilter()
		{
			string text = filterComboBox.SelectedIndex != -1 ? filterComboBox.Items[filterComboBox.SelectedIndex] as string : filterComboBox.Text;

			GalleryBrowserFilter.Text = text;

			if (!string.IsNullOrEmpty(text))
			{
				GalleryModel.AddFilter(text);
			}
		}
	}

	public class ComboBoxItem
	{
		public string Name { get; }
		public string Text { get; }
		public Action Action { get; }
		public object Tag { get; }

		public ComboBoxItem(string name, string text, Action action)
			: this(name, text, action, null)
		{
		}

		public ComboBoxItem(string name, string text, Action action, object tag)
		{
			Name = name;
			Text = text;
			Action = action;
			Tag = tag;
		}

		public override string ToString()
		{
			return Text;
		}
	}
}
