using Ash.System.Windows.Forms;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	public partial class BookmarksToolStrip : UserControl
	{
		private readonly ToolStrip toolStrip;
		private readonly ToolStripSpringTextBox filterTextBox;
		private readonly ToolStripDropDownButton filterButton;
		private readonly ToolStripMenuItem filterNoneButton;
		private readonly ToolStripMenuItem filterAllButton;
		private readonly ToolStripMenuItem filterRecentSearchButton;
		private readonly ToolStripMenuItem filterQuerySearchButton;
		private readonly ToolStripMenuItem filterTaggedSearchButton;
		private readonly ToolStripMenuItem filterLibraryButton;
		private readonly ToolStripMenuItem filterDetailsButton;
		private readonly ToolStripMenuItem filterDownloadButton;
		private readonly Timer filterTimer;

		public ToolStrip ToolStrip => toolStrip;
		public ToolStripSpringTextBox FilterTextBox => filterTextBox;
		public ToolStripDropDownButton FilterButton => filterButton;

		public BookmarksFilter BookmarksFilter { get; }
		public BookmarksModel BookmarksModel { get; }
		public Configuration.ConfigBookmarksList BookmarksListSettings { get; }

		public BookmarksToolStrip()
		{
			InitializeComponent();
		}

		public BookmarksToolStrip(BookmarksFilter bookmarksFilter, BookmarksModel bookmarksModel, Configuration.ConfigBookmarksList bookmarksListSettings, int filterDelay)
		{
			InitializeComponent();

			BookmarksFilter = bookmarksFilter;
			BookmarksModel = bookmarksModel;
			BookmarksListSettings = bookmarksListSettings;

			filterTextBox = new ToolStripSpringTextBox();
			filterButton = new ToolStripDropDownButton();
			filterNoneButton = new ToolStripMenuItem();
			filterAllButton = new ToolStripMenuItem();
			filterRecentSearchButton = new ToolStripMenuItem();
			filterQuerySearchButton = new ToolStripMenuItem();
			filterTaggedSearchButton = new ToolStripMenuItem();
			filterLibraryButton = new ToolStripMenuItem();
			filterDetailsButton = new ToolStripMenuItem();
			filterDownloadButton = new ToolStripMenuItem();
			toolStrip = new ToolStrip();
			filterTimer = new Timer(components);

			SuspendLayout();

			//
			// filter textbox
			//
			filterTextBox.Dock = DockStyle.Fill;
			filterTextBox.Margin = new Padding(0, 0, 6, 0);
			filterTextBox.TextChanged += FilterTextBox_TextChanged;

			//
			// filter button
			//
			filterButton.Text = "&Filters";
			filterButton.AutoToolTip = false;

			filterNoneButton.Name = "none";
			filterNoneButton.Text = "&None";
			filterNoneButton.Click += FilterNoneButton_Click;

			filterAllButton.Name = "all";
			filterAllButton.Text = "&All";
			filterAllButton.Click += FilterAllButton_Click;

			filterRecentSearchButton.Name = "recent";
			filterRecentSearchButton.Text = "&Recent Searches";
			filterRecentSearchButton.Click += FilterRecentSearchButton_Click;

			filterQuerySearchButton.Name = "query";
			filterQuerySearchButton.Text = "&Query Searches";
			filterQuerySearchButton.Click += FilterQuerySearchButton_Click;

			filterTaggedSearchButton.Name = "tagged";
			filterTaggedSearchButton.Text = "&Tagged Searches";
			filterTaggedSearchButton.Click += FilterTaggedSearchButton_Click;

			filterLibraryButton.Name = "library";
			filterLibraryButton.Text = "&Library";
			filterLibraryButton.Click += FilterLibraryButton_Click;

			filterDetailsButton.Name = "details";
			filterDetailsButton.Text = "&Details";
			filterDetailsButton.Click += FilterDetailsButton_Click;

			filterDownloadButton.Name = "download";
			filterDownloadButton.Text = "&Download";
			filterDownloadButton.Click += FilterDownloadButton_Click;

			ContextMenuStrip filterContextMenuStrip = new ContextMenuStrip();

			filterContextMenuStrip.ShowCheckMargin = true;
			filterContextMenuStrip.Items.Add(filterNoneButton);
			filterContextMenuStrip.Items.Add(filterAllButton);
			filterContextMenuStrip.Items.Add(new ToolStripSeparator());
			filterContextMenuStrip.Items.Add(filterRecentSearchButton);
			filterContextMenuStrip.Items.Add(filterQuerySearchButton);
			filterContextMenuStrip.Items.Add(filterTaggedSearchButton);
			filterContextMenuStrip.Items.Add(filterLibraryButton);
			filterContextMenuStrip.Items.Add(filterDetailsButton);
			filterContextMenuStrip.Items.Add(filterDownloadButton);

			filterContextMenuStrip.Opening += Dropdown_Opening;
			filterContextMenuStrip.RenderMode = ToolStripRenderMode.Professional;
			filterContextMenuStrip.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;

			filterButton.DropDown = filterContextMenuStrip;

			//
			// filter timer
			//
			filterTimer.Interval = filterDelay;
			filterTimer.Tick += FilterTimer_Tick;

			//
			// toolstrip
			//
			toolStrip.Dock = DockStyle.Fill;
			toolStrip.CanOverflow = false;
			toolStrip.Items.Add(filterTextBox);
			toolStrip.Items.Add(filterButton);

			//
			// this
			//
			Controls.Add(toolStrip);

			bookmarksFilter.FiltersChanged += BookmarksFilter_FiltersChanged;

			ResumeLayout(false);
		}

		private void Dropdown_Opening(object sender, CancelEventArgs e)
		{
			BookmarkFilters filters = BookmarksFilter.Filters;

			filterNoneButton.Checked = filters == BookmarkFilters.None;
			filterAllButton.Checked = filters == BookmarkFilters.All;
			filterRecentSearchButton.Checked = filters.HasFlag(BookmarkFilters.RecentSearch);
			filterQuerySearchButton.Checked = filters.HasFlag(BookmarkFilters.QuerySearch);
			filterTaggedSearchButton.Checked = filters.HasFlag(BookmarkFilters.TaggedSearch);
			filterLibraryButton.Checked = filters.HasFlag(BookmarkFilters.Library);
			filterDetailsButton.Checked = filters.HasFlag(BookmarkFilters.Details);
			filterDownloadButton.Checked = filters.HasFlag(BookmarkFilters.Download);

			filterNoneButton.Enabled = filters != BookmarkFilters.None;
			filterAllButton.Enabled = filters != BookmarkFilters.All;
		}

		private void BookmarksFilter_FiltersChanged(object sender, EventArgs e)
		{
			BookmarksListSettings.Filter.Filters = BookmarksFilter.Filters;
			Filter();
		}

		private void FilterNoneButton_Click(object sender, EventArgs e)
		{
			BookmarksFilter.Filters = BookmarkFilters.None;
		}

		private void FilterAllButton_Click(object sender, EventArgs e)
		{
			BookmarksFilter.Filters = BookmarkFilters.All;
		}

		private void FilterRecentSearchButton_Click(object sender, EventArgs e)
		{
			BookmarksFilter.Filters = BookmarksFilter.Filters.ToggleFlag(BookmarkFilters.RecentSearch);
		}

		private void FilterQuerySearchButton_Click(object sender, EventArgs e)
		{
			BookmarksFilter.Filters = BookmarksFilter.Filters.ToggleFlag(BookmarkFilters.QuerySearch);
		}

		private void FilterTaggedSearchButton_Click(object sender, EventArgs e)
		{
			BookmarksFilter.Filters = BookmarksFilter.Filters.ToggleFlag(BookmarkFilters.TaggedSearch);
		}

		private void FilterLibraryButton_Click(object sender, EventArgs e)
		{
			BookmarksFilter.Filters = BookmarksFilter.Filters.ToggleFlag(BookmarkFilters.Library);
		}

		private void FilterDetailsButton_Click(object sender, EventArgs e)
		{
			BookmarksFilter.Filters = BookmarksFilter.Filters.ToggleFlag(BookmarkFilters.Details);
		}

		private void FilterDownloadButton_Click(object sender, EventArgs e)
		{
			BookmarksFilter.Filters = BookmarksFilter.Filters.ToggleFlag(BookmarkFilters.Download);
		}
		
		private void FilterTimer_Tick(object sender, EventArgs e)
		{
			filterTimer.Stop();

			BookmarksFilter.Text = filterTextBox.Text;
		}

		private void FilterTextBox_TextChanged(object sender, EventArgs e)
		{
			Filter();
		}

		public void Filter()
		{
			filterTimer.Stop();
			filterTimer.Start();
		}
	}
}
