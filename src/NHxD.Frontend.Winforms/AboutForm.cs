using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	public partial class AboutForm : Form
	{
		private readonly BackgroundWorker backgoundWorker;

		public AboutTextFormatter AboutTextFormatter { get; }
		public DocumentTemplate<object> AboutDocumentTemplate { get; }

		public AboutForm()
		{
			InitializeComponent();
		}

		public AboutForm(AboutTextFormatter aboutTextFormatter, DocumentTemplate<object> aboutDocumentTemplate, Size size)
		{
			InitializeComponent();

			AboutTextFormatter = aboutTextFormatter;
			AboutDocumentTemplate = aboutDocumentTemplate;

			if (!size.IsEmpty)
			{
				Size = size;
				MinimumSize = size;
			}

			backgoundWorker = new BackgroundWorker();

			backgoundWorker.WorkerSupportsCancellation = true;
			backgoundWorker.DoWork += BackgoundWorker_DoWork;
			backgoundWorker.RunWorkerCompleted += BackgoundWorker_RunWorkerCompleted;
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (!backgoundWorker.CancellationPending)
			{
				backgoundWorker.CancelAsync();
			}

			base.OnClosing(e);
		}

		private static void BackgoundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			BackgroundWorker backgroundWorker = sender as BackgroundWorker;
			IPathFormatter pathFormatter = e.Argument as IPathFormatter;
			string cacheDirectory = pathFormatter.GetCacheDirectory();

			if (Directory.Exists(cacheDirectory))
			{
				DirectoryInfo dirInfo = new DirectoryInfo(cacheDirectory);
				FileInfo[] coverFiles = dirInfo.GetFiles("*.jpg", SearchOption.TopDirectoryOnly);
				Random rand = new Random();
				int randomIndex = rand.Next(coverFiles.Length);

				if (backgroundWorker.CancellationPending)
				{
					e.Cancel = true;
				}
				else
				{
					e.Result = coverFiles[randomIndex].FullName.Replace('\\', '/');
				}
			}
			else
			{
				e.Cancel = true;
			}
		}

		private void BackgoundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error != null)
			{
				MessageBox.Show(e.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			if (!e.Cancelled)
			{
				if (webBrowser == null
					|| string.IsNullOrEmpty(webBrowser.DocumentText))
				{
					return;
				}

				webBrowser.Document?.InvokeScript("__onCoverReady", new object[]
					{
						e.Result as string
					});
			}
		}

		private void AboutForm_Load(object sender, EventArgs e)
		{
			webBrowser.DocumentCompleted += WebBrowser_DocumentCompleted;
			webBrowser.DocumentText = AboutDocumentTemplate.GetFormattedText();
		}

		private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			backgoundWorker.RunWorkerAsync(AboutTextFormatter.PathFormatter);
		}

		private void OkButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
