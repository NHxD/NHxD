using Ash.System.Windows.Forms;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	public partial class TagsToolStrip : UserControl
	{
		private readonly ToolStrip toolStrip;
		private readonly ToolStripSpringTextBox filterTextBox;
		private readonly ToolStripDropDownButton filterButton;
		private readonly ToolStripMenuItem filterNoneButton;
		private readonly ToolStripMenuItem filterAllButton;
		private readonly ToolStripMenuItem filterWhitelistButton;
		private readonly ToolStripMenuItem filterBlacklistButton;
		private readonly ToolStripMenuItem filterIgnorelistButton;
		private readonly ToolStripMenuItem filterHidelistButton;
		private readonly ToolStripMenuItem filterOtherButton;
		private readonly ToolStripDropDownButton sortButton;
		private readonly ToolStripMenuItem sortTypeNoneButton;
		private readonly ToolStripMenuItem sortTypeNameButton;
		private readonly ToolStripMenuItem sortTypeCountButton;
		private readonly ToolStripMenuItem sortOrderAscendingButton;
		private readonly ToolStripMenuItem sortOrderDescendingButton;
		private readonly Timer filterTimer;

		public ToolStrip ToolStrip => toolStrip;
		public ToolStripSpringTextBox FilterTextBox => filterTextBox;
		public ToolStripDropDownButton FilterButton => filterButton;
		public ToolStripDropDownButton SortButton => sortButton;

		public TagsFilter TagsFilter { get; }
		public TagsModel TagsModel { get; }
		public Configuration.ConfigTagsList TagsListSettings { get; }
		public MetadataKeywordLists MetadataKeywordLists { get; }

		public TagsToolStrip()
		{
			InitializeComponent();
		}

		public TagsToolStrip(TagsFilter tagsFilter, TagsModel tagsModel, Configuration.ConfigTagsList tagsListSettings, MetadataKeywordLists metadataKeywordLists, int filterDelay)
		{
			InitializeComponent();

			TagsFilter = tagsFilter;
			TagsModel = tagsModel;
			TagsListSettings = tagsListSettings;
			MetadataKeywordLists = metadataKeywordLists;

			filterTextBox = new ToolStripSpringTextBox();
			toolStrip = new ToolStrip();
			filterButton = new ToolStripDropDownButton();
			filterNoneButton = new ToolStripMenuItem();
			filterAllButton = new ToolStripMenuItem();
			filterWhitelistButton = new ToolStripMenuItem();
			filterBlacklistButton = new ToolStripMenuItem();
			filterIgnorelistButton = new ToolStripMenuItem();
			filterHidelistButton = new ToolStripMenuItem();
			filterOtherButton = new ToolStripMenuItem();
			sortButton = new ToolStripDropDownButton();
			sortTypeNoneButton = new ToolStripMenuItem();
			sortTypeNameButton = new ToolStripMenuItem();
			sortTypeCountButton = new ToolStripMenuItem();
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

			filterWhitelistButton.Name = "whitelist";
			filterWhitelistButton.Text = "&Whitelisted";
			filterWhitelistButton.Click += FilterWhitelistButton_Click;

			filterBlacklistButton.Name = "blacklist";
			filterBlacklistButton.Text = "&Blacklisted";
			filterBlacklistButton.Click += FilterBlacklistButton_Click;

			filterIgnorelistButton.Name = "ignorelist";
			filterIgnorelistButton.Text = "&Ignored";
			filterIgnorelistButton.Click += FilterIgnorelistButton_Click;

			filterHidelistButton.Name = "hidelist";
			filterHidelistButton.Text = "&Hidden";
			filterHidelistButton.Click += FilterHidelistButton_Click;

			filterOtherButton.Name = "other";
			filterOtherButton.Text = "&Unfiltered";
			filterOtherButton.Click += FilterOtherButton_Click;

			ContextMenuStrip filterContextMenuStrip = new ContextMenuStrip();

			filterContextMenuStrip.ShowCheckMargin = true;
			filterContextMenuStrip.Items.Add(filterNoneButton);
			filterContextMenuStrip.Items.Add(filterAllButton);
			filterContextMenuStrip.Items.Add(new ToolStripSeparator());
			filterContextMenuStrip.Items.Add(filterWhitelistButton);
			filterContextMenuStrip.Items.Add(filterBlacklistButton);
			filterContextMenuStrip.Items.Add(filterIgnorelistButton);
			filterContextMenuStrip.Items.Add(filterHidelistButton);
			filterContextMenuStrip.Items.Add(filterOtherButton);

			filterContextMenuStrip.Opening += FilterContextMenuStrip_Opening;
			filterContextMenuStrip.RenderMode = ToolStripRenderMode.Professional;
			filterContextMenuStrip.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;

			filterButton.DropDown = filterContextMenuStrip;

			//
			// sort button
			//
			sortButton.Text = "&Sort";
			sortButton.AutoToolTip = false;

			sortTypeNoneButton.Name = "sort_type_none";
			sortTypeNoneButton.Text = "&Date Added";
			sortTypeNoneButton.Click += SortTypeNoneButton_Click;

			sortTypeNameButton.Name = "sort_type_name";
			sortTypeNameButton.Text = "&Name";
			sortTypeNameButton.Click += SortTypeNameButton_Click;

			sortTypeCountButton.Name = "sort_type_count";
			sortTypeCountButton.Text = "&Count";
			sortTypeCountButton.Click += SortTypeCountButton_Click;

			sortOrderAscendingButton.Name = "sort_order_ascending";
			sortOrderAscendingButton.Text = "&Ascending";
			sortOrderAscendingButton.Click += SortOrderAscendingButton_Click;

			sortOrderDescendingButton.Name = "sort_order_descending";
			sortOrderDescendingButton.Text = "&Descending";
			sortOrderDescendingButton.Click += SortOrderDescendingButton_Click;

			ContextMenuStrip sortContextMenuStrip = new ContextMenuStrip();

			sortContextMenuStrip.ShowCheckMargin = true;
			sortContextMenuStrip.Items.Add(sortTypeNameButton);
			sortContextMenuStrip.Items.Add(sortTypeCountButton);
			sortContextMenuStrip.Items.Add(sortTypeNoneButton);
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
			toolStrip.Items.Add(filterButton);
			toolStrip.Items.Add(sortButton);

			//
			// this
			//
			Controls.Add(toolStrip);
			
			tagsFilter.FiltersChanged += TagsFilter_FiltersChanged;
			tagsFilter.SortTypeChanged += TagsFilter_SortTypeChanged;
			tagsFilter.SortOrderChanged += TagsFilter_SortOrderChanged;

			ResumeLayout(false);
		}

		private void TagsFilter_FiltersChanged(object sender, EventArgs e)
		{
			TagsListSettings.Filter.Filters = TagsFilter.Filters;
			Filter();
		}

		private void TagsFilter_SortTypeChanged(object sender, EventArgs e)
		{
			TagsListSettings.SortType = TagsFilter.SortType;
			Filter();
		}

		private void TagsFilter_SortOrderChanged(object sender, EventArgs e)
		{
			TagsListSettings.SortOrder = TagsFilter.SortOrder;
			Filter();
		}

		private void SortOrderDescendingButton_Click(object sender, EventArgs e)
		{
			TagsFilter.SortOrder = SortOrder.Descending;
		}

		private void SortOrderAscendingButton_Click(object sender, EventArgs e)
		{
			TagsFilter.SortOrder = SortOrder.Ascending;
		}

		private void SortTypeCountButton_Click(object sender, EventArgs e)
		{
			TagsFilter.SortType = TagSortType.Count;
		}

		private void SortTypeNameButton_Click(object sender, EventArgs e)
		{
			TagsFilter.SortType = TagSortType.Name;
		}

		private void SortTypeNoneButton_Click(object sender, EventArgs e)
		{
			TagsFilter.SortType = TagSortType.DateAdded;
		}

		private void SortContextMenuStrip_Opening(object sender, CancelEventArgs e)
		{
			sortTypeNoneButton.Checked = TagsFilter.SortType == TagSortType.DateAdded;
			sortTypeNameButton.Checked = TagsFilter.SortType == TagSortType.Name;
			sortTypeCountButton.Checked = TagsFilter.SortType == TagSortType.Count;

			sortOrderAscendingButton.Checked = TagsFilter.SortOrder == SortOrder.Ascending;
			sortOrderDescendingButton.Checked = TagsFilter.SortOrder == SortOrder.Descending;

			sortTypeNoneButton.Enabled = !sortTypeNoneButton.Checked;
			sortTypeNameButton.Enabled = !sortTypeNameButton.Checked;
			sortTypeCountButton.Enabled = !sortTypeCountButton.Checked;

			sortOrderAscendingButton.Enabled = !sortOrderAscendingButton.Checked;
			sortOrderDescendingButton.Enabled = !sortOrderDescendingButton.Checked;
		}

		private void FilterContextMenuStrip_Opening(object sender, CancelEventArgs e)
		{
			TagsFilters filters = TagsFilter.Filters;

			filterNoneButton.Checked = filters == TagsFilters.None;
			filterAllButton.Checked = filters == TagsFilters.All;
			filterWhitelistButton.Checked = filters.HasFlag(TagsFilters.Whitelist);
			filterBlacklistButton.Checked = filters.HasFlag(TagsFilters.Blacklist);
			filterIgnorelistButton.Checked = filters.HasFlag(TagsFilters.Ignorelist);
			filterHidelistButton.Checked = filters.HasFlag(TagsFilters.Hidelist);
			filterOtherButton.Checked = filters.HasFlag(TagsFilters.Other);

			filterNoneButton.Enabled = filters != TagsFilters.None;
			filterAllButton.Enabled = filters != TagsFilters.All;
		}

		private void FilterNoneButton_Click(object sender, EventArgs e)
		{
			TagsFilter.Filters = TagsFilters.None;
		}

		private void FilterAllButton_Click(object sender, EventArgs e)
		{
			TagsFilter.Filters = TagsFilters.All;
		}

		private void FilterWhitelistButton_Click(object sender, EventArgs e)
		{
			TagsFilter.Filters = TagsFilter.Filters.ToggleFlag(TagsFilters.Whitelist);
		}

		private void FilterBlacklistButton_Click(object sender, EventArgs e)
		{
			TagsFilter.Filters = TagsFilter.Filters.ToggleFlag(TagsFilters.Blacklist);
		}

		private void FilterIgnorelistButton_Click(object sender, EventArgs e)
		{
			TagsFilter.Filters = TagsFilter.Filters.ToggleFlag(TagsFilters.Ignorelist);
		}

		private void FilterHidelistButton_Click(object sender, EventArgs e)
		{
			TagsFilter.Filters = TagsFilter.Filters.ToggleFlag(TagsFilters.Hidelist);
		}

		private void FilterOtherButton_Click(object sender, EventArgs e)
		{
			TagsFilter.Filters = TagsFilter.Filters.ToggleFlag(TagsFilters.Other);
		}

		private void FilterTimer_Tick(object sender, EventArgs e)
		{
			filterTimer.Stop();

			TagsFilter.Text = filterTextBox.Text;
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
