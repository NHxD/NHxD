using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using NHxD.Frontend.Winforms.Configuration;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	public partial class CookieForm : Form
	{
		private readonly WebView2 webView2;
		private readonly Label textBox;
		private readonly Button button;

		public Settings Settings { get; }
		public IPathFormatter PathFormatter { get; }

		public CookieForm(Settings settings, IPathFormatter pathFormatter)
			: this()
		{
			Settings = settings;
			PathFormatter = pathFormatter;
		}

		public CookieForm()
		{
			InitializeComponent();

			webView2 = new WebView2();

			((ISupportInitialize)(webView2)).BeginInit();

			webView2.Dock = DockStyle.Fill;
			webView2.DefaultBackgroundColor = Color.White;

			textBox = new Label();

			textBox.Dock = DockStyle.Top;
			textBox.Text = "Please wait for the verification to finish, then press OK.";

			button = new Button();

			button.Dock = DockStyle.Bottom;
			button.Text = "&OK";
			button.Enabled = false;
			button.Click += Button_Click;

			SuspendLayout();

			AutoScaleDimensions = new SizeF(8F, 16F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(640, 480);
			StartPosition = FormStartPosition.CenterParent;
			ShowIcon = false;
			Text = "Cookies";

			Controls.Add(webView2);
			Controls.Add(textBox);
			Controls.Add(button);

			Load += CookieForm_Load;

			((ISupportInitialize)(webView2)).EndInit();

			ResumeLayout(performLayout: true);
		}

		private async void CookieForm_Load(object sender, EventArgs e)
		{
			if (webView2.CoreWebView2 is null)
			{
				webView2.CoreWebView2InitializationCompleted += WebView2_CoreWebView2InitializationCompleted;
			}

			var coreWebView2Environment = await CoreWebView2Environment.CreateAsync(browserExecutableFolder: null, userDataFolder: PathFormatter.GetCacheDirectory(), options: null);

			await webView2.EnsureCoreWebView2Async(coreWebView2Environment, controllerOptions: null);
		}

		private async void Button_Click(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(Settings.Network.Client.UserAgent))
			{
				webView2.CoreWebView2.Settings.UserAgent = Settings.Network.Client.UserAgent;
			}

			var cookies = await webView2.CoreWebView2.CookieManager.GetCookiesAsync(webView2.CoreWebView2.Source);

			foreach (var cookie in cookies)
			{
				Settings.Network.Client.Cookies.Add(cookie.Name, cookie.Value);
			}

			if (string.IsNullOrEmpty(Settings.Network.Client.UserAgent))
			{
				Settings.Network.Client.UserAgent = webView2.CoreWebView2.Settings.UserAgent;
			}

			DialogResult = DialogResult.OK;
		}

		private void WebView2_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
		{
			if (e.InitializationException != null)
			{
				MessageBox.Show(this, e.InitializationException.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			if (e.IsSuccess)
			{
				if (webView2.Source is null || webView2.Source.Equals(new Uri("about:blank")))
				{
					webView2.NavigationCompleted += WebView2_NavigationCompleted;
					webView2.Source = new Uri("https://nhentai.net/api/gallery/" + 1, UriKind.Absolute);
				}
			}
		}

		private void WebView2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
		{
			if (e.IsSuccess)
			{
				button.Enabled = true;
			}
			else
			{
				MessageBox.Show(this, e.WebErrorStatus.ToString(), "Error " + e.HttpStatusCode, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}
}
