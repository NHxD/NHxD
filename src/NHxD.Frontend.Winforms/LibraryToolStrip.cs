using Ash.System.Windows.Forms;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	public partial class LibraryToolStrip : UserControl
	{
		private readonly ToolStrip toolStrip;
		private readonly ToolStripSpringTextBox filterTextBox;
		private readonly ToolStripDropDownButton dateFilterButton;
		private readonly ToolStripMenuItem dateFilterAfterButton;
		private readonly DateTimePicker dateFilterAfterDateTimePicker;
		private readonly ToolStripMenuItem dateFilterBeforeButton;
		private readonly DateTimePicker dateFilterBeforeDateTimePicker;
		private readonly ToolStripDropDownButton filterButton;
		private readonly ToolStripMenuItem filterNoneButton;
		private readonly ToolStripMenuItem filterAllButton;
		private readonly ToolStripMenuItem filterQuerySearchButton;
		private readonly ToolStripMenuItem filterTaggedSearchButton;
		private readonly ToolStripDropDownButton sortButton;
		private readonly ToolStripMenuItem sortTypeCreationTimeButton;
		private readonly ToolStripMenuItem sortTypeLastAccessTimeButton;
		private readonly ToolStripMenuItem sortTypeLastWriteTimeButton;
		private readonly ToolStripMenuItem sortOrderAscendingButton;
		private readonly ToolStripMenuItem sortOrderDescendingButton;
		private readonly Timer filterTimer;

		public ToolStrip ToolStrip => toolStrip;
		public ToolStripSpringTextBox FilterTextBox => filterTextBox;
		public ToolStripDropDownButton FilterButton => filterButton;
		public ToolStripDropDownButton SortButton => sortButton;

		public LibraryFilter LibraryFilter { get; }
		public LibraryModel LibraryModel { get; }
		public Configuration.ConfigLibraryList LibraryListSettings { get; }

		public LibraryToolStrip()
		{
			InitializeComponent();
		}

		public LibraryToolStrip(LibraryFilter libraryFilter, LibraryModel libraryModel, Configuration.ConfigLibraryList libraryListSettings, int filterDelay)
		{
			InitializeComponent();

			LibraryFilter = libraryFilter;
			LibraryModel = libraryModel;
			LibraryListSettings = libraryListSettings;

			toolStrip = new ToolStrip();
			filterTextBox = new ToolStripSpringTextBox();
			dateFilterButton = new ToolStripDropDownButton();
			dateFilterAfterButton = new ToolStripMenuItem();
			dateFilterAfterDateTimePicker = new DateTimePicker();
			dateFilterBeforeButton = new ToolStripMenuItem();
			dateFilterBeforeDateTimePicker = new DateTimePicker();
			filterButton = new ToolStripDropDownButton();
			filterNoneButton = new ToolStripMenuItem();
			filterAllButton = new ToolStripMenuItem();
			filterQuerySearchButton = new ToolStripMenuItem();
			filterTaggedSearchButton = new ToolStripMenuItem();
			sortButton = new ToolStripDropDownButton();
			sortTypeCreationTimeButton = new ToolStripMenuItem();
			sortTypeLastAccessTimeButton = new ToolStripMenuItem();
			sortTypeLastWriteTimeButton = new ToolStripMenuItem();
			sortOrderAscendingButton = new ToolStripMenuItem();
			sortOrderDescendingButton = new ToolStripMenuItem();
			filterTimer = new Timer();

			SuspendLayout();

			//
			// filter textbox
			//
			filterTextBox.Dock = DockStyle.Fill;
			filterTextBox.Margin = new Padding(0, 0, 6, 0);
			filterTextBox.TextChanged += FilterTextBox_TextChanged;

			//
			// date filter button
			//
			dateFilterButton.Text = "&Date";
			dateFilterButton.AutoToolTip = false;

			dateFilterAfterButton.Name = "after";
			dateFilterAfterButton.Text = "&After";
			dateFilterAfterButton.Click += DateFilterAfterButton_Click;

			dateFilterAfterDateTimePicker.ShowUpDown = true;
			dateFilterAfterDateTimePicker.ValueChanged += DateFilterAfterDateTimePicker_ValueChanged;

			dateFilterBeforeButton.Name = "before";
			dateFilterBeforeButton.Text = "&Before";
			dateFilterBeforeButton.Click += DateFilterBeforeButton_Click;

			dateFilterBeforeDateTimePicker.ShowUpDown = true;
			dateFilterBeforeDateTimePicker.ValueChanged += DateFilterBeforeDateTimePicker_ValueChanged;

			ContextMenuStrip dateFilterContextMenuStrip = new ContextMenuStrip();

			dateFilterContextMenuStrip.ShowCheckMargin = true;
			dateFilterContextMenuStrip.Items.Add(dateFilterAfterButton);
			ToolStripControlHost afterValueControlHost = new ToolStripControlHost(dateFilterAfterDateTimePicker);
			dateFilterContextMenuStrip.Items.Add(afterValueControlHost);
			dateFilterContextMenuStrip.Items.Add(new ToolStripSeparator());
			dateFilterContextMenuStrip.Items.Add(dateFilterBeforeButton);
			ToolStripControlHost beforeValueControlHost = new ToolStripControlHost(dateFilterBeforeDateTimePicker);
			dateFilterContextMenuStrip.Items.Add(beforeValueControlHost);

			dateFilterContextMenuStrip.Opening += DateFilterContextMenuStrip_Opening;
			dateFilterContextMenuStrip.RenderMode = ToolStripRenderMode.Professional;
			dateFilterContextMenuStrip.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;

			dateFilterButton.DropDown = dateFilterContextMenuStrip;

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

			filterQuerySearchButton.Name = "folders";
			filterQuerySearchButton.Text = "&Folders";
			filterQuerySearchButton.Click += FilterQuerySearchButton_Click;

			filterTaggedSearchButton.Name = "archives";
			filterTaggedSearchButton.Text = "&Archives";
			filterTaggedSearchButton.Click += FilterTaggedSearchButton_Click;

			ContextMenuStrip filterContextMenuStrip = new ContextMenuStrip();

			filterContextMenuStrip.ShowCheckMargin = true;
			filterContextMenuStrip.Items.Add(filterNoneButton);
			filterContextMenuStrip.Items.Add(filterAllButton);
			filterContextMenuStrip.Items.Add(new ToolStripSeparator());
			filterContextMenuStrip.Items.Add(filterQuerySearchButton);
			filterContextMenuStrip.Items.Add(filterTaggedSearchButton);

			filterContextMenuStrip.Opening += FilterContextMenuStrip_Opening;
			filterContextMenuStrip.RenderMode = ToolStripRenderMode.Professional;
			filterContextMenuStrip.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;

			filterButton.DropDown = filterContextMenuStrip;

			//
			// sort button
			//
			sortButton.Text = "&Sort";
			sortButton.AutoToolTip = false;

			sortTypeCreationTimeButton.Name = "creationTime";
			sortTypeCreationTimeButton.Text = "&Creation Time";
			sortTypeCreationTimeButton.Click += SortTypeCreationTimeButton_Click;

			sortTypeLastAccessTimeButton.Name = "lastAccessTime";
			sortTypeLastAccessTimeButton.Text = "Last &Access Time";
			sortTypeLastAccessTimeButton.Click += SortTypeLastAccessTimeButton_Click;

			sortTypeLastWriteTimeButton.Name = "lastWriteTime";
			sortTypeLastWriteTimeButton.Text = "Last &Write Time";
			sortTypeLastWriteTimeButton.Click += SortTypeLastWriteTimeButton_Click;

			sortOrderAscendingButton.Name = "sort_order_ascending";
			sortOrderAscendingButton.Text = "&Ascending";
			sortOrderAscendingButton.Click += SortOrderAscendingButton_Click;

			sortOrderDescendingButton.Name = "sort_order_descending";
			sortOrderDescendingButton.Text = "&Descending";
			sortOrderDescendingButton.Click += SortOrderDescendingButton_Click;

			ContextMenuStrip sortContextMenuStrip = new ContextMenuStrip();

			sortContextMenuStrip.ShowCheckMargin = true;
			sortContextMenuStrip.Items.Add(sortTypeCreationTimeButton);
			sortContextMenuStrip.Items.Add(sortTypeLastAccessTimeButton);
			sortContextMenuStrip.Items.Add(sortTypeLastWriteTimeButton);
			sortContextMenuStrip.Items.Add(new ToolStripSeparator());
			sortContextMenuStrip.Items.Add(sortOrderAscendingButton);
			sortContextMenuStrip.Items.Add(sortOrderDescendingButton);

			sortContextMenuStrip.Opening += SortContextMenuStrip_Opening;
			sortContextMenuStrip.RenderMode = ToolStripRenderMode.Professional;
			sortContextMenuStrip.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;

			sortButton.DropDown = sortContextMenuStrip;

			//
			// timer
			//
			filterTimer.Interval = filterDelay;
			filterTimer.Tick += FilterTimer_Tick;

			//
			// toolStrip
			//
			toolStrip.Dock = DockStyle.Fill;
			toolStrip.CanOverflow = false;
			toolStrip.Items.Add(filterTextBox);
			toolStrip.Items.Add(dateFilterButton);
			toolStrip.Items.Add(filterButton);
			toolStrip.Items.Add(sortButton);

			//
			// this
			//
			Controls.Add(toolStrip);

			libraryFilter.FiltersChanged += LibraryFilter_FiltersChanged;
			libraryFilter.DateFiltersChanged += LibraryFilter_DateFiltersChanged;
			libraryFilter.AfterDateChanged += LibraryFilter_AfterDateChanged;
			libraryFilter.BeforeDateChanged += LibraryFilter_BeforeDateChanged;
			libraryFilter.SortTypeChanged += LibraryFilter_SortTypeChanged;
			libraryFilter.SortOrderChanged += LibraryFilter_SortOrderChanged;

			ResumeLayout(false);
		}

		private void LibraryFilter_FiltersChanged(object sender, EventArgs e)
		{
			LibraryListSettings.Filter.Filters = LibraryFilter.Filters;
			Filter();
		}

		private void LibraryFilter_DateFiltersChanged(object sender, EventArgs e)
		{
			LibraryListSettings.Filter.DateFilters = LibraryFilter.DateFilters;
			Filter();
		}

		private void LibraryFilter_BeforeDateChanged(object sender, EventArgs e)
		{
			LibraryListSettings.Filter.BeforeDate = LibraryFilter.BeforeDate;
			Filter();
		}

		private void LibraryFilter_AfterDateChanged(object sender, EventArgs e)
		{
			LibraryListSettings.Filter.AfterDate = LibraryFilter.AfterDate;
			Filter();
		}

		private void LibraryFilter_SortTypeChanged(object sender, EventArgs e)
		{
			LibraryListSettings.SortType = LibraryFilter.SortType;
			Filter();
		}

		private void LibraryFilter_SortOrderChanged(object sender, EventArgs e)
		{
			LibraryListSettings.SortOrder = LibraryFilter.SortOrder;
			Filter();
		}


		private void DateFilterBeforeDateTimePicker_ValueChanged(object sender, EventArgs e)
		{
			LibraryFilter.BeforeDate = dateFilterBeforeDateTimePicker.Value.Date;
		}

		private void DateFilterAfterDateTimePicker_ValueChanged(object sender, EventArgs e)
		{
			LibraryFilter.AfterDate = dateFilterAfterDateTimePicker.Value.Date;
		}

		private void DateFilterContextMenuStrip_Opening(object sender, CancelEventArgs e)
		{
			dateFilterAfterButton.Checked = LibraryFilter.DateFilters.HasFlag(LibraryDateFilters.After);
			dateFilterBeforeButton.Checked = LibraryFilter.DateFilters.HasFlag(LibraryDateFilters.Before);
			
			dateFilterAfterDateTimePicker.Enabled = dateFilterAfterButton.Checked;
			dateFilterBeforeDateTimePicker.Enabled = dateFilterBeforeButton.Checked;
		}

		private void DateFilterAfterButton_Click(object sender, EventArgs e)
		{
			LibraryFilter.DateFilters = LibraryFilter.DateFilters.ToggleFlag(LibraryDateFilters.After);
		}

		private void DateFilterBeforeButton_Click(object sender, EventArgs e)
		{
			LibraryFilter.DateFilters = LibraryFilter.DateFilters.ToggleFlag(LibraryDateFilters.Before);
		}

		private void SortContextMenuStrip_Opening(object sender, CancelEventArgs e)
		{
			sortTypeCreationTimeButton.Checked = LibraryFilter.SortType == LibrarySortType.CreationTime;
			sortTypeLastAccessTimeButton.Checked = LibraryFilter.SortType == LibrarySortType.LastAccessTime;
			sortTypeLastWriteTimeButton.Checked = LibraryFilter.SortType == LibrarySortType.LastWriteTime;

			sortOrderAscendingButton.Checked = LibraryFilter.SortOrder == SortOrder.Ascending;
			sortOrderDescendingButton.Checked = LibraryFilter.SortOrder == SortOrder.Descending;

			sortTypeCreationTimeButton.Enabled = !sortTypeCreationTimeButton.Checked;
			sortTypeLastAccessTimeButton.Enabled = !sortTypeLastAccessTimeButton.Checked;
			sortTypeLastWriteTimeButton.Enabled = !sortTypeLastWriteTimeButton.Checked;

			sortOrderAscendingButton.Enabled = !sortOrderAscendingButton.Checked;
			sortOrderDescendingButton.Enabled = !sortOrderDescendingButton.Checked;
		}

		private void SortOrderDescendingButton_Click(object sender, EventArgs e)
		{
			LibraryFilter.SortOrder = SortOrder.Descending;
		}

		private void SortOrderAscendingButton_Click(object sender, EventArgs e)
		{
			LibraryFilter.SortOrder = SortOrder.Ascending;
		}

		private void SortTypeLastWriteTimeButton_Click(object sender, EventArgs e)
		{
			LibraryFilter.SortType = LibrarySortType.LastWriteTime;
		}

		private void SortTypeLastAccessTimeButton_Click(object sender, EventArgs e)
		{
			LibraryFilter.SortType = LibrarySortType.LastAccessTime;
		}

		private void SortTypeCreationTimeButton_Click(object sender, EventArgs e)
		{
			LibraryFilter.SortType = LibrarySortType.CreationTime;
		}

		private void FilterContextMenuStrip_Opening(object sender, CancelEventArgs e)
		{
			LibraryFilters filters = LibraryFilter.Filters;

			filterNoneButton.Checked = filters == LibraryFilters.None;
			filterAllButton.Checked = filters == LibraryFilters.All;
			filterQuerySearchButton.Checked = filters.HasFlag(LibraryFilters.Folder);
			filterTaggedSearchButton.Checked = filters.HasFlag(LibraryFilters.Archive);

			filterNoneButton.Enabled = filters != LibraryFilters.None;
			filterAllButton.Enabled = filters != LibraryFilters.All;
		}

		private void FilterNoneButton_Click(object sender, EventArgs e)
		{
			LibraryFilter.Filters = LibraryFilters.None;
		}

		private void FilterAllButton_Click(object sender, EventArgs e)
		{
			LibraryFilter.Filters = LibraryFilters.All;
		}

		private void FilterQuerySearchButton_Click(object sender, EventArgs e)
		{
			LibraryFilter.Filters = LibraryFilter.Filters.ToggleFlag(LibraryFilters.Folder);
		}

		private void FilterTaggedSearchButton_Click(object sender, EventArgs e)
		{
			LibraryFilter.Filters = LibraryFilter.Filters.ToggleFlag(LibraryFilters.Archive);
		}

		private void FilterTimer_Tick(object sender, EventArgs e)
		{
			filterTimer.Stop();

			LibraryFilter.Text = filterTextBox.Text;
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
