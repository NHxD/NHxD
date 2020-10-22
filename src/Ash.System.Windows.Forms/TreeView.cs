using System;
using System.Collections;
using System.Windows.Forms;

namespace Ash.System.Windows.Forms
{
	public static class TreeViewExtensionMethods
	{
		public static void Sort(this TreeView treeView, TreeNode node, IComparer comparer)
		{
			TreeNodeCollection nodes = (node == null) ? treeView.Nodes : node.Nodes;
			TreeNode[] sortedNodes = new TreeNode[nodes.Count];

			nodes.CopyTo(sortedNodes, 0);
			Array.Sort(sortedNodes, comparer);

			treeView.BeginUpdate();
			nodes.Clear();
			nodes.AddRange(sortedNodes);
			treeView.EndUpdate();
		}
	}
}
