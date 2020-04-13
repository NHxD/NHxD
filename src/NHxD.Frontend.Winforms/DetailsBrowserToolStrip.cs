using Ash.System.Windows.Forms;
using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	public partial class DetailsBrowserToolStrip : UserControl
	{
		private readonly ToolStrip toolStrip;
		private readonly ToolStripSpringComboBox searchComboBox;

		public ToolStrip ToolStrip => toolStrip;
		public ToolStripSpringComboBox SearchComboBox => searchComboBox;

		public DetailsBrowserFilter DetailsBrowserFilter { get; }
		public DetailsModel DetailsModel { get; }
		public SearchHandler SearchHandler { get; }

		public DetailsBrowserToolStrip()
		{
			InitializeComponent();
		}

		public DetailsBrowserToolStrip(DetailsBrowserFilter detailsBrowserFilter, DetailsModel detailsModel, SearchHandler searchHandler)
		{
			InitializeComponent();

			DetailsBrowserFilter = detailsBrowserFilter;
			DetailsModel = detailsModel;
			SearchHandler = searchHandler;

			searchComboBox = new ToolStripSpringComboBox();
			toolStrip = new ToolStrip();

			SuspendLayout();

			//
			// searchComboBox
			//
			searchComboBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
			searchComboBox.AutoCompleteSource = AutoCompleteSource.ListItems;
			searchComboBox.DropDownStyle = ComboBoxStyle.DropDown;
			// WORKAROUND: bug since win7 - selectedindex is wrong when a datasource is bound
//			searchComboBox.ComboBox.DataSource = DetailsModel.Searches;
//			searchComboBox.DataSource = DetailsModel.Searches;
			searchComboBox.Dock = DockStyle.Fill;
			searchComboBox.Margin = new Padding(0, 0, 6, 0);
			searchComboBox.ComboBox.SelectionChangeCommitted += SearchComboBox_SelectionChangeCommitted;
			searchComboBox.KeyDown += SearchComboBox_KeyDown;
			searchComboBox.ComboBox.OverrideMouseWheelBehaviour();
			searchComboBox.OverrideUpDownKeys();

			//
			// toolStrip
			//
			toolStrip.Dock = DockStyle.Fill;
			toolStrip.CanOverflow = false;
			toolStrip.Items.Add(searchComboBox);

			//
			// this
			//
			Controls.Add(toolStrip);

			//DetailsModel.Searches.ListChanged += Searches_ListChanged;
			DetailsModel.SearchesChanged += DetailsModel_SearchesChanged;

			ResumeLayout(false);
		}

		public void Rebind()
		{
			//DetailsModel.Searches.ResetBindings();
			searchComboBox.ComboBox.Items.Clear();
			searchComboBox.ComboBox.Items.AddRange(DetailsModel.Searches.Cast<object>().ToArray());
		}

		private void DetailsModel_SearchesChanged(object sender, EventArgs e)
		{
			Rebind();
			searchComboBox.Text = ((int)searchComboBox.ComboBox.Items[0]).ToString(CultureInfo.InvariantCulture);
		}

		/*private void Searches_ListChanged(object sender, ListChangedEventArgs e)
		{
			BindingList<int> bindingList = sender as BindingList<int>;

			if (e.ListChangedType == ListChangedType.ItemAdded)
			{
				searchComboBox.Text = bindingList[0].ToString();
			}
		}*/

		private void SearchComboBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return)
			{
				SubmitSearch();

				e.Handled = true;
				e.SuppressKeyPress = true;
			}
		}

		private void SearchComboBox_SelectionChangeCommitted(object sender, EventArgs e)
		{
			if (searchComboBox.SelectedIndex == -1)
			{
				return;
			}

			SubmitSearch();
		}

		private void SubmitSearch()
		{
			string text = searchComboBox.SelectedIndex != -1 ? ((int)searchComboBox.Items[searchComboBox.SelectedIndex]).ToString(CultureInfo.InvariantCulture) : searchComboBox.Text;
			
			if (string.IsNullOrEmpty(text))
			{
				return;
			}

			int galleryId;

			if (!int.TryParse(text, out galleryId))
			{
				MessageBox.Show("Please enter a valid gallery ID. " + text);
				return;
			}

			SearchHandler.ShowDetails(galleryId);
		}
	}
}
