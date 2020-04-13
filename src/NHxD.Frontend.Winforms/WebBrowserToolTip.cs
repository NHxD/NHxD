using Ash.System.Windows.Forms;
using Nhentai;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace NHxD.Frontend.Winforms
{
	//
	// NOTE: currently unused. (in favor of WebBrowserTreeNodeToolTip)
	//

	public partial class WebBrowserToolTip : UserControl
	{
		private readonly WebBrowserEx webBrowser;
		private readonly Dictionary<Control, int> associatedControls;

		public IPathFormatter PathFormatter { get; }
		public DocumentTemplate<Metadata> GalleryTooltipTemplate { get; }

		public WebBrowserToolTip()
		{
			InitializeComponent();
		}

		public WebBrowserToolTip(IPathFormatter pathFormatter, DocumentTemplate<Metadata> galleryTooltipTemplate)
		{
			InitializeComponent();

			Visible = false;

			PathFormatter = pathFormatter;
			GalleryTooltipTemplate = galleryTooltipTemplate;
			webBrowser = new WebBrowserEx();
			associatedControls = new Dictionary<Control, int>();

			SuspendLayout();

			//
			//
			//
			webBrowser.AllowWebBrowserDrop = false;
			webBrowser.AllowNavigation = false;
			webBrowser.ScriptErrorsSuppressed = true;
			webBrowser.DocumentCompleted += WebBrowser_DocumentCompleted;

			//
			//
			//
			Controls.Add(webBrowser);

			ResumeLayout(false);
		}

		public void RemoveToolTip(Control control)
		{
			if (associatedControls.ContainsKey(control))
			{
				associatedControls.Remove(control);

				control.MouseEnter -= Control_MouseEnter;
				control.MouseLeave -= Control_MouseLeave;
				control.GotFocus -= Control_GotFocus;
			}
		}

		public void SetToolTip(Control control, int galleryId)
		{
			if (associatedControls.ContainsKey(control))
			{
				associatedControls[control] = galleryId;
			}
			else
			{
				associatedControls.Add(control, galleryId);
			}

			control.MouseEnter += Control_MouseEnter;
			control.MouseLeave += Control_MouseLeave;
			control.GotFocus += Control_GotFocus;
		}

		private void Control_GotFocus(object sender, EventArgs e)
		{
			Control control = sender as Control;

			ShowToolTip(control);
		}

		private void Control_MouseLeave(object sender, EventArgs e)
		{
			HideToolTip();
		}

		private void Control_MouseEnter(object sender, EventArgs e)
		{
			Control control = sender as Control;

			ShowToolTip(control);
		}

		public void ShowToolTip(Control control)
		{
			if (!associatedControls.ContainsKey(control))
			{
				return;
			}

			int associatedGalleryId = associatedControls[control];
			string cachedMetadataFileName;

			if (PathFormatter.IsEnabled)
			{
				cachedMetadataFileName = PathFormatter.GetMetadata(associatedGalleryId);
			}
			else
			{
				cachedMetadataFileName = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", PathFormatter.GetCacheDirectory(), associatedGalleryId, ".json");
			}

			Metadata metadata = JsonUtility.LoadFromFile<Metadata>(cachedMetadataFileName);

			if (metadata == null)
			{
				return;
			}

			string html = GalleryTooltipTemplate.GetFormattedText(metadata);

			if (string.IsNullOrEmpty(html))
			{
				return;
			}

			Point toolTipLocation = control.Location;

			toolTipLocation.Offset(0, control.Height);

			Location = toolTipLocation;
			
			webBrowser.DocumentText = html;
		}

		private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			Show();
		}

		public void HideToolTip()
		{
			Hide();
		}
	}
}
