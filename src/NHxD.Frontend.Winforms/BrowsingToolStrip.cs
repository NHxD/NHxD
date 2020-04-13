using Ash.System.Windows.Forms;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	public partial class BrowsingToolStrip : UserControl
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
		private readonly ToolStripMenuItem filterRecentSearchButton;
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

		public BrowsingFilter BrowsingFilter { get; }
		public BrowsingModel BrowsingModel { get; }
		public Configuration.ConfigBrowsingList BrowsingListSettings { get; }
		
		public BrowsingToolStrip()
		{
			InitializeComponent();
		}

		public BrowsingToolStrip(BrowsingFilter browsingFilter, BrowsingModel browsingModel, Configuration.ConfigBrowsingList browsingListSettings, int filterDelay)
		{
			InitializeComponent();

			BrowsingFilter = browsingFilter;
			BrowsingModel = browsingModel;
			BrowsingListSettings = browsingListSettings;

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
			filterRecentSearchButton = new ToolStripMenuItem(); 
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

			filterRecentSearchButton.Name = "recent";
			filterRecentSearchButton.Text = "&Recent searches";
			filterRecentSearchButton.Click += FilterRecentSearchButton_Click;

			filterQuerySearchButton.Name = "query";
			filterQuerySearchButton.Text = "&Query searches";
			filterQuerySearchButton.Click += FilterQuerySearchButton_Click;

			filterTaggedSearchButton.Name = "tagged";
			filterTaggedSearchButton.Text = "&Tagged searches";
			filterTaggedSearchButton.Click += FilterTaggedSearchButton_Click;

			ContextMenuStrip filterContextMenuStrip = new ContextMenuStrip();

			filterContextMenuStrip.ShowCheckMargin = true;
			filterContextMenuStrip.Items.Add(filterNoneButton);
			filterContextMenuStrip.Items.Add(filterAllButton);
			filterContextMenuStrip.Items.Add(new ToolStripSeparator());
			filterContextMenuStrip.Items.Add(filterRecentSearchButton);
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

			browsingFilter.FiltersChanged += BrowsingFilter_FiltersChanged;
			browsingFilter.DateFiltersChanged += BrowsingFilter_DateFiltersChanged;
			browsingFilter.AfterDateChanged += BrowsingFilter_AfterDateChanged;
			browsingFilter.BeforeDateChanged += BrowsingFilter_BeforeDateChanged;
			browsingFilter.SortTypeChanged += BrowsingFilter_SortTypeChanged;
			browsingFilter.SortOrderChanged += BrowsingFilter_SortOrderChanged;

			ResumeLayout(false);
		}

		private void BrowsingFilter_FiltersChanged(object sender, EventArgs e)
		{
			BrowsingListSettings.Filter.Filters = BrowsingFilter.Filters;
			Filter();
		}

		private void BrowsingFilter_DateFiltersChanged(object sender, EventArgs e)
		{
			BrowsingListSettings.Filter.DateFilters = BrowsingFilter.DateFilters;
			Filter();
		}

		private void BrowsingFilter_BeforeDateChanged(object sender, EventArgs e)
		{
			BrowsingListSettings.Filter.BeforeDate = BrowsingFilter.BeforeDate;
			Filter();
		}

		private void BrowsingFilter_AfterDateChanged(object sender, EventArgs e)
		{
			BrowsingListSettings.Filter.AfterDate = BrowsingFilter.AfterDate;
			Filter();
		}

		private void BrowsingFilter_SortTypeChanged(object sender, EventArgs e)
		{
			BrowsingListSettings.SortType = BrowsingFilter.SortType;
			Filter();
		}

		private void BrowsingFilter_SortOrderChanged(object sender, EventArgs e)
		{
			BrowsingListSettings.SortOrder = BrowsingFilter.SortOrder;
			Filter();
		}

		private void DateFilterBeforeDateTimePicker_ValueChanged(object sender, EventArgs e)
		{
			BrowsingFilter.BeforeDate = dateFilterBeforeDateTimePicker.Value.Date;
		}

		private void DateFilterAfterDateTimePicker_ValueChanged(object sender, EventArgs e)
		{
			BrowsingFilter.AfterDate = dateFilterAfterDateTimePicker.Value.Date;
		}

		private void DateFilterContextMenuStrip_Opening(object sender, CancelEventArgs e)
		{
			dateFilterAfterButton.Checked = BrowsingFilter.DateFilters.HasFlag(BrowsingDateFilters.After);
			dateFilterBeforeButton.Checked = BrowsingFilter.DateFilters.HasFlag(BrowsingDateFilters.Before);
			
			dateFilterAfterDateTimePicker.Enabled = dateFilterAfterButton.Checked;
			dateFilterBeforeDateTimePicker.Enabled = dateFilterBeforeButton.Checked;
		}

		private void DateFilterAfterButton_Click(object sender, EventArgs e)
		{
			BrowsingFilter.DateFilters = BrowsingFilter.DateFilters.ToggleFlag(BrowsingDateFilters.After);
		}

		private void DateFilterBeforeButton_Click(object sender, EventArgs e)
		{
			BrowsingFilter.DateFilters = BrowsingFilter.DateFilters.ToggleFlag(BrowsingDateFilters.Before);
		}

		private void SortContextMenuStrip_Opening(object sender, CancelEventArgs e)
		{
			sortTypeCreationTimeButton.Checked = BrowsingFilter.SortType == BrowsingSortType.CreationTime;
			sortTypeLastAccessTimeButton.Checked = BrowsingFilter.SortType == BrowsingSortType.LastAccessTime;
			sortTypeLastWriteTimeButton.Checked = BrowsingFilter.SortType == BrowsingSortType.LastWriteTime;

			sortOrderAscendingButton.Checked = BrowsingFilter.SortOrder == SortOrder.Ascending;
			sortOrderDescendingButton.Checked = BrowsingFilter.SortOrder == SortOrder.Descending;

			sortTypeCreationTimeButton.Enabled = !sortTypeCreationTimeButton.Checked;
			sortTypeLastAccessTimeButton.Enabled = !sortTypeLastAccessTimeButton.Checked;
			sortTypeLastWriteTimeButton.Enabled = !sortTypeLastWriteTimeButton.Checked;

			sortOrderAscendingButton.Enabled = !sortOrderAscendingButton.Checked;
			sortOrderDescendingButton.Enabled = !sortOrderDescendingButton.Checked;
		}
		
		private void SortOrderDescendingButton_Click(object sender, EventArgs e)
		{
			BrowsingFilter.SortOrder = SortOrder.Descending;
		}

		private void SortOrderAscendingButton_Click(object sender, EventArgs e)
		{
			BrowsingFilter.SortOrder = SortOrder.Ascending;
		}

		private void SortTypeLastWriteTimeButton_Click(object sender, EventArgs e)
		{
			BrowsingFilter.SortType = BrowsingSortType.LastWriteTime;
		}

		private void SortTypeLastAccessTimeButton_Click(object sender, EventArgs e)
		{
			BrowsingFilter.SortType = BrowsingSortType.LastAccessTime;
		}

		private void SortTypeCreationTimeButton_Click(object sender, EventArgs e)
		{
			BrowsingFilter.SortType = BrowsingSortType.CreationTime;
		}

		private void FilterContextMenuStrip_Opening(object sender, CancelEventArgs e)
		{
			BrowsingFilters filters = BrowsingFilter.Filters;

			filterNoneButton.Checked = filters == BrowsingFilters.None;
			filterAllButton.Checked = filters == BrowsingFilters.All;
			filterRecentSearchButton.Checked = filters.HasFlag(BrowsingFilters.RecentSearch);
			filterQuerySearchButton.Checked = filters.HasFlag(BrowsingFilters.QuerySearch);
			filterTaggedSearchButton.Checked = filters.HasFlag(BrowsingFilters.TaggedSearch);

			filterNoneButton.Enabled = filters != BrowsingFilters.None;
			filterAllButton.Enabled = filters != BrowsingFilters.All;
		}

		private void FilterNoneButton_Click(object sender, EventArgs e)
		{
			BrowsingFilter.Filters = BrowsingFilters.None;
		}

		private void FilterAllButton_Click(object sender, EventArgs e)
		{
			BrowsingFilter.Filters = BrowsingFilters.All;
		}

		private void FilterRecentSearchButton_Click(object sender, EventArgs e)
		{
			BrowsingFilter.Filters = BrowsingFilter.Filters.ToggleFlag(BrowsingFilters.RecentSearch);
		}

		private void FilterQuerySearchButton_Click(object sender, EventArgs e)
		{
			BrowsingFilter.Filters = BrowsingFilter.Filters.ToggleFlag(BrowsingFilters.QuerySearch);
		}

		private void FilterTaggedSearchButton_Click(object sender, EventArgs e)
		{
			BrowsingFilter.Filters = BrowsingFilter.Filters.ToggleFlag(BrowsingFilters.TaggedSearch);
		}


		private void FilterTimer_Tick(object sender, EventArgs e)
		{
			filterTimer.Stop();

			BrowsingFilter.Text = filterTextBox.Text;
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
