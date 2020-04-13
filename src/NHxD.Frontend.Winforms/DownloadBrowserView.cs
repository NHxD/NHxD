using Ash.System.Windows.Forms;
using Nhentai;
using System;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	//
	// NOTE: currently unused.
	//

	public partial class DownloadBrowserView : UserControl
	{
		private readonly WebBrowserEx webBrowser;

		public WebBrowserEx WebBrowser => webBrowser;

		public DetailsModel DetailsModel { get; }
		public DocumentTemplate<Metadata> DownloadTemplate { get; }
		public PageDownloader PageDownloader { get; }
		public Configuration.ConfigDetailsBrowserView DownloadBrowserSettings { get; }	// NOTE: download browser view is currently unused so there's not settings matching.

		public DownloadBrowserView()
		{
			InitializeComponent();
		}

		public DownloadBrowserView(DetailsModel detailsModel, DocumentTemplate<Metadata> downloadTemplate, PageDownloader pageDownloader, Configuration.ConfigDetailsBrowserView downloadBrowserSettings)
		{
			InitializeComponent();

			DetailsModel = detailsModel;
			DownloadTemplate = downloadTemplate;
			PageDownloader = pageDownloader;
			DownloadBrowserSettings = downloadBrowserSettings;

			webBrowser = new WebBrowserEx();

			SuspendLayout();

			webBrowser.AllowWebBrowserDrop = false;
			webBrowser.Dock = DockStyle.Fill;
			webBrowser.Name = "downloadWwebBrowser";
			webBrowser.BeforeDocumentCompleted += WebBrowser_BeforeDocumentCompleted;

			PageDownloader.PageDownloadReportProgress += PageDownloader_PageDownloadReportProgress;
			PageDownloader.PagesDownloadStarted += PageDownloader_PagesDownloadStarted;
			PageDownloader.PagesDownloadCancelled += PageDownloader_PagesDownloadCancelled;
			PageDownloader.PagesDownloadCompleted += PageDownloader_PagesDownloadCompleted;

			DetailsModel.MetadataChanged += DetailsModel_MetadataChanged;

			Controls.Add(webBrowser);

			ResumeLayout(false);
		}

		private void DetailsModel_MetadataChanged(object sender, EventArgs e)
		{
			WebBrowser.Tag = DetailsModel.Metadata?.Id;
			WebBrowser.DocumentText = DownloadTemplate.GetFormattedText(DetailsModel.Metadata);
		}

		private void PageDownloader_PagesDownloadStarted(object sender, PagesDownloadStartedEventArgs e)
		{
			if (string.IsNullOrEmpty(WebBrowser.DocumentText))
			{
				return;
			}

			WebBrowser.Document?.InvokeScript("__onPagesDownloadStarted", e.ToObjectArray());
		}

		private void PageDownloader_PagesDownloadCompleted(object sender, PageDownloadCompletedEventArgs e)
		{
			if (string.IsNullOrEmpty(WebBrowser.DocumentText))
			{
				return;
			}

			WebBrowser.Document?.InvokeScript("__onPagesDownloadCompleted", e.ToObjectArray());
		}

		private void PageDownloader_PagesDownloadCancelled(object sender, PageDownloadCompletedEventArgs e)
		{
			if (string.IsNullOrEmpty(WebBrowser.DocumentText))
			{
				return;
			}

			WebBrowser.Document?.InvokeScript("__onPagesDownloadCancelled", e.ToObjectArray());
		}

		private void PageDownloader_PageDownloadReportProgress(object sender, PageDownloadReportProgressEventArgs e)
		{
			if (string.IsNullOrEmpty(WebBrowser.DocumentText))
			{
				return;
			}

			WebBrowser.Document?.InvokeScript("__onPageDownloadReportProgress", e.ToObjectArray());
		}

		private void WebBrowser_BeforeDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			webBrowser.ZoomFactor = DownloadBrowserSettings.ZoomRatio;
		}
	}
}
