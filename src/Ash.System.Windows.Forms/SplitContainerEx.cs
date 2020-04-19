using System.Windows.Forms;

namespace Ash.System.Windows.Forms
{
	public class SplitContainerEx : SplitContainer
	{
		public SplitContainerEx()
		{
			SuspendLayout();

			DoubleBuffered = true;

			ResumeLayout(false);
		}
	}
}
