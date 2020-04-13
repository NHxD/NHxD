using System.Windows.Forms;

namespace Ash.System.Windows.Forms
{
	public static class TreeNodeExtensionMethods
	{
		public static void ToggleCollapse(this TreeNode node)
		{
			if (node.IsExpanded)
			{
				node.Collapse();
			}
			else
			{
				node.Expand();
			}
		}
	}
}
