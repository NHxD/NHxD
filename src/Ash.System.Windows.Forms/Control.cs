using System.Reflection;
using System.Windows.Forms;

namespace Ash.System.Windows.Forms
{
	public static class ControlExtensionMethods
	{
		public static void SetDoubleBuffered(this Control control, bool value)
		{
			if (SystemInformation.TerminalServerSession)
			{
				return;
			}

			PropertyInfo doubleBufferedProperty = typeof(Control).GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance);

			doubleBufferedProperty.SetValue(control, value, null);
		}
	}
}
