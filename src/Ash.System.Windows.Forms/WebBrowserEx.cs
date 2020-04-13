using System;
using System.Reflection;
using System.Windows.Forms;

namespace Ash.System.Windows.Forms
{
	public partial class WebBrowserEx : WebBrowser
	{
		public float ZoomFactor { get; set; } = 1.0f;

		public event WebBrowserDocumentCompletedEventHandler BeforeDocumentCompleted = delegate { };

		public WebBrowserEx()
		{
			SuspendLayout();

			DoubleBuffered = true;

			DocumentCompleted += WebBrowserEx_DocumentCompleted;

			ResumeLayout(false);
		}

		protected virtual void OnBeforeDocumentCompleted(WebBrowserDocumentCompletedEventArgs e)
		{
			BeforeDocumentCompleted.Invoke(this, e);
		}

		private void WebBrowserEx_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			WebBrowser webBrowser = sender as WebBrowser;

			OnBeforeDocumentCompleted(e);

			webBrowser.DocumentCompleted -= WebBrowserEx_DocumentCompleted;

			int zoomFactor = Math.Max(Math.Min((int)(ZoomFactor * 100.0f), 1000), 10);
			object zoom = zoomFactor;

			try
			{
				object iwb2 = webBrowser.ActiveXInstance;
				//dynamic iwb2 = webBrowser.ActiveXInstance;

				//iwb2.ExecWB(ActiveX.OLECMDID.OPTICAL_ZOOM, ActiveX.OLECMDEXECOPT.DONTPROMPTUSER, zoom, zoom);
				//iwb2.ExecWB(ActiveX.OLECMDID.OPTICAL_ZOOM, ActiveX.OLECMDEXECOPT.DONTPROMPTUSER, ref zoom, IntPtr.Zero);
				iwb2.GetType().InvokeMember("ExecWB", BindingFlags.InvokeMethod, null, iwb2,
					new object[]
					{
						ActiveX.NativeMethods.OLECMDID.OPTICAL_ZOOM, ActiveX.NativeMethods.OLECMDEXECOPT.DONTPROMPTUSER, zoom, IntPtr.Zero
					});
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), "WebBrowser Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}

	internal partial class ActiveX
	{
		private ActiveX() { }

		internal partial class NativeMethods
		{
			private NativeMethods() { }

			internal enum OLECMDID
			{
				OPTICAL_ZOOM = 63,
				OPTICAL_GETZOOMRANGE = 64,
			}

			internal enum OLECMDEXECOPT
			{
				DODEFAULT,
				PROMPTUSER,
				DONTPROMPTUSER,
				SHOWHELP
			}
		}
	}
}
