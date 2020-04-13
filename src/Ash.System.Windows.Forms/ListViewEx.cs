using System.Windows.Forms;

namespace Ash.System.Windows.Forms
{
	public class ListViewEx : ListView
	{
		public ListViewEx()
		{
			SuspendLayout();

			DoubleBuffered = true;
			SetStyle(ControlStyles.SupportsTransparentBackColor, true);

			ResumeLayout(false);
		}
	}
}
