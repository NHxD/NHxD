using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Ash.System.Windows.Forms
{
	public partial class TabControlEx : TabControl
	{
		public TabControlEx()
		{
			SuspendLayout();

			DoubleBuffered = true;

			ResumeLayout(false);
		}

		// WORKAROUND: Selected is never raised if the first tab is selected. (which is annoying if you want to initialize it)
		public void SelectTab(string tabPageName, bool forceSelection)
		{
			if (!forceSelection)
			{
				base.SelectTab(tabPageName);
			}
			else
			{
				if (!TabPages.ContainsKey(tabPageName))
				{
					return;
				}

				TabPage targetTab = First(x => x.Name.Equals(tabPageName));

				if (TabPages.IndexOfKey(tabPageName) == 0)
				{
					OnSelected(new TabControlEventArgs(targetTab, 0, TabControlAction.Selected));
				}
				else
				{
					base.SelectTab(tabPageName);
				}
			}
		}

		public TabPage First(Func<TabPage, bool> predicate)
		{
			Func<TabPage, bool> predicateHandler = predicate;

			if (predicateHandler != null)
			{
				foreach (TabPage tabPage in this.TabPages)
				{
					if (predicateHandler.Invoke(tabPage))
					{
						return tabPage;
					}
				}
			}

			return null;
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == Commctrl.NativeMethods.TCM_ADJUSTRECT)
			{
				Commctrl.NativeMethods.RECT rc = (Commctrl.NativeMethods.RECT)m.GetLParam(typeof(Commctrl.NativeMethods.RECT));

				// HACK: get these values from somewhere.
				rc.left -= 3;
				rc.top -= 3;
				rc.right += 3;
				rc.bottom += 3;

				Marshal.StructureToPtr(rc, m.LParam, true);
			}

			base.WndProc(ref m);
		}
	}

	internal partial class Commctrl
	{
		private Commctrl() { }

		internal partial class NativeMethods
		{
			private NativeMethods() { }

			internal const Int32 TCM_FIRST = 0x1300;
			internal const Int32 TCM_ADJUSTRECT = (TCM_FIRST + 40);

			[StructLayout(LayoutKind.Sequential)]
			internal struct RECT
			{
				internal Int32 left;
				internal Int32 top;
				internal Int32 right;
				internal Int32 bottom;
			}
		}
	}
}
