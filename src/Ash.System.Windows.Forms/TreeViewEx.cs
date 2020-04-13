using System;
using System.Drawing;
using System.Windows.Forms;

namespace Ash.System.Windows.Forms
{
	// TODO: invoke NodeSelected when a node is selected by keyboard.

	public class TreeViewEx : TreeView
	{
		public event NodeContextMenuRequestedEventHandler NodeContextMenuRequested = delegate { };
		public event TreeViewExEventHandler NodeActivated = delegate { };
		public event TreeViewExEventHandler NodeSelected = delegate { };

		public TreeViewEx()
		{
			SuspendLayout();

			DoubleBuffered = true;

			ResumeLayout(false);
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space)
			{
				if (SelectedNode != null)
				{
					NodeActivated.Invoke(this, new TreeViewExEventArgs(SelectedNode, TreeViewAction.ByKeyboard));

					e.Handled = true;
					e.SuppressKeyPress = true;

					return;
				}
			}

			base.OnKeyDown(e);
		}

		protected override void OnBeforeSelect(TreeViewCancelEventArgs e)
		{
			if (e.Action == TreeViewAction.ByMouse)
			{
				Point position = PointToClient(Cursor.Position);

				TreeViewHitTestInfo hitTest = HitTest(position);

				if (hitTest.Node != e.Node
					|| hitTest.Location != TreeViewHitTestLocations.Label)
				{
					e.Cancel = true;
					return;
				}
			}

			base.OnBeforeSelect(e);
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);

			if (ModifierKeys.HasFlag(Keys.Control))
			{
				//int WHEEL_DELTA = SystemInformation.MouseWheelScrollDelta;

				//for (int i = 0; i < e.Delta; i += WHEEL_DELTA)
				{
					float newFontSize = Font.Size + (Math.Sign(e.Delta) * (Font.Size * 0.1f));

					if (newFontSize < 0.1f
						|| newFontSize > 100.0f)
					{
						return;
					}

					Font newFont = new Font(Font.FontFamily, newFontSize, Font.Style);

					Font.Dispose();
					Font = newFont;
				}
			}
		}

		protected override void OnNodeMouseClick(TreeNodeMouseClickEventArgs e)
		{
			base.OnNodeMouseClick(e);

			TreeViewHitTestInfo hitTest = HitTest(e.Location);

			if (hitTest.Node != e.Node)
			{
				return;
			}

			if (e.Button == MouseButtons.Right)
			{
				if (hitTest.Location == TreeViewHitTestLocations.Label)
				{
					this.SelectedNode = e.Node;
					NodeContextMenuRequested.Invoke(this, e);
				}
			}
			else if (e.Button == MouseButtons.Left)
			{
				if (hitTest.Location == TreeViewHitTestLocations.Label)
				{
					NodeSelected.Invoke(this, new TreeViewExEventArgs(e.Node, e));
				}
			}
		}

		protected override void OnNodeMouseDoubleClick(TreeNodeMouseClickEventArgs e)
		{
			base.OnNodeMouseDoubleClick(e);

			TreeViewHitTestInfo hitTest = this.HitTest(e.Location);

			if (hitTest.Node != e.Node)
			{
				return;
			}

			if (e.Button == MouseButtons.Left)
			{
				if (hitTest.Location == TreeViewHitTestLocations.Label)
				{
					NodeActivated.Invoke(this, new TreeViewExEventArgs(e.Node, e));
				}
			}
		}
	}

	public class TreeViewExEventArgs : TreeViewEventArgs
	{
		public TreeNodeMouseClickEventArgs NodeMouseClickEventArgs { get; }

		public TreeViewExEventArgs(TreeNode node)
			: base(node)
		{
		}

		public TreeViewExEventArgs(TreeNode node, TreeViewAction action)
			: base(node, action)
		{
		}

		public TreeViewExEventArgs(TreeNode node, TreeNodeMouseClickEventArgs nodeMouseClickEventArgs)
			: base(node, TreeViewAction.ByMouse)
		{
			NodeMouseClickEventArgs = nodeMouseClickEventArgs;
		}
	}

	public delegate void NodeContextMenuRequestedEventHandler(object sender, TreeNodeMouseClickEventArgs e);
	public delegate void TreeViewExEventHandler(object sender, TreeViewExEventArgs e);
}
