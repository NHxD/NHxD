using Ash.System.Windows.Forms;
using NHxD.Plugin;
using NHxD.Plugin.ArchiveWriter;
using NHxD.Plugin.MetadataConverter;
using NHxD.Plugin.MetadataProcessor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	public partial class PluginsForm : Form
	{
		private static readonly Type ArchiveWriterType = typeof(IArchiveWriter);
		private static readonly Type MetadataConverterType = typeof(IMetadataConverter);
		private static readonly Type MetadataProcessorType = typeof(IMetadataProcessor);

		private readonly ListViewEx listView;
		private readonly TableLayoutPanel tableLayoutPanel;
		private readonly Button okButton;
		private readonly Button loadFromFileButton;

		public List<IArchiveWriter> ArchiveWriters { get; set; }
		public List<IMetadataConverter> MetadataConverters { get; set; }
		public List<IMetadataProcessor> MetadataProcessors { get; set; }
		public string PluginDirectory { get; set; }

		public PluginsForm()
		{
			InitializeComponent();

			listView = new ListViewEx();
			tableLayoutPanel = new TableLayoutPanel();
			okButton = new Button();
			loadFromFileButton = new Button();

			SuspendLayout();

			listView.AllowColumnReorder = true;
			// TODO: enabling/disabling plugins at runtime currently not implemented.
			//listView.CheckBoxes = true;
			listView.Dock = DockStyle.Fill;
			listView.FullRowSelect = true;
			listView.GridLines = true;
			listView.ShowGroups = true;
			listView.Sorting = SortOrder.Ascending;
			listView.View = View.Details;

			listView.Columns.Add("Name");
			listView.Columns.Add("Description");
			listView.Columns.Add("Author");
			listView.Columns.Add("Version");

			okButton.Margin = new Padding(0, 0, 16, 0);
			okButton.Anchor = AnchorStyles.Right;
			okButton.Text = "&OK";
			okButton.AutoSize = true;
			okButton.Click += OkButton_Click;

			loadFromFileButton.Margin = new Padding(16, 0, 0, 0);
			loadFromFileButton.Anchor = AnchorStyles.Left;
			loadFromFileButton.Text = "&Load from file...";
			loadFromFileButton.AutoSize = true;
			loadFromFileButton.Click += LoadFromFileButton_Click;

			tableLayoutPanel.Dock = DockStyle.Fill;
			tableLayoutPanel.ColumnCount = 2;
			tableLayoutPanel.RowCount = 2;
			tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50.0f));
			tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50.0f));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100.0f));
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 48.0f));
			tableLayoutPanel.Controls.Add(listView);
			tableLayoutPanel.Controls.Add(loadFromFileButton);
			tableLayoutPanel.Controls.Add(okButton);
			tableLayoutPanel.SetCellPosition(listView, new TableLayoutPanelCellPosition(0, 0));
			tableLayoutPanel.SetColumnSpan(listView, 2);
			tableLayoutPanel.SetCellPosition(loadFromFileButton, new TableLayoutPanelCellPosition(0, 1));
			tableLayoutPanel.SetCellPosition(okButton, new TableLayoutPanelCellPosition(1, 1));

			AcceptButton = okButton;
			CancelButton = okButton;
			MinimumSize = new Size(300, 220);
			Controls.Add(tableLayoutPanel);

			// HACK: not fully implemented yet.
			loadFromFileButton.Visible = false;

			ResumeLayout(false);
		}

		private void LoadFromFileButton_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog ofn = new OpenFileDialog())
			{
				ofn.InitialDirectory = PluginDirectory;
				ofn.CheckFileExists = true;
				ofn.CheckPathExists = true;
				ofn.DefaultExt = ".dll";
				ofn.Filter = "Plugin Files (*.dll)|*.dll|All Files (*.*)|*.*";
				ofn.Multiselect = true;

				DialogResult dialogResult = ofn.ShowDialog(this);

				if (dialogResult == DialogResult.OK)
				{
					foreach (string fileName in ofn.FileNames)
					{
						LoadPluginFromFile(fileName);
					}
				}
			}
		}

		private void LoadPluginFromFile(string fileName)
		{
			Assembly assembly;

			try
			{
				assembly = Assembly.LoadFile(fileName);
			}
			catch (Exception ex)
			{
				MessageBox.Show(string.Format(CultureInfo.CurrentUICulture, "Couldn't load assembly from file {0}{1}{2}", fileName, Environment.NewLine, ex.ToString()), "Plugin Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			List<IArchiveWriter> archiveWriters = LoadPlugin<IArchiveWriter>(assembly);
			if (archiveWriters.Count > 0)
			{
				ArchiveWriters.AddRange(archiveWriters);
			}

			List<IMetadataConverter> metadataConverters = LoadPlugin<IMetadataConverter>(assembly);
			if (metadataConverters.Count > 0)
			{
				MetadataConverters.AddRange(metadataConverters);
			}

			List<IMetadataProcessor> metadataProcessors = LoadPlugin<IMetadataProcessor>(assembly);
			if (metadataProcessors.Count > 0)
			{
				MetadataProcessors.AddRange(metadataProcessors);
			}
		}

		private static List<TPlugin> LoadPlugin<TPlugin>(Assembly assembly) where TPlugin : IPlugin
		{
			List<TPlugin> result = new List<TPlugin>();

			try
			{
				Type pluginType = typeof(TPlugin);
				IEnumerable<Type> pluginImplementationTypes = assembly.ExportedTypes
					.Where(x => pluginType.IsAssignableFrom(x) && x.IsClass && !x.IsAbstract);

				foreach (Type type in pluginImplementationTypes)
				{
					TPlugin plugin = (TPlugin)Activator.CreateInstance(type);
					Dictionary<string, string> emptySettings = new Dictionary<string, string>();

					plugin.Initialize(emptySettings);

					result.Add(plugin);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), "Plugin Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			return result;
		}

		private void OkButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		public void Populate()
		{
			listView.Items.Clear();
			listView.Groups.Clear();

			if (ArchiveWriters != null && ArchiveWriters.Count > 0)
			{
				ListViewGroup group = new ListViewGroup("Archive Writers");

				listView.Groups.Add(group);

				foreach (IPlugin plugin in ArchiveWriters)
				{
					AddItem(plugin, ArchiveWriterType, group);
				}
			}

			if (MetadataConverters != null && MetadataConverters.Count > 0)
			{
				ListViewGroup group = new ListViewGroup("Metadata Converters");

				listView.Groups.Add(group);

				foreach (IPlugin plugin in MetadataConverters)
				{
					AddItem(plugin, MetadataConverterType, group);
				}
			}

			if (MetadataProcessors != null && MetadataProcessors.Count > 0)
			{
				ListViewGroup group = new ListViewGroup("Metadata Processors");

				listView.Groups.Add(group);

				foreach (IPlugin plugin in MetadataProcessors)
				{
					AddItem(plugin, MetadataProcessorType, group);
				}
			}

			listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
			listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
		}

		private void AddItem(IPlugin plugin, Type type, ListViewGroup group)
		{
			listView.Items.Add(new ListViewItem(new string[]
				{
						plugin.Info.Name,
						plugin.Info.Description,
						plugin.Info.Author,
						plugin.Info.Version
				}, group)
				// TODO: enabling/disabling plugins at runtime currently not implemented.
				//{ Checked = plugin.IsEnabled }
				);
		}

		private void PluginsForm_Load(object sender, EventArgs e)
		{
			Populate();
		}
	}
}
