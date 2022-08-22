using Nhentai;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	public partial class ManageMetadataCacheForm : Form
	{
		private readonly List<FileInfo> fileInfos = new List<FileInfo>();

		public IPathFormatter PathFormatter { get; }
		public IMetadataCache MetadataCache { get; }
		public ISearchResultCache SearchResultCache { get; }
		public MetadataCacheSnapshot MetadataCacheSnapshot { get; }
		public Configuration.ConfigMetadataCache MetadataCacheSettings { get; }

		public ManageMetadataCacheForm()
		{
			InitializeComponent();

			label3.Tag = 0L;

			label2.Text = "";
			label3.Text = "";
			label5.Text = "";
			toolStripStatusLabel1.Text = "";
			toolStripProgressBar1.Visible = false;
		}

		public ManageMetadataCacheForm(IPathFormatter pathFormatter, IMetadataCache metadataCache, ISearchResultCache searchResultCache, MetadataCacheSnapshot metadataCacheSnapshot, Configuration.ConfigMetadataCache metadataCacheSettings)
			: this()
		{
			PathFormatter = pathFormatter;
			MetadataCache = metadataCache;
			SearchResultCache = searchResultCache;
			MetadataCacheSnapshot = metadataCacheSnapshot;
			MetadataCacheSettings = metadataCacheSettings;

			UpdateCachedItemsView();
			UpdateCacheSnapshotView();
		}

		private void UpdateCacheSnapshotView()
		{
			if (MetadataCacheSnapshot.DoesExist)
			{
				label5.Tag = new FileInfo(MetadataCacheSnapshot.CachedFullDatabaseFilePath).Length;
				label5.Text = ((long)label5.Tag) / 1024.0f / 1024.0f + " MiB"; // TODO: prettify (auto display in mb, gb, etc.)
			}
			else
			{
				label5.Text = "n/a";
			}
		}

		private void UpdateCachedItemsView()
		{
			label2.Tag = (long)MetadataCache.Items.Count;
			label2.Text = (long)label2.Tag + " items";
		}

		private void ManageMetadataCacheForm_Load(object sender, EventArgs e)
		{
		}

		private void fileToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			bool isAnyBackgroundWorkerBusy = backgroundWorker1.IsBusy || backgroundWorker2.IsBusy || timer1.Enabled;

			buildToolStripMenuItem.Enabled = !isAnyBackgroundWorkerBusy;
			loadToolStripMenuItem.Enabled = !isAnyBackgroundWorkerBusy && MetadataCacheSnapshot.DoesExist;
			unloadToolStripMenuItem.Enabled = !isAnyBackgroundWorkerBusy && MetadataCache.Items.Count > 0;
			saveToolStripMenuItem.Enabled = !isAnyBackgroundWorkerBusy && MetadataCacheSnapshot.IsReady;
			deleteToolStripMenuItem.Enabled = !isAnyBackgroundWorkerBusy && MetadataCacheSnapshot.DoesExist;
		}

		private void onProgramStartToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			buildAtStartupToolStripMenuItem.Checked = MetadataCacheSettings.BuildAtStartup;
			loadAtStartupToolStripMenuItem1.Checked = MetadataCacheSettings.LoadAtStartup;
		}

		private void onProgramExitToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			deleteAtExitCacheToolStripMenuItem.Checked = MetadataCacheSettings.DeleteAtExit;
		}

		private void compressionLevelToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
		{
			bool isAnyBackgroundWorkerBusy = backgroundWorker1.IsBusy || backgroundWorker2.IsBusy;

			noneToolStripMenuItem.Enabled = !isAnyBackgroundWorkerBusy;
			fastestToolStripMenuItem.Enabled = !isAnyBackgroundWorkerBusy;
			optimalToolStripMenuItem.Enabled = !isAnyBackgroundWorkerBusy;

			noneToolStripMenuItem.Checked = MetadataCacheSettings.CompressionLevel == CompressionLevel.NoCompression;
			fastestToolStripMenuItem.Checked = MetadataCacheSettings.CompressionLevel == CompressionLevel.Fastest;
			optimalToolStripMenuItem.Checked = MetadataCacheSettings.CompressionLevel == CompressionLevel.Optimal;
		}

		private void buildToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (backgroundWorker1.IsBusy)
			{
				return;
			}

			cancelOperationToolStripMenuItem.Visible = true;

			backgroundWorker2.RunWorkerAsync(new DoWorkArg2(MetadataCache, SearchResultCache, PathFormatter, MetadataCacheSnapshot, fileInfos));
		}

		private void loadToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MetadataCacheSnapshot.LoadFromFile();

			UpdateCachedItemsView();
			UpdateCacheSnapshotView();
		}

		private void unloadToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MetadataCache.Items.Clear();

			UpdateCachedItemsView();
		}

		private void saveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MetadataCacheSnapshot.SaveToFile();
		}

		private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MetadataCacheSnapshot.Delete();

			UpdateCachedItemsView();
			UpdateCacheSnapshotView();
		}

		private void buildAtStartupToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MetadataCacheSettings.BuildAtStartup = !MetadataCacheSettings.BuildAtStartup;
		}

		private void loadAtStartupToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			MetadataCacheSettings.LoadAtStartup = !MetadataCacheSettings.LoadAtStartup;
		}

		private void deleteAtExitCacheToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MetadataCacheSettings.DeleteAtExit = !MetadataCacheSettings.DeleteAtExit;
		}

		private void noneToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MetadataCacheSettings.CompressionLevel = CompressionLevel.NoCompression;
		}

		private void fastestToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MetadataCacheSettings.CompressionLevel = CompressionLevel.Fastest;
		}

		private void optimalToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MetadataCacheSettings.CompressionLevel = CompressionLevel.Optimal;
		}

		private class DoWorkArg1
		{
			public IPathFormatter PathFormatter { get; }
			public List<FileInfo> FileInfos { get; }

			public DoWorkArg1(IPathFormatter pathFormatter, List<FileInfo> fileInfos)
			{
				FileInfos = fileInfos;
				PathFormatter = pathFormatter;
			}
		}

		private class ReportProgressArg1
		{
			public int TotalCount { get; }
			public int CurrentIndex { get; }
			public FileInfo FileInfo { get; }
			public bool ShouldUpdate { get; }

			public ReportProgressArg1(int totalCount, int currentIndex, FileInfo fileInfo, bool shouldUpdate)
			{
				TotalCount = totalCount;
				CurrentIndex = currentIndex;
				FileInfo = fileInfo;
				ShouldUpdate = shouldUpdate;
			}
		}

		private class DoWorkArg2
		{
			public IMetadataCache MetadataCache { get; }
			public ISearchResultCache SearchResultCache { get; }
			public MetadataCacheSnapshot MetadataCacheSnapshot { get; }
			public IEnumerable<FileInfo> FileInfos { get; }

			public DoWorkArg2(IMetadataCache metadataCache, ISearchResultCache searchResultCache, IPathFormatter pathFormatter, MetadataCacheSnapshot metadataCacheSnapshot, IEnumerable<FileInfo> fileInfos)
			{
				MetadataCache = metadataCache;
				SearchResultCache = searchResultCache;
				MetadataCacheSnapshot = metadataCacheSnapshot;
				FileInfos = fileInfos;
			}
		}

		private class ReportProgressArg2
		{
			public int TotalCount { get; }
			public int CurrentIndex { get; }
			public IMetadataCache MetadataCache { get; }
			public Metadata Metadata { get; }
			public bool ShouldUpdate { get; }

			public ReportProgressArg2(int totalCount, int currentIndex, IMetadataCache metadataCache, Metadata metadata, bool shouldUpdate)
			{
				TotalCount = totalCount;
				CurrentIndex = currentIndex;
				MetadataCache = metadataCache;
				Metadata = metadata;
				ShouldUpdate = shouldUpdate;
			}
		}

		private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
		{
			BackgroundWorker backgroundWorker = (BackgroundWorker)sender;
			DoWorkArg1 arg = (DoWorkArg1)e.Argument;
			DirectoryInfo dirInfo = new DirectoryInfo(arg.PathFormatter.GetMetadataDirectory());
			IEnumerable<FileInfo> fileInfos = dirInfo.EnumerateFiles("*.json", arg.PathFormatter.IsEnabled ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).Where(x => int.TryParse(Path.GetFileNameWithoutExtension(x.Name), out _));
			int totalCount = fileInfos.Count();
			int i = 0;
			DateTime lastTime = DateTime.Now;
			DateTime nextUpdateMinTime = lastTime + TimeSpan.FromSeconds(.5f);

			arg.FileInfos.AddRange(fileInfos);

			foreach (FileInfo fileInfo in fileInfos)
			{
				if (backgroundWorker.CancellationPending)
				{
					e.Cancel = true;
					break;
				}

				DateTime now = DateTime.Now;
				// FIXME
				bool shouldUpdate = true;// now >= nextUpdateMinTime;

				lastTime = now;

				if (shouldUpdate)
				{
					nextUpdateMinTime = lastTime + TimeSpan.FromSeconds(.5f);
				}

				backgroundWorker.ReportProgress(i * 100 / totalCount, new ReportProgressArg1(totalCount, i, fileInfo, shouldUpdate));

				++i;
			}
		}

		private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			ReportProgressArg1 arg = (ReportProgressArg1)e.UserState;

			label3.Tag = (long)label3.Tag + arg.FileInfo.Length;

			if (arg.ShouldUpdate)
			{
				label3.Text = arg.TotalCount + " files (" + ((long)label3.Tag) / 1024.0f / 1024.0f + " MiB)";   // TODO: prettify (auto display in mb, gb, etc.)

				toolStripStatusLabel1.Text = (arg.CurrentIndex + 1) + " / " + arg.TotalCount;
				toolStripProgressBar1.Visible = true;
				toolStripProgressBar1.Value = e.ProgressPercentage;
			}
		}

		private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			cancelOperationToolStripMenuItem.Visible = false;
			toolStripStatusLabel1.Text = "";
			toolStripProgressBar1.Visible = false;
			toolStripProgressBar1.Value = 0;

			if (e.Error != null)
			{
				MessageBox.Show(this, e.Error.Message, "NHxD", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			else if (e.Cancelled)
			{
				label3.Text = "n/a";
				toolStripStatusLabel1.Text = "Cancelled";
			}
			else
			{
			}
		}

		private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
		{
			BackgroundWorker backgroundWorker = (BackgroundWorker)sender;
			DoWorkArg2 arg = (DoWorkArg2)e.Argument;
			//DirectoryInfo dirInfo = new DirectoryInfo(arg.PathFormatter.GetMetadataDirectory());
			//IEnumerable<FileInfo> fileInfos = dirInfo.EnumerateFiles("*.json", arg.PathFormatter.IsEnabled ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).Where(x => x != null);
			int totalCount = arg.FileInfos.Count();
			int i = 0;
			DateTime lastTime = DateTime.Now;
			DateTime nextUpdateMinTime = lastTime + TimeSpan.FromSeconds(.5f);

			foreach (FileInfo fileInfo in arg.FileInfos)
			{
				if (backgroundWorker.CancellationPending)
				{
					e.Cancel = true;
					break;
				}

				int galleryId;

				if (int.TryParse(Path.GetFileNameWithoutExtension(fileInfo.Name), out galleryId))
				{
					string cachedMetadataFilePath = fileInfo.FullName;
					Metadata metadata = arg.SearchResultCache.Find(galleryId) ?? JsonUtility.LoadFromFile<Metadata>(cachedMetadataFilePath);

					if (metadata is null)
					{
						continue;
					}

					DateTime now = DateTime.Now;
					// FIXME
					bool shouldUpdate = true;// now >= nextUpdateMinTime;

					lastTime = now;

					if (shouldUpdate)
					{
						nextUpdateMinTime = lastTime + TimeSpan.FromSeconds(.5f);
					}

					backgroundWorker.ReportProgress(i * 100 / totalCount, new ReportProgressArg2(totalCount, i, arg.MetadataCache, metadata, shouldUpdate));
				}

				++i;
			}

			arg.MetadataCacheSnapshot.SaveToFile();
		}

		private void backgroundWorker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			ReportProgressArg2 arg = (ReportProgressArg2)e.UserState;

			if (arg.MetadataCache.Items.ContainsKey(arg.Metadata.Id))
			{
				// NOTE: there's no timestamps for updated tags so just assume it's more recent.
				arg.MetadataCache.Items[arg.Metadata.Id] = arg.Metadata;
			}
			else
			{
				arg.MetadataCache.Items.Add(arg.Metadata.Id, arg.Metadata);
			}

			if (arg.ShouldUpdate)
			{
				toolStripStatusLabel1.Text = (arg.CurrentIndex + 1) + " / " + arg.TotalCount;
				toolStripProgressBar1.Visible = true;
				toolStripProgressBar1.Value = e.ProgressPercentage;
			}
		}

		private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			cancelOperationToolStripMenuItem.Visible = false;
			toolStripStatusLabel1.Text = "";
			toolStripProgressBar1.Visible = false;
			toolStripProgressBar1.Value = 0;

			if (e.Error != null)
			{
				MessageBox.Show(this, e.Error.Message, "NHxD", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			else if (e.Cancelled)
			{
				MetadataCacheSnapshot.IsReady = false;

				UpdateCachedItemsView();

				label5.Text = "n/a";
				toolStripStatusLabel1.Text = "Cancelled";
			}
			else
			{
				MetadataCacheSnapshot.IsReady = true;

				UpdateCachedItemsView();
				UpdateCacheSnapshotView();
			}
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			timer1.Enabled = false;
			cancelOperationToolStripMenuItem.Visible = true;

			backgroundWorker1.RunWorkerAsync(new DoWorkArg1(PathFormatter, fileInfos));
		}

		private void cancelOperationToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (backgroundWorker1.IsBusy
				&& !backgroundWorker1.CancellationPending)
			{
				backgroundWorker1.CancelAsync();
			}
			else if (backgroundWorker2.IsBusy
				&& !backgroundWorker2.CancellationPending)
			{
				backgroundWorker2.CancelAsync();
			}
		}
	}
}
