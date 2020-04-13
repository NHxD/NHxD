using Ash.System.Windows.Forms;
using System.ComponentModel;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	public partial class StartupWebBrowserView : UserControl
	{
		private readonly WebBrowserEx webBrowser;

		public WebBrowserEx WebBrowser => webBrowser;

		public CoreTextFormatter CoreTextFormatter { get; }
		public DocumentTemplate<object> StartupTemplate { get; }
		public ApplicationLoader ApplicationLoader { get; }

		public StartupWebBrowserView()
		{
			InitializeComponent();
		}

		public StartupWebBrowserView(CoreTextFormatter coreTextFormatter, DocumentTemplate<object> startupTemplate, ApplicationLoader applicationLoader)
		{
			InitializeComponent();

			CoreTextFormatter = coreTextFormatter;
			StartupTemplate = startupTemplate;
			ApplicationLoader = applicationLoader;

			webBrowser = new WebBrowserEx();

			SuspendLayout();

			webBrowser.Name = "startupWebBrowser";
			webBrowser.Dock = DockStyle.Fill;
			webBrowser.AllowNavigation = false;
			webBrowser.AllowWebBrowserDrop = false;
			webBrowser.IsWebBrowserContextMenuEnabled = false;
			webBrowser.WebBrowserShortcutsEnabled = false;
			webBrowser.ScriptErrorsSuppressed = true;
			webBrowser.DocumentText = StartupTemplate.GetFormattedText();

			Controls.Add(webBrowser);

			ApplicationLoader.ProgressChanged += ApplicationLoader_ProgressChanged;

			ResumeLayout(false);
		}

		private void ApplicationLoader_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			if (string.IsNullOrEmpty(webBrowser.DocumentText))
			{
				return;
			}

			webBrowser.Document?.InvokeScript("__onLoadProgressChanged", new object[] { e.ProgressPercentage, (e.UserState as string) ?? "" });
		}
	}
}
