using Ash.System.Windows.Forms;
using Nhentai;
using System;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	public partial class DetailsBrowserView : UserControl
	{
		private readonly WebBrowserEx webBrowser;

		public WebBrowserEx WebBrowser => webBrowser;

		public DetailsBrowserFilter DetailsBrowserFilter { get; }
		public DetailsModel DetailsModel { get; }
		public DocumentTemplate<Metadata> DetailsTemplate { get; }
		public DocumentTemplate<Metadata> DownloadTemplate { get; }
		public DocumentTemplate<Metadata> DetailsPreloadTemplate { get; }
		//public DocumentTemplate<Metadata> DownloadPreloadTemplate { get; }
		public GalleryDownloader GalleryDownloader { get; }
		public PageDownloader PageDownloader { get; }
		public CoverDownloader CoverDownloader { get; }
		public MetadataKeywordLists MetadataKeywordLists { get; }
		public ISearchResultCache SearchResultCache { get; }
		public ICacheFileSystem CacheFileSystem { get; }
		public Configuration.ConfigDetailsBrowserView DetailsBrowserSettings { get; }
		// NOTE: at the moment, details & download views share the same browser.
		public Configuration.ConfigDetailsBrowserView DownloadBrowserSettings => DetailsBrowserSettings;

		public DetailsBrowserView()
		{
			InitializeComponent();
		}

		public DetailsBrowserView(DetailsBrowserFilter detailsBrowserFilter, DetailsModel detailsModel, DocumentTemplate<Metadata> detailsTemplate, DocumentTemplate<Metadata> downloadTemplate, DocumentTemplate<Metadata> detailsPreloadTemplate
			, GalleryDownloader galleryDownloader
			, PageDownloader pageDownloader
			, CoverDownloader coverDownloader
			, MetadataKeywordLists metadataKeywordLists
			, Configuration.ConfigDetailsBrowserView detailsBrowserSettings
			, ISearchResultCache searchResultCache
			, ICacheFileSystem cacheFileSystem)
		{
			InitializeComponent();

			DetailsBrowserFilter = detailsBrowserFilter;
			DetailsModel = detailsModel;
			DetailsTemplate = detailsTemplate;
			DownloadTemplate = downloadTemplate;
			DetailsPreloadTemplate = detailsPreloadTemplate;
			GalleryDownloader = galleryDownloader;
			PageDownloader = pageDownloader;
			CoverDownloader = coverDownloader;
			MetadataKeywordLists = metadataKeywordLists;
			DetailsBrowserSettings = detailsBrowserSettings;
			SearchResultCache = searchResultCache;
			CacheFileSystem = cacheFileSystem;

			webBrowser = new WebBrowserEx();

			SuspendLayout();

			webBrowser.AllowWebBrowserDrop = false;
			webBrowser.Dock = DockStyle.Fill;
			webBrowser.Name = "detailsWwebBrowser";
			webBrowser.BeforeDocumentCompleted += WebBrowser_BeforeDocumentCompleted;
			webBrowser.DocumentCompleted += WebBrowser_DocumentCompleted;

			GalleryDownloader.GalleryDownloadReportProgress += GalleryDownloader_GalleryDownloadReportProgress;
			GalleryDownloader.GalleryDownloadStarted += GalleryDownloader_GalleryDownloadStarted;
			GalleryDownloader.GalleryDownloadCancelled += GalleryDownloader_GalleryDownloadCancelled;
			GalleryDownloader.GalleryDownloadCompleted += GalleryDownloader_GalleryDownloadCompleted;

			PageDownloader.PageDownloadReportProgress += PageDownloader_PageDownloadReportProgress;
			PageDownloader.PagesDownloadStarted += PageDownloader_PagesDownloadStarted;
			PageDownloader.PagesDownloadCancelled += PageDownloader_PagesDownloadCancelled;
			PageDownloader.PagesDownloadCompleted += PageDownloader_PagesDownloadCompleted;

			CoverDownloader.CoverDownloadReportProgress += CoverDownloader_CoverDownloadReportProgress;
			CoverDownloader.CoversDownloadStarted += CoverDownloader_CoversDownloadStarted;
			CoverDownloader.CoversDownloadCancelled += CoverDownloader_CoversDownloadCancelled;
			CoverDownloader.CoversDownloadCompleted += CoverDownloader_CoversDownloadCompleted;

			MetadataKeywordLists.WhitelistChanged += Form_WhiteListChanged;
			MetadataKeywordLists.BlacklistChanged += Form_BlackListChanged;
			MetadataKeywordLists.IgnorelistChanged += Form_IgnoreListChanged;
			MetadataKeywordLists.HidelistChanged += Form_HideListChanged;

			DetailsModel.MetadataChanged += DetailsModel_MetadataChanged;

			Controls.Add(webBrowser);

			ResumeLayout(false);
		}

		private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			int galleryId = (int)webBrowser.Tag;

			if (galleryId < 0)
			{
				WebBrowser_PreloadDocumentCompleted(sender, e);
			}
			else
			{
				WebBrowser_GalleryDocumentCompleted(sender, e);
			}
		}

		private void WebBrowser_PreloadDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{

		}

		private void WebBrowser_GalleryDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			int galleryId = (int)webBrowser.Tag;

			if (!CacheFileSystem.DoesCoverExists(galleryId))
			{
				Metadata cachedMetadata = SearchResultCache.Find(galleryId);
				
				CoverDownloader.Download(cachedMetadata);
			}
		}
		private void Form_HideListChanged(object sender, MetadataKeywordListChangedEventArgs e)
		{
			if (string.IsNullOrEmpty(WebBrowser.DocumentText))
			{
				return;
			}

			WebBrowser.Document?.InvokeScript("__onHidelistChanged", e.ToObjectArray());
		}

		private void Form_IgnoreListChanged(object sender, MetadataKeywordListChangedEventArgs e)
		{
			if (string.IsNullOrEmpty(WebBrowser.DocumentText))
			{
				return;
			}

			WebBrowser.Document?.InvokeScript("__onIgnorelistChanged", e.ToObjectArray());
		}

		private void Form_BlackListChanged(object sender, MetadataKeywordListChangedEventArgs e)
		{
			if (string.IsNullOrEmpty(WebBrowser.DocumentText))
			{
				return;
			}

			WebBrowser.Document?.InvokeScript("__onBlacklistChanged", e.ToObjectArray());
		}

		private void Form_WhiteListChanged(object sender, MetadataKeywordListChangedEventArgs e)
		{
			if (string.IsNullOrEmpty(WebBrowser.DocumentText))
			{
				return;
			}

			WebBrowser.Document?.InvokeScript("__onWhitelistChanged", e.ToObjectArray());
		}
		
		private void GalleryDownloader_GalleryDownloadStarted(object sender, GalleryDownloadStartedEventArgs e)
		{
			if (string.IsNullOrEmpty(WebBrowser.DocumentText))
			{
				return;
			}

			WebBrowser.Tag = -e.GalleryId;	// quick hack, negative for preload
			WebBrowser.DocumentText = DetailsPreloadTemplate.GetFormattedText(null/*e.GalleryId*/);

			WebBrowser.Document?.InvokeScript("__onGalleryDownloadStarted", e.ToObjectArray());
		}

		private void GalleryDownloader_GalleryDownloadCompleted(object sender, GalleryDownloadCompletedEventArgs e)
		{
			if (string.IsNullOrEmpty(WebBrowser.DocumentText))
			{
				return;
			}

			DocumentTemplate<Metadata> documentTemplate;

			if (DetailsModel.Target == DetailsTarget.Download)
			{
				documentTemplate = DetailsTemplate;
			}
			else
			{
				documentTemplate = DownloadTemplate;
			}

			WebBrowser.Tag = e.Metadata.Id;
			WebBrowser.DocumentText = documentTemplate.GetFormattedText(e.Metadata);

			SearchResultCache.CacheRuntimeMetadata(e.Metadata);

			WebBrowser.Document?.InvokeScript("__onGalleryDownloadCompleted", e.ToObjectArray());
		}

		private void GalleryDownloader_GalleryDownloadCancelled(object sender, GalleryDownloadCancelledEventArgs e)
		{
			if (string.IsNullOrEmpty(WebBrowser.DocumentText))
			{
				return;
			}

			WebBrowser.Document?.InvokeScript("__onGalleryDownloadCancelled", e.ToObjectArray());
		}

		private void GalleryDownloader_GalleryDownloadReportProgress(object sender, GalleryDownloadReportProgressEventArgs e)
		{
			if (string.IsNullOrEmpty(WebBrowser.DocumentText))
			{
				return;
			}

			WebBrowser.Document?.InvokeScript("__onGalleryDownloadReportProgress", e.ToObjectArray());
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

		private void CoverDownloader_CoversDownloadStarted(object sender, CoversDownloadStartedEventArgs e)
		{
			if (string.IsNullOrEmpty(WebBrowser.DocumentText))
			{
				return;
			}

			WebBrowser.Document?.InvokeScript("__onCoversDownloadStarted", e.ToObjectArray());
		}

		private void CoverDownloader_CoversDownloadCompleted(object sender, CoverDownloadCompletedEventArgs e)
		{
			if (string.IsNullOrEmpty(WebBrowser.DocumentText))
			{
				return;
			}

			WebBrowser.Document?.InvokeScript("__onCoversDownloadCompleted", e.ToObjectArray());
		}

		private void CoverDownloader_CoversDownloadCancelled(object sender, CoverDownloadCompletedEventArgs e)
		{
			if (string.IsNullOrEmpty(WebBrowser.DocumentText))
			{
				return;
			}

			WebBrowser.Document?.InvokeScript("__onCoversDownloadCancelled", e.ToObjectArray());
		}

		private void CoverDownloader_CoverDownloadReportProgress(object sender, CoverDownloadReportProgressEventArgs e)
		{
			if (string.IsNullOrEmpty(WebBrowser.DocumentText))
			{
				return;
			}

			WebBrowser.Document?.InvokeScript("__onCoverDownloadReportProgress", e.ToObjectArray());
		}

		private void DetailsModel_MetadataChanged(object sender, EventArgs e)
		{
			if (DetailsModel.Target == DetailsTarget.Download)
			{
				WebBrowser.Tag = DetailsModel.Metadata?.Id;
				WebBrowser.DocumentText = DownloadTemplate.GetFormattedText(DetailsModel.Metadata);
			}
			else
			{
				WebBrowser.Tag = DetailsModel.Metadata?.Id;
				WebBrowser.DocumentText = DetailsTemplate.GetFormattedText(DetailsModel.Metadata);
			}
		}

		private void WebBrowser_BeforeDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			if (DetailsModel.Target == DetailsTarget.Download)
			{
				webBrowser.ZoomFactor = DownloadBrowserSettings.ZoomRatio;
			}
			else
			{
				webBrowser.ZoomFactor = DetailsBrowserSettings.ZoomRatio;
			}
		}
	}
}
